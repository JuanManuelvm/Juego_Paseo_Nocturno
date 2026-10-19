using UnityEngine;
using UnityEngine.InputSystem; // Importante para el nuevo Input System

[RequireComponent(typeof(CharacterController))]
public class ControlesJugador : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 5f;
    
    private CharacterController characterController;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        Move();
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

        moveInput = transform.TransformDirection(moveInput) * velocidad;
        characterController.Move(moveInput * Time.deltaTime);
    }
}