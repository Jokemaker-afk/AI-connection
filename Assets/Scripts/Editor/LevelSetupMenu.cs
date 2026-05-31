using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LevelSetupMenu
{
    const string Level1ScenePath = "Assets/Scenes/SampleScene.unity";
    const string Level2ScenePath = "Assets/Scenes/Level2.unity";

    [MenuItem("Tools/Setup Level 1 (Score Win)")]
    public static void SetupLevel1()
    {
        EnsureSceneOpen(Level1ScenePath);
        ConfigureLevelManager(LevelWinCondition.ScoreThreshold, 100, "Level2", string.Empty);
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("Level 1 configured: reach 100 score to enter Level2.");
    }

    [MenuItem("Tools/Create Level 2 Scene (Parkour)")]
    public static void CreateLevel2Scene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        foreach (var extra in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (extra.name == "Main Camera" || extra.name == "Directional Light")
            {
                Object.DestroyImmediate(extra);
            }
        }

        GameplayRigBuilder.EnsureCoreGameplayObjects(new Vector3(0f, 0.25f, -3f));
        ParkourLevelBuilder.Generate(true);

        var managerGo = new GameObject("LevelManager");
        ConfigureLevelManagerOnObject(managerGo, LevelWinCondition.ReachGoal, 0, string.Empty, "恭喜通关！到达终点！");

        EditorSceneManager.SaveScene(scene, Level2ScenePath);
        AddSceneToBuildSettings(Level2ScenePath);
        EnsureLevel1InBuildSettings();

        Debug.Log("Level 2 scene created at Assets/Scenes/Level2.unity");
    }

    static void EnsureSceneOpen(string scenePath)
    {
        var active = SceneManager.GetActiveScene();
        if (active.path != scenePath)
        {
            EditorSceneManager.OpenScene(scenePath);
        }
    }

    static void ConfigureLevelManager(LevelWinCondition condition, int score, string nextScene, string completeMessage)
    {
        var manager = Object.FindFirstObjectByType<LevelManager>();
        GameObject managerGo;
        if (manager == null)
        {
            managerGo = new GameObject("LevelManager");
            manager = managerGo.AddComponent<LevelManager>();
        }
        else
        {
            managerGo = manager.gameObject;
        }

        ConfigureLevelManagerOnObject(managerGo, condition, score, nextScene, completeMessage);
    }

    static void ConfigureLevelManagerOnObject(GameObject managerGo, LevelWinCondition condition, int score, string nextScene, string completeMessage)
    {
        var manager = managerGo.GetComponent<LevelManager>();
        if (manager == null)
        {
            manager = managerGo.AddComponent<LevelManager>();
        }

        var so = new SerializedObject(manager);
        so.FindProperty("winCondition").enumValueIndex = (int)condition;
        so.FindProperty("scoreToAdvance").intValue = score;
        so.FindProperty("nextSceneName").stringValue = nextScene;
        if (!string.IsNullOrEmpty(completeMessage))
        {
            so.FindProperty("levelCompleteMessage").stringValue = completeMessage;
        }

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void EnsureLevel1InBuildSettings()
    {
        AddSceneToBuildSettings(Level1ScenePath);
    }

    static void AddSceneToBuildSettings(string scenePath)
    {
        var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (var scene in scenes)
        {
            if (scene.path == scenePath)
            {
                return;
            }
        }

        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
