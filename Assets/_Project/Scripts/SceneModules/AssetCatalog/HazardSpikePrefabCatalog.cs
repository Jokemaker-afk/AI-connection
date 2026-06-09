using UnityEngine;

/// <summary>Spike trap module + visual prefab lookup.</summary>
public static class HazardSpikePrefabCatalog
{
    public static bool TryResolveModulePrefab(out GameObject prefab)
    {
        prefab = null;

        SceneModulePrefabRegistry registry = SceneModulePrefabRegistry.Load();
        if (registry == null)
        {
            return false;
        }

        if (registry.TryGetPrefab(SceneModuleKind.SpikeTrapHazard, out prefab) && prefab != null)
        {
            return true;
        }

        if (registry.TryGetPrefab(SceneModuleKind.SpikeHazard, out prefab) && prefab != null
            && prefab.transform.Find(SceneModuleVisualUtility.LogicRootName) != null)
        {
            return true;
        }

        return registry.TryGetPrefab(SceneModuleKind.Hazard, out prefab)
            && prefab != null
            && prefab.transform.Find(SceneModuleVisualUtility.LogicRootName) != null;
    }

    public static bool TryResolveVisualPrefab(out GameObject prefab)
    {
        prefab = null;

        SceneModulePrefabRegistry registry = SceneModulePrefabRegistry.Load();
        if (registry == null)
        {
            return false;
        }

        if (registry.TryGetPrefab(SceneModuleKind.SpikeHazard, out prefab) && prefab != null)
        {
            if (prefab.transform.Find(SceneModuleVisualUtility.LogicRootName) == null)
            {
                return true;
            }
        }

        if (registry.TryGetPrefab(SceneModuleKind.Hazard, out prefab) && prefab != null)
        {
            if (prefab.transform.Find(SceneModuleVisualUtility.LogicRootName) == null)
            {
                return true;
            }
        }

        if (TryResolveModulePrefab(out GameObject modulePrefab) && modulePrefab != null)
        {
            Transform visualRoot = modulePrefab.transform.Find(SceneModuleVisualUtility.VisualRootName);
            if (visualRoot != null && visualRoot.childCount > 0)
            {
                prefab = visualRoot.GetChild(0).gameObject;
                return prefab != null;
            }
        }

        prefab = null;
        return false;
    }

    /// <summary>Backward-compatible visual lookup (SpikeHazard → Hazard → module visual child).</summary>
    public static bool TryResolveSpikeHazardPrefab(out GameObject prefab)
    {
        return TryResolveVisualPrefab(out prefab);
    }
}
