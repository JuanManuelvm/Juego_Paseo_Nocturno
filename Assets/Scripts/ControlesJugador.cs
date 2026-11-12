using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class ControlesJugador : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidadCaminando = 5f;
    public float velocidadCorriendo = 8.5f;

    [Header("Cámara y Ratón")]
    public Transform camaraJugador;
    public float sensibilidadRaton = 0.1f;
    public float limiteVertical = 80f;

    [Header("Linterna y Animaciones")]
    public Light luzLinterna;
    public Animator animator;
    
    private CharacterController characterController;
    private float rotacionX = 0f;
    private bool linternaEncendida = false;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
        
	linternaEncendida = true; 
        if (luzLinterna != null)
        {
            luzLinterna.enabled = linternaEncendida;
        }
        if (animator != null)
	{
	    animator.SetBool("IsFlashlightOn", linternaEncendida);
	}
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

        // Detectar si presiona Shift (Sprint) solo si se está moviendo hacia adelante o a los lados
        bool estaCorriendo = estaMoviendose && (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed);

        if (moveInput.magnitude > 1f)
        {
            moveInput.Normalize();
        }

        // Seleccionar velocidad según si corre o camina
        float velocidadActual = estaCorriendo ? velocidadCorriendo : velocidadCaminando;
        moveInput = transform.TransformDirection(moveInput) * velocidadActual;
        characterController.Move(moveInput * Time.deltaTime);

        // Enviar parámetros al Animator Controller
        if (animator != null)
        {
            animator.SetBool("IsWalking", estaMoviendose && !estaCorriendo);
            animator.SetBool("IsSprinting", estaCorriendo);
            animator.SetBool("IsFlashlightOn", linternaEncendida);
        }
    }

    private void HandleFlashlight()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.fKey.wasPressedThisFrame)
        {
            linternaEncendida = !linternaEncendida;

            if (luzLinterna != null)
            {
                luzLinterna.enabled = linternaEncendida;
            }

            if (animator != null)
            {
                animator.SetBool("IsFlashlightOn", linternaEncendida);
            }
        }
    }
}
