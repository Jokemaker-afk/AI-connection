using UnityEngine;

public static class PlacedObjectBuilder
{
    const float GridSize = 1f;
    const float MinUpNormal = 0.65f;

    public static LayerMask PlacementSurfaceMask { get; set; } = ~0;
    public static LayerMask PlacementBlockingMask { get; set; } = ~0;
    public static float PlacementCheckPadding { get; set; } = 0.04f;

    public static Vector3 GetPlacedVisualScale(ItemKind itemKind)
    {
        if (PlacedObjectBoundsUtility.TryGetFittedPlacementSize(itemKind, out Vector3 fittedSize))
        {
            return fittedSize;
        }

        return GetCatalogVisualScale(itemKind);
    }

    public static Vector3 GetPlacementHalfExtents(ItemKind itemKind)
    {
        if (PlacedObjectBoundsUtility.TryGetFittedPlacementHalfExtents(itemKind, out Vector3 halfExtents))
        {
            return halfExtents;
        }

        Vector3 scale = GetCatalogVisualScale(itemKind);
        float pad = PlacementCheckPadding;
        return new Vector3(scale.x * 0.5f - pad, scale.y * 0.5f - pad, scale.z * 0.5f - pad);
    }

    public static GameObject SpawnPlacedObject(ItemKind itemKind, Vector3 position, Quaternion rotation)
    {
        // Prefer ItemModuleFactory.SpawnPlacedObject for new scene/item code.
        if (!ItemKindUtility.IsPlaceable(itemKind))
        {
            return null;
        }

        BuildingKind buildingKind = ItemKindUtility.GetPlacedBuildingKind(itemKind);
        WorkstationKind workstationKind = ItemKindUtility.GetPlacedWorkstationKind(itemKind);
        string displayName = ItemKindUtility.GetDisplayName(itemKind);

        var root = new GameObject($"Placed_{itemKind}");
        root.transform.position = position;
        root.transform.rotation = rotation;

        Vector3 scale = GetCatalogVisualScale(itemKind);
        Vector3 visualOffset = Vector3.up * scale.y * 0.5f;
        ItemVisualBuilder.CreatePlacedVisual(root.transform, itemKind, scale, visualOffset);

        PlacedObjectInteractionSetup.Configure(root, itemKind);

        var surface = root.GetComponent<PlacementSurface>() ?? root.AddComponent<PlacementSurface>();
        surface.Initialize(itemKind);

        var placed = root.GetComponent<PlacedBuilding>() ?? root.AddComponent<PlacedBuilding>();
        placed.Initialize(itemKind, buildingKind, workstationKind);

        if (workstationKind != WorkstationKind.None)
        {
            var station = root.AddComponent<CraftingStation>();
            station.Configure(workstationKind, $"按 E 打开制造（{displayName}）");
            placed.AttachCraftingStation(station);
        }

        ItemWorldLabel.Create(root.transform, displayName, Vector3.up * (scale.y * 0.65f + 0.2f), 0.08f);
        return root;
    }

    public static Vector3 GetCatalogVisualScale(ItemKind itemKind)
    {
        if (ItemCatalog.TryGet(itemKind, out ItemData data) && data.IsValid && data.PlacedBoundsSize.sqrMagnitude > 0.01f)
        {
            return data.PlacedBoundsSize;
        }

        switch (ItemKindUtility.GetCategory(itemKind))
        {
            case ItemCategory.BuildingMaterial:
                return new Vector3(1f, 1f, 1f);
            case ItemCategory.Storage:
                return new Vector3(1.2f, 0.8f, 0.9f);
            case ItemCategory.Workstation:
                return new Vector3(1.6f, 0.9f, 1.2f);
            default:
                return new Vector3(1f, 1f, 1f);
        }
    }

    public static bool TryGetPlacementPosition(Ray ray, ItemKind placingItem, out Vector3 position, out Vector3 normal, out Collider supportCollider)
    {
        position = Vector3.zero;
        normal = Vector3.up;
        supportCollider = null;

        RaycastHit[] hits = Physics.RaycastAll(
            ray,
            20f,
            PlacementSurfaceMask,
            QueryTriggerInteraction.Ignore);

        if (hits == null || hits.Length == 0)
        {
            return false;
        }

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];
            if (hit.collider == null || IsPlayerCollider(hit.collider))
            {
                continue;
            }

            if (TryResolveSurface(hit, placingItem, out float surfaceY, out Vector3 surfaceNormal))
            {
                position = SnapHorizontal(hit.point, surfaceY);
                normal = surfaceNormal;
                supportCollider = hit.collider;
                return true;
            }
        }

        return false;
    }

    static bool TryResolveSurface(RaycastHit hit, ItemKind placingItem, out float surfaceY, out Vector3 surfaceNormal)
    {
        surfaceY = 0f;
        surfaceNormal = hit.normal;

        var placementSurface = hit.collider.GetComponentInParent<PlacementSurface>();
        if (placementSurface != null
            && placementSurface.SupportsTopPlacement
            && ItemKindUtility.CanBePlacedOnTop(placingItem)
            && hit.normal.y > 0.35f)
        {
            surfaceY = placementSurface.GetTopSurfaceY();
            surfaceNormal = Vector3.up;
            return true;
        }

        if (hit.normal.y >= MinUpNormal)
        {
            surfaceY = hit.collider.bounds.max.y;
            if (Mathf.Abs(hit.point.y - surfaceY) < 0.35f)
            {
                surfaceY = hit.point.y;
            }

            return true;
        }

        if (hit.collider.GetComponentInParent<StaticPlacementGround>() != null && hit.normal.y > 0.35f)
        {
            surfaceY = hit.collider.bounds.max.y;
            return true;
        }

        return false;
    }

    static Vector3 SnapHorizontal(Vector3 worldPoint, float bottomY)
    {
        return new Vector3(
            Mathf.Round(worldPoint.x / GridSize) * GridSize,
            bottomY,
            Mathf.Round(worldPoint.z / GridSize) * GridSize);
    }

    public static bool IsPlacementValid(ItemKind itemKind, Vector3 position, Collider ignoreSupportCollider = null)
    {
        if (!ItemKindUtility.IsPlaceable(itemKind))
        {
            return false;
        }

        Vector3 scale = GetPlacedVisualScale(itemKind);
        Vector3 halfExtents = GetPlacementHalfExtents(itemKind);
        Vector3 center = position + Vector3.up * scale.y * 0.5f;

        Collider[] overlaps = Physics.OverlapBox(
            center,
            halfExtents,
            Quaternion.identity,
            PlacementBlockingMask,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < overlaps.Length; i++)
        {
            Collider overlap = overlaps[i];
            if (overlap == null || overlap.isTrigger)
            {
                continue;
            }

            if (ignoreSupportCollider != null && overlap == ignoreSupportCollider)
            {
                continue;
            }

            if (ignoreSupportCollider != null && overlap.transform.IsChildOf(ignoreSupportCollider.transform))
            {
                continue;
            }

            if (IsPlayerCollider(overlap))
            {
                return false;
            }

            if (overlap.GetComponentInParent<WorldPickupItem>() != null)
            {
                continue;
            }

            if (overlap.GetComponentInParent<StaticPlacementGround>() != null)
            {
                continue;
            }

            if (overlap.GetComponentInParent<PlacedPickupable>() != null
                || overlap.GetComponentInParent<PlacedBuilding>() != null)
            {
                return false;
            }

            var placementSurface = overlap.GetComponentInParent<PlacementSurface>();
            if (placementSurface != null && placementSurface.SupportsTopPlacement)
            {
                return false;
            }
        }

        return true;
    }

    static bool IsPlayerCollider(Collider collider)
    {
        return collider.GetComponentInParent<PlayerController>() != null
            || collider.GetComponentInParent<CharacterController>() != null;
    }
}
