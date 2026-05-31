using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] PlayerStats stats;
    [SerializeField] GameScore gameScore;
    [SerializeField] Slider healthSlider;
    [SerializeField] Slider staminaSlider;
    [SerializeField] Text scoreText;
    [SerializeField] bool buildUiIfMissing = true;

    void Awake()
    {
        BindReferences();

        if (buildUiIfMissing && NeedsBuildUi())
        {
            BuildRuntimeUI();
        }

        ConfigureSlider(healthSlider);
        ConfigureSlider(staminaSlider);
        SubscribeScore();
    }

    void OnDestroy()
    {
        UnsubscribeScore();
    }

    void LateUpdate()
    {
        var rect = GetComponent<RectTransform>();
        if (rect != null && rect.localScale.sqrMagnitude < 0.01f)
        {
            rect.localScale = Vector3.one;
        }
    }

    void Update()
    {
        if (stats == null)
        {
            return;
        }

        if (healthSlider != null)
        {
            healthSlider.value = stats.HealthNormalized;
        }

        if (staminaSlider != null)
        {
            staminaSlider.value = stats.StaminaNormalized;
        }
    }

    void BindReferences()
    {
        if (stats == null)
        {
            stats = FindFirstObjectByType<PlayerStats>();
        }

        if (gameScore == null)
        {
            gameScore = FindFirstObjectByType<GameScore>();
        }
    }

    void SubscribeScore()
    {
        if (gameScore == null)
        {
            return;
        }

        gameScore.OnScoreChanged += HandleScoreChanged;
        HandleScoreChanged(gameScore.Score);
    }

    void UnsubscribeScore()
    {
        if (gameScore != null)
        {
            gameScore.OnScoreChanged -= HandleScoreChanged;
        }
    }

    void HandleScoreChanged(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }

    static void ConfigureSlider(Slider slider)
    {
        if (slider == null)
        {
            return;
        }

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.interactable = false;
    }

    public void RebuildUi()
    {
        UnsubscribeScore();

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i);
            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }

        healthSlider = null;
        staminaSlider = null;
        scoreText = null;
        BuildRuntimeUI();
        ConfigureSlider(healthSlider);
        ConfigureSlider(staminaSlider);

        BindReferences();
        SubscribeScore();

        var rect = GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.localScale = Vector3.one;
        }
    }

    bool NeedsBuildUi()
    {
        return transform.Find("StatsPanel") == null
            || healthSlider == null
            || staminaSlider == null
            || scoreText == null;
    }

    void BuildRuntimeUI()
    {
        if (transform.Find("StatsPanel") != null)
        {
            return;
        }

        var canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            gameObject.AddComponent<GraphicRaycaster>();
        }

        var panel = CreatePanel(transform, "StatsPanel", new Vector2(24f, -24f), new Vector2(320f, 120f));
        healthSlider = CreateBar(panel, "HealthBar", new Vector2(0f, -8f), new Color(0.85f, 0.2f, 0.2f), "HP");
        staminaSlider = CreateBar(panel, "StaminaBar", new Vector2(0f, -48f), new Color(0.95f, 0.78f, 0.15f), "SP");
        scoreText = CreateScoreLabel(panel, "ScoreLabel", new Vector2(8f, -88f));
    }

    static Font GetUIFont()
    {
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null)
        {
            font = Font.CreateDynamicFontFromOSFont("Arial", 16);
        }

        return font;
    }

    static Text CreateScoreLabel(Transform parent, string name, Vector2 anchoredPos)
    {
        var labelGo = new GameObject(name, typeof(RectTransform), typeof(Text));
        labelGo.transform.SetParent(parent, false);

        var labelRect = labelGo.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 1f);
        labelRect.anchorMax = new Vector2(0f, 1f);
        labelRect.pivot = new Vector2(0f, 1f);
        labelRect.anchoredPosition = anchoredPos;
        labelRect.sizeDelta = new Vector2(280f, 24f);

        var label = labelGo.GetComponent<Text>();
        label.font = GetUIFont();
        label.text = "Score: 0";
        label.fontSize = 18;
        label.color = Color.white;
        label.alignment = TextAnchor.MiddleLeft;
        label.raycastTarget = false;
        return label;
    }

    static RectTransform CreatePanel(Transform parent, string name, Vector2 anchoredPos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);

        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = size;

        var image = go.GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.35f);
        image.raycastTarget = false;
        return rect;
    }

    static Slider CreateBar(Transform parent, string name, Vector2 anchoredPos, Color fillColor, string labelText)
    {
        var root = new GameObject(name, typeof(RectTransform));
        root.transform.SetParent(parent, false);

        var rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0f, 1f);
        rootRect.anchorMax = new Vector2(0f, 1f);
        rootRect.pivot = new Vector2(0f, 1f);
        rootRect.anchoredPosition = anchoredPos;
        rootRect.sizeDelta = new Vector2(280f, 28f);

        var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
        labelGo.transform.SetParent(root.transform, false);
        var labelRect = labelGo.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0f, 0.5f);
        labelRect.anchorMax = new Vector2(0f, 0.5f);
        labelRect.pivot = new Vector2(0f, 0.5f);
        labelRect.anchoredPosition = new Vector2(8f, 0f);
        labelRect.sizeDelta = new Vector2(36f, 24f);
        var label = labelGo.GetComponent<Text>();
        label.font = GetUIFont();
        label.text = labelText;
        label.fontSize = 16;
        label.color = Color.white;
        label.alignment = TextAnchor.MiddleLeft;
        label.raycastTarget = false;

        var sliderGo = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
        sliderGo.transform.SetParent(root.transform, false);
        var sliderRect = sliderGo.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0f, 0.5f);
        sliderRect.anchorMax = new Vector2(1f, 0.5f);
        sliderRect.pivot = new Vector2(0.5f, 0.5f);
        sliderRect.anchoredPosition = new Vector2(24f, 0f);
        sliderRect.sizeDelta = new Vector2(-56f, 18f);

        var background = CreateImage(sliderGo.transform, "Background", new Color(0.15f, 0.15f, 0.15f, 0.9f));
        StretchFull(background.GetComponent<RectTransform>());

        var fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderGo.transform, false);
        StretchFull(fillArea.GetComponent<RectTransform>());

        var fill = CreateImage(fillArea.transform, "Fill", fillColor);
        StretchFull(fill.GetComponent<RectTransform>());

        var slider = sliderGo.GetComponent<Slider>();
        slider.targetGraphic = fill.GetComponent<Image>();
        slider.fillRect = fill.GetComponent<RectTransform>();
        slider.direction = Slider.Direction.LeftToRight;
        slider.transition = Selectable.Transition.None;
        slider.interactable = false;
        return slider;
    }

    static GameObject CreateImage(Transform parent, string name, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = color;
        go.GetComponent<Image>().raycastTarget = false;
        return go;
    }

    static void StretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
