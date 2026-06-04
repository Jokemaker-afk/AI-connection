#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class CraftingRecipeAuditMenu
{
    [MenuItem("Tools/Crafting/Run Full Recipe Audit")]
    static void RunFullRecipeAudit()
    {
        CraftingRecipeAuditor.RunAuditAndLog(CraftingRecipeDatabase.AllRecipes, force: true);
        Debug.Log("[CraftingAudit] Full recipe audit complete. See Console for details.");
    }
}
#endif
