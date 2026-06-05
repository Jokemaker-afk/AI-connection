using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Modular projectile: manual movement with sphere-cast sweep. Damage only on physical hit.
/// </summary>
[DisallowMultipleComponent]
public class WeaponProjectile : MonoBehaviour
{
    [SerializeField] bool enableDebugLogs = true;

    float damage;
    float knockbackForce;
    float speed;
    float maxLifetime;
    float maxDistance;
    float radius;
    bool destroyOnHit;
    int remainingPierceCount;
    bool useGravity;
    float gravityScale;

    GameObject source;
    Vector3 direction;
    Vector3 previousPosition;
    Vector3 startPosition;
    float spawnTime;

    WeaponProfile weaponProfile;
    readonly HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();

    LayerMask enemyMask;
    LayerMask obstacleMask;

    Transform visualTransform;

    public void Initialize(
        WeaponProfile profile,
        ProjectileData projectileData,
        float weaponDamage,
        float weaponKnockback,
        Vector3 fireDirection,
        GameObject sourceObject)
    {
        weaponProfile = profile;
        damage = weaponDamage;
        knockbackForce = weaponKnockback;
        speed = profile.ProjectileSpeed > 0f ? profile.ProjectileSpeed : projectileData.Speed;
        maxLifetime = profile.ProjectileMaxLifetime > 0f ? profile.ProjectileMaxLifetime : projectileData.MaxLifetime;
        maxDistance = profile.ProjectileMaxDistance > 0f ? profile.ProjectileMaxDistance : projectileData.MaxDistance;
        radius = profile.ProjectileRadius > 0f ? profile.ProjectileRadius : projectileData.Radius;
        destroyOnHit = profile.ProjectileDestroyOnHit || projectileData.DestroyOnHit;
        remainingPierceCount = profile.ProjectileCanPierce || projectileData.CanPierce
            ? Mathf.Max(profile.ProjectileMaxPierceCount, projectileData.MaxPierceCount)
            : 0;
        useGravity = profile.ProjectileUseGravity || projectileData.UseGravity;
        gravityScale = profile.ProjectileGravityScale > 0f ? profile.ProjectileGravityScale : projectileData.GravityScale;

        source = sourceObject;
        direction = fireDirection.sqrMagnitude > 0.001f ? fireDirection.normalized : transform.forward;
        previousPosition = transform.position;
        startPosition = transform.position;
        spawnTime = Time.time;

        enemyMask = GameplayLayers.CombatTargetMask;
        obstacleMask = GameplayLayers.PickupLineOfSightObstacleMask;

        EnsureVisual(projectileData.VisualColor);
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);

        if (enableDebugLogs)
        {
            Debug.Log($"[WeaponProjectile] Projectile spawned: {projectileData.DisplayNameChinese}");
            Debug.Log($"[WeaponProjectile] Projectile direction: {direction}");
        }
    }

    void Update()
    {
        float deltaTime = Time.deltaTime;
        if (useGravity)
        {
            direction += Physics.gravity * gravityScale * deltaTime;
            direction = direction.normalized;
        }

        Vector3 nextPosition = transform.position + direction * speed * deltaTime;
        float stepDistance = Vector3.Distance(previousPosition, nextPosition);
        if (stepDistance > 0.0001f)
        {
            if (Physics.SphereCast(previousPosition, radius, direction, out RaycastHit hit, stepDistance, enemyMask | obstacleMask, QueryTriggerInteraction.Collide))
            {
                if (TryResolveEnemyHit(hit))
                {
                    return;
                }

                if (IsObstacleHit(hit))
                {
                    OnObstacleHit(hit.point, hit.normal);
                    return;
                }
            }
        }

        transform.position = nextPosition;
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        previousPosition = nextPosition;

        if (Time.time - spawnTime >= maxLifetime)
        {
            if (enableDebugLogs)
            {
                Debug.Log("[WeaponProjectile] Projectile expired by lifetime");
            }

            Destroy(gameObject);
            return;
        }

        if (Vector3.Distance(startPosition, transform.position) >= maxDistance)
        {
            if (enableDebugLogs)
            {
                Debug.Log("[WeaponProjectile] Projectile expired by max distance");
            }

            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (enableDebugLogs && Time.time - spawnTime >= maxLifetime * 0.95f && damagedTargets.Count == 0)
        {
            Debug.Log("[WeaponProjectile] Projectile missed: no collision");
        }
    }

    bool TryResolveEnemyHit(RaycastHit hit)
    {
        if (!GameplayLayers.IsInLayerMask(hit.collider.gameObject.layer, enemyMask))
        {
            return false;
        }

        if (source != null && hit.collider.transform.IsChildOf(source.transform))
        {
            return false;
        }

        IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
        if (damageable == null)
        {
            return false;
        }

        if (damagedTargets.Contains(damageable))
        {
            return true;
        }

        damagedTargets.Add(damageable);

        string enemyName = hit.collider.transform.root.name;
        if (enableDebugLogs)
        {
            Debug.Log($"[WeaponProjectile] Projectile hit enemy: {enemyName}");
        }

        DamageInfo info = DamageInfo.Create(damage, hit.point, direction, knockbackForce, source);
        damageable.TakeDamage(info);

        if (enableDebugLogs)
        {
            Debug.Log($"[WeaponProjectile] Projectile damage applied: {damage}");
        }

        WeaponHitEffectUtility.SpawnHitEffect(weaponProfile, hit.point, hit.normal);

        if (destroyOnHit && remainingPierceCount <= 0)
        {
            Destroy(gameObject);
            return true;
        }

        if (remainingPierceCount > 0)
        {
            remainingPierceCount--;
            previousPosition = hit.point + direction * (radius + 0.02f);
            transform.position = previousPosition;
        }

        return destroyOnHit;
    }

    bool IsObstacleHit(RaycastHit hit)
    {
        if (source != null && hit.collider.transform.IsChildOf(source.transform))
        {
            return false;
        }

        return GameplayLayers.IsInLayerMask(hit.collider.gameObject.layer, obstacleMask);
    }

    void OnObstacleHit(Vector3 point, Vector3 normal)
    {
        if (enableDebugLogs)
        {
            Debug.Log("[WeaponProjectile] Projectile hit obstacle");
        }

        WeaponHitEffectUtility.SpawnHitEffect(weaponProfile, point, normal);
        Destroy(gameObject);
    }

    void EnsureVisual(Color color)
    {
        if (visualTransform != null)
        {
            return;
        }

        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visual.name = "ProjectileVisual";
        visual.transform.SetParent(transform, false);
        visual.transform.localScale = Vector3.one * radius * 2.5f;
        Object.Destroy(visual.GetComponent<Collider>());
        visualTransform = visual.transform;

        Renderer renderer = visual.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = color;
            renderer.sharedMaterial = material;
        }

        GameplayLayers.TrySetIgnoreRaycastLayer(visual);
    }
}
