using UnityEngine;

public class PlayerGameplayTargeting : MonoBehaviour
{
    [Header("Range")]
    [SerializeField] float maxPickupDistance = 5f;
    [SerializeField] float maxInteractionDistance = 5f;
    [SerializeField] float maxPlacementDistance = 8f;

    [Header("First Person Assist")]
    [SerializeField] bool useFirstPersonTargetAssist = false;
    [SerializeField] float firstPersonTargetAssistAngle = 5f;

    [Header("Third Person Assist (Fortnite-style)")]
    [SerializeField] bool useThirdPersonTargetAssist = true;
    [SerializeField] float thirdPersonTargetAssistAngle = 15f;

    [Header("Layers")]
    [SerializeField] LayerMask targetLayerMask = ~0;
    [SerializeField] LayerMask obstacleLayerMask = ~0;

    Camera viewCamera;
    PlayerCameraController cameraController;
    PlayerPickupTargeting pickupTargeting;
    WorldPickupItem worldPickup;
    PlacedPickupable placedPickup;
    CraftingStation craftingStation;
    ToolInteractable toolInteractable;
    Renderer highlightedRenderer;
    Color highlightedOriginalEmission;
    bool highlightedHadEmission;

    public float InteractRange => Mathf.Max(maxPickupDistance, maxInteractionDistance);
    public float MaxPickupDistance => maxPickupDistance;
    public float MaxInteractionDistance => maxInteractionDistance;
    public float MaxPlacementDistance => maxPlacementDistance;
    public WorldPickupItem WorldPickup => worldPickup;
    public PlacedPickupable PlacedPickup => placedPickup;
    public CraftingStation CraftingStation => craftingStation;
    public ToolInteractable ToolInteractable => toolInteractable;
    public bool HasToolInteractable => toolInteractable != null && !toolInteractable.IsCompleted;
    public bool HasWorldPickup => worldPickup != null && !worldPickup.IsCollected;
    public bool CanPickupWorldTarget => pickupTargeting != null && pickupTargeting.CanPickupSelected;
    public bool HasIdentifiedWorldPickup => HasWorldPickup;
    public bool HasPlacedPickup => placedPickup != null;
    public bool HasCraftingStation => craftingStation != null;
    public bool HasAnyTarget => HasWorldPickup || HasPlacedPickup || HasCraftingStation || HasToolInteractable;
    public bool IsWorldPickupTooFar => HasWorldPickup && !CanPickupWorldTarget;
    public PlayerPickupTargeting PickupTargeting => pickupTargeting;

    void Awake()
    {
        viewCamera = CrosshairRayUtility.ResolveGameplayCamera();
        cameraController = FindFirstObjectByType<PlayerCameraController>();

        pickupTargeting = GetComponent<PlayerPickupTargeting>();
        if (pickupTargeting == null)
        {
            pickupTargeting = gameObject.AddComponent<PlayerPickupTargeting>();
        }
    }

    void LateUpdate()
    {
        viewCamera = CrosshairRayUtility.ResolveGameplayCamera();

        if (ShouldBlockTargeting())
        {
            ClearTargets();
            pickupTargeting?.ClearSelection();
            ClearHighlight();
            return;
        }

        UpdateTargets();
        UpdateHighlight();
    }

    bool ShouldBlockTargeting()
    {
        return GameplayCursorPolicy.IsAnyMenuOpen;
    }

    public Ray GetCenterScreenRay()
    {
        return CrosshairRayUtility.GetCrosshairRay(out _);
    }

    public Vector3 GetAimForward()
    {
        Ray crosshairRay = GetCenterScreenRay();
        return crosshairRay.direction.normalized;
    }

    void UpdateTargets()
    {
        worldPickup = null;
        placedPickup = null;
        craftingStation = null;
        toolInteractable = null;

        if (pickupTargeting != null)
        {
            pickupTargeting.UpdatePickupTarget();
            worldPickup = pickupTargeting.SelectedPickup;
        }

        Ray crosshairRay = GetCenterScreenRay();
        float rayRange = GetRaycastRange();
        if (TryResolveInteractionRaycast(crosshairRay, rayRange, out placedPickup, out craftingStation, out toolInteractable))
        {
            FinalizeTargetPriority();
            FilterTargetsByDistance();
            return;
        }

        if (ShouldUseAssist())
        {
            TryResolveInteractionConeBias(GetAssistAngle(), out placedPickup, out craftingStation, out toolInteractable);
            FinalizeTargetPriority();
        }

        FilterTargetsByDistance();
    }

    public string GetWorldPickupPrompt(bool inventoryCanAccept)
    {
        if (pickupTargeting != null)
        {
            return pickupTargeting.GetPickupPrompt(inventoryCanAccept);
        }

        if (!HasWorldPickup)
        {
            return string.Empty;
        }

        string itemName = ItemKindUtility.GetDisplayName(worldPickup.ItemKind);
        return inventoryCanAccept
            ? $"按 F 拾取：{itemName}"
            : $"按 F 拾取：{itemName}（背包已满）";
    }

    void FilterTargetsByDistance()
    {
        // World pickup range is validated by PlayerPickupTargeting (camera distance + player reach).

        if (placedPickup != null && GetHorizontalDistance(placedPickup.transform) > maxInteractionDistance)
        {
            placedPickup = null;
        }

        if (craftingStation != null && GetHorizontalDistance(craftingStation.transform) > maxInteractionDistance)
        {
            craftingStation = null;
        }

        if (toolInteractable != null && GetHorizontalDistance(toolInteractable.transform) > maxInteractionDistance)
        {
            toolInteractable = null;
        }
    }

    float GetHorizontalDistance(Transform targetTransform)
    {
        Vector3 flat = targetTransform.position - transform.position;
        flat.y = 0f;
        return flat.magnitude;
    }

    public static string GetStationDisplayName(CraftingStation station)
    {
        if (station == null)
        {
            return string.Empty;
        }

        var placed = station.GetComponentInParent<PlacedPickupable>();
        if (placed != null && ItemKindUtility.IsValid(placed.ItemKind))
        {
            return ItemKindUtility.GetDisplayName(placed.ItemKind);
        }

        return WorkstationDetector.GetColumnTitle(station.WorkstationKind);
    }

    void FinalizeTargetPriority()
    {
        if (placedPickup != null)
        {
            worldPickup = null;
        }

        if (toolInteractable != null && HasEquippedToolSelected())
        {
            worldPickup = null;
            placedPickup = null;
            craftingStation = null;
        }
    }

    bool HasEquippedToolSelected()
    {
        var inventory = GetComponent<PlayerInventory>();
        if (inventory == null)
        {
            return false;
        }

        InventorySlotData selected = inventory.GetHotbarSlot(inventory.SelectedHotbarIndex);
        return !selected.IsEmpty && ItemKindUtility.IsHandheldTool(selected.Kind);
    }

    float GetRaycastRange()
    {
        return Mathf.Max(maxPickupDistance, maxInteractionDistance, maxPlacementDistance);
    }

    float GetAssistRange()
    {
        return Mathf.Max(maxPickupDistance, maxInteractionDistance);
    }

    float GetCandidateMaxRange(Component candidate)
    {
        if (candidate is WorldPickupItem)
        {
            return maxPickupDistance;
        }

        if (candidate is ToolInteractable)
        {
            return maxInteractionDistance;
        }

        return maxInteractionDistance;
    }

    float GetAssistAngle()
    {
        return IsThirdPersonMode() ? thirdPersonTargetAssistAngle : firstPersonTargetAssistAngle;
    }

    bool ShouldUseAssist()
    {
        if (IsThirdPersonMode())
        {
            return useThirdPersonTargetAssist;
        }

        return useFirstPersonTargetAssist;
    }

    bool IsThirdPersonMode()
    {
        return cameraController == null || !cameraController.IsFirstPerson;
    }

    bool TryResolveInteractionRaycast(
        Ray ray,
        float range,
        out PlacedPickupable placed,
        out CraftingStation station,
        out ToolInteractable toolTarget)
    {
        placed = null;
        station = null;
        toolTarget = null;

        RaycastHit[] hits = Physics.RaycastAll(
            ray,
            range,
            targetLayerMask,
            QueryTriggerInteraction.Collide);

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        return ExtractInteractionTargetsFromHits(hits, ref placed, ref station, ref toolTarget);
    }

    bool TryResolveInteractionConeBias(
        float assistAngle,
        out PlacedPickupable placed,
        out CraftingStation station,
        out ToolInteractable toolTarget)
    {
        placed = null;
        station = null;
        toolTarget = null;

        Vector3 origin = viewCamera != null ? viewCamera.transform.position : transform.position + Vector3.up * 1.4f;
        Vector3 forward = GetAimForward();
        float bestScore = float.MaxValue;
        PlacedPickupable bestPlaced = null;
        CraftingStation bestStation = null;
        ToolInteractable bestTool = null;

        EvaluateConeCandidates(origin, forward, assistAngle, ref bestScore, ref bestPlaced, ref bestStation, ref bestTool, FindObjectsByType<PlacedPickupable>());
        EvaluateConeCandidates(origin, forward, assistAngle, ref bestScore, ref bestPlaced, ref bestStation, ref bestTool, FindObjectsByType<CraftingStation>());
        EvaluateConeCandidates(origin, forward, assistAngle, ref bestScore, ref bestPlaced, ref bestStation, ref bestTool, FindObjectsByType<ToolInteractable>());

        placed = bestPlaced;
        station = bestStation;
        toolTarget = bestTool;
        return placed != null || station != null || toolTarget != null;
    }

    void EvaluateConeCandidates<T>(
        Vector3 origin,
        Vector3 forward,
        float assistAngle,
        ref float bestScore,
        ref PlacedPickupable bestPlaced,
        ref CraftingStation bestStation,
        ref ToolInteractable bestTool,
        T[] candidates) where T : Component
    {
        for (int i = 0; i < candidates.Length; i++)
        {
            T candidate = candidates[i];
            if (candidate == null || !candidate.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (candidate is WorldPickupItem worldCandidate && worldCandidate.IsCollected)
            {
                continue;
            }

            float maxRange = GetCandidateMaxRange(candidate);
            if (GetHorizontalDistance(candidate.transform) > maxRange)
            {
                continue;
            }

            Vector3 targetPoint = GetTargetPoint(candidate.transform);
            Vector3 toTarget = targetPoint - origin;
            float distance = toTarget.magnitude;
            if (distance > GetAssistRange() + 1f || distance < 0.05f)
            {
                continue;
            }

            Vector3 direction = toTarget / distance;
            float angle = Vector3.Angle(forward, direction);
            if (angle > assistAngle)
            {
                continue;
            }

            if (Vector3.Dot(forward, direction) <= 0.05f)
            {
                continue;
            }

            if (IsBehindPlayer(targetPoint))
            {
                continue;
            }

            if (!HasLineOfSight(origin, targetPoint, candidate.transform))
            {
                continue;
            }

            float score = angle * 2f + distance * 0.12f;
            if (score >= bestScore)
            {
                continue;
            }

            if (candidate is PlacedPickupable placedCandidate)
            {
                bestPlaced = placedCandidate;
                bestStation = null;
                bestScore = score;
            }
            else if (candidate is CraftingStation stationCandidate)
            {
                if (bestPlaced != null || bestTool != null)
                {
                    continue;
                }

                bestStation = stationCandidate;
                bestScore = score;
            }
            else if (candidate is ToolInteractable toolCandidate)
            {
                if (toolCandidate.IsCompleted)
                {
                    continue;
                }

                if (bestPlaced != null)
                {
                    continue;
                }

                bestTool = toolCandidate;
                bestStation = null;
                bestScore = score;
            }
        }
    }

    static Vector3 GetTargetPoint(Transform targetTransform)
    {
        Collider col = targetTransform.GetComponentInChildren<Collider>();
        if (col != null)
        {
            return col.bounds.center;
        }

        return targetTransform.position + Vector3.up * 0.5f;
    }

    bool IsBehindPlayer(Vector3 worldPoint)
    {
        Vector3 toTarget = worldPoint - transform.position;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude < 0.01f)
        {
            return false;
        }

        Vector3 playerForward = transform.forward;
        playerForward.y = 0f;
        playerForward.Normalize();
        return Vector3.Dot(playerForward, toTarget.normalized) < -0.15f;
    }

    bool HasLineOfSight(Vector3 origin, Vector3 targetPoint, Transform ignoreRoot)
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
            if (hitCollider == null || IsPlayerCollider(hitCollider))
            {
                continue;
            }

            if (ignoreRoot != null && hitCollider.transform.IsChildOf(ignoreRoot))
            {
                return true;
            }

            if (hitCollider.GetComponentInParent<WorldPickupItem>() != null
                || hitCollider.GetComponentInParent<PlacedPickupable>() != null
                || hitCollider.GetComponentInParent<CraftingStation>() != null
                || hitCollider.GetComponentInParent<ToolInteractable>() != null)
            {
                return ignoreRoot != null && hitCollider.transform.IsChildOf(ignoreRoot);
            }

            return false;
        }

        return true;
    }

    static bool ExtractInteractionTargetsFromHits(
        RaycastHit[] hits,
        ref PlacedPickupable placed,
        ref CraftingStation station,
        ref ToolInteractable toolTarget)
    {
        bool found = false;
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider == null || IsPlayerCollider(hits[i].collider))
            {
                continue;
            }

            var toolCandidate = hits[i].collider.GetComponentInParent<ToolInteractable>();
            if (toolCandidate != null && !toolCandidate.IsCompleted && toolTarget == null)
            {
                toolTarget = toolCandidate;
                found = true;
                continue;
            }

            var placedCandidate = hits[i].collider.GetComponentInParent<PlacedPickupable>();
            if (placedCandidate != null && placed == null)
            {
                placed = placedCandidate;
                found = true;
                continue;
            }

            var stationCandidate = hits[i].collider.GetComponentInParent<CraftingStation>();
            if (stationCandidate != null && station == null)
            {
                station = stationCandidate;
                found = true;
            }
        }

        return found;
    }

    static bool IsPlayerCollider(Collider collider)
    {
        return collider.GetComponentInParent<PlayerController>() != null
            || collider.GetComponentInParent<CharacterController>() != null;
    }

    void ClearTargets()
    {
        worldPickup = null;
        placedPickup = null;
        craftingStation = null;
        toolInteractable = null;
        pickupTargeting?.ClearSelection();
    }

    public string GetPrimaryTargetName()
    {
        if (HasToolInteractable)
        {
            return toolInteractable.DisplayNameChinese;
        }

        if (HasPlacedPickup)
        {
            return ItemKindUtility.GetDisplayName(placedPickup.ItemKind);
        }

        if (HasWorldPickup)
        {
            return ItemKindUtility.GetDisplayName(worldPickup.ItemKind);
        }

        if (HasCraftingStation)
        {
            return GetStationDisplayName(craftingStation);
        }

        return string.Empty;
    }

    void UpdateHighlight()
    {
        Renderer nextRenderer = null;
        Color highlightColor = Color.white;

        if (HasPlacedPickup)
        {
            nextRenderer = placedPickup.GetComponentInChildren<Renderer>();
            highlightColor = ItemKindUtility.GetDisplayColor(placedPickup.ItemKind) * 0.55f;
        }
        else if (HasToolInteractable)
        {
            nextRenderer = toolInteractable.GetComponentInChildren<Renderer>();
            highlightColor = new Color(0.95f, 0.78f, 0.32f);
        }
        else if (HasWorldPickup && CanPickupWorldTarget)
        {
            nextRenderer = worldPickup.GetComponentInChildren<Renderer>();
            highlightColor = ItemKindUtility.GetDisplayColor(worldPickup.ItemKind) * 0.55f;
        }
        else if (HasCraftingStation)
        {
            nextRenderer = craftingStation.GetComponentInChildren<Renderer>();
            highlightColor = new Color(0.45f, 0.75f, 0.95f);
        }

        if (nextRenderer == highlightedRenderer)
        {
            return;
        }

        ClearHighlight();
        if (nextRenderer == null || nextRenderer.sharedMaterial == null)
        {
            return;
        }

        highlightedRenderer = nextRenderer;
        highlightedHadEmission = highlightedRenderer.sharedMaterial.IsKeywordEnabled("_EMISSION");
        highlightedOriginalEmission = highlightedRenderer.sharedMaterial.GetColor("_EmissionColor");
        highlightedRenderer.sharedMaterial.EnableKeyword("_EMISSION");
        highlightedRenderer.sharedMaterial.SetColor("_EmissionColor", highlightColor);
    }

    void ClearHighlight()
    {
        if (highlightedRenderer == null || highlightedRenderer.sharedMaterial == null)
        {
            highlightedRenderer = null;
            return;
        }

        highlightedRenderer.sharedMaterial.SetColor("_EmissionColor", highlightedOriginalEmission);
        if (!highlightedHadEmission)
        {
            highlightedRenderer.sharedMaterial.DisableKeyword("_EMISSION");
        }

        highlightedRenderer = null;
    }

    void OnDisable()
    {
        ClearHighlight();
    }
}
