using UnityEngine;
using UnityEngine.AI;

public class EnemigoIAAvanzado : MonoBehaviour
{
    [Header("Referencias")]
    public Transform jugador;
    public LayerMask capaObstaculos; // Para que las paredes tapen la vista
    public LayerMask capaJugador;

    [Header("Campo de Visión (FOV)")]
    public float rangoDeteccion = 12f;  // Distancia máxima de visión
    public float anguloVision = 110f;   // Cono de visión en grados (ej. 110°)

    [Header("Ataque")]
    public float distanciaAtaque = 2f;
    private bool yaAtaco = false;

    private NavMeshAgent agente;
    private Animator animator;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (jugador == null) return;

        float distanciaAlJugador = Vector3.Distance(transform.position, jugador.position);

        // 1. Si está lo suficientemente cerca para golpear
        if (distanciaAlJugador <= distanciaAtaque)
        {
            AtacarJugador();
        }
        // 2. Si el jugador está dentro del campo de visión
        else if (ViendoAlJugador(distanciaAlJugador))
        {
            PerseguirJugador();
        }
        // 3. Si perdió de vista al jugador
        else
        {
            QuedarseEnEspera();
        }

        // Actualizar la animación de movimiento basada en la velocidad real del agente
        if (animator != null)
        {
            animator.SetFloat("Velocidad", agente.velocity.magnitude);
        }
    }

    bool ViendoAlJugador(float distancia)
    {
        // Verificar si está dentro del rango máximo de visión
        if (distancia > rangoDeteccion) return false;

        // Calcular el ángulo entre la mirada del enemigo y la posición del jugador
        Vector3 direccionHaciaJugador = (jugador.position - transform.position).normalized;
        float angulo = Vector3.Angle(transform.forward, direccionHaciaJugador);

        // Si está dentro del cono de visión
        if (angulo < anguloVision / 2f)
        {
            // Lanzar un Raycast para comprobar si hay paredes interponiéndose
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up, direccionHaciaJugador, out hit, rangoDeteccion))
            {
                if (hit.transform.CompareTag("Player"))
                {
                    return true; // ¡Jugador detectado sin obstáculos!
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
    }

    void QuedarseEnEspera()
    {
        if (agente.isOnNavMesh)
        {
            agente.isStopped = true; // Se detiene y vuelve a Idle
        }
    }

    void AtacarJugador()
    {
        if (yaAtaco) return;

        agente.isStopped = true;
        
        // Hacer que mire hacia el jugador al golpear
        Vector3 direccion = (jugador.position - transform.position).normalized;
        direccion.y = 0;
        transform.rotation = Quaternion.LookRotation(direccion);

        if (animator != null)
        {
            animator.SetTrigger("Atacar");
        }

        yaAtaco = true;

        // Mostrar Game Over tras un breve retraso para sincronizar con la animación de golpe
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