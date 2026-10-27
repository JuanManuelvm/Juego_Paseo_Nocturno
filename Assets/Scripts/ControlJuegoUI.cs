using UnityEngine;
using UnityEngine.SceneManagement;

public class ControlJuegoUI : MonoBehaviour
{
    public static ControlJuegoUI Instance;

    [Header("Paneles de UI")]
    public GameObject panelGameOver; // Debería decir public
    public GameObject panelVictoria; // Debería decir public

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void MostrarGameOver()
    {
        if (panelGameOver != null)
        {
            panelGameOver.SetActive(true);
            FinalizarPartida();
        }
    }

    public void MostrarVictoria()
    {
        if (panelVictoria != null)
        {
            panelVictoria.SetActive(true);
            FinalizarPartida();
        }
    }

    private void FinalizarPartida()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ReiniciarJuego()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}