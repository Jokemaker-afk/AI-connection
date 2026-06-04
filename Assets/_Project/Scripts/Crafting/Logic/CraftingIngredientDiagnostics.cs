using System.Collections.Generic;
using UnityEngine;

public static class CraftingIngredientDiagnostics
{
    public static bool HasAllIngredients(PlayerInventory inventory, CraftingIngredient[] ingredients)
    {
        return inventory != null && BuildMissingEntries(inventory, ingredients).Count == 0;
    }

    public static string BuildMissingMessage(PlayerInventory inventory, CraftingIngredient[] ingredients)
    {
        List<MissingEntry> missing = BuildMissingEntries(inventory, ingredients);
        if (missing.Count == 0)
        {
            return string.Empty;
        }

        var parts = new string[missing.Count];
        for (int i = 0; i < missing.Count; i++)
        {
            MissingEntry entry = missing[i];
            parts[i] = $"{entry.DisplayName} {entry.Have}/{entry.Need}";
        }

        return $"缺少材料：{string.Join("，", parts)}";
    }

    public static string BuildInventorySnapshot(PlayerInventory inventory)
    {
        if (inventory == null)
        {
            return "inventory=null";
        }

        var parts = new List<string>(16);
        AppendOccupiedSlots(inventory, parts, hotbar: true);
        AppendOccupiedSlots(inventory, parts, hotbar: false);
        parts.Add($"emptySlots={inventory.CountEmptySlots()}");
        return parts.Count > 0 ? string.Join(" | ", parts) : "empty";
    }

    static void AppendOccupiedSlots(PlayerInventory inventory, List<string> parts, bool hotbar)
    {
        int size = hotbar ? PlayerInventory.HotbarSize : PlayerInventory.BackpackSize;
        string region = hotbar ? "hotbar" : "backpack";

        for (int i = 0; i < size; i++)
        {
            InventorySlotData slot = hotbar ? inventory.GetHotbarSlot(i) : inventory.GetBackpackSlot(i);
            if (slot.IsEmpty)
            {
                continue;
            }

            string label = ItemKindUtility.GetDisplayName(slot.Kind);
            if (string.IsNullOrEmpty(label))
            {
                label = slot.Kind.ToString();
            }

            parts.Add($"{region}[{i}]={label}x{slot.Count}");
        }
    }

    static List<MissingEntry> BuildMissingEntries(PlayerInventory inventory, CraftingIngredient[] ingredients)
    {
        var missing = new List<MissingEntry>();
        if (inventory == null || ingredients == null || ingredients.Length == 0)
        {
            return missing;
        }

        for (int i = 0; i < ingredients.Length; i++)
        {
            CraftingIngredient ingredient = ingredients[i];
            if (ingredient.Count <= 0)
            {
                continue;
            }

            int have = inventory.CountItem(ingredient.Item);
            if (have >= ingredient.Count)
            {
                continue;
            }

            missing.Add(new MissingEntry
            {
                Item = ingredient.Item,
                DisplayName = ItemKindUtility.GetDisplayName(ingredient.Item),
                Have = have,
                Need = ingredient.Count,
            });
        }

        return missing;
    }

    struct MissingEntry
    {
        public ItemKind Item;
        public string DisplayName;
        public int Have;
        public int Need;
    }
}
