using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraController : MonoBehaviour
{
    public enum CameraMode
    {
        ThirdPerson,
        FirstPerson
    }

    [SerializeField] Transform target;
    [SerializeField] Key toggleKey = Key.V;

    [Header("Third Person")]
    [SerializeField] float thirdPersonDistance = 7f;
    [SerializeField] float thirdPersonHeight = 2f;
    [SerializeField] float thirdPersonSmooth = 12f;
    [SerializeField] bool alwaysFollowTarget = false;
    [SerializeField] Vector3 lookAtOffset = new Vector3(0f, 1.2f, 0f);

    [Header("First Person")]
    [SerializeField] Vector3 firstPersonEyeOffset = new Vector3(0f, 0.6f, 0f);
    [SerializeField] bool hidePlayerMeshInFirstPerson = true;

    [Header("Mouse Look")]
    [SerializeField] float mouseSensitivity = 0.15f;
    [SerializeField] float minPitch = -80f;
    [SerializeField] float maxPitch = 80f;
    [SerializeField] float thirdPersonMinPitch = -15f;
    [SerializeField] float thirdPersonMaxPitch = 65f;
    [SerializeField] bool lockCursorInFirstPerson = true;

    float yaw;
    float pitch = 15f;
    CameraMode mode = CameraMode.ThirdPerson;
    Renderer[] playerRenderers;
    bool cursorLocked;

    public CameraMode Mode => mode;
    public bool IsFirstPerson => mode == CameraMode.FirstPerson;
    public float Yaw => yaw;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        CachePlayerRenderers();
        if (target != null)
        {
            yaw = target.eulerAngles.y;
        }
    }

    public void ConfigureAlwaysFollow(bool enabled, float distance = 6.5f, float height = 2.4f)
    {
        alwaysFollowTarget = enabled;
        thirdPersonDistance = distance;
        thirdPersonHeight = height;
        mode = CameraMode.ThirdPerson;
        ApplyPlayerVisibility();
    }

    void Awake()
    {
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
    }

    void ToggleMode()
    {
        mode = mode == CameraMode.ThirdPerson ? CameraMode.FirstPerson : CameraMode.ThirdPerson;
        ApplyPlayerVisibility();
        UpdateCursorLock(true);
    }

    void HandleMouseLook()
    {
        if (GameplayCursorPolicy.IsInventoryUiOpen)
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
        Vector3 focusPoint = target.position + lookAtOffset;
        Quaternion orbitRotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPosition = focusPoint + orbitRotation * new Vector3(0f, thirdPersonHeight, -thirdPersonDistance);

        if (alwaysFollowTarget)
        {
            transform.position = desiredPosition;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, desiredPosition, thirdPersonSmooth * Time.deltaTime);
        }

        transform.LookAt(focusPoint);
    }

    void CachePlayerRenderers()
    {
        if (target == null)
        {
            playerRenderers = null;
            return;
        }

        playerRenderers = target.GetComponentsInChildren<Renderer>();
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
        if (!lockCursorInFirstPerson)
        {
            return;
        }

        if (GameplayCursorPolicy.IsInventoryUiOpen)
        {
            if (force || cursorLocked)
            {
                cursorLocked = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            return;
        }

        bool shouldLock = mode == CameraMode.FirstPerson;
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
        if (GameplayCursorPolicy.IsInventoryUiOpen)
        {
            GameplayCursorPolicy.SetInventoryUiOpen(false);
        }

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
