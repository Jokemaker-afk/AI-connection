using UnityEngine;

/// <summary>
/// Marks a world pickup spawned from inventory drop (Q / drag). Never a placed building.
/// </summary>
public class DroppedItemMarker : MonoBehaviour
{
    public ItemKind ItemKind { get; private set; }
    public int Amount { get; private set; }

    public void Configure(ItemKind kind, int amount)
    {
        ItemKind = kind;
        Amount = amount;

        if (ItemKindUtility.IsPlaceable(kind))
        {
            Debug.Log($"[ItemDrop] Dropped {ItemKindUtility.GetDisplayName(kind)} is pickup only, not active workstation.");
        }
    }
}
