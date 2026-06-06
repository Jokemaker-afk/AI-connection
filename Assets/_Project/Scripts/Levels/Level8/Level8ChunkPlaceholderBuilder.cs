using UnityEngine;

public static class Level8ChunkPlaceholderBuilder
{
    public const float ChunkSize = Level8GenerationFlags.ChunkSize;
    public const float FloorTop = 0.125f;
    public const float FloorThickness = 0.25f;

    public static GameObject CreateChunk(
        Transform parent,
        Level8ChunkKind kind,
        Vector3 center,
        string areaLabelChinese,
        bool includeEmbeddedDataCoreInFinal = true,
        bool hasDataCoreMarker = false,
        int dataCoreIndex = -1,
        Level8ChunkBuildContext context = null)
    {
        context ??= new Level8ChunkBuildContext
        {
            BiomeProfile = Level8BiomeProfileCatalog.GetProfile(Level8BiomeKind.DataWilderness),
            Profile = Level8BiomeProfileCatalog.GetProfile(Level8BiomeKind.DataWilderness).ToVisualProfile(),
            Rng = new System.Random((int)(center.x * 17 + center.z * 31)),
            UseContinuousTerrain = Level8GenerationFlags.UseContinuousTerrainBase,
            ShowDebugBorders = Level8GenerationFlags.ShowChunkDebugBorders,
            ShowDebugLabels = Level8GenerationFlags.ShowChunkDebugLabels,
        };

        var chunkRoot = new GameObject($"Chunk_{kind}_{center.x:F0}_{center.z:F0}");
        chunkRoot.transform.SetParent(parent, false);
        chunkRoot.transform.position = Vector3.zero;

        var terrain = CreateChild(chunkRoot.transform, "Terrain");
        var props = CreateChild(chunkRoot.transform, "Props");
        var decorations = CreateChild(chunkRoot.transform, "Decorations");
        var sockets = CreateChild(chunkRoot.transform, "Sockets");
        var labels = CreateChild(chunkRoot.transform, "Labels");

        Color groundColor = ResolveGroundColor(kind, context.Profile);

        if (context.UseContinuousTerrain)
        {
            CreateVisualTintPatch(terrain, "ChunkTint", center, groundColor, context.Profile);
        }
        else
        {
            CreateCollidableGroundPlate(terrain, "Ground", center, new Vector3(ChunkSize, FloorThickness, ChunkSize), groundColor);
        }

        CreateSocket(sockets, "Socket_Center", center + Vector3.up * 0.05f);

        if (!string.IsNullOrEmpty(areaLabelChinese)
            && (Level8VisualHierarchyFlags.ShowChunkAreaLabels || context.ShowDebugLabels))
        {
            CreateAreaLabel(labels, "AreaLabel", center + new Vector3(0f, 2.2f, 0f), areaLabelChinese);
        }

        switch (kind)
        {
            case Level8ChunkKind.Spawn:
                BuildSpawnChunk(props, labels, center, context, decorations);
                break;
            case Level8ChunkKind.Path:
                BuildPathChunk(props, decorations, center, context);
                break;
            case Level8ChunkKind.Resource:
                BuildResourceChunk(props, decorations, center, context);
                break;
            case Level8ChunkKind.Hazard:
                BuildHazardChunk(props, labels, decorations, center, context);
                break;
            case Level8ChunkKind.DataCore:
                BuildDataCoreChunk(props, labels, decorations, center, dataCoreIndex, context);
                break;
            case Level8ChunkKind.FinalObjective:
                BuildFinalObjectiveChunk(props, labels, decorations, center, includeEmbeddedDataCoreInFinal, context);
                break;
        }

        if (hasDataCoreMarker && kind != Level8ChunkKind.DataCore && kind != Level8ChunkKind.FinalObjective)
        {
            PlaceDataCoreMarker(props, labels, center, dataCoreIndex, kind, context.BiomeProfile.DataCoreColor, context.ShowDebugLabels);
        }

        Level8BiomeDecorationBuilder.DecorateChunkInterior(decorations, center, kind, context.BiomeProfile, context.Rng);

        if (context.ShowDebugBorders)
        {
            CreateDebugChunkBorder(terrain, center);
        }

        return chunkRoot;
    }

    public static void PlaceDataCoreMarker(
        Transform props,
        Transform labels,
        Vector3 center,
        int dataCoreIndex,
        Level8ChunkKind hostKind,
        Color dataCoreColor,
        bool showLabel = true)
    {
        // Functional DataCoreCollectible instances are spawned by Level8ObjectiveBuilder.
        CreateVisualPlatform(
            props,
            $"DataCoreZone_{dataCoreIndex}",
            center + new Vector3(0f, 0.02f, 0f),
            new Vector3(4f, 0.04f, 4f),
            new Color(dataCoreColor.r, dataCoreColor.g, dataCoreColor.b, 0.35f));
    }

    static void BuildSpawnChunk(Transform props, Transform labels, Vector3 center, Level8ChunkBuildContext context, Transform decorations)
    {
        CreateVisualPlatform(props, "SpawnPlatform", center + new Vector3(0f, 0.03f, -8f), new Vector3(8f, 0.06f, 8f), new Color(0.35f, 0.85f, 0.95f, 0.4f));
        CreateMarkerPillar(props, "SpawnMarker", center + new Vector3(0f, 0.6f, -8f), new Color(0.35f, 0.85f, 0.95f), new Vector3(0.5f, 1.2f, 0.5f));

        if (Level8VisualHierarchyFlags.ShowChunkAreaLabels || context.ShowDebugLabels)
        {
            CreateWorldLabel(labels, "SpawnLabel", center + new Vector3(0f, 2.8f, -8f), "出生点 / Spawn");
        }

        Level8TerrainVisualBuilder.CreateGrassPatch(decorations, center + new Vector3(-6f, FloorTop + 0.02f, 4f), context.Profile, 3f, "SpawnGrassA");
        Level8TerrainVisualBuilder.CreateGrassPatch(decorations, center + new Vector3(5f, FloorTop + 0.02f, -3f), context.Profile, 2.5f, "SpawnGrassB");
    }

    static void BuildPathChunk(Transform props, Transform decorations, Vector3 center, Level8ChunkBuildContext context)
    {
        CreateVisualPlatform(props, "PathWear", center + new Vector3(0f, 0.012f, 0f), new Vector3(ChunkSize * 0.28f, 0.018f, ChunkSize * 0.75f), context.Profile.PathTintColor);

        int rockCount = 2 + context.Rng.Next(3);
        for (int i = 0; i < rockCount; i++)
        {
            float ox = (float)(context.Rng.NextDouble() * 36.0 - 18.0);
            float oz = (float)(context.Rng.NextDouble() * 36.0 - 18.0);
            Level8TerrainVisualBuilder.CreateRockCluster(
                decorations,
                center + new Vector3(ox, FloorTop + 0.2f, oz),
                context.Profile,
                context.Rng,
                $"PathRock_{i}");
        }

        CreateBrokenPathMarker(decorations, center + new Vector3(-8f, FloorTop + 0.05f, 6f), context);
        CreateBrokenPathMarker(decorations, center + new Vector3(10f, FloorTop + 0.05f, -7f), context);
    }

    static void BuildResourceChunk(Transform props, Transform decorations, Vector3 center, Level8ChunkBuildContext context)
    {
        Level8BiomeResourcePlacer.SpawnBiomeResources(props, center, context.BiomeProfile, context.Rng);
        CreateVisualPlatform(
            props,
            "ResourceZoneTint",
            center + new Vector3(0f, 0.015f, 0f),
            new Vector3(ChunkSize * 0.35f, 0.02f, ChunkSize * 0.35f),
            new Color(context.Profile.GrassPatchColor.r, context.Profile.GrassPatchColor.g, context.Profile.GrassPatchColor.b, 0.25f));
    }

    static void BuildHazardChunk(Transform props, Transform labels, Transform decorations, Vector3 center, Level8ChunkBuildContext context)
    {
        Level8BiomeDecorationBuilder.BuildHazardVisuals(props, decorations, center, context.BiomeProfile);
        Level8HazardZoneBuilder.Build(Level8ArenaBuildContext.Roots?.Hazards, center, context.BiomeProfile);
    }

    static void BuildDataCoreChunk(Transform props, Transform labels, Transform decorations, Vector3 center, int dataCoreIndex, Level8ChunkBuildContext context)
    {
        CreateVisualPlatform(props, "DataCorePlatform", center + new Vector3(0f, 0.04f, 0f), new Vector3(ChunkSize * 0.35f, 0.06f, ChunkSize * 0.35f), new Color(context.BiomeProfile.DataCoreColor.r, context.BiomeProfile.DataCoreColor.g, context.BiomeProfile.DataCoreColor.b, 0.55f));
        PlaceDataCoreMarker(props, labels, center, dataCoreIndex > 0 ? dataCoreIndex : 1, Level8ChunkKind.DataCore, context.BiomeProfile.DataCoreColor, showLabel: true);
        CreateRuinedTechProp(decorations, center + new Vector3(-8f, FloorTop + 0.3f, -6f));
        CreateRuinedTechProp(decorations, center + new Vector3(7f, FloorTop + 0.3f, 5f));
    }

    static void BuildFinalObjectiveChunk(
        Transform props,
        Transform labels,
        Transform decorations,
        Vector3 center,
        bool includeEmbeddedDataCore,
        Level8ChunkBuildContext context)
    {
        Color landmark = context.BiomeProfile.LandmarkColor;
        CreateVisualPlatform(props, "ObjectivePlatform", center + new Vector3(0f, 0.05f, 0f), new Vector3(ChunkSize * 0.5f, 0.08f, ChunkSize * 0.5f), new Color(landmark.r, landmark.g, landmark.b, 0.65f));
        CreateBrokenStructure(decorations, center + new Vector3(-6f, FloorTop + 0.5f, -2f));
    }

    static void CreateVisualTintPatch(Transform parent, string name, Vector3 center, Color kindColor, Level8BiomeVisualProfile profile)
    {
        Color tint = Color.Lerp(profile.BaseGroundColor, kindColor, profile.ChunkTintAlpha.a);
        tint.a = 0.45f;

        var patch = GameObject.CreatePrimitive(PrimitiveType.Cube);
        patch.name = name;
        patch.transform.SetParent(parent, false);
        patch.transform.position = center + Vector3.up * 0.008f;
        patch.transform.localScale = new Vector3(ChunkSize - 1.5f, 0.016f, ChunkSize - 1.5f);
        Object.Destroy(patch.GetComponent<Collider>());
        Level8VisualMaterialUtility.ApplyColor(patch.GetComponent<Renderer>(), tint, transparent: true);
    }

    static void CreateCollidableGroundPlate(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent, false);
        cube.transform.position = position;
        cube.transform.localScale = scale;
        cube.isStatic = true;
        Level8VisualMaterialUtility.ApplyColor(cube.GetComponent<Renderer>(), color);
        cube.AddComponent<StaticPlacementGround>();
    }

    static void CreateVisualPlatform(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent, false);
        cube.transform.position = position;
        cube.transform.localScale = scale;
        Object.Destroy(cube.GetComponent<Collider>());
        Level8VisualMaterialUtility.ApplyColor(cube.GetComponent<Renderer>(), color, transparent: color.a < 0.99f);
    }

    static void CreateDataCorePedestal(Transform props, Vector3 position, Color color)
    {
        var pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pedestal.name = "DataCorePedestal";
        pedestal.transform.SetParent(props, false);
        pedestal.transform.position = position + new Vector3(0f, -0.35f, 0f);
        pedestal.transform.localScale = new Vector3(1.1f, 0.25f, 1.1f);
        Object.Destroy(pedestal.GetComponent<Collider>());
        Level8VisualMaterialUtility.ApplyColor(pedestal.GetComponent<Renderer>(), Color.Lerp(color, Color.gray, 0.35f));
    }

    static void CreateBrokenPathMarker(Transform parent, Vector3 position, Level8ChunkBuildContext context)
    {
        var piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
        piece.name = "BrokenPathPiece";
        piece.transform.SetParent(parent, false);
        piece.transform.position = position;
        piece.transform.localScale = new Vector3(1.4f, 0.08f, 0.6f);
        piece.transform.rotation = Quaternion.Euler(0f, context.Rng.Next(0, 40), 0f);
        Object.Destroy(piece.GetComponent<Collider>());
        Level8VisualMaterialUtility.ApplyColor(piece.GetComponent<Renderer>(), context.Profile.PathTintColor);
    }

    static void CreateWarningMarker(Transform parent, Vector3 position)
    {
        var marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        marker.name = "WarningMarker";
        marker.transform.SetParent(parent, false);
        marker.transform.position = position;
        marker.transform.localScale = new Vector3(0.25f, 0.7f, 0.25f);
        Object.Destroy(marker.GetComponent<Collider>());
        Level8VisualMaterialUtility.ApplyColor(marker.GetComponent<Renderer>(), new Color(0.95f, 0.35f, 0.15f));
    }

    static void CreateRuinedTechProp(Transform parent, Vector3 position)
    {
        var prop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        prop.name = "RuinedTech";
        prop.transform.SetParent(parent, false);
        prop.transform.position = position;
        prop.transform.localScale = new Vector3(0.8f, 0.5f, 0.6f);
        prop.transform.rotation = Quaternion.Euler(0f, 25f, 8f);
        Object.Destroy(prop.GetComponent<Collider>());
        Level8VisualMaterialUtility.ApplyColor(prop.GetComponent<Renderer>(), new Color(0.38f, 0.42f, 0.48f));
    }

    static void CreateBrokenStructure(Transform parent, Vector3 position)
    {
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "BrokenWall";
        wall.transform.SetParent(parent, false);
        wall.transform.position = position;
        wall.transform.localScale = new Vector3(3f, 1.8f, 0.4f);
        Object.Destroy(wall.GetComponent<Collider>());
        Level8VisualMaterialUtility.ApplyColor(wall.GetComponent<Renderer>(), new Color(0.35f, 0.38f, 0.45f));

        var beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
        beam.name = "BrokenBeam";
        beam.transform.SetParent(parent, false);
        beam.transform.position = position + new Vector3(1.2f, 1.2f, 0.8f);
        beam.transform.localScale = new Vector3(2.5f, 0.25f, 0.25f);
        beam.transform.rotation = Quaternion.Euler(0f, 0f, 35f);
        Object.Destroy(beam.GetComponent<Collider>());
        Level8VisualMaterialUtility.ApplyColor(beam.GetComponent<Renderer>(), new Color(0.42f, 0.44f, 0.5f));
    }

    static void CreateDebugChunkBorder(Transform parent, Vector3 center)
    {
        float half = ChunkSize * 0.5f;
        float y = FloorTop + 0.12f;
        CreateVisualPlatform(parent, "DebugBorderN", center + new Vector3(0f, y, half), new Vector3(ChunkSize, 0.04f, 0.1f), new Color(1f, 0.9f, 0.2f, 0.9f));
        CreateVisualPlatform(parent, "DebugBorderS", center + new Vector3(0f, y, -half), new Vector3(ChunkSize, 0.04f, 0.1f), new Color(1f, 0.9f, 0.2f, 0.9f));
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

    static Color ResolveGroundColor(Level8ChunkKind kind, Level8BiomeVisualProfile profile)
    {
        switch (kind)
        {
            case Level8ChunkKind.Spawn:
                return Color.Lerp(profile.BaseGroundColor, profile.GrassPatchColor, 0.35f);
            case Level8ChunkKind.Path:
                return profile.PathTintColor;
            case Level8ChunkKind.Resource:
                return Color.Lerp(profile.BaseGroundColor, profile.GrassPatchColor, 0.25f);
            case Level8ChunkKind.Hazard:
                return Color.Lerp(profile.BaseGroundColor, profile.HazardColor, 0.2f);
            case Level8ChunkKind.DataCore:
                return Color.Lerp(profile.BaseGroundColor, profile.DataCoreColor, 0.15f);
            case Level8ChunkKind.FinalObjective:
                return Color.Lerp(profile.BaseGroundColor, profile.LandmarkColor, 0.2f);
            default:
                return profile.BaseGroundColor;
        }
    }

    static void CreateTransparentZone(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent, false);
        cube.transform.position = position;
        cube.transform.localScale = scale;
        cube.isStatic = true;

        Collider collider = cube.GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }

        Level8VisualMaterialUtility.ApplyColor(cube.GetComponent<Renderer>(), color, transparent: true);
    }

    static void CreateMarkerPillar(Transform parent, string name, Vector3 position, Color color, Vector3 scale)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent, false);
        cube.transform.position = position;
        cube.transform.localScale = scale;
        Object.Destroy(cube.GetComponent<Collider>());
        Level8VisualMaterialUtility.ApplyColor(cube.GetComponent<Renderer>(), color);
    }

    static void CreateGlowSphere(Transform parent, string name, Vector3 position, Color color, float radius)
    {
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = name;
        sphere.transform.SetParent(parent, false);
        sphere.transform.position = position;
        sphere.transform.localScale = Vector3.one * radius * 2f;
        Object.Destroy(sphere.GetComponent<Collider>());
        Level8VisualMaterialUtility.ApplyColor(sphere.GetComponent<Renderer>(), color);
    }

    static void CreateSignalRelayPlaceholder(Transform parent, Vector3 position, Color accent)
    {
        var baseCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseCube.name = "SignalRelayBase";
        baseCube.transform.SetParent(parent, false);
        baseCube.transform.position = position;
        baseCube.transform.localScale = new Vector3(1f, 1.2f, 1f);
        Object.Destroy(baseCube.GetComponent<Collider>());
        Level8VisualMaterialUtility.ApplyColor(baseCube.GetComponent<Renderer>(), accent);

        var antenna = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        antenna.name = "SignalRelayAntenna";
        antenna.transform.SetParent(parent, false);
        antenna.transform.position = position + new Vector3(0f, 1.1f, 0f);
        antenna.transform.localScale = new Vector3(0.18f, 0.7f, 0.18f);
        Object.Destroy(antenna.GetComponent<Collider>());
        Level8VisualMaterialUtility.ApplyColor(antenna.GetComponent<Renderer>(), Color.Lerp(accent, Color.white, 0.25f));
    }

    static void CreateSocket(Transform parent, string name, Vector3 position)
    {
        var socket = new GameObject(name);
        socket.transform.SetParent(parent, false);
        socket.transform.position = position;
    }

    static void CreateAreaLabel(Transform parent, string name, Vector3 position, string text)
    {
        CreateWorldLabel(parent, name, position, text);
    }

    static void CreateWorldLabel(Transform parent, string name, Vector3 position, string text)
    {
        var labelRoot = new GameObject(name);
        labelRoot.transform.SetParent(parent, false);
        labelRoot.transform.position = position;
        ItemWorldLabel.Create(labelRoot.transform, text, Vector3.zero, 0.11f);
    }

    static Transform CreateChild(Transform parent, string childName)
    {
        var go = new GameObject(childName);
        go.transform.SetParent(parent, false);
        return go.transform;
    }
}
