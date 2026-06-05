using UnityEngine;

/// <summary>
/// World-space melee attack origin in front of the player, aligned to crosshair yaw.
/// </summary>
[DisallowMultipleComponent]
[DefaultExecutionOrder(95)]
public class PlayerMeleeAttackOrigin : MonoBehaviour
{
    [Header("Attack Origin (local to player feet + aim)")]
    [SerializeField] Vector3 meleeAttackOriginLocalPosition = new Vector3(0.32f, 1.28f, 0.58f);
    [SerializeField] float meleeVerticalTolerance = 1.5f;
    [SerializeField] bool syncOriginHeightWithCameraPivot = false;

    [Header("Debug")]
    [SerializeField] bool showMeleeAttackDebug = true;
    [SerializeField] float debugArcRange = 2.5f;
    [SerializeField] float debugArcAngle = 42f;
    [SerializeField] int debugArcSegments = 24;

    PlayerCameraController cameraController;
    Transform playerRoot;
    Vector3 lastAimForward = Vector3.forward;

    public Vector3 LocalOriginOffset => meleeAttackOriginLocalPosition;
    public float VerticalTolerance => meleeVerticalTolerance;
    public Vector3 WorldPosition => transform.position;
    public Vector3 AimForward => lastAimForward;
    public bool ShowMeleeAttackDebug => showMeleeAttackDebug;

    public static PlayerMeleeAttackOrigin EnsureOnPlayer(GameObject playerRootObject)
    {
        if (playerRootObject == null)
        {
            return null;
        }

        PlayerMeleeAttackOrigin existing = playerRootObject.GetComponentInChildren<PlayerMeleeAttackOrigin>(true);
        if (existing != null)
        {
            return existing;
        }

        var originObject = new GameObject("MeleeAttackOrigin");
        originObject.transform.SetParent(playerRootObject.transform, false);
        return originObject.AddComponent<PlayerMeleeAttackOrigin>();
    }

    void Awake()
    {
        playerRoot = transform.parent != null ? transform.parent : transform;
        cameraController = FindFirstObjectByType<PlayerCameraController>();
    }

    void LateUpdate()
    {
        UpdateOriginTransform();
    }

    public void Configure(Vector3 localPosition, float verticalTolerance)
    {
        meleeAttackOriginLocalPosition = localPosition;
        meleeVerticalTolerance = verticalTolerance;
        UpdateOriginTransform();
    }

    void UpdateOriginTransform()
    {
        if (playerRoot == null)
        {
            playerRoot = transform.parent != null ? transform.parent : transform;
        }

        if (cameraController == null)
        {
            cameraController = FindFirstObjectByType<PlayerCameraController>();
        }

        Vector3 aimForward = GetImmediateAimForward();
        Vector3 aimRight = Vector3.Cross(Vector3.up, aimForward).normalized;
        Vector3 feet = playerRoot.position;
        float originHeight = meleeAttackOriginLocalPosition.y;
        if (syncOriginHeightWithCameraPivot && cameraController != null)
        {
            originHeight = cameraController.ThirdPersonLookAtHeight;
        }

        Vector3 worldPosition = feet
            + Vector3.up * originHeight
            + aimForward * meleeAttackOriginLocalPosition.z
            + aimRight * meleeAttackOriginLocalPosition.x;

        transform.SetPositionAndRotation(worldPosition, Quaternion.LookRotation(aimForward, Vector3.up));
        lastAimForward = aimForward;
    }

    Vector3 GetImmediateAimForward()
    {
        if (cameraController != null)
        {
            return cameraController.GetImmediateAimForward();
        }

        Camera camera = CrosshairRayUtility.ResolveGameplayCamera();
        if (camera != null)
        {
            Vector3 forward = camera.transform.forward;
            forward.y = 0f;
            return forward.sqrMagnitude > 0.001f ? forward.normalized : Vector3.forward;
        }

        Vector3 rootForward = playerRoot.forward;
        rootForward.y = 0f;
        return rootForward.sqrMagnitude > 0.001f ? rootForward.normalized : Vector3.forward;
    }

    public void LogValidation(MeleeHitValidationResult validation, string targetName)
    {
        if (!showMeleeAttackDebug)
        {
            return;
        }

        Debug.Log($"[MeleeAttack] Crosshair target: {targetName}");
        Debug.Log($"[MeleeAttack] Melee origin: {WorldPosition}");
        Debug.Log($"[MeleeAttack] Distance to target: {validation.Distance:F2} / range {validation.MaxRange:F2}");
        Debug.Log($"[MeleeAttack] Angle to target: {validation.Angle:F1} / angle {validation.MaxAngle:F1}");
        Debug.Log($"[MeleeAttack] Melee hit valid: {validation.IsValid}");
        if (!validation.IsValid && !string.IsNullOrEmpty(validation.MissReason))
        {
            Debug.Log($"[MeleeAttack] Melee miss reason: {validation.MissReason}");
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!showMeleeAttackDebug || !Application.isPlaying)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.08f);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, lastAimForward * 1.2f);

        float halfAngle = debugArcAngle * 0.5f * Mathf.Deg2Rad;
        float range = debugArcRange;
        Vector3 origin = transform.position;
        Vector3 forward = lastAimForward;
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

        Vector3 prevPoint = origin + forward * range;
        for (int i = 1; i <= debugArcSegments; i++)
        {
            float t = i / (float)debugArcSegments;
            float angle = Mathf.Lerp(-halfAngle, halfAngle, t);
            Vector3 dir = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.up) * forward;
            Vector3 point = origin + dir * range;
            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }

        Gizmos.DrawLine(origin, prevPoint);
        Gizmos.DrawLine(origin, origin + Quaternion.AngleAxis(-halfAngle * Mathf.Rad2Deg, Vector3.up) * forward * range);
    }
}
