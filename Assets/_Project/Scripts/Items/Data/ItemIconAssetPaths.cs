/// <summary>
/// Standard 2D inventory icon asset paths (Sprite/PNG).
/// World pickup prefabs are separate — these paths are for backpack / hotbar / crafting UI only.
/// Bind via ItemCatalog Register(..., icon:) after PNG assets exist.
/// </summary>
public static class ItemIconAssetPaths
{
    public const string ResourcesRoot = "Assets/_Project/Art/UI/Items/Icons/Resources/";

    public const string Wood = ResourcesRoot + "Icon_Item_Wood.png";
    public const string Stone = ResourcesRoot + "Icon_Item_Stone.png";
    public const string Fiber = ResourcesRoot + "Icon_Item_Fiber.png";
    public const string OreFragment = ResourcesRoot + "Icon_Item_OreFragment.png";
    public const string Coal = ResourcesRoot + "Icon_Item_Coal.png";
    public const string Berry = ResourcesRoot + "Icon_Item_Berry.png";

    public static string ForKind(ItemKind kind)
    {
        switch (kind)
        {
            case ItemKind.Wood: return Wood;
            case ItemKind.Stone: return Stone;
            case ItemKind.Fiber: return Fiber;
            case ItemKind.OreFragment: return OreFragment;
            case ItemKind.Coal: return Coal;
            case ItemKind.Berry: return Berry;
            default: return null;
        }
    }
}
