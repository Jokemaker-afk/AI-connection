using UnityEngine;

public static class Level6ProgressionBootstrap
{
    const float FloorTop = 0.125f;

    public static void EnsureLevel6Progression()
    {
        GameplayProgressionBootstrap.EnsureProgressionSystems();
        EnsureTaskTracker();

        Transform systemsParent = ResolveSystemsParent();
        var systemsGo = GameObject.Find("Level6Systems");
        if (systemsGo == null)
        {
            systemsGo = new GameObject("Level6Systems");
            systemsGo.transform.SetParent(systemsParent, false);
        }

        var beacon = systemsGo.GetComponent<SceneBeacon>();
        if (beacon == null)
        {
            beacon = systemsGo.AddComponent<SceneBeacon>();
        }

        Vector3 beaconPosition = new Vector3(0f, FloorTop + 0.525f, -14f);
        beacon.Configure(
            beaconPosition,
            "Level7",
            "第七关传送门\n按 Y 进入下一关",
            "工具教学完成",
            "三项任务已完成，前往传送门并按 Y 进入第七关",
            loadPortalOnEnter: false);

        var manager = systemsGo.GetComponent<Level6ProgressionManager>();
        if (manager == null)
        {
            manager = systemsGo.AddComponent<Level6ProgressionManager>();
        }

        manager.Configure("Level7", beacon, activationRange: 4f);

        var portalTransition = systemsGo.GetComponent<Level6PortalTransition>();
        if (portalTransition == null)
        {
            portalTransition = systemsGo.AddComponent<Level6PortalTransition>();
        }

        portalTransition.Bind(manager, beacon != null ? beacon.transform : null, 4f);
        EnsureObjectiveHud(manager);
        EnsureGameEventListener(systemsGo);
        EnsureStarterTools();
        UpdateWorldHints();
    }

    static void EnsureTaskTracker()
    {
        var root = GameObject.Find("GameplayProgression");
        if (root == null)
        {
            return;
        }

        if (root.GetComponent<Level6TaskProgressTracker>() == null)
        {
            root.AddComponent<Level6TaskProgressTracker>();
        }
    }

    static Transform ResolveSystemsParent()
    {
        var arena = GameObject.Find("Level6Arena");
        return arena != null ? arena.transform : null;
    }

    static void EnsureObjectiveHud(Level6ProgressionManager manager)
    {
        var hudGo = GameObject.Find("GameplayHUD");
        if (hudGo == null)
        {
            return;
        }

        var objectiveHud = hudGo.GetComponent<Level6ProgressionHud>();
        if (objectiveHud == null)
        {
            objectiveHud = hudGo.AddComponent<Level6ProgressionHud>();
        }

        objectiveHud.BindTo(manager);
    }

    static void EnsureGameEventListener(GameObject systemsGo)
    {
        if (systemsGo == null)
        {
            return;
        }

        if (systemsGo.GetComponent<Level6GameEventListener>() == null)
        {
            systemsGo.AddComponent<Level6GameEventListener>();
        }
    }

    static void EnsureStarterTools()
    {
        if (!Level6TutorialSettings.GiveDebugToolsOnSceneStart)
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

        EnsureTool(inventory, ItemKind.StonePickaxe);
        EnsureTool(inventory, ItemKind.StoneAxe);
        EnsureTool(inventory, ItemKind.RepairTool);
        inventory.SetSelectedHotbarIndex(0);
    }

    static void EnsureTool(PlayerInventory inventory, ItemKind itemKind)
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

    static void UpdateWorldHints()
    {
        RefreshLabel("WelcomeLabel", "第六关：工具制造教学 · 收集材料并制造石斧、石镐、修理工具");
        RefreshLabel("HintLabel", "F 拾取材料 · E 打开制造 · 1~9 切换工具 · 左键或 F 使用 · 完成后按 Y 进入第七关");
        RefreshLabel("MaterialLabel", "材料区");
    }

    static void RefreshLabel(string objectName, string text)
    {
        var labelRoot = GameObject.Find(objectName);
        if (labelRoot != null)
        {
            ItemWorldLabel.Refresh(labelRoot, text);
        }
    }
}
