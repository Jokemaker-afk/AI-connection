using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayCrosshairHud : MonoBehaviour
{
    public const string CrosshairRootName = "Crosshair";
    const string CrosshairPrefabResourcePath = "UI/PF_Gameplay_Crosshair_01";

    [SerializeField] bool manualHidden;

    RectTransform crosshairRoot;
    PlayerCameraController cameraController;
    int duplicatesRemovedLastEnsure;

    public RectTransform CrosshairRectTransform => crosshairRoot;
    public bool IsCrosshairVisible => crosshairRoot != null && crosshairRoot.gameObject.activeSelf;

    public void EnsureCrosshairExists()
    {
        EnsureCanvasOnRoot();
        duplicatesRemovedLastEnsure = RemoveDuplicateCrosshairRoots();

        if (crosshairRoot == null)
        {
            Transform existing = transform.Find(CrosshairRootName);
            if (existing is RectTransform existingRect)
            {
                crosshairRoot = existingRect;
            }
        }

        if (crosshairRoot == null)
        {
            if (!TryInstantiateCrosshairPrefab())
            {
                BuildUi();
            }
        }

        EnsureCrosshairLayout();
        crosshairRoot?.SetAsLastSibling();
        RefreshCrosshairVisibility();
    }

    public void RebuildUi()
    {
        if (crosshairRoot != null)
        {
            Destroy(crosshairRoot.gameObject);
            crosshairRoot = null;
        }

        EnsureCanvasOnRoot();
        if (!TryInstantiateCrosshairPrefab())
        {
            BuildUi();
        }

        EnsureCrosshairLayout();
        crosshairRoot?.SetAsLastSibling();
        RefreshCrosshairVisibility();
    }

    public void ShowCrosshair()
    {
        manualHidden = false;
        RefreshCrosshairVisibility();
    }

    public void HideCrosshair()
    {
        manualHidden = true;
        RefreshCrosshairVisibility();
    }

    public void SetCrosshairVisible(bool visible)
    {
        manualHidden = !visible;
        RefreshCrosshairVisibility();
    }

    public void RefreshForCurrentUIState()
    {
        RefreshCrosshairVisibility();
    }

    public void RefreshCrosshairVisibility()
    {
        if (crosshairRoot == null)
        {
            return;
        }

        bool visible = ShouldShowCrosshair();
        crosshairRoot.gameObject.SetActive(visible);
        if (visible)
        {
            crosshairRoot.anchoredPosition = Vector2.zero;
        }
    }

    void Start()
    {
        cameraController = FindFirstObjectByType<PlayerCameraController>();
        EnsureCrosshairExists();
    }

    void Update()
    {
        if (cameraController == null)
        {
            cameraController = FindFirstObjectByType<PlayerCameraController>();
        }

        RefreshCrosshairVisibility();
    }

    bool ShouldShowCrosshair()
    {
        if (manualHidden)
        {
            return false;
        }

        if (!GameplayCrosshairController.IsPlayerGameplayActive())
        {
            return false;
        }

        if (GameplayCursorPolicy.IsAnyMenuOpen)
        {
            return false;
        }

        return true;
    }

    void EnsureCanvasOnRoot()
    {
        var canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            var hud = GetComponent<PlayerHUD>();
            if (hud != null)
            {
                hud.RebuildUi();
            }

            canvas = GetComponent<Canvas>();
        }

        if (canvas != null && canvas.sortingOrder < 10)
        {
            canvas.sortingOrder = 10;
        }
    }

    int RemoveDuplicateCrosshairRoots()
    {
        var roots = new List<Transform>();
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.name == CrosshairRootName)
            {
                roots.Add(child);
            }
        }

        if (roots.Count == 0)
        {
            crosshairRoot = null;
            return 0;
        }

        crosshairRoot = roots[0] as RectTransform;
        int removed = 0;
        for (int i = 1; i < roots.Count; i++)
        {
            Destroy(roots[i].gameObject);
            removed++;
        }

        return removed;
    }

    void EnsureCrosshairLayout()
    {
        if (crosshairRoot == null)
        {
            return;
        }

        crosshairRoot.anchorMin = new Vector2(0.5f, 0.5f);
        crosshairRoot.anchorMax = new Vector2(0.5f, 0.5f);
        crosshairRoot.pivot = new Vector2(0.5f, 0.5f);
        crosshairRoot.anchoredPosition = Vector2.zero;

        ApplyCrosshairGraphics(crosshairRoot);
        DisableRaycastTargets(crosshairRoot);
    }

    static void ApplyCrosshairGraphics(Transform root)
    {
        var images = root.GetComponentsInChildren<Image>(true);
        for (int i = 0; i < images.Length; i++)
        {
            GameplayUiSpriteUtility.ApplyWhiteSprite(images[i]);
        }
    }

    static void DisableRaycastTargets(Transform root)
    {
        var images = root.GetComponentsInChildren<Image>(true);
        for (int i = 0; i < images.Length; i++)
        {
            images[i].raycastTarget = false;
        }
    }

    bool TryInstantiateCrosshairPrefab()
    {
        GameObject prefab = Resources.Load<GameObject>(CrosshairPrefabResourcePath);
        if (prefab == null)
        {
            return false;
        }

        GameObject instance = Instantiate(prefab, transform, false);
        Transform crosshairChild = instance.transform.Find(CrosshairRootName);
        if (crosshairChild is RectTransform childRect)
        {
            childRect.SetParent(transform, false);
            crosshairRoot = childRect;
            Destroy(instance);
        }
        else
        {
            crosshairRoot = instance.GetComponent<RectTransform>();
            if (crosshairRoot == null)
            {
                crosshairRoot = instance.AddComponent<RectTransform>();
            }

            instance.name = CrosshairRootName;
        }

        return crosshairRoot != null;
    }

    void BuildUi()
    {
        RectTransform canvas = GetComponent<RectTransform>();
        if (canvas == null)
        {
            return;
        }

        crosshairRoot = GameplayCrosshairBuilder.CreateCrosshairRoot(canvas);
    }

    public int ConsumeDuplicatesRemovedCount()
    {
        int count = duplicatesRemovedLastEnsure;
        duplicatesRemovedLastEnsure = 0;
        return count;
    }
}

public static class GameplayCrosshairBuilder
{
    public static RectTransform CreateCrosshairRoot(Transform parent)
    {
        var rootGo = new GameObject(GameplayCrosshairHud.CrosshairRootName, typeof(RectTransform));
        rootGo.transform.SetParent(parent, false);
        var crosshairRoot = rootGo.GetComponent<RectTransform>();
        crosshairRoot.anchorMin = new Vector2(0.5f, 0.5f);
        crosshairRoot.anchorMax = new Vector2(0.5f, 0.5f);
        crosshairRoot.pivot = new Vector2(0.5f, 0.5f);
        crosshairRoot.anchoredPosition = Vector2.zero;
        crosshairRoot.sizeDelta = new Vector2(24f, 24f);

        Color lineColor = new Color(0.95f, 0.95f, 0.95f, 0.95f);
        Color shadowColor = new Color(0f, 0f, 0f, 0.55f);
        const float armLength = 10f;
        const float armThickness = 2f;
        Vector2 shadowOffset = new Vector2(1f, -1f);

        CreateCrosshairPart(rootGo.transform, "VerticalShadow", shadowOffset, new Vector2(armThickness, armLength * 2f), shadowColor);
        CreateCrosshairPart(rootGo.transform, "HorizontalShadow", shadowOffset, new Vector2(armLength * 2f, armThickness), shadowColor);
        CreateCrosshairPart(rootGo.transform, "Vertical", Vector2.zero, new Vector2(armThickness, armLength * 2f), lineColor);
        CreateCrosshairPart(rootGo.transform, "Horizontal", Vector2.zero, new Vector2(armLength * 2f, armThickness), lineColor);
        CreateCrosshairPart(rootGo.transform, "CenterDot", Vector2.zero, new Vector2(2f, 2f), lineColor);

        return crosshairRoot;
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
        GameplayUiSpriteUtility.ApplyWhiteSprite(image);
        return image;
    }
}
