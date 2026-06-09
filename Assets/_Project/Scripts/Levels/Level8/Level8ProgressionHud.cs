using UnityEngine;
using UnityEngine.UI;

public class Level8ProgressionHud : MonoBehaviour, ILevelObjectiveHudPanel
{
    RectTransform panelRoot;
    Text objectiveText;

    public int ObjectiveLevel => 8;

    void Awake()
    {
        BuildUi();
    }

    void OnEnable()
    {
        Subscribe();
        RefreshObjective();
    }

    void OnDisable()
    {
        Unsubscribe();
    }

    void Subscribe()
    {
        Level8ObjectiveTracker tracker = Level8ObjectiveTracker.Instance;
        if (tracker != null)
        {
            tracker.OnProgressChanged -= RefreshObjective;
            tracker.OnProgressChanged += RefreshObjective;
        }
    }

    void Unsubscribe()
    {
        Level8ObjectiveTracker tracker = Level8ObjectiveTracker.Instance;
        if (tracker != null)
        {
            tracker.OnProgressChanged -= RefreshObjective;
        }
    }

    public void ResetForSceneTransition()
    {
        Unsubscribe();
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
        Subscribe();
        RefreshObjective();
    }

    void RefreshObjective()
    {
        if (objectiveText == null)
        {
            return;
        }

        Level8ObjectiveTracker tracker = Level8ObjectiveTracker.Instance;
        if (tracker == null)
        {
            panelRoot.gameObject.SetActive(false);
            return;
        }

        string text = tracker.GetCurrentObjectiveText();
        GameplayChineseText.PrepareUiText(objectiveText, text);
        panelRoot.gameObject.SetActive(!string.IsNullOrEmpty(text));
    }

    void BuildUi()
    {
        var canvas = GetComponent<RectTransform>();
        if (canvas == null)
        {
            return;
        }

        var panelGo = new GameObject("Level8ObjectivePanel", typeof(RectTransform), typeof(Image));
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
