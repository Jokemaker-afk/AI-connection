using UnityEngine;

/// <summary>
/// Central visual asset resolver for Level8. Phase B: Data Core objectives wired; other types fallback to primitives.
/// See Documentation/LevelDesign/Level8_PrefabDriven_Generation_Roadmap.md
/// </summary>
public static class Level8VisualAssetResolver
{
    public static bool TryResolveChunkPrefab(
        Level8BiomeKind biome,
        Level8ChunkKind chunkKind,
        int seed,
        out GameObject prefab)
    {
        return Level8ChunkPrefabCatalog.TryResolve(biome, chunkKind, seed, out prefab);
    }

    public static bool TryResolveDataCorePrefab(Level8BiomeKind biome, out GameObject prefab)
    {
        return Level8ObjectivePrefabCatalog.TryResolveDataCorePrefab(biome, out prefab);
    }

    public static GameObject ResolveObjectivePrefab(Level8ObjectiveKind kind, Level8BiomeKind biome)
    {
        return TryGetObjectivePrefab(kind, biome, out GameObject prefab) ? prefab : null;
    }

    public static bool TryGetObjectivePrefab(Level8ObjectiveKind kind, Level8BiomeKind biome, out GameObject prefab)
    {
        return Level8ObjectivePrefabCatalog.TryGet(kind, biome, out prefab);
    }

    public static GameObject InstantiateOrFallback(
        GameObject prefab,
        Transform parent,
        Vector3 position,
        Quaternion rotation,
        System.Func<GameObject> buildPlaceholder)
    {
        if (prefab != null)
        {
            return Object.Instantiate(prefab, position, rotation, parent);
        }

        return buildPlaceholder != null ? buildPlaceholder() : null;
    }
}
