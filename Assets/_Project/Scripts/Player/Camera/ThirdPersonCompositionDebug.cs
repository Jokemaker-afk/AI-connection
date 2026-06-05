using UnityEngine;

/// <summary>
/// Logs screen-space relationship between crosshair and player for third-person tuning.
/// </summary>
[DisallowMultipleComponent]
public class ThirdPersonCompositionDebug : MonoBehaviour
{
    [SerializeField] bool showThirdPersonCompositionDebug = true;
    [SerializeField] float logIntervalSeconds = 0.75f;

    PlayerCameraController cameraController;
    Transform playerRoot;
    Transform chestAnchor;
    float lastLogTime;

    public bool ShowThirdPersonCompositionDebug
    {
        get => showThirdPersonCompositionDebug;
        set => showThirdPersonCompositionDebug = value;
    }

    void Awake()
    {
        cameraController = GetComponent<PlayerCameraController>();
        if (cameraController == null)
        {
            cameraController = FindFirstObjectByType<PlayerCameraController>();
        }

        var player = GameObject.Find("Player");
        if (player != null)
        {
            playerRoot = player.transform;
        }
    }

    void LateUpdate()
    {
        if (!showThirdPersonCompositionDebug || cameraController == null || cameraController.IsFirstPerson)
        {
            return;
        }

        if (Time.time - lastLogTime < logIntervalSeconds)
        {
            return;
        }

        lastLogTime = Time.time;
        LogComposition();
    }

    void LogComposition()
    {
        Camera camera = cameraController.GetComponent<Camera>();
        if (camera == null || playerRoot == null)
        {
            return;
        }

        Vector2 crosshairScreen = cameraController.GetCrosshairScreenPoint();
        Vector3 chestWorld = GetChestWorldPoint();
        Vector3 headWorld = chestWorld + Vector3.up * 0.35f;
        Vector2 chestScreen = camera.WorldToScreenPoint(chestWorld);
        Vector2 headScreen = camera.WorldToScreenPoint(headWorld);

        float chestPixelDistance = Vector2.Distance(crosshairScreen, chestScreen);
        float headPixelDistance = Vector2.Distance(crosshairScreen, headScreen);
        bool overlap = chestPixelDistance < 80f || headPixelDistance < 80f;

        Ray crosshairRay = cameraController.GetCrosshairRay(camera);
        bool rayHitsPlayer = Physics.Raycast(
            crosshairRay,
            out RaycastHit hit,
            12f,
            GameplayLayers.PlayerMask,
            QueryTriggerInteraction.Ignore)
            && hit.collider != null
            && hit.collider.transform.IsChildOf(playerRoot);

        Debug.Log($"[ThirdPersonComp] Crosshair screen position: {crosshairScreen.x:F0}, {crosshairScreen.y:F0}");
        Debug.Log($"[ThirdPersonComp] Player chest screen position: {chestScreen.x:F0}, {chestScreen.y:F0}");
        Debug.Log($"[ThirdPersonComp] Player head screen position: {headScreen.x:F0}, {headScreen.y:F0}");
        Debug.Log($"[ThirdPersonComp] Chest/crosshair pixel distance: {chestPixelDistance:F0}");
        Debug.Log($"[ThirdPersonComp] Player/crosshair overlap: {overlap}");
        Debug.Log($"[ThirdPersonComp] Camera ray hits player first: {rayHitsPlayer}");
        Debug.Log($"[ThirdPersonComp] Camera position: {camera.transform.position}");
        Debug.Log($"[ThirdPersonComp] Camera forward: {camera.transform.forward}");
        Debug.Log($"[ThirdPersonComp] Aim yaw forward: {cameraController.GetImmediateAimForward()}");
    }

    Vector3 GetChestWorldPoint()
    {
        if (chestAnchor == null)
        {
            Transform visualRoot = playerRoot.Find(PlayerVisualBuilder.VisualRootName);
            chestAnchor = visualRoot != null ? visualRoot : playerRoot;
        }

        return playerRoot.position + Vector3.up * cameraController.ThirdPersonLookAtHeight;
    }

    void OnDrawGizmos()
    {
        if (!showThirdPersonCompositionDebug || !Application.isPlaying || cameraController == null || cameraController.IsFirstPerson)
        {
            return;
        }

        Camera camera = cameraController.GetComponent<Camera>();
        if (camera == null)
        {
            return;
        }

        Ray ray = cameraController.GetCrosshairRay(camera);
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(ray.origin, ray.direction * 10f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(GetChestWorldPoint(), 0.08f);
    }
}
