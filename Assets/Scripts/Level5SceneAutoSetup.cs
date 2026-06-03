using UnityEngine;
using UnityEngine.SceneManagement;

static class Level5SceneAutoSetup
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (SceneManager.GetActiveScene().name != "Level5")
        {
            return;
        }

        GameplayHudBootstrap.EnsureGameplayHud();
        Level5ProgressionBootstrap.EnsureLevel5Progression();
    }
}
