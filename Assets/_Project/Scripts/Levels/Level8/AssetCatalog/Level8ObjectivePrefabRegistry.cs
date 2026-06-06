using System;
using UnityEngine;

/// <summary>
/// Runtime registry for Level8 objective visual prefabs. Loaded from Resources/Level8/.
/// Assign prefabs in the asset or via Tools → Level8 → Create Data Core Test Prefab And Registry.
/// </summary>
[CreateAssetMenu(
    fileName = "Level8ObjectivePrefabRegistry",
    menuName = "Level8/Objective Prefab Registry",
    order = 0)]
public class Level8ObjectivePrefabRegistry : ScriptableObject
{
    const string DefaultResourcePath = "Level8/Level8ObjectivePrefabRegistry";

    [SerializeField] GameObject genericDataCorePrefab;
    [SerializeField] BiomeObjectivePrefabEntry[] dataCoreBiomeOverrides = Array.Empty<BiomeObjectivePrefabEntry>();

    static Level8ObjectivePrefabRegistry cached;

    [Serializable]
    public struct BiomeObjectivePrefabEntry
    {
        public Level8BiomeKind Biome;
        public GameObject Prefab;
    }

    public static Level8ObjectivePrefabRegistry Load()
    {
        if (cached != null)
        {
            return cached;
        }

        cached = Resources.Load<Level8ObjectivePrefabRegistry>(DefaultResourcePath);
        return cached;
    }

#if UNITY_EDITOR
    public static void SetCachedForTests(Level8ObjectivePrefabRegistry registry)
    {
        cached = registry;
    }
#endif

    public bool TryGetDataCorePrefab(Level8BiomeKind biome, out GameObject prefab)
    {
        prefab = null;

        for (int i = 0; i < dataCoreBiomeOverrides.Length; i++)
        {
            BiomeObjectivePrefabEntry entry = dataCoreBiomeOverrides[i];
            if (entry.Biome == biome && entry.Prefab != null)
            {
                prefab = entry.Prefab;
                return true;
            }
        }

        if (genericDataCorePrefab != null)
        {
            prefab = genericDataCorePrefab;
            return true;
        }

        return false;
    }

    public GameObject GenericDataCorePrefab => genericDataCorePrefab;
}
