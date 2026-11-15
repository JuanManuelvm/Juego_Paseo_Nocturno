using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class ControlesJugador : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidadCaminando = 5f;
    public float velocidadCorriendo = 8.5f;
    public float velocidadAgachado = 2.5f;
    public float gravedad = -9.81f;

    [Header("Mecánica de Agachado")]
    public float alturaDePie = 2.0f;
    public float alturaAgachado = 1.0f;
    public float alturaCamaraDePie = 0.8f;
    public float alturaCamaraAgachado = 0.2f;
    public float velocidadTransicionAgachado = 10f;
    private bool estaAgachado = false;

    [Header("Sistema de Estamina")]
    public float estaminaMaxima = 100f;
    public float estaminaActual;
    public float consumoEstamina = 20f;
    public float recargaEstamina = 15f;
    private bool exhausto = false;

    [Header("Cámara y Ratón")]
    public Transform camaraJugador;
    public float sensibilidadRaton = 0.1f;
    public float limiteVertical = 80f;

    [Header("Linterna y Animaciones")]
    public Light luzLinterna;
    public Animator animator;

    [Header("Audio SFX")]
    public AudioSource audioSource;
    public AudioSource audioSourceRespiracion;
    public AudioClip sonidoPasosCaminar;
    public AudioClip sonidoPasosCorrer;
    public AudioClip sonidoLinterna;
    public AudioClip sonidoCansado;

    private CharacterController characterController;
    private float rotacionX = 0f;
    private bool linternaEncendida = true;
    private float velocidadY = 0f;

    private Vector3 centroDePie;
    private Vector3 centroAgachado;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        estaminaActual = estaminaMaxima;

        // Configurar los centros del CharacterController según la altura
        centroDePie = characterController.center;
        centroAgachado = new Vector3(centroDePie.x, alturaAgachado / 2f, centroDePie.z);
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (animator == null) animator = GetComponentInChildren<Animator>();

        linternaEncendida = true;
        if (luzLinterna != null) luzLinterna.enabled = linternaEncendida;
        if (animator != null) animator.SetBool("IsFlashlightOn", linternaEncendida);

        // Asentar la cámara exactamente en su altura de pie sin Lerp al iniciar
        if (camaraJugador != null)
        {
            Vector3 posInicial = camaraJugador.localPosition;
            posInicial.y = alturaCamaraDePie;
            camaraJugador.localPosition = posInicial;
        }

        // Forzar asentamiento inicial en el piso para evitar la caída del primer frame
        velocidadY = -2f;
    }

    private void Update()
    {
        Look();
        HandleCrouch();
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

    private void HandleCrouch()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.cKey.wasPressedThisFrame || keyboard.leftCtrlKey.wasPressedThisFrame)
        {
            estaAgachado = !estaAgachado;
        }

        float alturaObjetivo = estaAgachado ? alturaAgachado : alturaDePie;
        Vector3 centroObjetivo = estaAgachado ? centroAgachado : centroDePie;

        characterController.height = Mathf.Lerp(characterController.height, alturaObjetivo, Time.deltaTime * velocidadTransicionAgachado);
        characterController.center = Vector3.Lerp(characterController.center, centroObjetivo, Time.deltaTime * velocidadTransicionAgachado);

        if (camaraJugador != null)
        {
            float alturaCamaraObjetivo = estaAgachado ? alturaCamaraAgachado : alturaCamaraDePie;
            Vector3 posCamara = camaraJugador.localPosition;
            
            // Solo suavizar si hay una diferencia notable de altura (evita micro-caídas de inicio)
            if (Mathf.Abs(posCamara.y - alturaCamaraObjetivo) > 0.01f)
            {
                posCamara.y = Mathf.Lerp(posCamara.y, alturaCamaraObjetivo, Time.deltaTime * velocidadTransicionAgachado);
                camaraJugador.localPosition = posCamara;
            }
        }
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

        // Gestión del agotamiento
        if (estaminaActual <= 0f) exhausto = true;
        else if (estaminaActual >= 25f) exhausto = false;

        // NO se puede correr si se está agachado
        bool estaCorriendo = estaMoviendose && intentaCorrer && !exhausto && !estaAgachado;

        // Consumo / Regeneración de Estamina
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

        GestionarSonidoCansancio();

        if (moveInput.magnitude > 1f) moveInput.Normalize();

        // Determinar velocidad según estado (Agachado, Corriendo o Caminando)
        float velocidadActual = velocidadCaminando;
        if (estaAgachado)
        {
            velocidadActual = velocidadAgachado;
        }
        else if (estaCorriendo)
        {
            velocidadActual = velocidadCorriendo;
        }

        Vector3 movimientoHorizontal = transform.TransformDirection(moveInput) * velocidadActual;

        if (characterController.isGrounded && velocidadY < 0)
        {
            velocidadY = -2f;
        }
        velocidadY += gravedad * Time.deltaTime;

        Vector3 movimientoFinal = movimientoHorizontal + new Vector3(0f, velocidadY, 0f);
        characterController.Move(movimientoFinal * Time.deltaTime);

        // Actualización de parámetros en el Animator
        if (animator != null)
        {
            animator.SetBool("IsCrouching", estaAgachado);
            animator.SetBool("IsWalking", estaMoviendose && !estaCorriendo && !estaAgachado);
            animator.SetBool("IsCrouchWalking", estaMoviendose && estaAgachado);
            animator.SetBool("IsSprinting", estaCorriendo);
        }

        // Audio de Pasos
        if (estaMoviendose && characterController.isGrounded)
        {
            AudioClip clipDeseado = estaCorriendo ? sonidoPasosCorrer : sonidoPasosCaminar;
            float volumenDeseado = estaAgachado ? 0.15f : (estaCorriendo ? 0.45f : 0.25f);

            if (audioSource != null && clipDeseado != null)
            {
                if (!audioSource.isPlaying || audioSource.clip != clipDeseado)
                {
                    audioSource.clip = clipDeseado;
                    audioSource.volume = volumenDeseado;
                    audioSource.loop = true;
                    audioSource.Play();
                }
                else
                {
                    audioSource.volume = volumenDeseado;
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