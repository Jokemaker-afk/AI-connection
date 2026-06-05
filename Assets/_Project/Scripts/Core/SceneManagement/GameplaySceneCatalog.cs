using UnityEngine;

public static class GameplaySceneCatalog
{
    public static bool IsSupportedGameplayScene(string sceneName)
    {
        return GetLevelNumber(sceneName) > 0;
    }

    public static int GetLevelNumber(string sceneName)
    {
        switch (sceneName)
        {
            case "SampleScene":
                return 1;
            case "Level2":
                return 2;
            case "Level3":
                return 3;
            case "Level4":
                return 4;
            case "Level5":
                return 5;
            case "Level6":
                return 6;
            case "Level7":
                return 7;
            case "Level8":
                return 8;
            default:
                return 0;
        }
    }

    public static Vector3 GetDefaultSpawn(string sceneName)
    {
        switch (sceneName)
        {
            case "SampleScene":
                return new Vector3(0f, 0.25f, -3f);
            case "Level2":
                return new Vector3(0f, 0.25f, -3f);
            case "Level3":
                return new Vector3(0f, 1.25f, 0f);
            case "Level4":
                return new Vector3(0f, 0.25f, 0f);
            case "Level5":
                return new Vector3(0f, 0f, -2f);
            case "Level6":
                return new Vector3(0f, 0f, -6f);
            case "Level7":
                return new Vector3(0f, 0f, -2f);
            case "Level8":
                return new Vector3(0f, 0f, -2f);
            default:
                return Vector3.zero;
        }
    }

    public static GameplayAbility GetInheritedAbilities(string sceneName)
    {
        return GetInheritedAbilitiesForLevel(GetLevelNumber(sceneName));
    }

    public static GameplayAbility GetInheritedAbilitiesForLevel(int level)
    {
        if (level <= 0)
        {
            return GameplayAbility.None;
        }

        GameplayAbility abilities = GameplayAbility.Hud
            | GameplayAbility.HealthStamina
            | GameplayAbility.SceneProgression;

        if (level >= 2)
        {
            abilities |= GameplayAbility.CameraSwitch;
        }

        if (level >= 4)
        {
            abilities |= GameplayAbility.InventoryBackpack
                | GameplayAbility.ItemPickup
                | GameplayAbility.ItemTooltip
                | GameplayAbility.CrosshairTargeting;
        }

        if (level >= 5)
        {
            abilities |= GameplayAbility.Crafting
                | GameplayAbility.BuildingPlacement
                | GameplayAbility.WorkstationInteraction;
        }

        if (level >= 6)
        {
            abilities |= GameplayAbility.HandheldTools;
        }

        if (level >= 7)
        {
            abilities |= GameplayAbility.WeaponCombat;
        }

        return abilities;
    }
}
