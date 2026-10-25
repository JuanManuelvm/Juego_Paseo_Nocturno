using UnityEngine;
using UnityEngine.InputSystem; // Necesario para el nuevo Input System

public class ControlLinterna : MonoBehaviour
{
    [Header("Configuración de Linterna")]
    public Light luzLinterna; // Referencia a la Spotlight de la linterna
    public AudioSource audioSource; // Componente para reproducir el sonido
    public AudioClip sonidoClic; // Archivo de audio (.wav o .mp3) del clic

    private bool estaEncendida = true;

    void Start()
    {
        // Asegurar que el estado inicial de la luz coincida con la variable
        if (luzLinterna != null)
        {
            luzLinterna.enabled = estaEncendida;
        }
    }

    void Update()
    {
        // Verificamos si el teclado está disponible
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Detectar si la tecla F fue presionada en este fotograma
        if (keyboard.fKey.wasPressedThisFrame)
        {
            AlternarLinterna();
        }
    }

    void AlternarLinterna()
    {
        // Invertir el estado (encendido/apagado)
        estaEncendida = !estaEncendida;

        if (luzLinterna != null)
        {
            luzLinterna.enabled = estaEncendida;
        }

        // Reproducir el sonido si están asignados el AudioSource y el clip
        if (audioSource != null && sonidoClic != null)
        {
            audioSource.PlayOneShot(sonidoClic);
        }
    }
}