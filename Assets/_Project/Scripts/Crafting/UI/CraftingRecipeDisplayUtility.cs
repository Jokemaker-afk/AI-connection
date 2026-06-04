using System.Collections.Generic;

public static class CraftingRecipeDisplayUtility
{
    public static string GetSourceDisplayName(WorkstationKind workstation)
    {
        if (workstation == WorkstationKind.Player)
        {
            workstation = WorkstationKind.None;
        }

        return WorkstationDetector.GetColumnTitle(workstation);
    }

    public static string GetCategoryDisplayName(CraftingRecipeCategory category)
    {
        return CraftingRecipeCategoryUtility.GetDisplayName(category);
    }

    public static IEnumerable<CraftingRecipe> EnumerateKnownRecipes(CraftingRecipeCategory? categoryFilter)
    {
        IReadOnlyList<CraftingRecipe> all = CraftingRecipeDatabase.AllRecipes;
        for (int i = 0; i < all.Count; i++)
        {
            CraftingRecipe recipe = all[i];
            if (CraftingRecipeCategoryUtility.MatchesFilter(recipe, categoryFilter))
            {
                yield return recipe;
            }
        }
    }

    public static string BuildMaterialsLine(CraftingRecipe recipe)
    {
        string summary = recipe.GetIngredientsSummary();
        return string.IsNullOrEmpty(summary) ? "材料：无" : $"材料：{summary.Replace(" + ", "，")}";
    }

    public static string BuildHandbookDetail(
        CraftingRecipe recipe,
        CraftingUnlockResult result,
        WorkstationKind evaluationContext)
    {
        string source = GetSourceDisplayName(recipe.RequiredWorkstation);
        string category = GetCategoryDisplayName(recipe.Category);
        string materials = BuildMaterialsLine(recipe);
        string stationNote = evaluationContext != recipe.RequiredWorkstation
            && recipe.RequiredWorkstation != WorkstationKind.Player
            ? $"（需在{source}旁制作）"
            : string.Empty;

        if (!result.IsUnlocked)
        {
            return $"{materials}\n制作来源：{source}{stationNote}\n分类：{category}\n状态：{result.Message}";
        }

        if (result.CanCraftNow)
        {
            return $"{materials}\n制作来源：{source}{stationNote}\n分类：{category}\n状态：可制作";
        }

        string status = string.IsNullOrEmpty(result.Message) ? "暂不可制作" : result.Message;
        return $"{materials}\n制作来源：{source}{stationNote}\n分类：{category}\n状态：{status}";
    }

    public static CraftingUnlockResult EvaluateForHandbook(CraftingRecipe recipe, PlayerInventory inventory)
    {
        WorkstationKind context = recipe.RequiredWorkstation;
        if (context == WorkstationKind.Player)
        {
            context = WorkstationKind.None;
        }

        return CraftingUnlockEvaluator.Evaluate(recipe, context, inventory);
    }
}
