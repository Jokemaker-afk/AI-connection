using System.Collections.Generic;

public static class CraftingRecipeDatabase
{
    static readonly List<CraftingRecipe> Recipes = new List<CraftingRecipe>();

    static CraftingRecipeDatabase()
    {
        RegisterHandCraftRecipes();
        RegisterWorkbenchRecipes();
        RegisterLoomRecipes();
        RegisterFurnaceRecipes();
        RegisterForgeRecipes();
        RegisterAlchemyRecipes();
        RegisterBuildingMaterialRecipes();
        RegisterStorageRecipes();
    }

    public static IReadOnlyList<CraftingRecipe> AllRecipes => Recipes;

    public static IEnumerable<CraftingRecipe> GetRecipesForContext(WorkstationKind context)
    {
        for (int i = 0; i < Recipes.Count; i++)
        {
            if (CraftingUnlockEvaluator.MatchesCraftingContext(Recipes[i], context))
            {
                yield return Recipes[i];
            }
        }
    }

    public static CraftingRecipe GetById(string recipeId)
    {
        for (int i = 0; i < Recipes.Count; i++)
        {
            if (Recipes[i].RecipeId == recipeId)
            {
                return Recipes[i];
            }
        }

        return default;
    }

    static void Register(CraftingRecipe recipe)
    {
        Recipes.Add(recipe);
    }

    static void RegisterHandCraftRecipes()
    {
        Register(new CraftingRecipe(
            "plank",
            ItemKind.Plank,
            2,
            new[] { new CraftingIngredient(ItemKind.Wood, 1) },
            CraftingRecipeCategory.IntermediateMaterial,
            WorkstationKind.None,
            BuildingKind.None,
            true));

        Register(new CraftingRecipe(
            "stick_from_wood",
            ItemKind.Stick,
            2,
            new[] { new CraftingIngredient(ItemKind.Wood, 1) },
            CraftingRecipeCategory.IntermediateMaterial,
            WorkstationKind.None,
            BuildingKind.None,
            true));

        Register(new CraftingRecipe(
            "stick_from_plank",
            ItemKind.Stick,
            2,
            new[] { new CraftingIngredient(ItemKind.Plank, 1) },
            CraftingRecipeCategory.IntermediateMaterial,
            WorkstationKind.None,
            BuildingKind.None,
            true));

        Register(new CraftingRecipe(
            "rope_from_grass",
            ItemKind.Rope,
            1,
            new[] { new CraftingIngredient(ItemKind.Grass, 2) },
            CraftingRecipeCategory.IntermediateMaterial,
            WorkstationKind.None,
            BuildingKind.None,
            true));

        Register(new CraftingRecipe(
            "rope_from_fiber",
            ItemKind.Rope,
            1,
            new[] { new CraftingIngredient(ItemKind.Fiber, 3) },
            CraftingRecipeCategory.IntermediateMaterial,
            WorkstationKind.None,
            BuildingKind.None,
            true));

        Register(new CraftingRecipe(
            "campfire",
            ItemKind.Campfire,
            1,
            new[]
            {
                new CraftingIngredient(ItemKind.Wood, 3),
                new CraftingIngredient(ItemKind.Stone, 2),
                new CraftingIngredient(ItemKind.Coal, 1),
            },
            CraftingRecipeCategory.Workstation,
            WorkstationKind.None,
            BuildingKind.None,
            true,
            TechnologyKind.BasicSurvival));

        Register(new CraftingRecipe(
            "crafting_table",
            ItemKind.CraftingTable,
            1,
            new[] { new CraftingIngredient(ItemKind.Plank, 4) },
            CraftingRecipeCategory.Workstation,
            WorkstationKind.None,
            BuildingKind.None,
            true,
            TechnologyKind.BasicSurvival));
    }

    static void RegisterWorkbenchRecipes()
    {
        Register(new CraftingRecipe(
            "stone_axe",
            ItemKind.StoneAxe,
            1,
            new[]
            {
                new CraftingIngredient(ItemKind.Stick, 1),
                new CraftingIngredient(ItemKind.Stone, 2),
                new CraftingIngredient(ItemKind.Rope, 1),
            },
            CraftingRecipeCategory.Tool,
            WorkstationKind.Workbench,
            BuildingKind.Workbench,
            technologies: new[] { TechnologyKind.Stonework }));

        Register(new CraftingRecipe(
            "stone_pickaxe",
            ItemKind.StonePickaxe,
            1,
            new[]
            {
                new CraftingIngredient(ItemKind.Stick, 2),
                new CraftingIngredient(ItemKind.Stone, 3),
                new CraftingIngredient(ItemKind.Rope, 1),
            },
            CraftingRecipeCategory.Tool,
            WorkstationKind.Workbench,
            BuildingKind.Workbench,
            technologies: new[] { TechnologyKind.Stonework }));

        Register(new CraftingRecipe(
            "simple_backpack",
            ItemKind.SimpleBackpack,
            1,
            new[]
            {
                new CraftingIngredient(ItemKind.Cloth, 2),
                new CraftingIngredient(ItemKind.Rope, 2),
            },
            CraftingRecipeCategory.Tool,
            WorkstationKind.Workbench,
            BuildingKind.Workbench,
            technologies: new[] { TechnologyKind.Woodworking }));

        Register(new CraftingRecipe(
            "furnace",
            ItemKind.Furnace,
            1,
            new[]
            {
                new CraftingIngredient(ItemKind.Stone, 6),
                new CraftingIngredient(ItemKind.Clay, 2),
                new CraftingIngredient(ItemKind.Coal, 2),
            },
            CraftingRecipeCategory.Workstation,
            WorkstationKind.Workbench,
            BuildingKind.Workbench,
            technologies: new[] { TechnologyKind.Stonework }));

        Register(new CraftingRecipe(
            "forge",
            ItemKind.Forge,
            1,
            new[]
            {
                new CraftingIngredient(ItemKind.Stone, 4),
                new CraftingIngredient(ItemKind.Brick, 4),
                new CraftingIngredient(ItemKind.Coal, 2),
            },
            CraftingRecipeCategory.Workstation,
            WorkstationKind.Workbench,
            BuildingKind.Workbench,
            technologies: new[] { TechnologyKind.Smelting }));

        Register(new CraftingRecipe(
            "loom",
            ItemKind.Loom,
            1,
            new[]
            {
                new CraftingIngredient(ItemKind.Plank, 6),
                new CraftingIngredient(ItemKind.Rope, 2),
            },
            CraftingRecipeCategory.Workstation,
            WorkstationKind.Workbench,
            BuildingKind.Workbench,
            technologies: new[] { TechnologyKind.Woodworking }));

        Register(new CraftingRecipe(
            "alchemy_table",
            ItemKind.AlchemyTable,
            1,
            new[]
            {
                new CraftingIngredient(ItemKind.Plank, 4),
                new CraftingIngredient(ItemKind.Stone, 4),
                new CraftingIngredient(ItemKind.Berry, 3),
            },
            CraftingRecipeCategory.Workstation,
            WorkstationKind.Workbench,
            BuildingKind.Workbench,
            technologies: new[] { TechnologyKind.BasicSurvival }));

        Register(new CraftingRecipe(
            "science_lab",
            ItemKind.ScienceLab,
            1,
            new[]
            {
                new CraftingIngredient(ItemKind.Plank, 8),
                new CraftingIngredient(ItemKind.MetalIngot, 2),
                new CraftingIngredient(ItemKind.Coal, 4),
            },
            CraftingRecipeCategory.Workstation,
            WorkstationKind.Workbench,
            BuildingKind.Workbench,
            technologies: new[] { TechnologyKind.Metalworking }));
    }

    static void RegisterLoomRecipes()
    {
        Register(new CraftingRecipe(
            "cloth",
            ItemKind.Cloth,
            1,
            new[] { new CraftingIngredient(ItemKind.Fiber, 4) },
            CraftingRecipeCategory.IntermediateMaterial,
            WorkstationKind.Loom,
            BuildingKind.Loom,
            technologies: new[] { TechnologyKind.Woodworking }));
    }

    static void RegisterFurnaceRecipes()
    {
        Register(new CraftingRecipe(
            "brick",
            ItemKind.Brick,
            1,
            new[]
            {
                new CraftingIngredient(ItemKind.Clay, 2),
                new CraftingIngredient(ItemKind.Coal, 1),
            },
            CraftingRecipeCategory.IntermediateMaterial,
            WorkstationKind.Furnace,
            BuildingKind.Furnace,
            technologies: new[] { TechnologyKind.Smelting }));

        Register(new CraftingRecipe(
            "metal_ingot",
            ItemKind.MetalIngot,
            1,
            new[]
            {
                new CraftingIngredient(ItemKind.OreFragment, 3),
                new CraftingIngredient(ItemKind.Coal, 1),
            },
            CraftingRecipeCategory.IntermediateMaterial,
            WorkstationKind.Furnace,
            BuildingKind.Furnace,
            technologies: new[] { TechnologyKind.Smelting }));
    }

    static void RegisterForgeRecipes()
    {
        Register(new CraftingRecipe(
            "basic_sword",
            ItemKind.BasicSword,
            1,
            new[]
            {
                new CraftingIngredient(ItemKind.Stick, 1),
                new CraftingIngredient(ItemKind.MetalIngot, 2),
            },
            CraftingRecipeCategory.Weapon,
            WorkstationKind.Forge,
            BuildingKind.Forge,
            technologies: new[] { TechnologyKind.Metalworking, TechnologyKind.BasicCombat }));

        Register(new CraftingRecipe(
            "key_fragment",
            ItemKind.KeyFragment,
            1,
            new[]
            {
                new CraftingIngredient(ItemKind.MetalIngot, 1),
                new CraftingIngredient(ItemKind.Stone, 2),
            },
            CraftingRecipeCategory.Progression,
            WorkstationKind.Forge,
            BuildingKind.Forge,
            technologies: new[] { TechnologyKind.Metalworking }));
    }

    static void RegisterAlchemyRecipes()
    {
        Register(new CraftingRecipe(
            "bandage",
            ItemKind.Bandage,
            1,
            new[]
            {
                new CraftingIngredient(ItemKind.Cloth, 1),
                new CraftingIngredient(ItemKind.Berry, 1),
            },
            CraftingRecipeCategory.Consumable,
            WorkstationKind.AlchemyTable,
            BuildingKind.AlchemyTable,
            technologies: new[] { TechnologyKind.BasicSurvival }));
    }

    static void RegisterBuildingMaterialRecipes()
    {
        Register(new CraftingRecipe(
            "wood_wall",
            ItemKind.WoodWall,
            2,
            new[] { new CraftingIngredient(ItemKind.Plank, 2) },
            CraftingRecipeCategory.Building,
            WorkstationKind.None,
            BuildingKind.None,
            true,
            TechnologyKind.Woodworking));

        Register(new CraftingRecipe(
            "wood_floor",
            ItemKind.WoodFloor,
            2,
            new[] { new CraftingIngredient(ItemKind.Plank, 1) },
            CraftingRecipeCategory.Building,
            WorkstationKind.None,
            BuildingKind.None,
            true,
            TechnologyKind.Woodworking));

        Register(new CraftingRecipe(
            "stone_wall",
            ItemKind.StoneWall,
            2,
            new[]
            {
                new CraftingIngredient(ItemKind.Stone, 2),
                new CraftingIngredient(ItemKind.Brick, 1),
            },
            CraftingRecipeCategory.Building,
            WorkstationKind.Workbench,
            BuildingKind.Workbench,
            technologies: new[] { TechnologyKind.Stonework }));

        Register(new CraftingRecipe(
            "stone_floor",
            ItemKind.StoneFloor,
            2,
            new[]
            {
                new CraftingIngredient(ItemKind.Stone, 2),
            },
            CraftingRecipeCategory.Building,
            WorkstationKind.Workbench,
            BuildingKind.Workbench,
            technologies: new[] { TechnologyKind.Stonework }));
    }

    static void RegisterStorageRecipes()
    {
        Register(new CraftingRecipe(
            "wood_chest",
            ItemKind.WoodChest,
            1,
            new[]
            {
                new CraftingIngredient(ItemKind.Plank, 6),
                new CraftingIngredient(ItemKind.Rope, 1),
            },
            CraftingRecipeCategory.Storage,
            WorkstationKind.Workbench,
            BuildingKind.Workbench,
            technologies: new[] { TechnologyKind.Woodworking }));

        Register(new CraftingRecipe(
            "large_chest",
            ItemKind.LargeChest,
            1,
            new[]
            {
                new CraftingIngredient(ItemKind.Plank, 10),
                new CraftingIngredient(ItemKind.MetalIngot, 1),
                new CraftingIngredient(ItemKind.Rope, 2),
            },
            CraftingRecipeCategory.Storage,
            WorkstationKind.Workbench,
            BuildingKind.Workbench,
            technologies: new[] { TechnologyKind.Storage }));
    }
}
