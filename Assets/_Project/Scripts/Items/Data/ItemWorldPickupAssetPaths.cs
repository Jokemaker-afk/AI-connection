/// <summary>
/// Standard world-pickup visual prefab paths (cross-level). FBX/materials/icons use separate folders.
/// Gameplay is merged via editor menus — prefabs include LogicRoot + WorldPickupItem when complete.
/// </summary>
public static class ItemWorldPickupAssetPaths
{
    public const string ResourcesPrefabRoot = "Assets/_Project/Prefabs/Items/WorldPickups/Resources/";
    public const string CraftedMaterialsPrefabRoot = "Assets/_Project/Prefabs/Items/WorldPickups/CraftedMaterials/";
    public const string ConsumablesPrefabRoot = "Assets/_Project/Prefabs/Items/WorldPickups/Consumables/";
    public const string ToolsPrefabRoot = "Assets/_Project/Prefabs/Items/WorldPickups/Tools/";

    public const string CraftedMaterialsFbxRoot = "Assets/_Project/Art/Imported/FBX/Items/CraftedMaterials/";
    public const string ConsumablesFbxRoot = "Assets/_Project/Art/Imported/FBX/Items/Consumables/";

    public const string WoodPrefab = ResourcesPrefabRoot + "PF_Generic_Resource_Wood_01.prefab";
    public const string StonePrefab = ResourcesPrefabRoot + "PF_Generic_Resource_Stone_01.prefab";
    public const string FiberPrefab = ResourcesPrefabRoot + "PF_Generic_Resource_Fiber_01.prefab";
    public const string OreFragmentPrefab = ResourcesPrefabRoot + "PF_Generic_Resource_OreFragment_01.prefab";
    public const string CoalPrefab = ResourcesPrefabRoot + "PF_Generic_Resource_Coal_01.prefab";
    public const string BerryPrefab = ResourcesPrefabRoot + "PF_Generic_Resource_Berry_01.prefab";
    public const string GrassPrefab = ResourcesPrefabRoot + "PF_Generic_Resource_Grass_01.prefab";
    public const string ClayPrefab = ResourcesPrefabRoot + "PF_Generic_Resource_Clay_01.prefab";
    public const string FlintPrefab = ResourcesPrefabRoot + "PF_Generic_Resource_Flint_01.prefab";
    public const string VinePrefab = ResourcesPrefabRoot + "PF_Generic_Resource_Vine_01.prefab";

    public const string StoneAxeWorldPickupPrefab = ToolsPrefabRoot + "PF_Generic_Item_StoneAxe_01.prefab";

    public const string StickPrefab = CraftedMaterialsPrefabRoot + "PF_Generic_Item_Stick_01.prefab";
    public const string PlankPrefab = CraftedMaterialsPrefabRoot + "PF_Generic_Item_Plank_01.prefab";
    public const string RopePrefab = CraftedMaterialsPrefabRoot + "PF_Generic_Item_Rope_01.prefab";
    public const string ClothPrefab = CraftedMaterialsPrefabRoot + "PF_Generic_Item_Cloth_01.prefab";
    public const string MetalIngotPrefab = CraftedMaterialsPrefabRoot + "PF_Generic_Item_MetalIngot_01.prefab";
    public const string BrickPrefab = CraftedMaterialsPrefabRoot + "PF_Generic_Item_Brick_01.prefab";
    public const string BandagePrefab = ConsumablesPrefabRoot + "PF_Generic_Item_Bandage_01.prefab";

    /// <summary>Legacy Level8-only folder — prefabs migrated to <see cref="ResourcesPrefabRoot"/>.</summary>
    public const string LegacyLevel8ResourcePrefabRoot = "Assets/_Project/Prefabs/Level8/ResourcePrefabs/";

    public static string PrefabForKind(ItemKind kind)
    {
        switch (kind)
        {
            case ItemKind.Wood: return WoodPrefab;
            case ItemKind.Stone: return StonePrefab;
            case ItemKind.Fiber: return FiberPrefab;
            case ItemKind.OreFragment: return OreFragmentPrefab;
            case ItemKind.Coal: return CoalPrefab;
            case ItemKind.Berry: return BerryPrefab;
            case ItemKind.Grass: return GrassPrefab;
            case ItemKind.Clay: return ClayPrefab;
            case ItemKind.Flint: return FlintPrefab;
            case ItemKind.Vine: return VinePrefab;
            case ItemKind.StoneAxe: return StoneAxeWorldPickupPrefab;
            case ItemKind.Stick: return StickPrefab;
            case ItemKind.Plank: return PlankPrefab;
            case ItemKind.Rope: return RopePrefab;
            case ItemKind.Cloth: return ClothPrefab;
            case ItemKind.MetalIngot: return MetalIngotPrefab;
            case ItemKind.Brick: return BrickPrefab;
            case ItemKind.Bandage: return BandagePrefab;
            default: return null;
        }
    }
}
