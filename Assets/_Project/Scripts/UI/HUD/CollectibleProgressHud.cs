using UnityEngine;
using UnityEngine.UI;

public class CollectibleProgressHud : MonoBehaviour
{
    [SerializeField] CollectibleManager collectibleManager;

    RectTransform panelRoot;
    Text progressText;

    void Awake()
    {
        if (collectibleManager == null)
        {
            collectibleManager = FindFirstObjectByType<CollectibleManager>();
        }

        BuildUi();
    }

    void OnEnable()
    {
        if (collectibleManager == null)
        {
            collectibleManager = FindFirstObjectByType<CollectibleManager>();
        }

        Subscribe();
        Refresh();
    }

    void OnDisable()
    {
        Unsubscribe();
    }

    public void BindTo(CollectibleManager manager)
    {
        Unsubscribe();
        collectibleManager = manager;
        Subscribe();
        Refresh();
    }

    void Subscribe()
    {
        if (collectibleManager == null)
        {
            return;
        }

        collectibleManager.OnProgressChanged -= HandleProgressChanged;
        collectibleManager.OnProgressChanged += HandleProgressChanged;
    }

    void Unsubscribe()
    {
        if (collectibleManager == null)
        {
            return;
        }

        collectibleManager.OnProgressChanged -= HandleProgressChanged;
    }

    void HandleProgressChanged(int collected, int total, float percentage)
    {
        Refresh(collected, total, percentage);
    }

    void Refresh()
    {
        if (collectibleManager == null)
        {
            return;
        }

        Refresh(
            collectibleManager.CollectedCount,
            collectibleManager.TotalCollectibleCount,
            collectibleManager.CollectedPercentage);
    }

    void Refresh(int collected, int total, float percentage)
    {
        if (progressText == null)
        {
            return;
        }

        int unlockPercent = Mathf.RoundToInt(collectibleManager != null ? collectibleManager.UnlockThreshold * 100f : 30f);
        progressText.text =
            $"已收集：{collected} / {total}\n传送门解锁进度：{Mathf.RoundToInt(percentage * 100f)}% / {unlockPercent}%";
    }

    void BuildUi()
    {
        if (transform.Find("CollectibleProgressPanel") != null)
        {
            panelRoot = transform.Find("CollectibleProgressPanel") as RectTransform;
            progressText = panelRoot.GetComponentInChildren<Text>();
            return;
        }

        var panelGo = new GameObject("CollectibleProgressPanel", typeof(RectTransform), typeof(Image));
        panelGo.transform.SetParent(transform, false);

        panelRoot = panelGo.GetComponent<RectTransform>();
        panelRoot.anchorMin = new Vector2(0f, 1f);
        panelRoot.anchorMax = new Vector2(0f, 1f);
        panelRoot.pivot = new Vector2(0f, 1f);
        panelRoot.anchoredPosition = new Vector2(24f, -152f);
        panelRoot.sizeDelta = new Vector2(320f, 56f);

        var image = panelGo.GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.35f);
        image.raycastTarget = false;

        var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
        textGo.transform.SetParent(panelGo.transform, false);
        var textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10f, 6f);
        textRect.offsetMax = new Vector2(-10f, -6f);

        progressText = textGo.GetComponent<Text>();
        progressText.font = GetFont();
        progressText.fontSize = 16;
        progressText.alignment = TextAnchor.UpperLeft;
        progressText.color = Color.white;
        progressText.raycastTarget = false;
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
}
