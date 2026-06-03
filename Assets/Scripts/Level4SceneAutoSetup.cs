using UnityEngine;
using UnityEngine.SceneManagement;

static class Level4SceneAutoSetup
{
    const float FloorTop = 0.125f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (SceneManager.GetActiveScene().name != "Level4")
        {
            return;
        }

        GameplayHudBootstrap.EnsureGameplayHud();
        EnsureCollectibleSystems();
    }

    static void EnsureCollectibleSystems()
    {
        var systemsGo = GameObject.Find("Level4Systems");
        if (systemsGo == null)
        {
            systemsGo = new GameObject("Level4Systems");
            var arena = GameObject.Find("Level4Arena");
            if (arena != null)
            {
                systemsGo.transform.SetParent(arena.transform, false);
            }
        }

        var collectibleManager = systemsGo.GetComponent<CollectibleManager>();
        if (collectibleManager == null)
        {
            collectibleManager = systemsGo.AddComponent<CollectibleManager>();
        }

        var portalUnlock = systemsGo.GetComponent<PortalUnlockManager>();
        if (portalUnlock == null)
        {
            portalUnlock = systemsGo.AddComponent<PortalUnlockManager>();
        }

        Vector3 portalPosition = new Vector3(0f, FloorTop + 0.525f, 0f);
        portalUnlock.Configure(portalPosition, "Level5", 0.3f);

        collectibleManager.RefreshTotalCount();

        var hudGo = GameObject.Find("GameplayHUD");
        if (hudGo != null)
        {
            var progressHud = hudGo.GetComponent<CollectibleProgressHud>();
            if (progressHud == null)
            {
                progressHud = hudGo.AddComponent<CollectibleProgressHud>();
            }

            progressHud.BindTo(collectibleManager);
        }
    }
}
