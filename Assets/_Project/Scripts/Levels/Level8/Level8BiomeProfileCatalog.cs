using UnityEngine;

/// <summary>Default biome profiles for Level8 Phase 3.</summary>
public static class Level8BiomeProfileCatalog
{
    public static readonly Level8BiomeKind[] Phase3Biomes =
    {
        Level8BiomeKind.DataWilderness,
        Level8BiomeKind.Plain,
        Level8BiomeKind.Grassland,
        Level8BiomeKind.Hill,
        Level8BiomeKind.MountainEdge,
        Level8BiomeKind.Swamp,
        Level8BiomeKind.Forest,
    };

    public static Level8BiomeProfile GetProfile(Level8BiomeKind kind)
    {
        switch (kind)
        {
            case Level8BiomeKind.Plain: return Plain;
            case Level8BiomeKind.Grassland: return Grassland;
            case Level8BiomeKind.Hill: return Hill;
            case Level8BiomeKind.MountainEdge: return MountainEdge;
            case Level8BiomeKind.Swamp: return Swamp;
            case Level8BiomeKind.Forest: return Forest;
            default: return DataWilderness;
        }
    }

    public static Level8BiomeProfile DataWilderness => new Level8BiomeProfile
    {
        BiomeKind = Level8BiomeKind.DataWilderness,
        DisplayNameChinese = "数据荒野 / Data Wilderness",
        DescriptionChinese = "自然与数字腐蚀交织的混合荒野",
        BaseGroundColor = new Color(0.24f, 0.32f, 0.34f),
        ChunkTintColor = new Color(0.30f, 0.42f, 0.44f),
        PathTintColor = new Color(0.52f, 0.58f, 0.62f, 0.55f),
        EdgeBlendColor = new Color(0.30f, 0.38f, 0.40f, 0.7f),
        HazardColor = new Color(0.75f, 0.20f, 0.55f, 0.45f),
        DataCoreColor = new Color(0.35f, 0.90f, 1f),
        LandmarkColor = new Color(0.38f, 0.62f, 0.92f),
        DecorationDensity = 0.55f,
        HeightVariationAmplitude = 1.2f,
        HillFrequency = 0.35f,
        RockDensity = 0.35f,
        GrassDensity = 0.30f,
        TreeDensity = 0.15f,
        RuinDensity = 0.55f,
        DataCorruptionDensity = 0.65f,
        TransitionPropDensity = 0.50f,
        WoodWeight = 0.8f, StoneWeight = 0.7f, FiberWeight = 0.7f, VineWeight = 0.4f,
        BerryWeight = 0.5f, OreWeight = 0.6f, CoalWeight = 0.4f, DataShardWeight = 1.0f,
        PreferredHazardKind = Level8HazardKind.DataGlitchZone,
        HazardDensity = 0.45f, HazardSizeMultiplier = 1f,
        MaxHeightVariation = 1.5f, GroundRoughness = 0.4f,
        AllowWaterOrMud = false, AllowSteepPatches = false,
        MinResourceNodes = 2, MaxResourceNodes = 4, MinDangerZones = 1, MaxDangerZones = 2,
        BeginnerFriendly = false, DifficultyRating = 3,
    };

    public static Level8BiomeProfile Plain => new Level8BiomeProfile
    {
        BiomeKind = Level8BiomeKind.Plain,
        DisplayNameChinese = "平原 / Plain",
        DescriptionChinese = "开阔平坦的低地",
        BaseGroundColor = new Color(0.42f, 0.58f, 0.36f),
        ChunkTintColor = new Color(0.46f, 0.62f, 0.40f),
        PathTintColor = new Color(0.58f, 0.50f, 0.38f, 0.50f),
        EdgeBlendColor = new Color(0.44f, 0.56f, 0.38f, 0.65f),
        HazardColor = new Color(0.85f, 0.45f, 0.20f, 0.30f),
        DataCoreColor = new Color(0.55f, 0.75f, 0.95f),
        LandmarkColor = new Color(0.50f, 0.55f, 0.60f),
        DecorationDensity = 0.25f,
        HeightVariationAmplitude = 0.4f,
        HillFrequency = 0.05f, RockDensity = 0.20f, GrassDensity = 0.35f,
        TreeDensity = 0.05f, RuinDensity = 0.05f, DataCorruptionDensity = 0f,
        TransitionPropDensity = 0.25f,
        WoodWeight = 0.6f, StoneWeight = 0.4f, FiberWeight = 0.9f, VineWeight = 0.2f,
        BerryWeight = 0.5f, OreWeight = 0.2f, CoalWeight = 0.1f, DataShardWeight = 0.1f,
        PreferredHazardKind = Level8HazardKind.ThornField,
        HazardDensity = 0.15f, HazardSizeMultiplier = 0.75f,
        MaxHeightVariation = 0.5f, GroundRoughness = 0.15f,
        AllowWaterOrMud = false, AllowSteepPatches = false,
        MinResourceNodes = 1, MaxResourceNodes = 3, MinDangerZones = 0, MaxDangerZones = 1,
        BeginnerFriendly = true, DifficultyRating = 1,
    };

    public static Level8BiomeProfile Grassland => new Level8BiomeProfile
    {
        BiomeKind = Level8BiomeKind.Grassland,
        DisplayNameChinese = "草地 / Grassland",
        DescriptionChinese = "起伏草坡与灌木",
        BaseGroundColor = new Color(0.34f, 0.52f, 0.32f),
        ChunkTintColor = new Color(0.38f, 0.56f, 0.36f),
        PathTintColor = new Color(0.52f, 0.46f, 0.34f, 0.50f),
        EdgeBlendColor = new Color(0.36f, 0.50f, 0.34f, 0.65f),
        HazardColor = new Color(0.90f, 0.35f, 0.18f, 0.35f),
        DataCoreColor = new Color(0.40f, 0.85f, 0.95f),
        LandmarkColor = new Color(0.48f, 0.58f, 0.42f),
        DecorationDensity = 0.45f,
        HeightVariationAmplitude = 0.8f,
        HillFrequency = 0.20f, RockDensity = 0.25f, GrassDensity = 0.70f,
        TreeDensity = 0.15f, RuinDensity = 0.10f, DataCorruptionDensity = 0.05f,
        TransitionPropDensity = 0.40f,
        WoodWeight = 0.7f, StoneWeight = 0.4f, FiberWeight = 1.0f, VineWeight = 0.3f,
        BerryWeight = 0.9f, OreWeight = 0.2f, CoalWeight = 0.1f, DataShardWeight = 0.15f,
        PreferredHazardKind = Level8HazardKind.ThornField,
        HazardDensity = 0.25f, HazardSizeMultiplier = 0.85f,
        MaxHeightVariation = 1.0f, GroundRoughness = 0.25f,
        AllowWaterOrMud = false, AllowSteepPatches = false,
        MinResourceNodes = 2, MaxResourceNodes = 4, MinDangerZones = 1, MaxDangerZones = 2,
        BeginnerFriendly = true, DifficultyRating = 2,
    };

    public static Level8BiomeProfile Hill => new Level8BiomeProfile
    {
        BiomeKind = Level8BiomeKind.Hill,
        DisplayNameChinese = "丘陵 / Hill",
        DescriptionChinese = "缓丘与裸岩交错",
        BaseGroundColor = new Color(0.38f, 0.48f, 0.34f),
        ChunkTintColor = new Color(0.42f, 0.50f, 0.36f),
        PathTintColor = new Color(0.50f, 0.42f, 0.32f, 0.55f),
        EdgeBlendColor = new Color(0.40f, 0.46f, 0.36f, 0.68f),
        HazardColor = new Color(0.85f, 0.30f, 0.15f, 0.40f),
        DataCoreColor = new Color(0.38f, 0.82f, 0.92f),
        LandmarkColor = new Color(0.52f, 0.48f, 0.40f),
        DecorationDensity = 0.50f,
        HeightVariationAmplitude = 1.4f,
        HillFrequency = 0.65f, RockDensity = 0.55f, GrassDensity = 0.40f,
        TreeDensity = 0.20f, RuinDensity = 0.15f, DataCorruptionDensity = 0.08f,
        TransitionPropDensity = 0.45f,
        WoodWeight = 0.7f, StoneWeight = 0.9f, FiberWeight = 0.5f, VineWeight = 0.3f,
        BerryWeight = 0.4f, OreWeight = 0.8f, CoalWeight = 0.3f, DataShardWeight = 0.2f,
        PreferredHazardKind = Level8HazardKind.RockFallZone,
        HazardDensity = 0.40f, HazardSizeMultiplier = 1.0f,
        MaxHeightVariation = 2.0f, GroundRoughness = 0.45f,
        AllowWaterOrMud = false, AllowSteepPatches = true,
        MinResourceNodes = 2, MaxResourceNodes = 5, MinDangerZones = 1, MaxDangerZones = 2,
        BeginnerFriendly = false, DifficultyRating = 3,
    };

    public static Level8BiomeProfile MountainEdge => new Level8BiomeProfile
    {
        BiomeKind = Level8BiomeKind.MountainEdge,
        DisplayNameChinese = "山地边缘 / Mountain Edge",
        DescriptionChinese = "岩石山麓与断崖",
        BaseGroundColor = new Color(0.42f, 0.40f, 0.36f),
        ChunkTintColor = new Color(0.46f, 0.44f, 0.38f),
        PathTintColor = new Color(0.48f, 0.44f, 0.38f, 0.55f),
        EdgeBlendColor = new Color(0.44f, 0.42f, 0.38f, 0.70f),
        HazardColor = new Color(0.75f, 0.25f, 0.12f, 0.45f),
        DataCoreColor = new Color(0.42f, 0.78f, 0.88f),
        LandmarkColor = new Color(0.55f, 0.50f, 0.45f),
        DecorationDensity = 0.55f,
        HeightVariationAmplitude = 1.8f,
        HillFrequency = 0.75f, RockDensity = 0.80f, GrassDensity = 0.15f,
        TreeDensity = 0.05f, RuinDensity = 0.20f, DataCorruptionDensity = 0.05f,
        TransitionPropDensity = 0.50f,
        WoodWeight = 0.3f, StoneWeight = 1.0f, FiberWeight = 0.2f, VineWeight = 0.1f,
        BerryWeight = 0.2f, OreWeight = 1.0f, CoalWeight = 0.9f, DataShardWeight = 0.25f,
        PreferredHazardKind = Level8HazardKind.RockFallZone,
        HazardDensity = 0.55f, HazardSizeMultiplier = 1.15f,
        MaxHeightVariation = 2.5f, GroundRoughness = 0.55f,
        AllowWaterOrMud = false, AllowSteepPatches = true,
        MinResourceNodes = 2, MaxResourceNodes = 5, MinDangerZones = 1, MaxDangerZones = 3,
        BeginnerFriendly = false, DifficultyRating = 4,
    };

    public static Level8BiomeProfile Swamp => new Level8BiomeProfile
    {
        BiomeKind = Level8BiomeKind.Swamp,
        DisplayNameChinese = "沼泽 / Swamp",
        DescriptionChinese = "泥泞湿地与藤蔓",
        BaseGroundColor = new Color(0.22f, 0.32f, 0.24f),
        ChunkTintColor = new Color(0.26f, 0.36f, 0.28f),
        PathTintColor = new Color(0.38f, 0.34f, 0.26f, 0.55f),
        EdgeBlendColor = new Color(0.28f, 0.34f, 0.28f, 0.70f),
        HazardColor = new Color(0.20f, 0.35f, 0.18f, 0.55f),
        DataCoreColor = new Color(0.32f, 0.72f, 0.65f),
        LandmarkColor = new Color(0.35f, 0.48f, 0.38f),
        DecorationDensity = 0.50f,
        HeightVariationAmplitude = 0.5f,
        HillFrequency = 0.10f, RockDensity = 0.15f, GrassDensity = 0.45f,
        TreeDensity = 0.25f, RuinDensity = 0.10f, DataCorruptionDensity = 0.15f,
        TransitionPropDensity = 0.45f,
        WoodWeight = 0.4f, StoneWeight = 0.2f, FiberWeight = 1.0f, VineWeight = 1.0f,
        BerryWeight = 0.7f, OreWeight = 0.15f, CoalWeight = 0.1f, DataShardWeight = 0.15f,
        PreferredHazardKind = Level8HazardKind.PoisonSwampPatch,
        HazardDensity = 0.50f, HazardSizeMultiplier = 1.1f,
        MaxHeightVariation = 0.8f, GroundRoughness = 0.30f,
        AllowWaterOrMud = true, AllowSteepPatches = false,
        MinResourceNodes = 2, MaxResourceNodes = 4, MinDangerZones = 1, MaxDangerZones = 2,
        BeginnerFriendly = false, DifficultyRating = 3,
    };

    public static Level8BiomeProfile Forest => new Level8BiomeProfile
    {
        BiomeKind = Level8BiomeKind.Forest,
        DisplayNameChinese = "森林 / Forest",
        DescriptionChinese = "密林、倒木与灌木",
        BaseGroundColor = new Color(0.20f, 0.36f, 0.22f),
        ChunkTintColor = new Color(0.24f, 0.40f, 0.26f),
        PathTintColor = new Color(0.42f, 0.36f, 0.28f, 0.50f),
        EdgeBlendColor = new Color(0.26f, 0.38f, 0.28f, 0.68f),
        HazardColor = new Color(0.55f, 0.65f, 0.18f, 0.40f),
        DataCoreColor = new Color(0.35f, 0.88f, 0.75f),
        LandmarkColor = new Color(0.40f, 0.55f, 0.35f),
        DecorationDensity = 0.60f,
        HeightVariationAmplitude = 1.0f,
        HillFrequency = 0.25f, RockDensity = 0.20f, GrassDensity = 0.35f,
        TreeDensity = 0.85f, RuinDensity = 0.12f, DataCorruptionDensity = 0.08f,
        TransitionPropDensity = 0.55f,
        WoodWeight = 1.0f, StoneWeight = 0.3f, FiberWeight = 0.8f, VineWeight = 0.5f,
        BerryWeight = 0.8f, OreWeight = 0.2f, CoalWeight = 0.15f, DataShardWeight = 0.12f,
        PreferredHazardKind = Level8HazardKind.ThornField,
        HazardDensity = 0.35f, HazardSizeMultiplier = 0.95f,
        MaxHeightVariation = 1.2f, GroundRoughness = 0.35f,
        AllowWaterOrMud = false, AllowSteepPatches = false,
        MinResourceNodes = 2, MaxResourceNodes = 5, MinDangerZones = 1, MaxDangerZones = 2,
        BeginnerFriendly = true, DifficultyRating = 2,
    };
}
