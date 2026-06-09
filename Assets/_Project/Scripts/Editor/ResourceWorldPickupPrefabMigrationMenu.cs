#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>Moves resource world-pickup visual prefabs from Level8 folder to shared Items/WorldPickups path (GUID-safe).</summary>
public static class ResourceWorldPickupPrefabMigrationMenu
{
    static readonly (string fileName, ItemKind kind)[] ResourcePrefabs =
    {
        ("PF_Generic_Resource_Wood_01.prefab", ItemKind.Wood),
        ("PF_Generic_Resource_Stone_01.prefab", ItemKind.Stone),
        ("PF_Generic_Resource_Fiber_01.prefab", ItemKind.Fiber),
        ("PF_Generic_Resource_OreFragment_01.prefab", ItemKind.OreFragment),
        ("PF_Generic_Resource_Coal_01.prefab", ItemKind.Coal),
        ("PF_Generic_Resource_Berry_01.prefab", ItemKind.Berry),
    };

    [MenuItem("Tools/Items/Migrate Resource Prefabs To Shared Folder")]
    public static void MigrateResourcePrefabsToSharedFolder()
    {
        EnsureFolder(ItemWorldPickupAssetPaths.ResourcesPrefabRoot);

        int moved = 0;
        int alreadyInPlace = 0;
        int missing = 0;

        for (int i = 0; i < ResourcePrefabs.Length; i++)
        {
            string fileName = ResourcePrefabs[i].fileName;
            string legacyPath = ItemWorldPickupAssetPaths.LegacyLevel8ResourcePrefabRoot + fileName;
            string sharedPath = ItemWorldPickupAssetPaths.ResourcesPrefabRoot + fileName;

            GameObject sharedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(sharedPath);
            GameObject legacyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(legacyPath);

            if (sharedPrefab != null)
            {
                alreadyInPlace++;
                if (legacyPrefab != null)
                {
                    RepointRegistryIfNeeded(ResourcePrefabs[i].kind, sharedPrefab);
                    if (AssetDatabase.DeleteAsset(legacyPath))
                    {
                        Debug.Log("[Items] Removed legacy duplicate after shared prefab exists: " + legacyPath);
                    }
                    else
                    {
                        Debug.LogWarning("[Items] Legacy duplicate remains (delete failed): " + legacyPath);
                    }
                }

                continue;
            }

            if (AssetDatabase.LoadAssetAtPath<GameObject>(legacyPath) == null)
            {
                Debug.LogWarning("[Items] Missing resource prefab (legacy and shared): " + fileName);
                missing++;
                continue;
            }

            string error = AssetDatabase.MoveAsset(legacyPath, sharedPath);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError("[Items] Failed to move " + legacyPath + " → " + sharedPath + ": " + error);
                continue;
            }

            moved++;
            Debug.Log("[Items] Moved resource prefab: " + sharedPath);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Level8ResourcePrefabRegistry.SetCachedForTests(null);

        Debug.Log(
            $"[Items] Resource prefab migration — moved: {moved}, already in shared folder: {alreadyInPlace}, missing: {missing}. " +
            "Registry GUID references preserved when move succeeded.");
    }

    static void RepointRegistryIfNeeded(ItemKind kind, GameObject sharedPrefab)
    {
        const string registryPath = "Assets/_Project/Resources/Level8/Level8ResourcePrefabRegistry.asset";
        Level8ResourcePrefabRegistry registry = AssetDatabase.LoadAssetAtPath<Level8ResourcePrefabRegistry>(registryPath);
        if (registry == null || sharedPrefab == null)
        {
            return;
        }

        SerializedObject serialized = new SerializedObject(registry);
        SerializedProperty entries = serialized.FindProperty("genericEntries");
        for (int i = 0; i < entries.arraySize; i++)
        {
            SerializedProperty entry = entries.GetArrayElementAtIndex(i);
            if ((ItemKind)entry.FindPropertyRelative("Kind").enumValueIndex != kind)
            {
                continue;
            }

            Object current = entry.FindPropertyRelative("Prefab").objectReferenceValue;
            if (current == sharedPrefab)
            {
                return;
            }

            entry.FindPropertyRelative("Prefab").objectReferenceValue = sharedPrefab;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(registry);
            Debug.Log("[Items] Repointed registry " + kind + " to shared prefab.");
            return;
        }
    }

    static void EnsureFolder(string path)
    {
        if (string.IsNullOrEmpty(path) || AssetDatabase.IsValidFolder(path.TrimEnd('/')))
        {
            return;
        }

        string trimmed = path.TrimEnd('/');
        string[] parts = trimmed.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }

            current = next;
        }
    }
}
#endif
