using System.Collections.Generic;
using UnityEngine;

public class PlayerPickupTargeting : MonoBehaviour
{
    enum PickupResolveSource
    {
        None,
        DirectRay,
        SphereCast,
        ScreenAssist,
        GroundAssist,
        ConeSearch,
        Stickiness,
    }

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
    [SerializeField] float sphereCastRadius = 1.15f;
    [SerializeField] float thirdPersonSphereCastRadius = 1.45f;
    [SerializeField] float pickupAssistAngle = 22f;
    [SerializeField] float firstPersonPickupAssistAngle = 12f;
    [SerializeField] float screenAssistRadiusPixels = 90f;
    [SerializeField] float groundAssistSearchRadius = 1.25f;

    [Header("Target Stickiness")]
    [SerializeField] float targetStickinessTime = 0.2f;
    [SerializeField] float targetSwitchAngleImprovementThreshold = 4f;

    [Header("Layers")]
    [SerializeField] LayerMask pickupLayerMask;
    [SerializeField] LayerMask obstacleLayerMask = ~0;
    [SerializeField] LayerMask groundProbeMask = ~0;

    [Header("Debug")]
    [SerializeField] bool showCrosshairPickupDebug = true;
    [SerializeField] bool logPickupTargeting = true;

    readonly List<PickupCandidateScore> debugRejectedCandidates = new List<PickupCandidateScore>();
    readonly List<RaycastHit> sphereCastHits = new List<RaycastHit>();

    PlayerCameraController cameraController;
    Camera viewCamera;
    WorldPickupItem selectedPickup;
    DataCoreCollectible selectedDataCore;
    WorldPickupItem stickyPickup;
    float stickyUntil;
    PickupResolveSource lastResolveSource = PickupResolveSource.None;
    WorldPickupItem lastLoggedPickup;

    Ray lastCrosshairRay;
    Vector2 lastCrosshairScreenPoint;
    Vector3 lastDirectHitPoint;
    bool lastHadDirectHit;
    WorldPickupItem lastVisualTarget;

    public WorldPickupItem SelectedPickup => selectedPickup;
    public DataCoreCollectible SelectedDataCore => selectedDataCore;
    public bool HasDataCoreTarget => selectedDataCore != null && selectedDataCore.CanBeCollected;
    public bool HasIdentifiedTarget => (selectedPickup != null && selectedPickup.CanBePickedUp) || HasDataCoreTarget;
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
        obstacleLayerMask = GameplayLayers.PickupLineOfSightObstacleMask;
        groundProbeMask = GameplayLayers.PickupLineOfSightObstacleMask;
        cameraController = FindFirstObjectByType<PlayerCameraController>();
        viewCamera = CrosshairRayUtility.ResolveGameplayCamera();
    }

    public void UpdatePickupTarget()
    {
        viewCamera = CrosshairRayUtility.ResolveGameplayCamera();
        lastCrosshairRay = ResolveCrosshairRay(out lastCrosshairScreenPoint);
        Vector3 aimDirection = lastCrosshairRay.direction.normalized;

        selectedPickup = null;
        selectedDataCore = null;
        lastResolveSource = PickupResolveSource.None;
        lastHadDirectHit = false;
        lastDirectHitPoint = lastCrosshairRay.origin + aimDirection * maxCameraTargetDistance;
        debugRejectedCandidates.Clear();

        if (TryResolveDirectRaycast(lastCrosshairRay, out WorldPickupItem directPickup, out DataCoreCollectible directDataCore, out RaycastHit directHit))
        {
            lastHadDirectHit = true;
            lastDirectHitPoint = directHit.point;
            if (directPickup != null)
            {
                CommitSelection(directPickup, PickupResolveSource.DirectRay);
            }
            else
            {
                CommitDataCoreSelection(directDataCore, PickupResolveSource.DirectRay);
            }

            DrawDebug(aimDirection, directPickup, directDataCore);
            return;
        }

        if (ShouldUsePickupAssist())
        {
            float castRadius = GetSphereCastRadius();
            if (TryResolveSphereCast(lastCrosshairRay, aimDirection, castRadius, out WorldPickupItem spherePickup))
            {
                if (ApplyStickiness(spherePickup, aimDirection, PickupResolveSource.SphereCast))
                {
                    DrawDebug(aimDirection, selectedPickup, selectedDataCore);
                    return;
                }
            }

            if (IsThirdPersonMode() && TryResolveScreenSpaceAssist(out WorldPickupItem screenPickup))
            {
                if (ApplyStickiness(screenPickup, aimDirection, PickupResolveSource.ScreenAssist))
                {
                    DrawDebug(aimDirection, selectedPickup, selectedDataCore);
                    return;
                }
            }

            if (IsThirdPersonMode() && TryResolveGroundPointAssist(lastCrosshairRay, out WorldPickupItem groundPickup))
            {
                if (ApplyStickiness(groundPickup, aimDirection, PickupResolveSource.GroundAssist))
                {
                    DrawDebug(aimDirection, selectedPickup, selectedDataCore);
                    return;
                }
            }

            if (TryResolveConeSearch(lastCrosshairRay.origin, aimDirection, out WorldPickupItem conePickup))
            {
                if (ApplyStickiness(conePickup, aimDirection, PickupResolveSource.ConeSearch))
                {
                    DrawDebug(aimDirection, selectedPickup, selectedDataCore);
                    return;
                }
            }
        }

        ApplyStickiness(null, aimDirection, PickupResolveSource.None);
        DrawDebug(aimDirection, null, null);
    }

    public string GetPickupPrompt(bool inventoryCanAccept)
    {
        if (HasDataCoreTarget)
        {
            if (!IsWithinPlayerReach)
            {
                return showItemNameOutsidePickupRange ? selectedDataCore.DisplayNameChinese : string.Empty;
            }

            return $"按 F 收集：{selectedDataCore.DisplayNameChinese}";
        }

        if (!HasIdentifiedTarget || selectedPickup == null)
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
            return $"按 F 拾取：{itemName}（背包已满）";
        }

        return $"按 F 拾取：{itemName}";
    }

    public string GetIdentifiedItemName()
    {
        return HasIdentifiedTarget ? selectedPickup.DisplayNameChinese : string.Empty;
    }

    void CommitDataCoreSelection(DataCoreCollectible dataCore, PickupResolveSource source)
    {
        selectedPickup = null;
        selectedDataCore = dataCore;
        stickyPickup = null;
        stickyUntil = Time.time + targetStickinessTime;
        lastResolveSource = source;
        UpdateReachState();
        LogSelectionIfChanged();
    }

    void CommitSelection(WorldPickupItem pickup, PickupResolveSource source)
    {
        selectedPickup = pickup;
        selectedDataCore = null;
        stickyPickup = pickup;
        stickyUntil = Time.time + targetStickinessTime;
        lastResolveSource = source;
        UpdateReachState();
        UpdateTargetVisuals();
        LogSelectionIfChanged();
    }

    bool ApplyStickiness(WorldPickupItem newPickup, Vector3 aimDirection, PickupResolveSource source)
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
                    CommitSelection(stickyPickup, PickupResolveSource.Stickiness);
                    return true;
                }
            }

            CommitSelection(newPickup, source);
            return true;
        }

        if (stickyPickup != null && stickyPickup.CanBePickedUp && Time.time < stickyUntil)
        {
            if (TryScoreCandidate(stickyPickup, lastCrosshairRay.origin, aimDirection, false, out _))
            {
                CommitSelection(stickyPickup, PickupResolveSource.Stickiness);
                return true;
            }
        }

        stickyPickup = null;
        selectedPickup = null;
        selectedDataCore = null;
        lastResolveSource = PickupResolveSource.None;
        UpdateReachState();
        UpdateTargetVisuals();
        LogSelectionIfChanged();
        return false;
    }

    void LogSelectionIfChanged()
    {
        if (!logPickupTargeting || selectedPickup == lastLoggedPickup)
        {
            return;
        }

        lastLoggedPickup = selectedPickup;
        if (selectedPickup == null)
        {
            return;
        }

        string targetKind = selectedPickup.GetComponent<DroppedItemMarker>() != null ? "Dropped item" : "Pickup item";
        Debug.Log(
            $"[Pickup] {targetKind} selected: {selectedPickup.ItemKind} x{selectedPickup.Amount} | " +
            $"Source: {lastResolveSource} | Within pickup range: {IsWithinPlayerReach}");
    }

    Ray ResolveCrosshairRay(out Vector2 screenPoint)
    {
        if (AimReferenceProvider.Instance != null && !AimReferenceProvider.Instance.IsWorldAimingBlocked)
        {
            screenPoint = AimReferenceProvider.Instance.GetCrosshairScreenPoint();
            return AimReferenceProvider.Instance.GetCrosshairRay();
        }

        return CrosshairRayUtility.GetCrosshairRay(out screenPoint);
    }

    bool TryResolveDirectRaycast(Ray ray, out WorldPickupItem pickup, out DataCoreCollectible dataCore, out RaycastHit directHit)
    {
        pickup = null;
        dataCore = null;
        directHit = default;

        RaycastHit[] hits = Physics.RaycastAll(
            ray,
            maxCameraTargetDistance,
            pickupLayerMask,
            QueryTriggerInteraction.Collide);

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        for (int i = 0; i < hits.Length; i++)
        {
            if (TryGetPickupFromHit(hits[i], out WorldPickupItem pickupCandidate))
            {
                pickup = pickupCandidate;
                directHit = hits[i];
                return true;
            }

            if (TryGetDataCoreFromHit(hits[i], out DataCoreCollectible coreCandidate))
            {
                dataCore = coreCandidate;
                directHit = hits[i];
                return true;
            }
        }

        return false;
    }

    bool TryGetDataCoreFromHit(RaycastHit hit, out DataCoreCollectible dataCore)
    {
        dataCore = null;
        if (hit.collider == null || ShouldIgnoreCollider(hit.collider))
        {
            return false;
        }

        dataCore = hit.collider.GetComponentInParent<DataCoreCollectible>();
        return dataCore != null && dataCore.CanBeCollected;
    }

    bool TryResolveSphereCast(Ray ray, Vector3 aimDirection, float radius, out WorldPickupItem pickup)
    {
        pickup = null;
        WorldPickupItem bestPickup = null;
        float bestScore = float.MaxValue;

        RaycastHit[] hits = Physics.SphereCastAll(
            ray.origin,
            radius,
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

    bool TryResolveScreenSpaceAssist(out WorldPickupItem pickup)
    {
        pickup = null;
        if (viewCamera == null)
        {
            return false;
        }

        WorldPickupItem bestPickup = null;
        float bestScreenDistance = screenAssistRadiusPixels;
        WorldPickupItem[] allPickups = FindObjectsByType<WorldPickupItem>(FindObjectsSortMode.None);

        for (int i = 0; i < allPickups.Length; i++)
        {
            WorldPickupItem candidate = allPickups[i];
            if (candidate == null || !candidate.CanBePickedUp)
            {
                continue;
            }

            Vector3 targetPoint = candidate.GetInteractionPoint();
            Vector3 screenPoint3 = viewCamera.WorldToScreenPoint(targetPoint);
            if (screenPoint3.z <= 0.01f)
            {
                continue;
            }

            float screenDistance = Vector2.Distance(lastCrosshairScreenPoint, new Vector2(screenPoint3.x, screenPoint3.y));
            if (screenDistance > screenAssistRadiusPixels)
            {
                continue;
            }

            Vector3 toTarget = targetPoint - lastCrosshairRay.origin;
            float cameraDistance = toTarget.magnitude;
            if (cameraDistance > maxCameraTargetDistance + 0.01f)
            {
                continue;
            }

            if (!HasClearLineOfSight(lastCrosshairRay.origin, targetPoint, candidate.transform))
            {
                continue;
            }

            if (screenDistance < bestScreenDistance)
            {
                bestScreenDistance = screenDistance;
                bestPickup = candidate;
            }
        }

        pickup = bestPickup;
        return pickup != null;
    }

    bool TryResolveGroundPointAssist(Ray ray, out WorldPickupItem pickup)
    {
        pickup = null;
        if (!Physics.Raycast(
                ray,
                out RaycastHit groundHit,
                maxCameraTargetDistance,
                groundProbeMask,
                QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        Vector3 searchOrigin = groundHit.point + Vector3.up * 0.15f;
        Collider[] overlaps = Physics.OverlapSphere(
            searchOrigin,
            groundAssistSearchRadius,
            pickupLayerMask,
            QueryTriggerInteraction.Collide);

        WorldPickupItem bestPickup = null;
        float bestScore = float.MaxValue;
        Vector3 aimDirection = ray.direction.normalized;

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

            float horizontal = HorizontalDistance(searchOrigin, candidate.GetInteractionPoint());
            if (horizontal > groundAssistSearchRadius)
            {
                continue;
            }

            if (!TryScoreCandidate(candidate, ray.origin, aimDirection, false, out PickupCandidateScore score))
            {
                debugRejectedCandidates.Add(score);
                continue;
            }

            score.Score += horizontal * 0.15f;
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

            if (hits[i].point.y < targetPoint.y - 0.25f)
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

        Vector3 targetPoint = selectedDataCore != null
            ? selectedDataCore.GetInteractionPoint()
            : selectedPickup.GetInteractionPoint();
        Vector3 flat = targetPoint - transform.position;
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

    float GetSphereCastRadius()
    {
        return IsThirdPersonMode() ? thirdPersonSphereCastRadius : sphereCastRadius;
    }

    bool IsThirdPersonMode()
    {
        return cameraController == null || !cameraController.IsFirstPerson;
    }

    static float HorizontalDistance(Vector3 a, Vector3 b)
    {
        Vector3 flat = b - a;
        flat.y = 0f;
        return flat.magnitude;
    }

    static bool IsPlayerCollider(Collider collider)
    {
        return collider.GetComponentInParent<PlayerController>() != null
            || collider.GetComponentInParent<CharacterController>() != null;
    }

    void DrawDebug(Vector3 aimDirection, WorldPickupItem pickup, DataCoreCollectible dataCore)
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

        if (dataCore != null)
        {
            Vector3 point = dataCore.GetInteractionPoint();
            Debug.DrawLine(lastCrosshairRay.origin, point, Color.cyan, duration, false);
            Debug.DrawRay(point, Vector3.up * 0.5f, Color.cyan, duration, false);
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
        Gizmos.DrawWireSphere(lastCrosshairRay.origin, GetSphereCastRadius());
    }

    public void ClearSelection()
    {
        if (lastVisualTarget != null)
        {
            lastVisualTarget.SetCrosshairTargetState(false, false);
            lastVisualTarget = null;
        }

        selectedPickup = null;
        selectedDataCore = null;
        stickyPickup = null;
        stickyUntil = 0f;
        lastLoggedPickup = null;
        IsWithinPlayerReach = false;
        sphereCastHits.Clear();
        debugRejectedCandidates.Clear();
    }
}
