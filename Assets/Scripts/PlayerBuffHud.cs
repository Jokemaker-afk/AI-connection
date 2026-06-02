using UnityEngine;
using UnityEngine.UI;

public class PlayerBuffHud : MonoBehaviour
{
    const int MaxSlots = 6;
    const float SlotSize = 44f;
    const float SlotSpacing = 6f;

    [SerializeField] PlayerBuffController buffController;

    readonly ActiveBuffDisplay[] activeBuffBuffer = new ActiveBuffDisplay[MaxSlots];
    BuffSlotView[] slotViews;
    RectTransform buffBarRoot;

    void Awake()
    {
        BindController();
        BuildBuffBar();
    }

    void OnEnable()
    {
        BindController();
    }

    void Update()
    {
        RefreshSlots();
    }

    public void RebuildUi()
    {
        if (buffBarRoot != null)
        {
            GameplayUiUtility.DestroyForRebuild(buffBarRoot);
        }

        slotViews = null;
        buffBarRoot = null;
        BuildBuffBar();
    }

    public void BindTo(PlayerBuffController controller)
    {
        buffController = controller;
        RefreshSlots();
    }

    void BindController()
    {
        if (buffController == null)
        {
            buffController = FindFirstObjectByType<PlayerBuffController>();
        }
    }

    void BuildBuffBar()
    {
        if (transform.Find("BuffBarPanel") != null)
        {
            buffBarRoot = transform.Find("BuffBarPanel") as RectTransform;
            CacheSlotViews();
            return;
        }

        var panelGo = new GameObject("BuffBarPanel", typeof(RectTransform));
        panelGo.transform.SetParent(transform, false);

        buffBarRoot = panelGo.GetComponent<RectTransform>();
        buffBarRoot.anchorMin = new Vector2(0f, 1f);
        buffBarRoot.anchorMax = new Vector2(0f, 1f);
        buffBarRoot.pivot = new Vector2(0f, 1f);
        buffBarRoot.anchoredPosition = new Vector2(24f, -152f);
        buffBarRoot.sizeDelta = new Vector2(320f, SlotSize);

        slotViews = new BuffSlotView[MaxSlots];
        for (int i = 0; i < MaxSlots; i++)
        {
            slotViews[i] = BuffSlotView.Create(buffBarRoot, $"BuffSlot_{i}", i * (SlotSize + SlotSpacing), SlotSize);
        }
    }

    void CacheSlotViews()
    {
        if (buffBarRoot == null)
        {
            return;
        }

        slotViews = new BuffSlotView[MaxSlots];
        for (int i = 0; i < MaxSlots; i++)
        {
            var child = buffBarRoot.Find($"BuffSlot_{i}");
            if (child != null)
            {
                slotViews[i] = child.GetComponent<BuffSlotView>();
            }
        }
    }

    void RefreshSlots()
    {
        if (slotViews == null || buffController == null)
        {
            return;
        }

        int count = buffController.CollectActiveBuffs(activeBuffBuffer, MaxSlots);
        for (int i = 0; i < MaxSlots; i++)
        {
            if (i < count)
            {
                slotViews[i].Show(activeBuffBuffer[i]);
            }
            else
            {
                slotViews[i].Hide();
            }
        }
    }

    sealed class BuffSlotView : MonoBehaviour
    {
        Image backgroundImage;
        RectTransform durationLayerRect;
        Image durationLayerImage;
        Image durationEdgeImage;
        Image borderImage;
        Text labelText;

        Color baseAccentColor = Color.white;

        public static BuffSlotView Create(Transform parent, string name, float xOffset, float size)
        {
            var root = new GameObject(name, typeof(RectTransform));
            root.transform.SetParent(parent, false);

            var rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(xOffset, 0f);
            rect.sizeDelta = new Vector2(size, size);

            var view = root.AddComponent<BuffSlotView>();
            view.Build();
            view.Hide();
            return view;
        }

        void Build()
        {
            backgroundImage = CreateImage(transform, "Background", new Color(0.08f, 0.1f, 0.14f, 0.9f));
            StretchFull(backgroundImage.rectTransform);

            var layerGo = new GameObject("DurationLayer", typeof(RectTransform), typeof(Image));
            layerGo.transform.SetParent(transform, false);
            durationLayerRect = layerGo.GetComponent<RectTransform>();
            durationLayerRect.anchorMin = Vector2.zero;
            durationLayerRect.anchorMax = Vector2.one;
            durationLayerRect.pivot = new Vector2(0.5f, 0f);
            durationLayerRect.offsetMin = Vector2.zero;
            durationLayerRect.offsetMax = Vector2.zero;

            durationLayerImage = layerGo.GetComponent<Image>();
            durationLayerImage.raycastTarget = false;

            var edgeGo = new GameObject("DurationEdge", typeof(RectTransform), typeof(Image));
            edgeGo.transform.SetParent(layerGo.transform, false);
            durationEdgeImage = edgeGo.GetComponent<Image>();
            durationEdgeImage.raycastTarget = false;
            var edgeRect = durationEdgeImage.rectTransform;
            edgeRect.anchorMin = new Vector2(0f, 1f);
            edgeRect.anchorMax = new Vector2(1f, 1f);
            edgeRect.pivot = new Vector2(0.5f, 1f);
            edgeRect.anchoredPosition = Vector2.zero;
            edgeRect.sizeDelta = new Vector2(0f, 2f);

            borderImage = CreateImage(transform, "Border", new Color(1f, 1f, 1f, 0.18f));
            StretchFull(borderImage.rectTransform);

            var textGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(transform, false);
            StretchFull(textGo.GetComponent<RectTransform>());

            labelText = textGo.GetComponent<Text>();
            labelText.font = GetFont();
            labelText.fontSize = 15;
            labelText.fontStyle = FontStyle.Bold;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.color = new Color(1f, 1f, 1f, 0.95f);
            labelText.raycastTarget = false;
        }

        public void Show(ActiveBuffDisplay display)
        {
            float ratio = GetRemainingRatio(display);
            if (ratio <= 0f)
            {
                Hide();
                return;
            }

            gameObject.SetActive(true);

            baseAccentColor = GetBuffColor(display.Type);
            ApplyDurationLayer(ratio);
            borderImage.color = new Color(baseAccentColor.r, baseAccentColor.g, baseAccentColor.b, 0.35f + ratio * 0.45f);
            labelText.text = string.IsNullOrEmpty(display.OverrideLabel)
                ? GetBuffShortLabel(display.Type)
                : display.OverrideLabel;
            labelText.color = new Color(1f, 1f, 1f, 0.55f + ratio * 0.45f);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        static float GetRemainingRatio(ActiveBuffDisplay display)
        {
            if (display.UseCountdown && display.TotalSeconds > 0f)
            {
                return Mathf.Clamp01(display.RemainingSeconds / display.TotalSeconds);
            }

            return 1f;
        }

        void ApplyDurationLayer(float ratio)
        {
            ratio = Mathf.Clamp01(ratio);

            durationLayerRect.anchorMin = Vector2.zero;
            durationLayerRect.anchorMax = new Vector2(1f, ratio);
            durationLayerRect.offsetMin = Vector2.zero;
            durationLayerRect.offsetMax = Vector2.zero;

            float alpha = Mathf.Lerp(0f, baseAccentColor.a, ratio);
            durationLayerImage.color = new Color(baseAccentColor.r, baseAccentColor.g, baseAccentColor.b, alpha);

            durationEdgeImage.color = new Color(
                Mathf.Min(baseAccentColor.r + 0.25f, 1f),
                Mathf.Min(baseAccentColor.g + 0.25f, 1f),
                Mathf.Min(baseAccentColor.b + 0.25f, 1f),
                alpha > 0.01f ? 0.85f : 0f);
        }

        static string GetBuffShortLabel(BuffType type)
        {
            switch (type)
            {
                case BuffType.SpeedBoost:
                    return "速";
                case BuffType.InfiniteStamina:
                    return "精";
                case BuffType.Shield:
                    return "盾";
                case BuffType.Heal:
                    return "HP";
                default:
                    return "B";
            }
        }

        static Color GetBuffColor(BuffType type)
        {
            switch (type)
            {
                case BuffType.SpeedBoost:
                    return new Color(0.35f, 0.72f, 1f, 0.92f);
                case BuffType.InfiniteStamina:
                    return new Color(1f, 0.82f, 0.2f, 0.92f);
                case BuffType.Shield:
                    return new Color(0.65f, 0.5f, 1f, 0.92f);
                case BuffType.Heal:
                    return new Color(0.35f, 0.9f, 0.45f, 0.92f);
                default:
                    return new Color(0.7f, 0.7f, 0.7f, 0.92f);
            }
        }

        static Image CreateImage(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        static Font GetFont()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                font = Font.CreateDynamicFontFromOSFont("Arial", 16);
            }

            return font;
        }

        static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
