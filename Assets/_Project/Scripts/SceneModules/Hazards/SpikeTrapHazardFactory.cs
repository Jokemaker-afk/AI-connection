using UnityEngine;

/// <summary>Factory for the reusable PF_Generic_Hazard_SpikeTrap_01 hazard module.</summary>
public static class SpikeTrapHazardFactory
{
    public const float DefaultDamage = 18f;
    public const int DefaultSpikeCount = 4;

    public static SpikeTrap CreateSpikeHazard(
        Transform parent,
        string name,
        Vector3 position,
        Vector3 size,
        int spikeCount = DefaultSpikeCount,
        float damageAmount = DefaultDamage,
        bool active = true)
    {
        return CreateSpikeHazard(parent, name, position, Quaternion.identity, size, spikeCount, damageAmount, active);
    }

    public static SpikeTrap CreateSpikeHazard(
        Transform parent,
        string name,
        Vector3 position,
        Quaternion rotation,
        Vector3 size,
        int spikeCount = DefaultSpikeCount,
        float damageAmount = DefaultDamage,
        bool active = true)
    {
        return SpikeTrap.Create(parent, name, position, rotation, size, spikeCount, damageAmount, active);
    }

    public static GameObject CreateSpikeHazardModule(
        Transform parent,
        string name,
        Vector3 position,
        Vector3 size,
        int spikeCount = DefaultSpikeCount,
        float damageAmount = DefaultDamage,
        bool active = true)
    {
        SpikeTrap logic = CreateSpikeHazard(parent, name, position, size, spikeCount, damageAmount, active);
        return logic != null ? logic.transform.parent.gameObject : null;
    }

    public static bool TryResolveModulePrefab(out GameObject prefab)
    {
        return HazardSpikePrefabCatalog.TryResolveModulePrefab(out prefab);
    }

    public static bool IsCompleteModuleRoot(Transform root)
    {
        return SpikeTrap.IsCompleteModuleRoot(root);
    }
}
