#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>One-shot setup for Vine complete world-pickup prefab.</summary>
public static class VineWorldPickupSetupMenu
{
    [MenuItem("Tools/Items/Setup World Pickup (Vine)")]
    public static void SetupVine()
    {
        Level8ResourceVisualIntegrationMenu.IntegrateVine();
        ResourceWorldPickupGameplayMergeMenu.MergeResourcePublic(ItemKind.Vine, ItemWorldPickupAssetPaths.VinePrefab);
        Level8ResourcePrefabRegistry.SetCachedForTests(null);
        Debug.Log("[Items] Setup complete for Vine world pickup.");
    }
}
#endif
