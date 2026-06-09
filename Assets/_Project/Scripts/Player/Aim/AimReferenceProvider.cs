using UnityEngine;

/// <summary>
/// Single source of truth for crosshair-based world aiming.
/// All gameplay direction (facing, melee, projectiles, placement, pickup, enemy target) reads from here.
/// </summary>
[DisallowMultipleComponent]
[DefaultExecutionOrder(45)]
public class AimReferenceProvider : MonoBehaviour
{
    static AimReferenceProvider instance;

    [Header("Debug")]
    [SerializeField] bool showAimReferenceDebug;

    Vector2 crosshairScreenPoint;
    Ray crosshairRay;
    Vector3 aimDirection = Vector3.forward;
    Vector3 flatAimDirection = Vector3.forward;
    Vector3 flatAimRight = Vector3.right;
    Vector3 lastValidFlatAim = Vector3.forward;
    bool worldAimingBlocked;
    bool loggedRayOrigin;
    RectTransform boundCrosshairRect;

    public static AimReferenceProvider Instance => instance;

    public bool IsWorldAimingBlocked => worldAimingBlocked;
    public Vector2 CrosshairScreenPoint => crosshairScreenPoint;
    public Ray CrosshairRay => crosshairRay;
    public Vector3 AimDirection => aimDirection;
    public Vector3 FlatAimDirection => flatAimDirection;
    public Vector3 FlatAimRight => flatAimRight;
    public bool ShowAimReferenceDebug => showAimReferenceDebug;
    public RectTransform BoundCrosshairRect => boundCrosshairRect;

    public void BindCrosshairRect(RectTransform crosshairRect)
    {
        boundCrosshairRect = crosshairRect;
    }

    public static AimReferenceProvider EnsureOnPlayer(GameObject playerRoot)
    {
        if (playerRoot == null)
        {
            return null;
        }

        AimReferenceProvider existing = playerRoot.GetComponent<AimReferenceProvider>();
        if (existing != null)
        {
            return existing;
        }

        return playerRoot.AddComponent<AimReferenceProvider>();
    }

    void Awake()
    {
        instance = this;
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    void LateUpdate()
    {
        Refresh();
    }

    public void Refresh()
    {
        worldAimingBlocked = GameplayCursorPolicy.IsAnyMenuOpen;
        crosshairRay = CrosshairRayUtility.GetCrosshairRay(out crosshairScreenPoint);
        aimDirection = crosshairRay.direction.sqrMagnitude > 0.0001f
            ? crosshairRay.direction.normalized
            : lastValidFlatAim;

        Vector3 flat = aimDirection;
        flat.y = 0f;
        if (flat.sqrMagnitude > 0.0001f)
        {
            flat.Normalize();
            flatAimDirection = flat;
            lastValidFlatAim = flat;
        }
        else
        {
            flatAimDirection = lastValidFlatAim;
        }

        flatAimRight = Vector3.Cross(Vector3.up, flatAimDirection).normalized;

        if (showAimReferenceDebug && !loggedRayOrigin)
        {
            Camera camera = CrosshairRayUtility.ResolveGameplayCamera();
            Debug.Log($"[AimReference] AimReference ray origin: {(camera != null ? "camera" : "fallback")}");
            Debug.Log("[AimReference] AimReference screen point: center/crosshair UI");
            loggedRayOrigin = true;
        }
    }

    public Vector2 GetCrosshairScreenPoint() => crosshairScreenPoint;

    public Ray GetCrosshairRay() => crosshairRay;

    public Vector3 GetAimDirection() => aimDirection;

    public Vector3 GetFlatAimDirection() => flatAimDirection;

    public Vector3 GetFlatAimRight() => flatAimRight;

    public bool TryGetAimHit(out RaycastHit hit, LayerMask mask, float maxDistance)
    {
        return Physics.Raycast(crosshairRay, out hit, maxDistance, mask, QueryTriggerInteraction.Collide);
    }

    public Vector3 GetAimPoint(float fallbackDistance)
    {
        float distance = Mathf.Max(1f, fallbackDistance);
        LayerMask mask = GameplayLayers.PickupLineOfSightObstacleMask | GameplayLayers.CombatTargetMask;
        if (Physics.Raycast(crosshairRay, out RaycastHit hit, distance, mask, QueryTriggerInteraction.Ignore))
        {
            return hit.point;
        }

        return crosshairRay.origin + crosshairRay.direction * distance;
    }

    public static bool TryGetFlatAim(out Vector3 flatAim)
    {
        if (instance != null && !instance.worldAimingBlocked)
        {
            flatAim = instance.GetFlatAimDirection();
            return true;
        }

        Ray ray = CrosshairRayUtility.GetCrosshairRay(out _);
        flatAim = ray.direction;
        flatAim.y = 0f;
        if (flatAim.sqrMagnitude > 0.0001f)
        {
            flatAim.Normalize();
            return true;
        }

        flatAim = Vector3.forward;
        return false;
    }
}
