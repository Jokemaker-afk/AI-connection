using UnityEngine;

public static class Level6PlaceholderBuilder
{
    const float FloorTop = 0.125f;

    public static GameObject Generate(bool removeOld = true)
    {
        if (removeOld)
        {
            DestroyIfExists("Level6Arena");
        }

        var root = new GameObject("Level6Arena");
        var terrain = CreateChild(root.transform, "Terrain");
        var interactables = CreateChild(root.transform, "Interactables");
        var markers = CreateChild(root.transform, "Markers");

        var groundColor = new Color(0.36f, 0.48f, 0.58f);
        var pathColor = new Color(0.48f, 0.44f, 0.38f);

        CreateBlock(terrain, "MainFloor", new Vector3(0f, FloorTop, 0f), new Vector3(36f, 0.25f, 36f), groundColor);
        CreateBlock(terrain, "CenterPath", new Vector3(0f, FloorTop + 0.02f, 0f), new Vector3(3f, 0.08f, 28f), pathColor);
        CreateBlock(terrain, "WestPath", new Vector3(-8f, FloorTop + 0.02f, 0f), new Vector3(8f, 0.08f, 3f), pathColor);
        CreateBlock(terrain, "EastPath", new Vector3(8f, FloorTop + 0.02f, 0f), new Vector3(8f, 0.08f, 3f), pathColor);
        CreateBlock(terrain, "NorthPath", new Vector3(0f, FloorTop + 0.02f, 8f), new Vector3(3f, 0.08f, 8f), pathColor);

        CreateMiningRock(interactables, new Vector3(-10f, FloorTop + 0.5f, 0f));
        CreateTreeStump(interactables, new Vector3(10f, FloorTop + 0.35f, 0f));
        CreateSignalRelay(interactables, new Vector3(0f, FloorTop + 0.55f, 10f));

        var pickups = CreateChild(root.transform, "Pickups");
        SpawnTutorialMaterials(pickups);

        var stations = CreateChild(root.transform, "Stations");
        CraftingStation.Create(stations, "CraftingTableStation", new Vector3(-4f, FloorTop, -2f), WorkstationKind.Workbench);
        CraftingStation.Create(stations, "FurnaceStation", new Vector3(4f, FloorTop, -2f), WorkstationKind.Furnace);

        CreateWorldLabel(markers, "WelcomeLabel", new Vector3(0f, FloorTop + 2.2f, -6f), "第六关：工具制造教学 · 收集材料并制造石斧、石镐、修理工具");
        CreateWorldLabel(markers, "HintLabel", new Vector3(0f, FloorTop + 1.6f, -8f), "F 拾取材料 · E 打开制造 · 1~9 切换工具 · 左键或 F 使用 · 完成后按 Y 进入第七关");
        CreateWorldLabel(markers, "MaterialLabel", new Vector3(0f, FloorTop + 1.4f, -3.5f), "材料区");
        CreateWorldLabel(markers, "MiningLabel", new Vector3(-10f, FloorTop + 1.8f, -1.5f), "采矿区");
        CreateWorldLabel(markers, "LoggingLabel", new Vector3(10f, FloorTop + 1.8f, -1.5f), "伐木区");
        CreateWorldLabel(markers, "RepairLabel", new Vector3(0f, FloorTop + 1.8f, 8f), "修复区");

        return root;
    }

    static void CreateMiningRock(Transform parent, Vector3 position)
    {
        var root = CreateInteractableRoot(parent, "MiningRock", position, new Color(0.52f, 0.54f, 0.58f), new Vector3(1.4f, 1.1f, 1.4f));
        var interactable = root.AddComponent<ToolInteractable>();
        interactable.ConfigureResource(
            ToolKind.Pickaxe,
            "矿点",
            "使用石镐采矿",
            new[]
            {
                new ToolReward(ItemKind.Stone, 2, 4),
                new ToolReward(ItemKind.OreFragment, 1, 2, 0.75f),
            },
            Level6TaskProgressTracker.MineOnceTaskId,
            hitsRequired: 1,
            consumeOnComplete: false,
            dropToWorld: true,
            addToInventory: false);
        ItemWorldLabel.Create(root.transform, "矿点", Vector3.up * 1.1f, 0.1f);
    }

    static void CreateTreeStump(Transform parent, Vector3 position)
    {
        var root = CreateInteractableRoot(parent, "TreeStump", position, new Color(0.48f, 0.32f, 0.18f), new Vector3(1.1f, 0.7f, 1.1f));
        var interactable = root.AddComponent<ToolInteractable>();
        interactable.ConfigureResource(
            ToolKind.Axe,
            "木桩",
            "使用石斧伐木",
            new[]
            {
                new ToolReward(ItemKind.Wood, 2, 4),
                new ToolReward(ItemKind.Fiber, 1, 1, 0.5f),
            },
            Level6TaskProgressTracker.ChopOnceTaskId,
            hitsRequired: 1,
            consumeOnComplete: false,
            dropToWorld: true,
            addToInventory: false);
        ItemWorldLabel.Create(root.transform, "木桩", Vector3.up * 0.85f, 0.1f);
    }

    static void CreateSignalRelay(Transform parent, Vector3 position)
    {
        var root = CreateInteractableRoot(parent, "SignalRelay", position, new Color(0.38f, 0.62f, 0.92f), new Vector3(1f, 1.2f, 1f));
        var interactable = root.AddComponent<ToolInteractable>();
        interactable.ConfigureEventTrigger(
            ToolKind.RepairTool,
            "信号中继器",
            "使用修理工具修复",
            Level6TaskProgressTracker.RepairSignalRelayTaskId,
            "signal_relay_repaired",
            "信号中继器已修复。",
            hitsRequired: 1,
            consumeOnComplete: false);
        ItemWorldLabel.Create(root.transform, "信号中继器", Vector3.up * 1.05f, 0.1f);
    }

    static void SpawnTutorialMaterials(Transform parent)
    {
        float pickupY = FloorTop + 0.62f;

        // Near spawn — enough for Stick, Rope, and all three tools.
        PlacePickupCluster(parent, new Vector3(-2f, pickupY, -3.5f), ItemKind.Wood, 6);
        PlacePickupCluster(parent, new Vector3(0f, pickupY, -4f), ItemKind.Stone, 8);
        PlacePickupCluster(parent, new Vector3(2f, pickupY, -3.5f), ItemKind.Grass, 6);
        PlacePickupCluster(parent, new Vector3(-3.5f, pickupY, -2.5f), ItemKind.Fiber, 6);
        PlacePickupCluster(parent, new Vector3(3.5f, pickupY, -2.5f), ItemKind.Flint, 2);
        PlacePickupCluster(parent, new Vector3(0f, pickupY, -2f), ItemKind.OreFragment, 2);
        PlacePickupCluster(parent, new Vector3(-4f, pickupY, -3f), ItemKind.Coal, 4);
        PlacePickupCluster(parent, new Vector3(4f, pickupY, -3f), ItemKind.Berry, 4);

        // Near mining area.
        PlacePickupCluster(parent, new Vector3(-12f, pickupY, 1.5f), ItemKind.Stone, 4);
        PlacePickupCluster(parent, new Vector3(-11f, pickupY, -1.5f), ItemKind.OreFragment, 2);

        // Near logging area.
        PlacePickupCluster(parent, new Vector3(12f, pickupY, 1.5f), ItemKind.Wood, 4);
        PlacePickupCluster(parent, new Vector3(11f, pickupY, -1.5f), ItemKind.Grass, 2);

        // Near repair area.
        PlacePickupCluster(parent, new Vector3(2f, pickupY, 9f), ItemKind.OreFragment, 2);
        PlacePickupCluster(parent, new Vector3(-2f, pickupY, 9f), ItemKind.Stone, 2);
        PlacePickupCluster(parent, new Vector3(0f, pickupY, 8.5f), ItemKind.Fiber, 2);
        PlacePickupCluster(parent, new Vector3(3f, pickupY, 9.5f), ItemKind.Coal, 2);
        PlacePickupCluster(parent, new Vector3(-3f, pickupY, 9.5f), ItemKind.Berry, 3);
    }

    static void PlacePickupCluster(Transform parent, Vector3 center, ItemKind kind, int totalAmount)
    {
        if (totalAmount <= 0 || !ItemKindUtility.IsValid(kind))
        {
            return;
        }

        int piles = Mathf.Clamp(Mathf.CeilToInt(totalAmount / 3f), 1, 3);
        int remaining = totalAmount;

        for (int i = 0; i < piles; i++)
        {
            int pileAmount = i == piles - 1 ? remaining : Mathf.Max(1, totalAmount / piles);
            remaining -= pileAmount;

            float angle = i * Mathf.PI * 2f / piles;
            Vector3 offset = new Vector3(Mathf.Cos(angle) * 0.55f, 0f, Mathf.Sin(angle) * 0.55f);
            ItemModuleFactory.SpawnWorldPickup(kind, center + offset, pileAmount, parent);
        }
    }

    static GameObject CreateInteractableRoot(Transform parent, string name, Vector3 position, Color color, Vector3 scale)
    {
        var root = GameObject.CreatePrimitive(PrimitiveType.Cube);
        root.name = name;
        root.transform.SetParent(parent, false);
        root.transform.position = position;
        root.transform.localScale = scale;
        root.isStatic = true;

        var renderer = root.GetComponent<Renderer>();
        if (renderer != null)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = color;
            renderer.sharedMaterial = material;
        }

        return root;
    }

    static void CreateWorldLabel(Transform parent, string name, Vector3 position, string text)
    {
        var labelRoot = new GameObject(name);
        labelRoot.transform.SetParent(parent, false);
        labelRoot.transform.position = position;
        ItemWorldLabel.Create(labelRoot.transform, text, Vector3.zero, 0.12f);
    }

    static GameObject CreateBlock(Transform parent, string blockName, Vector3 position, Vector3 scale, Color color)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = blockName;
        cube.transform.SetParent(parent, false);
        cube.transform.position = position;
        cube.transform.localScale = scale;
        cube.isStatic = true;

        var renderer = cube.GetComponent<Renderer>();
        if (renderer != null)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = color;
            renderer.sharedMaterial = material;
        }

        if (parent != null && parent.name == "Terrain")
        {
            cube.AddComponent<StaticPlacementGround>();
        }

        return cube;
    }

    static Transform CreateChild(Transform parent, string childName)
    {
        var go = new GameObject(childName);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    static void DestroyIfExists(string objectName)
    {
        var obj = GameObject.Find(objectName);
        if (obj != null)
        {
            Object.DestroyImmediate(obj);
        }
    }
}
