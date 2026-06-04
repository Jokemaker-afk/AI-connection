using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPickupInteractor : MonoBehaviour
{
    PlayerInventory inventory;
    PlayerGameplayTargeting targeting;
    PlayerCraftingInteractor craftingInteractor;
    string lastPickupMessage;

    public WorldPickupItem CurrentTarget => targeting != null ? targeting.WorldPickup : null;
    public PlacedPickupable CurrentPlacedTarget => targeting != null ? targeting.GetPlacedTargetForF() : null;
    public bool HasTarget => HasWorldTarget || HasPlacedTarget;
    public bool HasWorldTarget => targeting != null && targeting.HasWorldPickup;
    public bool HasPlacedTarget => CurrentPlacedTarget != null;
    public string LastPickupMessage => lastPickupMessage;

    void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
        if (inventory == null)
        {
            inventory = gameObject.AddComponent<PlayerInventory>();
        }

        targeting = GetComponent<PlayerGameplayTargeting>();
        if (targeting == null)
        {
            targeting = gameObject.AddComponent<PlayerGameplayTargeting>();
        }

        EnsureCraftingInteractor();
    }

    void EnsureCraftingInteractor()
    {
        if (craftingInteractor == null)
        {
            craftingInteractor = GetComponent<PlayerCraftingInteractor>();
        }

        if (craftingInteractor == null)
        {
            craftingInteractor = gameObject.AddComponent<PlayerCraftingInteractor>();
        }
    }

    void Update()
    {
        if (Keyboard.current == null || GameplayCursorPolicy.IsAnyMenuOpen)
        {
            return;
        }

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryInteract();
        }

        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            HandlePickupOrToolKey();
        }
    }

    void HandlePickupOrToolKey()
    {
        if (TryPickupCurrentTarget())
        {
            return;
        }

        TryUseToolOnCrosshairTarget();
    }

    void TryUseToolOnCrosshairTarget()
    {
        if (targeting == null || !targeting.HasToolInteractable)
        {
            return;
        }

        var toolController = GetComponent<PlayerToolController>();
        if (toolController == null || !toolController.HasEquippedTool)
        {
            return;
        }

        toolController.TryUseSelectedTool();
    }

    void TryInteract()
    {
        EnsureCraftingInteractor();
        if (craftingInteractor == null)
        {
            return;
        }

        if (targeting != null && targeting.HasCraftingStation)
        {
            if (craftingInteractor.TryOpenCrafting())
            {
                GameplayCore.Instance?.Log(
                    $"Opened crafting UI from station: {PlayerGameplayTargeting.GetStationDisplayName(targeting.GetCraftingStationForE())}");
            }

            return;
        }

        if (targeting != null && targeting.HasWorldPickup)
        {
            return;
        }

        if (targeting != null && targeting.PlacedPickup != null)
        {
            return;
        }

        if (craftingInteractor.TryOpenCrafting())
        {
            GameplayCore.Instance?.Log("Opened crafting UI (portable / nearby workstation).");
        }
    }

    public bool CanPickupCurrentTarget()
    {
        if (inventory == null || targeting == null)
        {
            return false;
        }

        if (targeting.HasWorldPickup && targeting.CanPickupWorldTarget && CurrentTarget != null)
        {
            return UnifiedInventoryAcquisition.CanAcquire(inventory, CurrentTarget.ItemKind, CurrentTarget.Amount);
        }

        PlacedPickupable placedTarget = targeting.GetPlacedTargetForF();
        return placedTarget != null && placedTarget.CanRecover(inventory);
    }

    public bool TryPickupCurrentTarget()
    {
        lastPickupMessage = string.Empty;

        if (inventory == null || targeting == null)
        {
            return false;
        }

        if (targeting.HasWorldPickup && targeting.CanPickupWorldTarget && CurrentTarget != null)
        {
            if (CurrentTarget.TryCollect(inventory))
            {
                return true;
            }

            return false;
        }

        PlacedPickupable placedTarget = targeting.GetPlacedTargetForF();
        if (placedTarget != null && placedTarget.TryRecover(inventory, out string placedMessage))
        {
            lastPickupMessage = placedMessage;
            if (GameplayTargetingPriority.IsWorkstationPlaced(placedTarget))
            {
                GameplayCore.Instance?.Log(
                    $"[TargetPriority] Recovering workstation: {placedTarget.DisplayNameChinese}");
            }

            return true;
        }

        return false;
    }
}
