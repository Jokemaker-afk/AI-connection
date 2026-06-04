using UnityEngine;

public static class Level4PlaceholderBuilder
{
    const float FloorTop = 0.125f;
    const int PickupSeed = 20260328;

    static readonly ItemKind[] PickupKinds =
    {
        ItemKind.RedBlock,
        ItemKind.OrangeBlock,
        ItemKind.YellowBlock,
        ItemKind.GreenBlock,
        ItemKind.CyanBlock,
        ItemKind.BlueBlock,
        ItemKind.PurpleBlock,
        ItemKind.PinkBlock,
        ItemKind.WhiteBlock,
        ItemKind.GrayBlock,
    };

    public static GameObject Generate(bool removeOld = true)
    {
        if (removeOld)
        {
            DestroyIfExists("Level4Arena");
        }

        var root = new GameObject("Level4Arena");
        var terrain = CreateChild(root.transform, "Terrain");
        var pickups = CreateChild(root.transform, "Pickups");
        var markers = CreateChild(root.transform, "Markers");

        var groundColor = new Color(0.42f, 0.58f, 0.38f);
        var pathColor = new Color(0.52f, 0.48f, 0.42f);
        var hillColor = new Color(0.48f, 0.54f, 0.4f);
        var rockColor = new Color(0.45f, 0.47f, 0.5f);

        CreateBlock(terrain, "MainBasin", new Vector3(0f, FloorTop, 0f), new Vector3(56f, 0.25f, 56f), groundColor);
        CreateBlock(terrain, "NorthPath", new Vector3(0f, FloorTop + 0.02f, 18f), new Vector3(10f, 0.12f, 18f), pathColor);
        CreateBlock(terrain, "SouthPath", new Vector3(0f, FloorTop + 0.02f, -16f), new Vector3(12f, 0.12f, 16f), pathColor);
        CreateBlock(terrain, "EastPath", new Vector3(18f, FloorTop + 0.02f, 2f), new Vector3(16f, 0.12f, 8f), pathColor);
        CreateBlock(terrain, "WestPath", new Vector3(-17f, FloorTop + 0.02f, -4f), new Vector3(14f, 0.12f, 9f), pathColor);

        CreateHill(terrain, "Hill_NW", new Vector3(-16f, FloorTop + 1.2f, 14f), new Vector3(10f, 2.4f, 10f), hillColor);
        CreateHill(terrain, "Hill_NE", new Vector3(17f, FloorTop + 1.6f, 16f), new Vector3(11f, 3.2f, 9f), hillColor * 1.05f);
        CreateHill(terrain, "Hill_SE", new Vector3(15f, FloorTop + 0.9f, -15f), new Vector3(9f, 1.8f, 8f), hillColor * 0.95f);
        CreateHill(terrain, "Hill_SW", new Vector3(-14f, FloorTop + 1.1f, -13f), new Vector3(8f, 2.2f, 8f), hillColor * 0.92f);
        CreateHill(terrain, "CentralMound", new Vector3(4f, FloorTop + 0.55f, 5f), new Vector3(7f, 1.1f, 6f), hillColor * 1.08f);

        CreateBlock(terrain, "RockA", new Vector3(-6f, FloorTop + 0.55f, 8f), new Vector3(2.2f, 1.1f, 1.8f), rockColor);
        CreateBlock(terrain, "RockB", new Vector3(9f, FloorTop + 0.45f, -3f), new Vector3(1.6f, 0.9f, 2.4f), rockColor * 0.95f);
        CreateBlock(terrain, "RockC", new Vector3(-10f, FloorTop + 0.35f, -6f), new Vector3(2.8f, 0.7f, 1.4f), rockColor * 1.05f);
        CreateBlock(terrain, "PondPad", new Vector3(12f, FloorTop + 0.01f, 2f), new Vector3(5f, 0.08f, 4f), new Color(0.28f, 0.62f, 0.82f, 0.85f));

        ScatterPickups(pickups);
        CreateSign(markers, "WelcomeSign", new Vector3(0f, FloorTop + 2.4f, -8f), "第四关：探索地形，按 F 拾取，按 B 打开背包");
        CreateSign(markers, "HintSign", new Vector3(0f, FloorTop + 1.8f, -10f), "1-9 切换物品栏  ·  背包与物品栏点击交换");

        var player = GameObject.Find("Player");
        if (player != null)
        {
            EnsureInventoryComponents(player);
            var cc = player.GetComponent<CharacterController>();
            Vector3 spawn = cc != null
                ? PlayerAnchorUtility.GetSpawnPositionForGround(Vector3.zero, FloorTop + 0.125f, cc)
                : new Vector3(0f, FloorTop + 1.25f, 0f);
            player.transform.position = spawn;
            player.transform.rotation = Quaternion.identity;
        }

        return root;
    }

    static void ScatterPickups(Transform parent)
    {
        var random = new System.Random(PickupSeed);
        int pickupCount = 36;

        for (int i = 0; i < pickupCount; i++)
        {
            float x = Mathf.Lerp(-22f, 22f, (float)random.NextDouble());
            float z = Mathf.Lerp(-22f, 22f, (float)random.NextDouble());
            if (x * x + z * z < 16f)
            {
                z += 12f;
            }

            ItemKind kind = PickupKinds[i % PickupKinds.Length];
            if (random.NextDouble() > 0.7d)
            {
                kind = PickupKinds[random.Next(PickupKinds.Length)];
            }

            float y = FloorTop + 0.55f + (float)random.NextDouble() * 0.35f;
            ItemModuleFactory.SpawnWorldPickup(kind, new Vector3(x, y, z), 1, parent);
        }

        PlacePickupRing(parent, new Vector3(0f, FloorTop + 0.65f, 6f), 4.5f);
        PlacePickupRing(parent, new Vector3(-12f, FloorTop + 0.7f, 10f), 3.2f);
        PlacePickupRing(parent, new Vector3(14f, FloorTop + 0.75f, -10f), 3.5f);
    }

    static void PlacePickupRing(Transform parent, Vector3 center, float radius)
    {
        for (int i = 0; i < PickupKinds.Length; i++)
        {
            float angle = i * Mathf.PI * 2f / PickupKinds.Length;
            Vector3 pos = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            ItemModuleFactory.SpawnWorldPickup(PickupKinds[i], pos, 1, parent);
        }
    }

    static void CreateHill(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
    {
        CreateBlock(parent, name, position, scale, color);
    }

    static void EnsureInventoryComponents(GameObject player)
    {
        GameplayHudBootstrap.BindAll();
    }

    static void CreateSign(Transform parent, string name, Vector3 position, string text)
    {
        var sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sign.name = name;
        sign.transform.SetParent(parent, false);
        sign.transform.position = position;
        sign.transform.localScale = new Vector3(6f, 1f, 0.12f);
        sign.isStatic = true;
        Object.DestroyImmediate(sign.GetComponent<Collider>());

        var renderer = sign.GetComponent<Renderer>();
        if (renderer != null)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(0.14f, 0.18f, 0.24f);
            renderer.sharedMaterial = material;
        }
    }

    static GameObject CreateBlock(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
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

    static Transform CreateChild(Transform parent, string name)
    {
        var go = new GameObject(name);
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
