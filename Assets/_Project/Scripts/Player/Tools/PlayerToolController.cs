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
    PlayerHeldItemController heldItemController;

    ItemKind equippedToolKind = ItemKind.None;
    float nextUseTime;
    float toolMessageTimer;
    string lastToolMessage;

    public ItemKind EquippedToolKind => equippedToolKind;
    public bool HasEquippedTool => ItemKindUtility.IsHandheldTool(equippedToolKind);
    public string LastToolMessage => lastToolMessage;
    public float MaxToolUseDistance => maxToolUseDistance;

    void Awake()
    {
        HandheldToolDebug.Enabled = enableToolVisualDebugLogs;
        inventory = GetComponent<PlayerInventory>();
        targeting = GetComponent<PlayerGameplayTargeting>();
        heldItemController = GetComponent<PlayerHeldItemController>();
        if (heldItemController == null)
        {
            heldItemController = gameObject.AddComponent<PlayerHeldItemController>();
        }
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
        heldItemController?.RefreshHeldItem();

        if (inventory == null)
        {
            equippedToolKind = ItemKind.None;
            return;
        }

        InventorySlotData selected = inventory.GetHotbarSlot(inventory.SelectedHotbarIndex);
        if (selected.IsEmpty || !ItemKindUtility.IsHandheldTool(selected.Kind))
        {
            equippedToolKind = ItemKind.None;
            return;
        }

        equippedToolKind = selected.Kind;
        HandheldToolDebug.Log($"Selected tool: {ItemKindUtility.GetDisplayName(selected.Kind)}", this);
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

        HandheldToolVisual activeToolVisual = heldItemController?.GetActiveToolVisual();

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
        return $"{target.DisplayNameChinese}\n{prompt}\n左键使用";
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
}
