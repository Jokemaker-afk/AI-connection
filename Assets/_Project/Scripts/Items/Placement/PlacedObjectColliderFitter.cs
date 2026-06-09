using UnityEngine;

/// <summary>
/// Fits physical, interaction, and placement bounds to the current visual model for placed objects.
/// </summary>
[DisallowMultipleComponent]
public class PlacedObjectColliderFitter : MonoBehaviour
{
    [SerializeField] Transform visualRoot;
    [SerializeField] ItemKind sourceItem = ItemKind.None;
    [SerializeField] bool fitPhysicalColliderToVisualBounds = true;
    [SerializeField] bool fitInteractionColliderToVisualBounds = true;
    [SerializeField] bool fitPlacementFootprintToVisualBounds = true;
    [SerializeField] Vector3 physicalColliderPadding = new Vector3(0.05f, 0.05f, 0.05f);
    [SerializeField] Vector3 interactionColliderPadding = new Vector3(0.4f, 0.4f, 0.4f);
    [SerializeField] Vector3 placementFootprintPadding = new Vector3(0.05f, 0.02f, 0.05f);
    [SerializeField] bool syncOnAwake = true;
    [SerializeField] bool syncOnValidate = true;

    [SerializeField] Vector3 cachedFootprintSize = Vector3.zero;
    [SerializeField] Vector3 cachedFootprintCenter = Vector3.zero;

    public bool HasValidFootprint => cachedFootprintSize.sqrMagnitude > 0.01f;

    public Vector3 PlacementHalfExtents => HasValidFootprint
        ? new Vector3(
            Mathf.Max(0.05f, cachedFootprintSize.x * 0.5f - PlacedObjectBuilder.PlacementCheckPadding),
            Mathf.Max(0.05f, cachedFootprintSize.y * 0.5f - PlacedObjectBuilder.PlacementCheckPadding),
            Mathf.Max(0.05f, cachedFootprintSize.z * 0.5f - PlacedObjectBuilder.PlacementCheckPadding))
        : PlacedObjectBuilder.GetPlacementHalfExtents(sourceItem);

    public void Initialize(ItemKind kind)
    {
        if (ItemKindUtility.IsValid(kind))
        {
            sourceItem = kind;
        }
    }

    public void ResolveVisualRootAuto()
    {
        if (visualRoot == null)
        {
            visualRoot = PlacedObjectBoundsUtility.ResolveVisualRoot(transform);
        }
    }

    public Bounds CalculateVisualBounds()
    {
        ResolveVisualRootAuto();
        if (TryGetVisualBounds(out Bounds bounds))
        {
            return bounds;
        }

        return PlacedObjectBoundsUtility.GetFallbackLocalBounds(sourceItem);
    }

    public void SyncPhysicalCollider()
    {
        if (!fitPhysicalColliderToVisualBounds)
        {
            return;
        }

        Bounds bounds = CalculateVisualBounds();
        bounds = PlacedObjectBoundsUtility.ApplyPadding(bounds, physicalColliderPadding);

        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null)
        {
            box = gameObject.AddComponent<BoxCollider>();
        }

        box.isTrigger = false;
        box.center = bounds.center;
        box.size = bounds.size;
    }

    public void SyncInteractionCollider()
    {
        if (!fitInteractionColliderToVisualBounds)
        {
            return;
        }

        Bounds bounds = CalculateVisualBounds();
        bounds = PlacedObjectBoundsUtility.ApplyPadding(bounds, interactionColliderPadding);

        Transform interactionTransform = transform.Find(PlacedObjectBoundsUtility.InteractionColliderName);
        if (interactionTransform == null)
        {
            var interactionGo = new GameObject(PlacedObjectBoundsUtility.InteractionColliderName);
            interactionTransform = interactionGo.transform;
            interactionTransform.SetParent(transform, false);
        }

        BoxCollider interactionBox = interactionTransform.GetComponent<BoxCollider>();
        if (interactionBox == null)
        {
            interactionBox = interactionTransform.gameObject.AddComponent<BoxCollider>();
        }

        interactionBox.isTrigger = true;
        interactionBox.center = bounds.center;
        interactionBox.size = bounds.size;
    }

    public void SyncPlacementFootprint()
    {
        if (!fitPlacementFootprintToVisualBounds)
        {
            return;
        }

        Bounds bounds = CalculateVisualBounds();
        bounds = PlacedObjectBoundsUtility.ApplyPadding(bounds, placementFootprintPadding);
        cachedFootprintCenter = bounds.center;
        cachedFootprintSize = bounds.size;
    }

    public void SyncOutlineBounds()
    {
        PlacedObjectHighlighter highlighter = GetComponent<PlacedObjectHighlighter>();
        if (highlighter != null)
        {
            highlighter.RefreshFromVisualBounds();
        }
    }

    public void SyncAll()
    {
        StripVisualSolidColliders();
        SyncPhysicalCollider();
        SyncInteractionCollider();
        SyncPlacementFootprint();
        SyncOutlineBounds();
    }

    void Awake()
    {
        if (syncOnAwake)
        {
            SyncAll();
        }
    }

    void OnValidate()
    {
        if (!syncOnValidate)
        {
            return;
        }

        ResolveVisualRootAuto();
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            SyncAll();
        }
#endif
    }

    bool TryGetVisualBounds(out Bounds localBounds)
    {
        ResolveVisualRootAuto();
        if (visualRoot != null
            && PlacedObjectBoundsUtility.TryCalculateLocalRendererBounds(transform, visualRoot, out localBounds))
        {
            return true;
        }

        localBounds = PlacedObjectBoundsUtility.GetFallbackLocalBounds(sourceItem);
        return false;
    }

    void StripVisualSolidColliders()
    {
        if (visualRoot == null)
        {
            return;
        }

        Collider[] colliders = visualRoot.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            if (collider == null || collider.isTrigger)
            {
                continue;
            }

            if (collider.transform == transform)
            {
                continue;
            }

            if (Application.isPlaying)
            {
                Destroy(collider);
            }
            else
            {
                DestroyImmediate(collider);
            }
        }
    }
}
