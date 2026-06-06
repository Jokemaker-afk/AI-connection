using UnityEngine;

/// <summary>
/// Resource pickup visual prefab lookup: biome+kind → generic kind → ItemCatalog (via ItemPickupVisualResolver).
/// </summary>
public static class Level8ResourcePrefabCatalog
{
    public static bool TryGet(Level8BiomeKind biome, ItemKind kind, out GameObject prefab)
    {
        prefab = null;

        Level8ResourcePrefabRegistry registry = Level8ResourcePrefabRegistry.Load();
        if (registry == null)
        {
            return false;
        }

        return registry.TryGetPrefab(biome, kind, out prefab) && prefab != null;
    }
}
