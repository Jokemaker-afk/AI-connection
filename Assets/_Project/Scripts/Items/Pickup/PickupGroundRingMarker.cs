using UnityEngine;

public static class PickupGroundRingMarker
{
    const float RingThickness = 0.012f;
    const float DefaultFloorY = 0.25f;

    public static GameObject Create(Transform parent, string objectName, Color color, float diameter = 0.78f)
    {
        var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = objectName;
        if (parent != null)
        {
            ring.transform.SetParent(parent, false);
        }

        ring.transform.localRotation = Quaternion.identity;
        ring.transform.localScale = new Vector3(diameter, RingThickness, diameter);

        Collider collider = ring.GetComponent<Collider>();
        if (collider != null)
        {
            Object.Destroy(collider);
        }

        Renderer renderer = ring.GetComponent<Renderer>();
        if (renderer != null)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.renderQueue = 3000;
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.color = color;
            material.SetColor("_EmissionColor", color * 1.35f);
            material.EnableKeyword("_EMISSION");
            renderer.sharedMaterial = material;
        }

        return ring;
    }

    public static void PlaceOnGroundUnder(Transform ringTransform, Vector3 sampleOrigin, Transform ignoreRoot)
    {
        if (ringTransform == null)
        {
            return;
        }

        if (TryFindGroundPoint(sampleOrigin, ignoreRoot, out Vector3 groundPoint))
        {
            float ringCenterY = groundPoint.y + GetRingHalfHeight(ringTransform);
            ringTransform.position = new Vector3(groundPoint.x, ringCenterY, groundPoint.z);
        }
        else
        {
            float fallbackY = GetFallbackGroundY(sampleOrigin, ignoreRoot) + GetRingHalfHeight(ringTransform);
            ringTransform.position = new Vector3(sampleOrigin.x, fallbackY, sampleOrigin.z);
        }

        ringTransform.rotation = Quaternion.identity;
    }

    public static bool TryFindGroundPoint(Vector3 sampleOrigin, Transform ignoreRoot, out Vector3 groundPoint)
    {
        Vector3 rayOrigin = new Vector3(sampleOrigin.x, sampleOrigin.y + 12f, sampleOrigin.z);
        RaycastHit[] hits = Physics.RaycastAll(
            rayOrigin,
            Vector3.down,
            24f,
            ~0,
            QueryTriggerInteraction.Ignore);

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        for (int i = 0; i < hits.Length; i++)
        {
            if (ShouldIgnoreGroundHit(hits[i].collider, ignoreRoot))
            {
                continue;
            }

            groundPoint = hits[i].point;
            return true;
        }

        groundPoint = default;
        return false;
    }

    static bool ShouldIgnoreGroundHit(Collider collider, Transform ignoreRoot)
    {
        if (collider == null)
        {
            return true;
        }

        if (ignoreRoot != null && collider.transform.IsChildOf(ignoreRoot))
        {
            return true;
        }

        if (collider.GetComponentInParent<WorldPickupItem>() != null)
        {
            return true;
        }

        if (collider.GetComponentInParent<BillboardLabel>() != null)
        {
            return true;
        }

        string objectName = collider.gameObject.name;
        if (objectName == "PickableGroundRing" || objectName == "Label")
        {
            return true;
        }

        return false;
    }

    static float GetFallbackGroundY(Vector3 sampleOrigin, Transform ignoreRoot)
    {
        if (ignoreRoot != null)
        {
            var pickup = ignoreRoot.GetComponent<WorldPickupItem>();
            if (pickup != null)
            {
                return pickup.GetEstimatedGroundY();
            }

            Renderer renderer = ignoreRoot.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                return renderer.bounds.min.y - 0.08f;
            }
        }

        return DefaultFloorY;
    }

    static float GetRingHalfHeight(Transform ringTransform)
    {
        // Unity cylinder primitive height is 2 units.
        return ringTransform.lossyScale.y;
    }
}
