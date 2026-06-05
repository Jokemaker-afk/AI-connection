using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerStats))]
[DefaultExecutionOrder(50)]
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
    [SerializeField] float voidFallBelowGroundMargin = 1.2f;

    [Header("Third-Person Movement (Fortnite-style)")]
    [SerializeField] MovementMode movementMode = MovementMode.AimFacingStrafe;

    [Header("Visual Facing (independent from movement)")]
    [SerializeField] float visualAimTurnSpeed = 24f;
    [SerializeField] bool useInstantVisualAimRotation = false;
    [SerializeField] bool useInstantVisualAimInThirdPerson = true;
    [SerializeField] bool rotateMovementRootInFirstPerson = true;
    [SerializeField] bool pauseVisualFacingWhenMenuOpen = true;

    [Header("Placement Aim Override")]
    [SerializeField] bool rotateVisualTowardAimWhenPlacing = true;

    CharacterController controller;
    PlayerStats stats;
    PlayerBuffController buffController;
    PlayerCameraController cameraController;
    PlayerPlacementController placementController;
    Transform visualRoot;
    Vector3 velocity;
    Vector3 lastMovementDirection = Vector3.forward;
    Vector3 lastValidAimForward = Vector3.forward;
    float lastGroundedTime;
    float lastJumpPressedTime;
    float lastSafeGroundY;
    Vector3 spawnPosition;
    Quaternion spawnRotation;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        stats = EnsureSinglePlayerStats();
        if (GetComponent<PlayerBuffController>() == null)
        {
            gameObject.AddComponent<PlayerBuffController>();
        }

        if (GetComponent<PlayerShieldVisual>() == null)
        {
            gameObject.AddComponent<PlayerShieldVisual>();
        }

        buffController = GetComponent<PlayerBuffController>();
        PlayerVisualBuilder.EnsurePlayerVisual(gameObject);
        CacheVisualRoot();
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (cameraTransform != null)
        {
            cameraController = cameraTransform.GetComponent<PlayerCameraController>();
        }

        placementController = GetComponent<PlayerPlacementController>();

        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
        lastSafeGroundY = transform.position.y;
        lastValidAimForward = transform.forward;
        stats.OnDeath += HandleDeath;
    }

    public void BindCameraTransform(Transform camera)
    {
        cameraTransform = camera;
        cameraController = camera != null ? camera.GetComponent<PlayerCameraController>() : null;
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().name == "Level2")
        {
            enableVoidFallPenalty = true;
            voidFallBelowGroundMargin = 1.2f;
            voidDamageStartY = -0.5f;
        }

        lastSafeGroundY = transform.position.y;
        BindGameplayHud();
    }

    PlayerStats EnsureSinglePlayerStats()
    {
        var allStats = GetComponents<PlayerStats>();
        if (allStats.Length == 0)
        {
            return gameObject.AddComponent<PlayerStats>();
        }

        for (int i = 1; i < allStats.Length; i++)
        {
            Destroy(allStats[i]);
        }

        return allStats[0];
    }

    void BindGameplayHud()
    {
        GameplayHudBootstrap.BindAll();
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
        stats.ResetForRespawn();
        lastSafeGroundY = spawnPosition.y;
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
        if (grounded)
        {
            lastSafeGroundY = transform.position.y;
        }

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

        if (!GameplayCursorPolicy.IsAnyMenuOpen && Keyboard.current != null)
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

        Vector3 move = BuildMovementDirection(input);
        if (move.sqrMagnitude > 0.001f)
        {
            lastMovementDirection = move.normalized;
        }

        bool wantsSprint = shiftHeld && move.sqrMagnitude > 0.001f && stats.CanSprint();
        float buffSpeed = buffController != null ? buffController.GetSpeedMultiplier() : 1f;
        float speed = moveSpeed * buffSpeed * (wantsSprint ? sprintSpeedMultiplier : 1f);

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

        ApplyFirstPersonRootRotation();
    }

    void LateUpdate()
    {
        if (!stats.IsAlive)
        {
            return;
        }

        if (pauseVisualFacingWhenMenuOpen && GameplayCursorPolicy.IsAnyMenuOpen)
        {
            return;
        }

        UpdateVisualFacing();
    }

    Vector3 BuildMovementDirection(Vector3 input)
    {
        if (input.sqrMagnitude <= 0.001f)
        {
            return Vector3.zero;
        }

        GetMovementPlanarAxes(out Vector3 aimForward, out Vector3 aimRight);
        return aimForward * input.z + aimRight * input.x;
    }

    void GetMovementPlanarAxes(out Vector3 aimForward, out Vector3 aimRight)
    {
        switch (movementMode)
        {
            case MovementMode.MoveFacing:
                if (lastMovementDirection.sqrMagnitude > 0.001f)
                {
                    aimForward = lastMovementDirection;
                    aimForward.y = 0f;
                    aimForward.Normalize();
                    aimRight = Vector3.Cross(Vector3.up, aimForward).normalized;
                    return;
                }

                GetImmediateAimPlanarAxes(out aimForward, out aimRight);
                return;
            case MovementMode.CameraRelativeFreeMove:
            case MovementMode.AimFacingStrafe:
            default:
                GetImmediateAimPlanarAxes(out aimForward, out aimRight);
                return;
        }
    }

    void GetImmediateAimPlanarAxes(out Vector3 aimForward, out Vector3 aimRight)
    {
        if (cameraController != null)
        {
            aimForward = cameraController.GetImmediateAimForward();
            aimRight = cameraController.GetImmediateAimRight();
            return;
        }

        if (cameraTransform != null)
        {
            aimForward = cameraTransform.forward;
            aimRight = cameraTransform.right;
            aimForward.y = 0f;
            aimRight.y = 0f;
            aimForward.Normalize();
            aimRight.Normalize();
            return;
        }

        aimForward = lastValidAimForward;
        aimRight = Vector3.Cross(Vector3.up, aimForward).normalized;
    }

    void UpdateVisualFacing()
    {
        Vector3 desiredForward = GetVisualFacingForward();
        if (desiredForward.sqrMagnitude < 0.001f)
        {
            return;
        }

        ApplyVisualRootFacing(desiredForward.normalized);
    }

    Vector3 GetVisualFacingForward()
    {
        if (ShouldUsePlacementAimOverride())
        {
            return GetImmediateAimForward();
        }

        switch (movementMode)
        {
            case MovementMode.MoveFacing:
                if (lastMovementDirection.sqrMagnitude > 0.001f)
                {
                    return lastMovementDirection;
                }

                return GetImmediateAimForward();
            case MovementMode.CameraRelativeFreeMove:
            case MovementMode.AimFacingStrafe:
            default:
                return GetImmediateAimForward();
        }
    }

    bool ShouldUsePlacementAimOverride()
    {
        return rotateVisualTowardAimWhenPlacing
            && placementController != null
            && placementController.IsPlacementActive;
    }

    Vector3 GetImmediateAimForward()
    {
        if (AimReferenceProvider.Instance != null && !AimReferenceProvider.Instance.IsWorldAimingBlocked)
        {
            Vector3 aimForward = AimReferenceProvider.Instance.GetFlatAimDirection();
            lastValidAimForward = aimForward;
            return aimForward;
        }

        if (cameraController != null)
        {
            Vector3 aimForward = cameraController.GetImmediateAimForward();
            lastValidAimForward = aimForward;
            return aimForward;
        }

        Vector3 fallback = GetCrosshairAimForwardPlanar();
        if (fallback.sqrMagnitude > 0.001f)
        {
            lastValidAimForward = fallback;
        }

        return fallback;
    }

    Vector3 GetCrosshairAimForwardPlanar()
    {
        Ray crosshairRay = CrosshairRayUtility.GetCrosshairRay(out _);
        Vector3 aimForward = crosshairRay.direction;
        aimForward.y = 0f;

        if (aimForward.sqrMagnitude < 0.001f)
        {
            return lastValidAimForward;
        }

        aimForward.Normalize();
        return aimForward;
    }

    void ApplyVisualRootFacing(Vector3 desiredForward)
    {
        CacheVisualRoot();
        if (visualRoot == null)
        {
            return;
        }

        desiredForward.y = 0f;
        if (desiredForward.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(desiredForward.normalized);
        if (useInstantVisualAimRotation || IsThirdPersonInstantAim())
        {
            visualRoot.rotation = targetRotation;
            return;
        }

        float blend = Mathf.Clamp01(visualAimTurnSpeed * Time.deltaTime);
        visualRoot.rotation = Quaternion.Slerp(visualRoot.rotation, targetRotation, blend);
    }

    bool IsThirdPersonInstantAim()
    {
        return useInstantVisualAimInThirdPerson
            && cameraController != null
            && !cameraController.IsFirstPerson;
    }

    void ApplyFirstPersonRootRotation()
    {
        if (!rotateMovementRootInFirstPerson
            || cameraController == null
            || !cameraController.IsFirstPerson)
        {
            return;
        }

        transform.rotation = Quaternion.Euler(0f, cameraController.Yaw, 0f);
    }

    void CacheVisualRoot()
    {
        if (visualRoot == null)
        {
            visualRoot = transform.Find(PlayerVisualBuilder.VisualRootName);
        }
    }

    void HandleVoidFallPenalty()
    {
        if (!enableVoidFallPenalty || stats == null || !stats.IsAlive)
        {
            return;
        }

        float y = transform.position.y;
        if (IsGrounded())
        {
            lastSafeGroundY = y;
            return;
        }

        float fallDamageThreshold = lastSafeGroundY - voidFallBelowGroundMargin;
        if (y < fallDamageThreshold || y < voidDamageStartY)
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
