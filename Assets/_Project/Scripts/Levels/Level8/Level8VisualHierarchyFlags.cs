/// <summary>Visual hierarchy tuning for Level8 readability (does not affect generation logic).</summary>
public static class Level8VisualHierarchyFlags
{
    // Decoration noise reduction
    public const int MaxDecorationCountPerChunk = 4;
    public const float MinDecorationDistance = 3.5f;
    public const float DecorationScaleMin = 0.55f;
    public const float DecorationScaleMax = 0.85f;
    public const float DecorationDensityMultiplier = 0.6f;

    // Resource readability
    public const float ResourcePickupScale = 1.35f;
    public const int MaxResourcePickupsPerChunk = 4;

    // Spawn legend (testing aid)
    public const bool ShowLevel8LegendBoard = true;

    // Chunk area labels (resource/hazard chunk titles) — off in normal play
    public const bool ShowChunkAreaLabels = false;

    // Distance-based label visibility
    public const float DataCoreLabelDistance = 80f;
    public const float SignalRelayLabelDistance = 100f;
    public const float ExitLabelDistance = 100f;
    public const float HazardLabelDistance = 60f;
    public const float ResourceLabelDistance = 22f;
    public const float SpawnLabelDistance = 40f;
}
