using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class CraftingRecipeAuditor
{
    static bool auditLogged;

    public static void RunAuditAndLog(IReadOnlyList<CraftingRecipe> recipes, bool force = false)
    {
        if (auditLogged && !force)
        {
            return;
        }

        auditLogged = true;
        int validCount = 0;
        int issueCount = 0;
        var report = new StringBuilder(4096);
        report.AppendLine($"[CraftingAudit] Recipe audit: total recipes = {recipes.Count}");

        for (int i = 0; i < recipes.Count; i++)
        {
            CraftingRecipe recipe = recipes[i];
            AuditRecipe(recipe, report, ref validCount, ref issueCount);
        }

        report.AppendLine($"[CraftingAudit] Summary: valid={validCount}, issues={issueCount}");
        Debug.Log(report.ToString());
    }

    static void AuditRecipe(CraftingRecipe recipe, StringBuilder report, ref int validCount, ref int issueCount)
    {
        bool recipeValid = true;
        string outputName = ItemKindUtility.GetDisplayName(recipe.Output);
        string source = WorkstationDetector.GetColumnTitle(NormalizeStation(recipe.RequiredWorkstation));
        string category = CraftingRecipeCategoryUtility.GetDisplayName(recipe.Category);

        if (string.IsNullOrEmpty(recipe.RecipeId))
        {
            report.AppendLine("[CraftingAudit] Recipe missing RecipeId.");
            issueCount++;
            return;
        }

        if (!ItemKindUtility.IsValid(recipe.Output) || recipe.OutputCount <= 0)
        {
            report.AppendLine($"[CraftingAudit] Missing/invalid output ItemData: {recipe.RecipeId} -> {recipe.Output}");
            recipeValid = false;
            issueCount++;
        }

        if (recipe.Ingredients == null || recipe.Ingredients.Length == 0)
        {
            report.AppendLine($"[CraftingAudit] Recipe has no ingredients: {recipe.RecipeId}");
            recipeValid = false;
            issueCount++;
        }
        else
        {
            for (int i = 0; i < recipe.Ingredients.Length; i++)
            {
                CraftingIngredient ingredient = recipe.Ingredients[i];
                if (!ItemKindUtility.IsValid(ingredient.Item) || ingredient.Count <= 0)
                {
                    report.AppendLine(
                        $"[CraftingAudit] Missing ItemData for recipe ingredient: {recipe.RecipeId} -> {ingredient.Item} x{ingredient.Count}");
                    recipeValid = false;
                    issueCount++;
                }
            }
        }

        string ingredientSummary = recipe.GetIngredientsSummary();
        report.AppendLine(
            $"[CraftingAudit] Recipe: {outputName} ({recipe.RecipeId}) | output = {recipe.Output} x{recipe.OutputCount} | " +
            $"source = {source} | category = {category} | ingredients = {ingredientSummary} | unlockedByDefault = {recipe.IsUnlockedByDefault}");

        if (recipeValid)
        {
            validCount++;
        }
    }

    static WorkstationKind NormalizeStation(WorkstationKind station)
    {
        return station == WorkstationKind.Player ? WorkstationKind.None : station;
    }
}
