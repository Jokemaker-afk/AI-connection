using UnityEngine;

/// <summary>
/// Full Level8 biome profile (Phase 3). Drives visuals, decoration, resources, and hazards.
/// </summary>
public struct Level8BiomeProfile
{
    public Level8BiomeKind BiomeKind;
    public string DisplayNameChinese;
    public string DescriptionChinese;

    public Color BaseGroundColor;
    public Color ChunkTintColor;
    public Color PathTintColor;
    public Color EdgeBlendColor;
    public Color HazardColor;
    public Color DataCoreColor;
    public Color LandmarkColor;

    public float DecorationDensity;
    public float HeightVariationAmplitude;
    public float HillFrequency;
    public float RockDensity;
    public float GrassDensity;
    public float TreeDensity;
    public float RuinDensity;
    public float DataCorruptionDensity;
    public float TransitionPropDensity;

    public float WoodWeight;
    public float StoneWeight;
    public float FiberWeight;
    public float VineWeight;
    public float BerryWeight;
    public float OreWeight;
    public float CoalWeight;
    public float DataShardWeight;

    public Level8HazardKind PreferredHazardKind;
    public float HazardDensity;
    public float HazardSizeMultiplier;

    public float MaxHeightVariation;
    public float GroundRoughness;
    public bool AllowWaterOrMud;
    public bool AllowSteepPatches;

    public int MinResourceNodes;
    public int MaxResourceNodes;
    public int MinDangerZones;
    public int MaxDangerZones;
    public bool BeginnerFriendly;
    public int DifficultyRating;

    public Level8BiomeVisualProfile ToVisualProfile()
    {
        return new Level8BiomeVisualProfile
        {
            BiomeId = BiomeKind.ToString(),
            DisplayName = DisplayNameChinese,
            BaseGroundColor = BaseGroundColor,
            PathTintColor = PathTintColor,
            ChunkTintAlpha = new Color(ChunkTintColor.r, ChunkTintColor.g, ChunkTintColor.b, 0.35f),
            EdgeBlendColor = EdgeBlendColor,
            CorruptionAccentColor = new Color(HazardColor.r, HazardColor.g, HazardColor.b, 0.45f),
            GrassPatchColor = new Color(GrassDensity + 0.2f, 0.52f, 0.32f, 0.65f),
            RockColor = new Color(0.45f, 0.44f, 0.42f),
            DecorationDensity = DecorationDensity,
            HeightVariationAmplitude = HeightVariationAmplitude,
            TransitionPropDensity = TransitionPropDensity,
            DataCoreColor = DataCoreColor,
            LandmarkColor = LandmarkColor,
            HazardColor = HazardColor,
        };
    }
}
