using System;
using UnityEngine;

[Serializable]
public struct LevelModuleDefinition
{
    public string ModuleId;
    public LevelModuleType ModuleType;
    public int MinLevelIndex;
    public string[] RequiredUnlockedModuleIds;
    public float Weight;
    public ItemKind PickupItemKind;
    public int PickupAmount;
    public ItemKind PlacedItemKind;
    public WorkstationKind WorkstationKind;
    public ToolKind ToolKind;
    public ItemKind ToolOutputItemKind;
    public int ToolOutputAmount;
    public string ToolTaskId;
    public string DisplayLabelChinese;
    public Vector3 PlaceholderScale;
    public Color PlaceholderColor;

    public bool IsValid => !string.IsNullOrEmpty(ModuleId);
}
