using System;
using UnityEngine;

[Serializable]
public struct CraftingIngredient
{
    public ItemKind Item;
    public int Count;

    public CraftingIngredient(ItemKind item, int count)
    {
        Item = item;
        Count = count;
    }
}
