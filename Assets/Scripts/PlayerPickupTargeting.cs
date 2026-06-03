using System.Collections.Generic;
using UnityEngine;

public class PlayerPickupTargeting : MonoBehaviour
{
    struct PickupCandidateScore
    {
        public WorldPickupItem Pickup;
        public float Score;
        public float Angle;
        public float CameraDistance;
    }

    [Header("Pickup Range")]
    [SerializeField] float maxCameraTargetDistance = 9f;
    [SerializeField] float playerPickupReach = 4f;

    [Header("Display")]
    [SerializeField] bool showItemNameOutsidePickupRange = true;
    [SerializeField] bool showPickableMarkerOnlyInRange = true;

    [Header("Pickup Assist")]
    [SerializeField] bool pickupAssistEnabledInThirdPerson = true;
    [SerializeField] bool pickupAssistEnabledInFirstPerson = true;
    [SerializeField] float sphereCastRadius = 1f;
    [SerializeField] float pickupAssistAngle = 20f;
    [SerializeField] float firstPersonPickupAssistAngle = 12f;

    [Header("Target Stickiness")]
    [SerializeField] float targetStickinessTime = 0.2f;
    [SerializeField] float targetSwitchAngleImprovementThreshold = 4f;

    [Header("Layers")]
    [SerializeField] LayerMask pickupLayerMask;
    [SerializeField] LayerMask obstacleLayerMask = ~0;

    [Header("Debug")]
    [SerializeField] bool showCrosshairPickupDebug = true;

    readonly List<PickupCandidateScore> debugRejectedCandidates = new List<PickupCandidateScore>();
    readonly List<RaycastHit> sphereCastHits = new List<RaycastHit>();

    PlayerCameraController cameraController;
    Camera viewCamera;
    WorldPickupItem selectedPickup;
    WorldPickupItem stickyPickup;
    float stickyUntil;

    Ray lastCrosshairRay;
    Vector2 lastCrosshairScreenPoint;
    Vector3 lastDirectHitPoint;
    bool lastHadDirectHit;
    WorldPickupItem lastVisualTarget;

    public WorldPickupItem SelectedPickup => selectedPickup;
    public bool HasIdentifiedTarget => selectedPickup != null && selectedPickup.CanBePickedUp;
    public bool HasPickupTarget => HasIdentifiedTarget;
    public bool CanPickupSelected => HasIdentifiedTarget && IsWithinPlayerReach;
    public bool IsWithinPlayerReach { get; private set; }
    public bool IsTooFarFromPlayer => HasIdentifiedTarget && !IsWithinPlayerReach;
    public float MaxCameraTargetDistance => maxCameraTargetDistance;
    public float PlayerPickupReach => playerPickupReach;
    public Ray LastCrosshairRay => lastCrosshairRay;
    public Vector2 LastCrosshairScreenPoint => lastCrosshairScreenPoint;

    void Awake()
    {
        pickupLayerMask = GameplayLayers.PickupDetectionMask;
        cameraController = FindFirstObjectByType<PlayerCameraController>();
        viewCamera = CrosshairRayUtility.ResolveGameplayCamera();
    }

    public void UpdatePickupTarget()
    {
        viewCamera = CrosshairRayUtility.ResolveGameplayCamera();
        lastCrosshairRay = CrosshairRayUtility.GetCrosshairRay(out lastCrosshairScreenPoint);
        Vector3 aimDirection = lastCrosshairRay.direction.normalized;

        selectedPickup = null;
        lastHadDirectHit = false;
        lastDirectHitPoint = lastCrosshairRay.origin + aimDirection * maxCameraTargetDistance;
        debugRejectedCandidates.Clear();

        if (TryResolveDirectRaycast(lastCrosshairRay, out WorldPickupItem directPickup, out RaycastHit directHit))
        {
            lastHadDirectHit = true;
            lastDirectHitPoint = directHit.point;
            CommitSelection(directPickup, true);
            DrawDebug(aimDirection, directPickup);
            return;
        }

        if (ShouldUsePickupAssist() && TryResolveSphereCast(lastCrosshairRay, aimDirection, out WorldPickupItem spherePickup))
        {
            if (ApplyStickiness(spherePickup, aimDirection))
            {
                DrawDebug(aimDirection, selectedPickup);
                return;
            }
        }

        if (ShouldUsePickupAssist() && TryResolveConeSearch(lastCrosshairRay.origin, aimDirection, out WorldPickupItem conePickup))
        {
            if (ApplyStickiness(conePickup, aimDirection))
            {
                DrawDebug(aimDirection, selectedPickup);
                return;
            }
        }

        ApplyStickiness(null, aimDirection);
        DrawDebug(aimDirection, null);
    }

    public string GetPickupPrompt(bool inventoryCanAccept)
    {
        if (!HasIdentifiedTarget)
        {
            return string.Empty;
        }

        string itemName = selectedPickup.DisplayNameChinese;
        if (!IsWithinPlayerReach)
        {
            return showItemNameOutsidePickupRange ? itemName : string.Empty;
        }

        if (!inventoryCanAccept)
        {
            return $"{itemName}\n按 F 拾取（背包已满）";
        }

        return $"{itemName}\n按 F 拾取";
    }

    public string GetIdentifiedItemName()
    {
        return HasIdentifiedTarget ? selectedPickup.DisplayNameChinese : string.Empty;
    }

    void CommitSelection(WorldPickupItem pickup, bool directHit)
    {
        selectedPickup = pickup;
        stickyPickup = pickup;
        stickyUntil = Time.time + targetStickinessTime;
        UpdateReachState();
        UpdateTargetVisuals();
    }

    bool ApplyStickiness(WorldPickupItem newPickup, Vector3 aimDirection)
    {
        if (newPickup != null)
        {
            if (stickyPickup != null
                && stickyPickup.CanBePickedUp
                && Time.time < stickyUntil
                && stickyPickup != newPickup
                && TryScoreCandidate(stickyPickup, lastCrosshairRay.origin, aimDirection, false, out PickupCandidateScore stickyScore)
                && TryScoreCandidate(newPickup, lastCrosshairRay.origin, aimDirection, false, out PickupCandidateScore newScore))
            {
                bool clearlyBetter = newScore.Angle + targetSwitchAngleImprovementThreshold < stickyScore.Angle
                    || newScore.Score + 0.08f < stickyScore.Score;

                if (!clearlyBetter)
                {
                    CommitSelection(stickyPickup, false);
                    return true;
                }
            }

            CommitSelection(newPickup, false);
            return true;
        }

        if (stickyPickup != null && stickyPickup.CanBePickedUp && Time.time < stickyUntil)
        {
            if (TryScoreCandidate(stickyPickup, lastCrosshairRay.origin, aimDirection, false, out _))
            {
                CommitSelection(stickyPickup, false);
                return true;
            }
        }

        stickyPickup = null;
        selectedPickup = null;
        UpdateReachState();
        UpdateTargetVisuals();
        return false;
    }

    bool TryResolveDirectRaycast(Ray ray, out WorldPickupItem pickup, out RaycastHit directHit)
    {
        pickup = null;
        directHit = default;

        RaycastHit[] hits = Physics.RaycastAll(
            ray,
            maxCameraTargetDistance,
            pickupLayerMask,
            QueryTriggerInteraction.Collide);

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        for (int i = 0; i < hits.Length; i++)
        {
            if (!TryGetPickupFromHit(hits[i], out WorldPickupItem candidate))
            {
                continue;
            }

            pickup = candidate;
            directHit = hits[i];
            return true;
        }

        return false;
    }

    bool TryResolveSphereCast(Ray ray, Vector3 aimDirection, out WorldPickupItem pickup)
    {
        pickup = null;
        WorldPickupItem bestPickup = null;
        float bestScore = float.MaxValue;

        RaycastHit[] hits = Physics.SphereCastAll(
            ray.origin,
            sphereCastRadius,
            ray.direction,
            maxCameraTargetDistance,
            pickupLayerMask,
            QueryTriggerInteraction.Collide);

        sphereCastHits.Clear();
        for (int i = 0; i < hits.Length; i++)
        {
            sphereCastHits.Add(hits[i]);
        }

        for (int i = 0; i < hits.Length; i++)
        {
            if (!TryGetPickupFromHit(hits[i], out WorldPickupItem candidate))
            {
                continue;
            }

            if (!TryScoreCandidate(candidate, ray.origin, aimDirection, false, out PickupCandidateScore score))
            {
                debugRejectedCandidates.Add(score);
                continue;
            }

            if (score.Score < bestScore)
            {
                bestScore = score.Score;
                bestPickup = candidate;
            }
        }

        pickup = bestPickup;
        return pickup != null;
    }

    bool TryResolveConeSearch(Vector3 origin, Vector3 aimDirection, out WorldPickupItem pickup)
    {
        pickup = null;
        float bestScore = float.MaxValue;
        WorldPickupItem bestPickup = null;
        float assistAngle = GetAssistAngle();

        Collider[] overlaps = Physics.OverlapSphere(
            origin,
            maxCameraTargetDistance,
            pickupLayerMask,
            QueryTriggerInteraction.Collide);

        HashSet<WorldPickupItem> seen = new HashSet<WorldPickupItem>();
        for (int i = 0; i < overlaps.Length; i++)
        {
            if (overlaps[i] == null || ShouldIgnoreCollider(overlaps[i]))
            {
                continue;
            }

            WorldPickupItem candidate = overlaps[i].GetComponentInParent<WorldPickupItem>();
            if (candidate == null || !candidate.CanBePickedUp || !seen.Add(candidate))
            {
                continue;
            }

            if (!TryScoreCandidate(candidate, origin, aimDirection, false, out PickupCandidateScore score))
            {
                debugRejectedCandidates.Add(score);
                continue;
            }

            if (score.Angle > assistAngle)
            {
                continue;
            }

            if (score.Score < bestScore)
            {
                bestScore = score.Score;
                bestPickup = candidate;
            }
        }

        if (bestPickup == null)
        {
            WorldPickupItem[] allPickups = FindObjectsByType<WorldPickupItem>(FindObjectsSortMode.None);
            for (int i = 0; i < allPickups.Length; i++)
            {
                WorldPickupItem candidate = allPickups[i];
                if (candidate == null || !candidate.CanBePickedUp || !seen.Add(candidate))
                {
                    continue;
                }

                if (!TryScoreCandidate(candidate, origin, aimDirection, false, out PickupCandidateScore score))
                {
                    debugRejectedCandidates.Add(score);
                    continue;
                }

                if (score.Angle > assistAngle)
                {
                    continue;
                }

                if (score.Score < bestScore)
                {
                    bestScore = score.Score;
                    bestPickup = candidate;
                }
            }
        }

        pickup = bestPickup;
        return pickup != null;
    }

    bool TryScoreCandidate(
        WorldPickupItem pickup,
        Vector3 origin,
        Vector3 aimDirection,
        bool directHit,
        out PickupCandidateScore result)
    {
        result = new PickupCandidateScore { Pickup = pickup, Score = float.MaxValue, Angle = 999f };

        if (pickup == null || !pickup.CanBePickedUp)
        {
            return false;
        }

        Vector3 targetPoint = pickup.GetInteractionPoint();
        Vector3 toTarget = targetPoint - origin;
        float cameraDistance = toTarget.magnitude;
        if (cameraDistance > maxCameraTargetDistance + 0.01f || cameraDistance < 0.01f)
        {
            return false;
        }

        Vector3 direction = toTarget / cameraDistance;
        float angle = Vector3.Angle(aimDirection, direction);
        if (!directHit && angle > GetAssistAngle())
        {
            result.Angle = angle;
            return false;
        }

        if (Vector3.Dot(aimDirection, direction) <= 0.02f)
        {
            result.Angle = angle;
            return false;
        }

        if (!directHit && !HasClearLineOfSight(origin, targetPoint, pickup.transform))
        {
            result.Angle = angle;
            return false;
        }

        float normalizedAngle = angle / Mathf.Max(GetAssistAngle(), 0.01f);
        float normalizedDistance = cameraDistance / Mathf.Max(maxCameraTargetDistance, 0.01f);
        float score = normalizedAngle * 2f + normalizedDistance;
        if (directHit)
        {
            score *= 0.01f;
        }

        result = new PickupCandidateScore
        {
            Pickup = pickup,
            Score = score,
            Angle = angle,
            CameraDistance = cameraDistance,
        };
        return true;
    }

    bool TryGetPickupFromHit(RaycastHit hit, out WorldPickupItem pickup)
    {
        pickup = null;
        if (hit.collider == null || ShouldIgnoreCollider(hit.collider))
        {
            return false;
        }

        pickup = hit.collider.GetComponentInParent<WorldPickupItem>();
        return pickup != null && pickup.CanBePickedUp;
    }

    bool ShouldIgnoreCollider(Collider collider)
    {
        if (collider == null)
        {
            return true;
        }

        if (IsPlayerCollider(collider))
        {
            return true;
        }

        if (GameplayLayers.HasPlayerLayer && collider.gameObject.layer == GameplayLayers.Player)
        {
            return true;
        }

        return false;
    }

    bool HasClearLineOfSight(Vector3 origin, Vector3 targetPoint, Transform targetRoot)
    {
        Vector3 direction = targetPoint - origin;
        float distance = direction.magnitude;
        if (distance < 0.01f)
        {
            return true;
        }

        direction /= distance;
        RaycastHit[] hits = Physics.RaycastAll(origin, direction, distance, obstacleLayerMask, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        for (int i = 0; i < hits.Length; i++)
        {
            Collider hitCollider = hits[i].collider;
            if (hitCollider == null || ShouldIgnoreCollider(hitCollider))
            {
                continue;
            }

            if (targetRoot != null && hitCollider.transform.IsChildOf(targetRoot))
            {
                return true;
            }

            if (hitCollider.GetComponentInParent<WorldPickupItem>() != null)
            {
                return targetRoot != null && hitCollider.transform.IsChildOf(targetRoot);
            }

            if (hitCollider.isTrigger)
            {
                continue;
            }

            return false;
        }

        return true;
    }

    void UpdateReachState()
    {
        if (!HasIdentifiedTarget)
        {
            IsWithinPlayerReach = false;
            return;
        }

        Vector3 flat = selectedPickup.GetInteractionPoint() - transform.position;
        flat.y = 0f;
        IsWithinPlayerReach = flat.magnitude <= playerPickupReach;
    }

    void UpdateTargetVisuals()
    {
        if (lastVisualTarget != null && lastVisualTarget != selectedPickup)
        {
            lastVisualTarget.SetCrosshairTargetState(false, false);
        }

        if (selectedPickup != null)
        {
            bool showPickableMarker = !showPickableMarkerOnlyInRange || IsWithinPlayerReach;
            selectedPickup.SetCrosshairTargetState(true, showPickableMarker);
            lastVisualTarget = selectedPickup;
        }
        else
        {
            lastVisualTarget = null;
        }
    }

    bool ShouldUsePickupAssist()
    {
        if (IsThirdPersonMode())
        {
            return pickupAssistEnabledInThirdPerson;
        }

        return pickupAssistEnabledInFirstPerson;
    }

    float GetAssistAngle()
    {
        return IsThirdPersonMode() ? pickupAssistAngle : firstPersonPickupAssistAngle;
    }

    bool IsThirdPersonMode()
    {
        return cameraController == null || !cameraController.IsFirstPerson;
    }

    static bool IsPlayerCollider(Collider collider)
    {
        return collider.GetComponentInParent<PlayerController>() != null
            || collider.GetComponentInParent<CharacterController>() != null;
    }

    void DrawDebug(Vector3 aimDirection, WorldPickupItem pickup)
    {
        if (!showCrosshairPickupDebug)
        {
            return;
        }

        float duration = Time.deltaTime;
        Vector3 rayEnd = lastCrosshairRay.origin + aimDirection * maxCameraTargetDistance;
        Debug.DrawLine(lastCrosshairRay.origin, rayEnd, lastHadDirectHit ? Color.green : Color.yellow, duration, false);
        Debug.DrawLine(lastCrosshairRay.origin, lastDirectHitPoint, lastHadDirectHit ? Color.green : Color.red, duration, false);

        DrawAssistCone(lastCrosshairRay.origin, aimDirection, GetAssistAngle(), maxCameraTargetDistance, new Color(0.2f, 0.8f, 1f, 1f), duration);

        for (int i = 0; i < sphereCastHits.Count; i++)
        {
            Debug.DrawLine(sphereCastHits[i].point, sphereCastHits[i].point + Vector3.up * 0.2f, Color.magenta, duration, false);
        }

        if (pickup != null)
        {
            Vector3 point = pickup.GetInteractionPoint();
            Debug.DrawLine(lastCrosshairRay.origin, point, Color.green, duration, false);
            Debug.DrawRay(point, Vector3.up * 0.5f, Color.green, duration, false);
        }

        for (int i = 0; i < debugRejectedCandidates.Count; i++)
        {
            if (debugRejectedCandidates[i].Pickup == null)
            {
                continue;
            }

            Vector3 rejectedPoint = debugRejectedCandidates[i].Pickup.GetInteractionPoint();
            Debug.DrawLine(lastCrosshairRay.origin, rejectedPoint, new Color(1f, 0.35f, 0.35f, 0.6f), duration, false);
        }
    }

    static void DrawAssistCone(Vector3 origin, Vector3 forward, float angleDegrees, float length, Color color, float duration)
    {
        Vector3 rightEdge = (Quaternion.AngleAxis(angleDegrees, Vector3.up) * forward).normalized;
        Vector3 leftEdge = (Quaternion.AngleAxis(-angleDegrees, Vector3.up) * forward).normalized;
        Debug.DrawRay(origin, leftEdge * length, color, duration, false);
        Debug.DrawRay(origin, rightEdge * length, color, duration, false);
    }

    void OnDrawGizmosSelected()
    {
        if (!showCrosshairPickupDebug || !Application.isPlaying)
        {
            return;
        }

        Gizmos.color = Color.green;
        if (selectedPickup != null)
        {
            Gizmos.DrawWireSphere(selectedPickup.GetInteractionPoint(), 0.25f);
        }

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.35f);
        Gizmos.DrawWireSphere(lastCrosshairRay.origin, sphereCastRadius);
    }

    public void ClearSelection()
    {
        if (lastVisualTarget != null)
        {
            lastVisualTarget.SetCrosshairTargetState(false, false);
            lastVisualTarget = null;
        }

        selectedPickup = null;
        stickyPickup = null;
        stickyUntil = 0f;
        IsWithinPlayerReach = false;
        sphereCastHits.Clear();
        debugRejectedCandidates.Clear();
    }
}
