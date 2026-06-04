using UnityEngine;

public class PlayerCraftingInteractor : MonoBehaviour
{
    CraftingHud craftingHud;
    PlayerGameplayTargeting targeting;

    public bool IsCraftingOpen => craftingHud != null && craftingHud.IsOpen;

    void Awake()
    {
        targeting = GetComponent<PlayerGameplayTargeting>();
    }

    public void BindCraftingHud(CraftingHud hud)
    {
        craftingHud = hud;
    }

    public bool TryOpenCrafting()
    {
        if (craftingHud != null && craftingHud.IsOpen)
        {
            return true;
        }

        var inventory = GetComponent<PlayerInventory>();
        if (inventory == null)
        {
            GameplayCore.Instance?.Log("E interaction failed: PlayerInventory missing.");
            return false;
        }

        if (craftingHud == null)
        {
            GameplayCore.Instance?.Log("E interaction failed: CraftingHud not bound.");
            return false;
        }

        var contexts = WorkstationDetector.GetCraftingContexts(inventory.transform.position);
        craftingHud.OpenCrafting(inventory, contexts);
        GameplayCore.Instance?.Log($"CraftingManager active. Workstation contexts: {contexts.Count - 1}");
        return true;
    }

    public string GetPromptText()
    {
        if (IsCraftingOpen || GameplayCursorPolicy.IsAnyMenuOpen)
        {
            return string.Empty;
        }

        if (targeting == null)
        {
            targeting = GetComponent<PlayerGameplayTargeting>();
        }

        if (targeting != null && targeting.HasCraftingStation)
        {
            string name = PlayerGameplayTargeting.GetStationDisplayName(targeting.CraftingStation);
            return $"按 E 使用：{name}";
        }

        if (targeting != null && (targeting.HasWorldPickup || targeting.HasRecoverablePlacedTarget))
        {
            return string.Empty;
        }

        return "按 E 打开制造";
    }
}
