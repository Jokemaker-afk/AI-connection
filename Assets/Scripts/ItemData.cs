using System;
using UnityEngine;

[Serializable]
public struct ItemData
{
    public ItemKind Id;
    public string DisplayName;
    public ItemCategory Category;
    public bool Stackable;
    public int MaxStack;
    public Color DisplayColor;
    public bool IsPlaceable;
    public bool CanBePlacedOnTop;
    public bool CanSupportPlacedObjects;
    public bool AllowStacking;
    public BuildingKind PlacedBuildingKind;
    public WorkstationKind PlacedWorkstationKind;
    public Vector3 PlacedBoundsSize;
    public GameObject WorldPickupPrefab;
    public GameObject PlacedPrefab;
    public Sprite Icon;

    public bool IsValid => ItemKindUtility.IsValid(Id);
}
