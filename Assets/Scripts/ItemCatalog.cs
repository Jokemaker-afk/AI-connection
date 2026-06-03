using System.Collections.Generic;
using UnityEngine;

public static class ItemCatalog
{
    static readonly Dictionary<ItemKind, ItemData> Items = new Dictionary<ItemKind, ItemData>();

    static ItemCatalog()
    {
        RegisterLegacyBlocks();
        RegisterBasicMaterials();
        RegisterIntermediateMaterials();
        RegisterToolsAndWeapons();
        RegisterConsumables();
        RegisterWorkstations();
        RegisterStorage();
        RegisterBuildingMaterials();
        RegisterProgressionItems();
    }

    public static ItemData Get(ItemKind kind)
    {
        if (Items.TryGetValue(kind, out ItemData data))
        {
            return data;
        }

        return default;
    }

    public static bool TryGet(ItemKind kind, out ItemData data)
    {
        return Items.TryGetValue(kind, out data);
    }

    public static IEnumerable<ItemKind> AllRegisteredKinds => Items.Keys;

    static void Register(
        ItemKind id,
        string name,
        ItemCategory category,
        Color color,
        bool stackable = true,
        int maxStack = 64,
        bool placeable = false,
        BuildingKind buildingKind = BuildingKind.None,
        WorkstationKind workstationKind = WorkstationKind.None,
        bool canBePlacedOnTop = true,
        bool canSupportPlacedObjects = false,
        bool allowStacking = true,
        Vector3 placedBoundsSize = default,
        GameObject worldPickupPrefab = null,
        GameObject placedPrefab = null,
        Sprite icon = null)
    {
        Items[id] = new ItemData
        {
            Id = id,
            DisplayName = name,
            Category = category,
            Stackable = stackable,
            MaxStack = maxStack,
            DisplayColor = color,
            IsPlaceable = placeable,
            CanBePlacedOnTop = canBePlacedOnTop,
            CanSupportPlacedObjects = canSupportPlacedObjects,
            AllowStacking = allowStacking,
            PlacedBuildingKind = buildingKind,
            PlacedWorkstationKind = workstationKind,
            PlacedBoundsSize = placedBoundsSize,
            WorldPickupPrefab = worldPickupPrefab,
            PlacedPrefab = placedPrefab,
            Icon = icon,
        };
    }

    static void RegisterLegacyBlocks()
    {
        Register(ItemKind.RedBlock, "红色方块", ItemCategory.LegacyBlock, new Color(0.92f, 0.28f, 0.24f));
        Register(ItemKind.OrangeBlock, "橙色方块", ItemCategory.LegacyBlock, new Color(0.98f, 0.55f, 0.18f));
        Register(ItemKind.YellowBlock, "黄色方块", ItemCategory.LegacyBlock, new Color(0.98f, 0.86f, 0.2f));
        Register(ItemKind.GreenBlock, "绿色方块", ItemCategory.LegacyBlock, new Color(0.32f, 0.82f, 0.38f));
        Register(ItemKind.CyanBlock, "青色方块", ItemCategory.LegacyBlock, new Color(0.28f, 0.82f, 0.88f));
        Register(ItemKind.BlueBlock, "蓝色方块", ItemCategory.LegacyBlock, new Color(0.28f, 0.48f, 0.95f));
        Register(ItemKind.PurpleBlock, "紫色方块", ItemCategory.LegacyBlock, new Color(0.62f, 0.38f, 0.92f));
        Register(ItemKind.PinkBlock, "粉色方块", ItemCategory.LegacyBlock, new Color(0.95f, 0.45f, 0.72f));
        Register(ItemKind.WhiteBlock, "白色方块", ItemCategory.LegacyBlock, new Color(0.92f, 0.92f, 0.92f));
        Register(ItemKind.GrayBlock, "灰色方块", ItemCategory.LegacyBlock, new Color(0.55f, 0.58f, 0.62f));
    }

    static void RegisterBasicMaterials()
    {
        Register(ItemKind.Wood, "木材", ItemCategory.BasicMaterial, new Color(0.55f, 0.36f, 0.18f));
        Register(ItemKind.Stone, "石头", ItemCategory.BasicMaterial, new Color(0.58f, 0.58f, 0.62f));
        Register(ItemKind.Grass, "草", ItemCategory.BasicMaterial, new Color(0.42f, 0.78f, 0.28f));
        Register(ItemKind.Fiber, "纤维", ItemCategory.BasicMaterial, new Color(0.72f, 0.82f, 0.38f));
        Register(ItemKind.Vine, "藤蔓", ItemCategory.BasicMaterial, new Color(0.28f, 0.55f, 0.22f));
        Register(ItemKind.Flint, "燧石", ItemCategory.BasicMaterial, new Color(0.35f, 0.38f, 0.42f));
        Register(ItemKind.Clay, "黏土", ItemCategory.BasicMaterial, new Color(0.72f, 0.48f, 0.35f));
        Register(ItemKind.OreFragment, "矿石碎片", ItemCategory.BasicMaterial, new Color(0.48f, 0.42f, 0.78f));
        Register(ItemKind.Coal, "煤炭", ItemCategory.BasicMaterial, new Color(0.18f, 0.18f, 0.2f));
        Register(ItemKind.Berry, "浆果", ItemCategory.Consumable, new Color(0.82f, 0.18f, 0.42f));
    }

    static void RegisterIntermediateMaterials()
    {
        Register(ItemKind.Plank, "木板", ItemCategory.IntermediateMaterial, new Color(0.72f, 0.52f, 0.28f));
        Register(ItemKind.Stick, "木棍", ItemCategory.IntermediateMaterial, new Color(0.62f, 0.42f, 0.2f));
        Register(ItemKind.Rope, "绳子", ItemCategory.IntermediateMaterial, new Color(0.78f, 0.68f, 0.32f));
        Register(ItemKind.Cloth, "布料", ItemCategory.IntermediateMaterial, new Color(0.88f, 0.86f, 0.72f));
        Register(ItemKind.Brick, "砖块", ItemCategory.IntermediateMaterial, new Color(0.68f, 0.32f, 0.24f));
        Register(ItemKind.MetalIngot, "金属锭", ItemCategory.IntermediateMaterial, new Color(0.72f, 0.74f, 0.82f));
    }

    static void RegisterToolsAndWeapons()
    {
        Register(ItemKind.StoneAxe, "石斧", ItemCategory.Tool, new Color(0.62f, 0.62f, 0.66f), stackable: false, maxStack: 1);
        Register(ItemKind.StonePickaxe, "石镐", ItemCategory.Tool, new Color(0.55f, 0.58f, 0.64f), stackable: false, maxStack: 1);
        Register(ItemKind.SimpleBackpack, "简易背包", ItemCategory.Tool, new Color(0.48f, 0.36f, 0.22f), stackable: false, maxStack: 1);
        Register(ItemKind.BasicSword, "基础剑", ItemCategory.Weapon, new Color(0.78f, 0.8f, 0.88f), stackable: false, maxStack: 1);
    }

    static void RegisterConsumables()
    {
        Register(ItemKind.Bandage, "绷带", ItemCategory.Consumable, new Color(0.95f, 0.92f, 0.82f));
    }

    static void RegisterWorkstations()
    {
        Register(
            ItemKind.CraftingTable,
            "工作台",
            ItemCategory.Workstation,
            new Color(0.58f, 0.4f, 0.22f),
            stackable: false,
            maxStack: 1,
            placeable: true,
            BuildingKind.Workbench,
            WorkstationKind.Workbench,
            canBePlacedOnTop: true,
            canSupportPlacedObjects: false,
            allowStacking: false);

        Register(
            ItemKind.Campfire,
            "营火",
            ItemCategory.Workstation,
            new Color(0.92f, 0.42f, 0.18f),
            stackable: false,
            maxStack: 1,
            placeable: true,
            BuildingKind.Campfire,
            canBePlacedOnTop: true,
            canSupportPlacedObjects: false,
            allowStacking: false);

        Register(
            ItemKind.Furnace,
            "熔炉",
            ItemCategory.Workstation,
            new Color(0.42f, 0.42f, 0.46f),
            stackable: false,
            maxStack: 1,
            placeable: true,
            BuildingKind.Furnace,
            WorkstationKind.Furnace);

        Register(
            ItemKind.Forge,
            "锻造台",
            ItemCategory.Workstation,
            new Color(0.5f, 0.32f, 0.28f),
            stackable: false,
            maxStack: 1,
            placeable: true,
            BuildingKind.Forge,
            WorkstationKind.Forge);

        Register(
            ItemKind.Loom,
            "织布台",
            ItemCategory.Workstation,
            new Color(0.72f, 0.55f, 0.68f),
            stackable: false,
            maxStack: 1,
            placeable: true,
            BuildingKind.Loom,
            WorkstationKind.Loom);

        Register(
            ItemKind.AlchemyTable,
            "炼金台",
            ItemCategory.Workstation,
            new Color(0.42f, 0.72f, 0.48f),
            stackable: false,
            maxStack: 1,
            placeable: true,
            BuildingKind.AlchemyTable,
            WorkstationKind.AlchemyTable);

        Register(
            ItemKind.ScienceLab,
            "科技研究台",
            ItemCategory.Workstation,
            new Color(0.38f, 0.55f, 0.88f),
            stackable: false,
            maxStack: 1,
            placeable: true,
            BuildingKind.ScienceLab,
            WorkstationKind.ScienceLab);
    }

    static void RegisterStorage()
    {
        Register(
            ItemKind.WoodChest,
            "木箱",
            ItemCategory.Storage,
            new Color(0.52f, 0.38f, 0.22f),
            stackable: false,
            maxStack: 1,
            placeable: true,
            BuildingKind.WoodChest,
            canSupportPlacedObjects: false,
            allowStacking: false);

        Register(
            ItemKind.LargeChest,
            "大型储物箱",
            ItemCategory.Storage,
            new Color(0.48f, 0.34f, 0.2f),
            stackable: false,
            maxStack: 1,
            placeable: true,
            BuildingKind.LargeChest,
            canSupportPlacedObjects: false,
            allowStacking: false);
    }

    static void RegisterBuildingMaterials()
    {
        Register(
            ItemKind.WoodWall,
            "木墙",
            ItemCategory.BuildingMaterial,
            new Color(0.62f, 0.44f, 0.26f),
            placeable: true,
            buildingKind: BuildingKind.WoodWall,
            canSupportPlacedObjects: true,
            allowStacking: true);

        Register(
            ItemKind.StoneWall,
            "石墙",
            ItemCategory.BuildingMaterial,
            new Color(0.5f, 0.52f, 0.56f),
            placeable: true,
            buildingKind: BuildingKind.StoneWall,
            canSupportPlacedObjects: true,
            allowStacking: true);

        Register(
            ItemKind.WoodFloor,
            "木地板",
            ItemCategory.BuildingMaterial,
            new Color(0.68f, 0.5f, 0.3f),
            placeable: true,
            buildingKind: BuildingKind.WoodFloor,
            canSupportPlacedObjects: true,
            allowStacking: true);

        Register(
            ItemKind.StoneFloor,
            "石地板",
            ItemCategory.BuildingMaterial,
            new Color(0.55f, 0.56f, 0.6f),
            placeable: true,
            buildingKind: BuildingKind.StoneFloor,
            canSupportPlacedObjects: true,
            allowStacking: true);
    }

    static void RegisterProgressionItems()
    {
        Register(ItemKind.KeyFragment, "钥匙碎片", ItemCategory.ProgressionItem, new Color(0.95f, 0.82f, 0.28f), stackable: false, maxStack: 1);
    }
}
