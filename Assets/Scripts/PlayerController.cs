using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float sprintSpeedMultiplier = 1.75f;
    [SerializeField] float jumpHeight = 1.25f;
    [SerializeField] float gravity = -20f;
    [SerializeField] Transform cameraTransform;

    [Header("Jump Feel")]
    [SerializeField] float coyoteTime = 0.15f;
    [SerializeField] float jumpBufferTime = 0.12f;
    [SerializeField] float groundCheckDistance = 0.2f;
    [SerializeField] LayerMask groundMask = ~0;

    [Header("Void Fall")]
    [SerializeField] bool enableVoidFallPenalty;
    [SerializeField] float voidDamageStartY = 0.05f;
    [SerializeField] float voidForceKillY = -8f;
    [SerializeField] float voidDamagePerSecond = 50f;

    CharacterController controller;
    PlayerStats stats;
    PlayerCameraController cameraController;
    Vector3 velocity;
    float lastGroundedTime;
    float lastJumpPressedTime;
    Vector3 spawnPosition;
    Quaternion spawnRotation;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        stats = GetComponent<PlayerStats>();
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (cameraTransform != null)
        {
            cameraController = cameraTransform.GetComponent<PlayerCameraController>();
        }

        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
        stats.OnDeath += HandleDeath;
    }

    void Start()
    {
        enableVoidFallPenalty = SceneManager.GetActiveScene().name == "Level2";
    }

    void OnDestroy()
    {
        if (stats != null)
        {
            stats.OnDeath -= HandleDeath;
        }
    }

    void HandleDeath()
    {
        Invoke(nameof(Respawn), 1.5f);
    }

    void Respawn()
    {
        controller.enabled = false;
        transform.SetPositionAndRotation(spawnPosition, spawnRotation);
        controller.enabled = true;
        velocity = Vector3.zero;
        stats.ResetHealth();
    }

    void Update()
    {
        HandleVoidFallPenalty();

        if (!stats.IsAlive)
        {
            return;
        }
        bool shiftHeld = Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed;
        bool spaceHeld = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;

        stats.RegisterStaminaInput(shiftHeld, spaceHeld);

        bool grounded = IsGrounded();
        if (grounded && velocity.y < 0f)
        {
            velocity.y = -2f;
            lastGroundedTime = Time.time;
        }
        else if (grounded)
        {
            lastGroundedTime = Time.time;
        }

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            lastJumpPressedTime = Time.time;
        }

        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) vertical += 1f;
            if (Keyboard.current.sKey.isPressed) vertical -= 1f;
            if (Keyboard.current.aKey.isPressed) horizontal -= 1f;
            if (Keyboard.current.dKey.isPressed) horizontal += 1f;
        }

        Vector3 input = new Vector3(horizontal, 0f, vertical);
        if (input.sqrMagnitude > 1f)
        {
            input.Normalize();
        }

        Vector3 move = Vector3.zero;
        if (cameraTransform != null)
        {
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();
            move = forward * input.z + right * input.x;
        }
        else
        {
            move = transform.right * input.x + transform.forward * input.z;
        }

        bool wantsSprint = shiftHeld && move.sqrMagnitude > 0.001f && stats.CanSprint();
        float speed = moveSpeed * (wantsSprint ? sprintSpeedMultiplier : 1f);

        if (shiftHeld && move.sqrMagnitude > 0.001f)
        {
            stats.DrainSprint(Time.deltaTime);
        }

        bool canJump = Time.time - lastGroundedTime <= coyoteTime;
        bool wantsJump = Time.time - lastJumpPressedTime <= jumpBufferTime;

        if (canJump && wantsJump && stats.TryConsumeJumpStamina())
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            lastJumpPressedTime = -999f;
            lastGroundedTime = -999f;
        }
        else if (canJump && wantsJump)
        {
            lastJumpPressedTime = -999f;
        }

        velocity.y += gravity * Time.deltaTime;

        Vector3 displacement = move * speed * Time.deltaTime;
        displacement += velocity * Time.deltaTime;
        controller.Move(displacement);

        if (cameraController != null && cameraController.IsFirstPerson)
        {
            transform.rotation = Quaternion.Euler(0f, cameraController.Yaw, 0f);
        }
        else if (move.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 12f * Time.deltaTime);
        }
    }

    void HandleVoidFallPenalty()
    {
        if (!enableVoidFallPenalty || stats == null || !stats.IsAlive)
        {
            return;
        }

        float y = transform.position.y;
        if (y < voidDamageStartY)
        {
            stats.ApplyContinuousDamage(voidDamagePerSecond);
        }

        if (y < voidForceKillY)
        {
            stats.ForceKill();
        }
    }

    bool IsGrounded()
    {
        if (controller.isGrounded && velocity.y <= 0f)
        {
            return true;
        }

        float radius = controller.radius * 0.95f;
        Vector3 origin = transform.position + controller.center + Vector3.up * radius;
        float distance = groundCheckDistance + controller.skinWidth;

        return Physics.SphereCast(
            origin,
            radius,
            Vector3.down,
            out _,
            distance,
            groundMask,
            QueryTriggerInteraction.Ignore);
    }
}
