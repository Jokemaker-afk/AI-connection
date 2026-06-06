using UnityEngine;

/// <summary>
/// Phase 2: builds Level8 arena from a seed-driven chunk layout.
/// Falls back to fixed layout when generation fails.
/// </summary>
public static class Level8SeededLayoutBuilder
{
    public static GameObject Generate(bool removeOld = true, int? seedOverride = null)
    {
        int seed = ResolveSeed(seedOverride);
        Level8LayoutGenerationResult result = Level8LayoutGenerator.Generate(seed);

        if (!result.Success)
        {
            Debug.LogWarning(
                $"[Level8] Seeded layout failed (seed={seed}): {result.FailureReason}. Falling back to fixed test layout.");
            return Level8FixedLayoutBuilder.Generate(removeOld);
        }

        return BuildFromLayout(result.Layout, removeOld);
    }

    public static int ResolveSeed(int? seedOverride)
    {
        if (seedOverride.HasValue)
        {
            return seedOverride.Value;
        }

        if (GameplayCore.Exists)
        {
            int worldSeed = GameplayCore.Instance.ProgressionState.GetWorldSeed();
            return ModularLevelAssembler.CombineSeed(worldSeed, 8);
        }

        return Level8GenerationFlags.DefaultSeed;
    }

    static GameObject BuildFromLayout(Level8ChunkLayout layout, bool removeOld)
    {
        string welcome = Level8GenerationFlags.ShowGenerationSeedLabel
            ? $"第八关 · 种子随机布局\nSeed: {layout.Seed}"
            : string.Empty;

        GameObject root = Level8ArenaBuilder.BuildFromLayout(
            layout,
            removeOld,
            welcome,
            includeEmbeddedDataCoreInFinal: false);

        Debug.Log($"[Level8] Seeded layout built with {layout.Slots.Count} chunks (seed={layout.Seed}).");
        return root;
    }
}
