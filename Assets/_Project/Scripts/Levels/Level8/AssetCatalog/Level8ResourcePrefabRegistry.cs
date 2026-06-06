using System;
using UnityEngine;

/// <summary>
/// Runtime registry for Level8 resource pickup visual prefabs. Loaded from Resources/Level8/.
/// </summary>
[CreateAssetMenu(
    fileName = "Level8ResourcePrefabRegistry",
    menuName = "Level8/Resource Prefab Registry",
    order = 1)]
public class Level8ResourcePrefabRegistry : ScriptableObject
{
    const string DefaultResourcePath = "Level8/Level8ResourcePrefabRegistry";

    [SerializeField] ResourcePrefabEntry[] genericEntries = Array.Empty<ResourcePrefabEntry>();
    [SerializeField] BiomeResourcePrefabEntry[] biomeOverrides = Array.Empty<BiomeResourcePrefabEntry>();

    static Level8ResourcePrefabRegistry cached;

    [Serializable]
    public struct ResourcePrefabEntry
    {
        public ItemKind Kind;
        public GameObject Prefab;
    }

    [Serializable]
    public struct BiomeResourcePrefabEntry
    {
        public Level8BiomeKind Biome;
        public ItemKind Kind;
        public GameObject Prefab;
    }

    public static Level8ResourcePrefabRegistry Load()
    {
        if (cached != null)
        {
            return cached;
        }

        cached = Resources.Load<Level8ResourcePrefabRegistry>(DefaultResourcePath);
        return cached;
    }

#if UNITY_EDITOR
    public static void SetCachedForTests(Level8ResourcePrefabRegistry registry)
    {
        cached = registry;
    }
#endif

    public bool TryGetPrefab(Level8BiomeKind biome, ItemKind kind, out GameObject prefab)
    {
        prefab = null;

        for (int i = 0; i < biomeOverrides.Length; i++)
        {
            BiomeResourcePrefabEntry entry = biomeOverrides[i];
            if (entry.Biome == biome && entry.Kind == kind && entry.Prefab != null)
            {
                prefab = entry.Prefab;
                return true;
            }
        }

        for (int i = 0; i < genericEntries.Length; i++)
        {
            ResourcePrefabEntry entry = genericEntries[i];
            if (entry.Kind == kind && entry.Prefab != null)
            {
                prefab = entry.Prefab;
                return true;
            }
        }

        return false;
    }
}
