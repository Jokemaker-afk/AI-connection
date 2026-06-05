using UnityEngine;

/// <summary>
/// Validates whether a crosshair-selected enemy is inside the player melee attack volume.
/// Does not select targets — only validates an existing crosshair target.
/// </summary>
public static class MeleeHitValidator
{
    public const float DefaultVerticalTolerance = 1.5f;

    public static MeleeHitValidationResult Validate(
        WeaponProfile profile,
        Vector3 attackOrigin,
        Vector3 aimForward,
        WeaponTargetCandidate target,
        float verticalTolerance = DefaultVerticalTolerance)
    {
        var result = new MeleeHitValidationResult
        {
            HasTarget = target.Enemy != null && target.Collider != null,
            MaxRange = profile.AttackRange,
            MaxAngle = profile.AttackAngle * 0.5f,
        };

        if (!result.HasTarget)
        {
            result.MissReason = "no target";
            return result;
        }

        Vector3 aimPoint = target.Collider.ClosestPoint(attackOrigin);
        Vector3 toTarget = aimPoint - attackOrigin;

        float verticalDelta = Mathf.Abs(toTarget.y);
        Vector3 flatToTarget = toTarget;
        flatToTarget.y = 0f;
        float distance = flatToTarget.magnitude;

        Vector3 flatAim = aimForward;
        flatAim.y = 0f;
        if (flatAim.sqrMagnitude < 0.001f)
        {
            flatAim = Vector3.forward;
        }
        else
        {
            flatAim.Normalize();
        }

        float angle = flatToTarget.sqrMagnitude > 0.001f
            ? Vector3.Angle(flatAim, flatToTarget.normalized)
            : 0f;

        result.Distance = distance;
        result.Angle = angle;
        result.InRange = distance <= profile.AttackRange;
        result.InAngle = angle <= result.MaxAngle;
        result.InVertical = verticalDelta <= verticalTolerance;
        result.IsValid = result.InRange && result.InAngle && result.InVertical;

        if (!result.InRange)
        {
            result.MissReason = "out of range";
        }
        else if (!result.InAngle)
        {
            result.MissReason = "out of angle";
        }
        else if (!result.InVertical)
        {
            result.MissReason = "out of vertical tolerance";
        }

        return result;
    }
}
