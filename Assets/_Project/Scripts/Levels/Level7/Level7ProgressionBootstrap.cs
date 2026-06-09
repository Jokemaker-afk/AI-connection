using UnityEngine;

public static class Level7ProgressionBootstrap
{
    const float FloorTop = 0.125f;

    public static void EnsureLevel7Progression()
    {
        GameplayProgressionBootstrap.EnsureProgressionSystems();
        EnsureTaskTracker();

        Transform systemsParent = ResolveSystemsParent();
        var systemsGo = GameObject.Find("Level7Systems");
        if (systemsGo == null)
        {
            systemsGo = new GameObject("Level7Systems");
            systemsGo.transform.SetParent(systemsParent, false);
        }

        var beacon = systemsGo.GetComponent<SceneBeacon>();
        if (beacon == null)
        {
            beacon = systemsGo.AddComponent<SceneBeacon>();
        }

        Vector3 beaconPosition = new Vector3(0f, FloorTop + 0.525f, 14f);
        beacon.Configure(
            beaconPosition,
            "Level8",
            "第八关传送门\n按 Y 进入下一关",
            "武器教学完成",
            "完成三项战斗目标后前往传送门",
            loadPortalOnEnter: false);

        var manager = systemsGo.GetComponent<Level7ProgressionManager>();
        if (manager == null)
        {
            manager = systemsGo.AddComponent<Level7ProgressionManager>();
        }

        manager.Configure("Level8", beacon, activationRange: 4f);

        var portalTransition = systemsGo.GetComponent<Level7PortalTransition>();
        if (portalTransition == null)
        {
            portalTransition = systemsGo.AddComponent<Level7PortalTransition>();
        }

        portalTransition.Bind(manager, beacon != null ? beacon.transform : null, 4f);
        EnsureObjectiveHud(manager);
        EnsureStarterWeapons();
    }

    static void EnsureTaskTracker()
    {
        var root = GameObject.Find("GameplayProgression");
        if (root == null)
        {
            return;
        }

        if (root.GetComponent<Level7TaskProgressTracker>() == null)
        {
            root.AddComponent<Level7TaskProgressTracker>();
        }
    }

    static Transform ResolveSystemsParent()
    {
        var arena = GameObject.Find("Level7Arena");
        return arena != null ? arena.transform : null;
    }

    static void EnsureObjectiveHud(Level7ProgressionManager manager)
    {
        var hudGo = RuntimeSingletonCleanup.ResolveSingleGameplayHud();
        if (hudGo == null)
        {
            return;
        }

        var objectiveHud = hudGo.GetComponent<Level7ProgressionHud>();
        if (objectiveHud == null)
        {
            objectiveHud = hudGo.AddComponent<Level7ProgressionHud>();
        }

        objectiveHud.BindTo(manager);
        LevelObjectiveUiRegistry.NotifyProviderBound("Level7", nameof(Level7ProgressionManager));
    }

    static void EnsureStarterWeapons()
    {
        if (!Level7TutorialSettings.GiveDebugWeaponsOnSceneStart)
        {
            return;
        }

        if (GameplayCore.Exists && GameplayCore.Instance.ProgressionState.HasPersistentState)
        {
            return;
        }

        var player = PersistentPlayerRig.Player ?? GameObject.Find("Player");
        if (player == null)
        {
            return;
        }

        var inventory = player.GetComponent<PlayerInventory>();
        if (inventory == null)
        {
            return;
        }

        EnsureWeapon(inventory, ItemKind.BasicSword);
        EnsureWeapon(inventory, ItemKind.TrainingBlaster);
        inventory.SetSelectedHotbarIndex(0);
    }

    static void EnsureWeapon(PlayerInventory inventory, ItemKind itemKind)
    {
        if (inventory == null || !ItemKindUtility.IsValid(itemKind))
        {
            return;
        }

        if (inventory.CountItem(itemKind) > 0)
        {
            return;
        }

        UnifiedInventoryAcquisition.TryAcquire(inventory, itemKind, 1);
    }
}
