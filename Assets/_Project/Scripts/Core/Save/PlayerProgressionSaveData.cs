using System;
using UnityEngine;

[Serializable]
public struct InventorySaveData
{
    public InventorySlotData[] Hotbar;
    public InventorySlotData[] Backpack;
    public int SelectedHotbarIndex;
    public bool HasData;
}

[Serializable]
public struct PlayerStatsSaveData
{
    public float CurrentHealth;
    public float CurrentStamina;
    public bool HasData;
}

[Serializable]
public struct PlayerProgressionSaveData
{
    public const int CurrentSaveVersion = 1;

    public int SaveDataVersion;
    public string CharacterId;
    public int WorldSeed;
    public int CurrentChapter;
    public int CurrentLevelIndex;
    public string[] UnlockedModuleIds;
    public int[] CompletedLevelIndexes;
    public string[] EncounteredEventIds;
    public GeneratedLevelSummary[] GeneratedLevels;

    public string LastSceneName;
    public InventorySaveData Inventory;
    public PlayerStatsSaveData Stats;
    public TechnologyKind[] UnlockedTechnologies;
    public GameplayAbility UnlockedAbilities;
    public bool HasPersistentState;
}
