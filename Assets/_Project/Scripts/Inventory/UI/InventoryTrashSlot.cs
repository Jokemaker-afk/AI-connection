using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Backpack trash slot — permanently deletes dragged items.
/// </summary>
public class InventoryTrashSlot : MonoBehaviour, IDropHandler
{
    PlayerInventoryHud owner;

    public void Initialize(PlayerInventoryHud hud)
    {
        owner = hud;
    }

    public void OnDrop(PointerEventData eventData)
    {
        owner?.HandleTrashDropRequest();
    }
}
