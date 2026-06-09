#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>Merges existing pickup backend (LogicRoot + WorldPickupItem + colliders) into resource world-pickup prefabs.</summary>
public static class ResourceWorldPickupGameplayMergeMenu
{
    static readonly (ItemKind kind, string prefabPath)[] ResourcePrefabs =
    {
        (ItemKind.Wood, ItemWorldPickupAssetPaths.WoodPrefab),
        (ItemKind.Stone, ItemWorldPickupAssetPaths.StonePrefab),
        (ItemKind.Fiber, ItemWorldPickupAssetPaths.FiberPrefab),
        (ItemKind.OreFragment, ItemWorldPickupAssetPaths.OreFragmentPrefab),
        (ItemKind.Coal, ItemWorldPickupAssetPaths.CoalPrefab),
        (ItemKind.Berry, ItemWorldPickupAssetPaths.BerryPrefab),
        (ItemKind.Grass, ItemWorldPickupAssetPaths.GrassPrefab),
        (ItemKind.Clay, ItemWorldPickupAssetPaths.ClayPrefab),
        (ItemKind.Flint, ItemWorldPickupAssetPaths.FlintPrefab),
        (ItemKind.Vine, ItemWorldPickupAssetPaths.VinePrefab),
        (ItemKind.StoneAxe, ItemWorldPickupAssetPaths.StoneAxeWorldPickupPrefab),
    };

    [MenuItem("Tools/Items/Merge Resource Gameplay (Wood Prototype)")]
    public static void MergeWoodPrototype()
    {
        MergeResource(ItemKind.Wood, ItemWorldPickupAssetPaths.WoodPrefab);
    }

    [MenuItem("Tools/Items/Merge Resource Gameplay (Stone Fiber)")]
    public static void MergeStoneFiber()
    {
        MergeResource(ItemKind.Stone, ItemWorldPickupAssetPaths.StonePrefab);
        MergeResource(ItemKind.Fiber, ItemWorldPickupAssetPaths.FiberPrefab);
        Debug.Log("[Items] Merged Stone and Fiber resource gameplay prefabs.");
    }

    [MenuItem("Tools/Items/Merge Resource Gameplay (OreFragment Coal Berry)")]
    public static void MergeOreCoalBerry()
    {
        MergeResource(ItemKind.OreFragment, ItemWorldPickupAssetPaths.OreFragmentPrefab);
        MergeResource(ItemKind.Coal, ItemWorldPickupAssetPaths.CoalPrefab);
        MergeResource(ItemKind.Berry, ItemWorldPickupAssetPaths.BerryPrefab);
        Debug.Log("[Items] Merged OreFragment, Coal, and Berry resource gameplay prefabs.");
    }

    [MenuItem("Tools/Items/Merge Resource Gameplay (All Six)")]
    public static void MergeAllSix()
    {
        for (int i = 0; i < ResourcePrefabs.Length; i++)
        {
            MergeResource(ResourcePrefabs[i].kind, ResourcePrefabs[i].prefabPath);
        }

        Level8ResourcePrefabRegistry.SetCachedForTests(null);
        RepointRegistryToSharedPrefabs();
        Debug.Log("[Items] Merged gameplay into all six resource world-pickup prefabs.");
    }

    public static void RepointRegistryToSharedPrefabs()
    {
        const string registryPath = "Assets/_Project/Resources/Level8/Level8ResourcePrefabRegistry.asset";
        Level8ResourcePrefabRegistry registry = AssetDatabase.LoadAssetAtPath<Level8ResourcePrefabRegistry>(registryPath);
        if (registry == null)
        {
            return;
        }

        SerializedObject serialized = new SerializedObject(registry);
        SerializedProperty entries = serialized.FindProperty("genericEntries");
        for (int i = 0; i < ResourcePrefabs.Length; i++)
        {
            ItemKind kind = ResourcePrefabs[i].kind;
            GameObject sharedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ResourcePrefabs[i].prefabPath);
            if (sharedPrefab == null)
            {
                continue;
            }

            int index = FindOrAddRegistryEntryIndex(entries, kind);
            SerializedProperty entry = entries.GetArrayElementAtIndex(index);
            entry.FindPropertyRelative("Kind").enumValueIndex = (int)kind;
            entry.FindPropertyRelative("Prefab").objectReferenceValue = sharedPrefab;
        }

        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(registry);
        AssetDatabase.SaveAssets();
        Level8ResourcePrefabRegistry.SetCachedForTests(null);
    }

    static int FindOrAddRegistryEntryIndex(SerializedProperty entries, ItemKind kind)
    {
        for (int i = 0; i < entries.arraySize; i++)
        {
            if ((ItemKind)entries.GetArrayElementAtIndex(i).FindPropertyRelative("Kind").enumValueIndex == kind)
            {
                return i;
            }
        }

        int index = entries.arraySize;
        entries.InsertArrayElementAtIndex(index);
        return index;
    }

    public static void MergeResourcePublic(ItemKind kind, string prefabPath)
    {
        MergeResource(kind, prefabPath);
        RepointRegistryToSharedPrefabs();
    }

    static void MergeResource(ItemKind kind, string prefabPath)
    {
        if (string.IsNullOrEmpty(prefabPath) || !System.IO.File.Exists(prefabPath))
        {
            Debug.LogError("[Items] Missing resource prefab for merge: " + prefabPath);
            return;
        }

        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
        if (prefabRoot == null)
        {
            Debug.LogError("[Items] Failed to load prefab contents: " + prefabPath);
            return;
        }

        try
        {
            MergeGameplayIntoPrefab(prefabRoot, kind);
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            Debug.Log("[Items] Merged resource gameplay prefab: " + prefabPath + " (" + kind + ")");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
    }

    static void MergeGameplayIntoPrefab(GameObject prefabRoot, ItemKind kind)
    {
        Transform visual = prefabRoot.transform.Find(SceneModuleVisualUtility.VisualName);
        if (visual == null)
        {
            Debug.LogError("[Items] Prefab missing Visual child: " + prefabRoot.name);
            return;
        }

        Level8ResourceVisualUtility.StripGameplayFromVisualHierarchy(visual.gameObject);

        Transform logicRoot = prefabRoot.transform.Find(SceneModuleVisualUtility.LogicRootName);
        if (logicRoot == null)
        {
            var logicGo = new GameObject(SceneModuleVisualUtility.LogicRootName);
            logicGo.transform.SetParent(prefabRoot.transform, false);
            logicRoot = logicGo.transform;
        }

        GameObject logicHost = logicRoot.gameObject;
        WorldPickupItem pickup = logicHost.GetComponent<WorldPickupItem>();
        if (pickup == null)
        {
            pickup = logicHost.AddComponent<WorldPickupItem>();
        }

        SerializedObject serializedPickup = new SerializedObject(pickup);
        serializedPickup.FindProperty("itemKind").enumValueIndex = (int)kind;
        serializedPickup.FindProperty("amount").intValue = 1;
        serializedPickup.ApplyModifiedPropertiesWithoutUndo();

        PickupItemInteractionSetup.EnsureColliders(logicHost, pickup);

        Transform interactionPoint = logicHost.transform.Find("InteractionPoint");
        if (interactionPoint == null)
        {
            var pointGo = new GameObject("InteractionPoint");
            pointGo.transform.SetParent(logicHost.transform, false);
            pointGo.transform.localPosition = Vector3.up * 0.42f;
        }

        Transform labelAnchor = logicHost.transform.Find("LabelAnchor");
        if (labelAnchor == null)
        {
            var anchorGo = new GameObject("LabelAnchor");
            anchorGo.transform.SetParent(logicHost.transform, false);
            anchorGo.transform.localPosition = Vector3.up * 0.85f;
        }

        logicRoot.SetSiblingIndex(0);
        visual.SetSiblingIndex(1);
    }
}
#endif
