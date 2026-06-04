using System;
using UnityEngine;

[Serializable]
public struct InventorySlotData
{
    public ItemKind Kind;
    public int Count;

    public bool IsEmpty => Count <= 0;

    public static InventorySlotData Empty => new InventorySlotData { Kind = ItemKind.None, Count = 0 };

    public static InventorySlotData Of(ItemKind kind, int count = 1)
    {
        return new InventorySlotData { Kind = kind, Count = Mathf.Max(0, count) };
    }
}
