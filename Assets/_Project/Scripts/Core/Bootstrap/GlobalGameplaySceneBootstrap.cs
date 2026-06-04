using UnityEngine;
using UnityEngine.SceneManagement;

static class GlobalGameplaySceneBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void BeforeSceneLoad()
    {
        if (GameplayCore.Exists)
        {
            GameplayFoundationBootstrap.CapturePersistentState();
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AfterSceneLoad()
    {
        GameplayFoundationBootstrap.EnsureForActiveScene();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void RegisterSceneLoadedCallback()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!GameplaySceneCatalog.IsSupportedGameplayScene(scene.name))
        {
            return;
        }

        GameplayFoundationBootstrap.FinalizeSceneTransition(scene.name);
    }
}
