using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Keeps one persistent runtime owner for gameplay UI/bootstrap objects after scene transitions.
/// Data on PlayerRoot persists; duplicate UI GameObjects are removed.
/// </summary>
public static class RuntimeSingletonCleanup
{
    public static bool CleanupDuplicateRuntimeObjects(string sceneName)
    {
        if (!GameplaySceneCatalog.IsSupportedGameplayScene(sceneName))
        {
            return false;
        }

        bool hadDuplicates = false;
        hadDuplicates |= DedupeGameplayCore();
        hadDuplicates |= DedupePlayerRoots();
        hadDuplicates |= DedupeGameplayHud();
        hadDuplicates |= DedupeEventSystems();
        hadDuplicates |= DedupeMainCameras();
        hadDuplicates |= DedupeAudioListeners();
        return hadDuplicates;
    }

    public static GameObject ResolveSingleGameplayHud()
    {
        PlayerHUD[] huds = Object.FindObjectsByType<PlayerHUD>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (huds.Length == 0)
        {
            return null;
        }

        GameObject keep = ChooseKeepGameplayHud(huds);
        for (int i = 0; i < huds.Length; i++)
        {
            GameObject candidate = huds[i].gameObject;
            if (candidate == null || candidate == keep)
            {
                continue;
            }

            Object.Destroy(candidate);
            GameplayCore.Instance?.Log($"Removed duplicate GameplayHUD '{candidate.name}' from scene '{candidate.scene.name}'.");
        }

        if (keep != null && keep.scene.name != "DontDestroyOnLoad" && Application.isPlaying)
        {
            Object.DontDestroyOnLoad(keep);
        }

        return keep;
    }

    static bool DedupeGameplayHud()
    {
        PlayerHUD[] huds = Object.FindObjectsByType<PlayerHUD>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (huds.Length <= 1)
        {
            return false;
        }

        ResolveSingleGameplayHud();
        GameplayCore.Instance?.Log($"Deduped GameplayHUD instances to 1 (was {huds.Length}).");
        return true;
    }

    static GameObject ChooseKeepGameplayHud(PlayerHUD[] huds)
    {
        GameObject persistent = null;
        GameObject sceneLocal = null;

        for (int i = 0; i < huds.Length; i++)
        {
            if (huds[i] == null)
            {
                continue;
            }

            GameObject candidate = huds[i].gameObject;
            if (candidate.scene.name == "DontDestroyOnLoad")
            {
                persistent = candidate;
                break;
            }

            if (sceneLocal == null)
            {
                sceneLocal = candidate;
            }
        }

        return persistent != null ? persistent : sceneLocal;
    }

    static bool DedupeGameplayCore()
    {
        GameplayCore[] cores = Object.FindObjectsByType<GameplayCore>(FindObjectsSortMode.None);
        if (cores.Length <= 1)
        {
            return false;
        }

        GameplayCore keep = ChooseKeepComponent(cores);
        for (int i = 0; i < cores.Length; i++)
        {
            if (cores[i] != null && cores[i] != keep)
            {
                Object.Destroy(cores[i].gameObject);
            }
        }

        GameplayCore.Instance?.Log($"Deduped GameplayCore instances to 1 (was {cores.Length}).");
        return true;
    }

    static bool DedupePlayerRoots()
    {
        PlayerController[] controllers = Object.FindObjectsByType<PlayerController>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        GameObject keepPlayer = PersistentPlayerRig.Player;
        if (keepPlayer == null)
        {
            keepPlayer = ChooseKeepPlayer(controllers);
        }

        if (keepPlayer == null)
        {
            return false;
        }

        bool removed = false;
        for (int i = 0; i < controllers.Length; i++)
        {
            GameObject candidate = controllers[i].gameObject;
            if (candidate == null || candidate == keepPlayer || candidate.name != "Player")
            {
                continue;
            }

            Object.Destroy(candidate);
            removed = true;
        }

        if (removed)
        {
            GameplayCore.Instance?.Log("Removed duplicate PlayerRoot after scene transition.");
        }

        return removed;
    }

    static GameObject ChooseKeepPlayer(PlayerController[] controllers)
    {
        GameObject persistent = null;
        GameObject sceneLocal = null;

        for (int i = 0; i < controllers.Length; i++)
        {
            if (controllers[i] == null || controllers[i].gameObject.name != "Player")
            {
                continue;
            }

            GameObject candidate = controllers[i].gameObject;
            if (candidate.scene.name == "DontDestroyOnLoad")
            {
                persistent = candidate;
                break;
            }

            if (sceneLocal == null)
            {
                sceneLocal = candidate;
            }
        }

        return persistent != null ? persistent : sceneLocal;
    }

    public static bool DedupeEventSystems()
    {
        EventSystem[] eventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
        if (eventSystems.Length <= 1)
        {
            return false;
        }

        EventSystem keep = ChooseKeepComponent(eventSystems);
        for (int i = 0; i < eventSystems.Length; i++)
        {
            EventSystem candidate = eventSystems[i];
            if (candidate == null || candidate == keep)
            {
                continue;
            }

            Object.Destroy(candidate.gameObject);
        }

        GameplayCore.Instance?.Log($"Deduped EventSystem instances to 1 (was {eventSystems.Length}).");
        return true;
    }

    static bool DedupeMainCameras()
    {
        Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Camera keep = null;
        bool removed = false;

        for (int i = 0; i < cameras.Length; i++)
        {
            Camera candidate = cameras[i];
            if (candidate == null || !candidate.gameObject.CompareTag("MainCamera"))
            {
                continue;
            }

            if (keep == null)
            {
                keep = candidate;
                continue;
            }

            bool keepPersistent = keep.gameObject.scene.name == "DontDestroyOnLoad";
            bool candidatePersistent = candidate.gameObject.scene.name == "DontDestroyOnLoad";
            if (!keepPersistent && candidatePersistent)
            {
                Object.Destroy(keep.gameObject);
                keep = candidate;
            }
            else
            {
                Object.Destroy(candidate.gameObject);
            }

            removed = true;
        }

        return removed;
    }

    static bool DedupeAudioListeners()
    {
        AudioListener[] listeners = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        if (listeners.Length <= 1)
        {
            return false;
        }

        AudioListener keep = ChooseKeepComponent(listeners);
        for (int i = 0; i < listeners.Length; i++)
        {
            AudioListener candidate = listeners[i];
            if (candidate == null || candidate == keep)
            {
                continue;
            }

            candidate.enabled = false;
            if (candidate.gameObject.scene.name != "DontDestroyOnLoad")
            {
                Object.Destroy(candidate);
            }
        }

        if (keep != null)
        {
            keep.enabled = true;
        }

        GameplayCore.Instance?.Log($"Deduped AudioListener instances to 1 active (was {listeners.Length}).");
        return true;
    }

    static T ChooseKeepComponent<T>(T[] components) where T : Component
    {
        T persistent = null;
        T sceneLocal = null;

        for (int i = 0; i < components.Length; i++)
        {
            if (components[i] == null)
            {
                continue;
            }

            if (components[i].gameObject.scene.name == "DontDestroyOnLoad")
            {
                persistent = components[i];
                break;
            }

            if (sceneLocal == null)
            {
                sceneLocal = components[i];
            }
        }

        return persistent != null ? persistent : sceneLocal;
    }

    public static int CountGameplayHudRoots()
    {
        return Object.FindObjectsByType<PlayerHUD>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
    }

    public static int CountInventoryHudRoots()
    {
        return Object.FindObjectsByType<PlayerInventoryHud>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
    }

    public static int CountGameplayOverlayCanvases()
    {
        int count = 0;
        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < canvases.Length; i++)
        {
            Canvas canvas = canvases[i];
            if (canvas == null || canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                continue;
            }

            if (IsGameplayOwnedCanvas(canvas.gameObject))
            {
                count++;
            }
        }

        return count;
    }

    static bool IsGameplayOwnedCanvas(GameObject canvasOwner)
    {
        if (canvasOwner.GetComponent<PlayerHUD>() != null
            || canvasOwner.GetComponent<PlayerInventoryHud>() != null
            || canvasOwner.GetComponentInParent<PlayerHUD>() != null)
        {
            return true;
        }

        return canvasOwner.name == "GameplayHUD"
            || canvasOwner.name == "LevelTransitionOverlay";
    }

    public static bool HasDuplicateRuntimeUi()
    {
        return CountGameplayHudRoots() > 1
            || CountInventoryHudRoots() > 1
            || Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length > 1
            || CountGameplayOverlayCanvases() > 1;
    }
}
