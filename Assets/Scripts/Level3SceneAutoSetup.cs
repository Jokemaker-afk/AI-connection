using UnityEngine;
using UnityEngine.SceneManagement;

static class Level3SceneAutoSetup
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (SceneManager.GetActiveScene().name != "Level3")
        {
            return;
        }

        EnsureLevelManager();
        EnsureProgressSystems();
    }

    static void EnsureLevelManager()
    {
        var manager = Object.FindFirstObjectByType<LevelManager>();
        if (manager == null)
        {
            var managerGo = new GameObject("LevelManager");
            manager = managerGo.AddComponent<LevelManager>();
        }

        manager.ConfigureGoalTransition(
            "Level4",
            "Buff 教学完成！",
            "按 Y 键进入第四关");
    }

    static void EnsureProgressSystems()
    {
        var tracker = Object.FindFirstObjectByType<BuffCollectionTracker>();
        HubAdvancePortal portal = Object.FindFirstObjectByType<HubAdvancePortal>();

        if (tracker != null && portal != null)
        {
            return;
        }

        var systemsGo = GameObject.Find("Level3Systems");
        if (systemsGo == null)
        {
            systemsGo = new GameObject("Level3Systems");
            var buffHub = GameObject.Find("BuffHub");
            if (buffHub != null)
            {
                systemsGo.transform.SetParent(buffHub.transform, false);
            }
        }

        if (tracker == null)
        {
            tracker = systemsGo.GetComponent<BuffCollectionTracker>();
            if (tracker == null)
            {
                tracker = systemsGo.AddComponent<BuffCollectionTracker>();
            }
        }

        if (portal == null)
        {
            portal = systemsGo.GetComponent<HubAdvancePortal>();
            if (portal == null)
            {
                portal = systemsGo.AddComponent<HubAdvancePortal>();
            }
        }

        var player = Object.FindFirstObjectByType<PlayerController>();
        var cc = player != null ? player.GetComponent<CharacterController>() : null;
        const float floorTop = 0.125f;
        Vector3 hubSpawn = cc != null
            ? PlayerAnchorUtility.GetSpawnPositionForGround(Vector3.zero, floorTop + 0.125f, cc)
            : new Vector3(0f, floorTop + 1.25f, 0f);
        portal.Configure(hubSpawn + new Vector3(0f, 0.1f, 2.2f));
    }
}
