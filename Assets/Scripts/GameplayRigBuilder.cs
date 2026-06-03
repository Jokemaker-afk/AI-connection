using UnityEngine;

public static class GameplayRigBuilder
{
    public static void EnsureCoreGameplayObjects(Vector3 playerSpawn)
    {
        EnsureDirectionalLight();

        GameplayCore core = GameplayCore.EnsureExists();
        PersistentPlayerRig rig = PersistentPlayerRig.EnsureOnGameplayCore(core);
        rig.EnsurePlayer(playerSpawn, out _);
        rig.EnsureMainCamera();
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

    static void EnsureGameplayHud()
    {
        GameplayHudBootstrap.EnsureGameplayHud();
    }
}
