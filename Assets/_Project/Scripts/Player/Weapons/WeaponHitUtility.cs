using UnityEngine;

public static class WeaponHitUtility
{
    public static bool TryMeleeHit(Vector3 origin, Vector3 direction, float range, float radius, GameObject source, out IDamageable damageable, out RaycastHit hit)
    {
        damageable = null;
        hit = default;

        if (direction.sqrMagnitude < 0.001f)
        {
            return false;
        }

        direction.Normalize();
        LayerMask mask = GameplayLayers.CombatTargetMask;

        if (Physics.SphereCast(origin, radius, direction, out hit, range, mask, QueryTriggerInteraction.Ignore))
        {
            return TryResolveDamageable(hit.collider, out damageable);
        }

        Collider[] overlaps = Physics.OverlapSphere(origin + direction * Mathf.Min(range, 1.2f), radius, mask, QueryTriggerInteraction.Ignore);
        float bestDistance = float.MaxValue;
        for (int i = 0; i < overlaps.Length; i++)
        {
            if (overlaps[i] == null || !TryResolveDamageable(overlaps[i], out IDamageable candidate))
            {
                continue;
            }

            float distance = Vector3.Distance(origin, overlaps[i].transform.position);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                damageable = candidate;
            }
        }

        return damageable != null;
    }

    public static bool TryRangedHit(Ray ray, float range, GameObject source, out IDamageable damageable, out RaycastHit hit)
    {
        damageable = null;
        hit = default;

        LayerMask mask = GameplayLayers.CombatTargetMask;
        if (!Physics.Raycast(ray, out hit, range, mask, QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        return TryResolveDamageable(hit.collider, out damageable);
    }

    public static bool TryResolveDamageable(Collider collider, out IDamageable damageable)
    {
        damageable = null;
        if (collider == null)
        {
            return false;
        }

        damageable = collider.GetComponentInParent<IDamageable>();
        return damageable != null && !damageable.IsDead;
    }
}
