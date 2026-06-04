using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class CraftingHud : MonoBehaviour
{
    const float ColumnWidth = 300f;
    const float BasePanelHeight = 720f;
    const float RowHeight = 70f;
    const float MinPanelWidth = 680f;
    const float MaxPanelWidth = 1480f;

    float currentPanelHeight = BasePanelHeight;

    PlayerInventory inventory;
    readonly List<WorkstationKind> activeContexts = new List<WorkstationKind>();
    readonly List<RecipeRowView> recipeRows = new List<RecipeRowView>();
    readonly List<Button> categoryButtons = new List<Button>();

    RectTransform panelRoot;
    RectTransform columnsRoot;
    RectTransform categoryBarRoot;
    Text titleText;
    Text statusText;
    Text hintText;
    CraftingRecipeCategory? selectedCategoryFilter;

    ScrollRect columnsScrollRect;

    public bool IsOpen { get; private set; }

    void OnEnable()
    {
        SubscribeProgressionEvents();
    }

    void OnDisable()
    {
        UnsubscribeProgressionEvents();
    }

    void Awake()
    {
        EnsureEventSystem();
    }

    void Update()
    {
        if (!IsOpen)
        {
            return;
        }

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Close();
            return;
        }

        if (Mouse.current != null && columnsScrollRect != null)
        {
            float scroll = Mouse.current.scroll.y.ReadValue();
            if (Mathf.Abs(scroll) > 0.01f)
            {
                bool shiftHeld = Keyboard.current != null
                    && (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);
                if (shiftHeld && columnsScrollRect.horizontal)
                {
                    columnsScrollRect.horizontalNormalizedPosition += scroll > 0f ? 0.08f : -0.08f;
                }
            }
        }
    }

    public void RebuildUi()
    {
        DestroyPanel("CraftingPanel");
        panelRoot = null;
        columnsRoot = null;
        categoryBarRoot = null;
        columnsScrollRect = null;
        recipeRows.Clear();
        BuildUi();
        if (IsOpen)
        {
            RefreshAll();
        }
    }

    public void OpenCrafting(PlayerInventory playerInventory, List<WorkstationKind> contexts)
    {
        activeContexts.Clear();
        if (contexts == null || contexts.Count == 0)
        {
            activeContexts.Add(WorkstationKind.None);
        }
        else
        {
            activeContexts.AddRange(contexts);
        }

        OpenInternal(playerInventory);
    }

    public void OpenHandCraft(PlayerInventory playerInventory)
    {
        OpenCrafting(playerInventory, new List<WorkstationKind> { WorkstationKind.None });
    }

    public void OpenWorkstation(PlayerInventory playerInventory, CraftingStation station)
    {
        var contexts = WorkstationDetector.GetCraftingContexts(playerInventory.transform.position);
        OpenCrafting(playerInventory, contexts);
    }

    public void Open(PlayerInventory playerInventory, CraftingStation station)
    {
        OpenCrafting(playerInventory, WorkstationDetector.GetCraftingContexts(playerInventory.transform.position));
    }

    void OpenInternal(PlayerInventory playerInventory)
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

        if (panelRoot == null)
        {
            BuildUi();
        }

        currentPanelHeight = GetResponsivePanelHeight();
        float panelWidth = Mathf.Clamp(ColumnWidth * activeContexts.Count + 64f, MinPanelWidth, MaxPanelWidth);
        panelRoot.sizeDelta = new Vector2(panelWidth, currentPanelHeight);

        if (titleText != null)
        {
            titleText.text = "制造";
        }

        RebuildCategoryBar();
        RebuildColumns();

        IsOpen = true;
        panelRoot.gameObject.SetActive(true);
        GameplayCursorPolicy.SetCraftingUiOpen(true);
        DisableKeyboardNavigationForCraftingUi();
        RefreshAll();
        Debug.Log("[CraftingUI] Crafting UI opened: gameplay input disabled");
    }

    public void Close()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= RefreshAll;
        }

        IsOpen = false;
        activeContexts.Clear();
        if (panelRoot != null)
        {
            panelRoot.gameObject.SetActive(false);
        }

        if (statusText != null)
        {
            statusText.text = string.Empty;
        }

        GameplayCursorPolicy.SetCraftingUiOpen(false);
        Debug.Log("[CraftingUI] Crafting UI closed: gameplay input restored");
    }

    void BuildUi()
    {
        var canvas = GetComponent<RectTransform>();
        if (canvas == null)
        {
            return;
        }

        panelRoot = CreatePanel(canvas, "CraftingPanel", MinPanelWidth, BasePanelHeight);
        panelRoot.anchorMin = new Vector2(0.5f, 0.5f);
        panelRoot.anchorMax = new Vector2(0.5f, 0.5f);
        panelRoot.pivot = new Vector2(0.5f, 0.5f);
        panelRoot.anchoredPosition = Vector2.zero;

        var backdrop = panelRoot.GetComponent<Image>();
        backdrop.color = new Color(0.08f, 0.1f, 0.14f, 0.94f);

        titleText = CreateText(panelRoot, "Title", "制造", 22, TextAnchor.UpperCenter);
        var titleRect = titleText.rectTransform;
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -12f);
        titleRect.sizeDelta = new Vector2(-24f, 32f);

        hintText = CreateText(panelRoot, "Hint", "Esc 关闭  ·  滚轮滚动配方  ·  Shift+滚轮横向切换工作站列", 14, TextAnchor.UpperCenter);
        var hintRect = hintText.rectTransform;
        hintRect.anchorMin = new Vector2(0f, 1f);
        hintRect.anchorMax = new Vector2(1f, 1f);
        hintRect.pivot = new Vector2(0.5f, 1f);
        hintRect.anchoredPosition = new Vector2(0f, -42f);
        hintRect.sizeDelta = new Vector2(-24f, 22f);
        hintText.color = new Color(0.75f, 0.78f, 0.82f);

        var categoryBarGo = new GameObject("CategoryBar", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        categoryBarGo.transform.SetParent(panelRoot, false);
        categoryBarRoot = categoryBarGo.GetComponent<RectTransform>();
        categoryBarRoot.anchorMin = new Vector2(0f, 1f);
        categoryBarRoot.anchorMax = new Vector2(1f, 1f);
        categoryBarRoot.pivot = new Vector2(0.5f, 1f);
        categoryBarRoot.anchoredPosition = new Vector2(0f, -68f);
        categoryBarRoot.sizeDelta = new Vector2(-24f, 30f);

        var categoryLayout = categoryBarGo.GetComponent<HorizontalLayoutGroup>();
        categoryLayout.spacing = 4f;
        categoryLayout.childAlignment = TextAnchor.MiddleCenter;
        categoryLayout.childControlWidth = true;
        categoryLayout.childControlHeight = true;
        categoryLayout.childForceExpandWidth = false;
        categoryLayout.childForceExpandHeight = true;

        var scrollGo = new GameObject("ColumnsScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        scrollGo.transform.SetParent(panelRoot, false);
        var scrollRectTransform = scrollGo.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = new Vector2(0f, 0f);
        scrollRectTransform.anchorMax = new Vector2(1f, 1f);
        scrollRectTransform.offsetMin = new Vector2(12f, 56f);
        scrollRectTransform.offsetMax = new Vector2(-12f, -96f);

        var scrollBg = scrollGo.GetComponent<Image>();
        scrollBg.color = new Color(0.04f, 0.05f, 0.08f, 0.85f);

        var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewport.transform.SetParent(scrollGo.transform, false);
        var viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        viewport.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.02f);
        viewport.GetComponent<Mask>().showMaskGraphic = false;

        var content = new GameObject("Columns", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);
        columnsRoot = content.GetComponent<RectTransform>();
        columnsRoot.anchorMin = new Vector2(0f, 0f);
        columnsRoot.anchorMax = new Vector2(0f, 1f);
        columnsRoot.pivot = new Vector2(0f, 0.5f);
        columnsRoot.anchoredPosition = Vector2.zero;
        columnsRoot.sizeDelta = new Vector2(0f, 0f);

        var columnsLayout = content.GetComponent<HorizontalLayoutGroup>();
        columnsLayout.spacing = 10f;
        columnsLayout.padding = new RectOffset(6, 6, 6, 6);
        columnsLayout.childAlignment = TextAnchor.UpperLeft;
        columnsLayout.childControlWidth = false;
        columnsLayout.childControlHeight = true;
        columnsLayout.childForceExpandWidth = false;
        columnsLayout.childForceExpandHeight = true;

        var columnsFitter = content.GetComponent<ContentSizeFitter>();
        columnsFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        columnsFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

        var scrollRect = scrollGo.GetComponent<ScrollRect>();
        scrollRect.content = columnsRoot;
        scrollRect.viewport = viewportRect;
        scrollRect.horizontal = true;
        scrollRect.vertical = false;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 28f;
        scrollRect.horizontalScrollbar = CreateScrollbar(scrollGo.transform, "HorizontalScrollbar", false);
        columnsScrollRect = scrollRect;

        statusText = CreateText(panelRoot, "Status", string.Empty, 15, TextAnchor.LowerCenter);
        var statusRect = statusText.rectTransform;
        statusRect.anchorMin = new Vector2(0f, 0f);
        statusRect.anchorMax = new Vector2(1f, 0f);
        statusRect.pivot = new Vector2(0.5f, 0f);
        statusRect.anchoredPosition = new Vector2(0f, 12f);
        statusRect.sizeDelta = new Vector2(-24f, 36f);
        statusText.color = new Color(0.92f, 0.86f, 0.55f);

        var closeButton = CreateCloseButton(panelRoot, "CloseButton", "关闭");
        closeButton.onClick.AddListener(Close);

        panelRoot.gameObject.SetActive(false);
    }

    void RebuildCategoryBar()
    {
        if (categoryBarRoot == null)
        {
            return;
        }

        for (int i = categoryBarRoot.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(categoryBarRoot.GetChild(i).gameObject);
        }

        categoryButtons.Clear();
        CreateCategoryButton(null, "全部");

        for (int i = 0; i < CraftingRecipeCategoryUtility.FilterCategories.Length; i++)
        {
            CraftingRecipeCategory category = CraftingRecipeCategoryUtility.FilterCategories[i];
            CreateCategoryButton(category, CraftingRecipeCategoryUtility.GetDisplayName(category));
        }
    }

    void CreateCategoryButton(CraftingRecipeCategory? category, string label)
    {
        var button = CreateButton(categoryBarRoot, $"Category_{label}", label, Vector2.zero);
        var layout = button.gameObject.AddComponent<LayoutElement>();
        layout.minWidth = 72f;
        layout.preferredHeight = 26f;

        bool selected = selectedCategoryFilter == category;
        var image = button.GetComponent<Image>();
        image.color = selected
            ? new Color(0.34f, 0.52f, 0.38f)
            : new Color(0.18f, 0.2f, 0.24f);

        CraftingRecipeCategory? captured = category;
        button.onClick.AddListener(() =>
        {
            selectedCategoryFilter = captured;
            RebuildCategoryBar();
            RebuildColumns();
            RefreshAll();
        });

        categoryButtons.Add(button);
    }

    void RebuildColumns()
    {
        if (columnsRoot == null)
        {
            return;
        }

        for (int i = columnsRoot.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(columnsRoot.GetChild(i).gameObject);
        }

        recipeRows.Clear();

        for (int i = 0; i < activeContexts.Count; i++)
        {
            WorkstationKind context = activeContexts[i];
            RectTransform column = CreateColumn(columnsRoot, context);
            BuildRecipesForColumn(column, context);
        }

        DisableKeyboardNavigationForCraftingUi();
    }

    RectTransform CreateColumn(Transform parent, WorkstationKind context)
    {
        var columnGo = new GameObject($"Column_{context}", typeof(RectTransform), typeof(Image), typeof(LayoutElement), typeof(VerticalLayoutGroup));
        columnGo.transform.SetParent(parent, false);

        var columnRect = columnGo.GetComponent<RectTransform>();
        columnRect.sizeDelta = new Vector2(ColumnWidth, currentPanelHeight - 148f);

        var columnImage = columnGo.GetComponent<Image>();
        columnImage.color = new Color(0.1f, 0.12f, 0.16f, 0.92f);

        var columnElement = columnGo.GetComponent<LayoutElement>();
        columnElement.minWidth = ColumnWidth;
        columnElement.preferredWidth = ColumnWidth;

        var columnLayout = columnGo.GetComponent<VerticalLayoutGroup>();
        columnLayout.padding = new RectOffset(8, 8, 8, 8);
        columnLayout.spacing = 6f;
        columnLayout.childAlignment = TextAnchor.UpperCenter;
        columnLayout.childControlWidth = true;
        columnLayout.childControlHeight = true;
        columnLayout.childForceExpandWidth = true;
        columnLayout.childForceExpandHeight = true;

        var header = CreateText(columnGo.transform, "Header", WorkstationDetector.GetColumnTitle(context), 17, TextAnchor.MiddleCenter);
        header.fontStyle = FontStyle.Bold;
        var headerRect = header.rectTransform;
        headerRect.sizeDelta = new Vector2(0f, 28f);
        var headerLayout = header.gameObject.AddComponent<LayoutElement>();
        headerLayout.minHeight = 28f;
        headerLayout.preferredHeight = 28f;

        var scrollGo = new GameObject("RecipeScroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
        scrollGo.transform.SetParent(columnGo.transform, false);
        var scrollRectTransform = scrollGo.GetComponent<RectTransform>();
        scrollRectTransform.sizeDelta = new Vector2(ColumnWidth - 16f, 0f);
        var scrollLayout = scrollGo.AddComponent<LayoutElement>();
        scrollLayout.flexibleHeight = 1f;
        scrollLayout.minHeight = Mathf.Max(420f, currentPanelHeight - 220f);

        var scrollBg = scrollGo.GetComponent<Image>();
        scrollBg.color = new Color(0.06f, 0.07f, 0.1f, 0.65f);

        var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewport.transform.SetParent(scrollGo.transform, false);
        var viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        viewport.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.02f);
        viewport.GetComponent<Mask>().showMaskGraphic = false;

        var content = new GameObject("RecipeList", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);
        var listRect = content.GetComponent<RectTransform>();
        listRect.anchorMin = new Vector2(0f, 1f);
        listRect.anchorMax = new Vector2(1f, 1f);
        listRect.pivot = new Vector2(0.5f, 1f);
        listRect.anchoredPosition = Vector2.zero;
        listRect.sizeDelta = new Vector2(0f, 0f);

        var listLayout = content.GetComponent<VerticalLayoutGroup>();
        listLayout.spacing = 6f;
        listLayout.padding = new RectOffset(2, 2, 2, 2);
        listLayout.childAlignment = TextAnchor.UpperCenter;
        listLayout.childControlWidth = true;
        listLayout.childControlHeight = true;
        listLayout.childForceExpandWidth = true;
        listLayout.childForceExpandHeight = false;

        var listFitter = content.GetComponent<ContentSizeFitter>();
        listFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        listFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        var scrollRect = scrollGo.GetComponent<ScrollRect>();
        scrollRect.content = listRect;
        scrollRect.viewport = viewportRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 28f;
        scrollRect.verticalScrollbar = CreateScrollbar(scrollGo.transform, "VerticalScrollbar", true);

        return listRect;
    }

    void BuildRecipesForColumn(RectTransform columnListRoot, WorkstationKind context)
    {
        foreach (CraftingRecipe recipe in CraftingRecipeCategoryUtility.GetRecipesForContext(context, selectedCategoryFilter))
        {
            RecipeRowView row = CreateRecipeRow(columnListRoot, recipe, context, recipeRows.Count);
            recipeRows.Add(row);
        }
    }

    static float GetResponsivePanelHeight()
    {
        return Mathf.Clamp(Screen.height * 0.82f, 680f, 920f);
    }

    RecipeRowView CreateRecipeRow(RectTransform parent, CraftingRecipe recipe, WorkstationKind context, int index)
    {
        var rowGo = new GameObject($"RecipeRow_{index}", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        rowGo.transform.SetParent(parent, false);

        var rowImage = rowGo.GetComponent<Image>();
        rowImage.color = new Color(0.14f, 0.16f, 0.2f, 0.95f);

        var layoutElement = rowGo.GetComponent<LayoutElement>();
        layoutElement.minHeight = RowHeight + 12f;
        layoutElement.preferredHeight = RowHeight + 12f;

        var layout = rowGo.GetComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(8, 8, 6, 6);
        layout.spacing = 8f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = false;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = true;

        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconGo.transform.SetParent(rowGo.transform, false);
        var iconRect = iconGo.GetComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(40f, 40f);
        var iconImage = iconGo.GetComponent<Image>();
        iconImage.sprite = ItemKindUtility.GetIcon(recipe.Output);
        iconImage.preserveAspect = true;

        var textColumn = new GameObject("TextColumn", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
        textColumn.transform.SetParent(rowGo.transform, false);
        var textColumnLayout = textColumn.GetComponent<VerticalLayoutGroup>();
        textColumnLayout.childAlignment = TextAnchor.MiddleLeft;
        textColumnLayout.spacing = 2f;
        textColumnLayout.childControlHeight = true;
        textColumnLayout.childControlWidth = true;
        textColumnLayout.childForceExpandHeight = false;
        textColumnLayout.childForceExpandWidth = true;

        var textColumnElement = textColumn.GetComponent<LayoutElement>();
        textColumnElement.flexibleWidth = 1f;
        textColumnElement.minWidth = 120f;

        var nameText = CreateText(textColumn.transform, "Name", recipe.GetDisplayName(), 17, TextAnchor.MiddleLeft);
        nameText.fontStyle = FontStyle.Bold;
        nameText.color = Color.white;
        nameText.horizontalOverflow = HorizontalWrapMode.Overflow;
        nameText.verticalOverflow = VerticalWrapMode.Overflow;
        var nameLayout = nameText.gameObject.AddComponent<LayoutElement>();
        nameLayout.minHeight = 22f;
        nameLayout.preferredHeight = 22f;

        var ingredientText = CreateText(textColumn.transform, "Ingredients", recipe.GetIngredientsSummary(), 13, TextAnchor.MiddleLeft);
        ingredientText.color = new Color(0.78f, 0.82f, 0.88f);
        ingredientText.horizontalOverflow = HorizontalWrapMode.Wrap;
        var ingredientLayout = ingredientText.gameObject.AddComponent<LayoutElement>();
        ingredientLayout.minHeight = 18f;
        ingredientLayout.flexibleHeight = 1f;

        var metaText = CreateText(textColumn.transform, "Meta",
            $"分类：{CraftingRecipeDisplayUtility.GetCategoryDisplayName(recipe.Category)}",
            12, TextAnchor.MiddleLeft);
        metaText.color = new Color(0.62f, 0.66f, 0.72f);
        var metaLayout = metaText.gameObject.AddComponent<LayoutElement>();
        metaLayout.minHeight = 16f;

        var craftButton = CreateButton(rowGo.transform, "CraftButton", "制作", Vector2.zero);
        var buttonRect = craftButton.GetComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(72f, 36f);

        var rowView = new RecipeRowView
        {
            Recipe = recipe,
            Context = context,
            Root = rowGo.GetComponent<RectTransform>(),
            Background = rowImage,
            CraftButton = craftButton,
            NameText = nameText,
            IngredientText = ingredientText,
        };

        craftButton.onClick.AddListener(() => OnCraftClicked(rowView));
        return rowView;
    }

    void OnCraftClicked(RecipeRowView row)
    {
        if (inventory == null || string.IsNullOrEmpty(row.Recipe.RecipeId))
        {
            return;
        }

        if (CraftingManager.TryCraft(inventory, row.Recipe, row.Context, out string message))
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }
        else if (statusText != null)
        {
            statusText.text = message;
        }

        RefreshAll();
    }

    void RefreshAll()
    {
        if (inventory == null)
        {
            return;
        }

        for (int i = 0; i < recipeRows.Count; i++)
        {
            RefreshRow(recipeRows[i]);
        }
    }

    void RefreshRow(RecipeRowView row)
    {
        CraftingUnlockResult result = CraftingManager.Evaluate(inventory, row.Recipe, row.Context);
        bool unlocked = result.IsUnlocked;
        bool canCraft = result.CanCraftNow;
        bool hasMaterials = CraftingIngredientDiagnostics.HasAllIngredients(inventory, row.Recipe.Ingredients);
        bool hasOutputSpace = inventory != null && inventory.CanAcceptItem(row.Recipe.Output, row.Recipe.OutputCount);

        row.Root.gameObject.SetActive(true);
        row.CraftButton.interactable = canCraft;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (row.Recipe.Category == CraftingRecipeCategory.Tool && !canCraft && unlocked)
        {
            Debug.Log(
                $"[CraftingUI] {row.Recipe.GetDisplayName()} craftable: {canCraft} | " +
                $"materials: {hasMaterials} | station: {unlocked && result.LockReason != CraftingLockReason.WrongWorkstationContext} | " +
                $"outputSpace: {hasOutputSpace} | disabledReason: {result.Message}");
        }
#endif

        row.Background.color = canCraft
            ? new Color(0.16f, 0.22f, 0.18f, 0.96f)
            : unlocked
                ? new Color(0.12f, 0.12f, 0.14f, 0.92f)
                : new Color(0.1f, 0.1f, 0.11f, 0.85f);

        row.NameText.color = unlocked ? (canCraft ? Color.white : new Color(0.72f, 0.74f, 0.78f)) : new Color(0.45f, 0.46f, 0.48f);

        string detail = row.Recipe.GetIngredientsSummary();
        if (!unlocked && !string.IsNullOrEmpty(result.Message))
        {
            detail = result.Message;
        }
        else if (unlocked && !canCraft && !string.IsNullOrEmpty(result.Message))
        {
            detail = $"{detail}  ·  {result.Message}";
        }

        GameplayChineseText.PrepareUiText(row.NameText, row.Recipe.GetDisplayName());
        GameplayChineseText.PrepareUiText(row.IngredientText, detail);
        row.IngredientText.color = canCraft
            ? new Color(0.78f, 0.88f, 0.78f)
            : new Color(0.55f, 0.56f, 0.58f);
    }

    void SubscribeProgressionEvents()
    {
        if (TechnologyManager.Instance != null)
        {
            TechnologyManager.Instance.OnTechnologyUnlocked -= HandleProgressionChanged;
            TechnologyManager.Instance.OnTechnologyUnlocked += HandleProgressionChanged;
        }

        if (BuildingRegistry.Instance != null)
        {
            BuildingRegistry.Instance.OnBuildingCountChanged -= HandleBuildingChanged;
            BuildingRegistry.Instance.OnBuildingCountChanged += HandleBuildingChanged;
        }
    }

    void UnsubscribeProgressionEvents()
    {
        if (TechnologyManager.Instance != null)
        {
            TechnologyManager.Instance.OnTechnologyUnlocked -= HandleProgressionChanged;
        }

        if (BuildingRegistry.Instance != null)
        {
            BuildingRegistry.Instance.OnBuildingCountChanged -= HandleBuildingChanged;
        }
    }

    void HandleProgressionChanged(TechnologyKind technology)
    {
        if (IsOpen)
        {
            RefreshAll();
        }
    }

    void HandleBuildingChanged(BuildingKind building, int count)
    {
        if (IsOpen)
        {
            RefreshAll();
        }
    }

    static RectTransform CreatePanel(RectTransform parent, string name, float width, float height)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(width, height);
        return rect;
    }

    static Text CreateText(Transform parent, string name, string content, int fontSize, TextAnchor alignment)
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

    static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(72f, 36f);

        var image = go.GetComponent<Image>();
        image.color = new Color(0.28f, 0.48f, 0.32f);

        var button = go.GetComponent<Button>();
        var colors = button.colors;
        colors.highlightedColor = new Color(0.36f, 0.58f, 0.4f);
        colors.pressedColor = new Color(0.22f, 0.38f, 0.26f);
        colors.disabledColor = new Color(0.22f, 0.22f, 0.24f);
        button.colors = colors;

        CreateText(go.transform, "Label", label, 14, TextAnchor.MiddleCenter);
        return button;
    }

    static Button CreateCloseButton(RectTransform parent, string name, string label)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-12f, -12f);
        rect.sizeDelta = new Vector2(64f, 28f);

        var image = go.GetComponent<Image>();
        image.color = new Color(0.32f, 0.22f, 0.22f);

        var button = go.GetComponent<Button>();
        CreateText(go.transform, "Label", label, 14, TextAnchor.MiddleCenter);
        return button;
    }

    void DestroyPanel(string panelName)
    {
        var existing = transform.Find(panelName);
        if (existing != null)
        {
            DestroyImmediate(existing.gameObject);
        }
    }

    static Scrollbar CreateScrollbar(Transform parent, string name, bool vertical)
    {
        var trackGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Scrollbar));
        trackGo.transform.SetParent(parent, false);
        var trackRect = trackGo.GetComponent<RectTransform>();
        if (vertical)
        {
            trackRect.anchorMin = new Vector2(1f, 0f);
            trackRect.anchorMax = new Vector2(1f, 1f);
            trackRect.pivot = new Vector2(1f, 1f);
            trackRect.sizeDelta = new Vector2(12f, 0f);
            trackRect.anchoredPosition = Vector2.zero;
        }
        else
        {
            trackRect.anchorMin = new Vector2(0f, 0f);
            trackRect.anchorMax = new Vector2(1f, 0f);
            trackRect.pivot = new Vector2(0f, 0f);
            trackRect.sizeDelta = new Vector2(0f, 12f);
            trackRect.anchoredPosition = Vector2.zero;
        }

        var trackImage = trackGo.GetComponent<Image>();
        trackImage.color = new Color(0.12f, 0.14f, 0.18f, 0.95f);

        var handleGo = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handleGo.transform.SetParent(trackGo.transform, false);
        var handleRect = handleGo.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(8f, 8f);
        var handleImage = handleGo.GetComponent<Image>();
        handleImage.color = new Color(0.45f, 0.52f, 0.58f, 0.95f);

        var scrollbar = trackGo.GetComponent<Scrollbar>();
        scrollbar.handleRect = handleRect;
        scrollbar.targetGraphic = handleImage;
        scrollbar.direction = vertical ? Scrollbar.Direction.BottomToTop : Scrollbar.Direction.LeftToRight;
        DisableSelectableNavigation(scrollbar);
        return scrollbar;
    }

    void DisableKeyboardNavigationForCraftingUi()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            Debug.Log("[CraftingUI] EventSystem selected object cleared");
        }

        if (panelRoot != null)
        {
            DisableSelectableNavigationRecursive(panelRoot);
        }

        Debug.Log("[CraftingUI] WASD UI navigation disabled");
    }

    static void DisableSelectableNavigationRecursive(Transform root)
    {
        if (root == null)
        {
            return;
        }

        Selectable[] selectables = root.GetComponentsInChildren<Selectable>(true);
        for (int i = 0; i < selectables.Length; i++)
        {
            DisableSelectableNavigation(selectables[i]);
        }
    }

    static void DisableSelectableNavigation(Selectable selectable)
    {
        if (selectable == null)
        {
            return;
        }

        Navigation navigation = selectable.navigation;
        navigation.mode = Navigation.Mode.None;
        selectable.navigation = navigation;
    }

    static void EnsureEventSystem()
    {
        var eventSystem = FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            var eventSystemGo = new GameObject(
                "EventSystem",
                typeof(EventSystem),
                typeof(InputSystemUIInputModule));
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(eventSystemGo);
            }
        }
    }

    class RecipeRowView
    {
        public CraftingRecipe Recipe;
        public WorkstationKind Context;
        public RectTransform Root;
        public Image Background;
        public Button CraftButton;
        public Text NameText;
        public Text IngredientText;
    }
}
