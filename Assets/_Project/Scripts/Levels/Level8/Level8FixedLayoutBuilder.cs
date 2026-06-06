using UnityEngine;

/// <summary>
/// Phase 1 fixed Level8 layout: 6 connected placeholder chunks within 300x300.
/// </summary>
public static class Level8FixedLayoutBuilder
{
    public const float MapSize = Level8GenerationFlags.MapSize;
    public const float MapHeightBudget = Level8GenerationFlags.MapHeightBudget;
    public const float MapHalfExtent = Level8GenerationFlags.MapSize * 0.5f;

    public static GameObject Generate(bool removeOld = true)
    {
        Level8ChunkLayout layout = Level8FixedLayoutAdapter.CreateLayout();
        GameObject root = Level8ArenaBuilder.BuildFromLayout(
            layout,
            removeOld,
            "第八关 · 固定测试布局（Phase 1）\n数据荒野探索原型",
            includeEmbeddedDataCoreInFinal: false);

        Debug.Log("[Level8] Fixed test layout generated: 6 placeholder chunks within 300x300.");
        return root;
    }

    public static Vector3 GetChunkCenter(int gridX, int gridZ)
    {
        return Level8MapUtility.GetChunkCenter(gridX, gridZ);
    }
}
