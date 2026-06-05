using UnityEngine;

public static class WeaponMuzzleUtility
{
    public static Vector3 ResolveFireOrigin(PlayerHeldItemController heldItemController, Ray crosshairRay)
    {
        if (heldItemController?.ActiveHeldTransform != null)
        {
            WeaponMuzzlePoint muzzle = heldItemController.ActiveHeldTransform.GetComponentInChildren<WeaponMuzzlePoint>(true);
            if (muzzle != null)
            {
                return muzzle.transform.position;
            }

            return heldItemController.ActiveHeldTransform.position + heldItemController.ActiveHeldTransform.forward * 0.25f;
        }

        Camera camera = CrosshairRayUtility.ResolveGameplayCamera();
        if (camera != null)
        {
            Ray ray = CrosshairRayUtility.GetCrosshairRay(out _);
            Debug.Log("[WeaponProjectile] Projectile spawn fallback: crosshair ray offset");
            return ray.origin + ray.direction * 0.45f;
        }

        Debug.Log("[WeaponProjectile] Projectile spawn fallback: crosshair ray origin");
        return crosshairRay.origin + crosshairRay.direction * 0.5f;
    }

    public static Vector3 ResolveFireDirection(Vector3 fireOrigin, float aimDistance)
    {
        Vector3 targetPoint;
        if (AimReferenceProvider.Instance != null && !AimReferenceProvider.Instance.IsWorldAimingBlocked)
        {
            targetPoint = AimReferenceProvider.Instance.GetAimPoint(aimDistance);
        }
        else
        {
            Ray crosshairRay = CrosshairRayUtility.GetCrosshairRay(out _);
            targetPoint = crosshairRay.origin + crosshairRay.direction * Mathf.Max(8f, aimDistance);
        }

        Vector3 direction = targetPoint - fireOrigin;
        if (direction.sqrMagnitude < 0.001f)
        {
            if (AimReferenceProvider.TryGetFlatAim(out Vector3 flatAim))
            {
                direction = flatAim;
            }
            else
            {
                direction = CrosshairRayUtility.GetCrosshairRay(out _).direction;
            }
        }

        if (AimReferenceProvider.Instance != null && AimReferenceProvider.Instance.ShowAimReferenceDebug)
        {
            Debug.Log($"[AimReference] Projectile direction: {direction.normalized}");
        }

        return direction.normalized;
    }

    public static Vector3 ResolveFireDirection(Vector3 fireOrigin, Ray crosshairRay, float aimDistance)
    {
        return ResolveFireDirection(fireOrigin, aimDistance);
    }
}
