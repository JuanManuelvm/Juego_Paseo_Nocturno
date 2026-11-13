using System.Collections;
using UnityEngine;

public class ParpadeoLuz : MonoBehaviour
{
    [Header("Configuración de Luz")]
    public Light luzLampara;
    public float intensidadMinima = 0.1f;
    public float intensidadMaxima = 2.5f;

    [Header("Tiempos de Parpadeo")]
    public float tiempoMinimo = 0.05f;
    public float tiempoMaximo = 0.4f;

    [Header("Efecto de Falla (Opcional)")]
    [Tooltip("Probabilidad de que la luz se apague del todo por unos segundos")]
    public bool tieneFallasLargos = true;
    public float probabilidadFalla = 0.15f; // 15% de probabilidad

    private void Start()
    {
        if (luzLampara == null)
        {
            luzLampara = GetComponent<Light>();
        }

        StartCoroutine(RutinaParpadeo());
    }

    private IEnumerator RutinaParpadeo()
    {
        while (true)
        {
            // Cambia la intensidad de forma aleatoria para simular chispazos o variación de voltaje
            luzLampara.intensity = Random.Range(intensidadMinima, intensidadMaxima);

            // Simular un apagón o falla más largo de vez en cuando
            if (tieneFallasLargos && Random.value < probabilidadFalla)
            {
                luzLampara.intensity = 0f;
                yield return new WaitForSeconds(Random.Range(0.5f, 2.0f));
            }

            // Esperar un tiempo aleatorio corto antes del siguiente parpadeo
            yield return new WaitForSeconds(Random.Range(tiempoMinimo, tiempoMaximo));
        }
    }
}