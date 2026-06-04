using UnityEngine;
using UnityEngine.UI;

public class GameplayCrosshairHud : MonoBehaviour
{
    RectTransform crosshairRoot;
    PlayerCameraController cameraController;

    public RectTransform CrosshairRectTransform => crosshairRoot;

    public void RebuildUi()
    {
        if (crosshairRoot != null)
        {
            Destroy(crosshairRoot.gameObject);
        }

        BuildUi();
    }

    void Start()
    {
        cameraController = FindFirstObjectByType<PlayerCameraController>();
        if (crosshairRoot == null)
        {
            BuildUi();
        }
    }

    void Update()
    {
        if (cameraController == null)
        {
            cameraController = FindFirstObjectByType<PlayerCameraController>();
        }

        bool visible = ShouldShowCrosshair();
        if (crosshairRoot != null)
        {
            crosshairRoot.gameObject.SetActive(visible);
            ApplyCrosshairOffset();
        }
    }

    void ApplyCrosshairOffset()
    {
        if (crosshairRoot == null || cameraController == null)
        {
            return;
        }

        crosshairRoot.anchoredPosition = cameraController.CrosshairScreenOffset;
    }

    bool ShouldShowCrosshair()
    {
        if (GameplayCursorPolicy.IsAnyMenuOpen)
        {
            return false;
        }

        if (GameplayCore.Exists && !GameplayCore.Instance.Abilities.HasAbility(GameplayAbility.CrosshairTargeting))
        {
            return false;
        }

        return true;
    }

    void BuildUi()
    {
        var canvas = GetComponent<RectTransform>();
        if (canvas == null)
        {
            return;
        }

        var rootGo = new GameObject("Crosshair", typeof(RectTransform));
        rootGo.transform.SetParent(canvas, false);
        crosshairRoot = rootGo.GetComponent<RectTransform>();
        crosshairRoot.anchorMin = new Vector2(0.5f, 0.5f);
        crosshairRoot.anchorMax = new Vector2(0.5f, 0.5f);
        crosshairRoot.pivot = new Vector2(0.5f, 0.5f);
        crosshairRoot.anchoredPosition = Vector2.zero;
        crosshairRoot.sizeDelta = new Vector2(24f, 24f);

        Color lineColor = new Color(1f, 1f, 1f, 0.92f);
        const float armLength = 10f;
        const float armThickness = 2f;

        CreateCrosshairPart(rootGo.transform, "Vertical", Vector2.zero, new Vector2(armThickness, armLength * 2f), lineColor);
        CreateCrosshairPart(rootGo.transform, "Horizontal", Vector2.zero, new Vector2(armLength * 2f, armThickness), lineColor);
    }

    static Image CreateCrosshairPart(Transform parent, string name, Vector2 position, Vector2 size, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        var image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }
}
