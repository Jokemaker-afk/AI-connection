using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerProgressionState : MonoBehaviour
{
    [SerializeField] bool useDebugStarterInventoryWhenNoSaveState = true;
    [SerializeField] string defaultCharacterId = "player_01";
    [SerializeField] bool loadCharacterSaveFromDiskOnStart = true;
    [SerializeField] bool writeCharacterSaveToDiskOnCapture = true;

    PlayerProgressionSaveData saveData;
    bool hasCrossSceneSnapshot;
    readonly HashSet<string> unlockedModuleIdSet = new HashSet<string>();

    public bool HasPersistentState => hasCrossSceneSnapshot;
    public bool ShouldApplyDebugStarterInventory() => useDebugStarterInventoryWhenNoSaveState && !hasCrossSceneSnapshot;
    public PlayerProgressionSaveData SaveData => saveData;
    public int CurrentLevelIndex => saveData.CurrentLevelIndex;

    void Awake()
    {
        if (loadCharacterSaveFromDiskOnStart
            && CharacterSaveFileUtility.TryLoadFromDisk(defaultCharacterId, out PlayerProgressionSaveData diskData))
        {
            saveData = diskData;
            hasCrossSceneSnapshot = diskData.HasPersistentState;
            RebuildUnlockedModuleSet();
        }
    }

    public void EnsureCharacterProgressionInitialized(string sceneName)
    {
        MigrateSaveData(ref saveData);

        if (string.IsNullOrEmpty(saveData.CharacterId))
        {
            saveData.CharacterId = defaultCharacterId;
        }

        if (saveData.WorldSeed == 0)
        {
            saveData.WorldSeed = GenerateWorldSeed(saveData.CharacterId);
        }

        int level = GameplaySceneCatalog.GetLevelNumber(sceneName);
        if (level > 0)
        {
            saveData.CurrentLevelIndex = level;
        }

        if (saveData.CurrentChapter <= 0)
        {
            saveData.CurrentChapter = 1;
        }

        saveData.UnlockedModuleIds ??= Array.Empty<string>();
        saveData.CompletedLevelIndexes ??= Array.Empty<int>();
        saveData.EncounteredEventIds ??= Array.Empty<string>();
        saveData.GeneratedLevels ??= Array.Empty<GeneratedLevelSummary>();

        RebuildUnlockedModuleSet();
        UnlockModulesForLevel(Mathf.Max(1, level - 1));
    }

    public int GetWorldSeed()
    {
        if (saveData.WorldSeed == 0)
        {
            saveData.WorldSeed = GenerateWorldSeed(saveData.CharacterId);
        }

        return saveData.WorldSeed;
    }

    public GeneratedLevelSummary GetGeneratedLevelSummary(int levelIndex)
    {
        if (saveData.GeneratedLevels == null)
        {
            return default;
        }

        for (int i = 0; i < saveData.GeneratedLevels.Length; i++)
        {
            if (saveData.GeneratedLevels[i].LevelIndex == levelIndex)
            {
                return saveData.GeneratedLevels[i];
            }
        }

        return default;
    }

    public void SetGeneratedLevelSummary(GeneratedLevelSummary summary)
    {
        if (!summary.IsValid)
        {
            return;
        }

        var list = saveData.GeneratedLevels != null
            ? new List<GeneratedLevelSummary>(saveData.GeneratedLevels)
            : new List<GeneratedLevelSummary>();

        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (list[i].LevelIndex == summary.LevelIndex)
            {
                list.RemoveAt(i);
            }
        }

        list.Add(summary);
        saveData.GeneratedLevels = list.ToArray();
        hasCrossSceneSnapshot = true;
        saveData.HasPersistentState = true;
    }

    public bool AreModuleRequirementsMet(LevelModuleDefinition definition)
    {
        if (definition.RequiredUnlockedModuleIds == null || definition.RequiredUnlockedModuleIds.Length == 0)
        {
            return true;
        }

        for (int i = 0; i < definition.RequiredUnlockedModuleIds.Length; i++)
        {
            if (!unlockedModuleIdSet.Contains(definition.RequiredUnlockedModuleIds[i]))
            {
                return false;
            }
        }

        return true;
    }

    public void UnlockModule(string moduleId)
    {
        if (string.IsNullOrEmpty(moduleId) || unlockedModuleIdSet.Contains(moduleId))
        {
            return;
        }

        unlockedModuleIdSet.Add(moduleId);
        var list = saveData.UnlockedModuleIds != null
            ? new List<string>(saveData.UnlockedModuleIds)
            : new List<string>();
        list.Add(moduleId);
        saveData.UnlockedModuleIds = list.ToArray();
    }

    public void UnlockModulesForLevel(int completedLevelIndex)
    {
        foreach (LevelModuleDefinition definition in LevelModuleCatalog.AllDefinitions)
        {
            if (definition.MinLevelIndex <= completedLevelIndex)
            {
                UnlockModule(definition.ModuleId);
            }
        }

        MarkLevelCompleted(completedLevelIndex);
    }

    public void MarkLevelCompleted(int levelIndex)
    {
        if (levelIndex <= 0)
        {
            return;
        }

        var list = saveData.CompletedLevelIndexes != null
            ? new List<int>(saveData.CompletedLevelIndexes)
            : new List<int>();

        if (!list.Contains(levelIndex))
        {
            list.Add(levelIndex);
            list.Sort();
            saveData.CompletedLevelIndexes = list.ToArray();
        }
    }

    public void RegisterEncounteredEvent(string eventId)
    {
        if (string.IsNullOrEmpty(eventId))
        {
            return;
        }

        var list = saveData.EncounteredEventIds != null
            ? new List<string>(saveData.EncounteredEventIds)
            : new List<string>();

        if (!list.Contains(eventId))
        {
            list.Add(eventId);
            saveData.EncounteredEventIds = list.ToArray();
        }
    }

    public static void MigrateSaveData(ref PlayerProgressionSaveData data)
    {
        if (data.SaveDataVersion >= PlayerProgressionSaveData.CurrentSaveVersion)
        {
            return;
        }

        if (data.SaveDataVersion <= 0)
        {
            if (string.IsNullOrEmpty(data.CharacterId))
            {
                data.CharacterId = "player_01";
            }

            if (data.WorldSeed == 0 && data.HasPersistentState)
            {
                data.WorldSeed = GenerateWorldSeed(data.CharacterId);
            }

            data.UnlockedModuleIds ??= Array.Empty<string>();
            data.CompletedLevelIndexes ??= Array.Empty<int>();
            data.EncounteredEventIds ??= Array.Empty<string>();
            data.GeneratedLevels ??= Array.Empty<GeneratedLevelSummary>();
            data.CurrentChapter = Mathf.Max(1, data.CurrentChapter);
        }

        data.SaveDataVersion = PlayerProgressionSaveData.CurrentSaveVersion;
    }

    void RebuildUnlockedModuleSet()
    {
        unlockedModuleIdSet.Clear();
        if (saveData.UnlockedModuleIds == null)
        {
            return;
        }

        for (int i = 0; i < saveData.UnlockedModuleIds.Length; i++)
        {
            if (!string.IsNullOrEmpty(saveData.UnlockedModuleIds[i]))
            {
                unlockedModuleIdSet.Add(saveData.UnlockedModuleIds[i]);
            }
        }
    }

    static int GenerateWorldSeed(string characterId)
    {
        if (string.IsNullOrEmpty(characterId))
        {
            return Environment.TickCount;
        }

        unchecked
        {
            int hash = 17;
            for (int i = 0; i < characterId.Length; i++)
            {
                hash = hash * 31 + characterId[i];
            }

            return Mathf.Abs(hash);
        }
    }

    public void CaptureFromPlayer(GameObject player, string sceneName)
    {
        MigrateSaveData(ref saveData);
        EnsureCharacterProgressionInitialized(sceneName);

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

        if (writeCharacterSaveToDiskOnCapture)
        {
            CharacterSaveFileUtility.TrySaveToDisk(saveData);
        }
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

    public void ApplyBaselineAbilitiesForScene(string sceneName)
    {
        GameplayCore.Instance?.Abilities?.EnsureBaselineAbilitiesForScene(
            sceneName,
            CurrentLevelIndex);
    }

    public static void LogInventoryTransitionSnapshot(GameObject player, string sceneName, bool isLeavingScene)
    {
        if (player == null)
        {
            Debug.Log($"[SceneTransition] {(isLeavingScene ? "Leaving" : "Entering")} {sceneName}: player missing.");
            return;
        }

        var inventory = player.GetComponent<PlayerInventory>();
        if (inventory == null)
        {
            Debug.Log($"[SceneTransition] {(isLeavingScene ? "Leaving" : "Entering")} {sceneName}: inventory missing.");
            return;
        }

        string verb = isLeavingScene ? "Leaving" : "Entering";
        string backpackSummary = BuildSlotSummary(inventory, false);
        string hotbarSummary = BuildSlotSummary(inventory, true);
        int selected = inventory.SelectedHotbarIndex;
        InventorySlotData selectedSlot = inventory.GetHotbarSlot(selected);
        string selectedText = selectedSlot.IsEmpty
            ? "empty"
            : $"{(ItemCatalog.TryGet(selectedSlot.Kind, out ItemData selectedData) ? selectedData.DisplayName : selectedSlot.Kind.ToString())} x{selectedSlot.Count}";

        Debug.Log(
            $"[SceneTransition] {verb} {sceneName}: Persistent PlayerRuntimeState found on '{player.name}'. " +
            $"Backpack=[{backpackSummary}] | Toolbar=[{hotbarSummary}] | Selected slot {selected} = {selectedText}");
    }

    static string BuildSlotSummary(PlayerInventory inventory, bool hotbar)
    {
        int size = hotbar ? PlayerInventory.HotbarSize : PlayerInventory.BackpackSize;
        var parts = new System.Collections.Generic.List<string>(size);

        for (int i = 0; i < size; i++)
        {
            InventorySlotData slot = hotbar ? inventory.GetHotbarSlot(i) : inventory.GetBackpackSlot(i);
            if (slot.IsEmpty)
            {
                continue;
            }

            string label = ItemCatalog.TryGet(slot.Kind, out ItemData data) ? data.DisplayName : slot.Kind.ToString();
            parts.Add(hotbar
                ? $"slot {i}: {label} x{slot.Count}"
                : $"{label} x{slot.Count}");
        }

        return parts.Count > 0 ? string.Join(", ", parts) : "empty";
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

        if (level >= 6 && Level6TutorialSettings.GiveDebugToolsOnSceneStart)
        {
            EnsureItem(inventory, ItemKind.StonePickaxe, 1);
            EnsureItem(inventory, ItemKind.StoneAxe, 1);
            EnsureItem(inventory, ItemKind.RepairTool, 1);
            inventory.SetSelectedHotbarIndex(0);
        }

        if (level >= 6)
        {
            ApplyLevel6DebugPlacementLoadout(inventory);
        }
    }

    static void ApplyLevel6DebugPlacementLoadout(PlayerInventory inventory)
    {
        if (inventory == null)
        {
            return;
        }

        EnsureItem(inventory, ItemKind.CraftingTable, 1);
        EnsureItem(inventory, ItemKind.Furnace, 1);
        EnsureItem(inventory, ItemKind.Wood, 10);
        EnsureItem(inventory, ItemKind.Stone, 10);
        EnsureItem(inventory, ItemKind.Grass, 6);
        EnsureItem(inventory, ItemKind.Fiber, 6);

        if (inventory.CountItem(ItemKind.CraftingTable) > 0)
        {
            EnsureHotbarSelection(inventory, ItemKind.CraftingTable);
        }

        GameplayCore.Instance?.Log("Debug starter inventory created for Level 6 placement testing.");
    }

    static void EnsureHotbarSelection(PlayerInventory inventory, ItemKind kind)
    {
        if (inventory == null || !ItemKindUtility.IsValid(kind))
        {
            return;
        }

        for (int i = 0; i < PlayerInventory.HotbarSize; i++)
        {
            if (inventory.GetHotbarSlot(i).Kind == kind)
            {
                inventory.SetSelectedHotbarIndex(i);
                return;
            }
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

        int current = inventory.CountItem(kind);
        if (current >= amount)
        {
            return;
        }

        UnifiedInventoryAcquisition.TryAcquire(inventory, kind, amount - current);
    }
}
