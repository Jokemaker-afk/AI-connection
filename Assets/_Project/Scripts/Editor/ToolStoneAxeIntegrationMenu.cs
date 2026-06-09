#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>Integrates StoneAxe world-pickup + handheld prefabs and registry entries.</summary>
public static class ToolStoneAxeIntegrationMenu
{
    const string RegistryPath = "Assets/_Project/Resources/Items/ItemHandheldPrefabRegistry.asset";
    const string PickupRegistryPath = "Assets/_Project/Resources/Level8/Level8ResourcePrefabRegistry.asset";
    const string MatFolder = "Assets/_Project/Art/Imported/Materials/Tools/StoneAxe";

    static readonly string[] MaterialNames =
    {
        "StoneAxe_HandleMaterial",
        "StoneAxe_StoneHeadMaterial",
        "StoneAxe_StoneEdgeMaterial",
        "StoneAxe_BindingMaterial",
    };

    [MenuItem("Tools/Items/Setup Tool StoneAxe (World Pickup + Handheld + Registry)")]
    public static void SetupComplete()
    {
        IntegrateWorldPickup();
        IntegrateHandheld();
        ResourceWorldPickupGameplayMergeMenu.MergeResourcePublic(
            ItemKind.StoneAxe,
            ItemWorldPickupAssetPaths.StoneAxeWorldPickupPrefab);
        RegisterHandheldPrefab();
        ItemHandheldPrefabRegistry.SetCachedForTests(null);
        Level8ResourcePrefabRegistry.SetCachedForTests(null);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Items] StoneAxe tool setup complete.");
    }

    [MenuItem("Tools/Items/Integrate Tool StoneAxe World Pickup Visual")]
    public static void IntegrateWorldPickupOnly()
    {
        IntegrateWorldPickup();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/Items/Integrate Tool StoneAxe Handheld Visual")]
    public static void IntegrateHandheldOnly()
    {
        IntegrateHandheld();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    static void IntegrateWorldPickup()
    {
        EnsureFolder("Assets/_Project/Art/Imported/FBX/Tools");
        EnsureFolder(MatFolder);
        EnsureFolder(ItemToolAssetPaths.WorldPickupsToolsRoot.TrimEnd('/'));

        Material[] materials = CreateMaterials();
        ConfigureFbxImporter(ItemToolAssetPaths.StoneAxeFbx, materials);
        AssetDatabase.ImportAsset(ItemToolAssetPaths.StoneAxeFbx, ImportAssetOptions.ForceUpdate);

        GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(ItemToolAssetPaths.StoneAxeFbx);
        if (fbxAsset == null)
        {
            Debug.LogError("[Items] Missing StoneAxe FBX: " + ItemToolAssetPaths.StoneAxeFbx);
            return;
        }

        string prefabPath = ItemToolAssetPaths.StoneAxeWorldPickupPrefab;
        Transform preservedLogic = DetachExistingLogicRoot(prefabPath);
        GameObject root = new GameObject("PF_Generic_Item_StoneAxe_01");
        if (preservedLogic != null)
        {
            preservedLogic.SetParent(root.transform, false);
            preservedLogic.SetSiblingIndex(0);
        }

        GameObject visual = new GameObject(SceneModuleVisualUtility.VisualName);
        visual.transform.SetParent(root.transform, false);
        if (preservedLogic != null)
        {
            visual.transform.SetSiblingIndex(1);
        }

        Transform axis = EnsureAxisCorrection(visual.transform);
        GameObject fbxInstance = (GameObject)PrefabUtility.InstantiatePrefab(fbxAsset, axis);
        fbxInstance.name = "Generic_Tool_StoneAxe_01";
        fbxInstance.transform.localPosition = Vector3.zero;
        fbxInstance.transform.localRotation = Quaternion.identity;
        fbxInstance.transform.localScale = Vector3.one;

        SceneModuleVisualUtility.EnsureBlenderAxisCorrection(root);
        StripVisualColliders(root);

        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);
        RegisterWorldPickupRegistry();
        Debug.Log("[Items] Integrated StoneAxe world pickup: " + prefabPath);
    }

    static void IntegrateHandheld()
    {
        EnsureFolder(ItemToolAssetPaths.HandheldToolsRoot.TrimEnd('/'));

        GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(ItemToolAssetPaths.StoneAxeFbx);
        if (fbxAsset == null)
        {
            Debug.LogError("[Items] Missing StoneAxe FBX for handheld: " + ItemToolAssetPaths.StoneAxeFbx);
            return;
        }

        string prefabPath = ItemToolAssetPaths.StoneAxeHandheldPrefab;
        GameObject root = new GameObject("PF_Handheld_StoneAxe_01");

        CreateEmpty(root.transform, "GripPoint", new Vector3(0f, 0f, 0f));
        CreateEmpty(root.transform, "HitOrigin", new Vector3(0f, 0.55f, 0.12f));
        CreateEmpty(root.transform, "HitDirection", new Vector3(0f, 0.55f, 0.35f));

        GameObject visualRoot = new GameObject("VisualRoot");
        visualRoot.transform.SetParent(root.transform, false);

        GameObject fbxInstance = (GameObject)PrefabUtility.InstantiatePrefab(fbxAsset, visualRoot.transform);
        fbxInstance.name = "Generic_Tool_StoneAxe_01";
        fbxInstance.transform.localPosition = Vector3.zero;
        fbxInstance.transform.localRotation = Quaternion.identity;
        fbxInstance.transform.localScale = Vector3.one;

        var visual = root.AddComponent<HandheldToolVisual>();
        HandheldToolProfile profile = ItemKindUtility.GetHandheldToolProfile(ItemKind.StoneAxe);
        visual.Configure(ItemKind.StoneAxe, profile.Animations, profile.FallbackSwingEnabled);

        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);
        Debug.Log("[Items] Integrated StoneAxe handheld: " + prefabPath);
    }

    static void RegisterHandheldPrefab()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ItemToolAssetPaths.StoneAxeHandheldPrefab);
        if (prefab == null)
        {
            Debug.LogError("[Items] Cannot register missing StoneAxe handheld prefab.");
            return;
        }

        ItemHandheldPrefabRegistry registry = AssetDatabase.LoadAssetAtPath<ItemHandheldPrefabRegistry>(RegistryPath);
        if (registry == null)
        {
            EnsureFolder("Assets/_Project/Resources/Items");
            registry = ScriptableObject.CreateInstance<ItemHandheldPrefabRegistry>();
            AssetDatabase.CreateAsset(registry, RegistryPath);
        }

        SerializedObject serialized = new SerializedObject(registry);
        SerializedProperty entries = serialized.FindProperty("entries");
        int index = FindOrAddEntryIndex(entries, ItemKind.StoneAxe);
        SerializedProperty entry = entries.GetArrayElementAtIndex(index);
        entry.FindPropertyRelative("Kind").enumValueIndex = (int)ItemKind.StoneAxe;
        entry.FindPropertyRelative("Prefab").objectReferenceValue = prefab;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(registry);
    }

    static void RegisterWorldPickupRegistry()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ItemToolAssetPaths.StoneAxeWorldPickupPrefab);
        if (prefab == null)
        {
            return;
        }

        Level8ResourcePrefabRegistry registry = AssetDatabase.LoadAssetAtPath<Level8ResourcePrefabRegistry>(PickupRegistryPath);
        if (registry == null)
        {
            EnsureFolder("Assets/_Project/Resources/Level8");
            registry = ScriptableObject.CreateInstance<Level8ResourcePrefabRegistry>();
            AssetDatabase.CreateAsset(registry, PickupRegistryPath);
        }

        SerializedObject serialized = new SerializedObject(registry);
        SerializedProperty entries = serialized.FindProperty("genericEntries");
        int index = FindOrAddEntryIndex(entries, ItemKind.StoneAxe);
        SerializedProperty entry = entries.GetArrayElementAtIndex(index);
        entry.FindPropertyRelative("Kind").enumValueIndex = (int)ItemKind.StoneAxe;
        entry.FindPropertyRelative("Prefab").objectReferenceValue = prefab;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(registry);
    }

    static Material[] CreateMaterials()
    {
        return new[]
        {
            CreateLitMaterial(MatFolder + "/StoneAxe_HandleMaterial.mat", new Color(0.52f, 0.34f, 0.18f), 0.03f, 0.72f),
            CreateLitMaterial(MatFolder + "/StoneAxe_StoneHeadMaterial.mat", new Color(0.38f, 0.42f, 0.48f), 0.08f, 0.78f),
            CreateLitMaterial(MatFolder + "/StoneAxe_StoneEdgeMaterial.mat", new Color(0.58f, 0.62f, 0.68f), 0.12f, 0.65f),
            CreateLitMaterial(MatFolder + "/StoneAxe_BindingMaterial.mat", new Color(0.62f, 0.48f, 0.28f), 0.02f, 0.68f),
        };
    }

    static Material CreateLitMaterial(string path, Color color, float metallic, float smoothness)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            EnsureFolder(MatFolder);
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }

        mat.shader = shader;
        mat.SetColor("_BaseColor", color);
        mat.SetFloat("_Metallic", metallic);
        mat.SetFloat("_Smoothness", smoothness);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static void ConfigureFbxImporter(string fbxPath, Material[] materials)
    {
        var importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
        if (importer == null)
        {
            return;
        }

        importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
        importer.materialLocation = ModelImporterMaterialLocation.External;
        importer.importCameras = false;
        importer.importLights = false;
        importer.bakeAxisConversion = true;

        for (int i = 0; i < MaterialNames.Length && i < materials.Length; i++)
        {
            importer.AddRemap(
                new AssetImporter.SourceAssetIdentifier(typeof(Material), MaterialNames[i]),
                materials[i]);
        }

        importer.SaveAndReimport();
    }

    static Transform DetachExistingLogicRoot(string prefabPath)
    {
        if (string.IsNullOrEmpty(prefabPath) || !System.IO.File.Exists(prefabPath))
        {
            return null;
        }

        GameObject existing = PrefabUtility.LoadPrefabContents(prefabPath);
        if (existing == null)
        {
            return null;
        }

        Transform logicRoot = existing.transform.Find(SceneModuleVisualUtility.LogicRootName);
        if (logicRoot == null || logicRoot.GetComponent<WorldPickupItem>() == null)
        {
            PrefabUtility.UnloadPrefabContents(existing);
            return null;
        }

        logicRoot.SetParent(null, true);
        PrefabUtility.UnloadPrefabContents(existing);
        return logicRoot;
    }

    static Transform EnsureAxisCorrection(Transform visual)
    {
        Transform axis = visual.Find(SceneModuleVisualUtility.AxisCorrectionName);
        if (axis == null)
        {
            var axisGo = new GameObject(SceneModuleVisualUtility.AxisCorrectionName);
            axis = axisGo.transform;
            axis.SetParent(visual, false);
        }

        return axis;
    }

    static void CreateEmpty(Transform parent, string name, Vector3 localPosition)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
    }

    static void StripVisualColliders(GameObject root)
    {
        Transform visual = root.transform.Find(SceneModuleVisualUtility.VisualName);
        if (visual == null)
        {
            return;
        }

        Collider[] colliders = visual.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Object.DestroyImmediate(colliders[i]);
        }
    }

    static int FindOrAddEntryIndex(SerializedProperty entries, ItemKind kind)
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

    static void EnsureFolder(string path)
    {
        if (string.IsNullOrEmpty(path) || AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string[] parts = path.Split('/');
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
