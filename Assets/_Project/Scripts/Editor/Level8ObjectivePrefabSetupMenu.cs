#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Creates Phase B Data Core test prefabs and registers them in Level8ObjectivePrefabRegistry.
/// </summary>
public static class Level8ObjectivePrefabSetupMenu
{
    const string GenericPrefabPath =
        "Assets/_Project/Prefabs/Level8/ObjectivePrefabs/PF_Generic_Objective_DataCore_01.prefab";

    const string DataWildernessPrefabPath =
        "Assets/_Project/Prefabs/Level8/ObjectivePrefabs/PF_DataWilderness_Objective_DataCore_01.prefab";

    const string RegistryResourcePath =
        "Assets/_Project/Resources/Level8/Level8ObjectivePrefabRegistry.asset";

    const string RegistryResourceLoadPath = "Level8/Level8ObjectivePrefabRegistry";

    [MenuItem("Tools/Level8/Create Data Core Test Prefab And Registry")]
    public static void CreateDataCoreTestPrefabAndRegistry()
    {
        EnsureFolder("Assets/_Project/Prefabs/Level8/ObjectivePrefabs");
        EnsureFolder("Assets/_Project/Resources/Level8");

        GameObject genericRoot = BuildDataCoreVisualHierarchy(
            "PF_Generic_Objective_DataCore_01",
            new Color(0.35f, 0.90f, 1f),
            new Color(0.55f, 0.65f, 0.72f));

        GameObject wildernessRoot = BuildDataCoreVisualHierarchy(
            "PF_DataWilderness_Objective_DataCore_01",
            new Color(0.55f, 0.35f, 1f),
            new Color(0.45f, 0.55f, 0.62f));

        GameObject genericPrefab = SavePrefab(genericRoot, GenericPrefabPath);
        GameObject wildernessPrefab = SavePrefab(wildernessRoot, DataWildernessPrefabPath);

        Object.DestroyImmediate(genericRoot);
        Object.DestroyImmediate(wildernessRoot);

        Level8ObjectivePrefabRegistry registry = LoadOrCreateRegistry();
        SerializedObject serialized = new SerializedObject(registry);
        serialized.FindProperty("genericDataCorePrefab").objectReferenceValue = genericPrefab;

        SerializedProperty overrides = serialized.FindProperty("dataCoreBiomeOverrides");
        overrides.arraySize = 1;
        SerializedProperty entry = overrides.GetArrayElementAtIndex(0);
        entry.FindPropertyRelative("Biome").enumValueIndex = (int)Level8BiomeKind.DataWilderness;
        entry.FindPropertyRelative("Prefab").objectReferenceValue = wildernessPrefab;

        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(registry);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Level8ObjectivePrefabRegistry.SetCachedForTests(null);

        Debug.Log(
            "[Level8] Data Core test prefabs created and registered.\n" +
            $"  Generic: {GenericPrefabPath}\n" +
            $"  DataWilderness: {DataWildernessPrefabPath}\n" +
            $"  Registry: {RegistryResourcePath}\n" +
            "  Play Level8 to verify prefab visuals. Clear registry prefab refs to test primitive fallback.");
    }

    [MenuItem("Tools/Level8/Clear Data Core Prefab Registry (Test Fallback)")]
    public static void ClearDataCorePrefabRegistry()
    {
        Level8ObjectivePrefabRegistry registry = LoadOrCreateRegistry();
        SerializedObject serialized = new SerializedObject(registry);
        serialized.FindProperty("genericDataCorePrefab").objectReferenceValue = null;
        serialized.FindProperty("dataCoreBiomeOverrides").arraySize = 0;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(registry);
        AssetDatabase.SaveAssets();
        Level8ObjectivePrefabRegistry.SetCachedForTests(null);
        Debug.Log("[Level8] Data Core prefab registry cleared — primitive fallback active.");
    }

    public static void EnsureFolderPublic(string unityPath)
    {
        EnsureFolder(unityPath);
    }

    static GameObject BuildDataCoreVisualHierarchy(string rootName, Color coreColor, Color baseColor)
    {
        var root = new GameObject(rootName);

        var visual = new GameObject("Visual");
        visual.transform.SetParent(root.transform, false);

        var baseRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        baseRing.name = "Base";
        baseRing.transform.SetParent(visual.transform, false);
        baseRing.transform.localPosition = new Vector3(0f, 0.05f, 0f);
        baseRing.transform.localScale = new Vector3(2.8f, 0.08f, 2.8f);
        Object.DestroyImmediate(baseRing.GetComponent<Collider>());
        Level8VisualMaterialUtility.ApplyColor(baseRing.GetComponent<Renderer>(), new Color(baseColor.r, baseColor.g, baseColor.b, 0.55f), transparent: true);

        var core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        core.name = "Core";
        core.transform.SetParent(visual.transform, false);
        core.transform.localPosition = new Vector3(0f, 0.95f, 0f);
        core.transform.localScale = Vector3.one * 1.5f;
        Object.DestroyImmediate(core.GetComponent<Collider>());
        Level8VisualMaterialUtility.ApplyColor(core.GetComponent<Renderer>(), coreColor);

        var beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        beam.name = "GlowBeam";
        beam.transform.SetParent(visual.transform, false);
        beam.transform.localPosition = new Vector3(0f, 3.2f, 0f);
        beam.transform.localScale = new Vector3(0.22f, 2.8f, 0.22f);
        Object.DestroyImmediate(beam.GetComponent<Collider>());
        Level8VisualMaterialUtility.ApplyColor(
            beam.GetComponent<Renderer>(),
            new Color(coreColor.r, coreColor.g, coreColor.b, 0.5f),
            transparent: true);

        return root;
    }

    static GameObject SavePrefab(GameObject root, string assetPath)
    {
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (existing != null)
        {
            AssetDatabase.DeleteAsset(assetPath);
        }

        return PrefabUtility.SaveAsPrefabAsset(root, assetPath);
    }

    static Level8ObjectivePrefabRegistry LoadOrCreateRegistry()
    {
        var registry = AssetDatabase.LoadAssetAtPath<Level8ObjectivePrefabRegistry>(RegistryResourcePath);
        if (registry != null)
        {
            return registry;
        }

        registry = ScriptableObject.CreateInstance<Level8ObjectivePrefabRegistry>();
        AssetDatabase.CreateAsset(registry, RegistryResourcePath);
        return registry;
    }

    static void EnsureFolder(string unityPath)
    {
        if (AssetDatabase.IsValidFolder(unityPath))
        {
            return;
        }

        string[] parts = unityPath.Split('/');
        if (parts.Length == 0)
        {
            return;
        }

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
