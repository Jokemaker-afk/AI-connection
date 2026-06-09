using UnityEngine;
using UnityEngine.UI;

public static class CrosshairHealthCheck
{
    public struct Report
    {
        public bool PlayerRootExists;
        public bool GameplayHudExists;
        public int CrosshairCount;
        public int DuplicatesRemoved;
        public bool FoundOfficialCrosshair;
        public bool ActiveSelf;
        public bool ActiveInHierarchy;
        public bool Visible;
        public bool Centered;
        public bool HasRenderableSprite;
        public bool AimReferenceBound;
        public int CanvasSortingOrder;
        public string CanvasRenderMode;
        public string CameraMode;
        public bool UiBlocking;
    }

    public static Report Evaluate()
    {
        GameObject player = PersistentPlayerRig.Player ?? GameObject.Find("Player");
        GameObject hudGo = RuntimeSingletonCleanup.ResolveSingleGameplayHud();
        GameplayCrosshairHud crosshairHud = hudGo != null
            ? hudGo.GetComponent<GameplayCrosshairHud>()
            : Object.FindFirstObjectByType<GameplayCrosshairHud>();

        RectTransform crosshairRect = crosshairHud != null ? crosshairHud.CrosshairRectTransform : null;
        int crosshairCount = CountCrosshairRoots(hudGo);

        Canvas canvas = hudGo != null ? hudGo.GetComponent<Canvas>() : null;
        if (canvas == null && crosshairRect != null)
        {
            canvas = crosshairRect.GetComponentInParent<Canvas>();
        }

        AimReferenceProvider aimReference = player != null ? player.GetComponent<AimReferenceProvider>() : null;
        PlayerCameraController cameraController = Object.FindFirstObjectByType<PlayerCameraController>();

        bool centered = crosshairRect != null
            && crosshairRect.anchorMin == new Vector2(0.5f, 0.5f)
            && crosshairRect.anchorMax == new Vector2(0.5f, 0.5f)
            && crosshairRect.pivot == new Vector2(0.5f, 0.5f);

        bool aimBound = aimReference != null
            && crosshairRect != null
            && aimReference.BoundCrosshairRect == crosshairRect;

        return new Report
        {
            PlayerRootExists = player != null,
            GameplayHudExists = hudGo != null,
            CrosshairCount = crosshairCount,
            DuplicatesRemoved = 0,
            FoundOfficialCrosshair = crosshairRect != null,
            ActiveSelf = crosshairRect != null && crosshairRect.gameObject.activeSelf,
            ActiveInHierarchy = crosshairRect != null && crosshairRect.gameObject.activeInHierarchy,
            Visible = crosshairHud != null && crosshairHud.IsCrosshairVisible,
            Centered = centered,
            HasRenderableSprite = HasAnySprite(crosshairRect),
            AimReferenceBound = aimBound,
            CanvasSortingOrder = canvas != null ? canvas.sortingOrder : 0,
            CanvasRenderMode = canvas != null ? canvas.renderMode.ToString() : "(none)",
            CameraMode = cameraController != null ? cameraController.Mode.ToString() : "(none)",
            UiBlocking = GameplayCursorPolicy.IsAnyMenuOpen,
        };
    }

    public static void LogReport(string sceneName)
    {
        GameplayCrosshairController.EnsureCrosshairForPlayer();
        GameplayCrosshairHud crosshairHud = GameplayCrosshairController.ResolveHud();
        int duplicatesRemoved = crosshairHud != null ? crosshairHud.ConsumeDuplicatesRemovedCount() : 0;
        Report report = Evaluate();

        string modeSuffix = report.PlayerRootExists
            ? $"Player exists. Ensured official crosshair. Visible={report.Visible}. Mode={report.CameraMode}."
            : "Player missing — crosshair deferred.";

        Debug.Log(
            "[Crosshair] " + modeSuffix + "\n" +
            $"  Scene: {sceneName}\n" +
            $"  PlayerRoot exists: {report.PlayerRootExists}\n" +
            $"  GameplayHUD exists: {report.GameplayHudExists}\n" +
            $"  Found official crosshair: {report.FoundOfficialCrosshair}\n" +
            $"  Crosshair count: {report.CrosshairCount}\n" +
            $"  Duplicate count removed: {duplicatesRemoved}\n" +
            $"  Active: {report.ActiveSelf}\n" +
            $"  Active in hierarchy: {report.ActiveInHierarchy}\n" +
            $"  Visible: {report.Visible}\n" +
            $"  Centered: {report.Centered}\n" +
            $"  Renderable sprite assigned: {report.HasRenderableSprite}\n" +
            $"  AimReference bound: {report.AimReferenceBound}\n" +
            $"  UI blocking state: {report.UiBlocking}\n" +
            $"  Canvas sorting order: {report.CanvasSortingOrder}\n" +
            $"  Canvas render mode: {report.CanvasRenderMode}");
    }

    static bool HasAnySprite(RectTransform crosshairRect)
    {
        if (crosshairRect == null)
        {
            return false;
        }

        Image[] images = crosshairRect.GetComponentsInChildren<Image>(true);
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] != null && images[i].sprite != null)
            {
                return true;
            }
        }

        return false;
    }

    static int CountCrosshairRoots(GameObject hudGo)
    {
        if (hudGo == null)
        {
            return 0;
        }

        int count = 0;
        Transform root = hudGo.transform;
        for (int i = 0; i < root.childCount; i++)
        {
            if (root.GetChild(i).name == GameplayCrosshairHud.CrosshairRootName)
            {
                count++;
            }
        }

        return count;
    }
}
