using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class LevelSetupMenu
{
    const string Level1ScenePath = "Assets/Scenes/SampleScene.unity";
    const string Level2ScenePath = "Assets/Scenes/Level2.unity";
    const string Level3ScenePath = "Assets/Scenes/Level3.unity";
    const string Level4ScenePath = "Assets/Scenes/Level4.unity";

    [MenuItem("Tools/Setup Level 1 (Score Win)")]
    public static void SetupLevel1()
    {
        EnsureSceneOpen(Level1ScenePath);
        ConfigureLevelManager(
            LevelWinCondition.ScoreThreshold,
            100,
            "Level2",
            string.Empty,
            "按 Y 键进入第二关");
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("Level 1 configured: reach 100 score to enter Level2.");
    }

    [MenuItem("Tools/Setup Level 2 (Parkour -> Level 3)")]
    public static void SetupLevel2()
    {
        EnsureSceneOpen(Level2ScenePath);
        ConfigureLevelManager(
            LevelWinCondition.ReachGoal,
            0,
            "Level3",
            "第二关通关！",
            "按 Y 键进入第三关（Buff 教学）");
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("Level 2 configured: reach goal to enter Level3.");
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
        ConfigureLevelManagerOnObject(
            managerGo,
            LevelWinCondition.ReachGoal,
            0,
            "Level3",
            "第二关通关！",
            "按 Y 键进入第三关（Buff 教学）");

        EditorSceneManager.SaveScene(scene, Level2ScenePath);
        AddSceneToBuildSettings(Level2ScenePath);
        EnsureLevel1InBuildSettings();

        Debug.Log("Level 2 scene created at Assets/Scenes/Level2.unity");
    }

    [MenuItem("Tools/Create Level 3 Scene (Buff Hub)")]
    public static void CreateLevel3Scene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        foreach (var extra in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (extra.name == "Main Camera" || extra.name == "Directional Light")
            {
                Object.DestroyImmediate(extra);
            }
        }

        GameplayRigBuilder.EnsureCoreGameplayObjects(new Vector3(0f, 0.25f, 0f));
        Level3BuffHubBuilder.Generate(true);
        EnsureLevel3LevelManager();

        EditorSceneManager.SaveScene(scene, Level3ScenePath);
        AddSceneToBuildSettings(Level3ScenePath);
        EnsureAllLevelsInBuildSettings();

        Debug.Log("Level 3 scene created at Assets/Scenes/Level3.unity");
    }

    [MenuItem("Tools/Setup Level 3 (Buff Hub -> Level 4)")]
    public static void SetupLevel3()
    {
        EnsureSceneOpen(Level3ScenePath);
        EnsureLevel3RuntimeSystems();
        EnsureLevel3LevelManager();
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("Level 3 configured: collect all 4 buffs, then use spawn portal to enter Level4.");
    }

    [MenuItem("Tools/Create Level 4 Scene")]
    public static void CreateLevel4Scene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        foreach (var extra in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (extra.name == "Main Camera" || extra.name == "Directional Light")
            {
                Object.DestroyImmediate(extra);
            }
        }

        GameplayRigBuilder.EnsureCoreGameplayObjects(new Vector3(0f, 0.25f, 0f));
        Level4PlaceholderBuilder.Generate(true);

        EditorSceneManager.SaveScene(scene, Level4ScenePath);
        AddSceneToBuildSettings(Level4ScenePath);
        EnsureAllLevelsInBuildSettings();

        Debug.Log("Level 4 scene created at Assets/Scenes/Level4.unity");
    }

    static void EnsureLevel3LevelManager()
    {
        ConfigureLevelManager(
            LevelWinCondition.ReachGoal,
            0,
            "Level4",
            "Buff 教学完成！",
            "按 Y 键进入第四关");
    }

    static void EnsureLevel3RuntimeSystems()
    {
        var buffHub = GameObject.Find("BuffHub");
        Transform root = buffHub != null ? buffHub.transform : null;

        var systemsGo = GameObject.Find("Level3Systems");
        if (systemsGo == null)
        {
            systemsGo = new GameObject("Level3Systems");
            if (root != null)
            {
                systemsGo.transform.SetParent(root, false);
            }
        }

        if (systemsGo.GetComponent<BuffCollectionTracker>() == null)
        {
            systemsGo.AddComponent<BuffCollectionTracker>();
        }

        var portal = systemsGo.GetComponent<HubAdvancePortal>();
        if (portal == null)
        {
            portal = systemsGo.AddComponent<HubAdvancePortal>();
        }

        var player = GameObject.Find("Player");
        var cc = player != null ? player.GetComponent<CharacterController>() : null;
        const float floorTop = 0.125f;
        Vector3 hubSpawn = cc != null
            ? PlayerAnchorUtility.GetSpawnPositionForGround(Vector3.zero, floorTop + 0.125f, cc)
            : new Vector3(0f, floorTop + 1.25f, 0f);
        portal.Configure(hubSpawn + new Vector3(0f, 0.1f, 2.2f));
    }

    static void EnsureSceneOpen(string scenePath)
    {
        var active = SceneManager.GetActiveScene();
        if (active.path != scenePath)
        {
            EditorSceneManager.OpenScene(scenePath);
        }
    }

    static void ConfigureLevelManager(
        LevelWinCondition condition,
        int score,
        string nextScene,
        string completeMessage,
        string advanceHint)
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

        ConfigureLevelManagerOnObject(managerGo, condition, score, nextScene, completeMessage, advanceHint);
    }

    static void ConfigureLevelManagerOnObject(
        GameObject managerGo,
        LevelWinCondition condition,
        int score,
        string nextScene,
        string completeMessage,
        string advanceHint)
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

        if (condition == LevelWinCondition.ScoreThreshold)
        {
            so.FindProperty("scoreAdvanceHint").stringValue = advanceHint;
        }
        else
        {
            so.FindProperty("goalAdvanceHint").stringValue = advanceHint;
        }

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void EnsureLevel1InBuildSettings()
    {
        AddSceneToBuildSettings(Level1ScenePath);
    }

    static void EnsureAllLevelsInBuildSettings()
    {
        AddSceneToBuildSettings(Level1ScenePath);
        AddSceneToBuildSettings(Level2ScenePath);
        AddSceneToBuildSettings(Level3ScenePath);
        AddSceneToBuildSettings(Level4ScenePath);
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
