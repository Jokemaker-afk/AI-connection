using System;
using System.Collections.Generic;
using UnityEngine;

public class RequiredPlacedObjectTracker : MonoBehaviour
{
    static RequiredPlacedObjectTracker instance;

    readonly HashSet<ItemKind> placedOnce = new HashSet<ItemKind>();

    public static RequiredPlacedObjectTracker Instance => instance;

    public event Action<ItemKind> OnItemPlacedOnce;
    public event Action OnProgressChanged;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public bool HasPlacedOnce(ItemKind itemKind)
    {
        return itemKind != ItemKind.None && placedOnce.Contains(itemKind);
    }

    public void RegisterPlacedOnce(ItemKind itemKind)
    {
        if (itemKind == ItemKind.None || !placedOnce.Add(itemKind))
        {
            return;
        }

        OnItemPlacedOnce?.Invoke(itemKind);
        OnProgressChanged?.Invoke();
    }

    public void ResetForSceneTransition()
    {
        placedOnce.Clear();
    }
}
