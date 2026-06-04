/// <summary>
/// Global entry point for adding items to the player inventory.
/// Pickup, crafting output, recovery, tool harvest, and debug rewards should all use this.
/// </summary>
public static class UnifiedInventoryAcquisition
{
    public static bool CanAcquire(PlayerInventory inventory, ItemKind kind, int amount = 1)
    {
        return inventory != null && inventory.CanAcceptItem(kind, amount);
    }

    public static InventoryAddResult TryAcquire(PlayerInventory inventory, ItemKind kind, int amount = 1)
    {
        if (inventory == null)
        {
            return InventoryAddResult.None(amount);
        }

        return inventory.TryAcquireItem(kind, amount);
    }

    public static bool TryAcquireFully(PlayerInventory inventory, ItemKind kind, int amount = 1)
    {
        return TryAcquire(inventory, kind, amount).FullyAdded;
    }
}
