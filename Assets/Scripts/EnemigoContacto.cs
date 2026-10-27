using UnityEngine;

public class EnemigoContacto : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Llama directamente al panel de Game Over
            if (ControlJuegoUI.Instance != null)
            {
                ControlJuegoUI.Instance.MostrarGameOver();
            }
        }
    }
}