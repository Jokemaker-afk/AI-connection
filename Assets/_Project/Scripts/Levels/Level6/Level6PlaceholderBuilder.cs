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

        var stations = CreateChild(root.transform, "Stations");
        CraftingStation.Create(stations, "CraftingTableStation", new Vector3(-4f, FloorTop, -2f), WorkstationKind.Workbench);
        CraftingStation.Create(stations, "FurnaceStation", new Vector3(4f, FloorTop, -2f), WorkstationKind.Furnace);

        CreateWorldLabel(markers, "WelcomeLabel", new Vector3(0f, FloorTop + 2.2f, -6f), "第六关：手持工具教学 · 石镐采矿、石斧伐木、修理工具修复中继器");
        CreateWorldLabel(markers, "HintLabel", new Vector3(0f, FloorTop + 1.6f, -8f), "1~9 或滚轮切换工具 · 准星对准目标 · 左键或 F 使用 · 完成后前往传送门按 Y 进入第七关");
        CreateWorldLabel(markers, "MiningLabel", new Vector3(-10f, FloorTop + 1.8f, -1.5f), "采矿区");
        CreateWorldLabel(markers, "LoggingLabel", new Vector3(10f, FloorTop + 1.8f, -1.5f), "伐木区");
        CreateWorldLabel(markers, "RepairLabel", new Vector3(0f, FloorTop + 1.8f, 8f), "修复区");

        return root;
    }

    static void CreateMiningRock(Transform parent, Vector3 position)
    {
        var root = CreateInteractableRoot(parent, "MiningRock", position, new Color(0.52f, 0.54f, 0.58f), new Vector3(1.4f, 1.1f, 1.4f));
        var interactable = root.AddComponent<ToolInteractable>();
        interactable.Configure(
            ToolKind.Pickaxe,
            "矿点",
            "使用石镐采矿",
            ItemKind.Stone,
            2,
            Level6TaskProgressTracker.MineOnceTaskId);
        ItemWorldLabel.Create(root.transform, "矿点", Vector3.up * 1.1f, 0.1f);
    }

    static void CreateTreeStump(Transform parent, Vector3 position)
    {
        var root = CreateInteractableRoot(parent, "TreeStump", position, new Color(0.48f, 0.32f, 0.18f), new Vector3(1.1f, 0.7f, 1.1f));
        var interactable = root.AddComponent<ToolInteractable>();
        interactable.Configure(
            ToolKind.Axe,
            "木桩",
            "使用石斧伐木",
            ItemKind.Wood,
            2,
            Level6TaskProgressTracker.ChopOnceTaskId);
        ItemWorldLabel.Create(root.transform, "木桩", Vector3.up * 0.85f, 0.1f);
    }

    static void CreateSignalRelay(Transform parent, Vector3 position)
    {
        var root = CreateInteractableRoot(parent, "SignalRelay", position, new Color(0.38f, 0.62f, 0.92f), new Vector3(1f, 1.2f, 1f));
        var interactable = root.AddComponent<ToolInteractable>();
        interactable.Configure(
            ToolKind.RepairTool,
            "信号中继器",
            "使用修理工具修复",
            ItemKind.None,
            0,
            Level6TaskProgressTracker.RepairSignalRelayTaskId,
            hitsRequired: 1,
            consumeOnComplete: false);
        ItemWorldLabel.Create(root.transform, "信号中继器", Vector3.up * 1.05f, 0.1f);
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
