using UnityEngine;
using UnityEngine.UI;

public static class ItemWorldLabel
{
    public static GameObject Create(Transform parent, string text, Vector3 localPosition, float characterSize = 0.07f)
    {
        var labelGo = new GameObject("Label");
        labelGo.transform.SetParent(parent, false);
        labelGo.transform.localPosition = localPosition;
        labelGo.transform.localRotation = Quaternion.identity;

        EnsureCanvasLabel(labelGo, characterSize);
        Refresh(labelGo, text);

        if (labelGo.GetComponent<BillboardLabel>() == null)
        {
            labelGo.AddComponent<BillboardLabel>();
        }

        return labelGo;
    }

    public static void Refresh(GameObject labelRoot, string text)
    {
        if (labelRoot == null)
        {
            return;
        }

        var uiText = labelRoot.GetComponentInChildren<Text>();
        if (uiText != null)
        {
            GameplayChineseText.ApplyWorldLabel(uiText, text, uiText.fontSize > 0 ? uiText.fontSize : 40);
            return;
        }

        var textMesh = labelRoot.GetComponent<TextMesh>();
        if (textMesh != null)
        {
            float legacyCharacterSize = textMesh.characterSize > 0f ? textMesh.characterSize : 0.07f;
            Object.Destroy(textMesh);
            var legacyRenderer = labelRoot.GetComponent<MeshRenderer>();
            if (legacyRenderer != null)
            {
                Object.Destroy(legacyRenderer);
            }

            EnsureCanvasLabel(labelRoot, legacyCharacterSize);
            uiText = labelRoot.GetComponentInChildren<Text>();
            if (uiText != null)
            {
                GameplayChineseText.ApplyWorldLabel(uiText, text, 32);
            }
        }
    }

    static void EnsureCanvasLabel(GameObject labelRoot, float characterSize)
    {
        var canvas = labelRoot.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = labelRoot.AddComponent<Canvas>();
        }

        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 20;
        canvas.overrideSorting = true;

        var rect = canvas.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(420f, 80f);
        // Keep world-space UI text close to old TextMesh visual size.
        float scale = Mathf.Max(0.0001f, characterSize * 0.18f);
        rect.localScale = new Vector3(scale, scale, scale);

        var uiText = labelRoot.GetComponentInChildren<Text>();
        if (uiText != null)
        {
            return;
        }

        var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
        textGo.transform.SetParent(labelRoot.transform, false);
        var textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        uiText = textGo.GetComponent<Text>();
        GameplayChineseText.ApplyWorldLabel(uiText, string.Empty, 40);
    }

    public static void UpgradeLegacyLabels()
    {
        var allTextMeshes = Object.FindObjectsByType<TextMesh>(FindObjectsSortMode.None);
        for (int i = 0; i < allTextMeshes.Length; i++)
        {
            var textMesh = allTextMeshes[i];
            if (textMesh == null || textMesh.gameObject.name != "Label")
            {
                continue;
            }

            Refresh(textMesh.gameObject, textMesh.text);
        }
    }
}
