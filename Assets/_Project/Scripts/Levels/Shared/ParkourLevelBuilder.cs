using UnityEngine;

public static class ParkourLevelBuilder
{
    const float FloorTop = 0.125f;
    const float LaserHeightAboveFloor = 0.225f;

    public static GameObject Generate(bool removeOld = true)
    {
        if (removeOld)
        {
            DestroyIfExists("ParkourCourse");
        }

        var root = new GameObject("ParkourCourse");
        var platformsRoot = CreateChild(root.transform, "Platforms");
        var hazardsRoot = CreateChild(root.transform, "Hazards");
        var markersRoot = CreateChild(root.transform, "Markers");

        var platformColor = new Color(0.55f, 0.58f, 0.62f);
        var accentColor = new Color(0.35f, 0.72f, 0.95f);
        var beamColor = new Color(0.48f, 0.5f, 0.54f);

        CreateBlock(platformsRoot, "StartPad", new Vector3(0f, FloorTop, 0f), new Vector3(10f, 0.25f, 10f), platformColor);
        CreateBlock(platformsRoot, "Step_A", new Vector3(0f, FloorTop + 0.15f, 10f), new Vector3(4f, 0.25f, 4f), accentColor);
        CreateBlock(platformsRoot, "Step_B", new Vector3(0f, FloorTop + 0.35f, 16f), new Vector3(3.5f, 0.25f, 3.5f), accentColor);
        CreateBlock(platformsRoot, "Step_C", new Vector3(0f, FloorTop + 0.55f, 22f), new Vector3(3f, 0.25f, 3f), accentColor);

        CreateBlock(platformsRoot, "BeamRun_1", new Vector3(0f, FloorTop + 0.55f, 29f), new Vector3(1.2f, 0.25f, 10f), beamColor);
        CreateBlock(platformsRoot, "BeamRun_2", new Vector3(2.8f, FloorTop + 0.75f, 36f), new Vector3(1.2f, 0.25f, 8f), beamColor);

        CreateBlock(platformsRoot, "RestPlatform", new Vector3(0f, FloorTop + 0.55f, 43f), new Vector3(8f, 0.25f, 6f), platformColor);

        CreateBlock(platformsRoot, "Climb_1", new Vector3(-2f, FloorTop + 1.05f, 50f), new Vector3(4f, 0.25f, 3f), accentColor);
        CreateBlock(platformsRoot, "Climb_2", new Vector3(1.5f, FloorTop + 1.55f, 55f), new Vector3(3.5f, 0.25f, 3f), accentColor);
        CreateBlock(platformsRoot, "Climb_3", new Vector3(-1f, FloorTop + 2.05f, 60f), new Vector3(3f, 0.25f, 3f), accentColor);

        CreateBlock(platformsRoot, "FinalRunway", new Vector3(0f, FloorTop + 2.05f, 72f), new Vector3(7f, 0.25f, 18f), platformColor);

        var lasers = CreateChild(hazardsRoot, "Lasers");
        var spikes = CreateChild(hazardsRoot, "Spikes");

        float beamTop = PlatformTop(FloorTop + 0.55f);
        float restTop = PlatformTop(FloorTop + 0.55f);
        float climbTop = PlatformTop(FloorTop + 1.05f);
        float finalTop = PlatformTop(FloorTop + 2.05f);

        LaserHazard.Create(lasers, "Laser_BeamSection", new Vector3(0f, beamTop + LaserHeightAboveFloor, 31f), new Vector3(0.15f, 0.12f, 8f));
        LaserHazard.Create(lasers, "Laser_ClimbGate", new Vector3(0f, climbTop + 0.75f + LaserHeightAboveFloor, 52.5f), new Vector3(5f, 0.12f, 0.35f));

        SpikeTrap.Create(spikes, "Spikes_Rest", new Vector3(2.5f, restTop, 43f), new Vector3(2f, 0.4f, 2f), 3);
        SpikeTrap.Create(spikes, "Spikes_Final", new Vector3(-2f, finalTop, 68f), new Vector3(2.4f, 0.4f, 1.4f), 4);

        RewardCrate.Create(markersRoot, "Bonus_Crate", new Vector3(2.8f, FloorTop + 2.55f, 36f), RewardTier.Silver);
        RewardCrate.Create(markersRoot, "Bonus_Crate_2", new Vector3(-2.5f, FloorTop + 2.55f, 60f), RewardTier.Gold);

        LevelGoal.Create(markersRoot, "GoalZone", new Vector3(0f, FloorTop + 2.55f, 82f), new Vector3(3.5f, 2.5f, 3.5f));

        CreateBlock(markersRoot, "GoalMarker", new Vector3(0f, FloorTop + 3.8f, 82f), new Vector3(0.35f, 2.5f, 0.35f), new Color(0.2f, 1f, 0.45f));

        var player = GameObject.Find("Player");
        if (player != null)
        {
            var characterController = player.GetComponent<CharacterController>();
            if (characterController != null)
            {
                float groundY = PlatformTop(FloorTop);
                player.transform.position = PlayerAnchorUtility.GetSpawnPositionForGround(
                    new Vector3(0f, 0f, -3f),
                    groundY,
                    characterController);
            }
            else
            {
                player.transform.position = new Vector3(0f, PlatformTop(FloorTop), -3f);
            }

            player.transform.rotation = Quaternion.identity;
        }

        ConfigureParkourGameplay(player);
        return root;
    }

    static void ConfigureParkourGameplay(GameObject player)
    {
        var camera = Camera.main != null ? Camera.main.GetComponent<PlayerCameraController>() : null;
        if (camera != null && player != null)
        {
            camera.SetTarget(player.transform);
            camera.ConfigureAlwaysFollow(true, 6.5f, 2.4f);
        }

        if (player == null)
        {
            return;
        }

        var controller = player.GetComponent<PlayerController>();
        if (controller == null)
        {
            controller = player.AddComponent<PlayerController>();
        }

        var stats = player.GetComponent<PlayerStats>();
        if (stats == null)
        {
            stats = player.AddComponent<PlayerStats>();
        }

        var hud = Object.FindFirstObjectByType<PlayerHUD>();
        if (hud != null)
        {
            hud.BindTo(stats, player.GetComponent<GameScore>());
        }
    }

    static float PlatformTop(float platformCenterY)
    {
        return platformCenterY + 0.125f;
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
