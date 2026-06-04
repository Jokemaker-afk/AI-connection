using UnityEngine;

public static class GameplayTargetingPriority
{
    public static int GetActionPriority(GameplayTargetType type, GameplayTargetAction action)
    {
        switch (action)
        {
            case GameplayTargetAction.PickupRecoverF:
                return GetFPriority(type);
            case GameplayTargetAction.InteractE:
                return GetEPriority(type);
            default:
                return GetDefaultPriority(type);
        }
    }

    public static GameplayTargetType ClassifyPlacedPickup(PlacedPickupable placed)
    {
        if (placed == null || !placed.IsRecoverableTarget)
        {
            return GameplayTargetType.None;
        }

        return IsWorkstationPlaced(placed)
            ? GameplayTargetType.Workstation
            : GameplayTargetType.PlacedRecoverable;
    }

    public static bool IsWorkstationPlaced(PlacedPickupable placed)
    {
        if (placed == null)
        {
            return false;
        }

        if (placed.GetComponent<CraftingStation>() != null
            || placed.GetComponentInChildren<CraftingStation>() != null)
        {
            return true;
        }

        if (ItemKindUtility.GetPlacedWorkstationKind(placed.ItemKind) != WorkstationKind.None)
        {
            return true;
        }

        return ItemKindUtility.GetCategory(placed.ItemKind) == ItemCategory.Workstation;
    }

    static int GetFPriority(GameplayTargetType type)
    {
        switch (type)
        {
            case GameplayTargetType.DroppedPickup:
                return 1;
            case GameplayTargetType.PlacedRecoverable:
                return 2;
            case GameplayTargetType.ToolInteractable:
                return 3;
            case GameplayTargetType.Workstation:
                return 4;
            default:
                return 99;
        }
    }

    static int GetEPriority(GameplayTargetType type)
    {
        switch (type)
        {
            case GameplayTargetType.Workstation:
                return 1;
            case GameplayTargetType.ToolInteractable:
                return 2;
            case GameplayTargetType.PlacedRecoverable:
                return 3;
            case GameplayTargetType.DroppedPickup:
                return 99;
            default:
                return 99;
        }
    }

    static int GetDefaultPriority(GameplayTargetType type)
    {
        switch (type)
        {
            case GameplayTargetType.DroppedPickup:
                return 1;
            case GameplayTargetType.PlacedRecoverable:
                return 2;
            case GameplayTargetType.ToolInteractable:
                return 3;
            case GameplayTargetType.Workstation:
                return 4;
            default:
                return 99;
        }
    }

    public static void LogFPriorityDecision(
        WorldPickupItem worldPickup,
        PlacedPickupable placedPickup,
        PlacedPickupable selectedPlaced,
        bool logEnabled = true)
    {
        if (!logEnabled || !GameplayCore.Exists)
        {
            return;
        }

        string candidates = string.Empty;
        if (worldPickup != null)
        {
            candidates += $"DroppedItem {ItemKindUtility.GetDisplayName(worldPickup.ItemKind)}";
        }

        if (placedPickup != null)
        {
            if (!string.IsNullOrEmpty(candidates))
            {
                candidates += ", ";
            }

            candidates += ItemKindUtility.GetDisplayName(placedPickup.ItemKind);
        }

        if (string.IsNullOrEmpty(candidates))
        {
            return;
        }

        if (worldPickup != null && selectedPlaced == null)
        {
            GameplayCore.Instance.Log(
                $"[TargetPriority] F target candidates: {candidates} | Selected F target: DroppedItem " +
                $"{ItemKindUtility.GetDisplayName(worldPickup.ItemKind)} because priority 1");
            return;
        }

        if (selectedPlaced != null && worldPickup != null)
        {
            GameplayCore.Instance.Log(
                $"[TargetPriority] Workbench skipped for F because dropped item has higher priority");
        }
        else if (selectedPlaced != null)
        {
            GameplayCore.Instance.Log(
                $"[TargetPriority] Recovering placed object: {selectedPlaced.DisplayNameChinese}");
        }
    }
}
