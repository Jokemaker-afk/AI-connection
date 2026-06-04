using UnityEngine;

public enum GameplayTargetType
{
    None = 0,
    DroppedPickup,
    PlacedRecoverable,
    ToolInteractable,
    Workstation,
    StaticScene,
}

public enum GameplayTargetAction
{
    PickupRecoverF,
    InteractE,
    PlacementLeftClick,
    ToolUseLeftClick,
}

public struct GameplayTargetCandidate
{
    public GameObject TargetObject;
    public GameplayTargetType TargetType;
    public ItemKind ItemKind;
    public float Score;
    public int ActionPriority;

    public bool IsValid => TargetObject != null && TargetType != GameplayTargetType.None;
}
