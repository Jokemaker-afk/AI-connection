using System.Collections.Generic;
using UnityEngine;

/// <summary>Builds a Level8ChunkLayout from the Phase 1 fixed placements.</summary>
public static class Level8FixedLayoutAdapter
{
    public static Level8ChunkLayout CreateLayout()
    {
        var layout = new Level8ChunkLayout
        {
            Seed = 0,
            TargetChunkCount = 6,
            FinalGrid = new Vector2Int(4, 2),
            SpawnGrid = new Vector2Int(0, 2),
        };

        layout.MainPath.Add(new Vector2Int(0, 2));
        layout.MainPath.Add(new Vector2Int(1, 2));
        layout.MainPath.Add(new Vector2Int(2, 2));
        layout.MainPath.Add(new Vector2Int(3, 2));
        layout.MainPath.Add(new Vector2Int(4, 2));

        AddSlot(layout, 0, 2, Level8ChunkKind.Spawn);
        AddSlot(layout, 1, 2, Level8ChunkKind.Path, hasDataCore: true, dataCoreIndex: 1);
        AddSlot(layout, 2, 2, Level8ChunkKind.DataCore, hasDataCore: true, dataCoreIndex: 2);
        AddSlot(layout, 3, 2, Level8ChunkKind.Resource, hasDataCore: true, dataCoreIndex: 3);
        AddSlot(layout, 3, 1, Level8ChunkKind.Hazard);
        AddSlot(layout, 4, 2, Level8ChunkKind.FinalObjective);

        Level8TerrainVisualBuilder.BuildNeighborConnections(layout);
        return layout;
    }

    static void AddSlot(
        Level8ChunkLayout layout,
        int gridX,
        int gridZ,
        Level8ChunkKind kind,
        bool hasDataCore = false,
        int dataCoreIndex = -1)
    {
        layout.Slots.Add(new Level8ChunkSlot
        {
            GridPosition = new Vector2Int(gridX, gridZ),
            WorldCenter = Level8MapUtility.GetChunkCenter(gridX, gridZ),
            ChunkKind = kind,
            IsRequired = kind == Level8ChunkKind.Spawn || kind == Level8ChunkKind.FinalObjective,
            HasDataCoreMarker = hasDataCore,
            DataCoreIndex = dataCoreIndex,
        });
    }
}
