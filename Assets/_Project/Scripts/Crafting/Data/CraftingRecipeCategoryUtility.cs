using System.Collections.Generic;

public static class CraftingRecipeCategoryUtility
{
    public static readonly CraftingRecipeCategory[] FilterCategories =
    {
        CraftingRecipeCategory.BasicMaterial,
        CraftingRecipeCategory.IntermediateMaterial,
        CraftingRecipeCategory.Tool,
        CraftingRecipeCategory.Weapon,
        CraftingRecipeCategory.Consumable,
        CraftingRecipeCategory.Building,
        CraftingRecipeCategory.Workstation,
        CraftingRecipeCategory.Storage,
        CraftingRecipeCategory.Progression,
    };

    public static string GetDisplayName(CraftingRecipeCategory category)
    {
        switch (category)
        {
            case CraftingRecipeCategory.BasicMaterial:
                return "基础材料";
            case CraftingRecipeCategory.IntermediateMaterial:
                return "中间材料";
            case CraftingRecipeCategory.Tool:
                return "工具";
            case CraftingRecipeCategory.Weapon:
                return "武器";
            case CraftingRecipeCategory.Consumable:
                return "消耗品";
            case CraftingRecipeCategory.Building:
                return "建筑材料";
            case CraftingRecipeCategory.Workstation:
                return "工作场所";
            case CraftingRecipeCategory.Storage:
                return "储物";
            case CraftingRecipeCategory.Progression:
                return "关键物品";
            default:
                return "全部";
        }
    }

    public static bool MatchesFilter(CraftingRecipe recipe, CraftingRecipeCategory? filter)
    {
        if (!filter.HasValue)
        {
            return true;
        }

        return recipe.Category == filter.Value;
    }

    public static IEnumerable<CraftingRecipe> GetRecipesForContext(
        WorkstationKind context,
        CraftingRecipeCategory? categoryFilter)
    {
        foreach (CraftingRecipe recipe in CraftingRecipeDatabase.GetRecipesForContext(context))
        {
            if (MatchesFilter(recipe, categoryFilter))
            {
                yield return recipe;
            }
        }
    }
}
