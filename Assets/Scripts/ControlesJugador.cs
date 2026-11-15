using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class ControlesJugador : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidadCaminando = 5f;
    public float velocidadCorriendo = 8.5f;
    public float gravedad = -9.81f;

    [Header("Sistema de Estamina")]
    public float estaminaMaxima = 100f;
    public float estaminaActual;
    public float consumoEstamina = 20f;     // Estamina consumida por segundo al correr
    public float recargaEstamina = 15f;     // Estamina regenerada por segundo al no correr
    private bool exhausto = false;           // Bloquea la carrera hasta recargar un mínimo

    [Header("Cámara y Ratón")]
    public Transform camaraJugador;
    public float sensibilidadRaton = 0.1f;
    public float limiteVertical = 80f;

    [Header("Linterna y Animaciones")]
    public Light luzLinterna;
    public Animator animator;

    [Header("Audio SFX")]
    public AudioSource audioSource;
    public AudioSource audioSourceRespiracion; // AudioSource secundario exclusivo para la respiración/cansancio
    public AudioClip sonidoPasosCaminar;
    public AudioClip sonidoPasosCorrer;
    public AudioClip sonidoLinterna;
    public AudioClip sonidoCansado;           // Clip de respiración fatigada en bucle

    private CharacterController characterController;
    private float rotacionX = 0f;
    private bool linternaEncendida = true;
    private float velocidadY = 0f;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        estaminaActual = estaminaMaxima;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (animator == null) animator = GetComponentInChildren<Animator>();

        linternaEncendida = true;
        if (luzLinterna != null) luzLinterna.enabled = linternaEncendida;
        if (animator != null) animator.SetBool("IsFlashlightOn", linternaEncendida);
    }

    private void Update()
    {
        Look();
        Move();
        HandleFlashlight();
    }

    private void Look()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 deltaRaton = mouse.delta.ReadValue() * sensibilidadRaton;

        rotacionX -= deltaRaton.y;
        rotacionX = Mathf.Clamp(rotacionX, -limiteVertical, limiteVertical);

        if (camaraJugador != null)
        {
            camaraJugador.localRotation = Quaternion.Euler(rotacionX, 0f, 0f);
        }

        transform.Rotate(Vector3.up * deltaRaton.x);
    }

    private void Move()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        float moveX = 0f;
        float moveZ = 0f;

        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) moveX += 1f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) moveX -= 1f;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) moveZ += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) moveZ -= 1f;

        Vector3 moveInput = new Vector3(moveX, 0f, moveZ);
        bool estaMoviendose = moveInput.sqrMagnitude > 0.01f;
        bool intentaCorrer = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;

        // Gestión del estado de Agotamiento (Exhausto)
        if (estaminaActual <= 0f)
        {
            exhausto = true;
        }
        else if (estaminaActual >= 25f) // Debe recuperar al menos el 25% para volver a correr
        {
            exhausto = false;
        }

        // Solo puede correr si se está moviendo, presiona Shift y NO está exhausto
        bool estaCorriendo = estaMoviendose && intentaCorrer && !exhausto;

        // Consumo y Regeneración de Estamina
        if (estaCorriendo)
        {
            estaminaActual -= consumoEstamina * Time.deltaTime;
            estaminaActual = Mathf.Clamp(estaminaActual, 0f, estaminaMaxima);
        }
        else
        {
            if (estaminaActual < estaminaMaxima)
            {
                estaminaActual += recargaEstamina * Time.deltaTime;
                estaminaActual = Mathf.Clamp(estaminaActual, 0f, estaminaMaxima);
            }
        }

        // Control del sonido de cansancio/respiración agitada
        GestionarSonidoCansancio();

        if (moveInput.magnitude > 1f) moveInput.Normalize();

        float velocidadActual = estaCorriendo ? velocidadCorriendo : velocidadCaminando;
        Vector3 movimientoHorizontal = transform.TransformDirection(moveInput) * velocidadActual;

        if (characterController.isGrounded && velocidadY < 0)
        {
            velocidadY = -2f;
        }
        velocidadY += gravedad * Time.deltaTime;

        Vector3 movimientoFinal = movimientoHorizontal + new Vector3(0f, velocidadY, 0f);
        characterController.Move(movimientoFinal * Time.deltaTime);

        if (animator != null)
        {
            animator.SetBool("IsWalking", estaMoviendose && !estaCorriendo);
            animator.SetBool("IsSprinting", estaCorriendo);
        }

        // Audio de Pasos
        if (estaMoviendose && characterController.isGrounded)
        {
            AudioClip clipDeseado = estaCorriendo ? sonidoPasosCorrer : sonidoPasosCaminar;
            float volumenDeseado = estaCorriendo ? 0.45f : 0.25f;

            if (audioSource != null && clipDeseado != null)
            {
                if (!audioSource.isPlaying || audioSource.clip != clipDeseado)
                {
                    audioSource.clip = clipDeseado;
                    audioSource.volume = volumenDeseado;
                    audioSource.loop = true;
                    audioSource.Play();
                }
            }
        }
        else
        {
            if (audioSource != null && audioSource.isPlaying && 
               (audioSource.clip == sonidoPasosCaminar || audioSource.clip == sonidoPasosCorrer))
            {
                audioSource.Stop();
                audioSource.loop = false;
            }
        }
    }

    private void GestionarSonidoCansancio()
    {
        if (audioSourceRespiracion == null || sonidoCansado == null) return;

        // Sonar agitado si está exhausto o le queda menos del 30% de estamina
        bool debeSonarCansado = exhausto || (estaminaActual < (estaminaMaxima * 0.3f));

        if (debeSonarCansado)
        {
            if (!audioSourceRespiracion.isPlaying)
            {
                audioSourceRespiracion.clip = sonidoCansado;
                audioSourceRespiracion.loop = true;
                audioSourceRespiracion.volume = 0.6f;
                audioSourceRespiracion.Play();
            }
        }
        else
        {
            if (audioSourceRespiracion.isPlaying)
            {
                audioSourceRespiracion.Stop();
            }
        }
    }

    private void HandleFlashlight()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.fKey.wasPressedThisFrame)
        {
            linternaEncendida = !linternaEncendida;
            if (luzLinterna != null) luzLinterna.enabled = linternaEncendida;
            if (animator != null) animator.SetBool("IsFlashlightOn", linternaEncendida);

            if (sonidoLinterna != null && audioSource != null)
            {
                audioSource.PlayOneShot(sonidoLinterna);
            }
        }
    }
}