using UnityEngine;
using UnityEngine.AI;

public class EnemigoIAAvanzado : MonoBehaviour
{
    [Header("Referencias")]
    public Transform jugador;
    public LayerMask capaObstaculos;
    public LayerMask capaJugador;

    [Header("Campo de Visión (FOV)")]
    public float rangoDeteccion = 12f;
    public float anguloVision = 110f;

    [Header("Ataque")]
    public float distanciaAtaque = 2f;
    private bool yaAtaco = false;

    [Header("Audio del Enemigo")]
    public AudioSource audioSource;
    public AudioClip sonidoPasos;
    public AudioClip sonidoAtaque;

    private NavMeshAgent agente;
    private Animator animator;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Si no se asignó manualmente, buscar el AudioSource en este mismo objeto
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (jugador == null) return;

        float distanciaAlJugador = Vector3.Distance(transform.position, jugador.position);

        if (distanciaAlJugador <= distanciaAtaque)
        {
            AtacarJugador();
        }
        else if (ViendoAlJugador(distanciaAlJugador))
        {
            PerseguirJugador();
        }
        else
        {
            QuedarseEnEspera();
        }

        // Actualizar parámetros de animación
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.SetFloat("Velocidad", agente.velocity.magnitude);
        }

        // Control del sonido de pasos según si se está moviendo o no
        GestionarSonidoPasos();
    }

    void GestionarSonidoPasos()
    {
        if (audioSource == null || sonidoPasos == null || yaAtaco) return;

        // Si el enemigo se está moviendo rápido y no está reproduciendo los pasos
        if (agente.velocity.magnitude > 0.5f && !agente.isStopped)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.clip = sonidoPasos;
                audioSource.loop = true; // Repetir el bucle de pasos
                audioSource.Play();
            }
        }
        else
        {
            // Si se detiene, pausar el sonido de pasos
            if (audioSource.isPlaying && audioSource.clip == sonidoPasos)
            {
                audioSource.Stop();
            }
        }
    }

    bool ViendoAlJugador(float distancia)
    {
        if (distancia > rangoDeteccion) return false;

        Vector3 direccionHaciaJugador = (jugador.position - transform.position).normalized;
        float angulo = Vector3.Angle(transform.forward, direccionHaciaJugador);

        if (angulo < anguloVision / 2f)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up, direccionHaciaJugador, out hit, rangoDeteccion))
            {
                if (hit.transform.CompareTag("Player"))
                {
                    return true;
                }
            }
        }

        return false;
    }

    void PerseguirJugador()
    {
        if (agente.isOnNavMesh)
        {
            agente.isStopped = false;
            agente.SetDestination(jugador.position);
        }
        if (MusicaAmbiente.Instance != null)
        {
            MusicaAmbiente.Instance.SetEstadoPersecucion(true);
        }
    }

    void QuedarseEnEspera()
    {
        if (agente.isOnNavMesh)
        {
            agente.isStopped = true;
        }
        if (MusicaAmbiente.Instance != null)
        {
            MusicaAmbiente.Instance.SetEstadoPersecucion(false);
        }
    }

    void AtacarJugador()
    {
        if (yaAtaco) return;

        agente.isStopped = true;
        
        // Detener los pasos
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // Orientar hacia el jugador
        Vector3 direccion = (jugador.position - transform.position).normalized;
        direccion.y = 0;
        transform.rotation = Quaternion.LookRotation(direccion);

        // Disparar animación
        if (animator != null)
        {
            animator.SetTrigger("Atacar");
        }

        // Reproducir sonido de ataque (un solo disparo)
        if (audioSource != null && sonidoAtaque != null)
        {
            audioSource.PlayOneShot(sonidoAtaque);
        }

        yaAtaco = true;
        Invoke(nameof(DispararGameOver), 0.8f);
    }

    void DispararGameOver()
    {
        if (ControlJuegoUI.Instance != null)
        {
            ControlJuegoUI.Instance.MostrarGameOver();
        }
    }
}