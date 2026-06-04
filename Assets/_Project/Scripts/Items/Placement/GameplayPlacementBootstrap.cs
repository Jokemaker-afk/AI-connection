using UnityEngine;

/// <summary>
/// Persistent placement subsystem owned by GameplayCore.
/// Configures layer masks and validates that placement prerequisites are ready.
/// </summary>
public static class GameplayPlacementBootstrap
{
    static bool initialized;

    public static bool IsInitialized => initialized;

    public static void EnsureInitialized(string sceneName = null)
    {
        if (initialized && string.IsNullOrEmpty(sceneName))
        {
            return;
        }

        ConfigureLayerMasks();
        LogWorkbenchItemData();
        LogDirectPlayDiagnostics(sceneName);
        initialized = true;

        GameplayCore.Instance?.Log("GameplayPlacementBootstrap initialized.");
    }

    public static bool CanUsePlacement()
    {
        if (!GameplayCore.Exists)
        {
            return true;
        }

        return GameplayCore.Instance.Abilities.HasAbility(GameplayAbility.BuildingPlacement);
    }

    public static void ConfigureLayerMasks()
    {
        LayerMask surfaceMask = ~0;
        LayerMask blockingMask = ~0;

        if (GameplayLayers.HasPickupItemLayer)
        {
            surfaceMask &= ~GameplayLayers.PickupItemMask;
            blockingMask &= ~GameplayLayers.PickupItemMask;
        }

        blockingMask &= ~GameplayLayers.TargetingIgnoreMask;

        int ignoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
        if (ignoreRaycast >= 0)
        {
            surfaceMask &= ~(1 << ignoreRaycast);
            blockingMask &= ~(1 << ignoreRaycast);
        }

        PlacedObjectBuilder.PlacementSurfaceMask = surfaceMask;
        PlacedObjectBuilder.PlacementBlockingMask = blockingMask;
    }

    static void LogWorkbenchItemData()
    {
        if (!GameplayCore.Exists)
        {
            return;
        }

        if (ItemCatalog.TryGet(ItemKind.CraftingTable, out ItemData data) && data.IsValid)
        {
            GameplayCore.Instance.Log(
                $"Workbench ItemData: placeable={data.IsPlaceable}, recoverable={data.Recoverable}, " +
                $"building={data.PlacedBuildingKind}, workstation={data.PlacedWorkstationKind}");
        }
        else
        {
            GameplayCore.Instance.Log("Workbench ItemData: missing from ItemCatalog.");
        }
    }

    static void LogDirectPlayDiagnostics(string sceneName)
    {
        if (!GameplayCore.Exists || string.IsNullOrEmpty(sceneName))
        {
            return;
        }

        if (GameplaySceneCatalog.GetLevelNumber(sceneName) != 6)
        {
            return;
        }

        GameplayCore.Instance.Log("Scene6 direct play detected — verifying placement prerequisites.");

        GameObject player = PersistentPlayerRig.Player;
        if (player != null)
        {
            bool placement = player.GetComponent<PlayerPlacementController>() != null;
            bool inventory = player.GetComponent<PlayerInventory>() != null;
            GameplayCore.Instance.Log(
                $"Persistent PlayerRoot found | placement={placement} | inventory={inventory}");
        }
        else
        {
            GameplayCore.Instance.Log("Persistent PlayerRoot not yet created.");
        }

        GameplayCore.Instance.Log(
            $"BuildingPlacement ability active={CanUsePlacement()}");
    }
}
