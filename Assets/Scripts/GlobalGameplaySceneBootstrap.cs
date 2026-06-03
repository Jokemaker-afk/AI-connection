using UnityEngine;
using UnityEngine.SceneManagement;

static class GlobalGameplaySceneBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void BeforeSceneLoad()
    {
        GameplayFoundationBootstrap.CapturePersistentState();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AfterSceneLoad()
    {
        GameplayFoundationBootstrap.EnsureForActiveScene();
    }
}
