using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class MultiFloorLevelBuilderMenu
{
    [MenuItem("Tools/Generate Multi-Floor Level")]
    public static void GenerateLevel()
    {
        MultiFloorLevelBuilder.Generate(true);
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("Multi-floor level generated: Building_Large");
    }
}
