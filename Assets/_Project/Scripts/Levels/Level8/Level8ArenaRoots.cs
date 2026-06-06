using UnityEngine;

/// <summary>Category parent transforms for Level8 arena hierarchy.</summary>
public class Level8ArenaRoots
{
    public Transform Arena;
    public Transform Terrain;
    public Transform Chunks;
    public Transform Objectives;
    public Transform DataCores;
    public Transform SignalRelay;
    public Transform Exit;
    public Transform Resources;
    public Transform Hazards;
    public Transform Decorations;
    public Transform Labels;
    public Transform Debug;
    public Transform Legend;

    public static Level8ArenaRoots Create(Transform arenaRoot)
    {
        var roots = new Level8ArenaRoots { Arena = arenaRoot };
        roots.Terrain = EnsureChild(arenaRoot, "Terrain");
        roots.Chunks = EnsureChild(arenaRoot, "Chunks");
        roots.Objectives = EnsureChild(arenaRoot, "Objectives");
        roots.DataCores = EnsureChild(roots.Objectives, "DataCores");
        roots.SignalRelay = EnsureChild(roots.Objectives, "SignalRelay");
        roots.Exit = EnsureChild(roots.Objectives, "Exit");
        roots.Resources = EnsureChild(arenaRoot, "Resources");
        roots.Hazards = EnsureChild(arenaRoot, "Hazards");
        roots.Decorations = EnsureChild(arenaRoot, "Decorations");
        roots.Labels = EnsureChild(arenaRoot, "Labels");
        roots.Debug = EnsureChild(arenaRoot, "Debug");
        roots.Legend = EnsureChild(arenaRoot, "Legend");
        return roots;
    }

    static Transform EnsureChild(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
        {
            return existing;
        }

        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }
}

/// <summary>Active arena roots during Level8 generation (read-only for builders).</summary>
public static class Level8ArenaBuildContext
{
    public static Level8ArenaRoots Roots { get; set; }
}
