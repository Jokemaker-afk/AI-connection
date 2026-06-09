using UnityEngine;

public static class Level5ProgressionBootstrap
{
    const float FloorTop = 0.125f;

    public static void EnsureLevel5Progression()
    {
        GameplayProgressionBootstrap.EnsureProgressionSystems();
        RemovePreplacedWorkstations();

        Transform systemsParent = ResolveSystemsParent();
        var systemsGo = GameObject.Find("Level5Systems");
        if (systemsGo == null)
        {
            systemsGo = new GameObject("Level5Systems");
            systemsGo.transform.SetParent(systemsParent, false);
        }

        var beacon = systemsGo.GetComponent<SceneBeacon>();
        if (beacon == null)
        {
            beacon = systemsGo.AddComponent<SceneBeacon>();
        }

        Vector3 beaconPosition = new Vector3(0f, FloorTop + 0.525f, 12f);
        beacon.Configure(beaconPosition, "Level6");

        var manager = systemsGo.GetComponent<SceneProgressionManager>();
        if (manager == null)
        {
            manager = systemsGo.AddComponent<SceneProgressionManager>();
        }

        manager.Configure(
            new[] { ItemKind.CraftingTable, ItemKind.Furnace },
            "Level6",
            beacon,
            latchBeacon: true);

        EnsureObjectiveHud(manager);
        UpdateWorldHints();
    }

    static Transform ResolveSystemsParent()
    {
        var arena = GameObject.Find("Level5Arena");
        return arena != null ? arena.transform : null;
    }

    static void RemovePreplacedWorkstations()
    {
        var craftingTable = GameObject.Find("CraftingTableStation");
        if (craftingTable != null)
        {
            Object.Destroy(craftingTable);
        }

        var stationsRoot = GameObject.Find("Stations");
        if (stationsRoot == null)
        {
            return;
        }

        CraftingStation[] stations = stationsRoot.GetComponentsInChildren<CraftingStation>(true);
        for (int i = 0; i < stations.Length; i++)
        {
            if (stations[i] != null)
            {
                Object.Destroy(stations[i].gameObject);
            }
        }

        if (stationsRoot.transform.childCount == 0)
        {
            Object.Destroy(stationsRoot);
        }
    }

    static void EnsureObjectiveHud(SceneProgressionManager manager)
    {
        var hudGo = RuntimeSingletonCleanup.ResolveSingleGameplayHud();
        if (hudGo == null)
        {
            return;
        }

        var objectiveHud = hudGo.GetComponent<SceneProgressionHud>();
        if (objectiveHud == null)
        {
            objectiveHud = hudGo.AddComponent<SceneProgressionHud>();
        }

        objectiveHud.BindTo(manager);
        LevelObjectiveUiRegistry.NotifyProviderBound("Level5", nameof(SceneProgressionManager));
    }

    static void UpdateWorldHints()
    {
        RefreshLabel("WelcomeLabel", "第五关：制造与建造 · 先制作并放置工作台与熔炉");
        RefreshLabel("HintLabel", "收集材料 → C 随身制作 → 放置工作台 → 在工作台制作熔炉 → 激活信标");
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
