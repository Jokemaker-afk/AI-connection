using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Q-key toolbar drop and shared drop spawning for inventory UI.
/// </summary>
[DisallowMultipleComponent]
public class PlayerItemDropController : MonoBehaviour
{
    [SerializeField] float dropCooldown = 0.15f;

    PlayerInventory inventory;
    float nextDropTime;

    void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
    }

    void Update()
    {
        if (ShouldBlockQDrop())
        {
            return;
        }

        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
        {
            Debug.Log("[ItemDrop] Q drop pressed");
            TryDropOneFromSelectedToolbarSlot();
        }
    }

    bool ShouldBlockQDrop()
    {
        if (GameplayCursorPolicy.IsAnyMenuOpen)
        {
            return true;
        }

        var inventoryHud = FindFirstObjectByType<PlayerInventoryHud>();
        if (inventoryHud != null && inventoryHud.IsBackpackOpen)
        {
            return true;
        }

        var craftingHud = FindFirstObjectByType<CraftingHud>();
        if (craftingHud != null && craftingHud.IsOpen)
        {
            return true;
        }

        return false;
    }

    public bool TryDropOneFromSelectedToolbarSlot()
    {
        if (inventory == null || Time.time < nextDropTime)
        {
            return false;
        }

        int slotIndex = inventory.SelectedHotbarIndex;
        InventorySlotData slot = inventory.GetHotbarSlot(slotIndex);
        if (slot.IsEmpty)
        {
            Debug.Log($"[ItemDrop] Q drop ignored: toolbar slot {slotIndex} is empty");
            return false;
        }

        bool isPlaceable = ItemKindUtility.IsPlaceable(slot.Kind);
        Debug.Log(
            $"[ItemDrop] Selected slot: {slotIndex}, {slot.Kind}, amount {slot.Count} | " +
            $"Selected item is placeable: {isPlaceable}");

        if (!inventory.TryRemoveFromSlot(InventorySlotRegion.Hotbar, slotIndex, 1))
        {
            Debug.Log("[ItemDrop] Q drop failed: could not remove 1 item from toolbar slot");
            return false;
        }

        nextDropTime = Time.time + dropCooldown;
        InventorySlotData afterSlot = inventory.GetHotbarSlot(slotIndex);
        int afterAmount = afterSlot.IsEmpty ? 0 : afterSlot.Count;
        Debug.Log(
            $"[ItemDrop] Dropped item spawned as pickup-only | Toolbar amount after drop: {afterAmount}");
        SpawnDroppedItem(slot.Kind, 1);
        return true;
    }

    public bool TryDropStack(ItemKind kind, int amount)
    {
        if (!ItemKindUtility.IsValid(kind) || amount <= 0)
        {
            return false;
        }

        SpawnDroppedItem(kind, amount);
        return true;
    }

    public void SpawnDroppedItem(ItemKind kind, int amount)
    {
        Camera camera = CrosshairRayUtility.ResolveGameplayCamera();
        Vector3 position = ItemDropUtility.ResolveDropPosition(transform, camera);
        Transform parent = ResolveDropParent();
        ItemModuleFactory.SpawnDroppedItem(kind, position, amount, parent);
    }

    static Transform ResolveDropParent()
    {
        var pickupsRoot = GameObject.Find("Pickups");
        return pickupsRoot != null ? pickupsRoot.transform : null;
    }
}
