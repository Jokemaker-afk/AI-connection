using System;
using UnityEngine;

[Serializable]
public struct ItemData
{
    public ItemKind Id;
    public string DisplayName;
    public string DescriptionChinese;
    public ItemCategory Category;
    public bool Stackable;
    public int MaxStack;
    public Color DisplayColor;
    public bool IsPlaceable;
    public bool Recoverable;
    public bool Craftable;
    public WorkstationKind RequiredWorkstation;
    public bool CanBePlacedOnTop;
    public bool CanSupportPlacedObjects;
    public bool AllowStacking;
    public BuildingKind PlacedBuildingKind;
    public WorkstationKind PlacedWorkstationKind;
    public Vector3 PlacedBoundsSize;
    public GameObject WorldPickupPrefab;
    public GameObject PlacedPrefab;
    public Sprite Icon;
    public HandheldToolProfile HandheldTool;
    public GameObject FutureModelPrefab;
    public Texture FutureTextureReference;

    public bool IsValid => ItemKindUtility.IsValid(Id);
    public bool IsHandheldTool => HandheldTool.IsTool;
    public string DisplayNameChinese => DisplayName;
}
