using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Third-person over-the-shoulder camera. Crosshair aims along camera forward; player stays offset on screen.
/// </summary>
[DefaultExecutionOrder(40)]
public class PlayerCameraController : MonoBehaviour
{
    public enum CameraMode
    {
        ThirdPerson,
        FirstPerson
    }

    [SerializeField] Transform target;
    [SerializeField] Key toggleKey = Key.V;

    [Header("Composition Preset")]
    [SerializeField] ThirdPersonCompositionPreset compositionPreset = ThirdPersonCompositionPreset.CombatOverShoulder;
    [SerializeField] bool applyPresetOnStart = true;

    [Header("Third Person Over-Shoulder")]
    [SerializeField] float thirdPersonCameraDistance = 4f;
    [SerializeField] float thirdPersonCameraHeight = 0.55f;
    [SerializeField] float thirdPersonShoulderOffset = 0.6f;
    [SerializeField] float thirdPersonLookAtHeight = 1.3f;
    [SerializeField] float thirdPersonCameraPitch = 10f;
    [SerializeField] bool useRightShoulder = true;
    [SerializeField] Vector3 thirdPersonCameraOffset = Vector3.zero;
    [SerializeField] Vector2 crosshairScreenOffset = Vector2.zero;
    [SerializeField] float thirdPersonFieldOfView = 68f;
    [SerializeField] bool thirdPersonLookAtPivot = false;

    [Header("Third Person Smoothing")]
    [SerializeField] bool smoothThirdPersonCameraPosition = false;
    [SerializeField] float cameraFollowSmoothTime = 0.04f;
    [SerializeField] float cameraRotationSmoothTime = 0f;
    [SerializeField] bool alwaysFollowTarget = false;

    [Header("Third Person Collision")]
    [SerializeField] float cameraCollisionRadius = 0.28f;
    [SerializeField] float minThirdPersonDistance = 1.35f;
    [SerializeField] LayerMask cameraObstacleMask = ~0;

    [Header("First Person")]
    [SerializeField] Vector3 firstPersonEyeOffset = new Vector3(0f, 0.6f, 0f);
    [SerializeField] bool hidePlayerMeshInFirstPerson = true;
    [SerializeField] float firstPersonFieldOfView = 75f;

    [Header("Mouse Look")]
    [SerializeField] float mouseSensitivity = 0.15f;
    [SerializeField] float minPitch = -80f;
    [SerializeField] float maxPitch = 80f;
    [SerializeField] float thirdPersonMinPitch = -18f;
    [SerializeField] float thirdPersonMaxPitch = 52f;
    [SerializeField] bool lockCursorInFirstPerson = true;

    [Header("Debug")]
    [SerializeField] bool showThirdPersonCompositionDebug = true;

    float yaw;
    float pitch;
    float thirdPersonDistanceCurrent;
    CameraMode mode = CameraMode.ThirdPerson;
    Camera viewCamera;
    Renderer[] playerRenderers;
    CharacterController targetController;
    bool cursorLocked;
    Vector3 thirdPersonVelocity;
    ThirdPersonCompositionDebug compositionDebug;

    public CameraMode Mode => mode;
    public bool IsFirstPerson => mode == CameraMode.FirstPerson;
    public float Yaw => yaw;
    public float Pitch => pitch;
    public Vector3 AimForward => transform.forward;
    public Vector2 CrosshairScreenOffset => crosshairScreenOffset;
    public Transform Target => target;
    public float ThirdPersonDistance => thirdPersonCameraDistance;
    public float ThirdPersonHeight => thirdPersonCameraHeight;
    public float ShoulderOffsetX => thirdPersonShoulderOffset;
    public float ThirdPersonLookAtHeight => thirdPersonLookAtHeight;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        targetController = target != null ? target.GetComponent<CharacterController>() : null;
        CachePlayerRenderers();
        if (target != null)
        {
            yaw = target.eulerAngles.y;
        }
    }

    /// <summary>Horizontal aim forward from center-screen crosshair ray (flattened).</summary>
    public Vector3 GetImmediateAimForward()
    {
        if (AimReferenceProvider.TryGetFlatAim(out Vector3 flatAim))
        {
            return flatAim;
        }

        return GetFlatAimFromCrosshairRay();
    }

    public Vector3 GetImmediateAimRight()
    {
        if (AimReferenceProvider.Instance != null && !AimReferenceProvider.Instance.IsWorldAimingBlocked)
        {
            return AimReferenceProvider.Instance.GetFlatAimRight();
        }

        Vector3 forward = GetImmediateAimForward();
        return Vector3.Cross(Vector3.up, forward).normalized;
    }

    Vector3 GetFlatAimFromCrosshairRay()
    {
        Ray ray = GetCrosshairRay(viewCamera);
        Vector3 flat = ray.direction;
        flat.y = 0f;
        if (flat.sqrMagnitude > 0.0001f)
        {
            return flat.normalized;
        }

        return Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
    }

    public Vector3 GetPlanarForward() => GetImmediateAimForward();
    public Vector3 GetPlanarRight() => GetImmediateAimRight();

    public void ApplyCompositionPreset(ThirdPersonCompositionPreset preset, bool resetPitch = true)
    {
        compositionPreset = preset;
        switch (preset)
        {
            case ThirdPersonCompositionPreset.Exploration:
                thirdPersonCameraDistance = 5.2f;
                thirdPersonCameraHeight = 0.7f;
                thirdPersonShoulderOffset = 0.35f;
                thirdPersonLookAtHeight = 1.25f;
                thirdPersonCameraPitch = 8f;
                break;
            case ThirdPersonCompositionPreset.BuildingPlacement:
                thirdPersonCameraDistance = 4.6f;
                thirdPersonCameraHeight = 0.85f;
                thirdPersonShoulderOffset = 0.4f;
                thirdPersonLookAtHeight = 1.35f;
                thirdPersonCameraPitch = 14f;
                break;
            case ThirdPersonCompositionPreset.CombatOverShoulder:
            default:
                thirdPersonCameraDistance = 4f;
                thirdPersonCameraHeight = 0.55f;
                thirdPersonShoulderOffset = 0.6f;
                thirdPersonLookAtHeight = 1.3f;
                thirdPersonCameraPitch = 10f;
                break;
        }

        thirdPersonLookAtPivot = false;
        thirdPersonDistanceCurrent = thirdPersonCameraDistance;
        if (resetPitch)
        {
            pitch = thirdPersonCameraPitch;
        }
    }

    public void ConfigureAlwaysFollow(bool enabled, float distance = 4f, float height = 0.55f)
    {
        alwaysFollowTarget = enabled;
        thirdPersonCameraDistance = distance;
        thirdPersonCameraHeight = height;
        thirdPersonDistanceCurrent = distance;
        mode = CameraMode.ThirdPerson;
        ApplyPlayerVisibility();
    }

    public Vector2 GetCrosshairScreenPoint()
    {
        return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f) + crosshairScreenOffset;
    }

    public Ray GetCrosshairRay(Camera camera)
    {
        if (camera == null)
        {
            return new Ray(transform.position, GetImmediateAimForward());
        }

        return camera.ScreenPointToRay(GetCrosshairScreenPoint());
    }

    void Awake()
    {
        thirdPersonDistanceCurrent = thirdPersonCameraDistance;
        pitch = thirdPersonCameraPitch;
        viewCamera = GetComponent<Camera>();
        if (viewCamera == null)
        {
            viewCamera = Camera.main;
        }

        EnsureTarget();
        EnsureCompositionDebug();
    }

    void Start()
    {
        EnsureTarget();
        CachePlayerRenderers();
        if (applyPresetOnStart)
        {
            ApplyCompositionPreset(compositionPreset, true);
        }

        if (target != null)
        {
            yaw = target.eulerAngles.y;
        }

        ApplyFieldOfView();
        SyncCompositionDebug();
    }

    void EnsureCompositionDebug()
    {
        compositionDebug = GetComponent<ThirdPersonCompositionDebug>();
        if (compositionDebug == null && showThirdPersonCompositionDebug)
        {
            compositionDebug = gameObject.AddComponent<ThirdPersonCompositionDebug>();
        }
    }

    void SyncCompositionDebug()
    {
        if (compositionDebug != null)
        {
            compositionDebug.ShowThirdPersonCompositionDebug = showThirdPersonCompositionDebug;
        }
    }

    void EnsureTarget()
    {
        if (target != null)
        {
            return;
        }

        var player = GameObject.Find("Player");
        if (player != null)
        {
            SetTarget(player.transform);
        }
    }

    void Update()
    {
        if (target == null)
        {
            return;
        }

        if (Keyboard.current != null && Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            ToggleMode();
        }

        HandleMouseLook();
        UpdateCursorLock();
    }

    void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        if (mode == CameraMode.FirstPerson)
        {
            ApplyFirstPersonCamera();
        }
        else
        {
            ApplyThirdPersonCamera();
        }

        ApplyFieldOfView();
    }

    void ApplyFieldOfView()
    {
        if (viewCamera == null)
        {
            return;
        }

        float desiredFov = mode == CameraMode.FirstPerson ? firstPersonFieldOfView : thirdPersonFieldOfView;
        if (Mathf.Abs(viewCamera.fieldOfView - desiredFov) > 0.01f)
        {
            viewCamera.fieldOfView = desiredFov;
        }
    }

    void ToggleMode()
    {
        mode = mode == CameraMode.ThirdPerson ? CameraMode.FirstPerson : CameraMode.ThirdPerson;
        thirdPersonDistanceCurrent = thirdPersonCameraDistance;
        ApplyPlayerVisibility();
        UpdateCursorLock(true);
    }

    void HandleMouseLook()
    {
        if (GameplayCursorPolicy.IsAnyMenuOpen)
        {
            return;
        }

        if (Mouse.current == null)
        {
            return;
        }

        Vector2 delta = Mouse.current.delta.ReadValue() * mouseSensitivity;
        yaw += delta.x;
        pitch -= delta.y;

        if (mode == CameraMode.FirstPerson)
        {
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }
        else
        {
            pitch = Mathf.Clamp(pitch, thirdPersonMinPitch, thirdPersonMaxPitch);
        }
    }

    void ApplyFirstPersonCamera()
    {
        Vector3 eyePosition = target.position + target.TransformVector(firstPersonEyeOffset);
        transform.position = eyePosition;
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void ApplyThirdPersonCamera()
    {
        Quaternion orbitRotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 orbitOrigin = target.position + Vector3.up * thirdPersonLookAtHeight;

        float shoulderX = useRightShoulder ? thirdPersonShoulderOffset : -thirdPersonShoulderOffset;
        Vector3 shoulderSpaceOffset = new Vector3(
            shoulderX + thirdPersonCameraOffset.x,
            thirdPersonCameraHeight + thirdPersonCameraOffset.y,
            -thirdPersonCameraDistance + thirdPersonCameraOffset.z);

        Vector3 desiredPosition = orbitOrigin + orbitRotation * shoulderSpaceOffset;
        desiredPosition = ResolveCameraCollision(orbitOrigin, desiredPosition);

        bool usePositionSmoothing = smoothThirdPersonCameraPosition
            && !alwaysFollowTarget
            && cameraFollowSmoothTime > 0.001f;

        if (usePositionSmoothing)
        {
            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref thirdPersonVelocity,
                cameraFollowSmoothTime);
        }
        else
        {
            transform.position = desiredPosition;
            thirdPersonVelocity = Vector3.zero;
        }

        Quaternion targetRotation = orbitRotation;
        if (thirdPersonLookAtPivot)
        {
            Vector3 toOrigin = orbitOrigin - transform.position;
            if (toOrigin.sqrMagnitude > 0.001f)
            {
                targetRotation = Quaternion.LookRotation(toOrigin.normalized, Vector3.up);
            }
        }

        if (cameraRotationSmoothTime > 0.001f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime / cameraRotationSmoothTime);
        }
        else
        {
            transform.rotation = targetRotation;
        }
    }

    Vector3 ResolveCameraCollision(Vector3 pivot, Vector3 desiredPosition)
    {
        Vector3 direction = desiredPosition - pivot;
        float distance = direction.magnitude;
        if (distance < 0.01f)
        {
            return desiredPosition;
        }

        direction /= distance;
        float resolvedDistance = distance;

        if (Physics.SphereCast(
            pivot,
            cameraCollisionRadius,
            direction,
            out RaycastHit hit,
            distance,
            cameraObstacleMask,
            QueryTriggerInteraction.Ignore))
        {
            if (!IsTargetCollider(hit.collider))
            {
                resolvedDistance = Mathf.Max(minThirdPersonDistance, hit.distance - cameraCollisionRadius);
            }
        }

        thirdPersonDistanceCurrent = resolvedDistance;
        return pivot + direction * resolvedDistance;
    }

    bool IsTargetCollider(Collider collider)
    {
        if (collider == null || target == null)
        {
            return false;
        }

        return collider.transform == target
            || collider.transform.IsChildOf(target)
            || collider.GetComponentInParent<CharacterController>() == targetController;
    }

    void CachePlayerRenderers()
    {
        if (target == null)
        {
            playerRenderers = null;
            return;
        }

        var visualSwitcher = target.GetComponent<PlayerVisualSwitcher>();
        if (visualSwitcher != null)
        {
            playerRenderers = visualSwitcher.GetActiveRenderers();
            return;
        }

        Transform visualRoot = target.Find(PlayerVisualBuilder.VisualRootName);
        if (visualRoot != null)
        {
            Transform activeModel = visualRoot.Find(PlayerVisualBuilder.DirectionalModelName);
            playerRenderers = activeModel != null
                ? activeModel.GetComponentsInChildren<Renderer>(true)
                : visualRoot.GetComponentsInChildren<Renderer>(true);
            return;
        }

        playerRenderers = target.GetComponentsInChildren<Renderer>(true);
    }

    void ApplyPlayerVisibility()
    {
        if (!hidePlayerMeshInFirstPerson || playerRenderers == null)
        {
            return;
        }

        bool visible = mode != CameraMode.FirstPerson;
        for (int i = 0; i < playerRenderers.Length; i++)
        {
            if (playerRenderers[i] != null)
            {
                playerRenderers[i].enabled = visible;
            }
        }
    }

    void UpdateCursorLock(bool force = false)
    {
        if (GameplayCursorPolicy.IsAnyMenuOpen)
        {
            if (force || cursorLocked)
            {
                cursorLocked = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            return;
        }

        bool shouldLock = mode == CameraMode.FirstPerson && lockCursorInFirstPerson;
        if (force || shouldLock != cursorLocked)
        {
            cursorLocked = shouldLock;
            Cursor.lockState = shouldLock ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !shouldLock;
        }
    }

    public void ApplyGameplayCursorLock()
    {
        UpdateCursorLock(true);
    }

    void OnDisable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        ApplyPlayerVisibilityOnDisable();
    }

    void ApplyPlayerVisibilityOnDisable()
    {
        if (!hidePlayerMeshInFirstPerson || playerRenderers == null)
        {
            return;
        }

        for (int i = 0; i < playerRenderers.Length; i++)
        {
            if (playerRenderers[i] != null)
            {
                playerRenderers[i].enabled = true;
            }
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        SyncCompositionDebug();
    }
#endif
}
