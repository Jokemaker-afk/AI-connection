using UnityEngine;

public static class MultiFloorLevelBuilder
{
    const float FloorHeight = 6f;
    const float FloorThickness = 0.25f;
    const float WallThickness = 0.35f;
    const int StepCountPerFlight = 18;
    const float StepDepth = 0.34f;
    const float StairWidth = 4.5f;

    public static GameObject Generate(bool removeOldBuilding = true)
    {
        if (removeOldBuilding)
        {
            DestroyIfExists("Building");
            DestroyIfExists("Building_Large");
        }

        var root = new GameObject("Building_Large");
        var floorsRoot = CreateChild(root.transform, "Floors");
        var wallsRoot = CreateChild(root.transform, "Walls");
        var stairsRoot = CreateChild(root.transform, "Stairs");
        var propsRoot = CreateChild(root.transform, "Props");

        float width = 40f;
        float depth = 32f;

        var floorColor = new Color(0.52f, 0.52f, 0.55f);
        var wallColor = new Color(0.82f, 0.8f, 0.74f);
        var stairColor = new Color(0.62f, 0.6f, 0.58f);
        var railColor = new Color(0.35f, 0.35f, 0.38f);
        var propColor = new Color(0.45f, 0.3f, 0.18f);

        float stairRun = StepCountPerFlight * StepDepth;
        float openingW = StairWidth + 2f;
        float openingD = stairRun * 2f + 4.5f;
        float stairX = 12f;
        float stairZ = 0f;

        for (int floor = 0; floor < 3; floor++)
        {
            float y = floor * FloorHeight;
            CreateFloorWithOpening(floorsRoot, $"Floor_{floor + 1}", y, width, depth, stairX, stairZ, openingW, openingD, floorColor);
            CreatePerimeterWalls(wallsRoot, $"Walls_F{floor + 1}", y, width, depth, FloorHeight, wallColor);
            CreateInteriorWalls(wallsRoot, $"Interior_F{floor + 1}", y, floor, stairX, stairZ, openingW, openingD, wallColor);
        }

        float southStartZ = stairZ - openingD * 0.5f + 1.1f;
        float northStartZ = stairZ + openingD * 0.5f - 1.1f - StepDepth;

        CreateStairFlight(
            stairsRoot,
            "Stair_F1_F2_Up",
            0f,
            FloorHeight,
            stairX,
            southStartZ,
            StairWidth,
            StepCountPerFlight,
            +1,
            stairColor,
            railColor);

        CreateLanding(
            stairsRoot,
            "Landing_F2",
            FloorHeight,
            stairX,
            northStartZ,
            StairWidth + 1.2f,
            2.8f,
            stairColor);

        CreateStairFlight(
            stairsRoot,
            "Stair_F2_F3_Down",
            FloorHeight,
            FloorHeight * 2f,
            stairX,
            northStartZ,
            StairWidth,
            StepCountPerFlight,
            -1,
            stairColor,
            railColor);

        CreateLanding(
            stairsRoot,
            "Landing_F3",
            FloorHeight * 2f,
            stairX,
            southStartZ,
            StairWidth + 1.2f,
            2.8f,
            stairColor);

        CreateProps(propsRoot, propColor, railColor);

        var spawn = GameObject.Find("Player");
        if (spawn != null)
        {
            spawn.transform.position = new Vector3(-10f, 1f, -10f);
            spawn.transform.rotation = Quaternion.identity;
        }

        return root;
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

    static void CreateFloorWithOpening(
        Transform parent,
        string name,
        float y,
        float width,
        float depth,
        float openingX,
        float openingZ,
        float openingW,
        float openingD,
        Color color)
    {
        var floorRoot = CreateChild(parent, name);

        float halfW = width * 0.5f;
        float halfD = depth * 0.5f;
        float openHalfW = openingW * 0.5f;
        float openHalfD = openingD * 0.5f;

        float leftWidth = (openingX - openHalfW) - (-halfW);
        if (leftWidth > 0.5f)
        {
            CreateBlock(
                floorRoot,
                "Section_Left",
                new Vector3((-halfW + openingX - openHalfW) * 0.5f, y, 0f),
                new Vector3(leftWidth, FloorThickness, depth),
                color);
        }

        float rightWidth = halfW - (openingX + openHalfW);
        if (rightWidth > 0.5f)
        {
            CreateBlock(
                floorRoot,
                "Section_Right",
                new Vector3((openingX + openHalfW + halfW) * 0.5f, y, 0f),
                new Vector3(rightWidth, FloorThickness, depth),
                color);
        }

        float frontDepth = (openingZ - openHalfD) - (-halfD);
        if (frontDepth > 0.5f)
        {
            CreateBlock(
                floorRoot,
                "Section_Front",
                new Vector3(openingX, y, (-halfD + openingZ - openHalfD) * 0.5f),
                new Vector3(openingW, FloorThickness, frontDepth),
                color);
        }

        float backDepth = halfD - (openingZ + openHalfD);
        if (backDepth > 0.5f)
        {
            CreateBlock(
                floorRoot,
                "Section_Back",
                new Vector3(openingX, y, (openingZ + openHalfD + halfD) * 0.5f),
                new Vector3(openingW, FloorThickness, backDepth),
                color);
        }
    }

    static void CreatePerimeterWalls(
        Transform parent,
        string name,
        float baseY,
        float width,
        float depth,
        float height,
        Color color)
    {
        var wallRoot = CreateChild(parent, name);
        float halfW = width * 0.5f;
        float halfD = depth * 0.5f;
        float centerY = baseY + height * 0.5f;

        CreateBlock(wallRoot, "North", new Vector3(0f, centerY, halfD), new Vector3(width, height, WallThickness), color);
        CreateBlock(wallRoot, "South", new Vector3(0f, centerY, -halfD), new Vector3(width, height, WallThickness), color);
        CreateBlock(wallRoot, "East", new Vector3(halfW, centerY, 0f), new Vector3(WallThickness, height, depth), color);
        CreateBlock(wallRoot, "West", new Vector3(-halfW, centerY, 0f), new Vector3(WallThickness, height, depth), color);
    }

    static void CreateInteriorWalls(
        Transform parent,
        string name,
        float baseY,
        int floorIndex,
        float stairX,
        float stairZ,
        float openingW,
        float openingD,
        Color color)
    {
        var interiorRoot = CreateChild(parent, name);
        float centerY = baseY + FloorHeight * 0.5f;

        if (floorIndex == 0)
        {
            CreateBlock(interiorRoot, "Lobby_Divider", new Vector3(-6f, centerY, 0f), new Vector3(0.25f, FloorHeight, 18f), color);
        }
        else if (floorIndex == 1)
        {
            CreateBlock(interiorRoot, "Office_A", new Vector3(-10f, centerY, 7f), new Vector3(10f, FloorHeight, 0.25f), color);
            CreateBlock(interiorRoot, "Office_B", new Vector3(-14f, centerY, -2f), new Vector3(0.25f, FloorHeight, 12f), color);
        }
        else
        {
            CreateBlock(interiorRoot, "Roof_Divider_A", new Vector3(-8f, centerY, -5f), new Vector3(14f, FloorHeight, 0.25f), color);
            CreateBlock(interiorRoot, "Roof_Divider_B", new Vector3(0f, centerY, 8f), new Vector3(0.25f, FloorHeight, 12f), color);
        }
    }

    static void CreateStairFlight(
        Transform parent,
        string name,
        float startY,
        float endY,
        float startX,
        float startZ,
        float width,
        int stepCount,
        int zDirection,
        Color stepColor,
        Color railColor)
    {
        var flightRoot = CreateChild(parent, name);
        float totalRise = endY - startY;
        float stepHeight = totalRise / stepCount;

        for (int i = 0; i < stepCount; i++)
        {
            float y = startY + stepHeight * i + stepHeight * 0.5f;
            float z = startZ + StepDepth * i * zDirection + StepDepth * 0.5f * zDirection;
            CreateBlock(
                flightRoot,
                $"Step_{i + 1}",
                new Vector3(startX, y, z),
                new Vector3(width, stepHeight * 0.96f, StepDepth * 0.96f),
                stepColor);
        }

        float railHeight = 0.8f;
        float midY = startY + totalRise * 0.5f + 0.35f;
        float midZ = startZ + StepDepth * stepCount * 0.5f * zDirection;
        float railLength = StepDepth * stepCount;
        CreateBlock(flightRoot, "Rail_Left", new Vector3(startX - width * 0.5f - 0.04f, midY, midZ), new Vector3(0.05f, railHeight, railLength), railColor);
        CreateBlock(flightRoot, "Rail_Right", new Vector3(startX + width * 0.5f + 0.04f, midY, midZ), new Vector3(0.05f, railHeight, railLength), railColor);
    }

    static void CreateLanding(
        Transform parent,
        string name,
        float y,
        float x,
        float z,
        float width,
        float depth,
        Color color)
    {
        CreateBlock(parent, name, new Vector3(x, y + FloorThickness * 0.5f, z), new Vector3(width, FloorThickness, depth), color);
    }

    static void CreateProps(Transform parent, Color wood, Color metal)
    {
        CreateBlock(parent, "Desk_F1", new Vector3(-12f, 0.45f, 9f), new Vector3(2.4f, 0.9f, 1.1f), wood);
        CreateBlock(parent, "Desk_F2", new Vector3(-12f, FloorHeight + 0.45f, 9f), new Vector3(2.4f, 0.9f, 1.1f), wood);
        CreateBlock(parent, "Sofa_F2", new Vector3(-4f, FloorHeight + 0.35f, 9f), new Vector3(2.8f, 0.7f, 1.2f), new Color(0.25f, 0.35f, 0.65f));
        CreateBlock(parent, "Table_F3", new Vector3(-4f, FloorHeight * 2f + 0.45f, 7f), new Vector3(3f, 0.9f, 1.4f), wood);
        CreateBlock(parent, "Column_1", new Vector3(-16f, FloorHeight * 1.5f, -12f), new Vector3(0.8f, FloorHeight * 3f, 0.8f), metal);
        CreateBlock(parent, "Column_2", new Vector3(16f, FloorHeight * 1.5f, 12f), new Vector3(0.8f, FloorHeight * 3f, 0.8f), metal);
    }

    static GameObject CreateBlock(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent, false);
        cube.transform.position = position;
        cube.transform.localScale = scale;
        cube.isStatic = true;

        var renderer = cube.GetComponent<Renderer>();
        if (renderer != null)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = color;
            renderer.sharedMaterial = material;
        }

        return cube;
    }
}
