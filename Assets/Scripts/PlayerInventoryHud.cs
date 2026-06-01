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

    RectTransform hotbarRoot;
    RectTransform backpackRoot;
    RectTransform pickupPromptRoot;
    Text pickupPromptText;
    InventorySlotView[] hotbarSlotViews;
    InventorySlotView[] backpackSlotViews;

    RectTransform dragGhost;
    Image dragGhostIcon;
    Text dragGhostCount;

    bool backpackOpen;
    bool isDragging;
    InventorySlotRegion dragFromRegion;
    int dragFromIndex = -1;

    public bool IsBackpackOpen => backpackOpen;

    void Awake()
    {
        EnsureEventSystem();
        BindInventory();
        BuildUi();
    }

    void OnEnable()
    {
        BindInventory();
        SubscribeInventory();
        RefreshAll();
    }

    void OnDisable()
    {
        UnsubscribeInventory();
    }

    void Update()
    {
        HandleKeyboardInput();
        RefreshPickupPrompt();

        if (isDragging)
        {
            UpdateDragGhostPosition(Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero);
        }
    }

    public void RebuildUi()
    {
        DestroyPanel("HotbarPanel");
        DestroyPanel("BackpackPanel");
        DestroyPanel("PickupPromptPanel");
        DestroyPanel("DragGhost");
        hotbarRoot = null;
        backpackRoot = null;
        pickupPromptRoot = null;
        dragGhost = null;
        hotbarSlotViews = null;
        backpackSlotViews = null;
        BuildUi();
        RefreshAll();
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

        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            SetBackpackOpen(!backpackOpen);
        }

        for (int i = 0; i < PlayerInventory.HotbarSize; i++)
        {
            if (WasDigitPressed(i))
            {
                inventory.SetSelectedHotbarIndex(i);
                break;
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
        if (backpackRoot != null)
        {
            backpackRoot.gameObject.SetActive(open);
        }

        if (!open)
        {
            EndDrag(false);
        }

        RefreshAll();
    }

    void BuildUi()
    {
        EnsureCanvas();
        BuildHotbar();
        BuildBackpackPanel();
        BuildPickupPrompt();
        BuildDragGhost();
    }

    void EnsureCanvas()
    {
        var canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;

            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

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
            return;
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
            CacheBackpackViews();
            return;
        }

        var panelGo = new GameObject("BackpackPanel", typeof(RectTransform), typeof(Image));
        panelGo.transform.SetParent(transform, false);
        panelGo.SetActive(false);

        backpackRoot = panelGo.GetComponent<RectTransform>();
        StretchFull(backpackRoot);

        var backdrop = panelGo.GetComponent<Image>();
        backdrop.color = new Color(0f, 0f, 0f, 0.55f);
        backdrop.raycastTarget = true;

        float gridWidth = BackpackColumns * SlotSize + (BackpackColumns - 1) * SlotSpacing;
        float gridHeight = BackpackRows * SlotSize + (BackpackRows - 1) * SlotSpacing;

        var windowGo = new GameObject("Window", typeof(RectTransform), typeof(Image));
        windowGo.transform.SetParent(panelGo.transform, false);
        var windowRect = windowGo.GetComponent<RectTransform>();
        windowRect.anchorMin = new Vector2(0.5f, 0.5f);
        windowRect.anchorMax = new Vector2(0.5f, 0.5f);
        windowRect.pivot = new Vector2(0.5f, 0.5f);
        windowRect.sizeDelta = new Vector2(gridWidth + 48f, gridHeight + 96f);

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
        hint.text = "拖拽物品到目标格：同类自动堆叠，异类交换，按 B 关闭";
        hint.fontSize = 14;
        hint.alignment = TextAnchor.MiddleCenter;
        hint.color = new Color(0.82f, 0.86f, 0.92f, 0.9f);

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

        ghostGo.SetActive(false);
    }

    void CacheHotbarViews()
    {
        hotbarSlotViews = new InventorySlotView[HotbarColumns];
        for (int i = 0; i < HotbarColumns; i++)
        {
            var child = hotbarRoot.Find($"HotbarSlot_{i}");
            if (child != null)
            {
                hotbarSlotViews[i] = child.GetComponent<InventorySlotView>();
            }
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
            if (child != null)
            {
                backpackSlotViews[i] = child.GetComponent<InventorySlotView>();
            }
        }
    }

    void RefreshAll()
    {
        RefreshSlots();
        RefreshHotbarSelection();
        RefreshPickupPrompt();
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

        if (pickupInteractor != null && pickupInteractor.HasTarget)
        {
            var target = pickupInteractor.CurrentTarget;
            string itemName = ItemKindUtility.GetDisplayName(target.ItemKind);
            bool canTake = inventory != null && inventory.CanAcceptItem(target.ItemKind, target.Amount);
            pickupPromptText.text = canTake
                ? $"按 F 拾取 {itemName}"
                : $"按 F 拾取 {itemName}（背包已满）";
            pickupPromptRoot.gameObject.SetActive(true);
            return;
        }

        pickupPromptRoot.gameObject.SetActive(false);
    }

    internal void HandleSlotPointerClick(InventorySlotRegion region, int index)
    {
        if (inventory == null)
        {
            return;
        }

        if (region == InventorySlotRegion.Hotbar)
        {
            inventory.SetSelectedHotbarIndex(index);
        }
    }

    internal void HandleSlotBeginDrag(InventorySlotRegion region, int index, PointerEventData eventData)
    {
        if (!backpackOpen || inventory == null)
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
        if (!isDragging || inventory == null)
        {
            return;
        }

        inventory.TryMoveOrSwapSlot(dragFromRegion, dragFromIndex, toRegion, toIndex);
    }

    internal void HandleSlotEndDrag(PointerEventData eventData)
    {
        EndDrag(true);
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
        if (dragGhost != null)
        {
            dragGhost.gameObject.SetActive(false);
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

        if (Application.isPlaying)
        {
            Destroy(panel.gameObject);
        }
        else
        {
            DestroyImmediate(panel.gameObject);
        }
    }

    static void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        var eventSystemGo = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        DontDestroyOnLoad(eventSystemGo);
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

    sealed class InventorySlotView : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        Image iconImage;
        Image borderImage;
        Text countText;

        InventorySlotRegion region;
        int index;
        PlayerInventoryHud owner;
        bool hotbarSelected;

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

        void Build(int displayIndex)
        {
            var bg = CreateImage(transform, "Background", new Color(0.12f, 0.14f, 0.18f, 0.95f));
            StretchFull(bg.rectTransform);
            bg.raycastTarget = true;

            iconImage = CreateImage(transform, "Icon", new Color(1f, 1f, 1f, 0f));
            StretchWithPadding(iconImage.rectTransform, 6f);
            iconImage.raycastTarget = false;
            iconImage.preserveAspect = true;

            borderImage = CreateImage(transform, "Border", new Color(1f, 1f, 1f, 0.12f));
            StretchFull(borderImage.rectTransform);
            borderImage.raycastTarget = false;

            if (displayIndex > 0)
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

            borderImage.color = hotbarSelected
                ? new Color(1f, 1f, 1f, 0.92f)
                : new Color(1f, 1f, 1f, 0.12f);
        }

        public void SetHotbarSelected(bool selected)
        {
            EnsureRefs();
            hotbarSelected = selected;
            borderImage.color = selected
                ? new Color(1f, 1f, 1f, 0.92f)
                : new Color(1f, 1f, 1f, 0.12f);
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

            if (countText == null)
            {
                countText = transform.Find("Count")?.GetComponent<Text>();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            owner?.HandleSlotPointerClick(region, index);
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
