/// <summary>Reusable world scene module categories for prefab catalog lookup.</summary>
public enum SceneModuleKind
{
    ReturnButton,
    InteractionButton,
    BubbleShell,
    BuffStation,
    HealStation,
    StaminaStation,
    SpeedStation,
    DamageStation,
    DefenseStation,
    Portal,
    TutorialSign,
    Hazard,
    ElectricField,
    /// <summary>Full electric field hazard module (LogicRoot + VisualRoot + WarningArea).</summary>
    ElectricFieldHazard,
    /// <summary>Spike trap visual alias; resolves to Hazard registry entry when not registered separately.</summary>
    SpikeHazard,
    /// <summary>Full spike trap hazard module (LogicRoot + VisualRoot + WarningArea).</summary>
    SpikeTrapHazard,
}
