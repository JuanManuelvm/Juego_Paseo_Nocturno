using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; // Necesario para el nuevo Input System

public class ControlLinterna : MonoBehaviour
{
    [Header("Configuración de Linterna")]
    public Light luzLinterna; // Referencia a la Spotlight de la linterna
    public AudioSource audioSource; // Componente para reproducir el sonido
    public AudioClip sonidoClic; // Archivo de audio del clic

    [Header("Configuración de Batería")]
    public Slider sliderBateria; // Barra de UI en el Canvas
    public float bateriaMaxima = 100f;
    public float bateriaActual;
    public float consumoPorSegundo = 4f; // Consume la batería en ~25 segundos encendida

    [Header("Estado")]
    public bool estaEncendida = true;

    void Start()
    {
        bateriaActual = bateriaMaxima;

        // Configurar el estado inicial del Slider de la UI
        if (sliderBateria != null)
        {
            sliderBateria.maxValue = bateriaMaxima;
            sliderBateria.value = bateriaActual;
        }

        // Asegurar que el estado inicial de la luz coincida con la variable y la batería
        ActualizarEstadoLuz();
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;

        // Detectar si la tecla F O el clic izquierdo del mouse fueron presionados
        bool tecladoPresionado = (keyboard != null && keyboard.fKey.wasPressedThisFrame);
        bool mousePresionado = (mouse != null && mouse.leftButton.wasPressedThisFrame);

        if (tecladoPresionado || mousePresionado)
        {
            // Solo permite encender/apagar si le queda batería
            if (bateriaActual > 0)
            {
                AlternarLinterna();
            }
        }

        // Consumo de batería con el tiempo (SOLO cuando está encendida y tiene energía)
        if (estaEncendida && bateriaActual > 0)
        {
            bateriaActual -= consumoPorSegundo * Time.deltaTime;

            if (bateriaActual <= 0)
            {
                bateriaActual = 0;
                estaEncendida = false; // Se apaga al agotarse
            }

            ActualizarEstadoLuz();
        }

        // Actualizar el valor visual de la barra de batería
        if (sliderBateria != null)
        {
            sliderBateria.value = bateriaActual;
        }
    }

    void AlternarLinterna()
    {
        estaEncendida = !estaEncendida;
        ActualizarEstadoLuz();

        // Reproducir el sonido del interruptor
        if (audioSource != null && sonidoClic != null)
        {
            audioSource.PlayOneShot(sonidoClic);
        }
    }

    void ActualizarEstadoLuz()
    {
        if (luzLinterna != null)
        {
            // La luz solo emitirá si la linterna está encendida Y tiene batería restante
            luzLinterna.enabled = estaEncendida && (bateriaActual > 0);
        }
    }

    // Método invocado exclusivamente por los puntos específicos de recarga (PuntoRecarga.cs)
    public void RecargarCompleto()
    {
        bateriaActual = bateriaMaxima;
        estaEncendida = true;
        ActualizarEstadoLuz();

        if (sliderBateria != null)
        {
            sliderBateria.value = bateriaActual;
        }

        Debug.Log("Batería recargada al 100% en estación fija.");
    }
}
