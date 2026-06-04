using UnityEngine;

public class PlacementSurface : MonoBehaviour
{
    [SerializeField] ItemKind sourceItem = ItemKind.None;

    public ItemKind SourceItem => sourceItem;

    public void Initialize(ItemKind itemKind)
    {
        sourceItem = itemKind;
    }

    public bool SupportsTopPlacement => ItemKindUtility.CanSupportPlacedObjects(sourceItem);

    public float GetTopSurfaceY()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            return col.bounds.max.y;
        }

        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds.max.y;
        }

        return transform.position.y;
    }

    public Bounds GetPlacementBounds()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            return col.bounds;
        }

        Renderer renderer = GetComponentInChildren<Renderer>();
        return renderer != null ? renderer.bounds : new Bounds(transform.position, Vector3.one);
    }
}
