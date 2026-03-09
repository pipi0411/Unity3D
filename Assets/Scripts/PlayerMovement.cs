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
    [SerializeField] private float rotationSpeed = 12f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -9.81f;

    [Header("Aim")]
    [SerializeField] private Transform aim;
    [SerializeField] private float aimSmoothTime = 0.05f;
    [SerializeField] private LayerMask aimLayerMask;

    private Vector2 moveInput;
    private float speed;
    private float verticalVelocity;

    private bool isRunning;
    private bool isAiming;

    private Camera cam;

    private Vector3 aimVelocity;

    private void Start()
    {
        player = GetComponent<Player>();
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        cam = Camera.main;

        speed = walkSpeed;

        AssignInputEvents();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleAim();
        HandleMovement();
        HandleRotation();
        ApplyGravity();
        UpdateAnimator();
    }

    // =========================
    // AIM SYSTEM
    // =========================
    private void HandleAim()
    {
        Ray ray = cam.ScreenPointToRay(
            new Vector3(Screen.width / 2f, Screen.height / 2f)
        );

        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, aimLayerMask))
        {
            targetPoint = hit.point;
        }
        else
        {
            // fallback nếu ray không trúng collider
            targetPoint = ray.GetPoint(20f);
        }

        aim.position = Vector3.SmoothDamp(
            aim.position,
            targetPoint,
            ref aimVelocity,
            aimSmoothTime
        );
    }

    // =========================
    // MOVEMENT
    // =========================
    private void HandleMovement()
    {
        Vector3 camForward = cam.transform.forward;
        Vector3 camRight = cam.transform.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 move =
            camForward * moveInput.y +
            camRight * moveInput.x;

        move = Vector3.ClampMagnitude(move, 1f);

        characterController.Move(move * speed * Time.deltaTime);
    }

    // =========================
    // ROTATION
    // =========================
    private void HandleRotation()
    {
        if (!isAiming) return;

        Vector3 direction = aim.position - transform.position;
        direction.y = 0;

        if (direction.magnitude < 0.5f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    // =========================
    // GRAVITY
    // =========================
    private void ApplyGravity()
    {
        if (characterController.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;

        characterController.Move(
            Vector3.up * verticalVelocity * Time.deltaTime
        );
    }

    // =========================
    // ANIMATOR
    // =========================
    private void UpdateAnimator()
    {
        animator.SetFloat("xVelocity", moveInput.x, 0.1f, Time.deltaTime);
        animator.SetFloat("zVelocity", moveInput.y, 0.1f, Time.deltaTime);
        animator.SetBool("isRunning", isRunning);
    }

    // =========================
    // INPUT
    // =========================
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