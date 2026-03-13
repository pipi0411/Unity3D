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
    [SerializeField] private float rotationSpeed = 15f; // Tăng nhẹ để xoay mượt hơn

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

        // Khóa chuột để POV của Cinemachine hoạt động chuẩn
        SetCursorLocked(true);
    }

    private void Update()
    {
        HandleCursorToggle();
        HandleAim();
        HandleMovement();
        HandleRotation();
        ApplyGravity();
        UpdateAnimator();
    }

    private void HandleCursorToggle()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.leftAltKey.wasPressedThisFrame || keyboard.rightAltKey.wasPressedThisFrame)
        {
            bool shouldUnlock = Cursor.lockState == CursorLockMode.Locked;
            SetCursorLocked(!shouldUnlock);
        }
    }

    private void SetCursorLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    private void HandleAim()
    {
        if (aim == null) return;

        // Bắn Ray từ tâm màn hình
        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, aimLayerMask))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(20f);
        }

        aim.position = Vector3.SmoothDamp(aim.position, targetPoint, ref aimVelocity, aimSmoothTime);
    }

    private void HandleMovement()
    {
        Vector3 camForward = cam.transform.forward;
        Vector3 camRight = cam.transform.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 move = camForward * moveInput.y + camRight * moveInput.x;
        move = Vector3.ClampMagnitude(move, 1f);

        characterController.Move(move * speed * Time.deltaTime);
    }

    private void HandleRotation()
    {
        Vector3 targetDirection = Vector3.zero;

        if (isAiming)
        {
            // Khi ngắm: Luôn xoay mặt về phía điểm Aim
            targetDirection = aim.position - transform.position;
        }
        else if (moveInput.sqrMagnitude > 0.01f)
        {
            // Khi di chuyển bình thường: Xoay mặt theo hướng chạy (dựa trên Camera)
            Vector3 camForward = cam.transform.forward;
            camForward.y = 0;
            targetDirection = camForward * moveInput.y + cam.transform.right * moveInput.x;
        }

        if (targetDirection != Vector3.zero)
        {
            targetDirection.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void ApplyGravity()
    {
        if (characterController.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;
        characterController.Move(Vector3.up * verticalVelocity * Time.deltaTime);
    }

    private void UpdateAnimator()
    {
        animator.SetFloat("xVelocity", moveInput.x, 0.1f, Time.deltaTime);
        animator.SetFloat("zVelocity", moveInput.y, 0.1f, Time.deltaTime);
        bool isActuallyMoving = moveInput.sqrMagnitude > 0.01f;
        animator.SetBool("isRunning", isRunning && isActuallyMoving);
    }

    private void AssignInputEvents()
    {
        controls = player.controls;
        controls.Character.Movement.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Character.Movement.canceled += ctx => moveInput = Vector2.zero;

        controls.Character.Run.performed += ctx => { speed = runSpeed; isRunning = true; };
        controls.Character.Run.canceled += ctx => { speed = walkSpeed; isRunning = false; };

        controls.Character.Aim.performed += ctx => isAiming = true;
        controls.Character.Aim.canceled += ctx => isAiming = false;
    }
}