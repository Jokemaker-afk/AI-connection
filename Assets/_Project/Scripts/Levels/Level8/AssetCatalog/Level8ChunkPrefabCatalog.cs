using UnityEngine;

/// <summary>
/// Chunk prefab lookup: BiomeKind + ChunkKind + seed → variant prefab.
/// TODO: Register prefabs as Phase D assets land. Migrate to ScriptableObject profiles.
/// </summary>
public static class Level8ChunkPrefabCatalog
{
    public static bool TryResolve(
        Level8BiomeKind biome,
        Level8ChunkKind chunkKind,
        int seed,
        out GameObject prefab)
    {
        prefab = null;
        // TODO Phase D: weighted variant pick by seed; fallback Generic biome; then null → primitives
        return false;
    }
}
