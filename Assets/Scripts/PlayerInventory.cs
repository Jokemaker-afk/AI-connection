using System;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public const int HotbarSize = 9;
    public const int BackpackSize = 27;

    [SerializeField] int selectedHotbarIndex;

    readonly InventorySlotData[] hotbar = new InventorySlotData[HotbarSize];
    readonly InventorySlotData[] backpack = new InventorySlotData[BackpackSize];

    public int SelectedHotbarIndex => selectedHotbarIndex;

    public event Action OnInventoryChanged;
    public event Action<int> OnHotbarSelectionChanged;

    public bool TryAddItem(ItemKind kind, int amount = 1)
    {
        if (!ItemKindUtility.IsValid(kind) || amount <= 0)
        {
            return false;
        }

        int initialAmount = amount;
        int remaining = amount;

        // Minecraft style priority: stack/fill hotbar first, then backpack.
        remaining = TryStackIntoSlots(hotbar, kind, remaining);
        remaining = TryStackIntoSlots(backpack, kind, remaining);
        remaining = TryFillEmptySlots(hotbar, kind, remaining);
        remaining = TryFillEmptySlots(backpack, kind, remaining);

        if (remaining == initialAmount)
        {
            return false;
        }

        NotifyChanged();
        return true;
    }

    public bool CanAcceptItem(ItemKind kind, int amount = 1)
    {
        if (!ItemKindUtility.IsValid(kind) || amount <= 0)
        {
            return false;
        }

        int remaining = amount;
        remaining = SimulateStack(hotbar, kind, remaining);
        remaining = SimulateStack(backpack, kind, remaining);
        remaining = SimulateFillEmpty(hotbar, remaining);
        remaining = SimulateFillEmpty(backpack, remaining);
        return remaining <= 0;
    }

    public InventorySlotData GetHotbarSlot(int index)
    {
        return index >= 0 && index < HotbarSize ? hotbar[index] : InventorySlotData.Empty;
    }

    public InventorySlotData GetBackpackSlot(int index)
    {
        return index >= 0 && index < BackpackSize ? backpack[index] : InventorySlotData.Empty;
    }

    public void SetSelectedHotbarIndex(int index)
    {
        int clamped = Mathf.Clamp(index, 0, HotbarSize - 1);
        if (selectedHotbarIndex == clamped)
        {
            return;
        }

        selectedHotbarIndex = clamped;
        OnHotbarSelectionChanged?.Invoke(selectedHotbarIndex);
    }

    public bool TryMoveOrSwapSlot(InventorySlotRegion fromRegion, int fromIndex, InventorySlotRegion toRegion, int toIndex)
    {
        if (!TryGetSlotArray(fromRegion, out var fromArray, out int fromLength)
            || !TryGetSlotArray(toRegion, out var toArray, out int toLength)
            || fromIndex < 0 || fromIndex >= fromLength
            || toIndex < 0 || toIndex >= toLength)
        {
            return false;
        }

        if (fromRegion == toRegion && fromIndex == toIndex)
        {
            return false;
        }

        InventorySlotData from = fromArray[fromIndex];
        InventorySlotData to = toArray[toIndex];

        if (from.IsEmpty)
        {
            return false;
        }

        bool changed = false;

        if (to.IsEmpty)
        {
            toArray[toIndex] = from;
            fromArray[fromIndex] = InventorySlotData.Empty;
            changed = true;
        }
        else if (to.Kind == from.Kind && to.Count < ItemKindUtility.MaxStackSize)
        {
            int space = ItemKindUtility.MaxStackSize - to.Count;
            int moved = Mathf.Min(space, from.Count);
            if (moved > 0)
            {
                to.Count += moved;
                from.Count -= moved;
                toArray[toIndex] = to;
                fromArray[fromIndex] = from.Count > 0 ? from : InventorySlotData.Empty;
                changed = true;
            }
        }
        else
        {
            toArray[toIndex] = from;
            fromArray[fromIndex] = to;
            changed = true;
        }

        if (changed)
        {
            NotifyChanged();
        }

        return changed;
    }

    bool TryGetSlotArray(InventorySlotRegion region, out InventorySlotData[] slots, out int length)
    {
        switch (region)
        {
            case InventorySlotRegion.Hotbar:
                slots = hotbar;
                length = HotbarSize;
                return true;
            case InventorySlotRegion.Backpack:
                slots = backpack;
                length = BackpackSize;
                return true;
            default:
                slots = null;
                length = 0;
                return false;
        }
    }

    static int TryStackIntoSlots(InventorySlotData[] slots, ItemKind kind, int amount)
    {
        int remaining = amount;
        for (int i = 0; i < slots.Length && remaining > 0; i++)
        {
            if (slots[i].Kind != kind || slots[i].Count >= ItemKindUtility.MaxStackSize)
            {
                continue;
            }

            int space = ItemKindUtility.MaxStackSize - slots[i].Count;
            int moved = Mathf.Min(space, remaining);
            slots[i].Count += moved;
            remaining -= moved;
        }

        return remaining;
    }

    static int TryFillEmptySlots(InventorySlotData[] slots, ItemKind kind, int amount)
    {
        int remaining = amount;
        for (int i = 0; i < slots.Length && remaining > 0; i++)
        {
            if (!slots[i].IsEmpty)
            {
                continue;
            }

            int stack = Mathf.Min(ItemKindUtility.MaxStackSize, remaining);
            slots[i] = InventorySlotData.Of(kind, stack);
            remaining -= stack;
        }

        return remaining;
    }

    static int SimulateStack(InventorySlotData[] slots, ItemKind kind, int amount)
    {
        int remaining = amount;
        for (int i = 0; i < slots.Length && remaining > 0; i++)
        {
            if (slots[i].Kind != kind || slots[i].Count >= ItemKindUtility.MaxStackSize)
            {
                continue;
            }

            remaining -= ItemKindUtility.MaxStackSize - slots[i].Count;
        }

        return Mathf.Max(0, remaining);
    }

    static int SimulateFillEmpty(InventorySlotData[] slots, int amount)
    {
        int remaining = amount;
        for (int i = 0; i < slots.Length && remaining > 0; i++)
        {
            if (!slots[i].IsEmpty)
            {
                continue;
            }

            remaining -= ItemKindUtility.MaxStackSize;
        }

        return Mathf.Max(0, remaining);
    }

    void NotifyChanged()
    {
        OnInventoryChanged?.Invoke();
    }
}

public enum InventorySlotRegion
{
    Hotbar,
    Backpack,
}
