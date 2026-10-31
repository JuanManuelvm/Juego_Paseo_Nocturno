using UnityEngine;

public class MusicaAmbiente : MonoBehaviour
{
    public static MusicaAmbiente Instance;

    [Header("Fuentes de Audio")]
    public AudioSource audioCalma;
    public AudioSource audioPersecucion;

    [Header("Ajustes de Transición")]
    public float velocidadTransicion = 2f; // Qué tan rápido cambia el volumen
    public float volumenMaximo = 0.5f;

    private bool enPersecucion = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Configurar ambas fuentes para que inicien reproduciendo en bucle
        if (audioCalma != null && audioPersecucion != null)
        {
            audioCalma.loop = true;
            audioPersecucion.loop = true;

            audioCalma.volume = volumenMaximo;
            audioPersecucion.volume = 0f;

            audioCalma.Play();
            audioPersecucion.Play();
        }
    }

    void Update()
    {
        if (audioCalma == null || audioPersecucion == null) return;

        // Si está persiguiendo, sube la música de tensión y baja la normal
        float volumenObjetivoCalma = enPersecucion ? 0f : volumenMaximo;
        float volumenObjetivoPersecucion = enPersecucion ? volumenMaximo : 0f;

        audioCalma.volume = Mathf.MoveTowards(audioCalma.volume, volumenObjetivoCalma, velocidadTransicion * Time.deltaTime);
        audioPersecucion.volume = Mathf.MoveTowards(audioPersecucion.volume, volumenObjetivoPersecucion, velocidadTransicion * Time.deltaTime);
    }

    public void SetEstadoPersecucion(bool persiguiendo)
    {
        enPersecucion = persiguiendo;
    }
}