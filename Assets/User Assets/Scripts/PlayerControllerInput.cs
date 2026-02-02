using UnityEngine;
using UnityEngine.InputSystem;
using System;

[RequireComponent(typeof(CharacterController))]
public class PlayerControllerActions : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cameraTransform;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;     // Value/Vector2
    [SerializeField] private InputActionReference lookAction;     // Value/Vector2
    [SerializeField] private InputActionReference jumpAction;     // Button
    [SerializeField] private InputActionReference interactAction; // Button

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4.5f;

    [Header("Jump & Gravity")]
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float groundedStick = -2f;

    [Header("Look")]
    [SerializeField] private float lookSensitivity = 0.08f;
    [SerializeField] private float maxLookAngle = 85f;

    public event Action InteractPressed;

    private CharacterController controller;
    private float verticalVelocity;
    private float pitch;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool jumpQueued;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        // Make this script self-contained: enable actions here.
        // (If you later add PlayerInput, remove these Enables to avoid double-management.)
        moveAction?.action.Enable();
        lookAction?.action.Enable();
        jumpAction?.action.Enable();
        interactAction?.action.Enable();

        if (jumpAction != null)
            jumpAction.action.performed += OnJumpPerformed;
    }

    private void OnDisable()
    {
        if (jumpAction != null)
            jumpAction.action.performed -= OnJumpPerformed;
    }

    private void Update()
    {
        ReadInput();

        // Interact: read as a button edge this frame (reliable, still uses Input Actions binding)
        if (interactAction != null && interactAction.action.WasPressedThisFrame())
        {
            Debug.Log("Interact");
            InteractPressed?.Invoke();
        }

        ApplyLook();
        ApplyMovement();
    }

    private void ReadInput()
    {
        if (moveAction != null)
            moveInput = moveAction.action.ReadValue<Vector2>();
        else
            moveInput = Vector2.zero;

        if (lookAction != null)
            lookInput = lookAction.action.ReadValue<Vector2>();
        else
            lookInput = Vector2.zero;
    }

    private void ApplyLook()
    {
        if (cameraTransform == null) return;

        float yaw = lookInput.x * lookSensitivity;
        float look = lookInput.y * lookSensitivity;

        transform.Rotate(Vector3.up * yaw);

        pitch -= look;
        pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);
        cameraTransform.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }

    private void ApplyMovement()
    {
        Vector3 move = (transform.right * moveInput.x + transform.forward * moveInput.y);
        if (move.sqrMagnitude > 1f) move.Normalize();
        move *= moveSpeed;

        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = groundedStick;

        if (controller.isGrounded && jumpQueued)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpQueued = false;
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move;
        velocity.y = verticalVelocity;

        controller.Move(velocity * Time.deltaTime);
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        jumpQueued = true;
    }
}
