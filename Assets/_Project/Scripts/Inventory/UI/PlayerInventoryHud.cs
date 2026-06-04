using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class PlayerInventoryHud : MonoBehaviour
{
    const float SlotSize = 48f;
    const float SlotSpacing = 4f;
    const int HotbarColumns = PlayerInventory.HotbarSize;
    const int BackpackColumns = 9;
    const int BackpackRows = 3;

    [SerializeField] PlayerInventory inventory;
    [SerializeField] PlayerPickupInteractor pickupInteractor;
    [SerializeField] PlayerGameplayTargeting gameplayTargeting;
    [SerializeField] CraftingHud craftingHud;

    [Header("Trash Slot")]
    [SerializeField] float trashSlotSize = 56f;

    [Header("Right Click Split")]
    [SerializeField] float rightClickHoldThreshold = 1.5f;
    [SerializeField] float rightClickExtractionRate = 8f;

    PlayerItemDropController itemDropController;

    RectTransform hotbarRoot;
    RectTransform backpackRoot;
    RectTransform backpackWindowRect;
    RectTransform trashSlotRect;
    RectTransform pickupPromptRoot;
    Text pickupPromptText;
    InventorySlotView[] hotbarSlotViews;
    InventorySlotView[] backpackSlotViews;

    RectTransform dragGhost;
    Image dragGhostIcon;
    Text dragGhostCount;

    RectTransform itemTooltipRoot;
    Text itemTooltipText;
    LayoutElement itemTooltipLayout;

    bool itemTooltipVisible;
    InventorySlotRegion itemTooltipRegion;
    int itemTooltipIndex = -1;
    bool itemTooltipRegionValid;

    bool backpackOpen;
    bool isDragging;
    InventorySlotRegion dragFromRegion;
    int dragFromIndex = -1;

    InventorySlotData cursorHeld;
    bool hasCursorHeld;
    InventorySlotRegion cursorHeldSourceRegion;
    int cursorHeldSourceIndex = -1;
    bool cursorHeldHasSource;

    bool rightClickTracking;
    float rightClickStartTime;
    InventorySlotRegion rightClickRegion;
    int rightClickIndex = -1;
    bool rightClickLongPressActive;
    int rightClickLongExtractedTotal;

    public bool IsBackpackOpen => backpackOpen;
    public bool HasCursorHeld => hasCursorHeld;

    void Awake()
    {
        EnsureEventSystem();
        BindInventory();
    }

    void Start()
    {
        if (hotbarRoot == null || hotbarSlotViews == null || !HasValidHotbarViews())
        {
            BuildUi();
        }

        BindInventory();
        SubscribeInventory();
        RefreshAll();
    }

    void OnEnable()
    {
        BindInventory();
        SubscribeInventory();
        RefreshAll();
    }

    void Update()
    {
        HandleKeyboardInput();
        RefreshPickupPrompt();
        HandleRightClickExtraction();

        Vector2 mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        if (isDragging || hasCursorHeld)
        {
            UpdateDragGhostPosition(mousePos);
            HideItemTooltip();
        }
        else if (itemTooltipVisible && backpackOpen)
        {
            UpdateItemTooltipPosition(mousePos);
        }

        if (backpackOpen && Mouse.current != null && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            HandleInventoryPointerRelease(mousePos);
        }
    }

    void HandleInventoryPointerRelease(Vector2 screenPos)
    {
        if (hasCursorHeld)
        {
            if (IsPointerOverTrashSlot(screenPos))
            {
                TryTrashCursorHeld();
            }
            else if (!IsPointerOverInventoryPanel(screenPos))
            {
                TryDropCursorHeldToWorld();
            }

            return;
        }

        if (isDragging)
        {
            if (IsPointerOverTrashSlot(screenPos))
            {
                TryTrashDraggedStack();
                EndDrag(true);
            }
            else if (!IsPointerOverInventoryPanel(screenPos))
            {
                TryDropDraggedStackToWorld();
                EndDrag(true);
            }
        }
    }

    public void RebuildUi()
    {
        DestroyPanel("HotbarPanel");
        DestroyPanel("BackpackPanel");
        DestroyPanel("PickupPromptPanel");
        DestroyPanel("DragGhost");
        DestroyPanel("ItemDetailTooltip");
        hotbarRoot = null;
        backpackRoot = null;
        pickupPromptRoot = null;
        dragGhost = null;
        itemTooltipRoot = null;
        itemTooltipText = null;
        itemTooltipLayout = null;
        itemTooltipVisible = false;
        itemTooltipRegionValid = false;
        hotbarSlotViews = null;
        backpackSlotViews = null;
        BuildUi();
        RefreshAll();

        if (backpackOpen)
        {
            BringInventoryUiToFront();
        }
    }

    public void BindTo(PlayerInventory playerInventory, PlayerPickupInteractor interactor = null)
    {
        UnsubscribeInventory();
        inventory = playerInventory;
        pickupInteractor = interactor;
        SubscribeInventory();
        RefreshAll();
    }

    void BindInventory()
    {
        if (inventory == null)
        {
            inventory = FindFirstObjectByType<PlayerInventory>();
        }

        if (pickupInteractor == null)
        {
            pickupInteractor = FindFirstObjectByType<PlayerPickupInteractor>();
        }

        if (gameplayTargeting == null && pickupInteractor != null)
        {
            gameplayTargeting = pickupInteractor.GetComponent<PlayerGameplayTargeting>();
        }

        if (gameplayTargeting == null)
        {
            gameplayTargeting = FindFirstObjectByType<PlayerGameplayTargeting>();
        }

        if (craftingHud == null)
        {
            craftingHud = GetComponent<CraftingHud>();
        }

        if (craftingHud == null)
        {
            craftingHud = FindFirstObjectByType<CraftingHud>();
        }

        if (itemDropController == null)
        {
            itemDropController = FindFirstObjectByType<PlayerItemDropController>();
        }
    }

    void SubscribeInventory()
    {
        if (inventory == null)
        {
            return;
        }

        inventory.OnInventoryChanged -= RefreshAll;
        inventory.OnInventoryChanged += RefreshAll;
        inventory.OnHotbarSelectionChanged -= HandleHotbarSelectionChanged;
        inventory.OnHotbarSelectionChanged += HandleHotbarSelectionChanged;
    }

    void UnsubscribeInventory()
    {
        if (inventory == null)
        {
            return;
        }

        inventory.OnInventoryChanged -= RefreshAll;
        inventory.OnHotbarSelectionChanged -= HandleHotbarSelectionChanged;
    }

    void HandleHotbarSelectionChanged(int index)
    {
        RefreshHotbarSelection();
    }

    void HandleKeyboardInput()
    {
        if (Keyboard.current == null || inventory == null)
        {
            return;
        }

        bool craftingOpen = craftingHud != null && craftingHud.IsOpen;

        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            SetBackpackOpen(!backpackOpen);
        }

        if (!craftingOpen)
        {
            for (int i = 0; i < PlayerInventory.HotbarSize; i++)
            {
                if (WasDigitPressed(i))
                {
                    inventory.SetSelectedHotbarIndex(i);
                    break;
                }
            }
        }

        if (Mouse.current != null && !backpackOpen && !craftingOpen)
        {
            float scroll = Mouse.current.scroll.y.ReadValue();
            if (scroll > 0f)
            {
                int next = (inventory.SelectedHotbarIndex + PlayerInventory.HotbarSize - 1) % PlayerInventory.HotbarSize;
                inventory.SetSelectedHotbarIndex(next);
            }
            else if (scroll < 0f)
            {
                int next = (inventory.SelectedHotbarIndex + 1) % PlayerInventory.HotbarSize;
                inventory.SetSelectedHotbarIndex(next);
            }
        }
    }

    static bool WasDigitPressed(int index)
    {
        switch (index)
        {
            case 0: return Keyboard.current.digit1Key.wasPressedThisFrame;
            case 1: return Keyboard.current.digit2Key.wasPressedThisFrame;
            case 2: return Keyboard.current.digit3Key.wasPressedThisFrame;
            case 3: return Keyboard.current.digit4Key.wasPressedThisFrame;
            case 4: return Keyboard.current.digit5Key.wasPressedThisFrame;
            case 5: return Keyboard.current.digit6Key.wasPressedThisFrame;
            case 6: return Keyboard.current.digit7Key.wasPressedThisFrame;
            case 7: return Keyboard.current.digit8Key.wasPressedThisFrame;
            case 8: return Keyboard.current.digit9Key.wasPressedThisFrame;
            default: return false;
        }
    }

    void SetBackpackOpen(bool open)
    {
        backpackOpen = open;
        GameplayCursorPolicy.SetInventoryUiOpen(open);

        if (backpackRoot != null)
        {
            backpackRoot.gameObject.SetActive(open);
        }

        if (open)
        {
            BringInventoryUiToFront();
        }

        if (!open)
        {
            EndDrag(false);
            ReturnCursorHeldSafely();
            CancelRightClickTracking();
            HideItemTooltip();
        }

        RefreshAll();
    }

    void BringInventoryUiToFront()
    {
        if (hotbarRoot != null)
        {
            hotbarRoot.SetAsLastSibling();
        }

        if (backpackRoot != null)
        {
            backpackRoot.SetAsLastSibling();
        }

        if (itemTooltipRoot != null)
        {
            itemTooltipRoot.SetAsLastSibling();
        }

        if (pickupPromptRoot != null)
        {
            pickupPromptRoot.SetAsLastSibling();
        }

        if (dragGhost != null)
        {
            dragGhost.SetAsLastSibling();
        }
    }

    void OnDisable()
    {
        UnsubscribeInventory();
        ReturnCursorHeldSafely();
        CancelRightClickTracking();
        HideItemTooltip();

        if (backpackOpen)
        {
            backpackOpen = false;
            GameplayCursorPolicy.SetInventoryUiOpen(false);
        }
    }

    void BuildUi()
    {
        EnsureCanvas();
        BuildHotbar();
        BuildBackpackPanel();
        BuildPickupPrompt();
        BuildDragGhost();
        BuildItemDetailTooltip();
    }

    void EnsureCanvas()
    {
        var canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
        }

        if (GetComponent<GraphicRaycaster>() == null)
        {
            gameObject.AddComponent<GraphicRaycaster>();
        }
    }

    void BuildHotbar()
    {
        var existing = transform.Find("HotbarPanel");
        if (existing != null)
        {
            hotbarRoot = existing as RectTransform;
            CacheHotbarViews();
            if (HasValidHotbarViews())
            {
                return;
            }

            DestroyPanel("HotbarPanel");
            hotbarRoot = null;
            hotbarSlotViews = null;
        }

        var panelGo = new GameObject("HotbarPanel", typeof(RectTransform));
        panelGo.transform.SetParent(transform, false);

        hotbarRoot = panelGo.GetComponent<RectTransform>();
        hotbarRoot.anchorMin = new Vector2(0.5f, 0f);
        hotbarRoot.anchorMax = new Vector2(0.5f, 0f);
        hotbarRoot.pivot = new Vector2(0.5f, 0f);
        hotbarRoot.anchoredPosition = new Vector2(0f, 24f);

        float totalWidth = HotbarColumns * SlotSize + (HotbarColumns - 1) * SlotSpacing;
        hotbarRoot.sizeDelta = new Vector2(totalWidth, SlotSize + 8f);

        hotbarSlotViews = new InventorySlotView[HotbarColumns];
        for (int i = 0; i < HotbarColumns; i++)
        {
            hotbarSlotViews[i] = InventorySlotView.Create(
                hotbarRoot,
                $"HotbarSlot_{i}",
                i * (SlotSize + SlotSpacing),
                SlotSize,
                InventorySlotRegion.Hotbar,
                i,
                i + 1,
                this);
        }
    }

    void BuildBackpackPanel()
    {
        var existing = transform.Find("BackpackPanel");
        if (existing != null)
        {
            backpackRoot = existing as RectTransform;
            var existingBackdrop = backpackRoot.GetComponent<Image>();
            if (existingBackdrop != null)
            {
                existingBackdrop.raycastTarget = false;
            }

            CacheBackpackViews();
            if (HasValidBackpackViews())
            {
                return;
            }

            DestroyPanel("BackpackPanel");
            backpackRoot = null;
            backpackSlotViews = null;
        }

        var panelGo = new GameObject("BackpackPanel", typeof(RectTransform), typeof(Image));
        panelGo.transform.SetParent(transform, false);
        panelGo.SetActive(false);

        backpackRoot = panelGo.GetComponent<RectTransform>();
        StretchFull(backpackRoot);

        var backdrop = panelGo.GetComponent<Image>();
        backdrop.color = new Color(0f, 0f, 0f, 0.55f);
        backdrop.raycastTarget = false;

        float gridWidth = BackpackColumns * SlotSize + (BackpackColumns - 1) * SlotSpacing;
        float gridHeight = BackpackRows * SlotSize + (BackpackRows - 1) * SlotSpacing;

        var windowGo = new GameObject("Window", typeof(RectTransform), typeof(Image));
        windowGo.transform.SetParent(panelGo.transform, false);
        var windowRect = windowGo.GetComponent<RectTransform>();
        windowRect.anchorMin = new Vector2(0.5f, 0.5f);
        windowRect.anchorMax = new Vector2(0.5f, 0.5f);
        windowRect.pivot = new Vector2(0.5f, 0.5f);
        windowRect.sizeDelta = new Vector2(gridWidth + trashSlotSize + 72f, gridHeight + 96f);
        backpackWindowRect = windowRect;

        var windowImage = windowGo.GetComponent<Image>();
        windowImage.color = new Color(0.1f, 0.12f, 0.16f, 0.94f);
        windowImage.raycastTarget = true;

        var titleGo = new GameObject("Title", typeof(RectTransform), typeof(Text));
        titleGo.transform.SetParent(windowGo.transform, false);
        var titleRect = titleGo.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 1f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -12f);
        titleRect.sizeDelta = new Vector2(-24f, 28f);

        var title = titleGo.GetComponent<Text>();
        title.font = GetFont();
        title.text = "背包";
        title.fontSize = 24;
        title.fontStyle = FontStyle.Bold;
        title.alignment = TextAnchor.MiddleCenter;
        title.color = Color.white;
        title.raycastTarget = false;

        var hintGo = new GameObject("Hint", typeof(RectTransform), typeof(Text));
        hintGo.transform.SetParent(windowGo.transform, false);
        var hintRect = hintGo.GetComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(0f, 0f);
        hintRect.anchorMax = new Vector2(1f, 0f);
        hintRect.pivot = new Vector2(0.5f, 0f);
        hintRect.anchoredPosition = new Vector2(0f, 12f);
        hintRect.sizeDelta = new Vector2(-24f, 24f);

        var hint = hintGo.GetComponent<Text>();
        hint.font = GetFont();
        hint.text = "左键拖拽移动 · 拖出界面丢弃 · 拖入垃圾桶删除 · 右键拆分 · 按 B 关闭";
        hint.fontSize = 14;
        hint.alignment = TextAnchor.MiddleCenter;
        hint.color = new Color(0.82f, 0.86f, 0.92f, 0.9f);
        hint.raycastTarget = false;

        var gridGo = new GameObject("Grid", typeof(RectTransform));
        gridGo.transform.SetParent(windowGo.transform, false);
        var gridRect = gridGo.GetComponent<RectTransform>();
        gridRect.anchorMin = new Vector2(0.5f, 0.5f);
        gridRect.anchorMax = new Vector2(0.5f, 0.5f);
        gridRect.pivot = new Vector2(0.5f, 0.5f);
        gridRect.sizeDelta = new Vector2(gridWidth, gridHeight);
        gridRect.anchoredPosition = new Vector2(0f, 8f);

        backpackSlotViews = new InventorySlotView[BackpackColumns * BackpackRows];
        for (int row = 0; row < BackpackRows; row++)
        {
            for (int col = 0; col < BackpackColumns; col++)
            {
                int index = row * BackpackColumns + col;
                float x = col * (SlotSize + SlotSpacing);
                float y = -row * (SlotSize + SlotSpacing);
                backpackSlotViews[index] = InventorySlotView.Create(
                    gridRect,
                    $"BackpackSlot_{index}",
                    new Vector2(x, y),
                    SlotSize,
                    InventorySlotRegion.Backpack,
                    index,
                    -1,
                    this);
            }
        }

        BuildTrashSlot(windowGo.transform, gridWidth, gridHeight);
    }

    void BuildTrashSlot(Transform windowParent, float gridWidth, float gridHeight)
    {
        var existing = windowParent.Find("TrashSlot");
        if (existing != null)
        {
            trashSlotRect = existing as RectTransform;
            return;
        }

        var trashGo = new GameObject("TrashSlot", typeof(RectTransform), typeof(Image), typeof(InventoryTrashSlot));
        trashGo.transform.SetParent(windowParent, false);
        trashSlotRect = trashGo.GetComponent<RectTransform>();
        trashSlotRect.anchorMin = new Vector2(0.5f, 0.5f);
        trashSlotRect.anchorMax = new Vector2(0.5f, 0.5f);
        trashSlotRect.pivot = new Vector2(0f, 0.5f);
        trashSlotRect.sizeDelta = new Vector2(trashSlotSize, trashSlotSize);
        trashSlotRect.anchoredPosition = new Vector2(gridWidth * 0.5f + 20f, 8f);

        var trashImage = trashGo.GetComponent<Image>();
        trashImage.color = new Color(0.45f, 0.12f, 0.12f, 0.92f);
        trashImage.raycastTarget = true;

        var trashLabelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
        trashLabelGo.transform.SetParent(trashGo.transform, false);
        StretchFull(trashLabelGo.GetComponent<RectTransform>());
        var trashLabel = trashLabelGo.GetComponent<Text>();
        trashLabel.font = GetFont();
        trashLabel.text = "垃圾桶\n删除";
        trashLabel.fontSize = 13;
        trashLabel.alignment = TextAnchor.MiddleCenter;
        trashLabel.color = new Color(1f, 0.86f, 0.86f, 0.95f);
        trashLabel.raycastTarget = false;

        trashGo.GetComponent<InventoryTrashSlot>().Initialize(this);
    }

    void BuildPickupPrompt()
    {
        var existing = transform.Find("PickupPromptPanel");
        if (existing != null)
        {
            pickupPromptRoot = existing as RectTransform;
            pickupPromptText = pickupPromptRoot.GetComponentInChildren<Text>();
            return;
        }

        var panelGo = new GameObject("PickupPromptPanel", typeof(RectTransform), typeof(Image));
        panelGo.transform.SetParent(transform, false);

        pickupPromptRoot = panelGo.GetComponent<RectTransform>();
        pickupPromptRoot.anchorMin = new Vector2(0.5f, 0f);
        pickupPromptRoot.anchorMax = new Vector2(0.5f, 0f);
        pickupPromptRoot.pivot = new Vector2(0.5f, 0f);
        pickupPromptRoot.anchoredPosition = new Vector2(0f, 92f);
        pickupPromptRoot.sizeDelta = new Vector2(460f, 34f);

        var image = panelGo.GetComponent<Image>();
        image.color = new Color(0.05f, 0.08f, 0.12f, 0.72f);
        image.raycastTarget = false;

        var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
        textGo.transform.SetParent(panelGo.transform, false);
        StretchFull(textGo.GetComponent<RectTransform>());

        pickupPromptText = textGo.GetComponent<Text>();
        pickupPromptText.font = GetFont();
        pickupPromptText.fontSize = 18;
        pickupPromptText.alignment = TextAnchor.MiddleCenter;
        pickupPromptText.color = Color.white;
        pickupPromptText.raycastTarget = false;

        panelGo.SetActive(false);
    }

    void BuildDragGhost()
    {
        var existing = transform.Find("DragGhost");
        if (existing != null)
        {
            dragGhost = existing as RectTransform;
            dragGhostIcon = dragGhost.Find("Icon")?.GetComponent<Image>();
            dragGhostCount = dragGhost.Find("Count")?.GetComponent<Text>();
            return;
        }

        var ghostGo = new GameObject("DragGhost", typeof(RectTransform));
        ghostGo.transform.SetParent(transform, false);
        dragGhost = ghostGo.GetComponent<RectTransform>();
        dragGhost.anchorMin = new Vector2(0f, 0f);
        dragGhost.anchorMax = new Vector2(0f, 0f);
        dragGhost.pivot = new Vector2(0.5f, 0.5f);
        dragGhost.sizeDelta = new Vector2(SlotSize, SlotSize);

        dragGhostIcon = CreateImage(dragGhost, "Icon", new Color(1f, 1f, 1f, 0f));
        StretchWithPadding(dragGhostIcon.rectTransform, 6f);
        dragGhostIcon.raycastTarget = false;
        dragGhostIcon.preserveAspect = true;

        var countGo = new GameObject("Count", typeof(RectTransform), typeof(Text));
        countGo.transform.SetParent(dragGhost, false);
        var countRect = countGo.GetComponent<RectTransform>();
        countRect.anchorMin = new Vector2(1f, 0f);
        countRect.anchorMax = new Vector2(1f, 0f);
        countRect.pivot = new Vector2(1f, 0f);
        countRect.anchoredPosition = new Vector2(-4f, 2f);
        countRect.sizeDelta = new Vector2(24f, 16f);

        dragGhostCount = countGo.GetComponent<Text>();
        dragGhostCount.font = GetFont();
        dragGhostCount.fontSize = 13;
        dragGhostCount.fontStyle = FontStyle.Bold;
        dragGhostCount.alignment = TextAnchor.LowerRight;
        dragGhostCount.color = Color.white;
        dragGhostCount.raycastTarget = false;

        var ghostGroup = ghostGo.GetComponent<CanvasGroup>();
        if (ghostGroup == null)
        {
            ghostGroup = ghostGo.AddComponent<CanvasGroup>();
        }

        ghostGroup.blocksRaycasts = false;
        ghostGroup.interactable = false;

        ghostGo.SetActive(false);
    }

    void BuildItemDetailTooltip()
    {
        var existing = transform.Find("ItemDetailTooltip");
        if (existing != null)
        {
            itemTooltipRoot = existing as RectTransform;
            itemTooltipText = itemTooltipRoot.GetComponentInChildren<Text>();
            itemTooltipLayout = itemTooltipRoot.GetComponent<LayoutElement>();
            itemTooltipRoot.gameObject.SetActive(false);
            return;
        }

        var panelGo = new GameObject(
            "ItemDetailTooltip",
            typeof(RectTransform),
            typeof(Image),
            typeof(LayoutElement),
            typeof(CanvasGroup));
        panelGo.transform.SetParent(transform, false);

        itemTooltipRoot = panelGo.GetComponent<RectTransform>();
        itemTooltipRoot.anchorMin = new Vector2(0f, 0f);
        itemTooltipRoot.anchorMax = new Vector2(0f, 0f);
        itemTooltipRoot.pivot = new Vector2(0f, 1f);

        var background = panelGo.GetComponent<Image>();
        background.color = new Color(0.05f, 0.08f, 0.12f, 0.92f);
        background.raycastTarget = false;

        var canvasGroup = panelGo.GetComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        itemTooltipLayout = panelGo.GetComponent<LayoutElement>();
        itemTooltipLayout.minWidth = 96f;
        itemTooltipLayout.preferredHeight = 34f;

        var textGo = new GameObject("Name", typeof(RectTransform), typeof(Text));
        textGo.transform.SetParent(panelGo.transform, false);

        itemTooltipText = textGo.GetComponent<Text>();
        itemTooltipText.font = GetFont();
        itemTooltipText.fontSize = 17;
        itemTooltipText.fontStyle = FontStyle.Bold;
        itemTooltipText.alignment = TextAnchor.MiddleLeft;
        itemTooltipText.color = Color.white;
        itemTooltipText.raycastTarget = false;
        itemTooltipText.horizontalOverflow = HorizontalWrapMode.Overflow;
        itemTooltipText.verticalOverflow = VerticalWrapMode.Overflow;

        var textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(12f, 4f);
        textRect.offsetMax = new Vector2(-12f, -4f);

        panelGo.SetActive(false);
    }

    void ShowItemTooltip(ItemKind kind, InventorySlotRegion region, int index)
    {
        if (!backpackOpen || kind == ItemKind.None || !ItemKindUtility.IsValid(kind))
        {
            HideItemTooltip();
            return;
        }

        if (itemTooltipRoot == null || itemTooltipText == null)
        {
            BuildItemDetailTooltip();
        }

        itemTooltipRegion = region;
        itemTooltipIndex = index;
        itemTooltipRegionValid = true;
        itemTooltipVisible = true;
        itemTooltipText.text = ItemKindUtility.GetDisplayName(kind);
        Canvas.ForceUpdateCanvases();
        float textWidth = itemTooltipText.preferredWidth;
        itemTooltipRoot.sizeDelta = new Vector2(Mathf.Max(96f, textWidth + 24f), 34f);
        itemTooltipRoot.gameObject.SetActive(true);
        itemTooltipRoot.SetAsLastSibling();

        if (dragGhost != null)
        {
            dragGhost.SetAsLastSibling();
        }

        Vector2 mousePos = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        UpdateItemTooltipPosition(mousePos);
    }

    void HideItemTooltip()
    {
        itemTooltipVisible = false;
        itemTooltipRegionValid = false;
        itemTooltipIndex = -1;

        if (itemTooltipRoot != null)
        {
            itemTooltipRoot.gameObject.SetActive(false);
        }
    }

    void UpdateItemTooltipPosition(Vector2 screenPos)
    {
        if (!itemTooltipVisible || itemTooltipRoot == null)
        {
            return;
        }

        const float offsetX = 18f;
        const float offsetY = -14f;
        Vector2 position = screenPos + new Vector2(offsetX, offsetY);

        float width = itemTooltipRoot.rect.width > 1f ? itemTooltipRoot.rect.width : 120f;
        float height = itemTooltipRoot.rect.height > 1f ? itemTooltipRoot.rect.height : 34f;
        position.x = Mathf.Clamp(position.x, 4f, Screen.width - width - 4f);
        position.y = Mathf.Clamp(position.y, height + 4f, Screen.height - 4f);

        itemTooltipRoot.position = position;
    }

    void ValidateItemTooltipSlot()
    {
        if (!itemTooltipVisible || !itemTooltipRegionValid || inventory == null)
        {
            return;
        }

        InventorySlotData slot = GetSlot(itemTooltipRegion, itemTooltipIndex);
        if (slot.IsEmpty)
        {
            HideItemTooltip();
            return;
        }

        if (itemTooltipText != null)
        {
            itemTooltipText.text = ItemKindUtility.GetDisplayName(slot.Kind);
        }
    }

    bool HasValidHotbarViews()
    {
        if (hotbarSlotViews == null || hotbarSlotViews.Length != HotbarColumns)
        {
            return false;
        }

        for (int i = 0; i < hotbarSlotViews.Length; i++)
        {
            if (hotbarSlotViews[i] == null || !hotbarSlotViews[i].HasVisualRefs())
            {
                return false;
            }
        }

        return true;
    }

    bool HasValidBackpackViews()
    {
        if (backpackSlotViews == null || backpackSlotViews.Length != BackpackColumns * BackpackRows)
        {
            return false;
        }

        for (int i = 0; i < backpackSlotViews.Length; i++)
        {
            if (backpackSlotViews[i] == null || !backpackSlotViews[i].HasVisualRefs())
            {
                return false;
            }
        }

        return true;
    }

    void CacheHotbarViews()
    {
        hotbarSlotViews = new InventorySlotView[HotbarColumns];
        for (int i = 0; i < HotbarColumns; i++)
        {
            var child = hotbarRoot != null ? hotbarRoot.Find($"HotbarSlot_{i}") : null;
            if (child == null)
            {
                continue;
            }

            var view = child.GetComponent<InventorySlotView>();
            if (view == null)
            {
                view = child.gameObject.AddComponent<InventorySlotView>();
            }

            view.Configure(InventorySlotRegion.Hotbar, i, i + 1, this);
            hotbarSlotViews[i] = view;
        }
    }

    void CacheBackpackViews()
    {
        backpackSlotViews = new InventorySlotView[BackpackColumns * BackpackRows];
        var grid = backpackRoot != null ? backpackRoot.Find("Window/Grid") : null;
        if (grid == null)
        {
            return;
        }

        for (int i = 0; i < backpackSlotViews.Length; i++)
        {
            var child = grid.Find($"BackpackSlot_{i}");
            if (child == null)
            {
                continue;
            }

            var view = child.GetComponent<InventorySlotView>();
            if (view == null)
            {
                view = child.gameObject.AddComponent<InventorySlotView>();
            }

            view.Configure(InventorySlotRegion.Backpack, i, -1, this);
            backpackSlotViews[i] = view;
        }
    }

    void RefreshAll()
    {
        RefreshSlots();
        RefreshHotbarSelection();
        RefreshPickupPrompt();
        UpdateCursorGhostVisual();
        ValidateItemTooltipSlot();
    }

    void RefreshSlots()
    {
        if (inventory == null)
        {
            return;
        }

        if (hotbarSlotViews != null)
        {
            for (int i = 0; i < hotbarSlotViews.Length; i++)
            {
                hotbarSlotViews[i]?.ApplySlot(inventory.GetHotbarSlot(i));
            }
        }

        if (backpackSlotViews != null)
        {
            for (int i = 0; i < backpackSlotViews.Length; i++)
            {
                backpackSlotViews[i]?.ApplySlot(inventory.GetBackpackSlot(i));
            }
        }
    }

    void RefreshHotbarSelection()
    {
        if (inventory == null || hotbarSlotViews == null)
        {
            return;
        }

        for (int i = 0; i < hotbarSlotViews.Length; i++)
        {
            hotbarSlotViews[i]?.SetHotbarSelected(inventory.SelectedHotbarIndex == i);
        }
    }

    void RefreshPickupPrompt()
    {
        if (pickupPromptRoot == null || pickupPromptText == null)
        {
            return;
        }

        if (gameplayTargeting == null)
        {
            gameplayTargeting = FindFirstObjectByType<PlayerGameplayTargeting>();
        }

        if (gameplayTargeting != null && gameplayTargeting.HasRecoverablePlacedTarget)
        {
            string itemName = ItemKindUtility.GetDisplayName(gameplayTargeting.PlacedPickup.ItemKind);
            bool canTake = inventory != null
                && gameplayTargeting.PlacedPickup.CanRecover(inventory);
            string recoverLine = canTake
                ? $"按 F 回收：{itemName}"
                : $"按 F 回收：{itemName}（背包已满）";

            if (gameplayTargeting.HasInteractableCraftingStation)
            {
                string stationName = PlayerGameplayTargeting.GetStationDisplayName(gameplayTargeting.CraftingStation);
                GameplayChineseText.PrepareUiText(pickupPromptText, $"按 E 使用：{stationName}\n{recoverLine}");
            }
            else
            {
                GameplayChineseText.PrepareUiText(pickupPromptText, recoverLine);
            }

            pickupPromptRoot.gameObject.SetActive(true);
            return;
        }

        var toolController = pickupInteractor != null
            ? pickupInteractor.GetComponent<PlayerToolController>()
            : FindFirstObjectByType<PlayerToolController>();
        if (toolController != null && !string.IsNullOrEmpty(toolController.LastToolMessage))
        {
            GameplayChineseText.PrepareUiText(pickupPromptText, toolController.LastToolMessage);
            pickupPromptRoot.gameObject.SetActive(true);
            return;
        }

        if (gameplayTargeting != null && gameplayTargeting.HasToolInteractable && toolController != null)
        {
            string toolPrompt = toolController.GetToolPrompt();
            if (!string.IsNullOrEmpty(toolPrompt))
            {
                GameplayChineseText.PrepareUiText(pickupPromptText, toolPrompt);
                pickupPromptRoot.gameObject.SetActive(true);
                return;
            }
        }

        if (gameplayTargeting != null && gameplayTargeting.HasWorldPickup)
        {
            bool canTake = inventory != null
                && inventory.CanAcceptItem(gameplayTargeting.WorldPickup.ItemKind, gameplayTargeting.WorldPickup.Amount);
            string prompt = gameplayTargeting.GetWorldPickupPrompt(canTake);
            GameplayChineseText.PrepareUiText(pickupPromptText, prompt);
            pickupPromptRoot.gameObject.SetActive(!string.IsNullOrEmpty(prompt));
            return;
        }

        if (pickupInteractor != null && pickupInteractor.HasPlacedTarget)
        {
            string itemName = ItemKindUtility.GetDisplayName(pickupInteractor.CurrentPlacedTarget.ItemKind);
            bool canTake = pickupInteractor.CurrentPlacedTarget != null
                && pickupInteractor.CurrentPlacedTarget.CanRecover(inventory);
            string prompt = canTake
                ? $"按 F 回收：{itemName}"
                : $"按 F 回收：{itemName}（背包已满）";
            GameplayChineseText.PrepareUiText(pickupPromptText, prompt);
            pickupPromptRoot.gameObject.SetActive(true);
            return;
        }

        if (pickupInteractor != null && pickupInteractor.HasWorldTarget)
        {
            bool canTake = inventory != null
                && pickupInteractor.CurrentTarget != null
                && inventory.CanAcceptItem(pickupInteractor.CurrentTarget.ItemKind, pickupInteractor.CurrentTarget.Amount);
            string prompt = gameplayTargeting != null
                ? gameplayTargeting.GetWorldPickupPrompt(canTake)
                : string.Empty;
            GameplayChineseText.PrepareUiText(pickupPromptText, prompt);
            pickupPromptRoot.gameObject.SetActive(!string.IsNullOrEmpty(prompt));
            return;
        }

        if (gameplayTargeting != null && gameplayTargeting.HasCraftingStation)
        {
            string stationName = PlayerGameplayTargeting.GetStationDisplayName(gameplayTargeting.CraftingStation);
            GameplayChineseText.PrepareUiText(pickupPromptText, $"按 E 使用：{stationName}");
            pickupPromptRoot.gameObject.SetActive(true);
            return;
        }

        var placementController = pickupInteractor != null
            ? pickupInteractor.GetComponent<PlayerPlacementController>()
            : FindFirstObjectByType<PlayerPlacementController>();
        if (placementController != null && placementController.IsPlacementActive)
        {
            string itemName = ItemKindUtility.GetDisplayName(placementController.ActiveItem);
            GameplayChineseText.PrepareUiText(pickupPromptText, $"左键放置：{itemName}");
            pickupPromptRoot.gameObject.SetActive(true);
            return;
        }

        if (placementController != null && !string.IsNullOrEmpty(placementController.LastPlacementMessage))
        {
            GameplayChineseText.PrepareUiText(pickupPromptText, placementController.LastPlacementMessage);
            pickupPromptRoot.gameObject.SetActive(true);
            return;
        }

        var craftingInteractor = pickupInteractor != null
            ? pickupInteractor.GetComponent<PlayerCraftingInteractor>()
            : FindFirstObjectByType<PlayerCraftingInteractor>();
        if (craftingInteractor != null)
        {
            string craftingPrompt = craftingInteractor.GetPromptText();
            if (!string.IsNullOrEmpty(craftingPrompt))
            {
                GameplayChineseText.PrepareUiText(pickupPromptText, craftingPrompt);
                pickupPromptRoot.gameObject.SetActive(true);
                return;
            }
        }

        pickupPromptRoot.gameObject.SetActive(false);
    }

    internal void HandleSlotPointerEnter(InventorySlotRegion region, int index)
    {
        if (!backpackOpen || inventory == null || hasCursorHeld || isDragging)
        {
            return;
        }

        InventorySlotData slot = GetSlot(region, index);
        if (slot.IsEmpty)
        {
            HideItemTooltip();
            return;
        }

        ShowItemTooltip(slot.Kind, region, index);
    }

    internal void HandleSlotPointerExit(InventorySlotRegion region, int index)
    {
        if (!itemTooltipVisible || !itemTooltipRegionValid)
        {
            return;
        }

        if (itemTooltipRegion == region && itemTooltipIndex == index)
        {
            HideItemTooltip();
        }
    }

    internal void HandleSlotPointerClick(InventorySlotRegion region, int index, PointerEventData eventData)
    {
        if (inventory == null || !backpackOpen)
        {
            bool craftingOpen = craftingHud != null && craftingHud.IsOpen;
            if (region == InventorySlotRegion.Hotbar && inventory != null && !craftingOpen)
            {
                inventory.SetSelectedHotbarIndex(index);
            }

            return;
        }

        if (eventData != null && eventData.button == PointerEventData.InputButton.Right)
        {
            return;
        }

        if (hasCursorHeld)
        {
            TryPlaceCursorHeld(region, index);
            return;
        }

        if (region == InventorySlotRegion.Hotbar)
        {
            inventory.SetSelectedHotbarIndex(index);
        }
    }

    internal void HandleSlotPointerDown(InventorySlotRegion region, int index, PointerEventData eventData)
    {
        if (!backpackOpen || inventory == null || eventData == null)
        {
            return;
        }

        if (eventData.button != PointerEventData.InputButton.Right)
        {
            return;
        }

        InventorySlotData slot = GetSlot(region, index);
        if (slot.IsEmpty || !ItemKindUtility.IsStackable(slot.Kind))
        {
            return;
        }

        if (hasCursorHeld && cursorHeld.Kind != slot.Kind)
        {
            return;
        }

        rightClickTracking = true;
        rightClickLongPressActive = false;
        rightClickLongExtractedTotal = 0;
        rightClickStartTime = Time.unscaledTime;
        rightClickRegion = region;
        rightClickIndex = index;
    }

    internal void HandleSlotPointerUp(InventorySlotRegion region, int index, PointerEventData eventData)
    {
        // Release is handled centrally in HandleRightClickExtraction().
    }

    void HandleRightClickExtraction()
    {
        if (!backpackOpen || inventory == null || Mouse.current == null)
        {
            return;
        }

        if (rightClickTracking && Mouse.current.rightButton.isPressed)
        {
            float heldTime = Time.unscaledTime - rightClickStartTime;
            if (heldTime >= rightClickHoldThreshold)
            {
                if (!rightClickLongPressActive)
                {
                    rightClickLongPressActive = true;
                }

                int targetTotal = CalculateLongPressTargetExtract(heldTime);
                int delta = targetTotal - rightClickLongExtractedTotal;
                if (delta > 0)
                {
                    int extracted = TryExtractToCursor(rightClickRegion, rightClickIndex, delta);
                    rightClickLongExtractedTotal += extracted;
                }
            }
        }

        if (rightClickTracking && Mouse.current.rightButton.wasReleasedThisFrame)
        {
            FinishRightClickExtraction();
        }
    }

    void FinishRightClickExtraction()
    {
        if (!rightClickTracking)
        {
            return;
        }

        InventorySlotRegion sourceRegion = rightClickRegion;
        int sourceIndex = rightClickIndex;
        bool wasLongPress = rightClickLongPressActive;
        CancelRightClickTracking();

        if (!wasLongPress)
        {
            TryExtractToCursor(sourceRegion, sourceIndex, 1);
        }
    }

    int TryExtractToCursor(InventorySlotRegion region, int index, int amount)
    {
        if (inventory == null || amount <= 0)
        {
            return 0;
        }

        InventorySlotData slot = GetSlot(region, index);
        if (slot.IsEmpty)
        {
            return 0;
        }

        if (!ItemKindUtility.IsStackable(slot.Kind))
        {
            return 0;
        }

        if (hasCursorHeld && cursorHeld.Kind != slot.Kind)
        {
            return 0;
        }

        if (hasCursorHeld)
        {
            int space = ItemKindUtility.MaxStackSize - cursorHeld.Count;
            amount = Mathf.Min(amount, space);
        }

        amount = Mathf.Min(amount, slot.Count);
        if (amount <= 0)
        {
            return 0;
        }

        if (!inventory.TrySplitFromSlot(region, index, amount, out InventorySlotData extracted))
        {
            return 0;
        }

        if (!hasCursorHeld)
        {
            cursorHeld = extracted;
            hasCursorHeld = true;
            cursorHeldSourceRegion = region;
            cursorHeldSourceIndex = index;
            cursorHeldHasSource = true;
        }
        else
        {
            var held = cursorHeld;
            held.Count += extracted.Count;
            cursorHeld = held;
        }

        UpdateCursorGhostVisual();
        RefreshAll();
        return extracted.Count;
    }

    int CalculateLongPressTargetExtract(float heldTime)
    {
        if (heldTime < rightClickHoldThreshold)
        {
            return 0;
        }

        int target = 1 + Mathf.FloorToInt((heldTime - rightClickHoldThreshold) * rightClickExtractionRate);
        return Mathf.Clamp(target, 1, ItemKindUtility.MaxStackSize);
    }

    void CancelRightClickTracking()
    {
        rightClickTracking = false;
        rightClickLongPressActive = false;
        rightClickLongExtractedTotal = 0;
        rightClickIndex = -1;
    }

    void TryPlaceCursorHeld(InventorySlotRegion region, int index)
    {
        if (!hasCursorHeld || inventory == null)
        {
            return;
        }

        InventorySlotData held = cursorHeld;
        if (inventory.TryApplyHeldToSlot(region, index, ref held))
        {
            cursorHeld = held;
            hasCursorHeld = !held.IsEmpty;
            if (!hasCursorHeld)
            {
                ClearCursorHeldSource();
            }

            UpdateCursorGhostVisual();
            RefreshAll();
        }
    }

    void ReturnCursorHeldSafely()
    {
        if (!hasCursorHeld || inventory == null)
        {
            ClearCursorHeld();
            return;
        }

        InventorySlotData held = cursorHeld;

        if (cursorHeldHasSource && cursorHeldSourceIndex >= 0)
        {
            inventory.TryApplyHeldToSlot(cursorHeldSourceRegion, cursorHeldSourceIndex, ref held);
        }

        if (!held.IsEmpty)
        {
            inventory.TryReabsorbHeld(ref held);
        }

        ClearCursorHeld();
        RefreshAll();
    }

    void ClearCursorHeld()
    {
        cursorHeld = InventorySlotData.Empty;
        hasCursorHeld = false;
        ClearCursorHeldSource();
        UpdateCursorGhostVisual();
    }

    void ClearCursorHeldSource()
    {
        cursorHeldHasSource = false;
        cursorHeldSourceIndex = -1;
    }

    void UpdateCursorGhostVisual()
    {
        if (dragGhost == null || dragGhostIcon == null || dragGhostCount == null)
        {
            return;
        }

        InventorySlotData display = isDragging
            ? GetSlot(dragFromRegion, dragFromIndex)
            : hasCursorHeld ? cursorHeld : InventorySlotData.Empty;

        if (display.IsEmpty)
        {
            if (!isDragging)
            {
                dragGhost.gameObject.SetActive(false);
            }

            return;
        }

        dragGhostIcon.sprite = ItemKindUtility.GetIconSprite(display.Kind);
        dragGhostIcon.color = Color.white;

        if (hasCursorHeld && !isDragging)
        {
            dragGhostCount.text = cursorHeld.Count.ToString();
        }
        else
        {
            dragGhostCount.text = display.Count > 1 ? display.Count.ToString() : string.Empty;
        }

        dragGhost.gameObject.SetActive(true);
    }

    internal void HandleSlotBeginDrag(InventorySlotRegion region, int index, PointerEventData eventData)
    {
        HideItemTooltip();

        if (!backpackOpen || inventory == null || hasCursorHeld)
        {
            return;
        }

        InventorySlotData slot = GetSlot(region, index);
        if (slot.IsEmpty)
        {
            return;
        }

        dragFromRegion = region;
        dragFromIndex = index;
        isDragging = true;

        dragGhostIcon.sprite = ItemKindUtility.GetIconSprite(slot.Kind);
        dragGhostIcon.color = Color.white;
        dragGhostCount.text = slot.Count > 1 ? slot.Count.ToString() : string.Empty;
        dragGhost.gameObject.SetActive(true);
        UpdateDragGhostPosition(eventData.position);
    }

    internal void HandleSlotDrag(PointerEventData eventData)
    {
        if (!isDragging)
        {
            return;
        }

        UpdateDragGhostPosition(eventData.position);
    }

    internal void HandleSlotDrop(InventorySlotRegion toRegion, int toIndex, PointerEventData eventData)
    {
        if (inventory == null)
        {
            return;
        }

        if (hasCursorHeld)
        {
            TryPlaceCursorHeld(toRegion, toIndex);
            return;
        }

        if (!isDragging)
        {
            return;
        }

        if (inventory.TryMoveOrSwapSlot(dragFromRegion, dragFromIndex, toRegion, toIndex))
        {
            EndDrag(true);
            return;
        }
    }

    internal void HandleSlotEndDrag(PointerEventData eventData)
    {
        if (!isDragging)
        {
            return;
        }

        Vector2 screenPos = eventData != null ? eventData.position : Vector2.zero;
        if (IsPointerOverTrashSlot(screenPos))
        {
            TryTrashDraggedStack();
            EndDrag(true);
            return;
        }

        if (!IsPointerOverInventoryPanel(screenPos))
        {
            TryDropDraggedStackToWorld();
            EndDrag(true);
            return;
        }

        EndDrag(true);
    }

    bool IsPointerOverInventoryPanel(Vector2 screenPos)
    {
        if (backpackWindowRect != null
            && RectTransformUtility.RectangleContainsScreenPoint(backpackWindowRect, screenPos, null))
        {
            return true;
        }

        if (hotbarRoot != null
            && RectTransformUtility.RectangleContainsScreenPoint(hotbarRoot, screenPos, null))
        {
            return true;
        }

        return false;
    }

    bool IsPointerOverTrashSlot(Vector2 screenPos)
    {
        return trashSlotRect != null
            && trashSlotRect.gameObject.activeInHierarchy
            && RectTransformUtility.RectangleContainsScreenPoint(trashSlotRect, screenPos, null);
    }

    PlayerItemDropController ResolveItemDropController()
    {
        if (itemDropController == null)
        {
            itemDropController = FindFirstObjectByType<PlayerItemDropController>();
        }

        return itemDropController;
    }

    void TryDropDraggedStackToWorld()
    {
        if (inventory == null || dragFromIndex < 0)
        {
            return;
        }

        InventorySlotData slot = GetSlot(dragFromRegion, dragFromIndex);
        if (slot.IsEmpty)
        {
            return;
        }

        int amount = slot.Count;
        ItemKind kind = slot.Kind;
        if (!inventory.TryRemoveFromSlot(dragFromRegion, dragFromIndex, amount))
        {
            return;
        }

        ResolveItemDropController()?.TryDropStack(kind, amount);
        Debug.Log($"[ItemDrop] Inventory drag drop outside UI: dropped {kind} x{amount}");
    }

    void TryTrashDraggedStack()
    {
        if (inventory == null || dragFromIndex < 0)
        {
            return;
        }

        InventorySlotData slot = GetSlot(dragFromRegion, dragFromIndex);
        if (slot.IsEmpty)
        {
            return;
        }

        if (inventory.TryRemoveFromSlot(dragFromRegion, dragFromIndex, slot.Count))
        {
            Debug.Log($"[ItemDrop] Trash delete: removed {slot.Kind} x{slot.Count}");
        }
    }

    void TryDropCursorHeldToWorld()
    {
        if (!hasCursorHeld || inventory == null || cursorHeld.IsEmpty)
        {
            return;
        }

        ItemKind kind = cursorHeld.Kind;
        int amount = cursorHeld.Count;
        ClearCursorHeld();
        ResolveItemDropController()?.TryDropStack(kind, amount);
        Debug.Log($"[ItemDrop] Inventory drag drop outside UI: dropped {kind} x{amount}");
        RefreshAll();
    }

    internal void HandleTrashDropRequest()
    {
        if (hasCursorHeld)
        {
            TryTrashCursorHeld();
            return;
        }

        if (isDragging)
        {
            TryTrashDraggedStack();
            EndDrag(true);
        }
    }

    internal void TryTrashCursorHeld()
    {
        if (!hasCursorHeld || cursorHeld.IsEmpty)
        {
            return;
        }

        Debug.Log($"[ItemDrop] Trash delete: removed {cursorHeld.Kind} x{cursorHeld.Count}");
        ClearCursorHeld();
        RefreshAll();
    }

    internal void HandleTrashDrop(InventorySlotRegion fromRegion, int fromIndex)
    {
        if (inventory == null || fromIndex < 0)
        {
            return;
        }

        InventorySlotData slot = GetSlot(fromRegion, fromIndex);
        if (slot.IsEmpty)
        {
            return;
        }

        if (inventory.TryRemoveFromSlot(fromRegion, fromIndex, slot.Count))
        {
            Debug.Log($"[ItemDrop] Trash delete: removed {slot.Kind} x{slot.Count}");
            EndDrag(true);
        }
    }

    InventorySlotData GetSlot(InventorySlotRegion region, int index)
    {
        switch (region)
        {
            case InventorySlotRegion.Hotbar:
                return inventory.GetHotbarSlot(index);
            case InventorySlotRegion.Backpack:
                return inventory.GetBackpackSlot(index);
            default:
                return InventorySlotData.Empty;
        }
    }

    void EndDrag(bool refresh)
    {
        isDragging = false;
        dragFromIndex = -1;

        if (!hasCursorHeld && dragGhost != null)
        {
            dragGhost.gameObject.SetActive(false);
        }
        else
        {
            UpdateCursorGhostVisual();
        }

        if (refresh)
        {
            RefreshAll();
        }
    }

    void UpdateDragGhostPosition(Vector2 screenPos)
    {
        if (dragGhost == null)
        {
            return;
        }

        dragGhost.position = screenPos;
    }

    void DestroyPanel(string panelName)
    {
        var panel = transform.Find(panelName);
        if (panel == null)
        {
            return;
        }

        GameplayUiUtility.DestroyForRebuild(panel);
    }

    static void EnsureEventSystem()
    {
        var eventSystem = FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            var eventSystemGo = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            DontDestroyOnLoad(eventSystemGo);
            return;
        }

        if (eventSystem.GetComponent<InputSystemUIInputModule>() == null
            && eventSystem.GetComponent<StandaloneInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
        }
    }

    static Font GetFont()
    {
        return GameplayUiUtility.GetUiFont();
    }

    static Image CreateImage(Transform parent, string name, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.color = color;
        return image;
    }

    static void StretchFull(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    static void StretchWithPadding(RectTransform rect, float padding)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(padding, padding);
        rect.offsetMax = new Vector2(-padding, -padding);
    }

    sealed class InventorySlotView : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        Image iconImage;
        Image borderImage;
        Image selectionOutline;
        Text countText;

        InventorySlotRegion region;
        int index;
        PlayerInventoryHud owner;
        bool hotbarSelected;
        bool visualsBuilt;

        public static InventorySlotView Create(
            Transform parent,
            string name,
            float xOffset,
            float size,
            InventorySlotRegion slotRegion,
            int slotIndex,
            int displayIndex,
            PlayerInventoryHud hud)
        {
            return Create(parent, name, new Vector2(xOffset, 0f), size, slotRegion, slotIndex, displayIndex, hud);
        }

        public static InventorySlotView Create(
            Transform parent,
            string name,
            Vector2 anchoredPosition,
            float size,
            InventorySlotRegion slotRegion,
            int slotIndex,
            int displayIndex,
            PlayerInventoryHud hud)
        {
            var root = new GameObject(name, typeof(RectTransform));
            root.transform.SetParent(parent, false);

            var rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(size, size);

            var view = root.AddComponent<InventorySlotView>();
            view.region = slotRegion;
            view.index = slotIndex;
            view.owner = hud;
            view.Build(displayIndex);
            return view;
        }

        public void Configure(InventorySlotRegion slotRegion, int slotIndex, int displayIndex, PlayerInventoryHud hud)
        {
            region = slotRegion;
            index = slotIndex;
            owner = hud;
            EnsureVisualHierarchy(displayIndex);
        }

        public bool HasVisualRefs()
        {
            EnsureRefs();
            return iconImage != null && borderImage != null && countText != null;
        }

        void EnsureVisualHierarchy(int displayIndex)
        {
            if (visualsBuilt && HasVisualRefs())
            {
                return;
            }

            Build(displayIndex);
        }

        void Build(int displayIndex)
        {
            if (transform.Find("Background") == null)
            {
                var bg = CreateImage(transform, "Background", new Color(0.12f, 0.14f, 0.18f, 0.95f));
                StretchFull(bg.rectTransform);
                bg.raycastTarget = true;
            }

            if (region == InventorySlotRegion.Hotbar && transform.Find("SelectionOutline") == null)
            {
                selectionOutline = CreateImage(transform, "SelectionOutline", Color.black);
                var outlineRect = selectionOutline.rectTransform;
                outlineRect.anchorMin = Vector2.zero;
                outlineRect.anchorMax = Vector2.one;
                outlineRect.offsetMin = new Vector2(-3f, -3f);
                outlineRect.offsetMax = new Vector2(3f, 3f);
                selectionOutline.raycastTarget = false;
                selectionOutline.gameObject.SetActive(false);
                selectionOutline.transform.SetAsFirstSibling();
            }

            if (transform.Find("Icon") == null)
            {
                iconImage = CreateImage(transform, "Icon", new Color(1f, 1f, 1f, 0f));
                StretchWithPadding(iconImage.rectTransform, 6f);
                iconImage.raycastTarget = false;
                iconImage.preserveAspect = true;
            }

            if (transform.Find("Border") == null)
            {
                borderImage = CreateImage(transform, "Border", new Color(1f, 1f, 1f, 0.12f));
                StretchFull(borderImage.rectTransform);
                borderImage.raycastTarget = false;
            }

            if (displayIndex > 0 && transform.Find("Index") == null)
            {
                var indexGo = new GameObject("Index", typeof(RectTransform), typeof(Text));
                indexGo.transform.SetParent(transform, false);
                var indexRect = indexGo.GetComponent<RectTransform>();
                indexRect.anchorMin = new Vector2(0f, 0f);
                indexRect.anchorMax = new Vector2(0f, 0f);
                indexRect.pivot = new Vector2(0f, 0f);
                indexRect.anchoredPosition = new Vector2(4f, 2f);
                indexRect.sizeDelta = new Vector2(16f, 14f);

                var indexText = indexGo.GetComponent<Text>();
                indexText.font = GetFont();
                indexText.text = displayIndex.ToString();
                indexText.fontSize = 12;
                indexText.color = new Color(0.85f, 0.88f, 0.92f, 0.85f);
                indexText.alignment = TextAnchor.LowerLeft;
                indexText.raycastTarget = false;
            }

            if (transform.Find("Count") == null)
            {
                var countGo = new GameObject("Count", typeof(RectTransform), typeof(Text));
                countGo.transform.SetParent(transform, false);
                var countRect = countGo.GetComponent<RectTransform>();
                countRect.anchorMin = new Vector2(1f, 0f);
                countRect.anchorMax = new Vector2(1f, 0f);
                countRect.pivot = new Vector2(1f, 0f);
                countRect.anchoredPosition = new Vector2(-4f, 2f);
                countRect.sizeDelta = new Vector2(24f, 16f);

                countText = countGo.GetComponent<Text>();
                countText.font = GetFont();
                countText.fontSize = 13;
                countText.fontStyle = FontStyle.Bold;
                countText.alignment = TextAnchor.LowerRight;
                countText.color = Color.white;
                countText.raycastTarget = false;
            }

            EnsureRefs();
            visualsBuilt = true;
        }

        public void ApplySlot(InventorySlotData slot)
        {
            EnsureRefs();

            if (slot.IsEmpty)
            {
                iconImage.sprite = null;
                iconImage.color = new Color(1f, 1f, 1f, 0f);
                countText.text = string.Empty;
            }
            else
            {
                iconImage.sprite = ItemKindUtility.GetIconSprite(slot.Kind);
                iconImage.color = Color.white;
                countText.text = slot.Count > 1 ? slot.Count.ToString() : string.Empty;
            }

            borderImage.color = new Color(1f, 1f, 1f, 0.12f);
            UpdateSelectionOutline();
        }

        public void SetHotbarSelected(bool selected)
        {
            EnsureRefs();
            hotbarSelected = selected;
            UpdateSelectionOutline();
        }

        void UpdateSelectionOutline()
        {
            if (selectionOutline == null)
            {
                return;
            }

            bool showOutline = hotbarSelected && region == InventorySlotRegion.Hotbar;
            selectionOutline.gameObject.SetActive(showOutline);
        }

        void EnsureRefs()
        {
            if (iconImage == null)
            {
                iconImage = transform.Find("Icon")?.GetComponent<Image>();
            }

            if (borderImage == null)
            {
                borderImage = transform.Find("Border")?.GetComponent<Image>();
            }

            if (selectionOutline == null)
            {
                selectionOutline = transform.Find("SelectionOutline")?.GetComponent<Image>();
            }

            if (countText == null)
            {
                countText = transform.Find("Count")?.GetComponent<Text>();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            owner?.HandleSlotPointerClick(region, index, eventData);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            owner?.HandleSlotPointerDown(region, index, eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            owner?.HandleSlotPointerUp(region, index, eventData);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            owner?.HandleSlotPointerEnter(region, index);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            owner?.HandleSlotPointerExit(region, index);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            owner?.HandleSlotBeginDrag(region, index, eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            owner?.HandleSlotDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            owner?.HandleSlotEndDrag(eventData);
        }

        public void OnDrop(PointerEventData eventData)
        {
            owner?.HandleSlotDrop(region, index, eventData);
        }
    }
}
