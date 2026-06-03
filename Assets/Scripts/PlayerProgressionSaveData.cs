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
    public string LastSceneName;
    public InventorySaveData Inventory;
    public PlayerStatsSaveData Stats;
    public TechnologyKind[] UnlockedTechnologies;
    public GameplayAbility UnlockedAbilities;
    public bool HasPersistentState;
}
