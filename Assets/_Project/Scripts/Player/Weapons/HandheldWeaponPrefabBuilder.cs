using System.Collections.Generic;
using UnityEngine;

public static class HandheldWeaponPrefabBuilder
{
    static readonly Dictionary<ItemKind, GameObject> PrefabCache = new Dictionary<ItemKind, GameObject>();

    public static GameObject GetOrCreatePrefab(ItemKind itemKind)
    {
        if (!ItemKindUtility.IsWeapon(itemKind))
        {
            return null;
        }

        if (PrefabCache.TryGetValue(itemKind, out GameObject cached) && cached != null)
        {
            EnsureMuzzlePointOnPrefab(cached, itemKind);
            return cached;
        }

        if (!ItemCatalog.TryGet(itemKind, out ItemData data))
        {
            return null;
        }

        GameObject prefab = BuildPrototype(itemKind, data.Weapon);
        PrefabCache[itemKind] = prefab;
        return prefab;
    }

    static GameObject BuildPrototype(ItemKind itemKind, WeaponProfile profile)
    {
        var root = new GameObject($"HandheldWeapon_{itemKind}");
        var visualRoot = new GameObject("VisualRoot");
        visualRoot.transform.SetParent(root.transform, false);

        Color bladeColor = ItemKindUtility.GetDisplayColor(itemKind);
        Color handleColor = new Color(0.4f, 0.26f, 0.14f);

        switch (profile.WeaponKind)
        {
            case WeaponKind.Melee:
                CreatePart(visualRoot.transform, "Handle", new Vector3(0f, -0.05f, 0f), new Vector3(0.07f, 0.22f, 0.07f), handleColor);
                CreatePart(visualRoot.transform, "Blade", new Vector3(0f, 0.12f, 0.02f), new Vector3(0.08f, 0.32f, 0.04f), bladeColor);
                break;
            case WeaponKind.Bow:
            case WeaponKind.Firearm:
            case WeaponKind.TrainingBlaster:
                CreatePart(visualRoot.transform, "Body", new Vector3(0f, 0f, 0.05f), new Vector3(0.14f, 0.1f, 0.28f), bladeColor);
                CreatePart(visualRoot.transform, "Barrel", new Vector3(0f, 0.02f, 0.22f), new Vector3(0.05f, 0.05f, 0.18f), bladeColor * 0.85f);
                AddMuzzlePoint(visualRoot.transform, new Vector3(0f, 0.02f, 0.32f));
                break;
            default:
                CreatePart(visualRoot.transform, "Body", Vector3.zero, new Vector3(0.16f, 0.16f, 0.16f), bladeColor);
                break;
        }

        var weaponVisual = root.AddComponent<HandheldWeaponVisual>();
        weaponVisual.Configure(itemKind, profile);
        RegisterAttackClips(legacyAnimation: root.GetComponent<Animation>() ?? root.AddComponent<Animation>(), profile);

        return root;
    }

    static void RegisterAttackClips(Animation legacyAnimation, WeaponProfile profile)
    {
        if (legacyAnimation == null)
        {
            return;
        }

        if (profile.FirstPersonAttackAnimation != null)
        {
            legacyAnimation.AddClip(profile.FirstPersonAttackAnimation, profile.FirstPersonAttackAnimation.name);
        }

        if (profile.ThirdPersonAttackAnimation != null
            && profile.ThirdPersonAttackAnimation != profile.FirstPersonAttackAnimation)
        {
            legacyAnimation.AddClip(profile.ThirdPersonAttackAnimation, profile.ThirdPersonAttackAnimation.name);
        }
    }

    static void CreatePart(Transform parent, string name, Vector3 localPos, Vector3 scale, Color color)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent, false);
        cube.transform.localPosition = localPos;
        cube.transform.localScale = scale;
        Object.Destroy(cube.GetComponent<Collider>());

        var renderer = cube.GetComponent<Renderer>();
        if (renderer != null)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = color;
            renderer.sharedMaterial = material;
        }
    }

    static void AddMuzzlePoint(Transform parent, Vector3 localPosition)
    {
        var muzzleObject = new GameObject("WeaponMuzzlePoint");
        muzzleObject.transform.SetParent(parent, false);
        muzzleObject.transform.localPosition = localPosition;
        muzzleObject.AddComponent<WeaponMuzzlePoint>();
    }

    static void EnsureMuzzlePointOnPrefab(GameObject prefab, ItemKind itemKind)
    {
        if (!ItemCatalog.TryGet(itemKind, out ItemData data) || data.Weapon.AttackMode != WeaponAttackMode.Projectile)
        {
            return;
        }

        if (prefab.GetComponentInChildren<WeaponMuzzlePoint>(true) != null)
        {
            return;
        }

        Transform visualRoot = prefab.transform.Find("VisualRoot");
        if (visualRoot == null)
        {
            return;
        }

        AddMuzzlePoint(visualRoot, new Vector3(0f, 0.02f, 0.32f));
    }
}
