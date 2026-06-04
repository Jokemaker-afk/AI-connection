#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class CraftingPipelineEditorTest
{
    [MenuItem("Tools/Crafting/Run Stone Axe Pipeline Test")]
    static void RunStoneAxePipelineTest()
    {
        var host = new GameObject("CraftingPipelineEditorTestHost");
        var inventory = host.AddComponent<PlayerInventory>();

        TestIntermediateRecipe("stick_from_wood", ItemKind.Wood, 1, ItemKind.Stick, 2, inventory);
        TestIntermediateRecipe("rope_from_grass", ItemKind.Grass, 2, ItemKind.Rope, 1, inventory);

        UnifiedInventoryAcquisition.TryAcquire(inventory, ItemKind.Stick, 1);
        UnifiedInventoryAcquisition.TryAcquire(inventory, ItemKind.Stone, 2);
        UnifiedInventoryAcquisition.TryAcquire(inventory, ItemKind.Rope, 1);

        CraftingRecipe stoneAxe = CraftingRecipeDatabase.GetById("stone_axe");
        CraftingUnlockResult evaluate = CraftingUnlockEvaluator.Evaluate(stoneAxe, WorkstationKind.None, inventory);
        Debug.Log(
            $"[CraftingTest] Stone Axe evaluate: unlocked={evaluate.IsUnlocked}, canCraft={evaluate.CanCraftNow}, message={evaluate.Message}");

        bool crafted = CraftingManager.TryCraft(inventory, stoneAxe, WorkstationKind.None, out string message);
        int axeCount = inventory.CountItem(ItemKind.StoneAxe);
        Debug.Log($"[CraftingTest] Stone Axe TryCraft: success={crafted}, message={message}, axeCount={axeCount}");

        if (!crafted || axeCount <= 0)
        {
            Debug.LogError("[CraftingTest] Stone Axe pipeline FAILED.");
        }
        else
        {
            Debug.Log("[CraftingTest] Stone Axe pipeline PASSED.");
        }

        TestToolRecipe("stone_pickaxe", ItemKind.StonePickaxe, inventory);
        TestToolRecipe("repair_tool", ItemKind.RepairTool, inventory);

        Object.DestroyImmediate(host);
    }

    static void TestToolRecipe(string recipeId, ItemKind outputKind, PlayerInventory inventory)
    {
        CraftingRecipe recipe = CraftingRecipeDatabase.GetById(recipeId);
        UnifiedInventoryAcquisition.TryAcquire(inventory, ItemKind.Stick, 3);
        UnifiedInventoryAcquisition.TryAcquire(inventory, ItemKind.Stone, 5);
        UnifiedInventoryAcquisition.TryAcquire(inventory, ItemKind.Rope, 2);

        bool crafted = CraftingManager.TryCraft(inventory, recipe, WorkstationKind.None, out string message);
        int count = inventory.CountItem(outputKind);
        Debug.Log($"[CraftingTest] {recipeId}: success={crafted}, count={count}, message={message}");
        if (!crafted || count <= 0)
        {
            Debug.LogError($"[CraftingTest] Tool recipe failed: {recipeId}");
        }
    }

    static void TestIntermediateRecipe(
        string recipeId,
        ItemKind inputKind,
        int inputCount,
        ItemKind outputKind,
        int outputCount,
        PlayerInventory inventory)
    {
        inventory.TryRemoveItem(inputKind, inventory.CountItem(inputKind));
        inventory.TryRemoveItem(outputKind, inventory.CountItem(outputKind));
        UnifiedInventoryAcquisition.TryAcquire(inventory, inputKind, inputCount);

        CraftingRecipe recipe = CraftingRecipeDatabase.GetById(recipeId);
        bool success = CraftingManager.TryCraft(inventory, recipe, WorkstationKind.None, out string message);
        int output = inventory.CountItem(outputKind);
        Debug.Log(
            $"[CraftingTest] {recipeId}: success={success}, output={output}/{outputCount}, message={message}");

        if (!success || output < outputCount)
        {
            Debug.LogError($"[CraftingTest] Intermediate recipe failed: {recipeId}");
        }
    }
}
#endif
