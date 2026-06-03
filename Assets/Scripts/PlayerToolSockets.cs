using UnityEngine;

[DisallowMultipleComponent]
public class PlayerToolSockets : MonoBehaviour
{
    [Header("First Person Tool Socket")]
    [SerializeField] Transform firstPersonToolSocket;
    [SerializeField] Vector3 firstPersonFollowLocalPosition = new Vector3(0.32f, -0.22f, 0.48f);
    [SerializeField] Vector3 firstPersonFollowLocalEuler = new Vector3(10f, -12f, 0f);
    [SerializeField] Vector3 firstPersonToolPositionOffset = Vector3.zero;
    [SerializeField] Vector3 firstPersonToolRotationOffset = Vector3.zero;
    [SerializeField] Vector3 firstPersonToolScale = Vector3.one;

    [Header("Third Person Tool Socket")]
    [SerializeField] Transform thirdPersonToolSocket;
    [SerializeField] Vector3 thirdPersonToolPositionOffset = new Vector3(0.45f, 0.5f, 0.35f);
    [SerializeField] Vector3 thirdPersonToolRotationOffset = new Vector3(10f, -25f, 8f);
    [SerializeField] Vector3 thirdPersonToolScale = Vector3.one;

    [Header("Debug")]
    [SerializeField] bool showThirdPersonToolDebug = true;

    PlayerCameraController cameraController;
    Camera viewCamera;
    Transform playerRoot;
    GameObject thirdPersonDebugMarker;
    string lastDebugLog;

    public Transform FirstPersonToolSocket => firstPersonToolSocket;
    public Transform ThirdPersonToolSocket => thirdPersonToolSocket;

    void Awake()
    {
        playerRoot = transform;
        cameraController = FindFirstObjectByType<PlayerCameraController>();
        viewCamera = CrosshairRayUtility.ResolveGameplayCamera();
        EnsureSockets();
    }

    void LateUpdate()
    {
        UpdateFirstPersonSocketFollow();
        ApplyThirdPersonSocketConfiguration();
        UpdateThirdPersonDebug();
    }

    void UpdateFirstPersonSocketFollow()
    {
        if (!IsFirstPersonActive() || firstPersonToolSocket == null || viewCamera == null)
        {
            return;
        }

        firstPersonToolSocket.position = viewCamera.transform.TransformPoint(firstPersonFollowLocalPosition);
        firstPersonToolSocket.rotation = viewCamera.transform.rotation * Quaternion.Euler(firstPersonFollowLocalEuler);
    }

    void ApplyThirdPersonSocketConfiguration()
    {
        if (thirdPersonToolSocket == null)
        {
            return;
        }

        thirdPersonToolSocket.localPosition = thirdPersonToolPositionOffset;
        thirdPersonToolSocket.localEulerAngles = thirdPersonToolRotationOffset;
        thirdPersonToolSocket.localScale = thirdPersonToolScale;
    }

    void UpdateThirdPersonDebug()
    {
        if (!showThirdPersonToolDebug)
        {
            if (thirdPersonDebugMarker != null)
            {
                thirdPersonDebugMarker.SetActive(false);
            }

            return;
        }

        EnsureThirdPersonDebugMarker();
        if (thirdPersonDebugMarker != null && thirdPersonToolSocket != null)
        {
            thirdPersonDebugMarker.SetActive(!IsFirstPersonActive());
            thirdPersonDebugMarker.transform.SetPositionAndRotation(
                thirdPersonToolSocket.position,
                thirdPersonToolSocket.rotation);
        }

        Transform equippedTool = thirdPersonToolSocket != null && thirdPersonToolSocket.childCount > 0
            ? thirdPersonToolSocket.GetChild(0)
            : null;

        string debugLog =
            $"[ToolSocketDebug] mode={(IsFirstPersonActive() ? "FP" : "TP")} " +
            $"socketParent={(thirdPersonToolSocket != null ? thirdPersonToolSocket.parent.name : "none")} " +
            $"socketPos={FormatVector(thirdPersonToolSocket != null ? thirdPersonToolSocket.position : Vector3.zero)} " +
            $"tool={(equippedTool != null ? equippedTool.name : "none")} " +
            $"toolActive={(equippedTool != null && equippedTool.gameObject.activeSelf)} " +
            $"toolScale={(equippedTool != null ? FormatVector(equippedTool.localScale) : "n/a")} " +
            $"renderers={(equippedTool != null ? CountEnabledRenderers(equippedTool) : 0)}";

        if (debugLog != lastDebugLog)
        {
            lastDebugLog = debugLog;
            HandheldToolDebug.Log(debugLog, this);
        }
    }

    public Transform GetActiveSocket(bool firstPerson)
    {
        EnsureSockets();
        return firstPerson ? firstPersonToolSocket : thirdPersonToolSocket;
    }

    public bool IsFirstPersonActive()
    {
        return cameraController != null && cameraController.IsFirstPerson;
    }

    public void EnsureSockets()
    {
        viewCamera = CrosshairRayUtility.ResolveGameplayCamera();
        if (cameraController == null)
        {
            cameraController = FindFirstObjectByType<PlayerCameraController>();
        }

        PlayerVisualBuilder.BuildResult visual = PlayerVisualBuilder.EnsurePlayerVisual(gameObject);
        if (visual.ThirdPersonToolSocket != null)
        {
            thirdPersonToolSocket = visual.ThirdPersonToolSocket;
        }

        if (visual.FirstPersonToolSocket != null)
        {
            firstPersonToolSocket = visual.FirstPersonToolSocket;
        }

        if (thirdPersonToolSocket == null)
        {
            Transform visualRoot = transform.Find(PlayerVisualBuilder.VisualRootName);
            thirdPersonToolSocket = CreateFallbackSocket(
                visualRoot != null ? visualRoot : playerRoot,
                PlayerVisualBuilder.ThirdPersonToolSocketName,
                thirdPersonToolPositionOffset);
        }

        if (firstPersonToolSocket == null)
        {
            firstPersonToolSocket = CreateFallbackSocket(
                playerRoot,
                PlayerVisualBuilder.FirstPersonToolSocketName,
                new Vector3(0.28f, 1.05f, 0.42f));
        }

        if (thirdPersonToolSocket != null)
        {
            thirdPersonToolSocket.gameObject.SetActive(true);
        }

        ApplyThirdPersonSocketConfiguration();
    }

    void EnsureThirdPersonDebugMarker()
    {
        if (thirdPersonDebugMarker != null)
        {
            return;
        }

        thirdPersonDebugMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        thirdPersonDebugMarker.name = "ThirdPersonToolSocketDebugMarker";
        thirdPersonDebugMarker.transform.SetParent(transform, true);
        thirdPersonDebugMarker.transform.localScale = Vector3.one * 0.18f;

        Collider collider = thirdPersonDebugMarker.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        Renderer renderer = thirdPersonDebugMarker.GetComponent<Renderer>();
        if (renderer != null)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(1f, 0.85f, 0.1f, 0.85f);
            renderer.sharedMaterial = material;
        }

        thirdPersonDebugMarker.SetActive(false);
    }

    static Transform CreateFallbackSocket(Transform parent, string socketName, Vector3 localPosition)
    {
        Transform existing = parent.Find(socketName);
        if (existing != null)
        {
            return existing;
        }

        var socketGo = new GameObject(socketName);
        var socket = socketGo.transform;
        socket.SetParent(parent, false);
        socket.localPosition = localPosition;
        socket.localRotation = Quaternion.identity;
        return socket;
    }

    static string FormatVector(Vector3 value)
    {
        return $"({value.x:F2}, {value.y:F2}, {value.z:F2})";
    }

    static int CountEnabledRenderers(Transform toolRoot)
    {
        if (toolRoot == null)
        {
            return 0;
        }

        int count = 0;
        Renderer[] renderers = toolRoot.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].enabled)
            {
                count++;
            }
        }

        return count;
    }
}
