using UnityEngine;

/// <summary>Marks a hazard zone with warning visuals and a distance label.</summary>
public static class Level8HazardZoneBuilder
{
    public static GameObject Build(Transform hazardsRoot, Vector3 center, Level8BiomeProfile biome)
    {
        if (hazardsRoot == null)
        {
            return null;
        }

        var root = new GameObject($"HazardZone_{center.x:F0}_{center.z:F0}");
        root.transform.SetParent(hazardsRoot, false);
        root.transform.position = center;

        float size = Level8GenerationFlags.ChunkSize * 0.42f * biome.HazardSizeMultiplier;
        Color warning = Color.Lerp(biome.HazardColor, new Color(0.95f, 0.35f, 0.12f), 0.45f);
        warning.a = 0.42f;

        Level8VisualHierarchyUtility.CreateGlowRing(
            root.transform,
            "WarningRing",
            new Vector3(0f, Level8ChunkPlaceholderBuilder.FloorTop + 0.04f, 0f),
            warning,
            size * 0.5f,
            0.1f);

        CreateWarningPosts(root.transform, size * 0.5f, warning);

        var zone = GameObject.CreatePrimitive(PrimitiveType.Cube);
        zone.name = "HazardGround";
        zone.transform.SetParent(root.transform, false);
        zone.transform.localPosition = new Vector3(0f, Level8ChunkPlaceholderBuilder.FloorTop + 0.03f, 0f);
        zone.transform.localScale = new Vector3(size, 0.06f, size);
        Object.Destroy(zone.GetComponent<Collider>());
        Level8VisualMaterialUtility.ApplyColor(zone.GetComponent<Renderer>(), warning, transparent: true);

        Level8VisualHierarchyUtility.CreateDistanceLabelAnchor(
            root.transform,
            "危险区域",
            new Vector3(0f, 2.4f, 0f),
            Level8VisualHierarchyFlags.HazardLabelDistance,
            characterSize: 0.12f);

        return root;
    }

    static void CreateWarningPosts(Transform parent, float radius, Color color)
    {
        Vector3[] corners =
        {
            new Vector3(radius, Level8ChunkPlaceholderBuilder.FloorTop + 0.5f, radius),
            new Vector3(-radius, Level8ChunkPlaceholderBuilder.FloorTop + 0.5f, radius),
            new Vector3(radius, Level8ChunkPlaceholderBuilder.FloorTop + 0.5f, -radius),
            new Vector3(-radius, Level8ChunkPlaceholderBuilder.FloorTop + 0.5f, -radius),
        };

        for (int i = 0; i < corners.Length; i++)
        {
            var post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            post.name = $"WarningPost_{i}";
            post.transform.SetParent(parent, false);
            post.transform.localPosition = corners[i];
            post.transform.localScale = new Vector3(0.18f, 0.55f, 0.18f);
            Object.Destroy(post.GetComponent<Collider>());
            Level8VisualMaterialUtility.ApplyColor(post.GetComponent<Renderer>(), Color.Lerp(color, Color.red, 0.35f));
        }
    }
}
