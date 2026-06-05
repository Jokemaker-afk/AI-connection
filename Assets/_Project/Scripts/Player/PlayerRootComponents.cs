using UnityEngine;

/// <summary>
/// Central registry for gameplay components that belong on PlayerRoot, not on visual meshes.
/// </summary>
public static class PlayerRootComponents
{
    public static void EnsureAll(GameObject playerRoot)
    {
        if (playerRoot == null)
        {
            return;
        }

        EnsureComponent<CharacterController>(playerRoot, () =>
        {
            var controller = playerRoot.AddComponent<CharacterController>();
            PlayerAnchorUtility.EnsureStandardController(controller);
        });

        EnsureComponent<PlayerStats>(playerRoot);
        EnsureComponent<GameScore>(playerRoot);
        EnsureComponent<PlayerBuffController>(playerRoot);
        EnsureComponent<PlayerShieldVisual>(playerRoot);
        EnsureComponent<PlayerInventory>(playerRoot);
        EnsureComponent<PlayerPickupInteractor>(playerRoot);
        EnsureComponent<PlayerCraftingInteractor>(playerRoot);
        EnsureComponent<PlayerPlacementController>(playerRoot);
        EnsureComponent<PlayerGameplayTargeting>(playerRoot);
        EnsureComponent<PlayerPickupTargeting>(playerRoot);
        EnsureComponent<PlayerToolSockets>(playerRoot);
        EnsureComponent<PlayerHeldItemController>(playerRoot);
        EnsureComponent<PlayerToolController>(playerRoot);
        EnsureComponent<PlayerWeaponController>(playerRoot);
        EnsureComponent<EnemyCombatTargetingController>(playerRoot);
        EnsureComponent<WeaponTargetHighlightController>(playerRoot);
        EnsureComponent<PlayerItemDropController>(playerRoot);
        EnsureComponent<PlayerController>(playerRoot);
        EnsureComponent<PlayerVisualSwitcher>(playerRoot);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        EnsureComponent<CraftingDebugRuntime>(playerRoot);
#endif

        PlayerVisualBuilder.EnsurePlayerVisual(playerRoot);
        GameplayLayers.TrySetPlayerLayer(playerRoot);
    }

    public static PlayerSetupDiagnostics.Report GenerateDiagnostics(GameObject playerRoot)
    {
        return PlayerSetupDiagnostics.Analyze(playerRoot);
    }

    static void EnsureComponent<T>(GameObject target) where T : Component
    {
        EnsureComponent<T>(target, null);
    }

    static void EnsureComponent<T>(GameObject target, System.Action onCreate) where T : Component
    {
        if (target.GetComponent<T>() != null)
        {
            return;
        }

        if (onCreate != null)
        {
            onCreate.Invoke();
        }
        else
        {
            target.AddComponent<T>();
        }
    }
}
