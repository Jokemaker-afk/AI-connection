using UnityEngine;

/// <summary>
/// Draws crosshair aim rays and compares fan/attack/facing directions when debug is enabled.
/// </summary>
[DisallowMultipleComponent]
public class AimReferenceDebug : MonoBehaviour
{
    [SerializeField] AimReferenceProvider aimReference;
    [SerializeField] PlayerController playerController;
    [SerializeField] PlayerMeleeAttackOrigin meleeAttackOrigin;
    [SerializeField] MeleeAttackRangeIndicatorController meleeIndicatorController;
    [SerializeField] float rayDrawDistance = 12f;

    void Awake()
    {
        if (aimReference == null)
        {
            aimReference = GetComponent<AimReferenceProvider>();
        }

        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
        }

        if (meleeAttackOrigin == null)
        {
            meleeAttackOrigin = GetComponentInChildren<PlayerMeleeAttackOrigin>(true);
        }

        if (meleeIndicatorController == null)
        {
            meleeIndicatorController = GetComponent<MeleeAttackRangeIndicatorController>();
        }
    }

    void LateUpdate()
    {
        if (aimReference == null || !aimReference.ShowAimReferenceDebug)
        {
            return;
        }

        Ray ray = aimReference.GetCrosshairRay();
        Vector3 flatAim = aimReference.GetFlatAimDirection();
        float duration = Time.deltaTime;

        Debug.DrawRay(ray.origin, ray.direction * rayDrawDistance, Color.cyan, duration, false);
        Debug.DrawRay(ray.origin, flatAim * rayDrawDistance, Color.green, duration, false);

        if (meleeAttackOrigin != null)
        {
            Debug.DrawRay(meleeAttackOrigin.WorldPosition, meleeAttackOrigin.AimForward * 2.5f, Color.yellow, duration, false);
        }
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying || aimReference == null || !aimReference.ShowAimReferenceDebug)
        {
            return;
        }

        Ray ray = aimReference.GetCrosshairRay();
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(ray.origin, ray.direction * rayDrawDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(ray.origin, aimReference.GetFlatAimDirection() * rayDrawDistance);
    }
}
