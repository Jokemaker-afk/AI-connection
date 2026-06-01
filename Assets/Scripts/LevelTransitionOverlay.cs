using UnityEngine;
using UnityEngine.UI;

public static class LevelTransitionOverlay
{
    static GameObject root;
    static Text messageText;

    public static void Show(string message)
    {
        EnsureCreated();
        messageText.fontSize = 42;
        messageText.text = message;
        root.SetActive(true);
    }

    public static void ShowScoreAdvancePrompt(string title, int currentScore, int targetScore, string hint)
    {
        EnsureCreated();
        messageText.fontSize = 38;
        messageText.text = $"{title}\n\n当前分数: {currentScore} / {targetScore}\n\n<size=30><color=#FFE066>{hint}</color></size>";
        root.SetActive(true);
    }

    public static void ShowAdvancePrompt(string title, string hint)
    {
        EnsureCreated();
        messageText.fontSize = 36;
        messageText.text = $"{title}\n\n<size=30><color=#FFE066>{hint}</color></size>";
        root.SetActive(true);
    }

    public static void Hide()
    {
        if (root != null)
        {
            root.SetActive(false);
        }
    }

    static void EnsureCreated()
    {
        if (root != null)
        {
            return;
        }

        root = new GameObject("LevelTransitionOverlay");
        Object.DontDestroyOnLoad(root);

        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;
        var scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        root.AddComponent<GraphicRaycaster>();

        var panelGo = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panelGo.transform.SetParent(root.transform, false);
        var panelRect = panelGo.GetComponent<RectTransform>();
        StretchFull(panelRect);
        panelGo.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.65f);

        var windowGo = new GameObject("Window", typeof(RectTransform), typeof(Image));
        windowGo.transform.SetParent(panelGo.transform, false);
        var windowRect = windowGo.GetComponent<RectTransform>();
        windowRect.anchorMin = new Vector2(0.5f, 0.5f);
        windowRect.anchorMax = new Vector2(0.5f, 0.5f);
        windowRect.pivot = new Vector2(0.5f, 0.5f);
        windowRect.sizeDelta = new Vector2(560f, 280f);
        windowGo.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.18f, 0.95f);

        var textGo = new GameObject("Message", typeof(RectTransform), typeof(Text));
        textGo.transform.SetParent(windowGo.transform, false);
        var textRect = textGo.GetComponent<RectTransform>();
        StretchFull(textRect);
        textRect.offsetMin = new Vector2(24f, 24f);
        textRect.offsetMax = new Vector2(-24f, -24f);

        messageText = textGo.GetComponent<Text>();
        messageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        messageText.fontSize = 38;
        messageText.alignment = TextAnchor.MiddleCenter;
        messageText.color = Color.white;
        messageText.supportRichText = true;
        messageText.text = string.Empty;

        root.SetActive(false);
    }

    static void StretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
