using UnityEngine;

/// <summary>Biome-specific placeholder decorations for Level8 chunks and transitions.</summary>
public static class Level8BiomeDecorationBuilder
{
    public static void DecorateChunkInterior(
        Transform decorations,
        Vector3 center,
        Level8ChunkKind chunkKind,
        Level8BiomeProfile biome,
        System.Random rng)
    {
        if (chunkKind == Level8ChunkKind.Hazard || chunkKind == Level8ChunkKind.FinalObjective)
        {
            return;
        }

        float density = biome.DecorationDensity * Level8VisualHierarchyFlags.DecorationDensityMultiplier;
        int propCount = Mathf.Min(
            Level8VisualHierarchyFlags.MaxDecorationCountPerChunk,
            1 + (int)(density * 3f));

        var placed = new System.Collections.Generic.List<Vector3>();
        for (int i = 0; i < propCount; i++)
        {
            if (rng.NextDouble() > density)
            {
                continue;
            }

            Vector3 pos = PickDecorationPosition(center, rng, placed);
            placed.Add(pos);
            float scale = Level8VisualHierarchyUtility.SampleDecorationScale(rng);

            var propRoot = new GameObject($"Decoration_{i}");
            propRoot.transform.SetParent(decorations, false);
            propRoot.transform.position = pos;
            propRoot.transform.localScale = Vector3.one * scale;

            switch (biome.BiomeKind)
            {
                case Level8BiomeKind.Plain:
                    DecoratePlain(propRoot.transform, pos, biome, rng, i);
                    break;
                case Level8BiomeKind.Grassland:
                    DecorateGrassland(propRoot.transform, pos, biome, rng, i);
                    break;
                case Level8BiomeKind.Hill:
                case Level8BiomeKind.MountainEdge:
                    DecorateHill(propRoot.transform, pos, biome, rng, i);
                    break;
                case Level8BiomeKind.Swamp:
                    DecorateSwamp(propRoot.transform, pos, biome, rng, i);
                    break;
                case Level8BiomeKind.Forest:
                    DecorateForest(propRoot.transform, pos, biome, rng, i);
                    break;
                default:
                    DecorateDataWilderness(propRoot.transform, pos, biome, rng, i);
                    break;
            }
        }
    }

    static Vector3 PickDecorationPosition(Vector3 center, System.Random rng, System.Collections.Generic.List<Vector3> placed)
    {
        for (int attempt = 0; attempt < 6; attempt++)
        {
            float ox = (float)(rng.NextDouble() * 28.0 - 14.0);
            float oz = (float)(rng.NextDouble() * 28.0 - 14.0);
            var candidate = center + new Vector3(ox, Level8ChunkPlaceholderBuilder.FloorTop, oz);

            bool tooClose = false;
            for (int i = 0; i < placed.Count; i++)
            {
                Vector3 flatA = new Vector3(candidate.x, 0f, candidate.z);
                Vector3 flatB = new Vector3(placed[i].x, 0f, placed[i].z);
                if (Vector3.Distance(flatA, flatB) < Level8VisualHierarchyFlags.MinDecorationDistance)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
            {
                return candidate;
            }
        }

        float fx = (float)(rng.NextDouble() * 20.0 - 10.0);
        float fz = (float)(rng.NextDouble() * 20.0 - 10.0);
        return center + new Vector3(fx, Level8ChunkPlaceholderBuilder.FloorTop, fz);
    }

    public static void CreateTransitionDecoration(
        Transform parent,
        Vector3 position,
        Level8BiomeProfile biome,
        System.Random rng,
        string name)
    {
        switch (biome.BiomeKind)
        {
            case Level8BiomeKind.Forest:
                CreateTreeCluster(parent, position, biome, rng, name);
                break;
            case Level8BiomeKind.Swamp:
                CreateMudPool(parent, position, biome, name);
                break;
            case Level8BiomeKind.DataWilderness:
                if (rng.NextDouble() < biome.DataCorruptionDensity)
                {
                    CreateGlowCube(parent, position, biome.HazardColor, $"{name}_Glow");
                }
                else
                {
                    CreateRuinFragment(parent, position, name);
                }
                break;
            case Level8BiomeKind.Hill:
            case Level8BiomeKind.MountainEdge:
                Level8TerrainVisualBuilder.CreateRockCluster(parent, position, biome.ToVisualProfile(), rng, name);
                break;
            default:
                if (rng.NextDouble() < 0.5)
                {
                    Level8TerrainVisualBuilder.CreateGrassPatch(parent, position, biome.ToVisualProfile(), 2.5f, name);
                }
                else
                {
                    Level8TerrainVisualBuilder.CreateRockCluster(parent, position, biome.ToVisualProfile(), rng, name);
                }
                break;
        }
    }

    public static void BuildHazardVisuals(
        Transform props,
        Transform decorations,
        Vector3 center,
        Level8BiomeProfile biome)
    {
        float size = Level8GenerationFlags.ChunkSize * 0.40f * biome.HazardSizeMultiplier;
        Color hazardColor = biome.HazardColor;

        switch (biome.PreferredHazardKind)
        {
            case Level8HazardKind.PoisonSwampPatch:
            case Level8HazardKind.MudSlowZone:
                CreateMudPool(props, center, biome, "HazardPool");
                hazardColor = new Color(0.18f, 0.28f, 0.16f, 0.55f);
                break;
            case Level8HazardKind.RockFallZone:
                CreateWarningRocks(decorations, center, biome);
                hazardColor = new Color(0.65f, 0.30f, 0.15f, 0.40f);
                break;
            case Level8HazardKind.ThornField:
                CreateThornBushes(decorations, center, biome);
                hazardColor = new Color(0.55f, 0.65f, 0.18f, 0.38f);
                break;
            case Level8HazardKind.DataGlitchZone:
            case Level8HazardKind.CorruptionZone:
                CreateCorruptionPatch(decorations, center, biome);
                hazardColor = biome.HazardColor;
                break;
        }

        CreateHazardZone(props, center, new Vector3(size, 0.06f, size), hazardColor);
    }

    static void DecoratePlain(Transform parent, Vector3 pos, Level8BiomeProfile biome, System.Random rng, int index)
    {
        if (index % 2 == 0)
        {
            Level8TerrainVisualBuilder.CreateGrassPatch(parent, pos, biome.ToVisualProfile(), 2f, $"PlainGrass_{index}");
        }
        else
        {
            Level8TerrainVisualBuilder.CreateRockCluster(parent, pos + Vector3.up * 0.2f, biome.ToVisualProfile(), rng, $"PlainRock_{index}");
        }
    }

    static void DecorateGrassland(Transform parent, Vector3 pos, Level8BiomeProfile biome, System.Random rng, int index)
    {
        Level8TerrainVisualBuilder.CreateGrassPatch(parent, pos, biome.ToVisualProfile(), 2.5f + index * 0.2f, $"GrasslandPatch_{index}");
        if (rng.NextDouble() < 0.35)
        {
            CreateFlowerMarker(parent, pos + new Vector3(0.5f, 0.08f, 0.5f), rng);
        }
    }

    static void DecorateHill(Transform parent, Vector3 pos, Level8BiomeProfile biome, System.Random rng, int index)
    {
        if (rng.NextDouble() < biome.HillFrequency)
        {
            CreateMound(parent, pos, biome, rng, index);
        }
        else
        {
            Level8TerrainVisualBuilder.CreateRockCluster(parent, pos + Vector3.up * 0.25f, biome.ToVisualProfile(), rng, $"HillRock_{index}");
        }
    }

    static void DecorateSwamp(Transform parent, Vector3 pos, Level8BiomeProfile biome, System.Random rng, int index)
    {
        if (rng.NextDouble() < 0.55)
        {
            CreateMudPool(parent, pos, biome, $"SwampPool_{index}");
        }
        else
        {
            CreateVineMarker(parent, pos + Vector3.up * 0.3f);
        }
    }

    static void DecorateForest(Transform parent, Vector3 pos, Level8BiomeProfile biome, System.Random rng, int index)
    {
        if (rng.NextDouble() < biome.TreeDensity)
        {
            CreateTreeCluster(parent, pos, biome, rng, $"ForestTree_{index}");
        }
        else if (rng.NextDouble() < 0.5)
        {
            CreateFallenLog(parent, pos);
        }
        else
        {
            CreateBush(parent, pos, biome);
        }
    }

    static void DecorateDataWilderness(Transform parent, Vector3 pos, Level8BiomeProfile biome, System.Random rng, int index)
    {
        if (rng.NextDouble() < biome.DataCorruptionDensity)
        {
            CreateGlowCube(parent, pos + Vector3.up * 0.15f, biome.HazardColor, $"CorruptGlow_{index}");
        }
        else if (rng.NextDouble() < biome.RuinDensity)
        {
            CreateRuinFragment(parent, pos, $"Ruin_{index}");
        }
        else
        {
            Level8TerrainVisualBuilder.CreateRockCluster(parent, pos + Vector3.up * 0.2f, biome.ToVisualProfile(), rng, $"WildRock_{index}");
        }
    }

    static void CreateTreeCluster(Transform parent, Vector3 center, Level8BiomeProfile biome, System.Random rng, string name)
    {
        int count = 2 + rng.Next(2);
        for (int i = 0; i < count; i++)
        {
            float ox = (float)(rng.NextDouble() * 2.0 - 1.0);
            float oz = (float)(rng.NextDouble() * 2.0 - 1.0);
            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = $"{name}_Trunk{i}";
            trunk.transform.SetParent(parent, false);
            trunk.transform.position = center + new Vector3(ox, 0.9f, oz);
            trunk.transform.localScale = new Vector3(0.35f, 0.9f, 0.35f);
            Object.Destroy(trunk.GetComponent<Collider>());
            Level8VisualMaterialUtility.ApplyColor(trunk.GetComponent<Renderer>(), new Color(0.35f, 0.25f, 0.15f));

            var canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            canopy.name = $"{name}_Canopy{i}";
            canopy.transform.SetParent(parent, false);
            canopy.transform.position = center + new Vector3(ox, 1.8f, oz);
            canopy.transform.localScale = Vector3.one * 1.4f;
            Object.Destroy(canopy.GetComponent<Collider>());
            Level8VisualMaterialUtility.ApplyColor(canopy.GetComponent<Renderer>(), new Color(0.18f, 0.42f, 0.22f));
        }
    }

    static void CreateFallenLog(Transform parent, Vector3 center)
    {
        var log = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        log.name = "FallenLog";
        log.transform.SetParent(parent, false);
        log.transform.position = center + new Vector3(0f, 0.15f, 0f);
        log.transform.localScale = new Vector3(0.4f, 0.15f, 1.8f);
        log.transform.rotation = Quaternion.Euler(0f, 35f, 90f);
        Object.Destroy(log.GetComponent<Collider>());
        Level8VisualMaterialUtility.ApplyColor(log.GetComponent<Renderer>(), new Color(0.38f, 0.28f, 0.18f));
    }

    static void CreateBush(Transform parent, Vector3 center, Level8BiomeProfile biome)
    {
        var bush = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bush.name = "Bush";
        bush.transform.SetParent(parent, false);
        bush.transform.position = center + new Vector3(0f, 0.35f, 0f);
        bush.transform.localScale = new Vector3(1.1f, 0.7f, 1.1f);
        Object.Destroy(bush.GetComponent<Collider>());
        Level8VisualMaterialUtility.ApplyColor(bush.GetComponent<Renderer>(), biome.ChunkTintColor);
    }

    static void CreateMound(Transform parent, Vector3 center, Level8BiomeProfile biome, System.Random rng, int index)
    {
        float h = 0.5f + (float)rng.NextDouble() * biome.HeightVariationAmplitude;
        var mound = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        mound.name = $"Mound_{index}";
        mound.transform.SetParent(parent, false);
        mound.transform.position = center + new Vector3(0f, h * 0.5f, 0f);
        mound.transform.localScale = new Vector3(2.5f, h * 0.5f, 2.5f);
        Object.Destroy(mound.GetComponent<Collider>());
        Level8VisualMaterialUtility.ApplyColor(mound.GetComponent<Renderer>(), Color.Lerp(biome.BaseGroundColor, biome.ChunkTintColor, 0.4f));
    }

    static void CreateMudPool(Transform parent, Vector3 center, Level8BiomeProfile biome, string name)
    {
        var pool = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pool.name = name;
        pool.transform.SetParent(parent, false);
        pool.transform.position = center + new Vector3(0f, 0.02f, 0f);
        pool.transform.localScale = new Vector3(4f, 0.03f, 4f);
        Object.Destroy(pool.GetComponent<Collider>());
        Color mud = new Color(0.22f, 0.28f, 0.18f, 0.65f);
        Level8VisualMaterialUtility.ApplyColor(pool.GetComponent<Renderer>(), mud, transparent: true);
    }

    static void CreateVineMarker(Transform parent, Vector3 center)
    {
        var vine = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        vine.name = "VineMarker";
        vine.transform.SetParent(parent, false);
        vine.transform.position = center;
        vine.transform.localScale = new Vector3(0.12f, 0.8f, 0.12f);
        Object.Destroy(vine.GetComponent<Collider>());
        Level8VisualMaterialUtility.ApplyColor(vine.GetComponent<Renderer>(), new Color(0.28f, 0.48f, 0.22f));
    }

    static void CreateGlowCube(Transform parent, Vector3 center, Color color, string name)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent, false);
        cube.transform.position = center;
        cube.transform.localScale = Vector3.one * 0.45f;
        Object.Destroy(cube.GetComponent<Collider>());
        Level8VisualMaterialUtility.ApplyColor(cube.GetComponent<Renderer>(), color);
    }

    static void CreateRuinFragment(Transform parent, Vector3 center, string name)
    {
        var ruin = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ruin.name = name;
        ruin.transform.SetParent(parent, false);
        ruin.transform.position = center + new Vector3(0f, 0.25f, 0f);
        ruin.transform.localScale = new Vector3(0.7f, 0.5f, 0.5f);
        ruin.transform.rotation = Quaternion.Euler(0f, 20f, 10f);
        Object.Destroy(ruin.GetComponent<Collider>());
        Level8VisualMaterialUtility.ApplyColor(ruin.GetComponent<Renderer>(), new Color(0.38f, 0.42f, 0.48f));
    }

    static void CreateFlowerMarker(Transform parent, Vector3 center, System.Random rng)
    {
        var flower = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flower.name = "FlowerMarker";
        flower.transform.SetParent(parent, false);
        flower.transform.position = center;
        flower.transform.localScale = Vector3.one * 0.25f;
        Object.Destroy(flower.GetComponent<Collider>());
        Color[] colors =
        {
            new Color(0.95f, 0.45f, 0.55f),
            new Color(0.95f, 0.85f, 0.25f),
            new Color(0.65f, 0.45f, 0.95f),
        };
        Level8VisualMaterialUtility.ApplyColor(flower.GetComponent<Renderer>(), colors[rng.Next(colors.Length)]);
    }

    static void CreateWarningRocks(Transform parent, Vector3 center, Level8BiomeProfile biome)
    {
        for (int i = 0; i < 3; i++)
        {
            var rock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rock.name = $"WarningRock_{i}";
            rock.transform.SetParent(parent, false);
            rock.transform.position = center + new Vector3(-6f + i * 6f, 0.5f, 4f);
            rock.transform.localScale = new Vector3(0.8f, 0.6f, 0.7f);
            rock.transform.rotation = Quaternion.Euler(0f, i * 15f, 10f);
            Object.Destroy(rock.GetComponent<Collider>());
            Level8VisualMaterialUtility.ApplyColor(rock.GetComponent<Renderer>(), biome.ToVisualProfile().RockColor);
        }
    }

    static void CreateThornBushes(Transform parent, Vector3 center, Level8BiomeProfile biome)
    {
        for (int i = 0; i < 4; i++)
        {
            var bush = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bush.name = $"ThornBush_{i}";
            bush.transform.SetParent(parent, false);
            float angle = i * 90f * Mathf.Deg2Rad;
            bush.transform.position = center + new Vector3(Mathf.Cos(angle) * 5f, 0.35f, Mathf.Sin(angle) * 5f);
            bush.transform.localScale = new Vector3(1f, 0.6f, 1f);
            Object.Destroy(bush.GetComponent<Collider>());
            Level8VisualMaterialUtility.ApplyColor(bush.GetComponent<Renderer>(), new Color(0.35f, 0.48f, 0.18f));
        }
    }

    static void CreateCorruptionPatch(Transform parent, Vector3 center, Level8BiomeProfile biome)
    {
        var patch = GameObject.CreatePrimitive(PrimitiveType.Cube);
        patch.name = "CorruptionPatch";
        patch.transform.SetParent(parent, false);
        patch.transform.position = center + new Vector3(0f, 0.03f, 0f);
        patch.transform.localScale = new Vector3(8f, 0.04f, 8f);
        Object.Destroy(patch.GetComponent<Collider>());
        Level8VisualMaterialUtility.ApplyColor(patch.GetComponent<Renderer>(), biome.HazardColor, transparent: true);
    }

    static void CreateHazardZone(Transform parent, Vector3 center, Vector3 scale, Color color)
    {
        var zone = GameObject.CreatePrimitive(PrimitiveType.Cube);
        zone.name = "DangerZone";
        zone.transform.SetParent(parent, false);
        zone.transform.position = center + new Vector3(0f, 0.04f, 0f);
        zone.transform.localScale = scale;
        zone.isStatic = true;
        Collider collider = zone.GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
        Level8VisualMaterialUtility.ApplyColor(zone.GetComponent<Renderer>(), color, transparent: true);
    }
}
