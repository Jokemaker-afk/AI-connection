using UnityEngine;

/// <summary>Shared Level8 arena assembly for fixed and seeded layouts.</summary>
public static class Level8ArenaBuilder
{
    public static GameObject BuildFromLayout(
        Level8ChunkLayout layout,
        bool removeOld,
        string welcomeMessage,
        bool includeEmbeddedDataCoreInFinal = false)
    {
        if (removeOld)
        {
            Level8MapUtility.DestroyIfExists("Level8Arena");
        }

        Level8BiomeSelector.ApplyBiomeToLayout(layout);
        layout.BiomeId = layout.BiomeProfile.BiomeKind.ToString();

        Level8TerrainVisualBuilder.BuildNeighborConnections(layout);

        var root = new GameObject("Level8Arena");
        Level8ArenaRoots roots = Level8ArenaRoots.Create(root.transform);
        Level8ArenaBuildContext.Roots = roots;

        var terrainVisuals = Level8MapUtility.CreateChild(roots.Terrain, "TerrainVisuals");
        var boundary = Level8MapUtility.CreateChild(root.transform, "Boundary");

        Level8TerrainVisualBuilder.BuildArenaVisuals(terrainVisuals, layout);
        CreateBoundaryVisuals(boundary);

        Vector3 spawnCenter = layout.SpawnSlot != null
            ? layout.SpawnSlot.WorldCenter
            : Level8MapUtility.GetChunkCenter(0, 2);

        for (int i = 0; i < layout.Slots.Count; i++)
        {
            Level8ChunkSlot slot = layout.Slots[i];
            Level8ChunkBuildContext context = Level8ChunkBuildContext.Create(layout, slot);
            string label = BuildChunkLabel(slot, context);

            Level8ChunkPlaceholderBuilder.CreateChunk(
                roots.Chunks,
                slot.ChunkKind,
                slot.WorldCenter,
                label,
                includeEmbeddedDataCoreInFinal && slot.ChunkKind == Level8ChunkKind.FinalObjective,
                slot.HasDataCoreMarker,
                slot.DataCoreIndex,
                context);
        }

        CreateMarkerLabels(roots, layout, spawnCenter, welcomeMessage);
        Level8MapUtility.EnsurePlayerSpawn(spawnCenter);
        Level8MapUtility.PositionExistingPlayer(spawnCenter);

        Level8LastGeneratedSummary.Current = Level8ObjectiveBuilder.SetupObjectives(layout, root.transform);
        Level8VisualHierarchyOrganizer.Organize(root, roots);
        Level8LegendBoardBuilder.BuildNearSpawn(roots, spawnCenter);
        Level8ObjectAudit.AuditAndLog(root);

        Level8ArenaBuildContext.Roots = null;
        return root;
    }

    static void CreateBoundaryVisuals(Transform boundary)
    {
        if (!Level8GenerationFlags.UseContinuousTerrainBase)
        {
            Level8MapUtility.CreateBoundaryFloor(boundary);
        }

        Level8MapUtility.CreateBoundaryMarkers(boundary);
    }

    static void CreateMarkerLabels(
        Level8ArenaRoots roots,
        Level8ChunkLayout layout,
        Vector3 spawnCenter,
        string welcomeMessage)
    {
        Level8BiomeProfile biome = layout.BiomeProfile;

        if (Level8GenerationFlags.ShowBiomeLabelAtSpawn)
        {
            string biomeLine = $"当前环境：{biome.DisplayNameChinese}";
            string seedLine = Level8GenerationFlags.ShowGenerationSeedLabel
                ? $"\nBiome: {biome.BiomeKind} | Seed: {layout.Seed}"
                : string.Empty;

            var biomeLabel = new GameObject("BiomeLabel");
            biomeLabel.transform.SetParent(roots.Labels, false);
            biomeLabel.transform.position = spawnCenter + new Vector3(0f, 3.2f, -12f);
            Level8VisualHierarchyUtility.CreateDistanceLabelAnchor(
                biomeLabel.transform,
                biomeLine + seedLine,
                Vector3.zero,
                Level8VisualHierarchyFlags.SpawnLabelDistance,
                characterSize: 0.1f,
                alwaysVisible: true);
        }

        if (Level8GenerationFlags.ShowGenerationSeedLabel && !string.IsNullOrEmpty(welcomeMessage))
        {
            var welcome = new GameObject("WelcomeLabel");
            welcome.transform.SetParent(roots.Debug, false);
            welcome.transform.position = spawnCenter + new Vector3(0f, 3.8f, -14f);
            ItemWorldLabel.Create(welcome.transform, welcomeMessage, Vector3.zero, 0.09f);
        }

        if (Level8GenerationFlags.ShowChunkDebugLabels)
        {
            var boundsLabel = new GameObject("MapBoundsLabel");
            boundsLabel.transform.SetParent(roots.Debug, false);
            boundsLabel.transform.position = new Vector3(0f, 3f, -Level8MapUtility.MapHalfExtent + 20f);
            ItemWorldLabel.Create(
                boundsLabel.transform,
                $"地图范围 300×300 · {biome.DisplayNameChinese} · Seed: {layout.Seed}",
                Vector3.zero,
                0.09f);
        }
    }

    static string BuildChunkLabel(Level8ChunkSlot slot, Level8ChunkBuildContext context)
    {
        if (!context.ShowDebugLabels && !Level8GenerationFlags.ShowChunkGridCoordinates)
        {
            return BuildGameplayLabel(slot.ChunkKind);
        }

        string kindName = slot.ChunkKind switch
        {
            Level8ChunkKind.Spawn => "Spawn Chunk",
            Level8ChunkKind.Path => "Path Chunk",
            Level8ChunkKind.Resource => "Resource Chunk",
            Level8ChunkKind.Hazard => "Hazard Chunk",
            Level8ChunkKind.DataCore => "Data Core Chunk",
            Level8ChunkKind.FinalObjective => "Final Objective",
            _ => "Chunk",
        };

        string gridTag = Level8GenerationFlags.ShowChunkGridCoordinates
            ? $" ({slot.GridX},{slot.GridZ})"
            : string.Empty;

        return slot.ChunkKind switch
        {
            Level8ChunkKind.Spawn => $"{kindName}{gridTag}\n出生点 / Spawn",
            Level8ChunkKind.Resource => $"{kindName}{gridTag}\n资源区 / Resource Area",
            Level8ChunkKind.Hazard => $"{kindName}{gridTag}\n危险区域 / Danger Zone",
            Level8ChunkKind.DataCore => $"{kindName}{gridTag}\n数据核心 / Data Core",
            Level8ChunkKind.FinalObjective => $"{kindName}{gridTag}\n终点目标区 / Final Objective",
            _ => $"{kindName}{gridTag}",
        };
    }

    static string BuildGameplayLabel(Level8ChunkKind kind)
    {
        if (!Level8VisualHierarchyFlags.ShowChunkAreaLabels && !Level8GenerationFlags.ShowChunkDebugLabels)
        {
            return string.Empty;
        }

        return kind switch
        {
            Level8ChunkKind.Spawn => "出生点 / Spawn",
            Level8ChunkKind.Resource => "资源区 / Resource Area",
            Level8ChunkKind.Hazard => "危险区域 / Danger Zone",
            Level8ChunkKind.DataCore => "数据核心 / Data Core",
            Level8ChunkKind.FinalObjective => "终点目标区 / Final Objective",
            _ => string.Empty,
        };
    }
}
