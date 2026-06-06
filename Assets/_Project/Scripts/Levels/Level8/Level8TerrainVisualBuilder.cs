using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Phase 2.5 terrain visual blending: continuous base, edge strips, path, props, debug overlays.
/// </summary>
public static class Level8TerrainVisualBuilder
{
    static readonly Vector2Int[] CardinalDirections =
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
    };

    public static void BuildArenaVisuals(Transform terrainRoot, Level8ChunkLayout layout)
    {
        if (layout == null)
        {
            return;
        }

        Level8BiomeProfile biome = layout.BiomeProfile;
        Level8BiomeVisualProfile visual = biome.ToVisualProfile();
        var rng = new System.Random(layout.Seed + 90210);

        if (Level8GenerationFlags.UseContinuousTerrainBase)
        {
            CreateContinuousBaseGround(terrainRoot, visual);
        }

        if (Level8GenerationFlags.UseHeightVariation)
        {
            CreateHeightVariation(terrainRoot, layout, biome, visual, rng);
        }

        if (Level8GenerationFlags.UseChunkEdgeBlending)
        {
            CreateEdgeBlendingStrips(terrainRoot, layout, visual);
        }

        if (Level8GenerationFlags.UseNavigationPathStrips && layout.MainPath.Count >= 2)
        {
            CreateNavigationPathStrips(terrainRoot, layout, visual);
        }

        if (Level8GenerationFlags.UseTransitionProps)
        {
            CreateTransitionProps(terrainRoot, layout, biome, visual, rng);
        }

        if (Level8GenerationFlags.ShowChunkDebugBorders
            || Level8GenerationFlags.ShowChunkConnectionLines)
        {
            CreateDebugOverlays(terrainRoot, layout);
        }
    }

    public static void BuildNeighborConnections(Level8ChunkLayout layout)
    {
        var slotMap = new Dictionary<Vector2Int, Level8ChunkSlot>();
        for (int i = 0; i < layout.Slots.Count; i++)
        {
            slotMap[layout.Slots[i].GridPosition] = layout.Slots[i];
        }

        for (int i = 0; i < layout.Slots.Count; i++)
        {
            Level8ChunkSlot slot = layout.Slots[i];
            slot.ConnectedNeighbors.Clear();

            for (int d = 0; d < CardinalDirections.Length; d++)
            {
                Vector2Int neighbor = slot.GridPosition + CardinalDirections[d];
                if (slotMap.ContainsKey(neighbor))
                {
                    slot.ConnectedNeighbors.Add(neighbor);
                }
            }
        }
    }

    public static void CreateContinuousBaseGround(Transform parent, Level8BiomeVisualProfile profile)
    {
        var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "ContinuousBaseGround";
        floor.transform.SetParent(parent, false);
        floor.transform.position = new Vector3(0f, Level8ChunkPlaceholderBuilder.FloorTop, 0f);
        floor.transform.localScale = new Vector3(Level8GenerationFlags.MapSize, 0.14f, Level8GenerationFlags.MapSize);
        floor.isStatic = true;

        Level8VisualMaterialUtility.ApplyColor(floor.GetComponent<Renderer>(), profile.BaseGroundColor);
        floor.AddComponent<StaticPlacementGround>();
    }

    static void CreateHeightVariation(
        Transform parent,
        Level8ChunkLayout layout,
        Level8BiomeProfile biome,
        Level8BiomeVisualProfile profile,
        System.Random rng)
    {
        var variationRoot = Level8MapUtility.CreateChild(parent, "HeightVariation");

        for (int i = 0; i < layout.Slots.Count; i++)
        {
            Level8ChunkSlot slot = layout.Slots[i];
            int moundCount = 1 + (int)(biome.HillFrequency * 4f) + rng.Next(2);

            for (int m = 0; m < moundCount; m++)
            {
                if (rng.NextDouble() > biome.DecorationDensity * 0.85f)
                {
                    continue;
                }

                float offsetX = (float)(rng.NextDouble() * 30.0 - 15.0);
                float offsetZ = (float)(rng.NextDouble() * 30.0 - 15.0);
                float height = 0.3f + (float)rng.NextDouble() * profile.HeightVariationAmplitude;
                height = Mathf.Min(height, biome.MaxHeightVariation);
                float radius = 1.2f + (float)rng.NextDouble() * 2.5f;

                var mound = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                mound.name = $"Mound_{slot.GridX}_{slot.GridZ}_{m}";
                mound.transform.SetParent(variationRoot, false);
                mound.transform.position = slot.WorldCenter + new Vector3(offsetX, Level8ChunkPlaceholderBuilder.FloorTop + height * 0.5f, offsetZ);
                mound.transform.localScale = new Vector3(radius, height * 0.5f, radius);
                Object.Destroy(mound.GetComponent<Collider>());

                Color moundColor = Color.Lerp(profile.BaseGroundColor, profile.GrassPatchColor, 0.35f);
                Level8VisualMaterialUtility.ApplyColor(mound.GetComponent<Renderer>(), moundColor);
            }
        }
    }

    static void CreateEdgeBlendingStrips(Transform parent, Level8ChunkLayout layout, Level8BiomeVisualProfile profile)
    {
        var stripRoot = Level8MapUtility.CreateChild(parent, "EdgeBlending");
        var processed = new HashSet<string>();
        float stripW = Level8GenerationFlags.EdgeBlendStripWidth;

        for (int i = 0; i < layout.Slots.Count; i++)
        {
            Level8ChunkSlot slot = layout.Slots[i];
            for (int n = 0; n < slot.ConnectedNeighbors.Count; n++)
            {
                Vector2Int neighbor = slot.ConnectedNeighbors[n];
                string key = EdgeKey(slot.GridPosition, neighbor);
                if (!processed.Add(key))
                {
                    continue;
                }

                Vector3 a = slot.WorldCenter;
                Vector3 b = Level8MapUtility.GetChunkCenter(neighbor);
                Vector3 mid = (a + b) * 0.5f;
                Vector3 delta = b - a;

                var strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
                strip.name = $"EdgeBlend_{slot.GridX}_{slot.GridZ}_{neighbor.x}_{neighbor.y}";
                strip.transform.SetParent(stripRoot, false);
                strip.transform.position = mid + Vector3.up * 0.018f;

                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.z))
                {
                    strip.transform.localScale = new Vector3(stripW, 0.025f, Level8GenerationFlags.ChunkSize * 0.92f);
                }
                else
                {
                    strip.transform.localScale = new Vector3(Level8GenerationFlags.ChunkSize * 0.92f, 0.025f, stripW);
                }

                Object.Destroy(strip.GetComponent<Collider>());
                Level8VisualMaterialUtility.ApplyColor(strip.GetComponent<Renderer>(), profile.EdgeBlendColor, transparent: true);
            }
        }
    }

    static void CreateNavigationPathStrips(Transform parent, Level8ChunkLayout layout, Level8BiomeVisualProfile profile)
    {
        var pathRoot = Level8MapUtility.CreateChild(parent, "NavigationPath");

        for (int i = 0; i < layout.MainPath.Count - 1; i++)
        {
            Vector3 a = Level8MapUtility.GetChunkCenter(layout.MainPath[i]);
            Vector3 b = Level8MapUtility.GetChunkCenter(layout.MainPath[i + 1]);
            Vector3 mid = (a + b) * 0.5f;
            Vector3 delta = b - a;
            float length = delta.magnitude + Level8GenerationFlags.ChunkSize * 0.35f;

            var strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            strip.name = $"NavPath_{i}";
            strip.transform.SetParent(pathRoot, false);
            strip.transform.position = mid + Vector3.up * 0.012f;

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.z))
            {
                strip.transform.localScale = new Vector3(length, 0.018f, 6f);
            }
            else
            {
                strip.transform.localScale = new Vector3(6f, 0.018f, length);
            }

            Object.Destroy(strip.GetComponent<Collider>());
            Level8VisualMaterialUtility.ApplyColor(strip.GetComponent<Renderer>(), profile.PathTintColor, transparent: true);
        }
    }

    static void CreateTransitionProps(
        Transform parent,
        Level8ChunkLayout layout,
        Level8BiomeProfile biome,
        Level8BiomeVisualProfile profile,
        System.Random rng)
    {
        var propsRoot = Level8MapUtility.CreateChild(parent, "TransitionProps");
        var processed = new HashSet<string>();

        for (int i = 0; i < layout.Slots.Count; i++)
        {
            Level8ChunkSlot slot = layout.Slots[i];
            for (int n = 0; n < slot.ConnectedNeighbors.Count; n++)
            {
                Vector2Int neighbor = slot.ConnectedNeighbors[n];
                string key = EdgeKey(slot.GridPosition, neighbor);
                if (!processed.Add(key))
                {
                    continue;
                }

                if (rng.NextDouble() > profile.TransitionPropDensity)
                {
                    continue;
                }

                Vector3 mid = (slot.WorldCenter + Level8MapUtility.GetChunkCenter(neighbor)) * 0.5f;
                float jitterX = (float)(rng.NextDouble() * 4.0 - 2.0);
                float jitterZ = (float)(rng.NextDouble() * 4.0 - 2.0);
                Vector3 pos = mid + new Vector3(jitterX, Level8ChunkPlaceholderBuilder.FloorTop + 0.25f, jitterZ);

                Level8BiomeDecorationBuilder.CreateTransitionDecoration(propsRoot, pos, biome, rng, $"Edge_{key}");

                if (biome.DataCorruptionDensity > 0.2f && rng.NextDouble() < biome.DataCorruptionDensity * 0.5f)
                {
                    CreateCorruptionLine(propsRoot, slot.WorldCenter, Level8MapUtility.GetChunkCenter(neighbor), profile);
                }
            }
        }
    }

    public static void CreateRockCluster(Transform parent, Vector3 center, Level8BiomeVisualProfile profile, System.Random rng, string name)
    {
        int count = 2 + rng.Next(3);
        for (int i = 0; i < count; i++)
        {
            float ox = (float)(rng.NextDouble() * 2.4 - 1.2);
            float oz = (float)(rng.NextDouble() * 2.4 - 1.2);
            float scale = 0.35f + (float)rng.NextDouble() * 0.55f;

            var rock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rock.name = $"{name}_Rock{i}";
            rock.transform.SetParent(parent, false);
            rock.transform.position = center + new Vector3(ox, scale * 0.35f, oz);
            rock.transform.localScale = new Vector3(scale, scale * 0.7f, scale * 0.9f);
            rock.transform.rotation = Quaternion.Euler(0f, rng.Next(0, 360), 0f);
            Object.Destroy(rock.GetComponent<Collider>());
            Level8VisualMaterialUtility.ApplyColor(rock.GetComponent<Renderer>(), profile.RockColor);
        }
    }

    public static void CreateGrassPatch(Transform parent, Vector3 center, Level8BiomeVisualProfile profile, float size, string name)
    {
        var patch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        patch.name = name;
        patch.transform.SetParent(parent, false);
        patch.transform.position = center;
        patch.transform.localScale = new Vector3(size, 0.04f, size);
        Object.Destroy(patch.GetComponent<Collider>());
        Level8VisualMaterialUtility.ApplyColor(patch.GetComponent<Renderer>(), profile.GrassPatchColor, transparent: true);
    }

    static void CreateCorruptionLine(
        Transform parent,
        Vector3 a,
        Vector3 b,
        Level8BiomeVisualProfile profile)
    {
        Vector3 mid = (a + b) * 0.5f;
        Vector3 delta = b - a;

        var line = GameObject.CreatePrimitive(PrimitiveType.Cube);
        line.name = "CorruptionLine";
        line.transform.SetParent(parent, false);
        line.transform.position = mid + Vector3.up * 0.015f;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.z))
        {
            line.transform.localScale = new Vector3(delta.magnitude * 0.6f, 0.012f, 0.35f);
        }
        else
        {
            line.transform.localScale = new Vector3(0.35f, 0.012f, delta.magnitude * 0.6f);
        }

        Object.Destroy(line.GetComponent<Collider>());
        Level8VisualMaterialUtility.ApplyColor(line.GetComponent<Renderer>(), profile.CorruptionAccentColor, transparent: true);
    }

    static void CreateDebugOverlays(Transform parent, Level8ChunkLayout layout)
    {
        var debugRoot = Level8MapUtility.CreateChild(parent, "DebugOverlays");
        float half = Level8GenerationFlags.ChunkSize * 0.5f;
        float y = Level8ChunkPlaceholderBuilder.FloorTop + 0.15f;

        if (Level8GenerationFlags.ShowChunkDebugBorders)
        {
            for (int i = 0; i < layout.Slots.Count; i++)
            {
                Level8ChunkSlot slot = layout.Slots[i];
                Vector3 c = slot.WorldCenter;
                CreateDebugEdge(debugRoot, c + new Vector3(0f, y, half), new Vector3(Level8GenerationFlags.ChunkSize, 0.06f, 0.12f));
                CreateDebugEdge(debugRoot, c + new Vector3(0f, y, -half), new Vector3(Level8GenerationFlags.ChunkSize, 0.06f, 0.12f));
                CreateDebugEdge(debugRoot, c + new Vector3(half, y, 0f), new Vector3(0.12f, 0.06f, Level8GenerationFlags.ChunkSize));
                CreateDebugEdge(debugRoot, c + new Vector3(-half, y, 0f), new Vector3(0.12f, 0.06f, Level8GenerationFlags.ChunkSize));
            }
        }

        if (Level8GenerationFlags.ShowChunkConnectionLines)
        {
            var processed = new HashSet<string>();
            for (int i = 0; i < layout.Slots.Count; i++)
            {
                Level8ChunkSlot slot = layout.Slots[i];
                for (int n = 0; n < slot.ConnectedNeighbors.Count; n++)
                {
                    Vector2Int neighbor = slot.ConnectedNeighbors[n];
                    string key = EdgeKey(slot.GridPosition, neighbor);
                    if (!processed.Add(key))
                    {
                        continue;
                    }

                    CreateConnectionLine(debugRoot, slot.WorldCenter, Level8MapUtility.GetChunkCenter(neighbor));
                }
            }
        }
    }

    static void CreateDebugEdge(Transform parent, Vector3 position, Vector3 scale)
    {
        var edge = GameObject.CreatePrimitive(PrimitiveType.Cube);
        edge.name = "DebugChunkBorder";
        edge.transform.SetParent(parent, false);
        edge.transform.position = position;
        edge.transform.localScale = scale;
        Object.Destroy(edge.GetComponent<Collider>());
        Level8VisualMaterialUtility.ApplyColor(edge.GetComponent<Renderer>(), new Color(1f, 0.85f, 0.2f, 0.85f));
    }

    static void CreateConnectionLine(Transform parent, Vector3 a, Vector3 b)
    {
        Vector3 mid = (a + b) * 0.5f + Vector3.up * 0.3f;
        Vector3 delta = b - a;

        var line = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        line.name = "DebugConnectionLine";
        line.transform.SetParent(parent, false);
        line.transform.position = mid;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.z))
        {
            line.transform.localScale = new Vector3(0.08f, 0.04f, delta.magnitude);
            line.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
        else
        {
            line.transform.localScale = new Vector3(0.08f, 0.04f, delta.magnitude);
            line.transform.rotation = Quaternion.Euler(90f, 90f, 0f);
        }

        Object.Destroy(line.GetComponent<Collider>());
        Level8VisualMaterialUtility.ApplyColor(line.GetComponent<Renderer>(), new Color(0.2f, 0.9f, 1f, 0.8f));
    }

    static string EdgeKey(Vector2Int a, Vector2Int b)
    {
        if (a.x < b.x || (a.x == b.x && a.y < b.y))
        {
            return $"{a.x}_{a.y}_{b.x}_{b.y}";
        }

        return $"{b.x}_{b.y}_{a.x}_{a.y}";
    }
}
