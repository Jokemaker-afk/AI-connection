using UnityEngine;

public class PlacedPickupable : MonoBehaviour
{
    [SerializeField] ItemKind itemKind = ItemKind.None;
    [SerializeField] float pickupRange = 3.5f;

    public ItemKind ItemKind => itemKind;
    public float PickupRange => pickupRange;

    public void Initialize(ItemKind kind)
    {
        itemKind = kind;
    }

    public bool IsPlayerInRange(Vector3 playerPosition)
    {
        Vector3 flat = transform.position - playerPosition;
        flat.y = 0f;
        return flat.sqrMagnitude <= pickupRange * pickupRange;
    }

    public bool TryPickup(PlayerInventory inventory, out string message)
    {
        message = string.Empty;

        if (!ItemKindUtility.IsValid(itemKind) || inventory == null)
        {
            return false;
        }

        if (!inventory.CanAcceptItem(itemKind, 1))
        {
            message = "背包已满，无法收回物品。";
            return false;
        }

        if (!inventory.TryAddItem(itemKind, 1))
        {
            message = "背包已满，无法收回物品。";
            return false;
        }

        message = $"已收回 {ItemKindUtility.GetDisplayName(itemKind)}。";
        Destroy(gameObject);
        return true;
    }
}
