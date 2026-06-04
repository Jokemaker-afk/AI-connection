using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BuildingLightingBuilderMenu
{
    [MenuItem("Tools/Generate Building Lights")]
    public static void GenerateBuildingLights()
    {
        var root = BuildingLightingBuilder.Generate();
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        int lightCount = root.GetComponentsInChildren<Light>(true).Length;
        Debug.Log($"Building lights generated: {lightCount} lights across 3 floors.");
    }

    [MenuItem("Tools/Regenerate Level 1 (Lights + Traps)")]
    public static void RegenerateLevel1()
    {
        const string level1Scene = "Assets/_Project/Scenes/Levels/Level1/SampleScene.unity";
        if (EditorSceneManager.GetActiveScene().path != level1Scene)
        {
            EditorSceneManager.OpenScene(level1Scene);
        }

        BuildingLightingBuilder.Generate();
        GameplayElementsBuilder.Generate();
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("Level 1 lights and gameplay elements regenerated and saved.");
    }
}
