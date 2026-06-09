using UnityEngine;

/// <summary>Electric field module + visual prefab lookup.</summary>
public static class HazardElectricFieldPrefabCatalog
{
    public static bool TryResolveModulePrefab(out GameObject prefab)
    {
        prefab = null;

        SceneModulePrefabRegistry registry = SceneModulePrefabRegistry.Load();
        if (registry == null)
        {
            return false;
        }

        if (registry.TryGetPrefab(SceneModuleKind.ElectricFieldHazard, out prefab) && prefab != null)
        {
            return true;
        }

        return registry.TryGetPrefab(SceneModuleKind.ElectricField, out prefab)
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

        if (registry.TryGetPrefab(SceneModuleKind.ElectricField, out prefab) && prefab != null)
        {
            if (prefab.transform.Find(SceneModuleVisualUtility.LogicRootName) == null)
            {
                return true;
            }
        }

        if (registry.TryGetPrefab(SceneModuleKind.ElectricFieldHazard, out GameObject modulePrefab)
            && modulePrefab != null)
        {
            Transform visual = SceneModuleVisualUtility.ResolveHazardVisualContainer(modulePrefab.transform);
            if (visual != null && visual.GetComponentInChildren<Renderer>(true) != null)
            {
                if (registry.TryGetPrefab(SceneModuleKind.ElectricField, out GameObject registeredVisual)
                    && registeredVisual != null
                    && registeredVisual.transform.Find(SceneModuleVisualUtility.LogicRootName) == null)
                {
                    prefab = registeredVisual;
                    return true;
                }
            }
        }

        prefab = null;
        return false;
    }
}
