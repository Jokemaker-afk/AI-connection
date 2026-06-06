using System;

using System.Collections.Generic;

using System.Linq;

using UnityEngine;



/// <summary>

/// Seed-driven Level8 chunk layout generator (Phase 2).

/// Produces a connected 6–10 chunk grid layout with required special chunks.

/// </summary>

public static class Level8LayoutGenerator

{

    static readonly Vector2Int SpawnGrid = new Vector2Int(0, 2);



    static readonly Vector2Int[] FinalCandidates =

    {

        new Vector2Int(4, 2),

        new Vector2Int(5, 2),

        new Vector2Int(4, 3),

        new Vector2Int(5, 3),

        new Vector2Int(4, 1),

        new Vector2Int(5, 1),

    };



    static readonly Vector2Int[] CardinalDirections =

    {

        new Vector2Int(1, 0),

        new Vector2Int(-1, 0),

        new Vector2Int(0, 1),

        new Vector2Int(0, -1),

    };



    public static Level8LayoutGenerationResult Generate(int seed)

    {

        for (int attempt = 0; attempt < Level8GenerationFlags.MaxGenerationAttempts; attempt++)

        {

            int attemptSeed = seed + attempt * 7919;

            var result = TryGenerate(attemptSeed, attempt);

            if (result.Success)

            {

                result.Layout.Seed = seed;

                LogGenerationResult(result);

                return result;

            }

        }



        return new Level8LayoutGenerationResult

        {

            Success = false,

            FailureReason = $"All {Level8GenerationFlags.MaxGenerationAttempts} generation attempts failed for seed {seed}.",

            AttemptIndex = Level8GenerationFlags.MaxGenerationAttempts,

        };

    }



    static Level8LayoutGenerationResult TryGenerate(int attemptSeed, int attemptIndex)

    {

        var rng = new System.Random(attemptSeed);

        var layout = new Level8ChunkLayout { Seed = attemptSeed };



        Vector2Int finalGrid = FinalCandidates[rng.Next(FinalCandidates.Length)];

        layout.FinalGrid = finalGrid;

        layout.SpawnGrid = SpawnGrid;



        int targetCount = rng.Next(

            Level8GenerationFlags.ChunkCountMin,

            Level8GenerationFlags.ChunkCountMax + 1);



        layout.TargetChunkCount = targetCount;



        List<Vector2Int> mainPath = BuildMainPath(SpawnGrid, finalGrid, rng);

        if (mainPath.Count == 0 || mainPath[mainPath.Count - 1] != finalGrid)

        {

            return Fail(attemptIndex, "Failed to build main path from spawn to final.");

        }



        layout.MainPath.AddRange(mainPath);



        var occupied = new HashSet<Vector2Int>(mainPath);

        if (!ExpandToTargetCount(occupied, targetCount, rng))

        {

            return Fail(attemptIndex, "Failed to expand layout to target chunk count.");

        }



        if (!AssignChunkKinds(layout, occupied, mainPath, rng))

        {

            return Fail(attemptIndex, "Failed to assign required chunk kinds.");

        }



        BuildNeighborConnections(layout);

        PopulateWorldCenters(layout);



        var result = new Level8LayoutGenerationResult

        {

            Success = true,

            Layout = layout,

            AttemptIndex = attemptIndex,

        };



        ValidateLayout(result);

        if (!result.Success)

        {

            result.FailureReason = result.FailureReason ?? "Validation failed.";

        }



        return result;

    }



    static List<Vector2Int> BuildMainPath(Vector2Int start, Vector2Int end, System.Random rng)

    {

        var path = new List<Vector2Int> { start };

        var visited = new HashSet<Vector2Int> { start };

        Vector2Int current = start;

        int safety = Level8GenerationFlags.GridSize * Level8GenerationFlags.GridSize * 2;



        while (current != end && safety-- > 0)

        {

            var candidates = new List<Vector2Int>();

            for (int i = 0; i < CardinalDirections.Length; i++)

            {

                Vector2Int next = current + CardinalDirections[i];

                if (!InBounds(next) || visited.Contains(next))

                {

                    continue;

                }



                candidates.Add(next);

            }



            if (candidates.Count == 0)

            {

                break;

            }



            candidates.Sort((a, b) => Manhattan(a, end).CompareTo(Manhattan(b, end)));



            int pickCount = Mathf.Min(candidates.Count, rng.Next(1, 3));

            Vector2Int chosen = candidates[rng.Next(pickCount)];

            path.Add(chosen);

            visited.Add(chosen);

            current = chosen;

        }



        if (current != end)

        {

            var fallback = BuildManhattanPath(start, end);

            return fallback;

        }



        return path;

    }



    static List<Vector2Int> BuildManhattanPath(Vector2Int start, Vector2Int end)

    {

        var path = new List<Vector2Int> { start };

        var current = start;

        var visited = new HashSet<Vector2Int> { start };



        while (current != end)

        {

            Vector2Int next = current;

            if (current.x != end.x)

            {

                next = new Vector2Int(current.x + Math.Sign(end.x - current.x), current.y);

            }

            else if (current.y != end.y)

            {

                next = new Vector2Int(current.x, current.y + Math.Sign(end.y - current.y));

            }



            if (!InBounds(next) || visited.Contains(next))

            {

                return new List<Vector2Int>();

            }



            path.Add(next);

            visited.Add(next);

            current = next;

        }



        return path;

    }



    static bool ExpandToTargetCount(HashSet<Vector2Int> occupied, int targetCount, System.Random rng)

    {

        int safety = Level8GenerationFlags.GridSize * Level8GenerationFlags.GridSize * 4;

        while (occupied.Count < targetCount && safety-- > 0)

        {

            var frontier = occupied.ToList();

            Vector2Int baseCell = frontier[rng.Next(frontier.Count)];



            var candidates = new List<Vector2Int>();

            for (int i = 0; i < CardinalDirections.Length; i++)

            {

                Vector2Int candidate = baseCell + CardinalDirections[i];

                if (InBounds(candidate) && !occupied.Contains(candidate))

                {

                    candidates.Add(candidate);

                }

            }



            if (candidates.Count == 0)

            {

                continue;

            }



            occupied.Add(candidates[rng.Next(candidates.Count)]);

        }



        return occupied.Count >= targetCount;

    }



    static bool AssignChunkKinds(

        Level8ChunkLayout layout,

        HashSet<Vector2Int> occupied,

        List<Vector2Int> mainPath,

        System.Random rng)

    {

        var assignable = occupied

            .Where(p => p != SpawnGrid && p != layout.FinalGrid)

            .OrderBy(_ => rng.Next())

            .ToList();



        if (assignable.Count < 4)

        {

            return false;

        }



        var mainPathSet = new HashSet<Vector2Int>(mainPath);

        var slotsByGrid = new Dictionary<Vector2Int, Level8ChunkSlot>();



        foreach (Vector2Int grid in occupied)

        {

            slotsByGrid[grid] = new Level8ChunkSlot

            {

                GridPosition = grid,

                ChunkKind = Level8ChunkKind.Path,

                DifficultyWeight = mainPathSet.Contains(grid) ? 1 : 2,

                IsRequired = grid == SpawnGrid || grid == layout.FinalGrid,

            };

        }



        slotsByGrid[SpawnGrid].ChunkKind = Level8ChunkKind.Spawn;

        slotsByGrid[layout.FinalGrid].ChunkKind = Level8ChunkKind.FinalObjective;



        Vector2Int resourceGrid = PickPreferred(assignable, mainPathSet, preferOffPath: true, rng);

        assignable.Remove(resourceGrid);

        slotsByGrid[resourceGrid].ChunkKind = Level8ChunkKind.Resource;

        slotsByGrid[resourceGrid].IsRequired = true;



        Vector2Int hazardGrid = PickPreferred(assignable, mainPathSet, preferOffPath: true, rng);

        assignable.Remove(hazardGrid);

        slotsByGrid[hazardGrid].ChunkKind = Level8ChunkKind.Hazard;

        slotsByGrid[hazardGrid].IsRequired = true;



        var dataCoreCandidates = assignable

            .OrderByDescending(p => mainPathSet.Contains(p) ? 1 : 0)

            .ThenBy(_ => rng.Next())

            .ToList();



        if (dataCoreCandidates.Count < 3)

        {

            return false;

        }



        var dataCoreGrids = new List<Vector2Int>();

        bool placedOnMainPath = false;



        for (int i = 0; i < dataCoreCandidates.Count && dataCoreGrids.Count < 3; i++)

        {

            Vector2Int candidate = dataCoreCandidates[i];

            if (dataCoreGrids.Contains(candidate))

            {

                continue;

            }



            bool onMainPath = mainPathSet.Contains(candidate);

            if (!placedOnMainPath && !onMainPath && dataCoreGrids.Count < 2)

            {

                continue;

            }



            dataCoreGrids.Add(candidate);

            if (onMainPath)

            {

                placedOnMainPath = true;

            }

        }



        if (dataCoreGrids.Count < 3)

        {

            dataCoreGrids.Clear();

            for (int i = 0; i < 3; i++)

            {

                dataCoreGrids.Add(dataCoreCandidates[i]);

            }

        }



        for (int i = 0; i < dataCoreGrids.Count; i++)

        {

            Vector2Int grid = dataCoreGrids[i];

            Level8ChunkSlot slot = slotsByGrid[grid];

            slot.HasDataCoreMarker = true;

            slot.DataCoreIndex = i + 1;



            if (slot.ChunkKind == Level8ChunkKind.Path)

            {

                slot.ChunkKind = Level8ChunkKind.DataCore;

            }

        }



        layout.Slots.Clear();

        layout.Slots.AddRange(

            occupied

                .OrderBy(p => p.y)

                .ThenBy(p => p.x)

                .Select(p => slotsByGrid[p]));



        return true;

    }



    static Vector2Int PickPreferred(

        List<Vector2Int> candidates,

        HashSet<Vector2Int> mainPathSet,

        bool preferOffPath,

        System.Random rng)

    {

        var preferred = candidates

            .Where(p => preferOffPath ? !mainPathSet.Contains(p) : mainPathSet.Contains(p))

            .ToList();



        if (preferred.Count == 0)

        {

            preferred = candidates;

        }



        return preferred[rng.Next(preferred.Count)];

    }



    static void BuildNeighborConnections(Level8ChunkLayout layout)

    {

        var slotMap = layout.Slots.ToDictionary(s => s.GridPosition);



        for (int i = 0; i < layout.Slots.Count; i++)

        {

            Level8ChunkSlot slot = layout.Slots[i];

            slot.ConnectedNeighbors.Clear();



            for (int d = 0; d < CardinalDirections.Length; d++)

            {

                Vector2Int neighbor = slot.GridPosition + CardinalDirections[d];

                if (slotMap.TryGetValue(neighbor, out Level8ChunkSlot neighborSlot))

                {

                    slot.ConnectedNeighbors.Add(neighbor);

                }

            }

        }

    }



    static void PopulateWorldCenters(Level8ChunkLayout layout)

    {

        for (int i = 0; i < layout.Slots.Count; i++)

        {

            layout.Slots[i].WorldCenter = Level8MapUtility.GetChunkCenter(layout.Slots[i].GridPosition);

        }

    }



    static void ValidateLayout(Level8LayoutGenerationResult result)

    {

        Level8ChunkLayout layout = result.Layout;

        if (layout == null || layout.Slots.Count == 0)

        {

            result.Success = false;

            result.FailureReason = "Layout is empty.";

            return;

        }



        int count = layout.Slots.Count;

        if (count < Level8GenerationFlags.ChunkCountMin || count > Level8GenerationFlags.ChunkCountMax)

        {

            result.Success = false;

            result.FailureReason = $"Chunk count {count} outside {Level8GenerationFlags.ChunkCountMin}-{Level8GenerationFlags.ChunkCountMax}.";

            return;

        }



        if (layout.SpawnSlot == null || layout.SpawnSlot.ChunkKind != Level8ChunkKind.Spawn)

        {

            result.Success = false;

            result.FailureReason = "Missing spawn chunk.";

            return;

        }



        if (layout.FinalSlot == null || layout.FinalSlot.ChunkKind != Level8ChunkKind.FinalObjective)

        {

            result.Success = false;

            result.FailureReason = "Missing final objective chunk.";

            return;

        }



        if (!layout.Slots.Any(s => s.ChunkKind == Level8ChunkKind.Resource))

        {

            result.Success = false;

            result.FailureReason = "Missing resource chunk.";

            return;

        }



        if (!layout.Slots.Any(s => s.ChunkKind == Level8ChunkKind.Hazard))

        {

            result.Success = false;

            result.FailureReason = "Missing hazard chunk.";

            return;

        }



        int dataCoreCount = layout.Slots.Count(s => s.HasDataCoreMarker);

        result.DataCoreCount = dataCoreCount;

        if (dataCoreCount != 3)

        {

            result.Success = false;

            result.FailureReason = $"Expected 3 data cores, found {dataCoreCount}.";

            return;

        }



        var grids = new HashSet<Vector2Int>();

        for (int i = 0; i < layout.Slots.Count; i++)

        {

            Vector2Int grid = layout.Slots[i].GridPosition;

            if (!InBounds(grid))

            {

                result.Success = false;

                result.FailureReason = $"Chunk {grid} outside grid bounds.";

                return;

            }



            if (!grids.Add(grid))

            {

                result.Success = false;

                result.FailureReason = $"Duplicate chunk at {grid}.";

                return;

            }

        }



        result.AllChunksInsideBounds = ValidateWorldBounds(layout);

        if (!result.AllChunksInsideBounds)

        {

            result.Success = false;

            result.FailureReason = "Chunk world centers outside 300x300 map.";

            return;

        }



        result.LayoutConnected = IsConnected(layout);

        if (!result.LayoutConnected)

        {

            result.Success = false;

            result.FailureReason = "Layout is not fully connected.";

            return;

        }



        if (!IsReachable(layout, layout.SpawnGrid, layout.FinalGrid))

        {

            result.Success = false;

            result.FailureReason = "Final objective is not reachable from spawn.";

            return;

        }



        result.Success = true;

    }



    static bool ValidateWorldBounds(Level8ChunkLayout layout)

    {

        float half = Level8MapUtility.MapHalfExtent;

        float halfChunk = Level8GenerationFlags.ChunkSize * 0.5f;



        for (int i = 0; i < layout.Slots.Count; i++)

        {

            Vector3 center = layout.Slots[i].WorldCenter;

            if (Mathf.Abs(center.x) + halfChunk > half || Mathf.Abs(center.z) + halfChunk > half)

            {

                return false;

            }

        }



        return true;

    }



    static bool IsConnected(Level8ChunkLayout layout)

    {

        if (layout.Slots.Count == 0)

        {

            return false;

        }



        var slotMap = layout.Slots.ToDictionary(s => s.GridPosition);

        var visited = new HashSet<Vector2Int>();

        var queue = new Queue<Vector2Int>();

        queue.Enqueue(layout.SpawnGrid);

        visited.Add(layout.SpawnGrid);



        while (queue.Count > 0)

        {

            Vector2Int current = queue.Dequeue();

            if (!slotMap.TryGetValue(current, out Level8ChunkSlot slot))

            {

                continue;

            }



            for (int i = 0; i < slot.ConnectedNeighbors.Count; i++)

            {

                Vector2Int neighbor = slot.ConnectedNeighbors[i];

                if (visited.Add(neighbor))

                {

                    queue.Enqueue(neighbor);

                }

            }

        }



        return visited.Count == layout.Slots.Count;

    }



    static bool IsReachable(Level8ChunkLayout layout, Vector2Int start, Vector2Int end)

    {

        var slotMap = layout.Slots.ToDictionary(s => s.GridPosition);

        if (!slotMap.ContainsKey(start) || !slotMap.ContainsKey(end))

        {

            return false;

        }



        var visited = new HashSet<Vector2Int>();

        var queue = new Queue<Vector2Int>();

        queue.Enqueue(start);

        visited.Add(start);



        while (queue.Count > 0)

        {

            Vector2Int current = queue.Dequeue();

            if (current == end)

            {

                return true;

            }



            Level8ChunkSlot slot = slotMap[current];

            for (int i = 0; i < slot.ConnectedNeighbors.Count; i++)

            {

                Vector2Int neighbor = slot.ConnectedNeighbors[i];

                if (visited.Add(neighbor))

                {

                    queue.Enqueue(neighbor);

                }

            }

        }



        return false;

    }



    static bool InBounds(Vector2Int grid)

    {

        return grid.x >= 0

            && grid.x < Level8GenerationFlags.GridSize

            && grid.y >= 0

            && grid.y < Level8GenerationFlags.GridSize;

    }



    static int Manhattan(Vector2Int a, Vector2Int b)

    {

        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

    }



    static Level8LayoutGenerationResult Fail(int attemptIndex, string reason)

    {

        return new Level8LayoutGenerationResult

        {

            Success = false,

            FailureReason = reason,

            AttemptIndex = attemptIndex,

        };

    }



    static void LogGenerationResult(Level8LayoutGenerationResult result)

    {

        Level8ChunkLayout layout = result.Layout;

        string dataCoreInfo = string.Join(", ",

            layout.Slots

                .Where(s => s.HasDataCoreMarker)

                .Select(s => $"({s.GridX},{s.GridZ})"));



        Debug.Log($"[Level8] Random generation seed: {layout.Seed}");

        Debug.Log($"[Level8] Generated chunk count: {layout.Slots.Count}");

        Debug.Log($"[Level8] Spawn chunk: ({layout.SpawnGrid.x},{layout.SpawnGrid.y})");

        Debug.Log($"[Level8] Final chunk: ({layout.FinalGrid.x},{layout.FinalGrid.y})");

        Debug.Log($"[Level8] Data Core chunks: {dataCoreInfo}");

        Debug.Log($"[Level8] Layout connected: {result.LayoutConnected}");

        Debug.Log($"[Level8] All chunks inside bounds: {result.AllChunksInsideBounds}");

    }

}

