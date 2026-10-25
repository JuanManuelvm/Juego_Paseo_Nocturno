using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para reiniciar la escena

public class EnemigoContacto : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Verificar si el objeto con el que chocó tiene la etiqueta "Player"
        if (other.CompareTag("Player"))
        {
            Debug.Log("¡El enemigo te ha atrapado!");
            
            // Reiniciar la escena actual (Game Over rápido)
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}