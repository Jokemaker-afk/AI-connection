using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerToolController : MonoBehaviour
{
    [SerializeField] float maxToolUseDistance = 4.5f;
    [SerializeField] float toolMessageDuration = 2f;
    [SerializeField] bool enableToolVisualDebugLogs = true;

    PlayerInventory inventory;
    PlayerGameplayTargeting targeting;
    PlayerToolSockets sockets;
    PlayerCameraController cameraController;

    GameObject activeToolInstance;
    HandheldToolVisual activeToolVisual;
    ItemKind equippedToolKind = ItemKind.None;
    float nextUseTime;
    float toolMessageTimer;
    string lastToolMessage;
    bool lastFirstPersonMode;

    public ItemKind EquippedToolKind => equippedToolKind;
    public bool HasEquippedTool => ItemKindUtility.IsHandheldTool(equippedToolKind);
    public string LastToolMessage => lastToolMessage;
    public float MaxToolUseDistance => maxToolUseDistance;

    void Awake()
    {
        HandheldToolDebug.Enabled = enableToolVisualDebugLogs;
        inventory = GetComponent<PlayerInventory>();
        targeting = GetComponent<PlayerGameplayTargeting>();
        sockets = GetComponent<PlayerToolSockets>();
        if (sockets == null)
        {
            sockets = gameObject.AddComponent<PlayerToolSockets>();
        }

        cameraController = FindFirstObjectByType<PlayerCameraController>();
        lastFirstPersonMode = sockets.IsFirstPersonActive();
    }

    void OnEnable()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged += RefreshEquippedTool;
            inventory.OnHotbarSelectionChanged += HandleHotbarSelectionChanged;
        }

        RefreshEquippedTool();
    }

    void OnDisable()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= RefreshEquippedTool;
            inventory.OnHotbarSelectionChanged -= HandleHotbarSelectionChanged;
        }

        ClearEquippedToolVisual();
    }

    void Update()
    {
        UpdateToolMessageTimer();

        if (ShouldBlockToolInput())
        {
            return;
        }

        if (WasToolUseRequested())
        {
            TryUseSelectedTool();
        }
    }

    void LateUpdate()
    {
        if (sockets == null)
        {
            return;
        }

        bool firstPerson = sockets.IsFirstPersonActive();
        if (firstPerson != lastFirstPersonMode)
        {
            lastFirstPersonMode = firstPerson;
            ReparentActiveTool(firstPerson);
        }
    }

    void UpdateToolMessageTimer()
    {
        if (string.IsNullOrEmpty(lastToolMessage))
        {
            return;
        }

        toolMessageTimer -= Time.deltaTime;
        if (toolMessageTimer <= 0f)
        {
            lastToolMessage = string.Empty;
        }
    }

    void SetToolMessage(string message)
    {
        lastToolMessage = message;
        toolMessageTimer = toolMessageDuration;
    }

    void HandleHotbarSelectionChanged(int index)
    {
        RefreshEquippedTool();
    }

    public void RefreshEquippedTool()
    {
        if (inventory == null)
        {
            ClearEquippedToolVisual();
            return;
        }

        InventorySlotData selected = inventory.GetHotbarSlot(inventory.SelectedHotbarIndex);
        if (selected.IsEmpty || !ItemKindUtility.IsHandheldTool(selected.Kind))
        {
            if (equippedToolKind != ItemKind.None)
            {
                HandheldToolDebug.Log("Selected item is not a tool. Clearing handheld visual.", this);
            }

            equippedToolKind = ItemKind.None;
            ClearEquippedToolVisual();
            return;
        }

        HandheldToolDebug.Log($"Selected tool: {ItemKindUtility.GetDisplayName(selected.Kind)}", this);

        if (equippedToolKind == selected.Kind && activeToolInstance != null)
        {
            EnsureEquippedToolVisible();
            return;
        }

        equippedToolKind = selected.Kind;
        SpawnEquippedToolVisual(equippedToolKind);
    }

    public bool TryUseSelectedTool()
    {
        if (!HasEquippedTool || inventory == null)
        {
            return false;
        }

        if (Time.time < nextUseTime)
        {
            return false;
        }

        HandheldToolProfile profile = ItemKindUtility.GetHandheldToolProfile(equippedToolKind);
        ToolKind equippedTool = ItemKindUtility.GetToolKind(equippedToolKind);

        if (activeToolVisual != null)
        {
            activeToolVisual.PlayUseAnimation(() => ResolveToolHit(equippedTool));
        }
        else
        {
            HandheldToolDebug.Log("No HandheldToolVisual found. Resolving tool hit without animation.", this);
            ResolveToolHit(equippedTool);
        }

        nextUseTime = Time.time + Mathf.Max(0.1f, profile.UseCooldown);
        return true;
    }

    void ResolveToolHit(ToolKind equippedTool)
    {
        if (targeting == null)
        {
            return;
        }

        ToolInteractable target = targeting.ToolInteractable;
        if (target == null || target.IsCompleted)
        {
            HandheldToolDebug.Log("Empty swing: no valid ToolInteractable target.", this);
            return;
        }

        if (!IsWithinToolUseDistance(target))
        {
            SetToolMessage("距离太远");
            HandheldToolDebug.Log("Tool hit rejected: out of range.", this);
            return;
        }

        if (!target.CanUseTool(equippedTool, out string denyMessage))
        {
            SetToolMessage(denyMessage);
            HandheldToolDebug.Log($"Tool hit rejected: {denyMessage}", this);
            return;
        }

        ApplyToolHit(target, equippedTool);
    }

    void ApplyToolHit(ToolInteractable target, ToolKind equippedTool)
    {
        if (target == null || inventory == null)
        {
            return;
        }

        if (target.TryApplyToolHit(equippedTool, inventory, out string resultMessage))
        {
            SetToolMessage(resultMessage);
            HandheldToolDebug.Log($"Tool hit applied: {resultMessage}", this);
        }
    }

    public string GetToolPrompt()
    {
        if (!HasEquippedTool || targeting == null || targeting.ToolInteractable == null)
        {
            return string.Empty;
        }

        ToolInteractable target = targeting.ToolInteractable;
        if (target.IsCompleted)
        {
            return string.Empty;
        }

        if (!IsWithinToolUseDistance(target))
        {
            return $"{target.DisplayNameChinese}\n距离太远";
        }

        ToolKind equippedTool = ItemKindUtility.GetToolKind(equippedToolKind);
        if (!target.CanUseTool(equippedTool, out string denyMessage))
        {
            return $"{target.DisplayNameChinese}\n{denyMessage}";
        }

        string prompt = string.IsNullOrEmpty(target.InteractionPromptChinese)
            ? $"对 {target.DisplayNameChinese} 使用工具"
            : target.InteractionPromptChinese;
        return $"{target.DisplayNameChinese}\n{prompt}\n左键或 F 使用";
    }

    bool IsWithinToolUseDistance(ToolInteractable target)
    {
        if (target == null)
        {
            return false;
        }

        Vector3 flat = target.transform.position - transform.position;
        flat.y = 0f;
        return flat.magnitude <= maxToolUseDistance;
    }

    bool ShouldBlockToolInput()
    {
        if (GameplayCursorPolicy.IsAnyMenuOpen)
        {
            return true;
        }

        var craftingHud = FindFirstObjectByType<CraftingHud>();
        if (craftingHud != null && craftingHud.IsOpen)
        {
            return true;
        }

        var inventoryHud = FindFirstObjectByType<PlayerInventoryHud>();
        if (inventoryHud != null && inventoryHud.IsBackpackOpen)
        {
            return true;
        }

        var placement = GetComponent<PlayerPlacementController>();
        if (placement != null && placement.IsPlacementActive)
        {
            return true;
        }

        return false;
    }

    bool WasToolUseRequested()
    {
        return Mouse.current != null
            && Mouse.current.leftButton.wasPressedThisFrame
            && HasEquippedTool;
    }

    void SpawnEquippedToolVisual(ItemKind itemKind)
    {
        ClearEquippedToolVisual();
        sockets.EnsureSockets();

        if (!ItemCatalog.TryGet(itemKind, out ItemData itemData))
        {
            HandheldToolDebug.Log($"Tool item data not found for {itemKind}.", this);
            return;
        }

        if (!itemData.IsHandheldTool)
        {
            HandheldToolDebug.Log($"Item data for {itemKind} is not marked as handheld tool.", this);
            return;
        }

        bool firstPerson = sockets.IsFirstPersonActive();
        Transform socket = sockets.GetActiveSocket(firstPerson);
        if (socket == null)
        {
            HandheldToolDebug.Log("Tool socket missing. Cannot spawn handheld visual.", this);
            return;
        }

        activeToolInstance = ItemModuleFactory.SpawnHandheldTool(itemKind, socket, firstPerson);
        if (activeToolInstance == null)
        {
            HandheldToolDebug.Log($"Handheld prefab missing for {ItemKindUtility.GetDisplayName(itemKind)}.", this);
            return;
        }

        HandheldToolProfile profile = itemData.HandheldTool;
        ConfigureEquippedToolLabel(activeToolInstance.transform, firstPerson, itemKind);
        EnsureToolRenderersEnabled(activeToolInstance);

        activeToolVisual = activeToolInstance.GetComponent<HandheldToolVisual>();
        activeToolVisual?.PlayEquipAnimation();
        lastFirstPersonMode = firstPerson;

        HandheldToolDebug.Log(
            $"Spawned handheld prefab: {activeToolInstance.name} " +
            $"Attached to {(firstPerson ? "FirstPersonToolSocket" : "ThirdPersonToolSocket")} " +
            $"parent={socket.parent.name} " +
            $"renderers={CountEnabledRenderers(activeToolInstance)}",
            this);
    }

    void ReparentActiveTool(bool firstPerson)
    {
        if (activeToolInstance == null || sockets == null || !HasEquippedTool)
        {
            return;
        }

        sockets.EnsureSockets();
        Transform socket = sockets.GetActiveSocket(firstPerson);
        if (socket == null)
        {
            return;
        }

        activeToolInstance.transform.SetParent(socket, false);
        HandheldToolProfile profile = ItemKindUtility.GetHandheldToolProfile(equippedToolKind);
        ApplyToolTransform(activeToolInstance.transform, profile, firstPerson);
        ConfigureEquippedToolLabel(activeToolInstance.transform, firstPerson, equippedToolKind);
        EnsureToolRenderersEnabled(activeToolInstance);

        HandheldToolDebug.Log(
            $"Reattached tool to {(firstPerson ? "FirstPersonToolSocket" : "ThirdPersonToolSocket")}.",
            this);
    }

    void EnsureEquippedToolVisible()
    {
        if (activeToolInstance == null)
        {
            return;
        }

        activeToolInstance.SetActive(true);
        EnsureToolRenderersEnabled(activeToolInstance);
    }

    static void ApplyToolTransform(Transform toolTransform, HandheldToolProfile profile, bool firstPerson)
    {
        if (firstPerson)
        {
            toolTransform.localPosition = profile.FirstPersonLocalPosition;
            toolTransform.localRotation = Quaternion.Euler(profile.FirstPersonLocalEuler);
            toolTransform.localScale = profile.FirstPersonLocalScale.sqrMagnitude > 0.001f
                ? profile.FirstPersonLocalScale
                : Vector3.one;
            return;
        }

        toolTransform.localPosition = profile.ThirdPersonLocalPosition;
        toolTransform.localRotation = Quaternion.Euler(profile.ThirdPersonLocalEuler);
        toolTransform.localScale = profile.ThirdPersonLocalScale.sqrMagnitude > 0.001f
            ? profile.ThirdPersonLocalScale
            : Vector3.one;
    }

    static void ConfigureEquippedToolLabel(Transform toolRoot, bool firstPerson, ItemKind itemKind)
    {
        if (toolRoot == null)
        {
            return;
        }

        Transform visualRoot = toolRoot.Find("VisualRoot");
        if (visualRoot == null)
        {
            return;
        }

        Transform label = visualRoot.Find("Label");
        if (label != null)
        {
            label.gameObject.SetActive(!firstPerson);
        }
    }

    static void EnsureToolRenderersEnabled(GameObject toolRoot)
    {
        if (toolRoot == null)
        {
            return;
        }

        Renderer[] renderers = toolRoot.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].enabled = true;
            }
        }
    }

    static int CountEnabledRenderers(GameObject toolRoot)
    {
        if (toolRoot == null)
        {
            return 0;
        }

        int count = 0;
        Renderer[] renderers = toolRoot.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && renderers[i].enabled)
            {
                count++;
            }
        }

        return count;
    }

    void ClearEquippedToolVisual()
    {
        if (activeToolInstance != null)
        {
            Destroy(activeToolInstance);
        }

        activeToolInstance = null;
        activeToolVisual = null;
    }
}
