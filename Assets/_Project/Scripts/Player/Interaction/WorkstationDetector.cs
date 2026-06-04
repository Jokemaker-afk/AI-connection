using System.Collections.Generic;
using UnityEngine;

public static class WorkstationDetector
{
    public const float DefaultRange = 4f;

    public static List<WorkstationKind> GetCraftingContexts(Vector3 playerPosition, float range = DefaultRange)
    {
        var contexts = new List<WorkstationKind> { WorkstationKind.None };
        var seen = new HashSet<WorkstationKind> { WorkstationKind.None };

        CraftingStation[] stations = Object.FindObjectsByType<CraftingStation>();
        for (int i = 0; i < stations.Length; i++)
        {
            CraftingStation station = stations[i];
            if (station == null || !station.gameObject.activeInHierarchy)
            {
                continue;
            }

            WorkstationKind kind = station.WorkstationKind;
            if (kind == WorkstationKind.None || kind == WorkstationKind.Player)
            {
                continue;
            }

            if (!station.IsPlayerInRange(playerPosition))
            {
                continue;
            }

            if (seen.Add(kind))
            {
                contexts.Add(kind);
            }
        }

        return contexts;
    }

    public static string GetColumnTitle(WorkstationKind kind)
    {
        switch (kind)
        {
            case WorkstationKind.None:
                return "随身可建造";
            case WorkstationKind.Workbench:
                return "工作台";
            case WorkstationKind.Furnace:
                return "熔炉";
            case WorkstationKind.Forge:
                return "锻造台";
            case WorkstationKind.Loom:
                return "织布台";
            case WorkstationKind.AlchemyTable:
                return "炼金台";
            case WorkstationKind.ScienceLab:
                return "科技研究台";
            default:
                return "制造";
        }
    }
}
