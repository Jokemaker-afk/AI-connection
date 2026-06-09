using System;
using UnityEngine;

/// <summary>Runtime registry for handheld tool prefabs. Loaded from Resources/Items/.</summary>
[CreateAssetMenu(
    fileName = "ItemHandheldPrefabRegistry",
    menuName = "Items/Handheld Prefab Registry",
    order = 3)]
public class ItemHandheldPrefabRegistry : ScriptableObject
{
    const string DefaultResourcePath = "Items/ItemHandheldPrefabRegistry";

    [SerializeField] HandheldPrefabEntry[] entries = Array.Empty<HandheldPrefabEntry>();

    static ItemHandheldPrefabRegistry cached;

    [Serializable]
    public struct HandheldPrefabEntry
    {
        public ItemKind Kind;
        public GameObject Prefab;
    }

    public static ItemHandheldPrefabRegistry Load()
    {
        if (cached != null)
        {
            return cached;
        }

        cached = Resources.Load<ItemHandheldPrefabRegistry>(DefaultResourcePath);
        return cached;
    }

#if UNITY_EDITOR
    public static void SetCachedForTests(ItemHandheldPrefabRegistry registry)
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

/// <summary>Resolves handheld prefabs from ItemCatalog HandheldToolProfile first, then registry.</summary>
public static class ItemHandheldPrefabResolver
{
    public static bool TryResolve(ItemKind kind, out GameObject prefab)
    {
        prefab = null;
        if (ItemCatalog.TryGet(kind, out ItemData data)
            && data.HandheldTool.HandheldPrefab != null)
        {
            prefab = data.HandheldTool.HandheldPrefab;
            return true;
        }

        ItemHandheldPrefabRegistry registry = ItemHandheldPrefabRegistry.Load();
        return registry != null && registry.TryGetPrefab(kind, out prefab) && prefab != null;
    }
}
