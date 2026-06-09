using UnityEngine;

/// <summary>Central resolver for scene module visual prefabs.</summary>
public static class SceneModuleVisualResolver
{
    public static bool TryResolveSceneModulePrefab(SceneModuleKind kind, out GameObject prefab)
    {
        prefab = null;

        if (kind == SceneModuleKind.ReturnButton)
        {
            return ReturnButtonPrefabCatalog.TryResolveReturnButtonPrefab(out prefab);
        }

        if (kind == SceneModuleKind.HealStation
            || kind == SceneModuleKind.StaminaStation
            || kind == SceneModuleKind.SpeedStation
            || kind == SceneModuleKind.DamageStation
            || kind == SceneModuleKind.DefenseStation)
        {
            return BuffStationPrefabCatalog.TryResolveBuffStationPrefab(kind, out prefab);
        }

        if (kind == SceneModuleKind.SpikeHazard || kind == SceneModuleKind.Hazard)
        {
            return HazardSpikePrefabCatalog.TryResolveVisualPrefab(out prefab);
        }

        if (kind == SceneModuleKind.SpikeTrapHazard)
        {
            return HazardSpikePrefabCatalog.TryResolveModulePrefab(out prefab);
        }

        if (kind == SceneModuleKind.ElectricField || kind == SceneModuleKind.ElectricFieldHazard)
        {
            if (kind == SceneModuleKind.ElectricFieldHazard)
            {
                return HazardElectricFieldPrefabCatalog.TryResolveModulePrefab(out prefab);
            }

            return HazardElectricFieldPrefabCatalog.TryResolveVisualPrefab(out prefab);
        }

        SceneModulePrefabRegistry registryFallback = SceneModulePrefabRegistry.Load();
        if (registryFallback == null)
        {
            return false;
        }

        return registryFallback.TryGetPrefab(kind, out prefab) && prefab != null;
    }
}
