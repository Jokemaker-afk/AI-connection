using UnityEngine;

public static class Level8PlaceholderBuilder
{
    public static GameObject Generate(bool removeOld = true, int? seedOverride = null)
    {
        if (Level8GenerationFlags.UseFixedTestLayout)
        {
            return Level8FixedLayoutBuilder.Generate(removeOld);
        }

        if (Level8GenerationFlags.UseSeededRandomLayout)
        {
            return Level8SeededLayoutBuilder.Generate(removeOld, seedOverride);
        }

        return GenerateLegacyPlaceholder(removeOld);
    }

    static GameObject GenerateLegacyPlaceholder(bool removeOld)
    {
        if (removeOld)
        {
            Level8MapUtility.DestroyIfExists("Level8Arena");
        }

        var root = new GameObject("Level8Arena");
        var terrain = Level8MapUtility.CreateChild(root.transform, "Terrain");
        var markers = Level8MapUtility.CreateChild(root.transform, "Markers");

        CreateBlock(terrain, "MainFloor", new Vector3(0f, Level8ChunkPlaceholderBuilder.FloorTop, 0f), new Vector3(24f, 0.25f, 24f), new Color(0.34f, 0.52f, 0.38f));
        Level8MapUtility.CreateWorldLabel(markers, "WelcomeLabel", new Vector3(0f, Level8ChunkPlaceholderBuilder.FloorTop + 2f, -2f), "第八关占位 · 半随机探索（待实现）");

        return root;
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

        cube.AddComponent<StaticPlacementGround>();
        return cube;
    }
}
