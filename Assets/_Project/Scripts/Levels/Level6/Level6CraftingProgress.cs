using UnityEngine;

/// <summary>
/// Tracks whether the player has crafted/obtained the three Level 6 tutorial tools.
/// </summary>
public static class Level6CraftingProgress
{
    public static bool HasBasicTool(PlayerInventory inventory, ItemKind kind)
    {
        return inventory != null && inventory.CountItem(kind) > 0;
    }

    public static bool HasAllBasicTools(PlayerInventory inventory)
    {
        if (inventory == null)
        {
            return false;
        }

        return HasBasicTool(inventory, ItemKind.StonePickaxe)
            && HasBasicTool(inventory, ItemKind.StoneAxe)
            && HasBasicTool(inventory, ItemKind.RepairTool);
    }

    public static bool IsMissingTool(PlayerInventory inventory, ItemKind kind)
    {
        return !HasBasicTool(inventory, kind);
    }

    public static string BuildMissingToolsSummary(PlayerInventory inventory)
    {
        if (inventory == null)
        {
            return "石斧、石镐、修理工具";
        }

        var missing = new System.Collections.Generic.List<string>(3);
        if (IsMissingTool(inventory, ItemKind.StoneAxe))
        {
            missing.Add("石斧");
        }

        if (IsMissingTool(inventory, ItemKind.StonePickaxe))
        {
            missing.Add("石镐");
        }

        if (IsMissingTool(inventory, ItemKind.RepairTool))
        {
            missing.Add("修理工具");
        }

        return missing.Count == 0 ? string.Empty : string.Join("、", missing);
    }
}
