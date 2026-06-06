using System;
using UnityEngine;

/// <summary>
/// Major objective types for prefab catalog lookup.
/// </summary>
public enum Level8ObjectiveKind
{
    DataCore,
    SignalRelay,
    LevelExitPortal,
    SavePoint,
    ResearchTerminal,
}

/// <summary>
/// Single weighted prop entry within a biome prop set.
/// </summary>
[Serializable]
public struct Level8PropSpawnRule
{
    public GameObject Prefab;
    public Level8PropCategory Category;
    public float Weight;
    public float MinScale;
    public float MaxScale;
    public int MinCount;
    public int MaxCount;
    public Level8ChunkKind[] AllowedChunkKinds;
    public bool AvoidNearObjective;
    public float MinDistanceBetweenSameType;
    public bool AlignToGround;
    public bool RandomYaw;
    public bool CanOverlapPath;
}

/// <summary>
/// Biome-scoped collection of decoration / environment prop spawn rules.
/// TODO: Migrate to ScriptableObject under ScriptableObjects/LevelGeneration/PropSets/
/// </summary>
[Serializable]
public class Level8BiomePropSet
{
    public Level8BiomeKind Biome;
    public string DisplayName;
    public Level8PropSpawnRule[] Rules = Array.Empty<Level8PropSpawnRule>();
}

/// <summary>
/// Optional metadata for individual environment props.
/// </summary>
[Serializable]
public class Level8EnvironmentPropProfile
{
    public string ProfileId;
    public GameObject Prefab;
    public Level8PropCategory Category;
    public Vector3 LocalBoundsSize;
    public bool BlocksMovement;
}
