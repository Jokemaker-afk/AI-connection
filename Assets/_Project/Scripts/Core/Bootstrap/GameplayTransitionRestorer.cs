using UnityEngine;

/// <summary>Shared post-scene-load reset so gameplay input, camera, and singletons work after every level transition.</summary>
public static class GameplayTransitionRestorer
{
    public static void RestoreGameplayAfterSceneLoad(string sceneName)
    {
        if (!GameplaySceneCatalog.IsSupportedGameplayScene(sceneName))
        {
            return;
        }

        Time.timeScale = 1f;
        LevelTransitionOverlay.Hide();
        RuntimeSingletonCleanup.CleanupDuplicateRuntimeObjects(sceneName);
        GameplayCursorPolicy.ResetForGameplay();
        CloseBlockingGameplayUi();

        GameplayCore core = GameplayCore.EnsureExists();
        PersistentPlayerRig rig = PersistentPlayerRig.EnsureOnGameplayCore(core);
        EnablePlayerGameplayComponents();
        GameplayHudBootstrap.EnsureGameplayHud();
        GameplayCrosshairController.EnsureCrosshairForPlayer();
        rig.EnsureMainCamera();
        rig.RebindSceneSystems();

        LevelObjectiveUiRegistry.ApplyForScene(sceneName);

        SceneTransitionHealthCheck.LogReport(sceneName);
        CrosshairHealthCheck.LogReport(sceneName);
        core.Log($"RestoreGameplayAfterSceneLoad complete for {sceneName}.");
    }

    static void EnablePlayerGameplayComponents()
    {
        GameObject player = PersistentPlayerRig.Player;
        if (player == null)
        {
            player = GameObject.Find("Player");
        }

        if (player == null)
        {
            return;
        }

        var playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        var characterController = player.GetComponent<CharacterController>();
        if (characterController != null && !characterController.enabled)
        {
            characterController.enabled = true;
        }

        var placement = player.GetComponent<PlayerPlacementController>();
        placement?.CancelPlacementForSceneTransition();

        AimReferenceProvider.EnsureOnPlayer(player);
        GameplayCrosshairController.EnsureCrosshairForPlayer();
    }

    static void CloseBlockingGameplayUi()
    {
        GameObject hudGo = RuntimeSingletonCleanup.ResolveSingleGameplayHud();
        if (hudGo == null)
        {
            return;
        }

        var craftingHud = hudGo.GetComponent<CraftingHud>();
        if (craftingHud != null && craftingHud.IsOpen)
        {
            craftingHud.Close();
        }

        var inventoryHud = hudGo.GetComponent<PlayerInventoryHud>();
        inventoryHud?.ForceCloseForSceneTransition();
    }
}
