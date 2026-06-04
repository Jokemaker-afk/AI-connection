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
#if UNITY_EDITOR
        ItemModuleValidation.ValidateCatalog();
#endif
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
        Sprite icon = null,
        HandheldToolProfile handheldTool = default,
        WeaponKind weaponKind = WeaponKind.None,
        string descriptionChinese = null,
        bool recoverable = true,
        bool craftable = false,
        WorkstationKind requiredWorkstation = WorkstationKind.None,
        GameObject futureModelPrefab = null,
        Texture futureTextureReference = null)
    {
        Items[id] = new ItemData
        {
            Id = id,
            DisplayName = name,
            DescriptionChinese = descriptionChinese ?? name,
            Category = category,
            Stackable = stackable,
            MaxStack = maxStack,
            DisplayColor = color,
            IsPlaceable = placeable,
            Recoverable = recoverable,
            Craftable = craftable,
            RequiredWorkstation = requiredWorkstation,
            CanBePlacedOnTop = canBePlacedOnTop,
            CanSupportPlacedObjects = canSupportPlacedObjects,
            AllowStacking = allowStacking,
            PlacedBuildingKind = buildingKind,
            PlacedWorkstationKind = workstationKind,
            PlacedBoundsSize = placedBoundsSize,
            WorldPickupPrefab = worldPickupPrefab,
            PlacedPrefab = placedPrefab,
            Icon = icon,
            HandheldTool = handheldTool,
            WeaponKind = weaponKind,
            FutureModelPrefab = futureModelPrefab,
            FutureTextureReference = futureTextureReference,
        };
    }

    static HandheldToolProfile CreateDefaultToolProfile(
        ToolKind toolKind,
        float useCooldown = 0.45f,
        Vector3? firstPersonPosition = null,
        Vector3? firstPersonEuler = null,
        Vector3? thirdPersonPosition = null,
        Vector3? thirdPersonEuler = null)
    {
        return new HandheldToolProfile
        {
            ToolKind = toolKind,
            FirstPersonLocalPosition = firstPersonPosition ?? new Vector3(0.05f, -0.04f, 0.08f),
            FirstPersonLocalEuler = firstPersonEuler ?? new Vector3(8f, -12f, 0f),
            FirstPersonLocalScale = Vector3.one,
            ThirdPersonLocalPosition = thirdPersonPosition ?? Vector3.zero,
            ThirdPersonLocalEuler = thirdPersonEuler ?? Vector3.zero,
            ThirdPersonLocalScale = new Vector3(1f, 1f, 1f),
            Animations = PrototypeAnimationFactory.CreateToolAnimationProfile(toolKind),
            UseCooldown = useCooldown,
            FallbackSwingEnabled = true,
            HasDurability = false,
            MaxDurability = 0,
        };
    }

    static void RegisterHandheldTool(
        ItemKind id,
        string name,
        ToolKind toolKind,
        Color color,
        float useCooldown = 0.45f,
        string descriptionChinese = null,
        WeaponKind weaponKind = WeaponKind.None)
    {
        Register(
            id,
            name,
            ItemCategory.Tool,
            color,
            stackable: false,
            maxStack: 1,
            descriptionChinese: descriptionChinese ?? $"{name}，可用于对应交互目标。",
            recoverable: false,
            craftable: true,
            handheldTool: CreateDefaultToolProfile(toolKind, useCooldown),
            weaponKind: weaponKind);
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
        RegisterMaterial(ItemKind.Wood, "木材", "基础材料，可用于合成与建造。", new Color(0.55f, 0.36f, 0.18f));
        RegisterMaterial(ItemKind.Stone, "石头", "基础材料，可用于合成与建造。", new Color(0.58f, 0.58f, 0.62f));
        RegisterMaterial(ItemKind.Grass, "草", "基础材料，可用于合成。", new Color(0.42f, 0.78f, 0.28f));
        RegisterMaterial(ItemKind.Fiber, "纤维", "基础材料，可用于制作绳子与布料。", new Color(0.72f, 0.82f, 0.38f));
        RegisterMaterial(ItemKind.Vine, "藤蔓", "基础材料，可用于合成。", new Color(0.28f, 0.55f, 0.22f));
        RegisterMaterial(ItemKind.Flint, "燧石", "基础材料，可用于工具制作。", new Color(0.35f, 0.38f, 0.42f));
        RegisterMaterial(ItemKind.Clay, "黏土", "基础材料，可用于烧制砖块。", new Color(0.72f, 0.48f, 0.35f));
        RegisterMaterial(ItemKind.OreFragment, "矿石碎片", "基础材料，可冶炼为金属锭。", new Color(0.48f, 0.42f, 0.78f));
        RegisterMaterial(ItemKind.Coal, "煤炭", "燃料材料，可用于熔炼。", new Color(0.18f, 0.18f, 0.2f));
        Register(
            ItemKind.Berry,
            "浆果",
            ItemCategory.Consumable,
            new Color(0.82f, 0.18f, 0.42f),
            maxStack: 32,
            descriptionChinese: "可食用的浆果。",
            craftable: false,
            recoverable: false);
    }

    static void RegisterMaterial(ItemKind id, string name, string description, Color color)
    {
        Register(
            id,
            name,
            ItemCategory.BasicMaterial,
            color,
            stackable: true,
            maxStack: 64,
            descriptionChinese: description,
            craftable: true,
            recoverable: false);
    }

    static void RegisterIntermediateMaterials()
    {
        RegisterIntermediate(ItemKind.Plank, "木板", "中级材料，由木材加工获得。", new Color(0.72f, 0.52f, 0.28f));
        RegisterIntermediate(ItemKind.Stick, "木棍", "中级材料，可用于工具与建筑。", new Color(0.62f, 0.42f, 0.2f));
        RegisterIntermediate(ItemKind.Rope, "绳子", "中级材料，由纤维编织获得。", new Color(0.78f, 0.68f, 0.32f));
        RegisterIntermediate(ItemKind.Cloth, "布料", "中级材料，可用于背包与装备。", new Color(0.88f, 0.86f, 0.72f));
        RegisterIntermediate(ItemKind.Brick, "砖块", "中级材料，由黏土烧制获得。", new Color(0.68f, 0.32f, 0.24f));
        RegisterIntermediate(ItemKind.MetalIngot, "金属锭", "中级材料，由矿石冶炼获得。", new Color(0.72f, 0.74f, 0.82f));
    }

    static void RegisterIntermediate(ItemKind id, string name, string description, Color color)
    {
        Register(
            id,
            name,
            ItemCategory.IntermediateMaterial,
            color,
            stackable: true,
            maxStack: 64,
            descriptionChinese: description,
            craftable: true,
            recoverable: false);
    }

    static void RegisterToolsAndWeapons()
    {
        RegisterHandheldTool(ItemKind.StonePickaxe, "石镐", ToolKind.Pickaxe, new Color(0.55f, 0.58f, 0.64f), descriptionChinese: "石镐，用于采矿。");
        RegisterHandheldTool(ItemKind.StoneAxe, "石斧", ToolKind.Axe, new Color(0.62f, 0.62f, 0.66f), useCooldown: 0.42f, descriptionChinese: "石斧，用于伐木。");
        RegisterHandheldTool(ItemKind.RepairTool, "修理工具", ToolKind.RepairTool, new Color(0.72f, 0.68f, 0.42f), useCooldown: 0.5f, descriptionChinese: "修理工具，用于修复中继器。");
        RegisterHandheldTool(ItemKind.BasicKnife, "简易小刀", ToolKind.Knife, new Color(0.68f, 0.72f, 0.78f), useCooldown: 0.35f, descriptionChinese: "简易小刀，可用于切割与近战。");
        RegisterHandheldTool(ItemKind.Hammer, "锤子", ToolKind.Hammer, new Color(0.58f, 0.52f, 0.48f), useCooldown: 0.48f, descriptionChinese: "锤子，可用于建造与敲击。");
        Register(
            ItemKind.SimpleBackpack,
            "简易背包",
            ItemCategory.Tool,
            new Color(0.48f, 0.36f, 0.22f),
            stackable: false,
            maxStack: 1,
            descriptionChinese: "扩大背包容量的工具。",
            craftable: true,
            recoverable: false);
        Register(
            ItemKind.BasicSword,
            "基础剑",
            ItemCategory.Weapon,
            new Color(0.78f, 0.8f, 0.88f),
            stackable: false,
            maxStack: 1,
            descriptionChinese: "基础近战武器。",
            craftable: true,
            recoverable: false,
            weaponKind: WeaponKind.Melee);
    }

    static void RegisterConsumables()
    {
        Register(
            ItemKind.Bandage,
            "绷带",
            ItemCategory.Consumable,
            new Color(0.95f, 0.92f, 0.82f),
            maxStack: 16,
            descriptionChinese: "恢复生命值的消耗品。",
            craftable: true,
            recoverable: false);

        Register(
            ItemKind.Torch,
            "火把",
            ItemCategory.Consumable,
            new Color(0.95f, 0.62f, 0.22f),
            stackable: true,
            maxStack: 16,
            descriptionChinese: "火把，可用于照明。",
            craftable: true,
            recoverable: false);
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
            allowStacking: false,
            descriptionChinese: "工作台，放置后可打开制造界面。",
            recoverable: true,
            craftable: true,
            requiredWorkstation: WorkstationKind.None,
            placedBoundsSize: new Vector3(1.6f, 0.9f, 1.2f));

        Register(
            ItemKind.Campfire,
            "营火",
            ItemCategory.Workstation,
            new Color(0.92f, 0.42f, 0.18f),
            stackable: false,
            maxStack: 1,
            placeable: true,
            buildingKind: BuildingKind.Campfire,
            canBePlacedOnTop: true,
            canSupportPlacedObjects: false,
            allowStacking: false,
            descriptionChinese: "营火，放置后可进行烹饪。",
            recoverable: true,
            craftable: true,
            placedBoundsSize: new Vector3(1.2f, 0.5f, 1.2f));

        Register(
            ItemKind.Furnace,
            "熔炉",
            ItemCategory.Workstation,
            new Color(0.42f, 0.42f, 0.46f),
            stackable: false,
            maxStack: 1,
            placeable: true,
            BuildingKind.Furnace,
            WorkstationKind.Furnace,
            descriptionChinese: "熔炉，放置后可进行熔炼制造。",
            recoverable: true,
            craftable: true,
            placedBoundsSize: new Vector3(1.4f, 1f, 1.2f));

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
        Register(
            ItemKind.KeyFragment,
            "钥匙碎片",
            ItemCategory.ProgressionItem,
            new Color(0.95f, 0.82f, 0.28f),
            stackable: false,
            maxStack: 1,
            descriptionChinese: "剧情推进物品，用于解锁区域。",
            craftable: false,
            recoverable: false);
    }
}
