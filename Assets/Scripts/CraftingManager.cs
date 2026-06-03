public static class CraftingManager
{
    public static bool CanCraft(PlayerInventory inventory, CraftingRecipe recipe, WorkstationKind activeWorkstation)
    {
        return Evaluate(inventory, recipe, activeWorkstation).CanCraftNow;
    }

    public static CraftingUnlockResult Evaluate(PlayerInventory inventory, CraftingRecipe recipe, WorkstationKind activeWorkstation)
    {
        return CraftingUnlockEvaluator.Evaluate(recipe, activeWorkstation, inventory);
    }

    public static bool TryCraft(
        PlayerInventory inventory,
        CraftingRecipe recipe,
        WorkstationKind activeWorkstation,
        out string message)
    {
        message = string.Empty;

        CraftingUnlockResult result = Evaluate(inventory, recipe, activeWorkstation);
        if (!result.IsUnlocked)
        {
            message = result.Message;
            return false;
        }

        if (!result.CanCraftNow)
        {
            message = string.IsNullOrEmpty(result.Message) ? "无法制作。" : result.Message;
            return false;
        }

        if (!inventory.TryConsumeIngredients(recipe.Ingredients))
        {
            message = "材料消耗失败。";
            return false;
        }

        if (!inventory.TryAddItem(recipe.Output, recipe.OutputCount))
        {
            message = "产物无法放入背包。";
            return false;
        }

        message = $"已制作 {recipe.GetDisplayName()}。";
        return true;
    }
}
