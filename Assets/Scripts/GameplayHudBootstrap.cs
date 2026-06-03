using UnityEngine;

public static class GameplayHudBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void BootstrapSceneHud()
    {
        if (GameObject.Find("GameplayHUD") == null)
        {
            return;
        }

        EnsureGameplayHud();
    }

    public static void EnsureGameplayHud()
    {
        GameplayProgressionBootstrap.EnsureProgressionSystems();
        EnsureEventSystem();

        var hudGo = GameObject.Find("GameplayHUD");
        if (hudGo == null)
        {
            hudGo = new GameObject("GameplayHUD", typeof(RectTransform));
        }

        if (hudGo.GetComponent<RectTransform>() == null)
        {
            hudGo.AddComponent<RectTransform>();
        }

        var hud = hudGo.GetComponent<PlayerHUD>();
        if (hud == null)
        {
            hud = hudGo.AddComponent<PlayerHUD>();
        }

        var buffHud = hudGo.GetComponent<PlayerBuffHud>();
        if (buffHud == null)
        {
            buffHud = hudGo.AddComponent<PlayerBuffHud>();
        }

        var inventoryHud = hudGo.GetComponent<PlayerInventoryHud>();
        if (inventoryHud == null)
        {
            inventoryHud = hudGo.AddComponent<PlayerInventoryHud>();
        }

        var craftingHud = hudGo.GetComponent<CraftingHud>();
        if (craftingHud == null)
        {
            craftingHud = hudGo.AddComponent<CraftingHud>();
        }

        var crosshairHud = hudGo.GetComponent<GameplayCrosshairHud>();
        if (crosshairHud == null)
        {
            crosshairHud = hudGo.AddComponent<GameplayCrosshairHud>();
        }

        hud.RebuildUi();
        crosshairHud.RebuildUi();

        BindAll(hudGo);

        GameplayUiUtility.EnsureRootScale(hudGo.GetComponent<RectTransform>());
    }

    public static void BindAll(GameObject hudGo = null)
    {
        ItemWorldLabel.UpgradeLegacyLabels();

        if (hudGo == null)
        {
            hudGo = GameObject.Find("GameplayHUD");
        }

        if (hudGo == null)
        {
            return;
        }

        var player = GameObject.Find("Player");
        if (player == null)
        {
            player = Object.FindFirstObjectByType<PlayerController>()?.gameObject;
        }

        if (player == null)
        {
            return;
        }

        EnsurePlayerComponents(player);

        var hud = hudGo.GetComponent<PlayerHUD>();
        var buffHud = hudGo.GetComponent<PlayerBuffHud>();
        var inventoryHud = hudGo.GetComponent<PlayerInventoryHud>();
        var craftingHud = hudGo.GetComponent<CraftingHud>();

        var stats = player.GetComponent<PlayerStats>();
        var score = player.GetComponent<GameScore>();
        var buffController = player.GetComponent<PlayerBuffController>();
        var inventory = player.GetComponent<PlayerInventory>();
        var pickupInteractor = player.GetComponent<PlayerPickupInteractor>();
        var craftingInteractor = player.GetComponent<PlayerCraftingInteractor>();

        if (hud != null && stats != null)
        {
            hud.BindTo(stats, score);
        }

        if (buffHud != null && buffController != null)
        {
            buffHud.BindTo(buffController);
        }

        if (inventoryHud != null && inventory != null)
        {
            inventoryHud.BindTo(inventory, pickupInteractor);
        }

        if (craftingHud != null && craftingInteractor != null)
        {
            craftingInteractor.BindCraftingHud(craftingHud);
        }
    }

    static void EnsurePlayerComponents(GameObject player)
    {
        if (player.GetComponent<PlayerStats>() == null)
        {
            player.AddComponent<PlayerStats>();
        }

        if (player.GetComponent<GameScore>() == null)
        {
            player.AddComponent<GameScore>();
        }

        if (player.GetComponent<PlayerBuffController>() == null)
        {
            player.AddComponent<PlayerBuffController>();
        }

        if (player.GetComponent<PlayerShieldVisual>() == null)
        {
            player.AddComponent<PlayerShieldVisual>();
        }

        if (player.GetComponent<PlayerInventory>() == null)
        {
            player.AddComponent<PlayerInventory>();
        }

        if (player.GetComponent<PlayerPickupInteractor>() == null)
        {
            player.AddComponent<PlayerPickupInteractor>();
        }

        if (player.GetComponent<PlayerCraftingInteractor>() == null)
        {
            player.AddComponent<PlayerCraftingInteractor>();
        }

        if (player.GetComponent<PlayerPlacementController>() == null)
        {
            player.AddComponent<PlayerPlacementController>();
        }

        if (player.GetComponent<PlayerGameplayTargeting>() == null)
        {
            player.AddComponent<PlayerGameplayTargeting>();
        }

        GameplayLayers.TrySetPlayerLayer(player);
    }

    static void EnsureEventSystem()
    {
        var eventSystem = Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem == null)
        {
            var eventSystemGo = new GameObject(
                "EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
            if (Application.isPlaying)
            {
                Object.DontDestroyOnLoad(eventSystemGo);
            }

            return;
        }

        if (eventSystem.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>() == null
            && eventSystem.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }
    }
}
