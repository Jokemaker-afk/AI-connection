using UnityEngine;
using UnityEngine.UI;

public class Level7ProgressionHud : MonoBehaviour
{
    [SerializeField] Level7ProgressionManager progressionManager;

    RectTransform panelRoot;
    Text objectiveText;

    void Awake()
    {
        BuildUi();
        BindManager();
    }

    void OnEnable()
    {
        BindManager();
        Subscribe();
        RefreshObjective();
    }

    void OnDisable()
    {
        Unsubscribe();
    }

    public void BindTo(Level7ProgressionManager manager)
    {
        Unsubscribe();
        progressionManager = manager;
        Subscribe();
        RefreshObjective();
    }

    void BindManager()
    {
        if (progressionManager == null)
        {
            progressionManager = FindFirstObjectByType<Level7ProgressionManager>();
        }
    }

    void Subscribe()
    {
        Level7TaskProgressTracker tracker = Level7TaskProgressTracker.Instance;
        if (tracker != null)
        {
            tracker.OnProgressChanged -= RefreshObjective;
            tracker.OnProgressChanged += RefreshObjective;
        }

        if (progressionManager != null)
        {
            progressionManager.OnPortalActivated -= RefreshObjective;
            progressionManager.OnPortalActivated += RefreshObjective;
        }
    }

    void Unsubscribe()
    {
        Level7TaskProgressTracker tracker = Level7TaskProgressTracker.Instance;
        if (tracker != null)
        {
            tracker.OnProgressChanged -= RefreshObjective;
        }

        if (progressionManager != null)
        {
            progressionManager.OnPortalActivated -= RefreshObjective;
        }
    }

    void RefreshObjective()
    {
        if (objectiveText == null)
        {
            return;
        }

        string text = BuildObjectiveText();
        GameplayChineseText.PrepareUiText(objectiveText, text);
        panelRoot.gameObject.SetActive(!string.IsNullOrEmpty(text));
    }

    string BuildObjectiveText()
    {
        Level7TaskProgressTracker tracker = Level7TaskProgressTracker.Instance;
        if (tracker == null)
        {
            return string.Empty;
        }

        if (progressionManager != null && progressionManager.IsPortalActivated)
        {
            return "目标：前往传送门，按 Y 进入第八关";
        }

        if (!tracker.IsTaskComplete(Level7TaskProgressTracker.HitMeleeDummyTaskId))
        {
            return "目标：选择基础剑并攻击训练假人";
        }

        if (!tracker.IsTaskComplete(Level7TaskProgressTracker.HitRangedTargetTaskId))
        {
            return "目标：选择训练发射器并击中远处训练靶";
        }

        if (!tracker.IsTaskComplete(Level7TaskProgressTracker.DefeatBasicWalkerTaskId))
        {
            return "目标：击败基础敌人";
        }

        return "目标：前往传送门，按 Y 进入第八关";
    }

    void BuildUi()
    {
        var canvas = GetComponent<RectTransform>();
        if (canvas == null)
        {
            return;
        }

        var panelGo = new GameObject("Level7ObjectivePanel", typeof(RectTransform), typeof(Image));
        panelGo.transform.SetParent(canvas, false);
        panelRoot = panelGo.GetComponent<RectTransform>();
        panelRoot.anchorMin = new Vector2(0.5f, 1f);
        panelRoot.anchorMax = new Vector2(0.5f, 1f);
        panelRoot.pivot = new Vector2(0.5f, 1f);
        panelRoot.anchoredPosition = new Vector2(0f, -92f);
        panelRoot.sizeDelta = new Vector2(760f, 44f);

        var image = panelGo.GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.45f);
        image.raycastTarget = false;

        var textGo = new GameObject("ObjectiveText", typeof(RectTransform), typeof(Text));
        textGo.transform.SetParent(panelRoot, false);
        var textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(12f, 4f);
        textRect.offsetMax = new Vector2(-12f, -4f);

        objectiveText = textGo.GetComponent<Text>();
        objectiveText.fontSize = 22;
        GameplayChineseText.PrepareUiText(objectiveText, string.Empty);
        panelRoot.gameObject.SetActive(false);
    }
}
