using UnityEngine;

public static class ItemModuleValidation
{
    public static bool ValidateItemData(ItemKind kind, bool logWarnings = true)
    {
        if (!ItemCatalog.TryGet(kind, out ItemData data) || !data.IsValid)
        {
            if (logWarnings)
            {
                Debug.LogWarning($"[ItemModule] Missing ItemData for {kind}.");
            }

            return false;
        }

        if (string.IsNullOrEmpty(data.DisplayName))
        {
            if (logWarnings)
            {
                Debug.LogWarning($"[ItemModule] Missing display name for {kind}.");
            }
        }

        if (data.IsHandheldTool && data.HandheldTool.ToolKind == ToolKind.None)
        {
            if (logWarnings)
            {
                Debug.LogWarning($"[ItemModule] Tool item {kind} missing ToolKind in HandheldToolProfile.");
            }
        }

        if (data.IsPlaceable && data.PlacedBuildingKind == BuildingKind.None && data.PlacedWorkstationKind == WorkstationKind.None)
        {
            if (logWarnings)
            {
                Debug.LogWarning($"[ItemModule] Placeable item {kind} missing building/workstation mapping.");
            }
        }

        return true;
    }

    public static void ValidateCatalog()
    {
        int issues = 0;
        foreach (ItemKind kind in ItemCatalog.AllRegisteredKinds)
        {
            if (!ValidateItemData(kind, logWarnings: false))
            {
                issues++;
            }
        }

        Debug.Log($"[ItemModule] Catalog validation complete. Issues={issues}");
    }

    public static void WarnLegacyCreationPath(string caller, ItemKind kind)
    {
        Debug.LogWarning($"[ItemModule] Legacy item creation path '{caller}' for {kind}. Prefer ItemModuleFactory.");
    }
}
