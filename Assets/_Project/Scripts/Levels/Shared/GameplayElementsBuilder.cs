using UnityEngine;

public static class GameplayElementsBuilder
{
    public static GameObject Generate(bool removeOld = true)
    {
        if (removeOld)
        {
            DestroyIfExists("GameplayElements");
        }

        var root = new GameObject("GameplayElements");
        var rewardsRoot = CreateChild(root.transform, "Rewards");
        var hazardsRoot = CreateChild(root.transform, "Hazards");

        PlaceRewards(rewardsRoot);
        PlaceHazards(hazardsRoot);

        return root;
    }

    public static void RealignExistingElements()
    {
        var root = GameObject.Find("GameplayElements");
        if (root == null)
        {
            Generate(true);
            return;
        }

        RealignGroup(root.transform.Find("Rewards"));
        RealignGroup(root.transform.Find("Hazards"));
    }

    static void RealignGroup(Transform group)
    {
        if (group == null)
        {
            return;
        }

        for (int i = 0; i < group.childCount; i++)
        {
            var child = group.GetChild(i);
            int floorNumber = ParseFloorNumber(child.name);
            if (floorNumber <= 0)
            {
                continue;
            }

            if (!BuildingGeometryProbe.TryGetFloorSurfaceAt(floorNumber, child.position.x, child.position.z, out float groundY))
            {
                groundY = BuildingGeometryProbe.GetFloorTopY(floorNumber);
            }

            var pos = child.position;

            if (child.name.StartsWith("Crate_"))
            {
                pos.y = groundY + GetRewardHoverHeight(child.name);
            }
            else if (child.name.StartsWith("Laser_"))
            {
                pos.y = groundY + 0.06f;
            }
            else if (child.name.StartsWith("Spikes_"))
            {
                pos.y = groundY;
            }

            child.position = pos;
        }
    }

    static void PlaceRewards(Transform parent)
    {
        PlaceReward(parent, 1, "Crate_Bronze_F1_A", new Vector3(-8f, 2.2f, -2f), RewardTier.Bronze);
        PlaceReward(parent, 1, "Crate_Bronze_F1_B", new Vector3(-14f, 2.6f, 2f), RewardTier.Bronze);
        PlaceReward(parent, 1, "Crate_Silver_F1", new Vector3(-3f, 2.8f, -5f), RewardTier.Silver);
        PlaceReward(parent, 1, "Crate_Gold_F1", new Vector3(-10f, 3.4f, 4f), RewardTier.Gold);

        PlaceReward(parent, 2, "Crate_Bronze_F2_A", new Vector3(-6f, 2.1f, 6f), RewardTier.Bronze);
        PlaceReward(parent, 2, "Crate_Silver_F2", new Vector3(-14f, 2.5f, 4f), RewardTier.Silver);
        PlaceReward(parent, 2, "Crate_Gold_F2", new Vector3(-4f, 3.2f, 2f), RewardTier.Gold);
        PlaceReward(parent, 2, "Crate_Diamond_F2", new Vector3(-11f, 3.8f, -4f), RewardTier.Diamond);

        PlaceReward(parent, 3, "Crate_Silver_F3", new Vector3(-2f, 2.3f, 3f), RewardTier.Silver);
        PlaceReward(parent, 3, "Crate_Gold_F3", new Vector3(-7f, 3f, 6f), RewardTier.Gold);
        PlaceReward(parent, 3, "Crate_Diamond_F3", new Vector3(-5f, 3.6f, -2f), RewardTier.Diamond);
    }

    static void PlaceHazards(Transform parent)
    {
        var lasers = CreateChild(parent, "Lasers");
        var spikes = CreateChild(parent, "Spikes");

        PlaceLaser(lasers, 1, "Laser_F1_A", new Vector3(-5f, 0f, 0f), new Vector3(6f, 0.12f, 0.35f));
        PlaceLaser(lasers, 1, "Laser_F1_B", new Vector3(-14f, 0f, -8f), new Vector3(4f, 0.12f, 0.35f));
        PlaceLaser(lasers, 1, "Laser_F1_C", new Vector3(-8f, 0f, 8f), new Vector3(5f, 0.12f, 0.35f));

        PlaceLaser(lasers, 2, "Laser_F2_A", new Vector3(-6f, 0f, 3f), new Vector3(7f, 0.12f, 0.35f));
        PlaceLaser(lasers, 2, "Laser_F2_B", new Vector3(-15f, 0f, 0f), new Vector3(3f, 0.12f, 0.35f));

        PlaceLaser(lasers, 3, "Laser_F3", new Vector3(-3f, 0f, 6f), new Vector3(5f, 0.12f, 0.35f));

        PlaceSpikes(spikes, 1, "Spikes_F1_A", new Vector3(-7f, 0f, 2f), new Vector3(2.4f, 0.4f, 1.2f), 4);
        PlaceSpikes(spikes, 1, "Spikes_F1_B", new Vector3(-11f, 0f, -6f), new Vector3(1.8f, 0.4f, 1.8f), 3);
        PlaceSpikes(spikes, 1, "Spikes_F1_C", new Vector3(-2f, 0f, 6f), new Vector3(2f, 0.4f, 2f), 3);

        PlaceSpikes(spikes, 2, "Spikes_F2_A", new Vector3(-8f, 0f, -3f), new Vector3(2.6f, 0.4f, 1.4f), 4);
        PlaceSpikes(spikes, 2, "Spikes_F2_B", new Vector3(-3f, 0f, 7f), new Vector3(2f, 0.4f, 2f), 3);

        PlaceSpikes(spikes, 3, "Spikes_F3", new Vector3(-6f, 0f, 4f), new Vector3(2.4f, 0.4f, 1.6f), 4);
    }

    static void PlaceReward(Transform parent, int floorNumber, string name, Vector3 xzAndHover, RewardTier tier)
    {
        if (!BuildingGeometryProbe.HasFloorAt(floorNumber, xzAndHover.x, xzAndHover.z))
        {
            return;
        }

        if (!BuildingGeometryProbe.TryGetFloorSurfaceAt(floorNumber, xzAndHover.x, xzAndHover.z, out float groundY))
        {
            return;
        }

        var position = new Vector3(xzAndHover.x, groundY + xzAndHover.y, xzAndHover.z);
        RewardCrate.Create(parent, name, position, tier);
    }

    static void PlaceLaser(Transform parent, int floorNumber, string name, Vector3 xzPosition, Vector3 size)
    {
        if (!BuildingGeometryProbe.HasFloorAt(floorNumber, xzPosition.x, xzPosition.z))
        {
            return;
        }

        if (!BuildingGeometryProbe.TryGetFloorSurfaceAt(floorNumber, xzPosition.x, xzPosition.z, out float groundY))
        {
            return;
        }

        var position = new Vector3(xzPosition.x, groundY + size.y * 0.5f, xzPosition.z);
        LaserHazard.Create(parent, name, position, size);
    }

    static void PlaceSpikes(Transform parent, int floorNumber, string name, Vector3 xzPosition, Vector3 size, int spikeCount)
    {
        if (!BuildingGeometryProbe.HasFloorAt(floorNumber, xzPosition.x, xzPosition.z))
        {
            return;
        }

        if (!BuildingGeometryProbe.TryGetFloorSurfaceAt(floorNumber, xzPosition.x, xzPosition.z, out float groundY))
        {
            return;
        }

        var position = new Vector3(xzPosition.x, groundY, xzPosition.z);
        SpikeTrap.Create(parent, name, position, size, spikeCount);
    }

    static int ParseFloorNumber(string objectName)
    {
        int fIndex = objectName.IndexOf("_F", System.StringComparison.Ordinal);
        if (fIndex < 0)
        {
            return -1;
        }

        int start = fIndex + 2;
        int end = start;
        while (end < objectName.Length && char.IsDigit(objectName[end]))
        {
            end++;
        }

        if (end <= start)
        {
            return -1;
        }

        if (int.TryParse(objectName.Substring(start, end - start), out int floorNumber))
        {
            return floorNumber;
        }

        return -1;
    }

    static float GetRewardHoverHeight(string crateName)
    {
        if (crateName.Contains("_F1_") || crateName.EndsWith("_F1"))
        {
            if (crateName.Contains("Gold")) return 3.4f;
            if (crateName.Contains("Silver")) return 2.8f;
            return crateName.Contains("_B") ? 2.6f : 2.2f;
        }

        if (crateName.Contains("_F2") || crateName.Contains("_F2_"))
        {
            if (crateName.Contains("Diamond")) return 3.8f;
            if (crateName.Contains("Gold")) return 3.2f;
            if (crateName.Contains("Silver")) return 2.5f;
            return 2.1f;
        }

        if (crateName.Contains("_F3") || crateName.Contains("_F3_"))
        {
            if (crateName.Contains("Diamond")) return 3.6f;
            if (crateName.Contains("Gold")) return 3f;
            return 2.3f;
        }

        return 2.2f;
    }

    static void DestroyIfExists(string name)
    {
        var obj = GameObject.Find(name);
        if (obj != null)
        {
            Object.DestroyImmediate(obj);
        }
    }

    static Transform CreateChild(Transform parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }
}
