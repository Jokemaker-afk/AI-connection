using UnityEngine;

public static class BuildingGeometryProbe
{
    const float FloorHeightFallback = 6f;
    const float FloorThicknessFallback = 0.25f;
    const float StairWidth = 4.5f;
    const float StepDepth = 0.34f;
    const int StepCountPerFlight = 18;
    public const float StairX = 12f;
    public const float StairZ = 0f;

    public static int GetFloorCount()
    {
        var building = GameObject.Find("Building_Large");
        if (building == null)
        {
            return 3;
        }

        var floorsRoot = building.transform.Find("Floors");
        if (floorsRoot == null || floorsRoot.childCount == 0)
        {
            return 3;
        }

        return floorsRoot.childCount;
    }

    public static float GetFloorTopY(int floorNumber)
    {
        GetFloorBoundsY(floorNumber, out float top, out _);
        if (top > float.MinValue)
        {
            return top;
        }

        return (floorNumber - 1) * FloorHeightFallback + FloorThicknessFallback * 0.5f;
    }

    public static bool TryGetCeilingSurfaceAt(int floorNumber, float x, float z, out float ceilingY)
    {
        ceilingY = GetWallTopY(floorNumber);

        if (floorNumber < GetFloorCount())
        {
            var upperFloorRoot = FindFloorTransform(floorNumber + 1);
            if (upperFloorRoot != null)
            {
                bool found = false;
                float bestBottom = float.MaxValue;
                for (int i = 0; i < upperFloorRoot.childCount; i++)
                {
                    var section = upperFloorRoot.GetChild(i);
                    if (!ContainsXZ(section, x, z))
                    {
                        continue;
                    }

                    float bottom = section.position.y - section.localScale.y * 0.5f;
                    if (bottom < bestBottom)
                    {
                        bestBottom = bottom;
                        found = true;
                    }
                }

                if (found)
                {
                    ceilingY = bestBottom;
                    return true;
                }
            }
        }

        return ceilingY > float.MinValue;
    }

    public static bool TryGetFloorSurfaceAt(int floorNumber, float x, float z, out float surfaceY)
    {
        surfaceY = GetFloorTopY(floorNumber);
        return TryGetFloorSectionSurface(floorNumber, x, z, out surfaceY);
    }

    static bool TryGetFloorSectionSurface(int floorNumber, float x, float z, out float surfaceY)
    {
        surfaceY = float.MinValue;
        var floorRoot = FindFloorTransform(floorNumber);
        if (floorRoot == null)
        {
            return false;
        }

        bool found = false;
        for (int i = 0; i < floorRoot.childCount; i++)
        {
            var section = floorRoot.GetChild(i);
            if (!ContainsXZ(section, x, z))
            {
                continue;
            }

            float top = section.position.y + section.localScale.y * 0.5f;
            if (!found || top > surfaceY)
            {
                surfaceY = top;
                found = true;
            }
        }

        return found;
    }

    static bool ContainsXZ(Transform section, float x, float z)
    {
        const float margin = 0.2f;
        var center = section.position;
        float halfX = section.localScale.x * 0.5f + margin;
        float halfZ = section.localScale.z * 0.5f + margin;
        return x >= center.x - halfX
            && x <= center.x + halfX
            && z >= center.z - halfZ
            && z <= center.z + halfZ;
    }

    public static bool TrySampleGroundY(float x, float z, out float groundY)
    {
        return TryGetFloorSurfaceAt(1, x, z, out groundY);
    }

    public static bool TrySampleGroundY(float x, float z, int floorNumber, out float groundY)
    {
        return TryGetFloorSurfaceAt(floorNumber, x, z, out groundY);
    }

    public static float GetWallTopY(int floorNumber)
    {
        var wallsRoot = GameObject.Find($"Building_Large/Walls/Walls_F{floorNumber}");
        if (wallsRoot == null)
        {
            return float.MinValue;
        }

        float wallTop = float.MinValue;
        for (int i = 0; i < wallsRoot.transform.childCount; i++)
        {
            var wall = wallsRoot.transform.GetChild(i);
            wallTop = Mathf.Max(wallTop, wall.position.y + wall.localScale.y * 0.5f);
        }

        return wallTop;
    }

    public static bool HasFloorAt(int floorNumber, float x, float z)
    {
        if (IsInStairOpening(x, z) && floorNumber > 1)
        {
            return false;
        }

        if (floorNumber < 1 || floorNumber > GetFloorCount())
        {
            return false;
        }

        return TryGetFloorSectionSurface(floorNumber, x, z, out _);
    }

    public static bool IsInStairOpening(float x, float z)
    {
        float stairRun = StepCountPerFlight * StepDepth;
        float openingW = StairWidth + 2f;
        float openingD = stairRun * 2f + 4.5f;
        float openHalfW = openingW * 0.5f;
        float openHalfD = openingD * 0.5f;

        return x >= StairX - openHalfW
            && x <= StairX + openHalfW
            && z >= StairZ - openHalfD
            && z <= StairZ + openHalfD;
    }

    public static void GetFloorBoundsY(int floorNumber, out float top, out float bottom)
    {
        top = float.MinValue;
        bottom = float.MaxValue;

        var floorRoot = FindFloorTransform(floorNumber);
        if (floorRoot == null)
        {
            return;
        }

        for (int i = 0; i < floorRoot.childCount; i++)
        {
            var section = floorRoot.GetChild(i);
            float halfHeight = section.localScale.y * 0.5f;
            top = Mathf.Max(top, section.position.y + halfHeight);
            bottom = Mathf.Min(bottom, section.position.y - halfHeight);
        }
    }

    static Transform FindFloorTransform(int floorNumber)
    {
        var floorsRoot = GameObject.Find("Building_Large/Floors");
        if (floorsRoot == null)
        {
            return null;
        }

        return floorsRoot.transform.Find($"Floor_{floorNumber}");
    }
}
