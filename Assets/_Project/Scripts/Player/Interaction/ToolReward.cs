using System;
using UnityEngine;

[Serializable]
public struct ToolReward
{
    public ItemKind ItemKind;
    public int MinAmount;
    public int MaxAmount;
    [Range(0f, 1f)] public float DropChance;

    public ToolReward(ItemKind itemKind, int minAmount, int maxAmount = -1, float dropChance = 1f)
    {
        ItemKind = itemKind;
        MinAmount = minAmount;
        MaxAmount = maxAmount < 0 ? minAmount : maxAmount;
        DropChance = dropChance;
    }

    public int RollAmount()
    {
        int min = Mathf.Max(1, MinAmount);
        int max = Mathf.Max(min, MaxAmount);
        return UnityEngine.Random.Range(min, max + 1);
    }

    public bool ShouldDrop()
    {
        return ItemKind != ItemKind.None && (DropChance >= 1f || UnityEngine.Random.value <= DropChance);
    }
}
