using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Read-only recipe codex (图鉴) shown from the backpack UI. Uses CraftingRecipeDatabase as the single data source.
/// </summary>
public class CraftingRecipeHandbookHud : MonoBehaviour
{
    const float PanelWidth = 380f;
    const float RowMinHeight = 88f;

    PlayerInventory inventory;
    PlayerInventoryHud ownerHud;
    CraftingRecipeCategory? selectedCategoryFilter;

    RectTransform panelRoot;
    RectTransform categoryBarRoot;
    RectTransform listContentRoot;
    ScrollRect listScrollRect;
    RectTransform tabButtonRect;
    readonly List<HandbookRowView> rows = new List<HandbookRowView>();

    public bool IsOpen { get; private set; }
    public float HandbookWidth => PanelWidth;

    public void EnsureBuilt(Transform windowParent, PlayerInventoryHud hud)
    {
        ownerHud = hud;
        if (panelRoot != null)
        {
            return;
        }

        BuildUi(windowParent);
    }

    public void BindInventory(PlayerInventory playerInventory)
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= RefreshAll;
        }

        inventory = playerInventory;
        if (inventory != null)
        {
            inventory.OnInventoryChanged += RefreshAll;
        }

        RefreshAll();
    }

    public void SetOpen(bool open)
    {
        if (IsOpen == open)
        {
            return;
        }

        IsOpen = open;
        if (panelRoot != null)
        {
            panelRoot.gameObject.SetActive(open);
        }

        if (tabButtonRect != null)
        {
            var tabImage = tabButtonRect.GetComponent<Image>();
            if (tabImage != null)
            {
                tabImage.color = open
                    ? new Color(0.34f, 0.52f, 0.38f)
                    : new Color(0.22f, 0.26f, 0.32f);
            }
        }

        ownerHud?.NotifyHandbookLayoutChanged();
        if (open)
        {
            RebuildList();
            RefreshAll();
        }
    }

    public void Toggle()
    {
        SetOpen(!IsOpen);
    }

    void BuildUi(Transform windowParent)
    {
        var tabGo = new GameObject("HandbookTab", typeof(RectTransform), typeof(Image), typeof(Button));
        tabGo.transform.SetParent(windowParent, false);
        tabButtonRect = tabGo.GetComponent<RectTransform>();
        tabButtonRect.anchorMin = new Vector2(0f, 0.5f);
        tabButtonRect.anchorMax = new Vector2(0f, 0.5f);
        tabButtonRect.pivot = new Vector2(1f, 0.5f);
        tabButtonRect.anchoredPosition = new Vector2(-8f, 0f);
        tabButtonRect.sizeDelta = new Vector2(34f, 120f);

        var tabImage = tabGo.GetComponent<Image>();
        tabImage.color = new Color(0.22f, 0.26f, 0.32f);

        var tabButton = tabGo.GetComponent<Button>();
        tabButton.onClick.AddListener(Toggle);
        CraftingUiUtility.DisableSelectableNavigation(tabButton);

        var tabLabel = CraftingUiUtility.CreateText(tabGo.transform, "Label", "制作\n图鉴", 14, TextAnchor.MiddleCenter);
        tabLabel.rectTransform.offsetMin = Vector2.zero;
        tabLabel.rectTransform.offsetMax = Vector2.zero;

        var panelGo = new GameObject("HandbookPanel", typeof(RectTransform), typeof(Image));
        panelGo.transform.SetParent(windowParent, false);
        panelRoot = panelGo.GetComponent<RectTransform>();
        panelRoot.anchorMin = new Vector2(0f, 0f);
        panelRoot.anchorMax = new Vector2(0f, 1f);
        panelRoot.pivot = new Vector2(0f, 0.5f);
        panelRoot.anchoredPosition = new Vector2(12f, 0f);
        panelRoot.sizeDelta = new Vector2(PanelWidth, -24f);

        var panelBg = panelGo.GetComponent<Image>();
        panelBg.color = new Color(0.09f, 0.11f, 0.15f, 0.98f);
        panelBg.raycastTarget = true;

        var title = CraftingUiUtility.CreateText(panelRoot, "Title", "制作图鉴", 20, TextAnchor.UpperCenter);
        title.fontStyle = FontStyle.Bold;
        var titleRect = title.rectTransform;
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -8f);
        titleRect.sizeDelta = new Vector2(-16f, 28f);

        var hint = CraftingUiUtility.CreateText(panelRoot, "Hint", "滚轮浏览 · 只读参考 · 实际制作请用制造面板", 12, TextAnchor.UpperCenter);
        hint.color = new Color(0.72f, 0.76f, 0.82f);
        var hintRect = hint.rectTransform;
        hintRect.anchorMin = new Vector2(0f, 1f);
        hintRect.anchorMax = new Vector2(1f, 1f);
        hintRect.pivot = new Vector2(0.5f, 1f);
        hintRect.anchoredPosition = new Vector2(0f, -34f);
        hintRect.sizeDelta = new Vector2(-12f, 20f);

        var categoryBarGo = new GameObject("CategoryBar", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        categoryBarGo.transform.SetParent(panelRoot, false);
        categoryBarRoot = categoryBarGo.GetComponent<RectTransform>();
        categoryBarRoot.anchorMin = new Vector2(0f, 1f);
        categoryBarRoot.anchorMax = new Vector2(1f, 1f);
        categoryBarRoot.pivot = new Vector2(0.5f, 1f);
        categoryBarRoot.anchoredPosition = new Vector2(0f, -56f);
        categoryBarRoot.sizeDelta = new Vector2(-12f, 26f);

        var categoryLayout = categoryBarGo.GetComponent<HorizontalLayoutGroup>();
        categoryLayout.spacing = 3f;
        categoryLayout.childAlignment = TextAnchor.MiddleCenter;
        categoryLayout.childControlWidth = true;
        categoryLayout.childControlHeight = true;
        categoryLayout.childForceExpandWidth = false;
        categoryLayout.childForceExpandHeight = true;

        RebuildCategoryBar();

        var scrollGo = new GameObject("HandbookScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        scrollGo.transform.SetParent(panelRoot, false);
        var scrollRectTransform = scrollGo.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = new Vector2(0f, 0f);
        scrollRectTransform.anchorMax = new Vector2(1f, 1f);
        scrollRectTransform.offsetMin = new Vector2(8f, 8f);
        scrollRectTransform.offsetMax = new Vector2(-8f, -88f);

        scrollGo.GetComponent<Image>().color = new Color(0.05f, 0.06f, 0.09f, 0.85f);

        var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewport.transform.SetParent(scrollGo.transform, false);
        var viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = new Vector2(-14f, 0f);
        viewport.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.02f);
        viewport.GetComponent<Mask>().showMaskGraphic = false;

        var content = new GameObject("List", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);
        listContentRoot = content.GetComponent<RectTransform>();
        listContentRoot.anchorMin = new Vector2(0f, 1f);
        listContentRoot.anchorMax = new Vector2(1f, 1f);
        listContentRoot.pivot = new Vector2(0.5f, 1f);
        listContentRoot.anchoredPosition = Vector2.zero;
        listContentRoot.sizeDelta = new Vector2(0f, 0f);

        var listLayout = content.GetComponent<VerticalLayoutGroup>();
        listLayout.spacing = 6f;
        listLayout.padding = new RectOffset(4, 4, 4, 4);
        listLayout.childControlWidth = true;
        listLayout.childControlHeight = true;
        listLayout.childForceExpandWidth = true;
        listLayout.childForceExpandHeight = false;

        content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        listScrollRect = scrollGo.GetComponent<ScrollRect>();
        listScrollRect.content = listContentRoot;
        listScrollRect.viewport = viewportRect;
        listScrollRect.horizontal = false;
        listScrollRect.vertical = true;
        listScrollRect.movementType = ScrollRect.MovementType.Clamped;
        listScrollRect.scrollSensitivity = 32f;
        listScrollRect.verticalScrollbar = CraftingUiUtility.CreateScrollbar(scrollGo.transform, "VerticalScrollbar", true);

        panelGo.SetActive(false);
    }

    void RebuildCategoryBar()
    {
        if (categoryBarRoot == null)
        {
            return;
        }

        for (int i = categoryBarRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(categoryBarRoot.GetChild(i).gameObject);
        }

        CreateHandbookCategoryButton(categoryBarRoot, null, "全部");
        for (int i = 0; i < CraftingRecipeCategoryUtility.FilterCategories.Length; i++)
        {
            CraftingRecipeCategory category = CraftingRecipeCategoryUtility.FilterCategories[i];
            CreateHandbookCategoryButton(categoryBarRoot, category, CraftingRecipeCategoryUtility.GetDisplayName(category));
        }
    }

    void CreateHandbookCategoryButton(Transform parent, CraftingRecipeCategory? category, string label)
    {
        var button = CraftingUiUtility.CreateButton(parent, $"Cat_{label}", label, Vector2.zero);
        var layout = button.gameObject.AddComponent<LayoutElement>();
        layout.minWidth = 56f;
        layout.preferredHeight = 24f;

        bool selected = selectedCategoryFilter == category;
        button.GetComponent<Image>().color = selected
            ? new Color(0.34f, 0.52f, 0.38f)
            : new Color(0.18f, 0.2f, 0.24f);

        CraftingRecipeCategory? captured = category;
        button.onClick.AddListener(() =>
        {
            selectedCategoryFilter = captured;
            RebuildCategoryBar();
            RebuildList();
            RefreshAll();
        });
    }

    void RebuildList()
    {
        if (listContentRoot == null)
        {
            return;
        }

        for (int i = listContentRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(listContentRoot.GetChild(i).gameObject);
        }

        rows.Clear();
        int index = 0;
        foreach (CraftingRecipe recipe in CraftingRecipeDisplayUtility.EnumerateKnownRecipes(selectedCategoryFilter))
        {
            rows.Add(CreateHandbookRow(recipe, index++));
        }
    }

    HandbookRowView CreateHandbookRow(CraftingRecipe recipe, int index)
    {
        var rowGo = new GameObject($"HandbookRow_{index}", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(LayoutElement));
        rowGo.transform.SetParent(listContentRoot, false);

        var rowImage = rowGo.GetComponent<Image>();
        rowImage.color = new Color(0.13f, 0.15f, 0.19f, 0.96f);

        var layoutElement = rowGo.GetComponent<LayoutElement>();
        layoutElement.minHeight = RowMinHeight;

        var layout = rowGo.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(10, 10, 8, 8);
        layout.spacing = 4f;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        var nameText = CraftingUiUtility.CreateText(rowGo.transform, "Name", recipe.GetDisplayName(), 17, TextAnchor.UpperLeft);
        nameText.fontStyle = FontStyle.Bold;

        var detailText = CraftingUiUtility.CreateText(rowGo.transform, "Detail", string.Empty, 13, TextAnchor.UpperLeft);
        detailText.horizontalOverflow = HorizontalWrapMode.Wrap;
        detailText.verticalOverflow = VerticalWrapMode.Overflow;
        detailText.color = new Color(0.78f, 0.82f, 0.88f);

        return new HandbookRowView
        {
            Recipe = recipe,
            Background = rowImage,
            NameText = nameText,
            DetailText = detailText,
        };
    }

    void RefreshAll()
    {
        if (!IsOpen || inventory == null)
        {
            return;
        }

        for (int i = 0; i < rows.Count; i++)
        {
            RefreshRow(rows[i]);
        }
    }

    void RefreshRow(HandbookRowView row)
    {
        CraftingUnlockResult result = CraftingRecipeDisplayUtility.EvaluateForHandbook(row.Recipe, inventory);
        string detail = CraftingRecipeDisplayUtility.BuildHandbookDetail(
            row.Recipe,
            result,
            row.Recipe.RequiredWorkstation);

        GameplayChineseText.PrepareUiText(row.NameText, row.Recipe.GetDisplayName());
        GameplayChineseText.PrepareUiText(row.DetailText, detail);

        row.Background.color = result.CanCraftNow
            ? new Color(0.15f, 0.2f, 0.16f, 0.96f)
            : result.IsUnlocked
                ? new Color(0.13f, 0.15f, 0.19f, 0.96f)
                : new Color(0.11f, 0.11f, 0.12f, 0.92f);

        row.NameText.color = result.IsUnlocked ? Color.white : new Color(0.55f, 0.56f, 0.58f);
        row.DetailText.color = result.CanCraftNow
            ? new Color(0.78f, 0.9f, 0.78f)
            : new Color(0.68f, 0.7f, 0.74f);
    }

    public void InvalidateUi()
    {
        IsOpen = false;
        panelRoot = null;
        categoryBarRoot = null;
        listContentRoot = null;
        listScrollRect = null;
        tabButtonRect = null;
        rows.Clear();
    }

    class HandbookRowView
    {
        public CraftingRecipe Recipe;
        public Image Background;
        public Text NameText;
        public Text DetailText;
    }

    void OnDisable()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= RefreshAll;
        }
    }
}
