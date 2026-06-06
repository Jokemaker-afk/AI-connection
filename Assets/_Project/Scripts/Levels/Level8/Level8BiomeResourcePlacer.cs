using System.Collections.Generic;

using UnityEngine;



/// <summary>Biome-weighted resource placement for Level8 resource chunks.</summary>

public static class Level8BiomeResourcePlacer

{

    struct WeightedItem

    {

        public ItemKind Kind;

        public float Weight;

    }



    public static void SpawnBiomeResources(Transform props, Vector3 center, Level8BiomeProfile biome, System.Random rng)

    {

        Transform resourcesRoot = Level8ArenaBuildContext.Roots?.Resources ?? props;

        var candidates = BuildCandidates(biome);

        int nodeCount = biome.MinResourceNodes + rng.Next(Mathf.Max(1, biome.MaxResourceNodes - biome.MinResourceNodes + 1));

        nodeCount = Mathf.Min(nodeCount, Level8VisualHierarchyFlags.MaxResourcePickupsPerChunk);



        var usedOffsets = new List<Vector3>();

        Level8BiomeKind previousBiome = Level8ResourceSpawnContext.CurrentBiome;
        Level8ResourceSpawnContext.CurrentBiome = biome.BiomeKind;

        try
        {
            for (int i = 0; i < nodeCount; i++)
            {
                ItemKind kind = PickWeighted(candidates, rng);
                Vector3 offset = PickOffset(rng, usedOffsets);
                usedOffsets.Add(offset);

                int amount = 1 + rng.Next(3);
                GameObject pickup = ItemModuleFactory.SpawnWorldPickup(kind, center + offset, amount, resourcesRoot);
                EnhancePickup(pickup, kind, biome.BiomeKind);
            }
        }
        finally
        {
            Level8ResourceSpawnContext.CurrentBiome = previousBiome;
        }

    }



    static void EnhancePickup(GameObject pickup, ItemKind kind, Level8BiomeKind biomeKind)
    {
        if (pickup == null)
        {
            return;
        }

        var enhancer = pickup.GetComponent<Level8ResourcePickupEnhancer>();
        if (enhancer == null)
        {
            enhancer = pickup.AddComponent<Level8ResourcePickupEnhancer>();
        }

        enhancer.Configure(kind, biomeKind);
    }



    static List<WeightedItem> BuildCandidates(Level8BiomeProfile biome)

    {

        var list = new List<WeightedItem>();

        AddIfPositive(list, ItemKind.Wood, biome.WoodWeight);

        AddIfPositive(list, ItemKind.Stone, biome.StoneWeight);

        AddIfPositive(list, ItemKind.Fiber, biome.FiberWeight);

        AddIfPositive(list, ItemKind.Vine, biome.VineWeight);

        AddIfPositive(list, ItemKind.Berry, biome.BerryWeight);

        AddIfPositive(list, ItemKind.OreFragment, biome.OreWeight);

        AddIfPositive(list, ItemKind.Coal, biome.CoalWeight);

        AddIfPositive(list, ItemKind.Grass, biome.FiberWeight * 0.5f);



        if (list.Count == 0)

        {

            list.Add(new WeightedItem { Kind = ItemKind.Wood, Weight = 1f });

            list.Add(new WeightedItem { Kind = ItemKind.Fiber, Weight = 1f });

        }



        return list;

    }



    static void AddIfPositive(List<WeightedItem> list, ItemKind kind, float weight)

    {

        if (weight > 0.01f)

        {

            list.Add(new WeightedItem { Kind = kind, Weight = weight });

        }

    }



    static ItemKind PickWeighted(List<WeightedItem> items, System.Random rng)

    {

        float total = 0f;

        for (int i = 0; i < items.Count; i++)

        {

            total += items[i].Weight;

        }



        float roll = (float)rng.NextDouble() * total;

        float acc = 0f;

        for (int i = 0; i < items.Count; i++)

        {

            acc += items[i].Weight;

            if (roll <= acc)

            {

                return items[i].Kind;

            }

        }



        return items[items.Count - 1].Kind;

    }



    static Vector3 PickOffset(System.Random rng, List<Vector3> used)

    {

        for (int attempt = 0; attempt < 8; attempt++)

        {

            float x = (float)(rng.NextDouble() * 24.0 - 12.0);

            float z = (float)(rng.NextDouble() * 24.0 - 12.0);

            var offset = new Vector3(x, Level8ChunkPlaceholderBuilder.FloorTop + 0.55f, z);



            bool tooClose = false;

            for (int i = 0; i < used.Count; i++)

            {

                if (Vector3.Distance(new Vector3(offset.x, 0f, offset.z), new Vector3(used[i].x, 0f, used[i].z)) < 4f)

                {

                    tooClose = true;

                    break;

                }

            }



            if (!tooClose)

            {

                return offset;

            }

        }



        return new Vector3(0f, Level8ChunkPlaceholderBuilder.FloorTop + 0.55f, 0f);

    }

}


