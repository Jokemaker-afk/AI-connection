using UnityEngine;

public static class WeaponProjectileSpawner
{
    public static WeaponProjectile Spawn(
        WeaponProfile profile,
        WeaponKind weaponKind,
        Vector3 fireOrigin,
        Vector3 fireDirection,
        GameObject source)
    {
        ProjectileKind projectileKind = profile.ProjectileKind != ProjectileKind.None
            ? profile.ProjectileKind
            : ProjectileKind.BasicBullet;

        if (!ProjectileCatalog.TryGet(projectileKind, out ProjectileData projectileData))
        {
            Debug.LogWarning($"[WeaponProjectile] Unknown projectile kind: {projectileKind}");
            return null;
        }

        GameObject projectileObject = new GameObject($"Projectile_{projectileData.DisplayNameChinese}");
        projectileObject.transform.position = fireOrigin;
        GameplayLayers.TrySetIgnoreRaycastLayer(projectileObject);

        WeaponProjectile projectile = projectileObject.AddComponent<WeaponProjectile>();
        projectile.Initialize(
            profile,
            projectileData,
            profile.Damage,
            profile.KnockbackForce,
            fireDirection,
            source);

        if (profile.MuzzleEffectPrefab != null)
        {
            Object.Instantiate(profile.MuzzleEffectPrefab, fireOrigin, Quaternion.LookRotation(fireDirection));
        }

        return projectile;
    }
}
