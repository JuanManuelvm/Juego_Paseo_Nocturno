using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipalManager : MonoBehaviour
{
    [Header("Paneles del Menú")]
    public GameObject panelMenuPrincipal;
    public GameObject panelControles;

    [Header("Escena a Cargar")]
    public string nombreEscenaJuego = "Prueba"; // El nombre exacto de tu escena del mapa

    void Start()
    {
        // Liberar y mostrar el cursor en el menú
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        MostrarMenuPrincipal();
    }

    public void Jugar()
    {
        // Carga directamente la escena del juego registrada en el Build Profile
        SceneManager.LoadScene(nombreEscenaJuego);
    }

    public void MostrarControles()
    {
        panelMenuPrincipal.SetActive(false);
        panelControles.SetActive(true);
    }

    public void MostrarMenuPrincipal()
    {
        panelMenuPrincipal.SetActive(true);
        panelControles.SetActive(false);
    }

    public void SalirDelJuego()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}
