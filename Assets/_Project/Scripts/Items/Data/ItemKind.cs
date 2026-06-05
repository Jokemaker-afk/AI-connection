using System.Collections.Generic;
using UnityEngine;

public enum ItemKind
{
    None = 0,

    // Legacy Level4 colored blocks
    RedBlock,
    OrangeBlock,
    YellowBlock,
    GreenBlock,
    CyanBlock,
    BlueBlock,
    PurpleBlock,
    PinkBlock,
    WhiteBlock,
    GrayBlock,

    // Level5 basic materials
    Wood,
    Stone,
    Grass,
    Fiber,
    Vine,
    Flint,
    Clay,
    OreFragment,
    Coal,
    Berry,

    // Level5 intermediate materials
    Plank,
    Stick,
    Rope,
    Cloth,
    Brick,
    MetalIngot,

    // Level5 functional items
    CraftingTable,
    StoneAxe,
    StonePickaxe,
    RepairTool,
    SimpleBackpack,
    Campfire,
    Furnace,
    Bandage,
    BasicSword,
    TrainingBlaster,
    KeyFragment,
    Torch,
    BasicKnife,
    Hammer,

    // Building materials (placeable)
    WoodWall,
    StoneWall,
    WoodFloor,
    StoneFloor,

    // Additional workstations (placeable)
    Forge,
    Loom,
    AlchemyTable,
    ScienceLab,

    // Storage (placeable)
    WoodChest,
    LargeChest,
}

public static class ItemKindUtility
{
    public const int MaxStackSize = 64;
    static readonly Dictionary<ItemKind, Sprite> IconCache = new Dictionary<ItemKind, Sprite>();

    public static string GetDisplayName(ItemKind kind)
    {
        if (ItemCatalog.TryGet(kind, out ItemData data) && data.IsValid)
        {
            return data.DisplayName;
        }

        return string.Empty;
    }

    public static Color GetDisplayColor(ItemKind kind)
    {
        if (ItemCatalog.TryGet(kind, out ItemData data) && data.IsValid)
        {
            return data.DisplayColor;
        }

        return new Color(0.2f, 0.2f, 0.2f, 0f);
    }

    public static ItemCategory GetCategory(ItemKind kind)
    {
        if (ItemCatalog.TryGet(kind, out ItemData data) && data.IsValid)
        {
            return data.Category;
        }

        return ItemCategory.None;
    }

    public static int GetMaxStackSize(ItemKind kind)
    {
        if (ItemCatalog.TryGet(kind, out ItemData data) && data.IsValid)
        {
            return data.Stackable ? data.MaxStack : 1;
        }

        return MaxStackSize;
    }

    public static bool IsValid(ItemKind kind)
    {
        return kind != ItemKind.None && ItemCatalog.TryGet(kind, out _);
    }

    public static bool IsStackable(ItemKind kind)
    {
        if (ItemCatalog.TryGet(kind, out ItemData data) && data.IsValid)
        {
            return data.Stackable;
        }

        return false;
    }

    public static bool IsPlaceable(ItemKind kind)
    {
        if (ItemCatalog.TryGet(kind, out ItemData data) && data.IsValid)
        {
            return data.IsPlaceable;
        }

        return false;
    }

    public static bool IsRecoverableWhenPlaced(ItemKind kind)
    {
        if (ItemCatalog.TryGet(kind, out ItemData data) && data.IsValid)
        {
            return data.Recoverable;
        }

        return false;
    }

    public static bool CanBePlacedOnTop(ItemKind kind)
    {
        if (ItemCatalog.TryGet(kind, out ItemData data) && data.IsValid)
        {
            return data.CanBePlacedOnTop;
        }

        return false;
    }

    public static bool CanSupportPlacedObjects(ItemKind kind)
    {
        if (ItemCatalog.TryGet(kind, out ItemData data) && data.IsValid)
        {
            return data.CanSupportPlacedObjects;
        }

        return false;
    }

    public static bool AllowStacking(ItemKind kind)
    {
        if (ItemCatalog.TryGet(kind, out ItemData data) && data.IsValid)
        {
            return data.AllowStacking;
        }

        return false;
    }

    public static Vector3 GetPlacedBoundsSize(ItemKind kind)
    {
        if (ItemCatalog.TryGet(kind, out ItemData data) && data.IsValid && data.PlacedBoundsSize.sqrMagnitude > 0.01f)
        {
            return data.PlacedBoundsSize;
        }

        return Vector3.zero;
    }

    public static BuildingKind GetPlacedBuildingKind(ItemKind kind)
    {
        if (ItemCatalog.TryGet(kind, out ItemData data) && data.IsValid)
        {
            return data.PlacedBuildingKind;
        }

        return BuildingKind.None;
    }

    public static WorkstationKind GetPlacedWorkstationKind(ItemKind kind)
    {
        if (ItemCatalog.TryGet(kind, out ItemData data) && data.IsValid)
        {
            return data.PlacedWorkstationKind;
        }

        return WorkstationKind.None;
    }

    public static GameObject GetWorldPickupPrefab(ItemKind kind)
    {
        if (ItemCatalog.TryGet(kind, out ItemData data) && data.IsValid)
        {
            return data.WorldPickupPrefab;
        }

        return null;
    }

    public static GameObject GetPlacedPrefab(ItemKind kind)
    {
        if (ItemCatalog.TryGet(kind, out ItemData data) && data.IsValid)
        {
            return data.PlacedPrefab;
        }

        return null;
    }

    public static Sprite GetIcon(ItemKind kind)
    {
        if (ItemCatalog.TryGet(kind, out ItemData data) && data.IsValid && data.Icon != null)
        {
            return data.Icon;
        }

        return GetIconSprite(kind);
    }

    public static Sprite GetIconSprite(ItemKind kind)
    {
        if (!IsValid(kind))
        {
            return null;
        }

        if (IconCache.TryGetValue(kind, out var cached) && cached != null)
        {
            return cached;
        }

        var sprite = BuildIconSprite(kind);
        IconCache[kind] = sprite;
        return sprite;
    }

    static Sprite BuildIconSprite(ItemKind kind)
    {
        const int size = 24;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        Color baseColor = GetDisplayColor(kind);
        Color topColor = Color.Lerp(baseColor, Color.white, 0.25f);
        Color sideColor = Color.Lerp(baseColor, Color.black, 0.28f);
        Color borderColor = Color.Lerp(baseColor, Color.black, 0.45f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                texture.SetPixel(x, y, new Color(0f, 0f, 0f, 0f));
            }
        }

        // Diamond-ish "block top" icon.
        for (int y = 3; y < size - 3; y++)
        {
            for (int x = 3; x < size - 3; x++)
            {
                int dx = Mathf.Abs(x - size / 2);
                int dy = Mathf.Abs(y - size / 2);
                if (dx + dy > 8)
                {
                    continue;
                }

                bool onBorder = dx + dy >= 8 || x <= 4 || x >= size - 5 || y <= 4 || y >= size - 5;
                Color pixel = y > size / 2 ? sideColor : topColor;
                texture.SetPixel(x, y, onBorder ? borderColor : pixel);
            }
        }

        texture.Apply(false, false);

        return Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            100f);
    }

    public static bool IsHandheldTool(ItemKind kind)
    {
        return ItemCatalog.TryGet(kind, out ItemData data) && data.IsHandheldTool;
    }

    public static ToolKind GetToolKind(ItemKind kind)
    {
        if (ItemCatalog.TryGet(kind, out ItemData data) && data.IsHandheldTool)
        {
            return data.HandheldTool.ToolKind;
        }

        return ToolKind.None;
    }

    public static HandheldToolProfile GetHandheldToolProfile(ItemKind kind)
    {
        if (ItemCatalog.TryGet(kind, out ItemData data) && data.IsHandheldTool)
        {
            return data.HandheldTool;
        }

        return default;
    }

    public static bool IsWeapon(ItemKind kind)
    {
        return ItemCatalog.TryGet(kind, out ItemData data) && data.IsWeapon;
    }

    public static WeaponKind GetWeaponKind(ItemKind kind)
    {
        if (ItemCatalog.TryGet(kind, out ItemData data) && data.IsValid)
        {
            if (data.Weapon.IsWeapon)
            {
                return data.Weapon.WeaponKind;
            }

            return data.WeaponKind;
        }

        return WeaponKind.None;
    }

    public static WeaponProfile GetWeaponProfile(ItemKind kind)
    {
        if (ItemCatalog.TryGet(kind, out ItemData data) && data.IsWeapon)
        {
            return data.Weapon;
        }

        return default;
    }

    public static string GetRequiredToolPrompt(ToolKind required)
    {
        switch (required)
        {
            case ToolKind.Pickaxe:
                return "需要石镐";
            case ToolKind.Axe:
                return "需要石斧";
            case ToolKind.RepairTool:
                return "需要修理工具";
            default:
                return "需要对应工具";
        }
    }
}
