#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>One-shot setup for Flint complete world-pickup prefab.</summary>
public static class FlintWorldPickupSetupMenu
{
    [MenuItem("Tools/Items/Setup World Pickup (Flint)")]
    public static void SetupFlint()
    {
        Level8ResourceVisualIntegrationMenu.IntegrateFlint();
        ResourceWorldPickupGameplayMergeMenu.MergeResourcePublic(ItemKind.Flint, ItemWorldPickupAssetPaths.FlintPrefab);
        Level8ResourcePrefabRegistry.SetCachedForTests(null);
        Debug.Log("[Items] Setup complete for Flint world pickup.");
    }
}
#endif
