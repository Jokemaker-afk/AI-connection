using UnityEngine;
using UnityEngine.EventSystems;

public static class GameplayHudBootstrap
{
    public static void EnsureGameplayHud()
    {
        GameplayProgressionBootstrap.EnsureProgressionSystems();
        EnsureEventSystem();
        RuntimeSingletonCleanup.DedupeEventSystems();

        GameObject hudGo = RuntimeSingletonCleanup.ResolveSingleGameplayHud();
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
        crosshairHud.EnsureCrosshairExists();
        GameplayCrosshairController.EnsureCrosshairForPlayer();

        BindAll(hudGo);

        if (Application.isPlaying)
        {
            Object.DontDestroyOnLoad(hudGo);
        }

        RuntimeSingletonCleanup.DedupeEventSystems();
        GameplayUiUtility.EnsureRootScale(hudGo.GetComponent<RectTransform>());
    }

    public static void BindAll(GameObject hudGo = null)
    {
        ItemWorldLabel.UpgradeLegacyLabels();

        if (hudGo == null)
        {
            hudGo = RuntimeSingletonCleanup.ResolveSingleGameplayHud();
        }

        if (hudGo == null)
        {
            return;
        }

        var player = PersistentPlayerRig.Player;
        if (player == null)
        {
            player = GameObject.Find("Player");
        }

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
        var craftingAbility = player.GetComponent<PlayerCraftingAbility>();
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

        if (craftingHud != null)
        {
            if (craftingAbility != null)
            {
                craftingAbility.BindCraftingHud(craftingHud);
            }
            else if (craftingInteractor != null)
            {
                craftingInteractor.BindCraftingHud(craftingHud);
            }
        }

        var toolController = player.GetComponent<PlayerToolController>();
        toolController?.RefreshEquippedTool();
    }

    public static void EnsurePlayerComponentsPublic(GameObject player)
    {
        PlayerRootComponents.EnsureAll(player);
    }

    static void EnsurePlayerComponents(GameObject player)
    {
        PlayerRootComponents.EnsureAll(player);
    }

    static void EnsureEventSystem()
    {
        RuntimeSingletonCleanup.DedupeEventSystems();

        var eventSystem = Object.FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            var eventSystemGo = new GameObject(
                "EventSystem",
                typeof(EventSystem),
                typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
            if (Application.isPlaying)
            {
                Object.DontDestroyOnLoad(eventSystemGo);
            }

            return;
        }

        if (eventSystem.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>() == null
            && eventSystem.GetComponent<StandaloneInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        if (Application.isPlaying && eventSystem.gameObject.scene.name != "DontDestroyOnLoad")
        {
            Object.DontDestroyOnLoad(eventSystem.gameObject);
        }
    }
}
