using UnityEngine;

public static class GameplayProgressionBootstrap
{
    public static void EnsureProgressionSystems()
    {
        var root = GameObject.Find("GameplayProgression");
        if (root == null)
        {
            root = new GameObject("GameplayProgression");
        }

        if (root.GetComponent<TechnologyManager>() == null)
        {
            root.AddComponent<TechnologyManager>();
        }

        if (root.GetComponent<BuildingRegistry>() == null)
        {
            root.AddComponent<BuildingRegistry>();
        }

        if (root.GetComponent<RequiredPlacedObjectTracker>() == null)
        {
            root.AddComponent<RequiredPlacedObjectTracker>();
        }
    }
}
