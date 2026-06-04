using UnityEngine;

public static class Level5PlaceholderBuilder
{
    const float FloorTop = 0.125f;
    const int PickupSeed = 20260601;

    static readonly ItemKind[] BasicMaterials =
    {
        ItemKind.Wood,
        ItemKind.Stone,
        ItemKind.Grass,
        ItemKind.Fiber,
        ItemKind.Vine,
        ItemKind.Flint,
        ItemKind.Clay,
        ItemKind.OreFragment,
        ItemKind.Coal,
        ItemKind.Berry,
    };

    public static GameObject Generate(bool removeOld = true)
    {
        if (removeOld)
        {
            DestroyIfExists("Level5Arena");
        }

        var root = new GameObject("Level5Arena");
        var terrain = CreateChild(root.transform, "Terrain");
        var pickups = CreateChild(root.transform, "Pickups");
        var markers = CreateChild(root.transform, "Markers");

        var groundColor = new Color(0.38f, 0.52f, 0.34f);
        var pathColor = new Color(0.48f, 0.44f, 0.38f);
        var rockColor = new Color(0.42f, 0.44f, 0.48f);

        CreateBlock(terrain, "MainBasin", new Vector3(0f, FloorTop, 0f), new Vector3(44f, 0.25f, 44f), groundColor);
        CreateBlock(terrain, "NorthPad", new Vector3(0f, FloorTop + 0.02f, 14f), new Vector3(10f, 0.1f, 8f), pathColor);
        CreateBlock(terrain, "SouthPad", new Vector3(0f, FloorTop + 0.02f, -12f), new Vector3(12f, 0.1f, 7f), pathColor);
        CreateBlock(terrain, "EastPad", new Vector3(14f, FloorTop + 0.02f, 2f), new Vector3(8f, 0.1f, 10f), pathColor);
        CreateBlock(terrain, "WestPad", new Vector3(-13f, FloorTop + 0.02f, -2f), new Vector3(7f, 0.1f, 9f), pathColor);
        CreateBlock(terrain, "RockA", new Vector3(-8f, FloorTop + 0.45f, 7f), new Vector3(2f, 0.9f, 1.6f), rockColor);
        CreateBlock(terrain, "RockB", new Vector3(10f, FloorTop + 0.35f, -5f), new Vector3(1.4f, 0.7f, 2.2f), rockColor * 0.95f);

        ScatterBasicMaterials(pickups);
        PlaceMaterialRing(pickups, new Vector3(-10f, FloorTop + 0.65f, 8f), 3.5f);
        PlaceMaterialRing(pickups, new Vector3(12f, FloorTop + 0.65f, -8f), 3.2f);

        CreateWorldLabel(markers, "WelcomeLabel", new Vector3(0f, FloorTop + 2.2f, -6f), "第五关：制造与建造 · 先制作并放置工作台与熔炉");
        CreateWorldLabel(markers, "HintLabel", new Vector3(0f, FloorTop + 1.6f, -8f), "收集材料 → C 随身制作 → 放置工作台 → 在工作台制作熔炉 → 激活信标");

        var player = GameObject.Find("Player");
        if (player != null)
        {
            GameplayHudBootstrap.BindAll();
            var cc = player.GetComponent<CharacterController>();
            Vector3 spawn = cc != null
                ? PlayerAnchorUtility.GetSpawnPositionForGround(new Vector3(0f, 0f, -4f), FloorTop + 0.125f, cc)
                : new Vector3(0f, FloorTop + 1.25f, -4f);
            player.transform.position = spawn;
            player.transform.rotation = Quaternion.identity;
        }

        return root;
    }

    static void ScatterBasicMaterials(Transform parent)
    {
        var random = new System.Random(PickupSeed);
        int pickupCount = 30;

        for (int i = 0; i < pickupCount; i++)
        {
            float x = Mathf.Lerp(-18f, 18f, (float)random.NextDouble());
            float z = Mathf.Lerp(-18f, 18f, (float)random.NextDouble());
            if (x * x + z * z < 12f)
            {
                z += 10f;
            }

            ItemKind kind = BasicMaterials[i % BasicMaterials.Length];
            if (random.NextDouble() > 0.65d)
            {
                kind = BasicMaterials[random.Next(BasicMaterials.Length)];
            }

            int amount = random.NextDouble() > 0.75d ? 2 : 1;
            float y = FloorTop + 0.55f + (float)random.NextDouble() * 0.25f;
            ItemModuleFactory.SpawnWorldPickup(kind, new Vector3(x, y, z), amount, parent);
        }
    }

    static void PlaceMaterialRing(Transform parent, Vector3 center, float radius)
    {
        for (int i = 0; i < BasicMaterials.Length; i++)
        {
            float angle = i * Mathf.PI * 2f / BasicMaterials.Length;
            Vector3 pos = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            ItemModuleFactory.SpawnWorldPickup(BasicMaterials[i], pos, 2, parent);
        }
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
