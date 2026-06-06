using UnityEngine;

/// <summary>
/// Hazard zone visual prefabs by hazard kind and/or biome.
/// TODO: Phase F — register hazard prefabs; wire Level8HazardZoneBuilder
/// </summary>
public static class Level8HazardPrefabCatalog
{
    public static bool TryGet(Level8BiomeKind biome, Level8HazardKind hazardKind, out GameObject prefab)
    {
        prefab = null;
        // TODO Phase F: map hazard profiles to prefabs under Prefabs/Level8/HazardPrefabs/
        return false;
    }
}
