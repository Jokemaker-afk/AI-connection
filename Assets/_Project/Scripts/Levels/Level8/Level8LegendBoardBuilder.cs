using UnityEngine;

/// <summary>Spawn legend board near Level8 spawn for visual category reference.</summary>
public static class Level8LegendBoardBuilder
{
    public static void BuildNearSpawn(Level8ArenaRoots roots, Vector3 spawnCenter)
    {
        if (!Level8VisualHierarchyFlags.ShowLevel8LegendBoard || roots?.Legend == null)
        {
            return;
        }

        Vector3 boardPos = spawnCenter + new Vector3(-10f, Level8ChunkPlaceholderBuilder.FloorTop + 0.05f, -6f);
        var board = new GameObject("LegendBoard");
        board.transform.SetParent(roots.Legend, false);
        board.transform.position = boardPos;

        var panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        panel.name = "Panel";
        panel.transform.SetParent(board.transform, false);
        panel.transform.localPosition = new Vector3(0f, 1.1f, 0f);
        panel.transform.localScale = new Vector3(4.2f, 2.2f, 0.12f);
        Object.Destroy(panel.GetComponent<Collider>());
        Level8VisualMaterialUtility.ApplyColor(panel.GetComponent<Renderer>(), new Color(0.12f, 0.14f, 0.18f, 0.85f), transparent: true);

        string legendText =
            "地图图例\n" +
            "蓝色光柱：数据核心\n" +
            "红色区域：危险区\n" +
            "灰/棕/绿小物：资源\n" +
            "高塔：信号中继器";

        Level8VisualHierarchyUtility.CreateDistanceLabelAnchor(
            board.transform,
            legendText,
            new Vector3(0f, 1.15f, -0.08f),
            Level8VisualHierarchyFlags.SpawnLabelDistance,
            characterSize: 0.09f,
            alwaysVisible: true);
    }
}
