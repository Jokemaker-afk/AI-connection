using UnityEngine;

public static class BuildingLightingBuilder
{
    const float CeilingFixtureHeight = 0.08f;
    const float CeilingInset = 0.015f;

    static readonly Vector2[] MainLightGrid =
    {
        new Vector2(-14f, -10f),
        new Vector2(-14f, 0f),
        new Vector2(-14f, 10f),
        new Vector2(-4f, -10f),
        new Vector2(-4f, 0f),
        new Vector2(-4f, 10f),
    };

    static readonly Vector2[] EastLightGrid =
    {
        new Vector2(17f, -11f),
        new Vector2(17f, 11f),
    };

    public static GameObject Generate(bool removeOld = true)
    {
        Transform buildingRoot = null;
        var building = GameObject.Find("Building_Large");
        if (building != null)
        {
            buildingRoot = building.transform;
        }

        if (removeOld)
        {
            if (buildingRoot != null)
            {
                var existing = buildingRoot.Find("Lights");
                if (existing != null)
                {
                    Object.DestroyImmediate(existing.gameObject);
                }
            }

            DestroyIfExists("BuildingLights");
        }

        var root = new GameObject("Lights");
        if (buildingRoot != null)
        {
            root.transform.SetParent(buildingRoot, false);
        }

        int floorCount = BuildingGeometryProbe.GetFloorCount();
        for (int floor = 1; floor <= floorCount; floor++)
        {
            var floorRoot = CreateChild(root.transform, $"Floor_{floor}");
            PlaceFloorLights(floorRoot, floor, floorCount);
        }

        ConfigureAmbientLight();
        return root;
    }

    static void PlaceFloorLights(Transform floorRoot, int floorNumber, int floorCount)
    {
        var warmWhite = new Color(1f, 0.94f, 0.82f);

        foreach (var grid in MainLightGrid)
        {
            if (!TryGetCeilingMountY(floorNumber, grid.x, grid.y, out float mountY))
            {
                continue;
            }

            CreateCeilingLight(
                floorRoot,
                $"Ceiling_{grid.x:0}_{grid.y:0}",
                new Vector3(grid.x, mountY, grid.y),
                warmWhite,
                intensity: 3.2f,
                range: 13f);
        }

        foreach (var grid in EastLightGrid)
        {
            if (floorNumber > 1 && BuildingGeometryProbe.IsInStairOpening(grid.x, grid.y))
            {
                continue;
            }

            if (!TryGetCeilingMountY(floorNumber, grid.x, grid.y, out float mountY))
            {
                continue;
            }

            CreateCeilingLight(
                floorRoot,
                $"Ceiling_E_{grid.x:0}_{grid.y:0}",
                new Vector3(grid.x, mountY, grid.y),
                warmWhite,
                intensity: 3f,
                range: 12f);
        }

        if (floorNumber < floorCount
            && BuildingGeometryProbe.TryGetFloorSurfaceAt(floorNumber, BuildingGeometryProbe.StairX - 3.2f, BuildingGeometryProbe.StairZ - 4f, out float floorTop)
            && TryGetCeilingSurfaceY(floorNumber, BuildingGeometryProbe.StairX - 3.2f, BuildingGeometryProbe.StairZ - 4f, out float ceilingTop))
        {
            float wallLightY = floorTop + (ceilingTop - floorTop) * 0.72f;
            CreateWallLight(
                floorRoot,
                "Stair_Light_A",
                new Vector3(BuildingGeometryProbe.StairX - 3.2f, wallLightY, BuildingGeometryProbe.StairZ - 4f),
                warmWhite * 0.95f,
                intensity: 2.4f,
                range: 9f);
        }
    }

    static bool TryGetCeilingMountY(int floorNumber, float x, float z, out float mountY)
    {
        mountY = 0f;
        if (!TryGetCeilingSurfaceY(floorNumber, x, z, out float ceilingSurface))
        {
            return false;
        }

        mountY = ceilingSurface - CeilingFixtureHeight * 0.5f - CeilingInset;
        return true;
    }

    static bool TryGetCeilingSurfaceY(int floorNumber, float x, float z, out float ceilingSurface)
    {
        if (floorNumber > 1 && BuildingGeometryProbe.IsInStairOpening(x, z))
        {
            ceilingSurface = 0f;
            return false;
        }

        return BuildingGeometryProbe.TryGetCeilingSurfaceAt(floorNumber, x, z, out ceilingSurface);
    }

    static void CreateCeilingLight(
        Transform parent,
        string name,
        Vector3 position,
        Color color,
        float intensity,
        float range)
    {
        var fixtureRoot = new GameObject(name);
        fixtureRoot.transform.SetParent(parent, false);
        fixtureRoot.transform.position = position;

        CreateFixtureMesh(fixtureRoot.transform, color, intensity);

        var lightGo = new GameObject("Light");
        lightGo.transform.SetParent(fixtureRoot.transform, false);
        lightGo.transform.localPosition = Vector3.down * (CeilingFixtureHeight * 0.5f + 0.02f);

        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.shadows = LightShadows.None;
    }

    static void CreateWallLight(
        Transform parent,
        string name,
        Vector3 position,
        Color color,
        float intensity,
        float range)
    {
        var fixtureRoot = new GameObject(name);
        fixtureRoot.transform.SetParent(parent, false);
        fixtureRoot.transform.position = position;

        CreateFixtureMesh(fixtureRoot.transform, color, intensity * 0.85f, new Vector3(0.35f, 0.12f, 0.35f));

        var lightGo = new GameObject("Light");
        lightGo.transform.SetParent(fixtureRoot.transform, false);
        lightGo.transform.localPosition = Vector3.zero;

        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.shadows = LightShadows.None;
    }

    static void CreateFixtureMesh(Transform parent, Color color, float intensity, Vector3? scale = null)
    {
        var mesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mesh.name = "Fixture";
        mesh.transform.SetParent(parent, false);
        mesh.transform.localPosition = Vector3.zero;
        mesh.transform.localScale = scale ?? new Vector3(0.9f, CeilingFixtureHeight, 0.9f);
        mesh.isStatic = true;
        Object.DestroyImmediate(mesh.GetComponent<Collider>());

        var renderer = mesh.GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.color = color * 0.55f;
        material.SetColor("_EmissionColor", color * Mathf.Clamp(intensity * 0.18f, 0.35f, 1.2f));
        material.EnableKeyword("_EMISSION");
        renderer.sharedMaterial = material;
    }

    static void ConfigureAmbientLight()
    {
        var sun = GameObject.Find("Directional Light");
        if (sun == null)
        {
            return;
        }

        var directional = sun.GetComponent<Light>();
        if (directional == null)
        {
            return;
        }

        directional.intensity = 0.35f;
        directional.color = new Color(0.82f, 0.88f, 1f);
        directional.shadows = LightShadows.Soft;
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
