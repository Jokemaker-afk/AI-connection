using UnityEngine;
using UnityEngine.UI;

/// <summary>Resets and binds scene-specific objective UI on the persistent GameplayHUD.</summary>
public static class LevelObjectiveUiRegistry
{
    static string activeSceneName = string.Empty;
    static string boundProviderName = string.Empty;

    public static string ActiveSceneName => activeSceneName;
    public static string BoundProviderName => boundProviderName;

    public static void ResetForScene(string sceneName)
    {
        activeSceneName = sceneName;
        boundProviderName = string.Empty;

        ResetSceneTrackersForLevel(sceneName);

        GameObject hud = RuntimeSingletonCleanup.ResolveSingleGameplayHud();
        if (hud == null)
        {
            return;
        }

        ResetPanel<SceneProgressionHud>(hud);
        ResetPanel<CollectibleProgressHud>(hud);
        ResetPanel<Level6ProgressionHud>(hud);
        ResetPanel<Level7ProgressionHud>(hud);
        ResetPanel<Level8ProgressionHud>(hud);

        HidePanelIfPresent(hud.transform, "SceneObjectivePanel");
        HidePanelIfPresent(hud.transform, "CollectibleProgressPanel");
        HidePanelIfPresent(hud.transform, "Level6ObjectivePanel");
        HidePanelIfPresent(hud.transform, "Level7ObjectivePanel");
        HidePanelIfPresent(hud.transform, "Level8ObjectivePanel");

        GameplayCore.Instance?.Log($"LevelObjectiveUiRegistry reset for {sceneName}.");
    }

    public static void NotifyProviderBound(string sceneName, string providerName)
    {
        activeSceneName = sceneName;
        boundProviderName = providerName;
        ApplyForScene(sceneName);
    }

    public static void ApplyForScene(string sceneName)
    {
        activeSceneName = sceneName;
        int level = GameplaySceneCatalog.GetLevelNumber(sceneName);

        GameObject hud = RuntimeSingletonCleanup.ResolveSingleGameplayHud();
        if (hud == null)
        {
            return;
        }

        ApplyPanel<CollectibleProgressHud>(hud, level == 4);
        ApplyPanel<SceneProgressionHud>(hud, level == 5);
        ApplyPanel<Level6ProgressionHud>(hud, level == 6);
        ApplyPanel<Level7ProgressionHud>(hud, level == 7);
        ApplyPanel<Level8ProgressionHud>(hud, level == 8);
    }

    public static string GetActiveObjectiveText()
    {
        GameObject hud = RuntimeSingletonCleanup.ResolveSingleGameplayHud();
        if (hud == null)
        {
            return string.Empty;
        }

        string text = ReadPanelText(hud.transform, "SceneObjectivePanel");
        if (!string.IsNullOrEmpty(text))
        {
            return text;
        }

        text = ReadPanelText(hud.transform, "Level6ObjectivePanel");
        if (!string.IsNullOrEmpty(text))
        {
            return text;
        }

        text = ReadPanelText(hud.transform, "Level7ObjectivePanel");
        if (!string.IsNullOrEmpty(text))
        {
            return text;
        }

        text = ReadPanelText(hud.transform, "Level8ObjectivePanel");
        if (!string.IsNullOrEmpty(text))
        {
            return text;
        }

        return ReadCollectiblePanelText(hud.transform);
    }

    public static int CountVisibleObjectivePanels()
    {
        GameObject hud = RuntimeSingletonCleanup.ResolveSingleGameplayHud();
        if (hud == null)
        {
            return 0;
        }

        int count = 0;
        count += CountActivePanel(hud.transform, "SceneObjectivePanel");
        count += CountActivePanel(hud.transform, "CollectibleProgressPanel");
        count += CountActivePanel(hud.transform, "Level6ObjectivePanel");
        count += CountActivePanel(hud.transform, "Level7ObjectivePanel");
        count += CountActivePanel(hud.transform, "Level8ObjectivePanel");
        return count;
    }

    public static bool IsPreviousLevelProviderAlive(int previousLevel)
    {
        switch (previousLevel)
        {
            case 5:
                return Object.FindFirstObjectByType<SceneProgressionManager>() != null;
            case 6:
                return Object.FindFirstObjectByType<Level6ProgressionManager>() != null;
            case 7:
                return Object.FindFirstObjectByType<Level7ProgressionManager>() != null;
            default:
                return false;
        }
    }

    static void ResetSceneTrackersForLevel(string sceneName)
    {
        int level = GameplaySceneCatalog.GetLevelNumber(sceneName);

        if (level == 5)
        {
            RequiredPlacedObjectTracker.Instance?.ResetForSceneTransition();
        }

        if (level == 6)
        {
            Level6TaskProgressTracker.Instance?.ResetForSceneEntry();
        }

        if (level == 7)
        {
            Level7TaskProgressTracker.Instance?.ResetForSceneEntry();
        }

        if (level == 8)
        {
            Level8ObjectiveTracker.Instance?.ResetForSceneEntry();
        }
    }

    static void ResetPanel<T>(GameObject hud) where T : MonoBehaviour, ILevelObjectiveHudPanel
    {
        T panel = hud.GetComponent<T>();
        panel?.ResetForSceneTransition();
    }

    static void ApplyPanel<T>(GameObject hud, bool activeForScene) where T : MonoBehaviour, ILevelObjectiveHudPanel
    {
        T panel = hud.GetComponent<T>();
        if (panel == null)
        {
            return;
        }

        if (!activeForScene)
        {
            panel.ResetForSceneTransition();
            return;
        }

        panel.RefreshObjectiveDisplay();
    }

    static void HidePanelIfPresent(Transform hudRoot, string panelName)
    {
        Transform panel = hudRoot.Find(panelName);
        if (panel != null)
        {
            panel.gameObject.SetActive(false);
        }
    }

    static int CountActivePanel(Transform hudRoot, string panelName)
    {
        Transform panel = hudRoot.Find(panelName);
        return panel != null && panel.gameObject.activeSelf ? 1 : 0;
    }

    static string ReadPanelText(Transform hudRoot, string panelName)
    {
        Transform panel = hudRoot.Find(panelName);
        if (panel == null || !panel.gameObject.activeSelf)
        {
            return string.Empty;
        }

        Text text = panel.GetComponentInChildren<Text>(true);
        return text != null ? text.text : string.Empty;
    }

    static string ReadCollectiblePanelText(Transform hudRoot)
    {
        Transform panel = hudRoot.Find("CollectibleProgressPanel");
        if (panel == null || !panel.gameObject.activeSelf)
        {
            return string.Empty;
        }

        Text text = panel.GetComponentInChildren<Text>(true);
        return text != null ? text.text : string.Empty;
    }
}
