using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
/// <summary>
/// Play-mode shortcut to verify backend crafting without UI (F9 = Stone Axe pipeline test).
/// </summary>
public class CraftingDebugRuntime : MonoBehaviour
{
    void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.f9Key.wasPressedThisFrame)
        {
            return;
        }

        var inventory = GetComponent<PlayerInventory>();
        if (inventory == null)
        {
            Debug.LogWarning("[Crafting] DebugCraft skipped: PlayerInventory missing.");
            return;
        }

        EnsureStoneAxeTestMaterials(inventory);
        bool success = CraftingManager.DebugCraft(inventory, ItemKind.StoneAxe, out string message);
        Debug.Log($"[Crafting] DebugCraft StoneAxe result: {success} | {message}");
    }

    static void EnsureStoneAxeTestMaterials(PlayerInventory inventory)
    {
        GiveIfMissing(inventory, ItemKind.Stick, 2);
        GiveIfMissing(inventory, ItemKind.Stone, 3);
        GiveIfMissing(inventory, ItemKind.Rope, 1);
        GiveIfMissing(inventory, ItemKind.OreFragment, 1);
    }

    static void GiveIfMissing(PlayerInventory inventory, ItemKind kind, int amount)
    {
        int missing = amount - inventory.CountItem(kind);
        if (missing > 0)
        {
            UnifiedInventoryAcquisition.TryAcquire(inventory, kind, missing);
        }
    }
}
#endif
