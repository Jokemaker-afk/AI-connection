using UnityEngine;

/// <summary>Visual slice derived from Level8BiomeProfile for terrain/chunk rendering.</summary>
public struct Level8BiomeVisualProfile
{
    public string BiomeId;
    public string DisplayName;

    public Color BaseGroundColor;
    public Color PathTintColor;
    public Color ChunkTintAlpha;
    public Color EdgeBlendColor;
    public Color CorruptionAccentColor;
    public Color GrassPatchColor;
    public Color RockColor;
    public Color DataCoreColor;
    public Color LandmarkColor;
    public Color HazardColor;

    public float DecorationDensity;
    public float HeightVariationAmplitude;
    public float TransitionPropDensity;

    public static Level8BiomeVisualProfile FromBiome(Level8BiomeProfile biome) => biome.ToVisualProfile();

    public static Level8BiomeVisualProfile Default =>
        Level8BiomeProfileCatalog.GetProfile(Level8BiomeKind.DataWilderness).ToVisualProfile();
}

/// <summary>Per-chunk build context for visual blending and decoration.</summary>
public class Level8ChunkBuildContext
{
    public Level8BiomeProfile BiomeProfile;
    public Level8BiomeVisualProfile Profile;
    public System.Random Rng;
    public Level8ChunkSlot Slot;
    public bool ShowDebugBorders;
    public bool ShowDebugLabels;
    public bool UseContinuousTerrain;

    public static Level8ChunkBuildContext Create(Level8ChunkLayout layout, Level8ChunkSlot slot)
    {
        Level8BiomeProfile biome = layout?.BiomeProfile
            ?? Level8BiomeProfileCatalog.GetProfile(Level8BiomeKind.DataWilderness);

        int slotSeed = layout != null
            ? layout.Seed + slot.GridX * 131 + slot.GridZ * 719
            : slot.GridX * 131 + slot.GridZ * 719;

        return new Level8ChunkBuildContext
        {
            BiomeProfile = biome,
            Profile = biome.ToVisualProfile(),
            Rng = new System.Random(slotSeed),
            Slot = slot,
            ShowDebugBorders = Level8GenerationFlags.ShowChunkDebugBorders,
            ShowDebugLabels = Level8GenerationFlags.ShowChunkDebugLabels,
            UseContinuousTerrain = Level8GenerationFlags.UseContinuousTerrainBase,
        };
    }
}
