using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Post-transition diagnostics for player, input, camera, UI singleton counts.</summary>
public static class SceneTransitionHealthCheck
{
    public struct Report
    {
        public string SceneName;
        public int GameplayCoreCount;
        public int PlayerRootCount;
        public int GameplayHudCount;
        public int InventoryHudCount;
        public int EventSystemCount;
        public int CanvasCount;
        public int MainCameraCount;
        public int AudioListenerCount;
        public int SceneBootstrapperCount;
        public int ObjectivePanelCount;
        public int ObjectiveTextComponentCount;
        public bool DuplicateUiDetected;
        public bool DuplicateObjectivePanels;
        public string ActiveObjectiveText;
        public string BoundObjectiveProvider;
        public bool OldLevel5ProviderAlive;
        public bool Level6ProviderAlive;
        public bool GameplayCoreFound;
        public bool PlayerRootFound;
        public bool PlayerControllerEnabled;
        public bool CharacterControllerEnabled;
        public bool GameplayInputLocked;
        public bool InventoryUiOpen;
        public bool CraftingUiOpen;
        public bool CursorLocked;
        public bool CursorVisible;
        public bool MainCameraBound;
        public bool AimReferenceBound;
        public bool CameraTargetBound;
        public float TimeScale;
        public string MainCameraName;
        public string CameraTargetName;
    }

    public static Report Evaluate(string sceneName)
    {
        GameObject player = PersistentPlayerRig.Player;
        if (player == null)
        {
            player = GameObject.Find("Player");
        }

        var playerController = player != null ? player.GetComponent<PlayerController>() : null;
        var characterController = player != null ? player.GetComponent<CharacterController>() : null;
        var aimReference = player != null ? player.GetComponent<AimReferenceProvider>() : null;

        Camera mainCamera = Camera.main;
        PlayerCameraController cameraController = mainCamera != null
            ? mainCamera.GetComponent<PlayerCameraController>()
            : null;

        int gameplayHudCount = RuntimeSingletonCleanup.CountGameplayHudRoots();
        int inventoryHudCount = RuntimeSingletonCleanup.CountInventoryHudRoots();
        int eventSystemCount = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length;
        int canvasCount = RuntimeSingletonCleanup.CountGameplayOverlayCanvases();
        int objectivePanelCount = LevelObjectiveUiRegistry.CountVisibleObjectivePanels();
        string activeObjectiveText = LevelObjectiveUiRegistry.GetActiveObjectiveText();

        var report = new Report
        {
            SceneName = sceneName,
            GameplayCoreCount = Object.FindObjectsByType<GameplayCore>(FindObjectsSortMode.None).Length,
            PlayerRootCount = CountPlayerRoots(),
            GameplayHudCount = gameplayHudCount,
            InventoryHudCount = inventoryHudCount,
            EventSystemCount = eventSystemCount,
            CanvasCount = canvasCount,
            MainCameraCount = CountMainCameras(),
            AudioListenerCount = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length,
            SceneBootstrapperCount = Object.FindObjectsByType<SceneGameplayBootstrap>(FindObjectsSortMode.None).Length,
            ObjectivePanelCount = objectivePanelCount,
            ObjectiveTextComponentCount = CountObjectiveTextComponents(),
            DuplicateObjectivePanels = objectivePanelCount > 1,
            ActiveObjectiveText = activeObjectiveText,
            BoundObjectiveProvider = LevelObjectiveUiRegistry.BoundProviderName,
            OldLevel5ProviderAlive = LevelObjectiveUiRegistry.IsPreviousLevelProviderAlive(5),
            Level6ProviderAlive = Object.FindFirstObjectByType<Level6ProgressionManager>() != null,
            DuplicateUiDetected = gameplayHudCount > 1
                || inventoryHudCount > 1
                || eventSystemCount > 1
                || canvasCount > 1
                || objectivePanelCount > 1,
            GameplayCoreFound = GameplayCore.Exists,
            PlayerRootFound = player != null,
            PlayerControllerEnabled = playerController != null && playerController.enabled,
            CharacterControllerEnabled = characterController != null && characterController.enabled,
            GameplayInputLocked = GameplayCursorPolicy.IsAnyMenuOpen,
            InventoryUiOpen = GameplayCursorPolicy.IsInventoryUiOpen,
            CraftingUiOpen = GameplayCursorPolicy.IsCraftingUiOpen,
            CursorLocked = Cursor.lockState == CursorLockMode.Locked,
            CursorVisible = Cursor.visible,
            MainCameraBound = mainCamera != null,
            AimReferenceBound = aimReference != null,
            CameraTargetBound = cameraController != null && cameraController.Target != null,
            TimeScale = Time.timeScale,
            MainCameraName = mainCamera != null ? mainCamera.name : "(none)",
            CameraTargetName = cameraController != null && cameraController.Target != null
                ? cameraController.Target.name
                : "(none)",
        };

        return report;
    }

    public static void LogReport(string sceneName)
    {
        LogReportInternal(sceneName, runCleanupIfNeeded: true);
    }

    static void LogReportInternal(string sceneName, bool runCleanupIfNeeded)
    {
        Report report = Evaluate(sceneName);
        LogReportBody(report);

        if (runCleanupIfNeeded && report.DuplicateUiDetected)
        {
            Debug.LogWarning(
                $"[SceneTransitionHealthCheck] WARNING: Duplicate runtime UI detected in {sceneName}. Cleaning duplicates.");
            RuntimeSingletonCleanup.CleanupDuplicateRuntimeObjects(sceneName);
            GameplayHudBootstrap.EnsureGameplayHud();
            GameplayCrosshairController.EnsureCrosshairForPlayer();
            GameplayHudBootstrap.BindAll();
            LevelObjectiveUiRegistry.ApplyForScene(sceneName);

            Report afterCleanup = Evaluate(sceneName);
            Debug.Log(
                "[SceneTransitionHealthCheck] Post-cleanup counts:\n" +
                $"  GameplayHUD count: {afterCleanup.GameplayHudCount}\n" +
                $"  Inventory UI count: {afterCleanup.InventoryHudCount}\n" +
                $"  EventSystem count: {afterCleanup.EventSystemCount}\n" +
                $"  Canvas count: {afterCleanup.CanvasCount}\n" +
                $"  Duplicate UI detected: {afterCleanup.DuplicateUiDetected}");
        }

        if (!report.PlayerControllerEnabled || report.GameplayInputLocked || report.TimeScale <= 0f
            || report.DuplicateObjectivePanels)
        {
            Debug.LogWarning(
                "[SceneTransitionHealthCheck] Gameplay may be blocked after scene transition. " +
                "Check PlayerController, UI lock state, objective panel duplication, or Time.timeScale.");
        }
    }

    static int CountObjectiveTextComponents()
    {
        GameObject hud = RuntimeSingletonCleanup.ResolveSingleGameplayHud();
        if (hud == null)
        {
            return 0;
        }

        Text[] texts = hud.GetComponentsInChildren<Text>(true);
        int count = 0;
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] != null && texts[i].name == "ObjectiveText")
            {
                count++;
            }
        }

        return count;
    }

    static void LogReportBody(Report report)
    {
        Debug.Log(
            "[SceneTransitionHealthCheck]\n" +
            $"  Entered scene: {report.SceneName}\n" +
            $"  After scene load: GameplayCore count = {report.GameplayCoreCount}\n" +
            $"  After scene load: PlayerRoot count = {report.PlayerRootCount}\n" +
            $"  After scene load: GameplayHUD count = {report.GameplayHudCount}\n" +
            $"  After scene load: Inventory UI count = {report.InventoryHudCount}\n" +
            $"  After scene load: EventSystem count = {report.EventSystemCount}\n" +
            $"  After scene load: Canvas count = {report.CanvasCount}\n" +
            $"  After scene load: MainCamera count = {report.MainCameraCount}\n" +
            $"  After scene load: AudioListener count = {report.AudioListenerCount}\n" +
            $"  After scene load: SceneBootstrapper count = {report.SceneBootstrapperCount}\n" +
            $"  After scene load: Objective panel count = {report.ObjectivePanelCount}\n" +
            $"  After scene load: Objective text component count = {report.ObjectiveTextComponentCount}\n" +
            $"  Duplicate UI detected: {report.DuplicateUiDetected}\n" +
            $"  Duplicate objective panels: {report.DuplicateObjectivePanels}\n" +
            $"  Bound objective provider: {(string.IsNullOrEmpty(report.BoundObjectiveProvider) ? "(none)" : report.BoundObjectiveProvider)}\n" +
            $"  Active objective text: {(string.IsNullOrEmpty(report.ActiveObjectiveText) ? "(empty)" : report.ActiveObjectiveText)}\n" +
            $"  Old Level5 provider alive: {report.OldLevel5ProviderAlive}\n" +
            $"  Level6 provider alive: {report.Level6ProviderAlive}\n" +
            $"  PlayerController enabled: {report.PlayerControllerEnabled}\n" +
            $"  CharacterController enabled: {report.CharacterControllerEnabled}\n" +
            $"  Gameplay input locked: {report.GameplayInputLocked}\n" +
            $"  Inventory UI open: {report.InventoryUiOpen}\n" +
            $"  Crafting UI open: {report.CraftingUiOpen}\n" +
            $"  Cursor locked: {report.CursorLocked}\n" +
            $"  Cursor visible: {report.CursorVisible}\n" +
            $"  Time.timeScale: {report.TimeScale}\n" +
            $"  Main camera after scene load: {report.MainCameraName}\n" +
            $"  Camera target player: {report.CameraTargetName}\n" +
            $"  Camera bound: {report.MainCameraBound}\n" +
            $"  AimReference bound: {report.AimReferenceBound}\n" +
            $"  Camera target bound: {report.CameraTargetBound}");
    }

    static int CountPlayerRoots()
    {
        int count = 0;
        var controllers = Object.FindObjectsByType<PlayerController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < controllers.Length; i++)
        {
            if (controllers[i] != null && controllers[i].gameObject.name == "Player")
            {
                count++;
            }
        }

        return count;
    }

    static int CountMainCameras()
    {
        int count = 0;
        var cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] != null && cameras[i].gameObject.CompareTag("MainCamera"))
            {
                count++;
            }
        }

        return count;
    }
}

/// <summary>Alias entry point for runtime gameplay validation after scene loads.</summary>
public static class GameplayRuntimeHealthCheck
{
    public static void LogAfterSceneLoad(string sceneName)
    {
        SceneTransitionHealthCheck.LogReport(sceneName);
    }
}

/// <summary>Validates bootstrap singleton counts and triggers cleanup when needed.</summary>
public static class GameplayBootstrapValidator
{
    public static bool ValidateAndCleanup(string sceneName)
    {
        bool duplicates = RuntimeSingletonCleanup.CleanupDuplicateRuntimeObjects(sceneName);
        SceneTransitionHealthCheck.LogReport(sceneName);
        return duplicates;
    }
}
