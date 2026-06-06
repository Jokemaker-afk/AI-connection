using UnityEngine;

/// <summary>Selects and applies Level8 biome at generation start.</summary>
public static class Level8BiomeSelector
{
    public static Level8BiomeKind SelectBiome(int layoutSeed)
    {
        if (!Level8GenerationFlags.UseRandomBiome)
        {
            return Level8GenerationFlags.ForcedBiomeKind;
        }

        Level8BiomeKind[] biomes = Level8BiomeProfileCatalog.Phase3Biomes;
        int biomeSeed = layoutSeed + Level8GenerationFlags.BiomeSeedOffset;
        int index = Mathf.Abs(biomeSeed * 486187739) % biomes.Length;
        return biomes[index];
    }

    public static void ApplyBiomeToLayout(Level8ChunkLayout layout)
    {
        if (layout == null)
        {
            return;
        }

        layout.SelectedBiomeKind = SelectBiome(layout.Seed);
        layout.BiomeProfile = Level8BiomeProfileCatalog.GetProfile(layout.SelectedBiomeKind);

        Level8BiomeProfile profile = layout.BiomeProfile;
        Debug.Log($"[Level8] biome selected: {profile.DisplayNameChinese}");
        Debug.Log($"[Level8] Biome visual profile applied: {profile.BiomeKind}");
        Debug.Log($"[Level8] Decoration density: {profile.DecorationDensity:F2}");
        Debug.Log($"[Level8] Height variation amplitude: {profile.HeightVariationAmplitude:F2}");
    }
}
