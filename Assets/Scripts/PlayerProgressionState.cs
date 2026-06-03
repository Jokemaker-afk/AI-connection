using UnityEngine;

[DisallowMultipleComponent]
public class PlayerProgressionState : MonoBehaviour
{
    [SerializeField] bool useDebugStarterInventoryWhenNoSaveState = true;

    PlayerProgressionSaveData saveData;
    bool hasCrossSceneSnapshot;

    public bool HasPersistentState => hasCrossSceneSnapshot;
    public bool ShouldApplyDebugStarterInventory() => useDebugStarterInventoryWhenNoSaveState && !hasCrossSceneSnapshot;

    public void CaptureFromPlayer(GameObject player, string sceneName)
    {
        saveData.LastSceneName = sceneName;

        if (player == null)
        {
            player = PersistentPlayerRig.Player ?? GameObject.Find("Player");
        }

        if (player == null)
        {
            GameplayCore.Instance?.Log("Capture skipped: no player found.");
            return;
        }

        var inventory = player.GetComponent<PlayerInventory>();
        if (inventory != null)
        {
            saveData.Inventory = inventory.CreateSaveData();
        }

        var stats = player.GetComponent<PlayerStats>();
        if (stats != null)
        {
            saveData.Stats = new PlayerStatsSaveData
            {
                CurrentHealth = stats.CurrentHealth,
                CurrentStamina = stats.CurrentStamina,
                HasData = true,
            };
        }

        var technology = TechnologyManager.Instance;
        if (technology != null)
        {
            saveData.UnlockedTechnologies = technology.ExportUnlocked();
        }

        var core = GameplayCore.Instance;
        if (core != null && core.Abilities != null)
        {
            saveData.UnlockedAbilities = core.Abilities.ExportAbilities();
        }

        saveData.HasPersistentState = true;
        hasCrossSceneSnapshot = true;
    }

    public void ApplySavedState(GameObject player)
    {
        if (player == null)
        {
            return;
        }

        ApplyNonInventoryState(player);

        var inventory = player.GetComponent<PlayerInventory>();
        if (inventory != null && saveData.Inventory.Hotbar != null)
        {
            inventory.ApplySaveData(saveData.Inventory);
        }
    }

    public void ApplyNonInventoryState(GameObject player)
    {
        if (player == null)
        {
            return;
        }

        var stats = player.GetComponent<PlayerStats>();
        if (stats != null && saveData.Stats.HasData)
        {
            stats.SetHealth(saveData.Stats.CurrentHealth);
            stats.SetStamina(saveData.Stats.CurrentStamina);
        }

        if (saveData.UnlockedTechnologies != null && saveData.UnlockedTechnologies.Length > 0)
        {
            TechnologyManager.Instance?.ImportUnlocked(saveData.UnlockedTechnologies);
        }

        GameplayCore.Instance?.Abilities?.ImportAbilities(saveData.UnlockedAbilities);
    }

    public void LogInventoryRestoreSummary(GameObject player, string sceneName)
    {
        if (player == null)
        {
            return;
        }

        var inventory = player.GetComponent<PlayerInventory>();
        if (inventory == null)
        {
            return;
        }

        int hotbarOccupied = 0;
        int backpackOccupied = 0;
        for (int i = 0; i < PlayerInventory.HotbarSize; i++)
        {
            if (!inventory.GetHotbarSlot(i).IsEmpty)
            {
                hotbarOccupied++;
            }
        }

        for (int i = 0; i < PlayerInventory.BackpackSize; i++)
        {
            if (!inventory.GetBackpackSlot(i).IsEmpty)
            {
                backpackOccupied++;
            }
        }

        GameplayCore.Instance?.Log(
            $"Inventory restored in {sceneName}: hotbar={hotbarOccupied} slots, backpack={backpackOccupied} slots.");
    }

    public static void ApplyDefaultInheritedLoadout(GameObject player, string sceneName)
    {
        if (player == null || !GameplaySceneCatalog.IsSupportedGameplayScene(sceneName))
        {
            return;
        }

        int level = GameplaySceneCatalog.GetLevelNumber(sceneName);
        var inventory = player.GetComponent<PlayerInventory>();
        if (inventory == null)
        {
            return;
        }

        UnlockTechnologiesForLevel(level);

        if (level >= 4)
        {
            EnsureItem(inventory, ItemKind.Wood, 16);
            EnsureItem(inventory, ItemKind.Stone, 16);
        }

        if (level >= 5)
        {
            EnsureItem(inventory, ItemKind.CraftingTable, 1);
            EnsureItem(inventory, ItemKind.Furnace, 1);
            EnsureItem(inventory, ItemKind.WoodWall, 4);
            EnsureItem(inventory, ItemKind.WoodFloor, 4);
        }

        if (level >= 6)
        {
            EnsureItem(inventory, ItemKind.StonePickaxe, 1);
            EnsureItem(inventory, ItemKind.StoneAxe, 1);
            EnsureItem(inventory, ItemKind.RepairTool, 1);
            inventory.SetSelectedHotbarIndex(0);
        }
    }

    static void UnlockTechnologiesForLevel(int level)
    {
        var technology = TechnologyManager.Instance;
        if (technology == null)
        {
            return;
        }

        technology.Unlock(TechnologyKind.BasicSurvival);
        if (level >= 4)
        {
            technology.Unlock(TechnologyKind.Woodworking);
            technology.Unlock(TechnologyKind.Stonework);
        }

        if (level >= 5)
        {
            technology.Unlock(TechnologyKind.Smelting);
            technology.Unlock(TechnologyKind.Metalworking);
            technology.Unlock(TechnologyKind.Storage);
        }

        if (level >= 6)
        {
            technology.Unlock(TechnologyKind.BasicCombat);
        }
    }

    static void EnsureItem(PlayerInventory inventory, ItemKind kind, int amount)
    {
        if (inventory == null || !ItemKindUtility.IsValid(kind) || amount <= 0)
        {
            return;
        }

        if (inventory.CountItem(kind) >= amount)
        {
            return;
        }

        inventory.TryAddItem(kind, amount);
    }
}
