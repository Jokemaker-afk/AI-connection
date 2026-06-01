using UnityEngine;
using UnityEngine.SceneManagement;

static class Level4SceneAutoSetup
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (SceneManager.GetActiveScene().name != "Level4")
        {
            return;
        }

        EnsurePlayerInventory();
        EnsureInventoryHud();
    }

    static void EnsurePlayerInventory()
    {
        var player = Object.FindFirstObjectByType<PlayerController>();
        if (player == null)
        {
            return;
        }

        if (player.GetComponent<PlayerInventory>() == null)
        {
            player.gameObject.AddComponent<PlayerInventory>();
        }

        if (player.GetComponent<PlayerPickupInteractor>() == null)
        {
            player.gameObject.AddComponent<PlayerPickupInteractor>();
        }
    }

    static void EnsureInventoryHud()
    {
        var hudGo = GameObject.Find("GameplayHUD");
        if (hudGo == null)
        {
            hudGo = new GameObject("GameplayHUD", typeof(RectTransform));
        }

        var playerHud = hudGo.GetComponent<PlayerHUD>();
        if (playerHud == null)
        {
            playerHud = hudGo.AddComponent<PlayerHUD>();
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

        playerHud.RebuildUi();
        buffHud.RebuildUi();
        inventoryHud.RebuildUi();

        var player = Object.FindFirstObjectByType<PlayerController>();
        var stats = player != null ? player.GetComponent<PlayerStats>() : null;
        var score = player != null ? player.GetComponent<GameScore>() : null;
        var buffController = player != null ? player.GetComponent<PlayerBuffController>() : null;
        var inventory = player != null ? player.GetComponent<PlayerInventory>() : null;
        var interactor = player != null ? player.GetComponent<PlayerPickupInteractor>() : null;

        if (stats != null)
        {
            playerHud.BindTo(stats, score);
        }

        if (buffController != null)
        {
            buffHud.BindTo(buffController);
        }

        if (inventory != null)
        {
            inventoryHud.BindTo(inventory, interactor);
        }
    }
}
