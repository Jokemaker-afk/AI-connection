using UnityEngine;

/// <summary>
/// Ensures placed objects have raycast-friendly colliders and recoverable metadata.
/// </summary>
public static class PlacedObjectInteractionSetup
{
    public static void Configure(GameObject root, ItemKind kind)
    {
        if (root == null || !ItemKindUtility.IsValid(kind))
        {
            return;
        }

        PlacedObjectColliderFitter fitter = root.GetComponent<PlacedObjectColliderFitter>();
        if (fitter == null)
        {
            fitter = root.AddComponent<PlacedObjectColliderFitter>();
            fitter.Initialize(kind);
            fitter.ResolveVisualRootAuto();
        }
        else
        {
            fitter.Initialize(kind);
        }

        fitter.SyncAll();

        var recoverable = root.GetComponent<PlacedPickupable>() ?? root.AddComponent<PlacedPickupable>();
        recoverable.Initialize(kind);

        var highlighter = root.GetComponent<PlacedObjectHighlighter>() ?? root.AddComponent<PlacedObjectHighlighter>();
        highlighter.RefreshFromVisualBounds();
    }
}
