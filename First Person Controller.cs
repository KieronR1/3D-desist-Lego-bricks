using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    public Transform playerCamera;

    [Header("Movement")]
    public float moveSpeed = 15f;
    public float sprintSpeed = 30f;
    public float jumpHeight = 6f;
    public float gravity = -30f;

    [Header("Camera")]
    public float mouseSensitivity = 0.1f;

    private CharacterController controller;
    private float verticalSpeed;
    private float cameraPitch;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (playerCamera == null)
        {
            Camera childCamera = GetComponentInChildren<Camera>();
            if (childCamera != null)
                playerCamera = childCamera.transform;
        }

        LockCursor();
    }

    void Update()
    {
        Debug.Log(controller.isGrounded);

        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;

        // Escape releases the cursor; clicking locks it again.
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            LockCursor();
        }

        bool controlsActive =
            Cursor.lockState == CursorLockMode.Locked;

        Vector2 input = Vector2.zero;
        bool sprinting = false;
        bool jumpPressed = false;

        if (controlsActive)
        {
            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed) input.y += 1;
                if (keyboard.sKey.isPressed) input.y -= 1;
                if (keyboard.dKey.isPressed) input.x += 1;
                if (keyboard.aKey.isPressed) input.x -= 1;

                sprinting = keyboard.leftShiftKey.isPressed;
                jumpPressed = keyboard.spaceKey.wasPressedThisFrame;
            }
            if (mouse != null && playerCamera != null)
            {
                Vector2 look =
                    mouse.delta.ReadValue() * mouseSensitivity;

                transform.Rotate(Vector3.up, look.x);

                cameraPitch =
                    Mathf.Clamp(cameraPitch + look.y, -85f, 85f);

                playerCamera.localRotation =
                    Quaternion.Euler(cameraPitch, 0f, 0f);
            }
        }

        // Prevent diagonal movement being faster.
        input = Vector2.ClampMagnitude(input, 1f);

        float speed = sprinting ? sprintSpeed : moveSpeed;

        Vector3 forward = playerCamera != null
            ? playerCamera.forward
            : transform.forward;
        Vector3 right = playerCamera != null
            ? playerCamera.right
            : transform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 movement = (right * input.x + forward * input.y) * speed;

        if (controller.isGrounded)
        {
            if (verticalSpeed < 0f)
                verticalSpeed = -2f;

            if (jumpPressed)
                verticalSpeed = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalSpeed += gravity * Time.deltaTime;
        movement.y = verticalSpeed;

        CollisionFlags collisions =
            controller.Move(movement * Time.deltaTime);

        // Stop rising if the player hits a ceiling.
        if ((collisions & CollisionFlags.Above) != 0 &&
            verticalSpeed > 0f)
        {
            verticalSpeed = 0f;
        }
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}