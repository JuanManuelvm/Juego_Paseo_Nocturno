using UnityEngine;

public class ZonaVictoria : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Llama directamente al panel de Victoria
            if (ControlJuegoUI.Instance != null)
            {
                ControlJuegoUI.Instance.MostrarVictoria();
            }
        }
    }
}