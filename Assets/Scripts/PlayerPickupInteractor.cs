using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPickupInteractor : MonoBehaviour
{
    PlayerInventory inventory;
    PlayerGameplayTargeting targeting;
    PlayerCraftingInteractor craftingInteractor;
    string lastPickupMessage;

    public WorldPickupItem CurrentTarget => targeting != null ? targeting.WorldPickup : null;
    public PlacedPickupable CurrentPlacedTarget => targeting != null ? targeting.PlacedPickup : null;
    public bool HasTarget => HasWorldTarget || HasPlacedTarget;
    public bool HasWorldTarget => targeting != null && targeting.HasWorldPickup;
    public bool HasPlacedTarget => targeting != null && targeting.HasPlacedPickup;
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
            var toolController = GetComponent<PlayerToolController>();
            if (toolController != null
                && toolController.HasEquippedTool
                && toolController.TryUseSelectedTool())
            {
                return;
            }

            TryPickupCurrentTarget();
        }
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
                GameplayCore.Instance?.Log($"Opened crafting UI from station: {PlayerGameplayTargeting.GetStationDisplayName(targeting.CraftingStation)}");
            }

            return;
        }

        if (targeting != null && (targeting.HasWorldPickup || targeting.HasPlacedPickup))
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
        if (HasPlacedTarget)
        {
            return inventory != null && inventory.CanAcceptItem(CurrentPlacedTarget.ItemKind, 1);
        }

        if (!HasWorldTarget || inventory == null)
        {
            return false;
        }

        if (!targeting.CanPickupWorldTarget)
        {
            return false;
        }

        return inventory.CanAcceptItem(CurrentTarget.ItemKind, CurrentTarget.Amount);
    }

    public bool TryPickupCurrentTarget()
    {
        lastPickupMessage = string.Empty;

        if (inventory == null || targeting == null)
        {
            return false;
        }

        if (HasPlacedTarget && CurrentPlacedTarget.TryPickup(inventory, out string placedMessage))
        {
            lastPickupMessage = placedMessage;
            return true;
        }

        if (!HasWorldTarget || !targeting.CanPickupWorldTarget)
        {
            return false;
        }

        if (CurrentTarget.TryCollect(inventory))
        {
            return true;
        }

        return false;
    }
}
