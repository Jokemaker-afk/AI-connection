using System.Collections.Generic;
using UnityEngine;

public static class LevelModuleCatalogValidation
{
    public static void ValidateAll(bool logWarnings = true)
    {
        var seen = new HashSet<string>();
        int issues = 0;

        foreach (LevelModuleDefinition definition in LevelModuleCatalog.AllDefinitions)
        {
            if (!definition.IsValid)
            {
                issues++;
                if (logWarnings)
                {
                    Debug.LogWarning("[LevelModuleCatalog] Invalid definition (empty id).");
                }

                continue;
            }

            if (!seen.Add(definition.ModuleId))
            {
                issues++;
                if (logWarnings)
                {
                    Debug.LogWarning($"[LevelModuleCatalog] Duplicate id: {definition.ModuleId}");
                }
            }

            if (definition.ModuleType == LevelModuleType.Pickup && !ItemKindUtility.IsValid(definition.PickupItemKind))
            {
                issues++;
                if (logWarnings)
                {
                    Debug.LogWarning($"[LevelModuleCatalog] Pickup module {definition.ModuleId} has invalid ItemKind.");
                }
            }
        }

        if (logWarnings)
        {
            Debug.Log($"[LevelModuleCatalog] Validation complete. Issues={issues}, Modules={seen.Count}");
        }
    }
}
