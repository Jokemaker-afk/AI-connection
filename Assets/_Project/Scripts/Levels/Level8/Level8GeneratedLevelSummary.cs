using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>Records generated Level8 layout metadata for debugging and future persistence.</summary>
public class Level8GeneratedLevelSummary
{
    public int Seed;
    public Level8BiomeKind Biome;
    public int ChunkCount;
    public bool ValidationPassed;
    public readonly List<Vector2Int> ChunkGrids = new List<Vector2Int>();
    public readonly List<Vector3> DataCorePositions = new List<Vector3>();
    public readonly List<int> DataCoreIndices = new List<int>();
    public readonly List<Vector2Int> DataCoreChunkGrids = new List<Vector2Int>();
    public Vector3 SignalRelayPosition;
    public Vector3 ExitPosition;
    public readonly List<Vector3> ResourceAreaCenters = new List<Vector3>();
    public readonly List<Vector3> HazardAreaCenters = new List<Vector3>();

    public static Level8GeneratedLevelSummary FromLayout(Level8ChunkLayout layout)
    {
        var summary = new Level8GeneratedLevelSummary
        {
            Seed = layout.Seed,
            Biome = layout.SelectedBiomeKind,
            ChunkCount = layout.Slots.Count,
            ValidationPassed = true,
        };

        for (int i = 0; i < layout.Slots.Count; i++)
        {
            Level8ChunkSlot slot = layout.Slots[i];
            summary.ChunkGrids.Add(slot.GridPosition);

            if (slot.ChunkKind == Level8ChunkKind.Resource)
            {
                summary.ResourceAreaCenters.Add(slot.WorldCenter);
            }

            if (slot.ChunkKind == Level8ChunkKind.Hazard)
            {
                summary.HazardAreaCenters.Add(slot.WorldCenter);
            }
        }

        if (layout.FinalSlot != null)
        {
            summary.SignalRelayPosition = Level8ObjectivePositions.GetSignalRelayPosition(layout.FinalSlot.WorldCenter);
            summary.ExitPosition = Level8ObjectivePositions.GetExitPosition(layout.FinalSlot.WorldCenter);
        }

        return summary;
    }

    public void RegisterDataCore(DataCoreCollectible collectible, Vector2Int chunkGrid)
    {
        if (collectible == null)
        {
            return;
        }

        DataCorePositions.Add(collectible.transform.position);
        DataCoreIndices.Add(collectible.DataCoreIndex);
        DataCoreChunkGrids.Add(chunkGrid);
    }

    public void RegisterSignalRelay(SignalRelayInteractable relay)
    {
        if (relay != null)
        {
            SignalRelayPosition = relay.transform.position;
        }
    }

    public void RegisterExitPortal(Level8ExitPortal portal)
    {
        if (portal != null)
        {
            ExitPosition = portal.transform.position;
        }
    }

    public void ValidateObjectives(IReadOnlyList<DataCoreCollectible> dataCores, SignalRelayInteractable relay, Level8ExitPortal exit)
    {
        int coreCount = dataCores != null ? dataCores.Count(c => c != null && c.CanBeCollected) : 0;
        ValidationPassed = coreCount == 3
            && dataCores.All(c => c != null && c.GetComponent<DataCoreCollectible>() != null)
            && relay != null
            && exit != null;

        if (coreCount != 3)
        {
            Debug.LogWarning($"[Level8] Validation: expected 3 active Data Cores, found {coreCount}.");
            ValidationPassed = false;
        }
    }

    public void LogSummary()
    {
        string relayPos = SignalRelayPosition.ToString("F1");
        string exitPos = ExitPosition.ToString("F1");
        string corePos = string.Join("; ", DataCorePositions.Select(p => p.ToString("F1")));

        Debug.Log(
            $"[Level8] Summary: seed={Seed}, biome={Biome}, chunks={ChunkCount}, " +
            $"dataCores={DataCorePositions.Count}, relay={relayPos}, exit={exitPos}, valid={ValidationPassed}");
        Debug.Log($"[Level8] Data Core positions: {corePos}");
    }
}

/// <summary>Shared world positions for Level8 objective props.</summary>
public static class Level8ObjectivePositions
{
    public const float FloorTop = Level8ChunkPlaceholderBuilder.FloorTop;

    public static Vector3 GetDataCoreWorldPosition(Level8ChunkSlot slot)
    {
        Vector3 offset = ResolveDataCoreOffset(slot.DataCoreIndex, slot.ChunkKind);
        return slot.WorldCenter + offset;
    }

    public static Vector3 GetSignalRelayPosition(Vector3 finalChunkCenter)
    {
        return finalChunkCenter + new Vector3(6f, FloorTop + 0.9f, -4f);
    }

    public static Vector3 GetExitPosition(Vector3 finalChunkCenter)
    {
        return finalChunkCenter + new Vector3(0f, FloorTop + 0.5f, 12f);
    }

    static Vector3 ResolveDataCoreOffset(int dataCoreIndex, Level8ChunkKind hostKind)
    {
        switch (hostKind)
        {
            case Level8ChunkKind.Resource:
                return dataCoreIndex switch
                {
                    1 => new Vector3(8f, FloorTop + 0.75f, -6f),
                    2 => new Vector3(-8f, FloorTop + 0.75f, 6f),
                    _ => new Vector3(0f, FloorTop + 0.75f, 0f),
                };
            case Level8ChunkKind.Path:
                return dataCoreIndex switch
                {
                    1 => new Vector3(-10f, FloorTop + 0.75f, 0f),
                    2 => new Vector3(10f, FloorTop + 0.75f, 4f),
                    _ => new Vector3(0f, FloorTop + 0.75f, -8f),
                };
            case Level8ChunkKind.Hazard:
                return new Vector3(12f, FloorTop + 0.75f, 12f);
            case Level8ChunkKind.DataCore:
                return new Vector3(0f, FloorTop + 0.75f, 0f);
            default:
                return new Vector3(0f, FloorTop + 0.75f, 0f);
        }
    }
}
