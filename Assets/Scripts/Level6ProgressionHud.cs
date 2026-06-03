using UnityEngine;
using UnityEngine.UI;

public class Level6ProgressionHud : MonoBehaviour
{
    [SerializeField] Level6ProgressionManager progressionManager;

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

    public void BindTo(Level6ProgressionManager manager)
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
            progressionManager = FindFirstObjectByType<Level6ProgressionManager>();
        }
    }

    void Subscribe()
    {
        Level6TaskProgressTracker tracker = Level6TaskProgressTracker.Instance;
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
        Level6TaskProgressTracker tracker = Level6TaskProgressTracker.Instance;
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
        Level6TaskProgressTracker tracker = Level6TaskProgressTracker.Instance;
        if (tracker == null)
        {
            return string.Empty;
        }

        if (progressionManager != null && progressionManager.IsPortalActivated)
        {
            return "目标：前往传送门，按 Y 进入第七关";
        }

        if (!tracker.IsTaskComplete(Level6TaskProgressTracker.MineOnceTaskId))
        {
            return "目标：选择石镐并采矿一次";
        }

        if (!tracker.IsTaskComplete(Level6TaskProgressTracker.ChopOnceTaskId))
        {
            return "目标：选择石斧并伐木一次";
        }

        if (!tracker.IsTaskComplete(Level6TaskProgressTracker.RepairSignalRelayTaskId))
        {
            return "目标：选择修理工具并修复信号中继器";
        }

        return "目标：前往传送门，按 Y 进入第七关";
    }

    void BuildUi()
    {
        var canvas = GetComponent<RectTransform>();
        if (canvas == null)
        {
            return;
        }

        var panelGo = new GameObject("Level6ObjectivePanel", typeof(RectTransform), typeof(Image));
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
