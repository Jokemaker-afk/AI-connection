using UnityEngine;

/// <summary>Buff station visual prefab lookup: exact kind → generic BuffStation → null.</summary>
public static class BuffStationPrefabCatalog
{
    public static bool TryResolveBuffStationPrefab(SceneModuleKind kind, out GameObject prefab)
    {
        prefab = null;

        if (kind != SceneModuleKind.HealStation
            && kind != SceneModuleKind.StaminaStation
            && kind != SceneModuleKind.SpeedStation
            && kind != SceneModuleKind.DamageStation
            && kind != SceneModuleKind.DefenseStation)
        {
            return false;
        }

        SceneModulePrefabRegistry registry = SceneModulePrefabRegistry.Load();
        if (registry == null)
        {
            return false;
        }

        if (registry.TryGetPrefab(kind, out prefab) && prefab != null)
        {
            return true;
        }

        return registry.TryGetPrefab(SceneModuleKind.BuffStation, out prefab) && prefab != null;
    }

    public static bool TryResolveBuffStationPrefab(BuffType buffType, out GameObject prefab)
    {
        prefab = null;

        SceneModuleKind? kind = MapBuffTypeToModuleKind(buffType);
        if (!kind.HasValue)
        {
            return false;
        }

        return TryResolveBuffStationPrefab(kind.Value, out prefab);
    }

    public static SceneModuleKind? MapBuffTypeToModuleKind(BuffType buffType)
    {
        switch (buffType)
        {
            case BuffType.Heal:
                return SceneModuleKind.HealStation;
            case BuffType.SpeedBoost:
                return SceneModuleKind.SpeedStation;
            case BuffType.InfiniteStamina:
                return SceneModuleKind.StaminaStation;
            case BuffType.Shield:
                return SceneModuleKind.DefenseStation;
            default:
                return null;
        }
    }
}
