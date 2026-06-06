using UnityEngine;

/// <summary>
/// Objective interactable visual prefabs (Data Core, Signal Relay, Exit, …).
/// Lookup: biome-specific → generic → null (caller uses primitive fallback).
/// </summary>
public static class Level8ObjectivePrefabCatalog
{
    public static bool TryGet(Level8ObjectiveKind kind, out GameObject prefab)
    {
        return TryGet(kind, Level8BiomeKind.Plain, out prefab);
    }

    public static bool TryGet(Level8ObjectiveKind kind, Level8BiomeKind biome, out GameObject prefab)
    {
        prefab = null;

        switch (kind)
        {
            case Level8ObjectiveKind.DataCore:
                return TryResolveDataCorePrefab(biome, out prefab);
            default:
                // TODO Phase B+: SignalRelay, LevelExitPortal
                return false;
        }
    }

    public static bool TryResolveDataCorePrefab(Level8BiomeKind biome, out GameObject prefab)
    {
        prefab = null;

        Level8ObjectivePrefabRegistry registry = Level8ObjectivePrefabRegistry.Load();
        if (registry == null)
        {
            return false;
        }

        return registry.TryGetDataCorePrefab(biome, out prefab) && prefab != null;
    }
}
