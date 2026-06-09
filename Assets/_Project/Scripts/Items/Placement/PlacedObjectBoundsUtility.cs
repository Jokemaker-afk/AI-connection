using UnityEngine;

/// <summary>Computes local bounds from visual renderers for placed buildables/workstations.</summary>
public static class PlacedObjectBoundsUtility
{
    public const string InteractionColliderName = "InteractionCollider";

    public static Transform ResolveVisualRoot(Transform root)
    {
        if (root == null)
        {
            return null;
        }

        Transform visual = root.Find(SceneModuleVisualUtility.VisualName);
        if (visual == null)
        {
            visual = root.Find(SceneModuleVisualUtility.VisualRootName);
        }

        return visual != null ? visual : root;
    }

    public static bool ShouldSkipRenderer(Renderer renderer)
    {
        if (renderer == null)
        {
            return true;
        }

        Transform t = renderer.transform;
        while (t != null)
        {
            string name = t.name;
            if (name == "SelectionOutline"
                || name == "OutlineMesh"
                || name == InteractionColliderName)
            {
                return true;
            }

            t = t.parent;
        }

        return false;
    }

    public static bool TryCalculateLocalRendererBounds(Transform objectRoot, Transform visualRoot, out Bounds localBounds)
    {
        localBounds = default;
        if (objectRoot == null || visualRoot == null)
        {
            return false;
        }

        Renderer[] renderers = visualRoot.GetComponentsInChildren<Renderer>(true);
        bool found = false;
        Bounds worldBounds = default;

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (ShouldSkipRenderer(renderer))
            {
                continue;
            }

            if (!found)
            {
                worldBounds = renderer.bounds;
                found = true;
            }
            else
            {
                worldBounds.Encapsulate(renderer.bounds);
            }
        }

        if (!found)
        {
            return false;
        }

        localBounds = WorldBoundsToLocalBounds(objectRoot, worldBounds);
        return localBounds.size.sqrMagnitude > 0.0001f;
    }

    public static Bounds WorldBoundsToLocalBounds(Transform localSpace, Bounds worldBounds)
    {
        Vector3 center = worldBounds.center;
        Vector3 extents = worldBounds.extents;

        Vector3 min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        Vector3 max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

        for (int x = -1; x <= 1; x += 2)
        {
            for (int y = -1; y <= 1; y += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    Vector3 corner = center + Vector3.Scale(extents, new Vector3(x, y, z));
                    Vector3 local = localSpace.InverseTransformPoint(corner);
                    min = Vector3.Min(min, local);
                    max = Vector3.Max(max, local);
                }
            }
        }

        return new Bounds((min + max) * 0.5f, max - min);
    }

    public static Bounds ApplyPadding(Bounds bounds, Vector3 padding)
    {
        bounds.size = new Vector3(
            bounds.size.x + padding.x * 2f,
            bounds.size.y + padding.y * 2f,
            bounds.size.z + padding.z * 2f);
        return bounds;
    }

    public static Bounds GetFallbackLocalBounds(ItemKind kind)
    {
        if (ItemCatalog.TryGet(kind, out ItemData data) && data.PlacedBoundsSize.sqrMagnitude > 0.01f)
        {
            Vector3 size = data.PlacedBoundsSize;
            return new Bounds(Vector3.up * size.y * 0.5f, size);
        }

        Vector3 scale = PlacedObjectBuilder.GetCatalogVisualScale(kind);
        return new Bounds(Vector3.up * scale.y * 0.5f, scale);
    }

    public static bool TryResolvePlacedPrefab(ItemKind kind, out GameObject prefab)
    {
        prefab = null;
        if (!ItemKindUtility.IsValid(kind))
        {
            return false;
        }

        if (ItemCatalog.TryGet(kind, out ItemData data) && data.PlacedPrefab != null)
        {
            prefab = data.PlacedPrefab;
            return true;
        }

        return ItemPlacedPrefabResolver.TryResolve(kind, out prefab) && prefab != null;
    }

    public static bool TryGetFittedPlacementHalfExtents(ItemKind kind, out Vector3 halfExtents)
    {
        halfExtents = default;
        if (!TryResolvePlacedPrefab(kind, out GameObject prefab) || prefab == null)
        {
            return false;
        }

        PlacedObjectColliderFitter fitter = prefab.GetComponent<PlacedObjectColliderFitter>();
        if (fitter != null && fitter.HasValidFootprint)
        {
            halfExtents = fitter.PlacementHalfExtents;
            return true;
        }

        Transform visualRoot = ResolveVisualRoot(prefab.transform);
        if (TryCalculateLocalRendererBounds(prefab.transform, visualRoot, out Bounds localBounds))
        {
            float pad = PlacedObjectBuilder.PlacementCheckPadding;
            halfExtents = new Vector3(
                Mathf.Max(0.05f, localBounds.extents.x - pad),
                Mathf.Max(0.05f, localBounds.extents.y - pad),
                Mathf.Max(0.05f, localBounds.extents.z - pad));
            return true;
        }

        return false;
    }

    public static bool TryGetFittedPlacementSize(ItemKind kind, out Vector3 size)
    {
        if (TryGetFittedPlacementHalfExtents(kind, out Vector3 halfExtents))
        {
            size = halfExtents * 2f;
            return true;
        }

        size = default;
        return false;
    }
}
