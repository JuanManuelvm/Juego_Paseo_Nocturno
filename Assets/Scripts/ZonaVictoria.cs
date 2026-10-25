using UnityEngine;

public class ZonaVictoria : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("¡Llegaste a salvo a casa!");
            // Aquí puedes activar un panel UI con el texto "Llegaste a salvo" o cargar la escena de créditos
        }
    }
}