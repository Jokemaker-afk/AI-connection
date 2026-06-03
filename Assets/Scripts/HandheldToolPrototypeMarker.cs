using UnityEngine;

/// <summary>
/// Marks runtime-built handheld tool prefab versions so cache can rebuild after visual changes.
/// </summary>
public class HandheldToolPrototypeMarker : MonoBehaviour
{
    public const int CurrentVersion = 3;

    [SerializeField] int version = CurrentVersion;
    [SerializeField] ItemKind itemKind = ItemKind.None;

    public int Version => version;
    public ItemKind ItemKind => itemKind;

    public void Configure(ItemKind kind)
    {
        itemKind = kind;
        version = CurrentVersion;
    }
}
