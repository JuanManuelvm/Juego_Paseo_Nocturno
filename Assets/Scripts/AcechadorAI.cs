using UnityEngine;
using UnityEngine.AI;

public class AcechadorAI : MonoBehaviour
{
    public enum EstadoEnemigo { Persiguiendo, Huyendo, Acechando }

    [Header("Referencias")]
    public Transform player;
    public Light linternaJugador;
    private NavMeshAgent agent;
    private Animator animator;
    private AudioSource audioSource;

    [Header("Configuración de Distancias y Tiempos")]
    public float distanciaAudioCercano = 15f;
    public float tiempoAcecho = 5f; // Tiempo que se queda en 'Espera' tras huir
    public float distanciaHuida = 15f;
    public float anguloLinterna = 45f;

    [Header("Clips de Audio")]
    public AudioClip sonidoPasosPasivos;
    public AudioClip sonidoGritoHuida;

    private EstadoEnemigo estadoActual = EstadoEnemigo.Persiguiendo;
    private float timerAcecho = 0f;
    private bool juegoTerminado = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (player == null)
        {
            GameObject objPlayer = GameObject.FindGameObjectWithTag("Player");
            if (objPlayer != null) player = objPlayer.transform;
        }
    }

    void Update()
    {
        if (juegoTerminado) return;

        GestionarAudioDistancia();

        switch (estadoActual)
        {
            case EstadoEnemigo.Persiguiendo:
                ComportamientoPerseguir();
                DetectarLinterna();
                break;

            case EstadoEnemigo.Huyendo:
                ComportamientoHuir();
                break;

            case EstadoEnemigo.Acechando:
                ComportamientoAcechar();
                break;
        }
    }

    void ComportamientoPerseguir()
    {
        if (agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }

        // Solo activa el Bool si actualmente está en false para evitar reiniciar la animación por frame
        if (!animator.GetBool("IsRunning"))
        {
            animator.SetBool("IsRunning", true);
            animator.SetBool("IsFleeing", false);
        }
    }

    void DetectarLinterna()
    {
        if (linternaJugador != null && linternaJugador.enabled)
        {
            Vector3 direccionHaciaEnemigo = (transform.position - player.position).normalized;
            float angulo = Vector3.Angle(player.forward, direccionHaciaEnemigo);

            if (angulo < anguloLinterna && Vector3.Distance(player.position, transform.position) < linternaJugador.range)
            {
                RaycastHit hit;
                if (Physics.Raycast(player.position, direccionHaciaEnemigo, out hit, linternaJugador.range))
                {
                    if (hit.transform == transform || hit.transform.IsChildOf(transform))
                    {
                        IniciarHuida();
                    }
                }
            }
        }
    }

    void IniciarHuida()
    {
        estadoActual = EstadoEnemigo.Huyendo;

        if (sonidoGritoHuida != null && audioSource != null)
            audioSource.PlayOneShot(sonidoGritoHuida);

        // Calcular punto alejado en dirección opuesta
        Vector3 direccionHuida = (transform.position - player.position).normalized;
        Vector3 puntoDestino = transform.position + direccionHuida * distanciaHuida;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(puntoDestino, out hit, distanciaHuida, NavMesh.AllAreas))
        {
            agent.isStopped = false;
            agent.SetDestination(hit.position);
        }

        // Cambiar animación a Huir
        animator.SetBool("IsRunning", false);
        animator.SetBool("IsFleeing", true);
    }

    void ComportamientoHuir()
    {
        // Cuando llega al punto de destino de huida
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            // Transición a la fase de Espera / Acecho
            estadoActual = EstadoEnemigo.Acechando;
            timerAcecho = tiempoAcecho;

            // Cambiar animaciones a la pose de Espera (Reposo)
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsFleeing", false);
            
            agent.isStopped = true;
        }
    }

    void ComportamientoAcechar()
    {
        // Se queda en la animación de espera contando el tiempo antes de volver a perseguir
        timerAcecho -= Time.deltaTime;

        if (timerAcecho <= 0f)
        {
            estadoActual = EstadoEnemigo.Persiguiendo;
        }
    }

    void GestionarAudioDistancia()
    {
        if (audioSource == null) return;

        float distancia = Vector3.Distance(transform.position, player.position);
        if (distancia <= distanciaAudioCercano && !audioSource.isPlaying)
        {
            audioSource.clip = sonidoPasosPasivos;
            audioSource.loop = true;
            audioSource.Play();
        }
        else if (distancia > distanciaAudioCercano && audioSource.isPlaying && estadoActual != EstadoEnemigo.Huyendo)
        {
            audioSource.Stop();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (juegoTerminado) return;

        if (other.CompareTag("Player"))
        {
            juegoTerminado = true;
            agent.isStopped = true;
            animator.SetBool("IsRunning", false);

            // 1. Activar la interfaz de Game Over
            if (ControlJuegoUI.Instance != null && ControlJuegoUI.Instance.panelGameOver != null)
            {
                ControlJuegoUI.Instance.panelGameOver.SetActive(true);
            }

            // 2. DESBLOQUEAR Y MOSTRAR EL MOUSE PARA EL BOTÓN
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // 3. Pausar el tiempo del juego
            Time.timeScale = 0f;
        }
    }
}