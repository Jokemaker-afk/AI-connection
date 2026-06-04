using UnityEditor;
using UnityEngine;

public static class GameplayElementsBuilderMenu
{
    [MenuItem("Tools/Generate Gameplay Elements")]
    public static void GenerateGameplayElements()
    {
        GameplayElementsBuilder.Generate();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Gameplay Elements", "奖励箱、激光与地刺已按当前地面高度重新生成。", "OK");
    }

    [MenuItem("Tools/Realign Gameplay Elements")]
    public static void RealignGameplayElements()
    {
        GameplayElementsBuilder.RealignExistingElements();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Gameplay Elements", "陷阱与奖励高度已重新对齐。", "OK");
    }
}
