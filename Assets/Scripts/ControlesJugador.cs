using UnityEngine;
using UnityEngine.InputSystem; // Importante para el nuevo Input System

[RequireComponent(typeof(CharacterController))]
public class ControlesJugador : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 5f;

    [Header("Cámara y Ratón")]
    public Transform camaraJugador; // Arrastra el objeto de la Cámara desde el Inspector
    public float sensibilidadRaton = 0.1f;
    public float limiteVertical = 80f; // Máximo ángulo para mirar arriba/abajo

    private CharacterController characterController;
    private float rotacionX = 0f; // Controla la inclinación vertical de la cámara

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Start()
    {
        // Bloquear el ratón en el centro de la pantalla y ocultarlo
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        Look();
        Move();
    }

    private void Look()
    {
        // Verificar si el ratón está conectado
        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        // Leer el movimiento delta del ratón
        Vector2 deltaRaton = mouse.delta.ReadValue() * sensibilidadRaton;

        // --- Rotación Vertical (Arriba / Abajo) ---
        rotacionX -= deltaRaton.y;
        rotacionX = Mathf.Clamp(rotacionX, -limiteVertical, limiteVertical); // Limitar el ángulo

        if (camaraJugador != null)
        {
            camaraJugador.localRotation = Quaternion.Euler(rotacionX, 0f, 0f);
        }

        // --- Rotación Horizontal (Izquierda / Derecha) ---
        transform.Rotate(Vector3.up * deltaRaton.x);
    }

    private void Move()
    {
        // Verificamos si el teclado está conectado
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        float moveX = 0f;
        float moveZ = 0f;

        // Leer flechas o teclas WASD
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) moveX += 1f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) moveX -= 1f;
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) moveZ += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) moveZ -= 1f;

        Vector3 moveInput = new Vector3(moveX, 0f, moveZ);

        // Normalizar para evitar velocidad extra en diagonal
        if (moveInput.magnitude > 1f)
        {
            moveInput.Normalize();
        }

        // El movimiento se calcula relativo a la dirección en la que está mirando el personaje
        moveInput = transform.TransformDirection(moveInput) * velocidad;
        characterController.Move(moveInput * Time.deltaTime);
    }
}