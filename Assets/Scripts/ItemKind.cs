using System.Collections.Generic;
using UnityEngine;

public enum ItemKind
{
    None = 0,
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
}

public static class ItemKindUtility
{
    public const int MaxStackSize = 64;
    static readonly Dictionary<ItemKind, Sprite> IconCache = new Dictionary<ItemKind, Sprite>();

    public static string GetDisplayName(ItemKind kind)
    {
        switch (kind)
        {
            case ItemKind.RedBlock: return "红色块";
            case ItemKind.OrangeBlock: return "橙色块";
            case ItemKind.YellowBlock: return "黄色块";
            case ItemKind.GreenBlock: return "绿色块";
            case ItemKind.CyanBlock: return "青色块";
            case ItemKind.BlueBlock: return "蓝色块";
            case ItemKind.PurpleBlock: return "紫色块";
            case ItemKind.PinkBlock: return "粉色块";
            case ItemKind.WhiteBlock: return "白色块";
            case ItemKind.GrayBlock: return "灰色块";
            default: return string.Empty;
        }
    }

    public static Color GetDisplayColor(ItemKind kind)
    {
        switch (kind)
        {
            case ItemKind.RedBlock: return new Color(0.92f, 0.28f, 0.24f);
            case ItemKind.OrangeBlock: return new Color(0.98f, 0.55f, 0.18f);
            case ItemKind.YellowBlock: return new Color(0.98f, 0.86f, 0.2f);
            case ItemKind.GreenBlock: return new Color(0.32f, 0.82f, 0.38f);
            case ItemKind.CyanBlock: return new Color(0.28f, 0.82f, 0.88f);
            case ItemKind.BlueBlock: return new Color(0.28f, 0.48f, 0.95f);
            case ItemKind.PurpleBlock: return new Color(0.62f, 0.38f, 0.92f);
            case ItemKind.PinkBlock: return new Color(0.95f, 0.45f, 0.72f);
            case ItemKind.WhiteBlock: return new Color(0.92f, 0.92f, 0.92f);
            case ItemKind.GrayBlock: return new Color(0.55f, 0.58f, 0.62f);
            default: return new Color(0.2f, 0.2f, 0.2f, 0f);
        }
    }

    public static bool IsValid(ItemKind kind)
    {
        return kind != ItemKind.None;
    }

    public static bool IsStackable(ItemKind kind)
    {
        return IsValid(kind);
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
}
