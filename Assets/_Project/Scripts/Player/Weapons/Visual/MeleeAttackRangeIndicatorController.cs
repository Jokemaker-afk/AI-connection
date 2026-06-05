using UnityEngine;

/// <summary>
/// Keeps melee ground fan synced with equipped weapon and crosshair aim direction.
/// </summary>
[DisallowMultipleComponent]
[DefaultExecutionOrder(100)]
public class MeleeAttackRangeIndicatorController : MonoBehaviour
{
    [SerializeField] PlayerWeaponController weaponController;
    [SerializeField] PlayerMeleeAttackOrigin meleeAttackOrigin;
    [SerializeField] AimReferenceProvider aimReference;
    [SerializeField] bool showOnlyWhenMeleeWeaponEquipped = true;

    MeleeAttackRangeIndicator indicator;

    void Awake()
    {
        if (weaponController == null)
        {
            weaponController = GetComponent<PlayerWeaponController>();
        }

        if (meleeAttackOrigin == null)
        {
            meleeAttackOrigin = GetComponentInChildren<PlayerMeleeAttackOrigin>(true);
        }

        if (aimReference == null)
        {
            aimReference = GetComponent<AimReferenceProvider>();
        }

        GameObject indicatorObject = new GameObject("MeleeAttackRangeIndicator");
        indicatorObject.transform.SetParent(transform, false);
        indicator = indicatorObject.AddComponent<MeleeAttackRangeIndicator>();
    }

    void LateUpdate()
    {
        if (indicator == null || weaponController == null)
        {
            return;
        }

        if (!weaponController.HasWeaponEquipped || !weaponController.CurrentWeaponProfile.IsMelee)
        {
            if (showOnlyWhenMeleeWeaponEquipped)
            {
                indicator.Hide("non-melee weapon");
            }

            return;
        }

        if (aimReference == null)
        {
            aimReference = GetComponent<AimReferenceProvider>();
        }

        WeaponProfile profile = weaponController.CurrentWeaponProfile;
        indicator.ShowFromWeapon(profile, weaponController.EquippedWeaponLabel);

        Vector3 origin = meleeAttackOrigin != null
            ? meleeAttackOrigin.WorldPosition
            : transform.position + GetFlatAimDirection() * 0.5f;

        Vector3 fanForward = GetFlatAimDirection();
        indicator.UpdatePlacement(origin, fanForward);
    }

    Vector3 GetFlatAimDirection()
    {
        if (aimReference != null && !aimReference.IsWorldAimingBlocked)
        {
            return aimReference.GetFlatAimDirection();
        }

        if (meleeAttackOrigin != null)
        {
            return meleeAttackOrigin.AimForward;
        }

        if (AimReferenceProvider.TryGetFlatAim(out Vector3 flatAim))
        {
            return flatAim;
        }

        return transform.forward;
    }
}
