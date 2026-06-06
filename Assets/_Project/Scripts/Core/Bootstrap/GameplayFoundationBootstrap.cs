using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameplayFoundationBootstrap
{
    static string lastBootstrappedScene = string.Empty;

    public static void EnsureForActiveScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (!GameplaySceneCatalog.IsSupportedGameplayScene(sceneName))
        {
            return;
        }

        if (lastBootstrappedScene == sceneName)
        {
            return;
        }

        BootstrapScene(sceneName);
    }

    public static void PrepareSceneTransition(string targetSceneName)
    {
        string currentScene = SceneManager.GetActiveScene().name;
        PlayerProgressionState.LogInventoryTransitionSnapshot(
            PersistentPlayerRig.Player,
            currentScene,
            isLeavingScene: true);

        CapturePersistentState();
        GameplayCore.Instance?.Log($"Preparing transition: {currentScene} -> {targetSceneName}");
    }

    public static void FinalizeSceneTransition(string sceneName)
    {
        if (!GameplaySceneCatalog.IsSupportedGameplayScene(sceneName))
        {
            return;
        }

        if (lastBootstrappedScene == sceneName)
        {
            LevelTransitionOverlay.Hide();
            PlayerProgressionState.LogInventoryTransitionSnapshot(PersistentPlayerRig.Player, sceneName, isLeavingScene: false);
            PersistentPlayerRig.EnsureOnGameplayCore(GameplayCore.EnsureExists())?.SchedulePostLoadRebind();
            GameplayCore.Instance?.Log($"Scene {sceneName} post-load rebind scheduled.");
            return;
        }

        BootstrapScene(sceneName);
    }

    static void BootstrapScene(string sceneName)
    {
        lastBootstrappedScene = sceneName;

        GameplayCore core = GameplayCore.EnsureExists();
        core.ProgressionState.EnsureCharacterProgressionInitialized(sceneName);
        core.Log($"Scene loaded: {sceneName}");

        if (GameplaySceneCatalog.GetLevelNumber(sceneName) == 6 && !core.ProgressionState.HasPersistentState)
        {
            core.Log("Scene6 direct play detected — bootstrapping persistent gameplay foundation.");
        }

        PersistentPlayerRig rig = PersistentPlayerRig.EnsureOnGameplayCore(core);

        EnsureSceneHierarchy(sceneName);
        Vector3 spawn = ResolveSpawnPosition(sceneName);

        rig.EnsurePlayer(spawn, out bool createdNewPlayer);
        rig.EnsureMainCamera();

        core.Abilities.EnsureInheritedAbilitiesForScene(sceneName);
        GameplayProgressionBootstrap.EnsureProgressionSystems();
        GameplayHudBootstrap.EnsureGameplayHud();

        ApplyPlayerProgression(core, sceneName, createdNewPlayer);

        core.Abilities.EnsureBaselineAbilitiesForScene(
            sceneName,
            core.ProgressionState != null ? core.ProgressionState.CurrentLevelIndex : 0);
        GameplayPlacementBootstrap.EnsureInitialized(sceneName);
        rig.RebindSceneSystems();

        EnsureLevelSpecificContent(sceneName);

        ModularLevelAssembler.EnsureAssembledForScene(core.ProgressionState, sceneName);

        LevelTransitionOverlay.Hide();
        rig.SchedulePostLoadRebind();

        PlayerProgressionState.LogInventoryTransitionSnapshot(
            PersistentPlayerRig.Player,
            sceneName,
            isLeavingScene: false);

        core.Log($"Scene {sceneName} inherited gameplay core successfully.");
    }

    static void ApplyPlayerProgression(GameplayCore core, string sceneName, bool createdNewPlayer)
    {
        GameObject player = PersistentPlayerRig.Player;
        if (player == null)
        {
            core.Log("WARNING: Persistent player missing after bootstrap.");
            return;
        }

        if (core.ProgressionState.HasPersistentState && !createdNewPlayer)
        {
            core.ProgressionState.ApplyNonInventoryState(player);
            core.ProgressionState.LogInventoryRestoreSummary(player, sceneName);
            core.Log($"Persistent player loaded into {sceneName} with live inventory.");
            return;
        }

        if (core.ProgressionState.HasPersistentState)
        {
            core.ProgressionState.ApplySavedState(player);
            core.ProgressionState.LogInventoryRestoreSummary(player, sceneName);
            core.Log($"Persistent player restored into {sceneName} from runtime state.");
            return;
        }

        if (core.ProgressionState.ShouldApplyDebugStarterInventory())
        {
            PlayerProgressionState.ApplyDefaultInheritedLoadout(player, sceneName);
            core.ProgressionState.LogInventoryRestoreSummary(player, sceneName);
            core.Log($"Applied debug starter inventory for direct {sceneName} play.");
        }
    }

    public static void CapturePersistentState()
    {
        lastBootstrappedScene = string.Empty;
        if (!GameplayCore.Exists)
        {
            return;
        }

        string sceneName = SceneManager.GetActiveScene().name;
        GameObject player = PersistentPlayerRig.Player ?? GameObject.Find("Player");
        GameplayCore.Instance.ProgressionState.CaptureFromPlayer(player, sceneName);
        GameplayCore.Instance.ProgressionState.UnlockModulesForLevel(
            GameplaySceneCatalog.GetLevelNumber(sceneName));
        GameplayCore.Instance.Log($"Captured player runtime state before leaving {sceneName}.");
    }

    static Vector3 ResolveSpawnPosition(string sceneName)
    {
        Vector3 catalogDefault = GameplaySceneCatalog.GetDefaultSpawn(sceneName);
        var spawnPoint = GameObject.Find("PlayerSpawnPoint");
        if (spawnPoint == null)
        {
            return catalogDefault;
        }

        if (spawnPoint.GetComponent<PlayerSpawnPoint>() != null)
        {
            return spawnPoint.transform.position;
        }

        if (spawnPoint.transform.position.sqrMagnitude < 0.001f && catalogDefault.sqrMagnitude > 0.001f)
        {
            spawnPoint.transform.position = catalogDefault;
            spawnPoint.AddComponent<PlayerSpawnPoint>();
            GameplayCore.Instance?.Log($"Positioned auto PlayerSpawnPoint for {sceneName} at {catalogDefault}.");
            return catalogDefault;
        }

        return spawnPoint.transform.position;
    }

    static void EnsureSceneHierarchy(string sceneName)
    {
        EnsureRoot("SceneRoot");
        EnsureComponent<SceneGameplayBootstrap>("GameplayBootstrapper");
        EnsurePlayerSpawnRoot(sceneName);
        EnsureRoot("Environment");
        EnsureRoot("Pickups");
        EnsureRoot("Interactables");
        EnsureRoot("PlacedObjects");
        EnsureRoot("LevelObjectives");
        EnsureRoot("ExitPortal");
        EnsureDirectionalLight();
    }

    static void EnsurePlayerSpawnRoot(string sceneName)
    {
        var spawnGo = EnsureRoot("PlayerSpawnPoint");
        if (spawnGo.GetComponent<PlayerSpawnPoint>() != null)
        {
            return;
        }

        if (spawnGo.transform.position.sqrMagnitude < 0.001f)
        {
            spawnGo.transform.position = GameplaySceneCatalog.GetDefaultSpawn(sceneName);
        }

        spawnGo.AddComponent<PlayerSpawnPoint>();
    }

    static void EnsureDirectionalLight()
    {
        var named = GameObject.Find("Directional Light");
        if (named != null)
        {
            return;
        }

        Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        for (int i = 0; i < lights.Length; i++)
        {
            if (lights[i] != null && lights[i].type == LightType.Directional)
            {
                return;
            }
        }

        var lightGo = new GameObject("Directional Light");
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.1f;
        light.color = new Color(1f, 0.96f, 0.9f);
        lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        GameplayCore.Instance?.Log("Created fallback Directional Light for gameplay scene.");
    }

    static void EnsureLevelSpecificContent(string sceneName)
    {
        switch (sceneName)
        {
            case "Level3":
                EnsureLevel3Content();
                break;
            case "Level4":
                EnsureLevel4Content();
                break;
            case "Level5":
                EnsureLevel5Content();
                break;
            case "Level6":
                EnsureLevel6Content();
                break;
            case "Level7":
                EnsureLevel7Content();
                break;
            case "Level8":
                EnsureLevel8Content();
                break;
        }
    }

    static void EnsureLevel3Content()
    {
        var manager = Object.FindFirstObjectByType<LevelManager>();
        if (manager == null)
        {
            var managerGo = new GameObject("LevelManager");
            manager = managerGo.AddComponent<LevelManager>();
        }

        manager.ConfigureGoalTransition("Level4", "Buff 教学完成！", "按 Y 键进入第四关");

        var tracker = Object.FindFirstObjectByType<BuffCollectionTracker>();
        HubAdvancePortal portal = Object.FindFirstObjectByType<HubAdvancePortal>();
        if (tracker != null && portal != null)
        {
            return;
        }

        var systemsGo = GameObject.Find("Level3Systems") ?? new GameObject("Level3Systems");
        var buffHub = GameObject.Find("BuffHub");
        if (buffHub != null)
        {
            systemsGo.transform.SetParent(buffHub.transform, false);
        }

        if (tracker == null)
        {
            tracker = systemsGo.GetComponent<BuffCollectionTracker>() ?? systemsGo.AddComponent<BuffCollectionTracker>();
        }

        if (portal == null)
        {
            portal = systemsGo.GetComponent<HubAdvancePortal>() ?? systemsGo.AddComponent<HubAdvancePortal>();
        }

        var player = PersistentPlayerRig.Player;
        var cc = player != null ? player.GetComponent<CharacterController>() : null;
        const float floorTop = 0.125f;
        Vector3 hubSpawn = cc != null
            ? PlayerAnchorUtility.GetSpawnPositionForGround(Vector3.zero, floorTop + 0.125f, cc)
            : new Vector3(0f, floorTop + 1.25f, 0f);
        portal.Configure(hubSpawn + new Vector3(0f, 0.1f, 2.2f));
    }

    static void EnsureLevel4Content()
    {
        var systemsGo = GameObject.Find("Level4Systems");
        if (systemsGo == null)
        {
            systemsGo = new GameObject("Level4Systems");
            var arena = GameObject.Find("Level4Arena");
            if (arena != null)
            {
                systemsGo.transform.SetParent(arena.transform, false);
            }
        }

        var collectibleManager = systemsGo.GetComponent<CollectibleManager>() ?? systemsGo.AddComponent<CollectibleManager>();
        var portalUnlock = systemsGo.GetComponent<PortalUnlockManager>() ?? systemsGo.AddComponent<PortalUnlockManager>();
        portalUnlock.Configure(new Vector3(0f, 0.125f + 0.525f, 0f), "Level5", 0.3f);
        collectibleManager.RefreshTotalCount();

        var hudGo = GameObject.Find("GameplayHUD");
        if (hudGo != null)
        {
            var progressHud = hudGo.GetComponent<CollectibleProgressHud>() ?? hudGo.AddComponent<CollectibleProgressHud>();
            progressHud.BindTo(collectibleManager);
        }
    }

    static void EnsureLevel5Content()
    {
        Level5ProgressionBootstrap.EnsureLevel5Progression();
    }

    static void EnsureLevel6Content()
    {
        if (GameObject.Find("Level6Arena") == null)
        {
            Level6PlaceholderBuilder.Generate(true);
        }

        Level6ProgressionBootstrap.EnsureLevel6Progression();
    }

    static void EnsureLevel7Content()
    {
        if (GameObject.Find("Level7Arena") == null)
        {
            Level7PlaceholderBuilder.Generate(true);
        }

        Level7ProgressionBootstrap.EnsureLevel7Progression();
    }

    static void EnsureLevel8Content()
    {
        Level8ProgressionBootstrap.EnsureLevel8Progression();
        Level8PlaceholderBuilder.Generate(true);
    }

    static GameObject EnsureRoot(string name)
    {
        var existing = GameObject.Find(name);
        if (existing != null)
        {
            return existing;
        }

        return new GameObject(name);
    }

    static T EnsureComponent<T>(string objectName) where T : Component
    {
        var go = GameObject.Find(objectName);
        if (go == null)
        {
            go = new GameObject(objectName);
        }

        return go.GetComponent<T>() ?? go.AddComponent<T>();
    }
}
