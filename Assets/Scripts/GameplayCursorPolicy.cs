using UnityEngine;

public static class GameplayCursorPolicy
{
    static bool inventoryUiOpen;

    public static bool IsInventoryUiOpen => inventoryUiOpen;

    public static void SetInventoryUiOpen(bool open)
    {
        inventoryUiOpen = open;
        Apply();
    }

    public static void Apply()
    {
        if (inventoryUiOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
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
    }
}
