using System.Collections.Generic;
using UnityEngine;

public static class WeaponTargetingSystem
{
    /// <summary>Strict crosshair-only weapon target. No cone, spherecast, or screen assist.</summary>
    public static WeaponTargetingResult ResolveStrictCrosshairTarget(
        WeaponProfile profile,
        Ray crosshairRay,
        Vector3 meleeAttackOrigin,
        Vector3 aimForward)
    {
        var result = new WeaponTargetingResult();
        LayerMask mask = GameplayLayers.CombatTargetMask;
        float maxDistance = Mathf.Max(profile.AttackRange, 20f);

        if (!Physics.Raycast(crosshairRay, out RaycastHit hit, maxDistance, mask, QueryTriggerInteraction.Ignore)
            || !EnemyTargetingUtility.TryResolveEnemyFromCollider(
                hit.collider,
                out EnemyController enemy,
                out Collider primaryCollider,
                out IDamageable damageable)
            || damageable.IsDead)
        {
            return result;
        }

        var candidate = new WeaponTargetCandidate
        {
            Damageable = enemy,
            Enemy = enemy,
            Collider = primaryCollider,
            HitPoint = hit.point,
            IsDirectRayHit = true,
            Distance = Vector3.Distance(crosshairRay.origin, hit.point),
        };

        result.Primary = candidate;
        result.CandidatesInArea = new[] { candidate };

        bool canDamage;
        if (profile.AttackMode == WeaponAttackMode.Projectile)
        {
            canDamage = false;
        }
        else if (profile.IsRanged || profile.AttackMode == WeaponAttackMode.Hitscan)
        {
            canDamage = Vector3.Distance(meleeAttackOrigin, hit.point) <= profile.AttackRange + 0.5f;
        }
        else
        {
            canDamage = MeleeHitValidator.Validate(profile, meleeAttackOrigin, aimForward, candidate).IsValid;
        }

        if (canDamage)
        {
            result.DamagedTargets = new[] { candidate };
        }

        return result;
    }

    public static WeaponTargetingResult ResolveTargets(
        WeaponProfile profile,
        Vector3 origin,
        Vector3 aimForward,
        Ray crosshairRay,
        GameObject source,
        bool applyDamageSelection = true)
    {
        if (profile.IsRanged || profile.AttackMode == WeaponAttackMode.Hitscan || profile.AttackMode == WeaponAttackMode.Projectile)
        {
            return ResolveRangedTargets(profile, crosshairRay, source, applyDamageSelection);
        }

        return ResolveMeleeTargets(profile, origin, aimForward, crosshairRay, source, applyDamageSelection);
    }

    static WeaponTargetingResult ResolveMeleeTargets(
        WeaponProfile profile,
        Vector3 origin,
        Vector3 aimForward,
        Ray crosshairRay,
        GameObject source,
        bool applyDamageSelection)
    {
        var result = new WeaponTargetingResult();
        if (aimForward.sqrMagnitude < 0.001f)
        {
            return result;
        }

        aimForward.Normalize();
        float range = Mathf.Max(0.5f, profile.AttackRange);
        float halfAngle = Mathf.Clamp(profile.AttackAngle, 5f, 120f) * 0.5f;
        LayerMask mask = GameplayLayers.CombatTargetMask;

        var candidates = new List<WeaponTargetCandidate>();
        EnemyController[] enemies = Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        Vector2 screenCenter = crosshairRay.direction.sqrMagnitude > 0.001f
            ? GetScreenPoint(crosshairRay)
            : new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        WeaponTargetCandidate directHit = default;
        if (Physics.Raycast(crosshairRay, out RaycastHit rayHit, range, mask, QueryTriggerInteraction.Ignore)
            && WeaponHitUtility.TryResolveDamageable(rayHit.collider, out IDamageable directDamageable)
            && directDamageable is MonoBehaviour directMono)
        {
            directHit = BuildCandidate(directDamageable, directMono, rayHit.collider, rayHit.point, origin, aimForward, screenCenter, true);
        }

        for (int i = 0; i < enemies.Length; i++)
        {
            EnemyController enemy = enemies[i];
            if (enemy == null || enemy.IsDead)
            {
                continue;
            }

            Collider collider = enemy.GetComponent<Collider>();
            if (collider == null)
            {
                continue;
            }

            Vector3 toEnemy = collider.bounds.center - origin;
            float distance = toEnemy.magnitude;
            if (distance > range + collider.bounds.extents.magnitude)
            {
                continue;
            }

            Vector3 flatToEnemy = toEnemy;
            flatToEnemy.y = 0f;
            if (flatToEnemy.sqrMagnitude < 0.001f)
            {
                continue;
            }

            float angle = Vector3.Angle(aimForward, flatToEnemy.normalized);
            if (angle > halfAngle)
            {
                continue;
            }

            if (!HasLineOfSight(origin, collider.bounds.center, collider, source))
            {
                continue;
            }

            bool isDirect = directHit.Collider == collider;
            WeaponTargetCandidate candidate = BuildCandidate(
                enemy,
                enemy,
                collider,
                collider.bounds.center,
                origin,
                aimForward,
                screenCenter,
                isDirect);
            candidates.Add(candidate);
        }

        if (directHit.Damageable != null && !ContainsCandidate(candidates, directHit.Collider))
        {
            candidates.Insert(0, directHit);
        }

        candidates.Sort(CompareCandidates);
        result.CandidatesInArea = candidates.ToArray();

        if (candidates.Count == 0)
        {
            return result;
        }

        result.Primary = candidates[0];

        if (!applyDamageSelection)
        {
            return result;
        }

        int maxTargets = ResolveMaxTargets(profile);
        int count = Mathf.Min(maxTargets, candidates.Count);
        var damaged = new WeaponTargetCandidate[count];
        for (int i = 0; i < count; i++)
        {
            damaged[i] = candidates[i];
        }

        result.DamagedTargets = damaged;
        return result;
    }

    static WeaponTargetingResult ResolveRangedTargets(
        WeaponProfile profile,
        Ray crosshairRay,
        GameObject source,
        bool applyDamageSelection)
    {
        var result = new WeaponTargetingResult();
        float range = Mathf.Max(2f, profile.AttackRange);
        LayerMask mask = GameplayLayers.CombatTargetMask;

        if (!Physics.Raycast(crosshairRay, out RaycastHit hit, range, mask, QueryTriggerInteraction.Ignore)
            || !WeaponHitUtility.TryResolveDamageable(hit.collider, out IDamageable damageable)
            || damageable is not MonoBehaviour mono)
        {
            return result;
        }

        Vector3 origin = crosshairRay.origin;
        Vector3 aimForward = crosshairRay.direction;
        Vector2 screenCenter = GetScreenPoint(crosshairRay);
        WeaponTargetCandidate candidate = BuildCandidate(damageable, mono, hit.collider, hit.point, origin, aimForward, screenCenter, true);
        result.Primary = candidate;
        result.CandidatesInArea = new[] { candidate };

        if (applyDamageSelection)
        {
            result.DamagedTargets = new[] { candidate };
        }

        return result;
    }

    static bool ContainsCandidate(List<WeaponTargetCandidate> candidates, Collider collider)
    {
        if (collider == null)
        {
            return false;
        }

        for (int i = 0; i < candidates.Count; i++)
        {
            if (candidates[i].Collider == collider)
            {
                return true;
            }
        }

        return false;
    }

    static int ResolveMaxTargets(WeaponProfile profile)
    {
        if (profile.TargetMode == WeaponTargetMode.SingleTarget || profile.TargetMode == WeaponTargetMode.Ray)
        {
            return 1;
        }

        return Mathf.Max(1, profile.MaxTargets);
    }

    static WeaponTargetCandidate BuildCandidate(
        IDamageable damageable,
        MonoBehaviour mono,
        Collider collider,
        Vector3 hitPoint,
        Vector3 origin,
        Vector3 aimForward,
        Vector2 screenCenter,
        bool isDirectRayHit)
    {
        EnemyController enemy = mono as EnemyController ?? mono.GetComponent<EnemyController>();
        Vector3 toTarget = hitPoint - origin;
        float angle = toTarget.sqrMagnitude > 0.001f
            ? Vector3.Angle(aimForward, toTarget.normalized)
            : 0f;
        float distance = toTarget.magnitude;
        float screenDistance = GetScreenDistance(collider, screenCenter);

        return new WeaponTargetCandidate
        {
            Damageable = damageable,
            Enemy = enemy,
            Collider = collider,
            HitPoint = hitPoint,
            AngleDegrees = angle,
            Distance = distance,
            ScreenDistance = screenDistance,
            IsDirectRayHit = isDirectRayHit,
        };
    }

    static int CompareCandidates(WeaponTargetCandidate a, WeaponTargetCandidate b)
    {
        if (a.IsDirectRayHit != b.IsDirectRayHit)
        {
            return a.IsDirectRayHit ? -1 : 1;
        }

        int screenCompare = a.ScreenDistance.CompareTo(b.ScreenDistance);
        if (screenCompare != 0)
        {
            return screenCompare;
        }

        int angleCompare = a.AngleDegrees.CompareTo(b.AngleDegrees);
        if (angleCompare != 0)
        {
            return angleCompare;
        }

        return a.Distance.CompareTo(b.Distance);
    }

    static Vector2 GetScreenPoint(Ray ray)
    {
        Camera camera = CrosshairRayUtility.ResolveGameplayCamera();
        if (camera == null)
        {
            return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        }

        Vector3 far = ray.GetPoint(50f);
        return camera.WorldToScreenPoint(far);
    }

    static float GetScreenDistance(Collider collider, Vector2 screenCenter)
    {
        Camera camera = CrosshairRayUtility.ResolveGameplayCamera();
        if (camera == null || collider == null)
        {
            return float.MaxValue;
        }

        Vector3 screenPoint = camera.WorldToScreenPoint(collider.bounds.center);
        return Vector2.Distance(screenCenter, new Vector2(screenPoint.x, screenPoint.y));
    }

    static bool HasLineOfSight(Vector3 origin, Vector3 targetPoint, Collider targetCollider, GameObject source)
    {
        Vector3 direction = targetPoint - origin;
        float distance = direction.magnitude;
        if (distance <= 0.01f)
        {
            return true;
        }

        if (!Physics.Raycast(origin, direction.normalized, out RaycastHit hit, distance, GameplayLayers.PickupLineOfSightObstacleMask, QueryTriggerInteraction.Ignore))
        {
            return true;
        }

        return hit.collider == targetCollider || hit.collider.transform.IsChildOf(targetCollider.transform);
    }
}
