using UnityEngine;
using UnityEngine.InputSystem;

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

    [Header("Third Person (Fortnite-style OTS)")]
    [SerializeField] float thirdPersonDistance = 5.2f;
    [SerializeField] float thirdPersonHeight = 2.0f;
    [SerializeField] bool smoothThirdPersonCameraPosition = false;
    [SerializeField] float cameraSmoothTime = 0.04f;
    [SerializeField] bool alwaysFollowTarget = false;
    [SerializeField] bool useRightShoulder = true;
    [SerializeField] float shoulderOffsetX = 0.85f;
    [SerializeField] float shoulderOffsetY = 0.3f;
    [SerializeField] Vector3 thirdPersonCameraOffset = new Vector3(0f, 0.15f, 0f);
    [SerializeField] float thirdPersonLookAtHeight = 1.55f;
    [SerializeField] float thirdPersonPivotForwardOffset = 0.35f;
    [SerializeField] Vector2 crosshairScreenOffset = new Vector2(0f, 18f);
    [SerializeField] float thirdPersonFieldOfView = 70f;

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

    float yaw;
    float pitch = 8f;
    float thirdPersonDistanceCurrent;
    CameraMode mode = CameraMode.ThirdPerson;
    Camera viewCamera;
    Renderer[] playerRenderers;
    CharacterController targetController;
    bool cursorLocked;
    Vector3 thirdPersonVelocity;

    public CameraMode Mode => mode;
    public bool IsFirstPerson => mode == CameraMode.FirstPerson;
    public float Yaw => yaw;
    public float Pitch => pitch;
    public Vector3 AimForward => transform.forward;
    public Vector2 CrosshairScreenOffset => crosshairScreenOffset;
    public Transform Target => target;

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

    /// <summary>Immediate horizontal aim forward from mouse yaw (same-frame, no camera smoothing).</summary>
    public Vector3 GetImmediateAimForward()
    {
        return Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
    }

    /// <summary>Immediate horizontal aim right from mouse yaw (same-frame, no camera smoothing).</summary>
    public Vector3 GetImmediateAimRight()
    {
        return Quaternion.Euler(0f, yaw, 0f) * Vector3.right;
    }

    public Vector3 GetPlanarForward() => GetImmediateAimForward();

    public Vector3 GetPlanarRight() => GetImmediateAimRight();

    public void ConfigureAlwaysFollow(bool enabled, float distance = 5.2f, float height = 2f)
    {
        alwaysFollowTarget = enabled;
        thirdPersonDistance = distance;
        thirdPersonHeight = height;
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
            return new Ray(transform.position, AimForward);
        }

        return camera.ScreenPointToRay(GetCrosshairScreenPoint());
    }

    void Awake()
    {
        thirdPersonDistanceCurrent = thirdPersonDistance;
        viewCamera = GetComponent<Camera>();
        if (viewCamera == null)
        {
            viewCamera = Camera.main;
        }

        EnsureTarget();
    }

    void Start()
    {
        EnsureTarget();
        CachePlayerRenderers();
        if (target != null)
        {
            yaw = target.eulerAngles.y;
        }

        ApplyFieldOfView();
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
        thirdPersonDistanceCurrent = thirdPersonDistance;
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
        Vector3 pivot = target.position
            + Vector3.up * thirdPersonLookAtHeight
            + orbitRotation * Vector3.forward * thirdPersonPivotForwardOffset;

        float shoulderX = useRightShoulder ? shoulderOffsetX : -shoulderOffsetX;
        Vector3 localCameraOffset = new Vector3(
            shoulderX + thirdPersonCameraOffset.x,
            thirdPersonHeight + shoulderOffsetY + thirdPersonCameraOffset.y,
            -thirdPersonDistance + thirdPersonCameraOffset.z);

        Vector3 desiredPosition = pivot + orbitRotation * localCameraOffset;
        desiredPosition = ResolveCameraCollision(pivot, desiredPosition);

        bool usePositionSmoothing = smoothThirdPersonCameraPosition
            && !alwaysFollowTarget
            && cameraSmoothTime > 0.001f;

        if (usePositionSmoothing)
        {
            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref thirdPersonVelocity,
                cameraSmoothTime);
        }
        else
        {
            transform.position = desiredPosition;
            thirdPersonVelocity = Vector3.zero;
        }

        transform.rotation = orbitRotation;
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
}
