using UnityEngine;

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

        string outputName = recipe.GetDisplayName();
        Debug.Log(
            $"[Crafting] Craft button clicked: {outputName} | Before craft inventory: {CraftingIngredientDiagnostics.BuildInventorySnapshot(inventory)}");

        CraftingUnlockResult result = Evaluate(inventory, recipe, activeWorkstation);
        if (!result.IsUnlocked)
        {
            message = result.Message;
            LogCraftAttempt(recipe, false, message);
            return false;
        }

        if (!result.CanCraftNow)
        {
            message = string.IsNullOrEmpty(result.Message) ? "无法制作。" : result.Message;
            LogCraftAttempt(recipe, false, message);
            return false;
        }

        Debug.Log($"[Crafting] Consume ingredients success: pending | {outputName}");
        if (!inventory.TryConsumeIngredients(recipe.Ingredients))
        {
            message = "材料消耗失败。";
            Debug.Log($"[Crafting] Consume ingredients success: false | {outputName}");
            LogCraftAttempt(recipe, false, message);
            return false;
        }

        Debug.Log($"[Crafting] Consume ingredients success: true | {outputName}");
        InventoryAddResult addResult = UnifiedInventoryAcquisition.TryAcquire(inventory, recipe.Output, recipe.OutputCount);
        if (!addResult.FullyAdded)
        {
            RestoreIngredients(inventory, recipe.Ingredients);
            message = ItemKindUtility.IsStackable(recipe.Output) ? "背包已满。" : "没有足够空间（工具需要 1 个空槽位）。";
            Debug.Log($"[Crafting] Add output success: false | {outputName} | added={addResult.Added}/{addResult.Requested}");
            LogCraftAttempt(recipe, false, message);
            return false;
        }

        Debug.Log(
            $"[Crafting] Add output success: true | {outputName} | After craft inventory: {CraftingIngredientDiagnostics.BuildInventorySnapshot(inventory)}");
        message = $"已制作 {recipe.GetDisplayName()}。";
        LogCraftAttempt(recipe, true, message, addResult);
        return true;
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public static bool DebugCraft(PlayerInventory inventory, ItemKind outputKind, out string message)
    {
        message = string.Empty;
        if (inventory == null)
        {
            message = "未找到背包。";
            return false;
        }

        CraftingRecipe recipe = FindRecipeForOutput(outputKind);
        if (string.IsNullOrEmpty(recipe.RecipeId))
        {
            message = $"未找到配方：{outputKind}";
            return false;
        }

        Debug.Log($"[Crafting] DebugCraft requested: {outputKind}");
        return TryCraft(inventory, recipe, WorkstationKind.None, out message);
    }

    static CraftingRecipe FindRecipeForOutput(ItemKind outputKind)
    {
        foreach (CraftingRecipe recipe in CraftingRecipeDatabase.AllRecipes)
        {
            if (recipe.Output == outputKind && recipe.RequiredWorkstation == WorkstationKind.None)
            {
                return recipe;
            }
        }

        return default;
    }
#endif

    static void LogCraftAttempt(CraftingRecipe recipe, bool success, string message, InventoryAddResult addResult = default)
    {
        if (!GameplayCore.Exists)
        {
            return;
        }

        string outputName = recipe.GetDisplayName();
        if (success)
        {
            GameplayCore.Instance.Log(
                $"[Crafting] Crafting {outputName}: success | Added non-stackable tool slots={addResult.Added}");
            return;
        }

        GameplayCore.Instance.Log($"[Crafting] Crafting failed: {outputName} | {message}");
    }

    static void RestoreIngredients(PlayerInventory inventory, CraftingIngredient[] ingredients)
    {
        if (inventory == null || ingredients == null)
        {
            return;
        }

        for (int i = 0; i < ingredients.Length; i++)
        {
            UnifiedInventoryAcquisition.TryAcquire(inventory, ingredients[i].Item, ingredients[i].Count);
        }
    }
}
