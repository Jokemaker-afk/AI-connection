using System;
using UnityEngine;

[Serializable]
public struct LevelModulePlacementData
{
    public string ModuleId;
    public Vector3 LocalPosition;
    public Vector3 LocalEulerAngles;
}

[Serializable]
public struct GeneratedLevelSummary
{
    public int LevelIndex;
    public string SceneName;
    public int LevelSeed;
    public string[] ModuleIds;
    public LevelModulePlacementData[] Placements;

    public bool IsValid => LevelIndex > 0 && ModuleIds != null && ModuleIds.Length > 0;
}
