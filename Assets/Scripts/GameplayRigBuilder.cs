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
            PlayerAnchorUtility.EnsureStandardController(controller);

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

        if (player.GetComponent<PlayerBuffController>() == null)
        {
            player.AddComponent<PlayerBuffController>();
        }

        if (player.GetComponent<PlayerShieldVisual>() == null)
        {
            player.AddComponent<PlayerShieldVisual>();
        }

        if (player.GetComponent<PlayerInventory>() == null)
        {
            player.AddComponent<PlayerInventory>();
        }

        if (player.GetComponent<PlayerPickupInteractor>() == null)
        {
            player.AddComponent<PlayerPickupInteractor>();
        }

        if (player.GetComponent<PlayerCraftingInteractor>() == null)
        {
            player.AddComponent<PlayerCraftingInteractor>();
        }

        if (player.GetComponent<PlayerPlacementController>() == null)
        {
            player.AddComponent<PlayerPlacementController>();
        }

        var characterController = player.GetComponent<CharacterController>();
        if (characterController != null)
        {
            PlayerAnchorUtility.AlignCapsuleVisual(player);
        }

        player.transform.position = spawn;
        player.transform.rotation = Quaternion.identity;
        GameplayLayers.TrySetPlayerLayer(player);
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
        GameplayHudBootstrap.EnsureGameplayHud();
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
