using UnityEngine;

/// <summary>
/// Recoverable placed object marker. Distinct from WorldPickupItem / dropped pickups.
/// </summary>
public class PlacedPickupable : MonoBehaviour
{
    [SerializeField] ItemKind itemKind = ItemKind.None;
    [SerializeField] int recoverAmount = 1;
    [SerializeField] bool recoverable = true;
    [SerializeField] float pickupRange = 3.5f;

    public ItemKind ItemKind => itemKind;
    public int RecoverAmount => recoverAmount;
    public float PickupRange => pickupRange;
    public bool IsRecoverableTarget => recoverable && ItemKindUtility.IsRecoverableWhenPlaced(itemKind);
    public string DisplayNameChinese => ItemKindUtility.GetDisplayName(itemKind);

    public void Initialize(ItemKind kind, int amount = 1)
    {
        itemKind = kind;
        recoverAmount = Mathf.Max(1, amount);

        if (ItemCatalog.TryGet(kind, out ItemData data) && data.IsValid)
        {
            recoverable = data.Recoverable;
        }
    }

    public bool IsPlayerInRange(Vector3 playerPosition)
    {
        Vector3 flat = transform.position - playerPosition;
        flat.y = 0f;
        return flat.sqrMagnitude <= pickupRange * pickupRange;
    }

    public bool CanRecover(PlayerInventory inventory)
    {
        return IsRecoverableTarget
            && inventory != null
            && UnifiedInventoryAcquisition.CanAcquire(inventory, itemKind, recoverAmount);
    }

    public bool TryPickup(PlayerInventory inventory, out string message)
    {
        return TryRecover(inventory, out message);
    }

    public bool TryRecover(PlayerInventory inventory, out string message)
    {
        message = string.Empty;

        if (!ItemKindUtility.IsValid(itemKind) || inventory == null)
        {
            return false;
        }

        if (!IsRecoverableTarget)
        {
            return false;
        }

        Debug.Log($"[PlacedTarget] F recovery attempted: {DisplayNameChinese}");

        if (!UnifiedInventoryAcquisition.CanAcquire(inventory, itemKind, recoverAmount))
        {
            message = "背包已满";
            Debug.Log("[PlacedTarget] Recovery inventory add success: false (full)");
            return false;
        }

        InventoryAddResult result = UnifiedInventoryAcquisition.TryAcquire(inventory, itemKind, recoverAmount);
        if (!result.FullyAdded)
        {
            message = "背包已满";
            Debug.Log("[PlacedTarget] Recovery inventory add success: false");
            return false;
        }

        message = $"已回收 {DisplayNameChinese}。";
        Debug.Log("[PlacedTarget] Recovery inventory add success: true");
        Destroy(gameObject);
        return true;
    }
}
