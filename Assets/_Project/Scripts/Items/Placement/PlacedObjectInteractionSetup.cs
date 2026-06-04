using UnityEngine;

/// <summary>
/// Ensures placed objects have raycast-friendly colliders and recoverable metadata.
/// </summary>
public static class PlacedObjectInteractionSetup
{
    const float MinColliderHeight = 0.3f;
    const float GroundClearance = 0.03f;
    const float TargetPadding = 0.04f;

    public static void Configure(GameObject root, ItemKind kind)
    {
        if (root == null || !ItemKindUtility.IsValid(kind))
        {
            return;
        }

        Vector3 scale = ResolvePlacedScale(kind);
        EnsureSolidCollider(root, scale);

        var recoverable = root.GetComponent<PlacedPickupable>() ?? root.AddComponent<PlacedPickupable>();
        recoverable.Initialize(kind);

        var highlighter = root.GetComponent<PlacedObjectHighlighter>() ?? root.AddComponent<PlacedObjectHighlighter>();
        highlighter.RefreshFromColliders();
    }

    static Vector3 ResolvePlacedScale(ItemKind kind)
    {
        if (ItemCatalog.TryGet(kind, out ItemData data) && data.PlacedBoundsSize.sqrMagnitude > 0.01f)
        {
            return data.PlacedBoundsSize;
        }

        return PlacedObjectBuilder.GetPlacedVisualScale(kind);
    }

    static void EnsureSolidCollider(GameObject root, Vector3 scale)
    {
        StripChildSolidColliders(root);

        BoxCollider box = root.GetComponent<BoxCollider>();
        if (box == null)
        {
            box = root.AddComponent<BoxCollider>();
        }

        float height = Mathf.Max(scale.y, MinColliderHeight);
        box.isTrigger = false;
        box.center = new Vector3(0f, height * 0.5f + GroundClearance, 0f);
        box.size = new Vector3(
            scale.x + TargetPadding,
            height + TargetPadding,
            scale.z + TargetPadding);
    }

    static void StripChildSolidColliders(GameObject root)
    {
        Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            if (collider == null || collider.transform == root.transform)
            {
                continue;
            }

            if (!collider.isTrigger)
            {
                Object.Destroy(collider);
            }
        }
    }
}
