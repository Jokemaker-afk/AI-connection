using UnityEngine;

/// <summary>Enhances Level8 world pickups with category visuals and proximity labels.</summary>
[DisallowMultipleComponent]
public class Level8ResourcePickupEnhancer : MonoBehaviour
{
    WorldPickupItem pickup;

    public void Configure(ItemKind kind, Level8BiomeKind biome = default)
    {
        pickup = GetComponentInChildren<WorldPickupItem>(true);

        if (!ItemPickupVisualResolver.HasResolvedVisual(kind, biome))
        {
            Level8VisualHierarchyUtility.ApplyResourceCategoryVisual(transform, kind);
        }
        else
        {
            Transform overlay = transform.Find("Level8ResourceVisual");
            if (overlay != null)
            {
                Destroy(overlay.gameObject);
            }
        }

        Transform existingLabel = transform.Find("Label");
        if (existingLabel != null)
        {
            existingLabel.gameObject.SetActive(false);
        }

        Transform existingLabelAnchor = transform.Find("LabelAnchor");
        if (existingLabelAnchor != null)
        {
            Destroy(existingLabelAnchor.gameObject);
        }

        string displayName = ItemKindUtility.GetDisplayName(kind);
        Level8VisualHierarchyUtility.CreateDistanceLabelAnchor(
            transform,
            displayName,
            Vector3.up * 0.85f,
            Level8VisualHierarchyFlags.ResourceLabelDistance,
            characterSize: 0.1f);
    }

    void Update()
    {
        if (pickup == null)
        {
            pickup = GetComponentInChildren<WorldPickupItem>(true);
        }

        if (pickup != null && pickup.IsCollected)
        {
            Destroy(this);
        }
    }
}
