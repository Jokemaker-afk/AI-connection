[System.Flags]
public enum GameplayAbility
{
    None = 0,
    Hud = 1 << 0,
    HealthStamina = 1 << 1,
    CameraSwitch = 1 << 2,
    InventoryBackpack = 1 << 3,
    ItemPickup = 1 << 4,
    ItemTooltip = 1 << 5,
    Crafting = 1 << 6,
    BuildingPlacement = 1 << 7,
    CrosshairTargeting = 1 << 8,
    WorkstationInteraction = 1 << 9,
    HandheldTools = 1 << 10,
    SceneProgression = 1 << 11,
}
