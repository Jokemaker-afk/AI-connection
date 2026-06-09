using UnityEngine;

public static class Level8ProgressionBootstrap
{
    public static void EnsureLevel8Progression()
    {
        GameplayProgressionBootstrap.EnsureProgressionSystems();
        EnsureObjectiveTracker();
        EnsureObjectiveHud();
    }

    public static Level8ProgressionManager EnsureProgressionManager(
        SignalRelayInteractable relay,
        Level8ExitPortal exitPortal)
    {
        var systemsGo = GameObject.Find("Level8Systems");
        if (systemsGo == null)
        {
            var arena = GameObject.Find("Level8Arena");
            systemsGo = new GameObject("Level8Systems");
            if (arena != null)
            {
                systemsGo.transform.SetParent(arena.transform, false);
            }
        }

        var manager = systemsGo.GetComponent<Level8ProgressionManager>();
        if (manager == null)
        {
            manager = systemsGo.AddComponent<Level8ProgressionManager>();
        }

        manager.BindSignalRelay(relay);
        manager.BindExitPortal(exitPortal);
        return manager;
    }

    static void EnsureObjectiveTracker()
    {
        var root = GameObject.Find("GameplayProgression");
        if (root == null)
        {
            return;
        }

        if (root.GetComponent<Level8ObjectiveTracker>() != null)
        {
            return;
        }

        root.AddComponent<Level8ObjectiveTracker>();
    }

    static void EnsureObjectiveHud()
    {
        var hudGo = RuntimeSingletonCleanup.ResolveSingleGameplayHud();
        if (hudGo == null)
        {
            return;
        }

        if (hudGo.GetComponent<Level8ProgressionHud>() == null)
        {
            hudGo.AddComponent<Level8ProgressionHud>();
        }

        LevelObjectiveUiRegistry.NotifyProviderBound("Level8", nameof(Level8ObjectiveTracker));
    }
}
