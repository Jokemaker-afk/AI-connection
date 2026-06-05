using UnityEngine;

public static class Level7PlaceholderBuilder
{
    const float FloorTop = 0.125f;

    public static GameObject Generate(bool removeOld = true)
    {
        if (removeOld)
        {
            DestroyIfExists("Level7Arena");
        }

        var root = new GameObject("Level7Arena");
        var terrain = CreateChild(root.transform, "Terrain");
        var enemies = CreateChild(root.transform, "Enemies");
        var pickups = CreateChild(root.transform, "Pickups");
        var markers = CreateChild(root.transform, "Markers");

        var floorColor = new Color(0.42f, 0.38f, 0.48f);
        var pathColor = new Color(0.5f, 0.46f, 0.4f);

        CreateBlock(terrain, "MainFloor", new Vector3(0f, FloorTop, 0f), new Vector3(32f, 0.25f, 32f), floorColor);
        CreateBlock(terrain, "SpawnPath", new Vector3(0f, FloorTop + 0.02f, -4f), new Vector3(3f, 0.08f, 10f), pathColor);
        CreateBlock(terrain, "MeleeLane", new Vector3(-9f, FloorTop + 0.02f, 2f), new Vector3(6f, 0.08f, 14f), pathColor);
        CreateBlock(terrain, "RangedLane", new Vector3(9f, FloorTop + 0.02f, 2f), new Vector3(6f, 0.08f, 14f), pathColor);
        CreateBlock(terrain, "WalkerLane", new Vector3(0f, FloorTop + 0.02f, 6f), new Vector3(8f, 0.08f, 10f), pathColor);
        CreateBlock(terrain, "ExitPad", new Vector3(0f, FloorTop + 0.04f, 14f), new Vector3(4f, 0.06f, 3f), new Color(0.35f, 0.55f, 0.78f));

        float spawnY = FloorTop + 0.85f;
        EnemyModuleFactory.SpawnEnemy(EnemyKind.TrainingDummy, new Vector3(-9f, spawnY, 4f), Quaternion.identity, enemies);
        EnemyModuleFactory.SpawnEnemy(EnemyKind.TrainingDummy, new Vector3(0f, spawnY, 2.6f), Quaternion.identity, enemies);
        EnemyModuleFactory.SpawnEnemy(EnemyKind.TrainingDummy, new Vector3(-1.4f, spawnY, 2.8f), Quaternion.identity, enemies);
        EnemyModuleFactory.SpawnEnemy(EnemyKind.TrainingDummy, new Vector3(1.4f, spawnY, 2.8f), Quaternion.identity, enemies);
        EnemyModuleFactory.SpawnEnemy(EnemyKind.TrainingRangedTarget, new Vector3(9f, spawnY, 12f), Quaternion.identity, enemies);
        EnemyModuleFactory.SpawnEnemy(EnemyKind.BasicWalker, new Vector3(0f, spawnY, 8f), Quaternion.identity, enemies);

        float pickupY = FloorTop + 0.62f;
        ItemModuleFactory.SpawnWorldPickup(ItemKind.BasicSword, new Vector3(-5f, pickupY, -2f), 1, pickups);
        ItemModuleFactory.SpawnWorldPickup(ItemKind.TrainingBlaster, new Vector3(5f, pickupY, -2f), 1, pickups);

        CreateWorldLabel(markers, "WelcomeLabel", new Vector3(0f, FloorTop + 2.4f, -6f),
            "第七关：武器与敌人教学 · 1~9 切换武器 · 左键攻击");
        CreateWorldLabel(markers, "HintLabel", new Vector3(0f, FloorTop + 1.8f, -8f),
            "近战区：基础剑 · 远程区：训练发射器 · 中央：击败基础敌人");
        CreateWorldLabel(markers, "MeleeZoneLabel", new Vector3(-9f, FloorTop + 2f, 0f), "近战区");
        CreateWorldLabel(markers, "RangedZoneLabel", new Vector3(9f, FloorTop + 2f, 6f), "远程区");
        CreateWorldLabel(markers, "WalkerZoneLabel", new Vector3(0f, FloorTop + 2f, 4f), "基础敌人区");

        var player = GameObject.Find("Player");
        if (player != null)
        {
            GameplayHudBootstrap.BindAll();
            var cc = player.GetComponent<CharacterController>();
            Vector3 spawn = cc != null
                ? PlayerAnchorUtility.GetSpawnPositionForGround(new Vector3(0f, 0f, -2f), FloorTop + 0.125f, cc)
                : new Vector3(0f, FloorTop + 1.25f, -2f);
            player.transform.position = spawn;
            player.transform.rotation = Quaternion.identity;
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
