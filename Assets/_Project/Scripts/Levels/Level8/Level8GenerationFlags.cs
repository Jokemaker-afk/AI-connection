/// <summary>
/// Level 8 generation mode flags.
/// Phase 1: fixed test layout. Phase 2: seed-based semi-random layout.
/// Phase 2.5: terrain visual blending.
/// </summary>
public static class Level8GenerationFlags
{
    /// <summary>When true, uses Phase 1 fixed authored layout (debug fallback).</summary>
    public const bool UseFixedTestLayout = false;

    /// <summary>When true and fixed layout is off, generates seed-based random chunk layout.</summary>
    public const bool UseSeededRandomLayout = true;

    /// <summary>Default layout seed when no save progression seed is available.</summary>
    public const int DefaultSeed = 12345;

    public const int ChunkCountMin = 6;
    public const int ChunkCountMax = 10;

    public const float ChunkSize = 50f;
    public const float MapSize = 300f;
    public const float MapHeightBudget = 100f;

    public const int GridSize = 6;

    /// <summary>Biome placeholder for Phase 2 (full profiles in Phase 3).</summary>
    public const string DefaultBiomeId = "data_wilderness";

    public const string DefaultBiomeDisplayName = "数据荒野 / Data Wilderness";

    public const int MaxGenerationAttempts = 8;

    // Phase 3 — biome selection
    public const bool UseRandomBiome = true;
    public const Level8BiomeKind ForcedBiomeKind = Level8BiomeKind.DataWilderness;
    public const int BiomeSeedOffset = 42001;
    public const bool AllowBiomeMixing = false;
    public const bool ShowBiomeLabelAtSpawn = true;

    // Phase 2.5 — terrain visual blending
    public const bool UseContinuousTerrainBase = true;
    public const bool UseChunkEdgeBlending = true;
    public const bool UseNavigationPathStrips = true;
    public const bool UseTransitionProps = true;
    public const bool UseHeightVariation = true;

    public const float EdgeBlendStripWidth = 3.5f;
    public const float HeightVariationMax = 1.5f;
    public const float MoundHeightMax = 2f;

    // Debug visualization (default off for natural look)
    public const bool ShowChunkDebugBorders = false;
    public const bool ShowChunkDebugLabels = false;
    public const bool ShowChunkGridCoordinates = false;
    public const bool ShowGenerationSeedLabel = false;
    public const bool ShowChunkConnectionLines = false;

    public static bool UsesLevel8ChunkLayout =>
        UseFixedTestLayout || UseSeededRandomLayout;
}
