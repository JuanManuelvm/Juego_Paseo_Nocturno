using System.Collections.Generic;
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

    [Header("Patrullaje")]
    public List<Transform> puntosPatrulla;
    public float tiempoEsperaEnPunto = 2f;
    public float velocidadPatrulla = 2f;      // <--- FALTA ESTA
    public float velocidadPersecucion = 5.5f;
    private int indicePuntoActual = 0;
    private float temporizadorEspera = 0f;

    [Header("Ataque")]
    public float distanciaAtaque = 2f;
    private bool yaAtaco = false;

    [Header("Audio del Enemigo")]
    public AudioSource audioSource;
    public AudioClip sonidoPasosPatrulla;    // Audio suave de caminata
    public AudioClip sonidoPasosPersecucion; // Audio pesado de carrera
    public AudioClip sonidoGritoAlerta; // <--- Sonido al detectar al jugador
    public AudioClip sonidoAtaque;

    private NavMeshAgent agente;
    private Animator animator;
    private bool yaGrito = false; // Control para no repetir el grito en bucle

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // Iniciar patrullaje al arrancar si hay puntos asignados
        IrAlSiguientePuntoPatrulla();
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
            Patrullar();
        }

        // Actualizar animación según la velocidad real del NavMeshAgent
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.SetFloat("Velocidad", agente.velocity.magnitude);
        }

        // Control del sonido de pasos según movimiento
        GestionarSonidoPasos();
    }

    void Patrullar()
    {
        if (yaGrito) yaGrito = false;

        // Ajustar velocidad del NavMeshAgent a modo caminata
        agente.speed = velocidadPatrulla;
        agente.isStopped = false;

        if (puntosPatrulla == null || puntosPatrulla.Count == 0) return;

        if (!agente.pathPending && agente.remainingDistance <= agente.stoppingDistance)
        {
            temporizadorEspera += Time.deltaTime;
            if (temporizadorEspera >= tiempoEsperaEnPunto)
            {
                IrAlSiguientePuntoPatrulla();
                temporizadorEspera = 0f;
            }
        }
    }

    void IrAlSiguientePuntoPatrulla()
    {
        if (puntosPatrulla.Count == 0) return;

        agente.destination = puntosPatrulla[indicePuntoActual].position;
        indicePuntoActual = (indicePuntoActual + 1) % puntosPatrulla.Count;
    }

    void PerseguirJugador()
    {
        if (agente.isOnNavMesh)
        {
            agente.isStopped = false;
            agente.speed = velocidadPersecucion; // Aumentar velocidad al perseguir
            agente.SetDestination(jugador.position);
        }

        if (!yaGrito)
        {
            if (audioSource != null && sonidoGritoAlerta != null)
            {
                audioSource.PlayOneShot(sonidoGritoAlerta, 1.0f);
            }
            yaGrito = true;
        }
    }

    void GestionarSonidoPasos()
    {
        if (audioSource == null || yaAtaco) return;

        bool estaEnMovimiento = agente.velocity.magnitude > 0.2f && !agente.isStopped;

        if (estaEnMovimiento)
        {
            // Determinar qué clip le corresponde según el estado de alerta
            AudioClip clipDeseado = yaGrito ? sonidoPasosPersecucion : sonidoPasosPatrulla;

            if (clipDeseado != null)
            {
                // Si cambió de estado (de patrulla a persecución) o no estaba sonando nada
                if (!audioSource.isPlaying || audioSource.clip != clipDeseado)
                {
                    audioSource.clip = clipDeseado;
                    audioSource.volume = yaGrito ? 0.8f : 0.4f;
                    audioSource.loop = true;
                    audioSource.Play();
                }
            }
        }
        else
        {
            // Detener sonido si se detiene en un punto de patrulla
            if (audioSource.isPlaying && (audioSource.clip == sonidoPasosPatrulla || audioSource.clip == sonidoPasosPersecucion))
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

    void AtacarJugador()
    {
        if (yaAtaco) return;

        agente.isStopped = true;

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        Vector3 direccion = (jugador.position - transform.position).normalized;
        direccion.y = 0;
        transform.rotation = Quaternion.LookRotation(direccion);

        if (animator != null)
        {
            animator.SetTrigger("Atacar");
        }

        if (audioSource != null && sonidoAtaque != null)
        {
            audioSource.PlayOneShot(sonidoAtaque, 1.0f);
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