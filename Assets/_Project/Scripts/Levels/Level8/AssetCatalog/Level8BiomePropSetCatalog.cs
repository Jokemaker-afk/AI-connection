using UnityEngine;

/// <summary>
/// Biome → decoration / environment prop sets.
/// TODO: Phase E — PlainPropSet, DesertPropSet, ForestPropSet, GenericPropSet
/// </summary>
public static class Level8BiomePropSetCatalog
{
    public static bool TryGetPropSet(Level8BiomeKind biome, out Level8BiomePropSet propSet)
    {
        propSet = null;
        // TODO Phase E: return registered prop sets; fallback Generic
        return false;
    }

    public static bool TryPickProp(
        Level8BiomeKind biome,
        Level8PropCategory category,
        Level8ChunkKind chunkKind,
        int seed,
        out Level8PropSpawnRule rule)
    {
        rule = default;
        // TODO Phase E: weighted random by seed within allowed chunk kinds
        return false;
    }
}
