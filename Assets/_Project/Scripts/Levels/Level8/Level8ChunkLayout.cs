using System.Collections.Generic;
using UnityEngine;

/// <summary>One chunk slot in a Level8 grid layout.</summary>
public class Level8ChunkSlot
{
    public Vector2Int GridPosition;
    public Vector3 WorldCenter;
    public Level8ChunkKind ChunkKind;
    public int DifficultyWeight;
    public bool IsRequired;
    public bool HasDataCoreMarker;
    public int DataCoreIndex = -1;
    public readonly List<Vector2Int> ConnectedNeighbors = new List<Vector2Int>();

    public int GridX => GridPosition.x;
    public int GridZ => GridPosition.y;
}

/// <summary>Full Level8 chunk layout produced by the layout generator.</summary>
public class Level8ChunkLayout
{
    public int Seed;
    public int TargetChunkCount;
    public string BiomeId = Level8GenerationFlags.DefaultBiomeId;
    public Level8BiomeKind SelectedBiomeKind = Level8BiomeKind.DataWilderness;
    public Level8BiomeProfile BiomeProfile;
    public Vector2Int SpawnGrid = new Vector2Int(0, 2);
    public Vector2Int FinalGrid;
    public readonly List<Level8ChunkSlot> Slots = new List<Level8ChunkSlot>();
    public readonly List<Vector2Int> MainPath = new List<Vector2Int>();

    public Level8ChunkSlot GetSlot(Vector2Int grid)
    {
        for (int i = 0; i < Slots.Count; i++)
        {
            if (Slots[i].GridPosition == grid)
            {
                return Slots[i];
            }
        }

        return null;
    }

    public Level8ChunkSlot SpawnSlot => GetSlot(SpawnGrid);
    public Level8ChunkSlot FinalSlot => GetSlot(FinalGrid);
}

/// <summary>Result of a layout generation attempt.</summary>
public class Level8LayoutGenerationResult
{
    public bool Success;
    public Level8ChunkLayout Layout;
    public string FailureReason;
    public bool LayoutConnected;
    public bool AllChunksInsideBounds;
    public int DataCoreCount;
    public int AttemptIndex;
}
