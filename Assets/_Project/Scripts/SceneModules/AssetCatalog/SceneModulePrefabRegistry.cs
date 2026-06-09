using System;
using UnityEngine;

/// <summary>
/// Runtime registry for scene module visual prefabs. Loaded from Resources/SceneModules/.
/// </summary>
[CreateAssetMenu(
    fileName = "SceneModulePrefabRegistry",
    menuName = "SceneModules/Prefab Registry",
    order = 0)]
public class SceneModulePrefabRegistry : ScriptableObject
{
    const string DefaultResourcePath = "SceneModules/SceneModulePrefabRegistry";

    [SerializeField] ModulePrefabEntry[] entries = Array.Empty<ModulePrefabEntry>();

    static SceneModulePrefabRegistry cached;

    [Serializable]
    public struct ModulePrefabEntry
    {
        public SceneModuleKind Kind;
        public GameObject Prefab;
    }

    public static SceneModulePrefabRegistry Load()
    {
        if (cached != null)
        {
            return cached;
        }

        cached = Resources.Load<SceneModulePrefabRegistry>(DefaultResourcePath);
        return cached;
    }

#if UNITY_EDITOR
    public static void SetCachedForTests(SceneModulePrefabRegistry registry)
    {
        cached = registry;
    }
#endif

    public bool TryGetPrefab(SceneModuleKind kind, out GameObject prefab)
    {
        prefab = null;

        for (int i = 0; i < entries.Length; i++)
        {
            ModulePrefabEntry entry = entries[i];
            if (entry.Kind == kind && entry.Prefab != null)
            {
                prefab = entry.Prefab;
                return true;
            }
        }

        return false;
    }
}
