using UnityEngine;
using UnityEngine.SceneManagement;

static class Level6SceneAutoSetup
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (SceneManager.GetActiveScene().name != "Level6")
        {
            return;
        }

        GameplayRigBuilder.EnsureCoreGameplayObjects(new Vector3(0f, 0f, -2f));

        if (GameObject.Find("Level6Arena") == null)
        {
            Level6PlaceholderBuilder.Generate(true);
        }

        GameplayHudBootstrap.EnsureGameplayHud();
    }
}
