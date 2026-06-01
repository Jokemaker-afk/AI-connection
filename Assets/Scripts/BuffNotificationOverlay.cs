using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public static class BuffNotificationOverlay
{
    const float DisplayDuration = 3f;
    const float PanelAlpha = 0.1f;

    static GameObject root;
    static Text messageText;
    static Coroutine hideRoutine;
    static MonoBehaviour runner;

    public static void Show(BuffType buffType)
    {
        EnsureCreated();
        messageText.text = GetMessage(buffType);
        root.SetActive(true);

        if (hideRoutine != null && runner != null)
        {
            runner.StopCoroutine(hideRoutine);
        }

        hideRoutine = runner.StartCoroutine(HideAfterDelay());
    }

    public static void ShowCustom(string title, string detail)
    {
        EnsureCreated();
        messageText.text = $"<b>{title}</b>\n{detail}";
        root.SetActive(true);

        if (hideRoutine != null && runner != null)
        {
            runner.StopCoroutine(hideRoutine);
        }

        hideRoutine = runner.StartCoroutine(HideAfterDelay());
    }

    static string GetMessage(BuffType buffType)
    {
        switch (buffType)
        {
            case BuffType.Heal:
                return "获得 Buff：生命恢复\n生命值已回满";
            case BuffType.SpeedBoost:
                return "获得 Buff：加速\n移动速度提升 15 秒";
            case BuffType.InfiniteStamina:
                return "获得 Buff：无限精力\n7 秒内冲刺与跳跃不消耗精力";
            case BuffType.Shield:
                return "获得 Buff：护盾\n抵消下一次伤害，并在 1 秒内无敌";
            default:
                return "获得 Buff";
        }
    }

    static IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(DisplayDuration);
        Hide();
        hideRoutine = null;
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

        var runnerGo = new GameObject("BuffNotificationRunner");
        Object.DontDestroyOnLoad(runnerGo);
        runner = runnerGo.AddComponent<BuffNotificationRunner>();

        root = new GameObject("BuffNotificationOverlay");
        Object.DontDestroyOnLoad(root);

        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 450;
        var scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        root.AddComponent<GraphicRaycaster>();

        var panelGo = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panelGo.transform.SetParent(root.transform, false);
        var panelRect = panelGo.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(0f, 180f);
        panelGo.GetComponent<Image>().color = new Color(0.05f, 0.08f, 0.14f, PanelAlpha);
        panelGo.GetComponent<Image>().raycastTarget = false;

        var textGo = new GameObject("Message", typeof(RectTransform), typeof(Text));
        textGo.transform.SetParent(panelGo.transform, false);
        var textRect = textGo.GetComponent<RectTransform>();
        StretchFull(textRect);
        textRect.offsetMin = new Vector2(32f, 16f);
        textRect.offsetMax = new Vector2(-32f, -16f);

        messageText = textGo.GetComponent<Text>();
        messageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        messageText.fontSize = 34;
        messageText.alignment = TextAnchor.MiddleCenter;
        messageText.color = new Color(1f, 1f, 1f, 0.95f);
        messageText.supportRichText = true;
        messageText.raycastTarget = false;

        root.SetActive(false);
    }

    static void StretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    sealed class BuffNotificationRunner : MonoBehaviour
    {
    }
}
