using UnityEngine;

public static class GameplayRigBuilder
{
    public static void EnsureCoreGameplayObjects(Vector3 playerSpawn)
    {
        EnsureDirectionalLight();
        EnsurePlayer(playerSpawn);
        EnsureMainCamera();
        EnsureGameplayHud();
    }

    static void EnsureDirectionalLight()
    {
        if (GameObject.Find("Directional Light") != null)
        {
            return;
        }

        var lightGo = new GameObject("Directional Light");
        var light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 0.45f;
        light.color = new Color(0.9f, 0.92f, 1f);
        light.shadows = LightShadows.Soft;
        lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }

    static void EnsurePlayer(Vector3 spawn)
    {
        var player = GameObject.Find("Player");
        if (player == null)
        {
            player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());

            var controller = player.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.35f;
            controller.center = new Vector3(0f, 1f, 0f);

            player.AddComponent<PlayerController>();
            player.AddComponent<PlayerStats>();
            player.AddComponent<GameScore>();
        }

        if (player.GetComponent<PlayerStats>() == null)
        {
            player.AddComponent<PlayerStats>();
        }

        RemoveDuplicateComponents<PlayerStats>(player);

        if (player.GetComponent<GameScore>() == null)
        {
            player.AddComponent<GameScore>();
        }

        player.transform.position = spawn;
        player.transform.rotation = Quaternion.identity;
    }

    static void EnsureMainCamera()
    {
        var cameraGo = GameObject.Find("Main Camera");
        if (cameraGo == null)
        {
            cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            cameraGo.AddComponent<Camera>();
            cameraGo.AddComponent<AudioListener>();
        }

        if (cameraGo.GetComponent<PlayerCameraController>() == null)
        {
            cameraGo.AddComponent<PlayerCameraController>();
        }

        cameraGo.transform.position = new Vector3(0f, 4f, -8f);
        cameraGo.transform.rotation = Quaternion.Euler(15f, 0f, 0f);

        var player = GameObject.Find("Player");
        var cameraController = cameraGo.GetComponent<PlayerCameraController>();
        if (cameraController != null && player != null)
        {
            cameraController.SetTarget(player.transform);
        }
    }

    static void EnsureGameplayHud()
    {
        var hudGo = GameObject.Find("GameplayHUD");
        if (hudGo == null)
        {
            hudGo = new GameObject("GameplayHUD", typeof(RectTransform));
        }

        if (hudGo.GetComponent<RectTransform>() == null)
        {
            hudGo.AddComponent<RectTransform>();
        }

        var hud = hudGo.GetComponent<PlayerHUD>();
        if (hud == null)
        {
            hud = hudGo.AddComponent<PlayerHUD>();
        }

        hud.RebuildUi();

        var player = GameObject.Find("Player");
        var stats = player != null ? player.GetComponent<PlayerStats>() : null;
        var score = player != null ? player.GetComponent<GameScore>() : null;
        if (stats != null)
        {
            hud.BindTo(stats, score);
        }

        var rect = hudGo.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.localScale = Vector3.one;
        }
    }

    static void RemoveDuplicateComponents<T>(GameObject target) where T : Component
    {
        var components = target.GetComponents<T>();
        for (int i = 1; i < components.Length; i++)
        {
            Object.DestroyImmediate(components[i]);
        }
    }
}
