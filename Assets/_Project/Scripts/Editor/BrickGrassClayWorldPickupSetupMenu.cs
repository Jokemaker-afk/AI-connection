#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>One-shot setup for Brick, Grass, and Clay complete world-pickup prefabs.</summary>
public static class BrickGrassClayWorldPickupSetupMenu
{
    [MenuItem("Tools/Items/Setup World Pickups (Brick Grass Clay)")]
    public static void SetupAll()
    {
        SetupBrick();
        SetupGrass();
        SetupClay();
        Debug.Log("[Items] Setup complete for Brick, Grass, and Clay world pickups.");
    }

    [MenuItem("Tools/Items/Setup World Pickup (Brick)")]
    public static void SetupBrick()
    {
        CraftedMaterialWorldPickupSetupMenu.IntegrateBrick();
        CraftedMaterialWorldPickupSetupMenu.MergePickup(ItemKind.Brick, ItemWorldPickupAssetPaths.BrickPrefab);
        Level8ResourcePrefabRegistry.SetCachedForTests(null);
    }

    [MenuItem("Tools/Items/Setup World Pickup (Grass)")]
    public static void SetupGrass()
    {
        Level8ResourceVisualIntegrationMenu.IntegrateGrass();
        ResourceWorldPickupGameplayMergeMenu.MergeResourcePublic(ItemKind.Grass, ItemWorldPickupAssetPaths.GrassPrefab);
        Level8ResourcePrefabRegistry.SetCachedForTests(null);
    }

    [MenuItem("Tools/Items/Setup World Pickup (Clay)")]
    public static void SetupClay()
    {
        Level8ResourceVisualIntegrationMenu.IntegrateClay();
        ResourceWorldPickupGameplayMergeMenu.MergeResourcePublic(ItemKind.Clay, ItemWorldPickupAssetPaths.ClayPrefab);
        Level8ResourcePrefabRegistry.SetCachedForTests(null);
    }
}
#endif
