using UnityEngine;

/// <summary>Legacy facade — delegates to <see cref="PlayerCraftingAbility"/>.</summary>
public class PlayerCraftingInteractor : MonoBehaviour
{
    PlayerCraftingAbility craftingAbility;

    public bool IsCraftingOpen => CraftingAbility.IsCraftingOpen;

    PlayerCraftingAbility CraftingAbility
    {
        get
        {
            if (craftingAbility == null)
            {
                craftingAbility = GetComponent<PlayerCraftingAbility>();
                if (craftingAbility == null)
                {
                    craftingAbility = gameObject.AddComponent<PlayerCraftingAbility>();
                }
            }

            return craftingAbility;
        }
    }

    public void BindCraftingHud(CraftingHud hud)
    {
        CraftingAbility.BindCraftingHud(hud);
    }

    public bool TryOpenCrafting()
    {
        return CraftingAbility.TryOpenWorkPanelOnE();
    }

    public string GetPromptText()
    {
        return CraftingAbility.GetWorkPanelPromptText();
    }
}
