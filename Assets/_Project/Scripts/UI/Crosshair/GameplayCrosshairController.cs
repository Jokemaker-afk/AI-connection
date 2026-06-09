using UnityEngine;

/// <summary>
/// Central API for gameplay crosshair visibility. Delegates to the single <see cref="GameplayCrosshairHud"/> on GameplayHUD.
/// </summary>
public static class GameplayCrosshairController
{
    public static GameplayCrosshairHud ResolveHud()
    {
        GameObject hudGo = RuntimeSingletonCleanup.ResolveSingleGameplayHud();
        if (hudGo == null)
        {
            return Object.FindFirstObjectByType<GameplayCrosshairHud>();
        }

        return hudGo.GetComponent<GameplayCrosshairHud>();
    }

    public static bool IsPlayerGameplayActive()
    {
        GameObject player = ResolvePlayerRoot();
        if (player == null || !player.activeInHierarchy)
        {
            return false;
        }

        if (Time.timeScale <= 0f)
        {
            return false;
        }

        var controller = player.GetComponent<PlayerController>();
        return controller == null || controller.enabled;
    }

    public static void EnsureCrosshairExists()
    {
        GameplayCrosshairHud hud = ResolveHud();
        if (hud == null)
        {
            GameplayHudBootstrap.EnsureGameplayHud();
            hud = ResolveHud();
        }

        hud?.EnsureCrosshairExists();
    }

    public static void EnsureCrosshairForPlayer()
    {
        if (!IsPlayerGameplayActive())
        {
            return;
        }

        EnsureCrosshairExists();
        RefreshCrosshairVisibility();
        BindAimReferenceCrosshair();
    }

    public static void ShowCrosshair()
    {
        ResolveHud()?.ShowCrosshair();
    }

    public static void HideCrosshair()
    {
        ResolveHud()?.HideCrosshair();
    }

    public static void SetCrosshairVisible(bool visible)
    {
        ResolveHud()?.SetCrosshairVisible(visible);
    }

    public static void RefreshForCurrentUIState()
    {
        RefreshCrosshairVisibility();
    }

    public static void RefreshCrosshairVisibility()
    {
        ResolveHud()?.RefreshCrosshairVisibility();
    }

    public static void BindAimReferenceCrosshair()
    {
        GameObject player = ResolvePlayerRoot();
        if (player == null)
        {
            return;
        }

        AimReferenceProvider aimReference = AimReferenceProvider.EnsureOnPlayer(player);
        RectTransform crosshairRect = ResolveHud()?.CrosshairRectTransform;
        aimReference?.BindCrosshairRect(crosshairRect);
    }

    public static void LogHealthCheck(string sceneName)
    {
        CrosshairHealthCheck.LogReport(sceneName);
    }

    static GameObject ResolvePlayerRoot()
    {
        GameObject player = PersistentPlayerRig.Player;
        if (player == null)
        {
            player = GameObject.Find("Player");
        }

        if (player == null)
        {
            player = Object.FindFirstObjectByType<PlayerController>()?.gameObject;
        }

        return player;
    }
}
