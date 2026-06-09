using UnityEngine;
using UnityEngine.UI;

public class SceneProgressionHud : MonoBehaviour, ILevelObjectiveHudPanel
{
    [SerializeField] SceneProgressionManager progressionManager;

    RectTransform panelRoot;
    Text objectiveText;

    public int ObjectiveLevel => 5;

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

    public void BindTo(SceneProgressionManager manager)
    {
        Unsubscribe();
        progressionManager = manager;
        Subscribe();
        RefreshObjectiveDisplay();
    }

    public void ResetForSceneTransition()
    {
        Unsubscribe();
        progressionManager = null;
        if (objectiveText != null)
        {
            GameplayChineseText.PrepareUiText(objectiveText, string.Empty);
        }

        if (panelRoot != null)
        {
            panelRoot.gameObject.SetActive(false);
        }
    }

    public void RefreshObjectiveDisplay()
    {
        RefreshObjective();
    }

    void BindManager()
    {
        if (progressionManager == null)
        {
            progressionManager = FindFirstObjectByType<SceneProgressionManager>();
        }
    }

    void Subscribe()
    {
        if (progressionManager == null)
        {
            return;
        }

        RequiredPlacedObjectTracker tracker = RequiredPlacedObjectTracker.Instance;
        if (tracker != null)
        {
            tracker.OnProgressChanged -= RefreshObjective;
            tracker.OnProgressChanged += RefreshObjective;
        }
        if (progressionManager != null)
        {
            progressionManager.OnBeaconActivated -= RefreshObjective;
            progressionManager.OnBeaconActivated += RefreshObjective;
        }
    }

    void Unsubscribe()
    {
        RequiredPlacedObjectTracker tracker = RequiredPlacedObjectTracker.Instance;
        if (tracker != null)
        {
            tracker.OnProgressChanged -= RefreshObjective;
        }

        if (progressionManager != null)
        {
            progressionManager.OnBeaconActivated -= RefreshObjective;
        }
    }

    void RefreshObjective()
    {
        if (objectiveText == null)
        {
            return;
        }

        if (progressionManager == null)
        {
            panelRoot.gameObject.SetActive(false);
            return;
        }

        string text = BuildObjectiveText(progressionManager);
        GameplayChineseText.PrepareUiText(objectiveText, text);
        panelRoot.gameObject.SetActive(!string.IsNullOrEmpty(text));
    }

    static string BuildObjectiveText(SceneProgressionManager manager)
    {
        if (manager == null)
        {
            return string.Empty;
        }

        if (manager.IsBeaconActivated)
        {
            return "目标：前往已激活的信标进入下一关";
        }

        bool hasWorkbench = manager.HasPlaced(ItemKind.CraftingTable);
        bool hasFurnace = manager.HasPlaced(ItemKind.Furnace);

        if (!hasWorkbench)
        {
            return "目标：制造并放置工作台";
        }

        if (!hasFurnace)
        {
            return "目标：使用工作台制造并放置熔炉";
        }

        return "目标：前往已激活的信标进入下一关";
    }

    void BuildUi()
    {
        var canvas = GetComponent<RectTransform>();
        if (canvas == null)
        {
            return;
        }

        var panelGo = new GameObject("SceneObjectivePanel", typeof(RectTransform), typeof(Image));
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
