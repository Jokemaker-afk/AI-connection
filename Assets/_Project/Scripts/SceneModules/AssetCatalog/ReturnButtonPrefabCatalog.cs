using UnityEngine;

/// <summary>ReturnButton visual prefab lookup: ReturnButton → generic InteractionButton → null.</summary>
public static class ReturnButtonPrefabCatalog
{
    public static bool TryResolveReturnButtonPrefab(out GameObject prefab)
    {
        prefab = null;

        SceneModulePrefabRegistry registry = SceneModulePrefabRegistry.Load();
        if (registry == null)
        {
            return false;
        }

        if (registry.TryGetPrefab(SceneModuleKind.ReturnButton, out prefab) && prefab != null)
        {
            return true;
        }

        return registry.TryGetPrefab(SceneModuleKind.InteractionButton, out prefab) && prefab != null;
    }
}
