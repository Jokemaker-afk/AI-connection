using UnityEngine;

/// <summary>Shared child names for hazard scene module prefabs (Spike, ElectricField, etc.).</summary>
public static class HazardModuleLayoutUtility
{
    // Layout helpers shared by SpikeTrap and LaserHazard modules.
    public const string WarningAreaName = "WarningArea";
    public const string LabelAnchorName = "LabelAnchor";
    public const string DebugBoundsName = "DebugBounds";

    public static Transform EnsureChild(Transform parent, string childName)
    {
        Transform child = parent.Find(childName);
        if (child != null)
        {
            return child;
        }

        var childGo = new GameObject(childName);
        child = childGo.transform;
        child.SetParent(parent, false);
        child.localPosition = Vector3.zero;
        child.localRotation = Quaternion.identity;
        child.localScale = Vector3.one;
        return child;
    }

    public static void BuildWarningAreaPlane(Transform warningRoot, Vector3 footprintSize, Color tint)
    {
        BuildWarningAreaPlane(warningRoot, Vector3.zero, footprintSize.x, footprintSize.z, 0.025f, 0f, tint);
    }

    public static void BuildWarningAreaPlane(
        Transform warningRoot,
        Vector3 centerInHazardLocal,
        Vector3 footprintSize,
        Color tint)
    {
        BuildWarningAreaPlane(
            warningRoot,
            centerInHazardLocal,
            footprintSize.x,
            footprintSize.z,
            0.025f,
            0f,
            tint);
    }

    public static void BuildWarningAreaPlane(
        Transform warningRoot,
        Vector3 centerInHazardLocal,
        float footprintSizeX,
        float footprintSizeZ,
        float groundOffset,
        float padding,
        Color tint)
    {
        if (warningRoot == null)
        {
            return;
        }

        ClearChildren(warningRoot);

        float sizeX = Mathf.Max(0.05f, footprintSizeX + padding * 2f);
        float sizeZ = Mathf.Max(0.05f, footprintSizeZ + padding * 2f);

        var plane = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plane.name = "WarningPlane";
        plane.transform.SetParent(warningRoot, false);
        plane.transform.localPosition = new Vector3(centerInHazardLocal.x, groundOffset, centerInHazardLocal.z);
        plane.transform.localRotation = Quaternion.identity;
        plane.transform.localScale = new Vector3(sizeX, 0.02f, sizeZ);
        DestroyColliderSafe(plane.GetComponent<Collider>());

        var renderer = plane.GetComponent<Renderer>();
        if (renderer != null)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            material.color = tint;
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", new Color(tint.r, tint.g, tint.b) * 0.4f);
            renderer.sharedMaterial = material;
        }
    }

    /// <summary>Ground footprint (hazard-local X/Z) from a box collider's oriented volume.</summary>
    public static bool TryCalculateGroundFootprintFromBox(
        BoxCollider box,
        Transform hazardRoot,
        out Vector3 centerInHazardLocal,
        out float footprintSizeX,
        out float footprintSizeZ)
    {
        centerInHazardLocal = Vector3.zero;
        footprintSizeX = 0f;
        footprintSizeZ = 0f;

        if (box == null || hazardRoot == null)
        {
            return false;
        }

        Transform boxTransform = box.transform;
        Vector3 center = box.center;
        Vector3 half = box.size * 0.5f;

        bool initialized = false;
        float minX = 0f;
        float maxX = 0f;
        float minZ = 0f;
        float maxZ = 0f;

        AppendGroundCorner(boxTransform, hazardRoot, center, half, ref initialized, ref minX, ref maxX, ref minZ, ref maxZ, 1, 1, 1);
        AppendGroundCorner(boxTransform, hazardRoot, center, half, ref initialized, ref minX, ref maxX, ref minZ, ref maxZ, 1, 1, -1);
        AppendGroundCorner(boxTransform, hazardRoot, center, half, ref initialized, ref minX, ref maxX, ref minZ, ref maxZ, 1, -1, 1);
        AppendGroundCorner(boxTransform, hazardRoot, center, half, ref initialized, ref minX, ref maxX, ref minZ, ref maxZ, 1, -1, -1);
        AppendGroundCorner(boxTransform, hazardRoot, center, half, ref initialized, ref minX, ref maxX, ref minZ, ref maxZ, -1, 1, 1);
        AppendGroundCorner(boxTransform, hazardRoot, center, half, ref initialized, ref minX, ref maxX, ref minZ, ref maxZ, -1, 1, -1);
        AppendGroundCorner(boxTransform, hazardRoot, center, half, ref initialized, ref minX, ref maxX, ref minZ, ref maxZ, -1, -1, 1);
        AppendGroundCorner(boxTransform, hazardRoot, center, half, ref initialized, ref minX, ref maxX, ref minZ, ref maxZ, -1, -1, -1);

        if (!initialized)
        {
            return false;
        }

        footprintSizeX = maxX - minX;
        footprintSizeZ = maxZ - minZ;
        centerInHazardLocal = new Vector3((minX + maxX) * 0.5f, 0f, (minZ + maxZ) * 0.5f);
        return footprintSizeX > 0.0001f || footprintSizeZ > 0.0001f;
    }

    /// <summary>Ground footprint from renderers projected into hazard-local X/Z.</summary>
    public static bool TryCalculateGroundFootprintFromRenderers(
        Transform visualRoot,
        Transform hazardRoot,
        out Vector3 centerInHazardLocal,
        out float footprintSizeX,
        out float footprintSizeZ)
    {
        centerInHazardLocal = Vector3.zero;
        footprintSizeX = 0f;
        footprintSizeZ = 0f;

        if (visualRoot == null || hazardRoot == null)
        {
            return false;
        }

        Renderer[] renderers = visualRoot.GetComponentsInChildren<Renderer>(true);
        if (renderers == null || renderers.Length == 0)
        {
            return false;
        }

        bool initialized = false;
        float minX = 0f;
        float maxX = 0f;
        float minZ = 0f;
        float maxZ = 0f;

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null)
            {
                continue;
            }

            Bounds worldBounds = renderer.bounds;
            Vector3 c = worldBounds.center;
            Vector3 e = worldBounds.extents;

            AppendGroundWorldCorner(hazardRoot, c + new Vector3(e.x, e.y, e.z), ref initialized, ref minX, ref maxX, ref minZ, ref maxZ);
            AppendGroundWorldCorner(hazardRoot, c + new Vector3(e.x, e.y, -e.z), ref initialized, ref minX, ref maxX, ref minZ, ref maxZ);
            AppendGroundWorldCorner(hazardRoot, c + new Vector3(e.x, -e.y, e.z), ref initialized, ref minX, ref maxX, ref minZ, ref maxZ);
            AppendGroundWorldCorner(hazardRoot, c + new Vector3(e.x, -e.y, -e.z), ref initialized, ref minX, ref maxX, ref minZ, ref maxZ);
            AppendGroundWorldCorner(hazardRoot, c + new Vector3(-e.x, e.y, e.z), ref initialized, ref minX, ref maxX, ref minZ, ref maxZ);
            AppendGroundWorldCorner(hazardRoot, c + new Vector3(-e.x, e.y, -e.z), ref initialized, ref minX, ref maxX, ref minZ, ref maxZ);
            AppendGroundWorldCorner(hazardRoot, c + new Vector3(-e.x, -e.y, e.z), ref initialized, ref minX, ref maxX, ref minZ, ref maxZ);
            AppendGroundWorldCorner(hazardRoot, c + new Vector3(-e.x, -e.y, -e.z), ref initialized, ref minX, ref maxX, ref minZ, ref maxZ);
        }

        if (!initialized)
        {
            return false;
        }

        footprintSizeX = maxX - minX;
        footprintSizeZ = maxZ - minZ;
        centerInHazardLocal = new Vector3((minX + maxX) * 0.5f, 0f, (minZ + maxZ) * 0.5f);
        return footprintSizeX > 0.0001f || footprintSizeZ > 0.0001f;
    }

    static void AppendGroundCorner(
        Transform boxTransform,
        Transform hazardRoot,
        Vector3 center,
        Vector3 half,
        ref bool initialized,
        ref float minX,
        ref float maxX,
        ref float minZ,
        ref float maxZ,
        int sx,
        int sy,
        int sz)
    {
        Vector3 localCorner = center + new Vector3(half.x * sx, half.y * sy, half.z * sz);
        Vector3 worldCorner = boxTransform.TransformPoint(localCorner);
        AppendGroundWorldCorner(hazardRoot, worldCorner, ref initialized, ref minX, ref maxX, ref minZ, ref maxZ);
    }

    static void AppendGroundWorldCorner(
        Transform hazardRoot,
        Vector3 worldCorner,
        ref bool initialized,
        ref float minX,
        ref float maxX,
        ref float minZ,
        ref float maxZ)
    {
        Vector3 hazardLocal = hazardRoot.InverseTransformPoint(worldCorner);
        if (!initialized)
        {
            minX = maxX = hazardLocal.x;
            minZ = maxZ = hazardLocal.z;
            initialized = true;
            return;
        }

        minX = Mathf.Min(minX, hazardLocal.x);
        maxX = Mathf.Max(maxX, hazardLocal.x);
        minZ = Mathf.Min(minZ, hazardLocal.z);
        maxZ = Mathf.Max(maxZ, hazardLocal.z);
    }

    public static void EnsureLabelAnchor(Transform hazardRoot, float height = 1.2f)
    {
        Transform anchor = EnsureChild(hazardRoot, LabelAnchorName);
        anchor.localPosition = new Vector3(0f, height, 0f);
    }

    public static void SyncDebugBounds(Transform hazardRoot, Vector3 colliderCenter, Vector3 colliderSize, bool visible)
    {
        Transform debugRoot = EnsureChild(hazardRoot, DebugBoundsName);
        debugRoot.gameObject.SetActive(visible);

        if (!visible)
        {
            return;
        }

        ClearChildren(debugRoot);
        var wire = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wire.name = "ColliderPreview";
        wire.transform.SetParent(debugRoot, false);
        wire.transform.localPosition = colliderCenter;
        wire.transform.localScale = colliderSize;
        DestroyColliderSafe(wire.GetComponent<Collider>());

        var renderer = wire.GetComponent<Renderer>();
        if (renderer != null)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(0.2f, 0.85f, 1f, 0.15f);
            renderer.sharedMaterial = material;
        }
    }

    public static void SyncDebugBounds(Transform hazardRoot, Vector3 colliderSize, bool visible)
    {
        SyncDebugBounds(hazardRoot, Vector3.zero, colliderSize, visible);
    }

    public static bool TryCalculateLocalBoundsFromRenderers(
        Transform visualRoot,
        Transform localSpace,
        out Bounds localBounds)
    {
        localBounds = default;
        if (visualRoot == null || localSpace == null)
        {
            return false;
        }

        Renderer[] renderers = visualRoot.GetComponentsInChildren<Renderer>(true);
        if (renderers == null || renderers.Length == 0)
        {
            return false;
        }

        bool initialized = false;
        Vector3 min = Vector3.zero;
        Vector3 max = Vector3.zero;

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null)
            {
                continue;
            }

            Bounds worldBounds = renderer.bounds;
            Vector3 center = worldBounds.center;
            Vector3 extents = worldBounds.extents;

            AppendCorner(ref initialized, ref min, ref max, localSpace, center + new Vector3(extents.x, extents.y, extents.z));
            AppendCorner(ref initialized, ref min, ref max, localSpace, center + new Vector3(extents.x, extents.y, -extents.z));
            AppendCorner(ref initialized, ref min, ref max, localSpace, center + new Vector3(extents.x, -extents.y, extents.z));
            AppendCorner(ref initialized, ref min, ref max, localSpace, center + new Vector3(extents.x, -extents.y, -extents.z));
            AppendCorner(ref initialized, ref min, ref max, localSpace, center + new Vector3(-extents.x, extents.y, extents.z));
            AppendCorner(ref initialized, ref min, ref max, localSpace, center + new Vector3(-extents.x, extents.y, -extents.z));
            AppendCorner(ref initialized, ref min, ref max, localSpace, center + new Vector3(-extents.x, -extents.y, extents.z));
            AppendCorner(ref initialized, ref min, ref max, localSpace, center + new Vector3(-extents.x, -extents.y, -extents.z));
        }

        if (!initialized)
        {
            return false;
        }

        Vector3 size = max - min;
        localBounds = new Bounds(min + size * 0.5f, size);
        return size.sqrMagnitude > 0.0001f;
    }

    static void AppendCorner(
        ref bool initialized,
        ref Vector3 min,
        ref Vector3 max,
        Transform localSpace,
        Vector3 worldCorner)
    {
        Vector3 local = localSpace.InverseTransformPoint(worldCorner);
        if (!initialized)
        {
            min = local;
            max = local;
            initialized = true;
            return;
        }

        min = Vector3.Min(min, local);
        max = Vector3.Max(max, local);
    }

    public static void SetRenderersActive(Transform root, bool active, float inactiveDim = 0.25f)
    {
        if (root == null)
        {
            return;
        }

        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null)
            {
                continue;
            }

            renderer.enabled = active;
            if (!active && renderer.material != null && renderer.material.HasProperty("_BaseColor"))
            {
                Color color = renderer.material.color;
                color.a *= inactiveDim;
                renderer.material.color = color;
            }
        }
    }

    static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            if (child != null)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(child.gameObject);
                }
                else
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }
        }
    }

    static void DestroyColliderSafe(Collider collider)
    {
        if (collider == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Object.Destroy(collider);
        }
        else
        {
            Object.DestroyImmediate(collider);
        }
    }
}
