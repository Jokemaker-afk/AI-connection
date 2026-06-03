public enum CraftingLockReason
{
    None = 0,
    MissingMaterials,
    InventoryFull,
    MissingWorkstation,
    MissingBuilding,
    MissingTechnology,
    WrongWorkstationContext,
}

public struct CraftingUnlockResult
{
    public bool IsUnlocked;
    public bool CanCraftNow;
    public CraftingLockReason LockReason;
    public string Message;

    public static CraftingUnlockResult Unlocked(bool canCraft, CraftingLockReason reason, string message)
    {
        return new CraftingUnlockResult
        {
            IsUnlocked = true,
            CanCraftNow = canCraft,
            LockReason = reason,
            Message = message ?? string.Empty,
        };
    }

    public static CraftingUnlockResult Locked(CraftingLockReason reason, string message)
    {
        return new CraftingUnlockResult
        {
            IsUnlocked = false,
            CanCraftNow = false,
            LockReason = reason,
            Message = message ?? string.Empty,
        };
    }
}

public static class CraftingUnlockEvaluator
{
    public static bool IsRecipeUnlocked(CraftingRecipe recipe)
    {
        return Evaluate(recipe, WorkstationKind.None, null).IsUnlocked;
    }

    public static CraftingUnlockResult Evaluate(
        CraftingRecipe recipe,
        WorkstationKind activeWorkstation,
        PlayerInventory inventory)
    {
        if (string.IsNullOrEmpty(recipe.RecipeId))
        {
            return CraftingUnlockResult.Locked(CraftingLockReason.MissingTechnology, "无效配方。");
        }

        if (!recipe.IsUnlockedByDefault)
        {
            if (recipe.RequiredTechnologies != null && recipe.RequiredTechnologies.Length > 0)
            {
                var tech = TechnologyManager.Instance;
                if (tech == null)
                {
                    return CraftingUnlockResult.Locked(CraftingLockReason.MissingTechnology, "需要解锁科技。");
                }

                for (int i = 0; i < recipe.RequiredTechnologies.Length; i++)
                {
                    if (!tech.IsUnlocked(recipe.RequiredTechnologies[i]))
                    {
                        return CraftingUnlockResult.Locked(
                            CraftingLockReason.MissingTechnology,
                            $"需要科技：{GetTechnologyName(recipe.RequiredTechnologies[i])}");
                    }
                }
            }
        }

        if (recipe.RequiredBuilding != BuildingKind.None && !BuildingRegistry.HasBuilding(recipe.RequiredBuilding))
        {
            return CraftingUnlockResult.Locked(
                CraftingLockReason.MissingBuilding,
                $"需要先建造：{GetBuildingName(recipe.RequiredBuilding)}");
        }

        WorkstationKind requiredStation = recipe.RequiredWorkstation;
        if (requiredStation == WorkstationKind.Player)
        {
            requiredStation = WorkstationKind.None;
        }

        if (activeWorkstation == WorkstationKind.Player)
        {
            activeWorkstation = WorkstationKind.None;
        }

        if (inventory == null)
        {
            return CraftingUnlockResult.Locked(CraftingLockReason.MissingMaterials, "未找到背包。");
        }

        if (requiredStation == WorkstationKind.None)
        {
            return EvaluateCraftability(recipe, inventory);
        }

        if (activeWorkstation != requiredStation)
        {
            return CraftingUnlockResult.Locked(
                CraftingLockReason.WrongWorkstationContext,
                $"需要在工作站制作：{GetWorkstationName(requiredStation)}");
        }

        return EvaluateCraftability(recipe, inventory);
    }

    static CraftingUnlockResult EvaluateCraftability(CraftingRecipe recipe, PlayerInventory inventory)
    {
        if (!inventory.HasIngredients(recipe.Ingredients))
        {
            return CraftingUnlockResult.Unlocked(false, CraftingLockReason.MissingMaterials, "材料不足。");
        }

        if (!inventory.CanAcceptItem(recipe.Output, recipe.OutputCount))
        {
            return CraftingUnlockResult.Unlocked(false, CraftingLockReason.InventoryFull, "背包已满。");
        }

        return CraftingUnlockResult.Unlocked(true, CraftingLockReason.None, string.Empty);
    }

    public static bool MatchesCraftingContext(CraftingRecipe recipe, WorkstationKind context)
    {
        WorkstationKind required = recipe.RequiredWorkstation;
        if (required == WorkstationKind.Player)
        {
            required = WorkstationKind.None;
        }

        if (context == WorkstationKind.Player)
        {
            context = WorkstationKind.None;
        }

        return required == context;
    }

    static string GetWorkstationName(WorkstationKind kind)
    {
        switch (kind)
        {
            case WorkstationKind.Workbench: return "工作台";
            case WorkstationKind.Furnace: return "熔炉";
            case WorkstationKind.Forge: return "锻造台";
            case WorkstationKind.Loom: return "织布台";
            case WorkstationKind.AlchemyTable: return "炼金台";
            case WorkstationKind.ScienceLab: return "科技研究台";
            default: return "随身制作";
        }
    }

    static string GetBuildingName(BuildingKind kind)
    {
        switch (kind)
        {
            case BuildingKind.Workbench: return "工作台";
            case BuildingKind.Furnace: return "熔炉";
            case BuildingKind.Forge: return "锻造台";
            case BuildingKind.Loom: return "织布台";
            case BuildingKind.AlchemyTable: return "炼金台";
            case BuildingKind.ScienceLab: return "科技研究台";
            case BuildingKind.Campfire: return "营火";
            case BuildingKind.WoodChest: return "木箱";
            case BuildingKind.LargeChest: return "大型储物箱";
            case BuildingKind.WoodWall: return "木墙";
            case BuildingKind.StoneWall: return "石墙";
            case BuildingKind.WoodFloor: return "木地板";
            case BuildingKind.StoneFloor: return "石地板";
            default: return kind.ToString();
        }
    }

    static string GetTechnologyName(TechnologyKind kind)
    {
        switch (kind)
        {
            case TechnologyKind.BasicSurvival: return "基础生存";
            case TechnologyKind.Woodworking: return "木工";
            case TechnologyKind.Stonework: return "石器加工";
            case TechnologyKind.Smelting: return "熔炼";
            case TechnologyKind.Metalworking: return "金属加工";
            case TechnologyKind.Storage: return "储物系统";
            case TechnologyKind.BasicCombat: return "基础战斗";
            default: return kind.ToString();
        }
    }
}
