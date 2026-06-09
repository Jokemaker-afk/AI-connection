using System;
using UnityEngine;

/// <summary>Runtime registry for placed-object prefabs (workstations, buildables). Loaded from Resources/Items/.</summary>
[CreateAssetMenu(
    fileName = "ItemPlacedPrefabRegistry",
    menuName = "Items/Placed Prefab Registry",
    order = 2)]
public class ItemPlacedPrefabRegistry : ScriptableObject
{
    const string DefaultResourcePath = "Items/ItemPlacedPrefabRegistry";

    [SerializeField] PlacedPrefabEntry[] entries = Array.Empty<PlacedPrefabEntry>();

    static ItemPlacedPrefabRegistry cached;

    [Serializable]
    public struct PlacedPrefabEntry
    {
        public ItemKind Kind;
        public GameObject Prefab;
    }

    public static ItemPlacedPrefabRegistry Load()
    {
        if (cached != null)
        {
            return cached;
        }

        cached = Resources.Load<ItemPlacedPrefabRegistry>(DefaultResourcePath);
        return cached;
    }

#if UNITY_EDITOR
    public static void SetCachedForTests(ItemPlacedPrefabRegistry registry)
    {
        cached = registry;
    }
#endif

    public bool TryGetPrefab(ItemKind kind, out GameObject prefab)
    {
        prefab = null;
        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i].Kind == kind && entries[i].Prefab != null)
            {
                prefab = entries[i].Prefab;
                return true;
            }
        }

        return false;
    }
}

/// <summary>Resolves placed prefabs from ItemCatalog first, then ItemPlacedPrefabRegistry.</summary>
public static class ItemPlacedPrefabResolver
{
    public static bool TryResolve(ItemKind kind, out GameObject prefab)
    {
        prefab = null;
        if (ItemCatalog.TryGet(kind, out ItemData data) && data.PlacedPrefab != null)
        {
            prefab = data.PlacedPrefab;
            return true;
        }

        ItemPlacedPrefabRegistry registry = ItemPlacedPrefabRegistry.Load();
        return registry != null && registry.TryGetPrefab(kind, out prefab) && prefab != null;
    }
}
