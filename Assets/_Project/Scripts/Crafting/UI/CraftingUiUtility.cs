using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shared UI helpers for CraftingHud and CraftingRecipeHandbookHud.
/// </summary>
public static class CraftingUiUtility
{
    public static Text CreateText(Transform parent, string name, string content, int fontSize, TextAnchor alignment)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<Text>();
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        GameplayChineseText.PrepareUiText(text, content);
        return text;
    }

    public static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(72f, 36f);
        rect.anchoredPosition = anchoredPosition;

        var image = go.GetComponent<Image>();
        image.color = new Color(0.28f, 0.48f, 0.32f);

        var button = go.GetComponent<Button>();
        var colors = button.colors;
        colors.highlightedColor = new Color(0.36f, 0.58f, 0.4f);
        colors.pressedColor = new Color(0.22f, 0.38f, 0.26f);
        colors.disabledColor = new Color(0.22f, 0.22f, 0.24f);
        button.colors = colors;

        CreateText(go.transform, "Label", label, 13, TextAnchor.MiddleCenter);
        DisableSelectableNavigation(button);
        return button;
    }

    public static Scrollbar CreateScrollbar(Transform parent, string name, bool vertical)
    {
        var trackGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Scrollbar));
        trackGo.transform.SetParent(parent, false);
        var trackRect = trackGo.GetComponent<RectTransform>();
        if (vertical)
        {
            trackRect.anchorMin = new Vector2(1f, 0f);
            trackRect.anchorMax = new Vector2(1f, 1f);
            trackRect.pivot = new Vector2(1f, 1f);
            trackRect.sizeDelta = new Vector2(14f, 0f);
            trackRect.anchoredPosition = Vector2.zero;
        }
        else
        {
            trackRect.anchorMin = new Vector2(0f, 0f);
            trackRect.anchorMax = new Vector2(1f, 0f);
            trackRect.pivot = new Vector2(0f, 0f);
            trackRect.sizeDelta = new Vector2(0f, 14f);
            trackRect.anchoredPosition = Vector2.zero;
        }

        trackGo.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.18f, 0.95f);

        var handleGo = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handleGo.transform.SetParent(trackGo.transform, false);
        var handleRect = handleGo.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(10f, 10f);
        var handleImage = handleGo.GetComponent<Image>();
        handleImage.color = new Color(0.5f, 0.58f, 0.66f, 0.95f);

        var scrollbar = trackGo.GetComponent<Scrollbar>();
        scrollbar.handleRect = handleRect;
        scrollbar.targetGraphic = handleImage;
        scrollbar.direction = vertical ? Scrollbar.Direction.BottomToTop : Scrollbar.Direction.LeftToRight;
        DisableSelectableNavigation(scrollbar);
        return scrollbar;
    }

    public static void DisableSelectableNavigation(Selectable selectable)
    {
        if (selectable == null)
        {
            return;
        }

        Navigation navigation = selectable.navigation;
        navigation.mode = Navigation.Mode.None;
        selectable.navigation = navigation;
    }
}
