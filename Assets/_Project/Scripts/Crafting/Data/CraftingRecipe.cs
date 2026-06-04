using System;
using UnityEngine;

[Serializable]
public struct CraftingRecipe
{
    public string RecipeId;
    public ItemKind Output;
    public int OutputCount;
    public CraftingIngredient[] Ingredients;
    public CraftingRecipeCategory Category;
    public WorkstationKind RequiredWorkstation;
    public BuildingKind RequiredBuilding;
    public TechnologyKind[] RequiredTechnologies;
    public bool IsUnlockedByDefault;

    public CraftingRecipe(
        string recipeId,
        ItemKind output,
        int outputCount,
        CraftingIngredient[] ingredients,
        CraftingRecipeCategory category = CraftingRecipeCategory.General,
        WorkstationKind workstation = WorkstationKind.None,
        BuildingKind building = BuildingKind.None,
        bool unlockedByDefault = false,
        params TechnologyKind[] technologies)
    {
        RecipeId = recipeId;
        Output = output;
        OutputCount = outputCount;
        Ingredients = ingredients ?? Array.Empty<CraftingIngredient>();
        Category = category;
        RequiredWorkstation = workstation;
        RequiredBuilding = building;
        RequiredTechnologies = technologies ?? Array.Empty<TechnologyKind>();
        IsUnlockedByDefault = unlockedByDefault;
    }

    public string GetDisplayName()
    {
        string outputName = ItemKindUtility.GetDisplayName(Output);
        return OutputCount > 1 ? $"{outputName} x{OutputCount}" : outputName;
    }

    public string GetIngredientsSummary()
    {
        if (Ingredients == null || Ingredients.Length == 0)
        {
            return string.Empty;
        }

        var parts = new string[Ingredients.Length];
        for (int i = 0; i < Ingredients.Length; i++)
        {
            parts[i] = $"{ItemKindUtility.GetDisplayName(Ingredients[i].Item)} x{Ingredients[i].Count}";
        }

        return string.Join(" + ", parts);
    }
}
