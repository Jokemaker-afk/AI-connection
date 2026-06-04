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

    public InventoryAddResult TryAcquireItem(ItemKind kind, int amount = 1)
    {
        if (!ItemKindUtility.IsValid(kind) || amount <= 0)
        {
            return InventoryAddResult.None(amount);
        }

        int initialAmount = amount;
        int remaining = amount;

        // Stack into existing matching slots first (hotbar, then backpack), then empty slots.
        remaining = TryStackIntoSlots(hotbar, kind, remaining);
        remaining = TryStackIntoSlots(backpack, kind, remaining);
        remaining = TryFillEmptySlots(hotbar, kind, remaining);
        remaining = TryFillEmptySlots(backpack, kind, remaining);

        int added = initialAmount - remaining;
        if (added > 0)
        {
            NotifyChanged();
        }

        return new InventoryAddResult
        {
            Requested = initialAmount,
            Added = added,
        };
    }

    public bool TryAddItem(ItemKind kind, int amount = 1)
    {
        return TryAcquireItem(kind, amount).FullyAdded;
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
        remaining = SimulateFillEmpty(hotbar, kind, remaining);
        remaining = SimulateFillEmpty(backpack, kind, remaining);
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

    public InventorySlotData GetSlot(InventorySlotRegion region, int index)
    {
        switch (region)
        {
            case InventorySlotRegion.Hotbar:
                return GetHotbarSlot(index);
            case InventorySlotRegion.Backpack:
                return GetBackpackSlot(index);
            default:
                return InventorySlotData.Empty;
        }
    }

    public bool TrySplitFromSlot(InventorySlotRegion region, int index, int amount, out InventorySlotData extracted)
    {
        extracted = InventorySlotData.Empty;

        if (!TryGetSlotArray(region, out var slots, out int length) || index < 0 || index >= length || amount <= 0)
        {
            return false;
        }

        InventorySlotData slot = slots[index];
        if (slot.IsEmpty)
        {
            return false;
        }

        if (!ItemKindUtility.IsStackable(slot.Kind))
        {
            amount = slot.Count;
        }
        else
        {
            amount = Mathf.Clamp(amount, 1, slot.Count);
        }

        extracted = InventorySlotData.Of(slot.Kind, amount);
        slot.Count -= amount;
        slots[index] = slot.Count > 0 ? slot : InventorySlotData.Empty;
        NormalizeSlotArray(slots);
        NotifyChanged();
        return true;
    }

    public bool TryApplyHeldToSlot(InventorySlotRegion region, int index, ref InventorySlotData held)
    {
        if (held.IsEmpty
            || !TryGetSlotArray(region, out var slots, out int length)
            || index < 0
            || index >= length)
        {
            return false;
        }

        InventorySlotData target = slots[index];
        bool changed = false;

        if (target.IsEmpty)
        {
            slots[index] = held;
            held = InventorySlotData.Empty;
            changed = true;
        }
        else if (target.Kind == held.Kind && target.Count < ItemKindUtility.GetMaxStackSize(target.Kind))
        {
            int space = ItemKindUtility.GetMaxStackSize(target.Kind) - target.Count;
            int moved = Mathf.Min(space, held.Count);
            if (moved > 0)
            {
                target.Count += moved;
                held.Count -= moved;
                slots[index] = target;
                if (held.Count <= 0)
                {
                    held = InventorySlotData.Empty;
                }

                changed = true;
            }
        }
        else if (target.Kind != held.Kind)
        {
            slots[index] = held;
            held = target;
            changed = true;
        }

        if (changed)
        {
            NormalizeSlotArray(slots);
            NotifyChanged();
        }

        return changed;
    }

    public int CountItem(ItemKind kind)
    {
        if (!ItemKindUtility.IsValid(kind))
        {
            return 0;
        }

        int total = 0;
        for (int i = 0; i < hotbar.Length; i++)
        {
            if (hotbar[i].Kind == kind)
            {
                total += hotbar[i].Count;
            }
        }

        for (int i = 0; i < backpack.Length; i++)
        {
            if (backpack[i].Kind == kind)
            {
                total += backpack[i].Count;
            }
        }

        return total;
    }

    public bool HasIngredients(CraftingIngredient[] ingredients)
    {
        if (ingredients == null)
        {
            return true;
        }

        for (int i = 0; i < ingredients.Length; i++)
        {
            if (CountItem(ingredients[i].Item) < ingredients[i].Count)
            {
                return false;
            }
        }

        return true;
    }

    public bool TryConsumeIngredients(CraftingIngredient[] ingredients)
    {
        if (!HasIngredients(ingredients))
        {
            return false;
        }

        for (int i = 0; i < ingredients.Length; i++)
        {
            if (!TryRemoveItem(ingredients[i].Item, ingredients[i].Count, notify: false))
            {
                return false;
            }
        }

        NotifyChanged();
        return true;
    }

    public bool TryRemoveItem(ItemKind kind, int amount, bool notify = true)
    {
        if (!ItemKindUtility.IsValid(kind) || amount <= 0 || CountItem(kind) < amount)
        {
            return false;
        }

        int remaining = amount;
        remaining = RemoveFromSlots(backpack, kind, remaining);
        remaining = RemoveFromSlots(hotbar, kind, remaining);
        if (remaining > 0)
        {
            return false;
        }

        NormalizeSlotArray(hotbar);
        NormalizeSlotArray(backpack);
        if (notify)
        {
            NotifyChanged();
        }

        return true;
    }

    public bool TryConsumeFromSelectedHotbar(int amount = 1)
    {
        return TryRemoveFromSlot(InventorySlotRegion.Hotbar, selectedHotbarIndex, amount);
    }

    public bool TryRemoveFromSlot(InventorySlotRegion region, int index, int amount, bool notify = true)
    {
        if (amount <= 0
            || !TryGetSlotArray(region, out InventorySlotData[] slots, out int length)
            || index < 0
            || index >= length)
        {
            return false;
        }

        InventorySlotData slot = slots[index];
        if (slot.IsEmpty || slot.Count < amount)
        {
            return false;
        }

        slot.Count -= amount;
        slots[index] = slot.Count > 0 ? slot : InventorySlotData.Empty;
        NormalizeSlotArray(hotbar);
        NormalizeSlotArray(backpack);

        if (notify)
        {
            NotifyChanged();
        }

        return true;
    }

    static int RemoveFromSlots(InventorySlotData[] slots, ItemKind kind, int amount)
    {
        int remaining = amount;
        for (int i = 0; i < slots.Length && remaining > 0; i++)
        {
            if (slots[i].Kind != kind)
            {
                continue;
            }

            int removed = Mathf.Min(slots[i].Count, remaining);
            slots[i].Count -= removed;
            remaining -= removed;
            if (slots[i].Count <= 0)
            {
                slots[i] = InventorySlotData.Empty;
            }
        }

        return remaining;
    }

    public bool TryReabsorbHeld(ref InventorySlotData held)
    {
        if (held.IsEmpty)
        {
            return true;
        }

        int initial = held.Count;
        int remaining = held.Count;
        remaining = TryStackIntoSlots(hotbar, held.Kind, remaining);
        remaining = TryStackIntoSlots(backpack, held.Kind, remaining);
        remaining = TryFillEmptySlots(hotbar, held.Kind, remaining);
        remaining = TryFillEmptySlots(backpack, held.Kind, remaining);

        if (remaining == initial)
        {
            return false;
        }

        held = remaining > 0 ? InventorySlotData.Of(held.Kind, remaining) : InventorySlotData.Empty;
        NotifyChanged();
        return remaining <= 0;
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
        else if (to.Kind == from.Kind && to.Count < ItemKindUtility.GetMaxStackSize(to.Kind))
        {
            int space = ItemKindUtility.GetMaxStackSize(to.Kind) - to.Count;
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
        else if (to.Kind == from.Kind)
        {
            return false;
        }
        else
        {
            toArray[toIndex] = from;
            fromArray[fromIndex] = to;
            changed = true;
        }

        if (changed)
        {
            NormalizeSlotArray(fromArray);
            if (!ReferenceEquals(fromArray, toArray))
            {
                NormalizeSlotArray(toArray);
            }

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
            if (slots[i].IsEmpty || slots[i].Kind != kind || slots[i].Count >= ItemKindUtility.GetMaxStackSize(kind))
            {
                continue;
            }

            int space = ItemKindUtility.GetMaxStackSize(kind) - slots[i].Count;
            int moved = Mathf.Min(space, remaining);
            var slot = slots[i];
            slot.Count += moved;
            slots[i] = slot;
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

            int stack = Mathf.Min(ItemKindUtility.GetMaxStackSize(kind), remaining);
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
            if (slots[i].IsEmpty || slots[i].Kind != kind || slots[i].Count >= ItemKindUtility.GetMaxStackSize(kind))
            {
                continue;
            }

            remaining -= ItemKindUtility.GetMaxStackSize(kind) - slots[i].Count;
        }

        return Mathf.Max(0, remaining);
    }

    static int SimulateFillEmpty(InventorySlotData[] slots, ItemKind kind, int amount)
    {
        int remaining = amount;
        int maxStack = ItemKindUtility.GetMaxStackSize(kind);
        for (int i = 0; i < slots.Length && remaining > 0; i++)
        {
            if (!slots[i].IsEmpty)
            {
                continue;
            }

            remaining -= maxStack;
        }

        return Mathf.Max(0, remaining);
    }

    static void NormalizeSlotArray(InventorySlotData[] slots)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty)
            {
                slots[i] = InventorySlotData.Empty;
            }
        }
    }

    public bool HasAnyItem()
    {
        for (int i = 0; i < hotbar.Length; i++)
        {
            if (!hotbar[i].IsEmpty)
            {
                return true;
            }
        }

        for (int i = 0; i < backpack.Length; i++)
        {
            if (!backpack[i].IsEmpty)
            {
                return true;
            }
        }

        return false;
    }

    void NotifyChanged()
    {
        OnInventoryChanged?.Invoke();
    }

    public InventorySaveData CreateSaveData()
    {
        var hotbarCopy = new InventorySlotData[HotbarSize];
        var backpackCopy = new InventorySlotData[BackpackSize];
        System.Array.Copy(hotbar, hotbarCopy, HotbarSize);
        System.Array.Copy(backpack, backpackCopy, BackpackSize);

        return new InventorySaveData
        {
            Hotbar = hotbarCopy,
            Backpack = backpackCopy,
            SelectedHotbarIndex = selectedHotbarIndex,
            HasData = HasAnyItem(),
        };
    }

    public void ApplySaveData(InventorySaveData saveData)
    {
        if (saveData.Hotbar != null && saveData.Hotbar.Length == HotbarSize)
        {
            System.Array.Copy(saveData.Hotbar, hotbar, HotbarSize);
        }

        if (saveData.Backpack != null && saveData.Backpack.Length == BackpackSize)
        {
            System.Array.Copy(saveData.Backpack, backpack, BackpackSize);
        }

        SetSelectedHotbarIndex(saveData.SelectedHotbarIndex);
        NormalizeSlotArray(hotbar);
        NormalizeSlotArray(backpack);
        NotifyChanged();
    }
}

public enum InventorySlotRegion
{
    Hotbar,
    Backpack,
}
