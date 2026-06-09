using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Player ability: press E to open portable crafting / work panel with nearby station columns.
/// Not blocked by crosshair item name display or out-of-range pickup targets.
/// </summary>
public class PlayerCraftingAbility : MonoBehaviour
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

    /// <summary>Open basic player recipes plus nearby workstation columns.</summary>
    public bool TryOpenWorkPanelOnE()
    {
        if (GameplayCursorPolicy.IsAnyMenuOpen)
        {
            return false;
        }

        if (craftingHud != null && craftingHud.IsOpen)
        {
            return true;
        }

        var inventory = GetComponent<PlayerInventory>();
        if (inventory == null)
        {
            GameplayCore.Instance?.Log("E crafting failed: PlayerInventory missing.");
            return false;
        }

        if (craftingHud == null)
        {
            GameplayCore.Instance?.Log("E crafting failed: CraftingHud not bound.");
            return false;
        }

        List<WorkstationKind> contexts = WorkstationDetector.GetCraftingContexts(inventory.transform.position);
        craftingHud.OpenCrafting(inventory, contexts);
        GameplayCore.Instance?.Log($"Crafting panel opened. Workstation contexts: {contexts.Count - 1}");
        return true;
    }

    public string GetWorkPanelPromptText()
    {
        if (IsCraftingOpen || GameplayCursorPolicy.IsAnyMenuOpen)
        {
            return string.Empty;
        }

        if (targeting == null)
        {
            targeting = GetComponent<PlayerGameplayTargeting>();
        }

        List<WorkstationKind> contexts = WorkstationDetector.GetCraftingContexts(transform.position);
        bool hasNearbyStation = contexts.Count > 1;

        if (targeting != null && targeting.HasInteractableCraftingStation)
        {
            string name = PlayerGameplayTargeting.GetStationDisplayName(targeting.GetCraftingStationForE());
            return $"按 E 打开制造（{name}）";
        }

        if (hasNearbyStation)
        {
            return $"按 E 打开制造（{BuildNearbyStationSummary(contexts)}）";
        }

        return "按 E 打开制造";
    }

    static string BuildNearbyStationSummary(List<WorkstationKind> contexts)
    {
        var builder = new StringBuilder();
        for (int i = 0; i < contexts.Count; i++)
        {
            if (contexts[i] == WorkstationKind.None)
            {
                continue;
            }

            if (builder.Length > 0)
            {
                builder.Append('、');
            }

            builder.Append(WorkstationDetector.GetColumnTitle(contexts[i]));
        }

        return builder.Length > 0 ? builder.ToString() : "附近工作台";
    }
}
