using UnityEngine;

/// <summary>Factory for the reusable PF_Generic_Hazard_ElectricField_01 hazard module.</summary>
public static class ElectricFieldHazardFactory
{
    public const float DefaultDamage = 12f;

    public static LaserHazard CreateElectricField(
        Transform parent,
        string name,
        Vector3 position,
        Vector3 size,
        float damage = DefaultDamage,
        bool active = true)
    {
        return CreateElectricField(parent, name, position, Quaternion.identity, size, damage, active);
    }

    public static LaserHazard CreateElectricField(
        Transform parent,
        string name,
        Vector3 position,
        Quaternion rotation,
        Vector3 size,
        float damage = DefaultDamage,
        bool active = true)
    {
        return LaserHazard.Create(parent, name, position, rotation, size, damage, active);
    }

    public static GameObject CreateElectricFieldModule(
        Transform parent,
        string name,
        Vector3 position,
        Vector3 size,
        float damage = DefaultDamage,
        bool active = true)
    {
        LaserHazard logic = CreateElectricField(parent, name, position, size, damage, active);
        return logic != null ? logic.transform.parent.gameObject : null;
    }

    public static GameObject CreateElectricFieldModule(
        Transform parent,
        string name,
        Vector3 position,
        Quaternion rotation,
        Vector3 size,
        float damage = DefaultDamage,
        bool active = true)
    {
        LaserHazard logic = CreateElectricField(parent, name, position, rotation, size, damage, active);
        return logic != null ? logic.transform.parent.gameObject : null;
    }

    public static bool TryResolveModulePrefab(out GameObject prefab)
    {
        return HazardElectricFieldPrefabCatalog.TryResolveModulePrefab(out prefab);
    }

    public static bool IsCompleteModuleRoot(Transform root)
    {
        return LaserHazard.IsCompleteModuleRoot(root);
    }
}
