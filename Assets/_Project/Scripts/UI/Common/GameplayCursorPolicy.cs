using UnityEngine;

public static class GameplayCursorPolicy
{
    static bool inventoryUiOpen;
    static bool craftingUiOpen;

    public static bool IsInventoryUiOpen => inventoryUiOpen;
    public static bool IsCraftingUiOpen => craftingUiOpen;
    public static bool IsAnyMenuOpen => inventoryUiOpen || craftingUiOpen;

    public static void SetInventoryUiOpen(bool open)
    {
        inventoryUiOpen = open;
        Apply();
    }

    public static void SetCraftingUiOpen(bool open)
    {
        craftingUiOpen = open;
        Apply();
    }

    public static void ResetForGameplay()
    {
        inventoryUiOpen = false;
        craftingUiOpen = false;
        Apply();
    }

    public static void Apply()
    {
        if (IsAnyMenuOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            GameplayCrosshairController.RefreshForCurrentUIState();
            return;
        }

        var cameraController = Object.FindFirstObjectByType<PlayerCameraController>();
        if (cameraController != null)
        {
            cameraController.ApplyGameplayCursorLock();
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        GameplayCrosshairController.RefreshCrosshairVisibility();
    }
}
