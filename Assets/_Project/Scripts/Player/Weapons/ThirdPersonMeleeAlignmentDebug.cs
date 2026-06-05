using UnityEngine;

/// <summary>
/// Logs and draws third-person melee geometry when a crosshair target is active.
/// </summary>
[DisallowMultipleComponent]
[DefaultExecutionOrder(105)]
public class ThirdPersonMeleeAlignmentDebug : MonoBehaviour
{
    [SerializeField] bool showMeleeAlignmentDebug = true;
    [SerializeField] float logIntervalSeconds = 0.75f;

    EnemyCombatTargetingController combatTargeting;
    PlayerMeleeAttackOrigin meleeOrigin;
    PlayerCameraController cameraController;
    Transform playerRoot;
    Transform visualRoot;

    Vector3 lastTargetPoint;
    bool lastCanHit;
    EnemyController lastLoggedEnemy;
    float lastLogTime;

    public bool ShowMeleeAlignmentDebug => showMeleeAlignmentDebug;

    void Awake()
    {
        playerRoot = transform;
        combatTargeting = GetComponent<EnemyCombatTargetingController>();
        meleeOrigin = GetComponentInChildren<PlayerMeleeAttackOrigin>(true);
        cameraController = FindFirstObjectByType<PlayerCameraController>();
    }

    void LateUpdate()
    {
        if (!showMeleeAlignmentDebug || combatTargeting == null || !combatTargeting.HasTarget)
        {
            lastLoggedEnemy = null;
            return;
        }

        WeaponTargetCandidate target = combatTargeting.CurrentTarget;
        if (target.Enemy == null)
        {
            return;
        }

        lastTargetPoint = target.Collider != null
            ? target.Collider.ClosestPoint(meleeOrigin != null ? meleeOrigin.WorldPosition : playerRoot.position)
            : target.HitPoint;
        lastCanHit = combatTargeting.LastMeleeValidation.IsValid;

        if (target.Enemy != lastLoggedEnemy || Time.time - lastLogTime >= logIntervalSeconds)
        {
            LogAlignmentSnapshot(target);
            lastLoggedEnemy = target.Enemy;
            lastLogTime = Time.time;
        }
    }

    void LogAlignmentSnapshot(WeaponTargetCandidate target)
    {
        Camera camera = CrosshairRayUtility.ResolveGameplayCamera();
        if (camera == null || meleeOrigin == null || target.Collider == null)
        {
            return;
        }

        Vector3 targetPoint = target.Collider.ClosestPoint(meleeOrigin.WorldPosition);
        Vector3 aimForward = meleeOrigin.AimForward;
        Vector3 playerForward = GetPlayerVisualForward();
        Vector3 meleeForward = meleeOrigin.transform.forward;

        float cameraDistance = Vector3.Distance(camera.transform.position, targetPoint);
        float playerDistance = Vector3.Distance(playerRoot.position, targetPoint);
        float originDistance = Vector3.Distance(meleeOrigin.WorldPosition, targetPoint);

        float aimAngle = AngleOnPlane(aimForward, targetPoint - meleeOrigin.WorldPosition);
        float playerAngle = AngleOnPlane(playerForward, targetPoint - playerRoot.position);
        float meleeAngle = AngleOnPlane(meleeForward, targetPoint - meleeOrigin.WorldPosition);

        MeleeHitValidationResult validation = combatTargeting.LastMeleeValidation;

        Debug.Log($"[MeleeAlign] Crosshair target: {target.DisplayName}");
        Debug.Log($"[MeleeAlign] Camera to target distance: {cameraDistance:F2}");
        Debug.Log($"[MeleeAlign] PlayerRoot to target distance: {playerDistance:F2}");
        Debug.Log($"[MeleeAlign] MeleeOrigin to target distance: {originDistance:F2}");
        Debug.Log($"[MeleeAlign] Aim direction angle to target: {aimAngle:F1}");
        Debug.Log($"[MeleeAlign] Player forward angle to target: {playerAngle:F1}");
        Debug.Log($"[MeleeAlign] Melee origin angle to target: {meleeAngle:F1}");
        Debug.Log($"[MeleeAlign] Can hit with current melee settings: {validation.IsValid}");
    }

    Vector3 GetPlayerVisualForward()
    {
        if (visualRoot == null)
        {
            Transform found = playerRoot.Find(PlayerVisualBuilder.VisualRootName);
            visualRoot = found != null ? found : playerRoot;
        }

        Vector3 forward = visualRoot.forward;
        forward.y = 0f;
        return forward.sqrMagnitude > 0.001f ? forward.normalized : Vector3.forward;
    }

    static float AngleOnPlane(Vector3 referenceForward, Vector3 toTarget)
    {
        Vector3 flatRef = referenceForward;
        flatRef.y = 0f;
        Vector3 flatTo = toTarget;
        flatTo.y = 0f;
        if (flatRef.sqrMagnitude < 0.001f || flatTo.sqrMagnitude < 0.001f)
        {
            return 0f;
        }

        return Vector3.Angle(flatRef.normalized, flatTo.normalized);
    }

    void OnDrawGizmos()
    {
        if (!showMeleeAlignmentDebug || !Application.isPlaying || combatTargeting == null || !combatTargeting.HasTarget)
        {
            return;
        }

        Camera camera = CrosshairRayUtility.ResolveGameplayCamera();
        if (camera == null || meleeOrigin == null)
        {
            return;
        }

        Ray crosshairRay = CrosshairRayUtility.GetCrosshairRay(out _);
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(crosshairRay.origin, crosshairRay.direction * 8f);

        Gizmos.color = Color.blue;
        Vector3 playerPos = playerRoot.position + Vector3.up * 0.1f;
        Gizmos.DrawRay(playerPos, GetPlayerVisualForward() * 1.5f);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(meleeOrigin.WorldPosition, meleeOrigin.AimForward * 1.5f);

        Gizmos.color = lastCanHit ? Color.green : Color.red;
        Gizmos.DrawLine(meleeOrigin.WorldPosition, lastTargetPoint);
        Gizmos.DrawSphere(lastTargetPoint, 0.1f);
    }
}
