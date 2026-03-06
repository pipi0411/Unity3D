using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Player player;
    private PlayerControls controls;
    private CharacterController characterController;
    private Animator animator;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float rotationSpeed = 15f;

    [Header("Aim")]
    [SerializeField] private Transform aim;
    [SerializeField] private LayerMask aimLayerMask;

    private float speed;
    private Vector2 moveInput;
    private bool isRunning;
    private bool isAiming;
    private float verticalVelocity;

    private void Start()
    {
        player = GetComponent<Player>();
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        speed = walkSpeed;
        AssignInputEvents();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation();
        HandleAimRaycast();
        UpdateAnimator();
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // =========================
    // MOVE THEO CAMERA
    // =========================
    private void HandleMovement()
    {
        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = camForward * moveInput.y +
                          camRight * moveInput.x;

        if (moveDir.magnitude > 0)
        {
            characterController.Move(moveDir * speed * Time.deltaTime);
        }

        ApplyGravity();
    }

    // =========================
    // XOAY PLAYER THEO CAMERA (KHI AIM)
    // =========================
    private void HandleRotation()
    {
        if (!isAiming) return;

        Vector3 camForward = Camera.main.transform.forward;
        camForward.y = 0;

        if (camForward.sqrMagnitude < 0.01f) return;

        Quaternion targetRot = Quaternion.LookRotation(camForward);

        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            targetRot,
            rotationSpeed * Time.deltaTime
        );
    }

    // =========================
    // RAYCAST TỪ TÂM MÀN HÌNH
    // =========================
    private void HandleAimRaycast()
    {
        Ray ray = Camera.main.ScreenPointToRay(
            new Vector3(Screen.width / 2f, Screen.height / 2f)
        );

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, aimLayerMask))
        {
            aim.position = hit.point;
        }
    }

    private void ApplyGravity()
    {
        if (!characterController.isGrounded)
            verticalVelocity -= 9.81f * Time.deltaTime;
        else
            verticalVelocity = -1f;

        characterController.Move(
            Vector3.up * verticalVelocity * Time.deltaTime
        );
    }

    private void UpdateAnimator()
    {
        animator.SetFloat("xVelocity", moveInput.x, 0.1f, Time.deltaTime);
        animator.SetFloat("zVelocity", moveInput.y, 0.1f, Time.deltaTime);
        animator.SetBool("isRunning", isRunning);
    }

    private void AssignInputEvents()
    {
        controls = player.controls;

        controls.Character.Movement.performed +=
            ctx => moveInput = ctx.ReadValue<Vector2>();

        controls.Character.Movement.canceled +=
            ctx => moveInput = Vector2.zero;

        controls.Character.Run.performed += ctx =>
        {
            speed = runSpeed;
            isRunning = true;
        };

        controls.Character.Run.canceled += ctx =>
        {
            speed = walkSpeed;
            isRunning = false;
        };

        controls.Character.Aim.performed += ctx =>
            isAiming = true;

        controls.Character.Aim.canceled += ctx =>
            isAiming = false;
    }
}