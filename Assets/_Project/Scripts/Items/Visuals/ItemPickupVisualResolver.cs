using UnityEngine;

/// <summary>
/// Resolves world pickup visual prefabs: ItemCatalog first, then Level8 resource registry.
/// </summary>
public static class ItemPickupVisualResolver
{
    public static bool TryResolve(ItemKind kind, Level8BiomeKind biome, out GameObject prefab)
    {
        prefab = null;

        if (ItemCatalog.TryGet(kind, out ItemData catalogData) && catalogData.WorldPickupPrefab != null)
        {
            prefab = catalogData.WorldPickupPrefab;
            return true;
        }

        return Level8ResourcePrefabCatalog.TryGet(biome, kind, out prefab) && prefab != null;
    }

    public static bool HasResolvedVisual(ItemKind kind, Level8BiomeKind biome)
    {
        return TryResolve(kind, biome, out _);
    }
}
