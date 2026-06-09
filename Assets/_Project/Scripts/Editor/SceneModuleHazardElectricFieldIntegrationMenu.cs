#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

/// <summary>Integrates Generic_Hazard_ElectricField_01 into visual + full hazard module prefabs.</summary>
public static class SceneModuleHazardElectricFieldIntegrationMenu
{
    const string FbxPath = "Assets/_Project/Art/Imported/FBX/SceneModules/Hazards/Generic_Hazard_ElectricField_01.fbx";
    const string MatFolder = "Assets/_Project/Art/Imported/Materials/SceneModules/Hazards/ElectricField";
    const string VisualPrefabPath = "Assets/_Project/Prefabs/SceneModules/Hazards/PF_Generic_Hazard_ElectricField_01_Visual.prefab";
    const string ModulePrefabPath = "Assets/_Project/Prefabs/SceneModules/Hazards/PF_Generic_Hazard_ElectricField_01.prefab";
    const string RegistryPath = "Assets/_Project/Resources/SceneModules/SceneModulePrefabRegistry.asset";

    [MenuItem("Tools/SceneModules/Restore ElectricField Visual From FBX")]
    public static void RestoreElectricFieldVisualFromFbx()
    {
        EnsureFolder(MatFolder);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        Material baseMat = CreateLitMaterial(MatFolder + "/ElectricField_BaseMaterial.mat", new Color(0.10f, 0.12f, 0.14f), 0.55f, 0.65f);
        Material postMat = CreateLitMaterial(MatFolder + "/ElectricField_PostMaterial.mat", new Color(0.18f, 0.20f, 0.24f), 0.65f, 0.45f);
        Material laserMat = CreateEmissiveMaterial(MatFolder + "/ElectricField_LaserMaterial.mat", new Color(0.15f, 0.85f, 1f), new Color(0.2f, 0.95f, 1f) * 3f);
        Material arcMat = CreateEmissiveMaterial(MatFolder + "/ElectricField_ArcMaterial.mat", new Color(0.7f, 0.95f, 1f), new Color(0.5f, 0.9f, 1f) * 4f);
        Material fieldMat = CreateTransparentMaterial(MatFolder + "/ElectricField_FieldPlaneMaterial.mat", new Color(0.1f, 0.75f, 0.95f, 0.12f), new Color(0.1f, 0.6f, 0.9f) * 1.5f);
        Material warningMat = CreateLitMaterial(MatFolder + "/ElectricField_WarningMaterial.mat", new Color(0.92f, 0.78f, 0.05f), 0.1f, 0.4f);

        ConfigureFbxImporter(FbxPath, baseMat, postMat, laserMat, arcMat, fieldMat, warningMat);
        AssetDatabase.ImportAsset(FbxPath, ImportAssetOptions.ForceUpdate);

        GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
        if (fbxAsset == null)
        {
            Debug.LogError("[SceneModules] Missing electric field FBX: " + FbxPath);
            return;
        }

        BuildVisualPrefab(fbxAsset);
        RestoreVisualOnModulePrefab(fbxAsset);
        GameObject visualPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(VisualPrefabPath);
        if (visualPrefab == null)
        {
            Debug.LogError("[SceneModules] Failed to create visual prefab: " + VisualPrefabPath);
            return;
        }

        RegisterPrefab(visualPrefab, SceneModuleKind.ElectricField);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        SceneModulePrefabRegistry.SetCachedForTests(null);
        Debug.Log("[SceneModules] Restored electric field visual from FBX on " + ModulePrefabPath);
    }

    static void RestoreVisualOnModulePrefab(GameObject fbxAsset)
    {
        GameObject moduleRoot = PrefabUtility.LoadPrefabContents(ModulePrefabPath);
        if (moduleRoot == null)
        {
            Debug.LogError("[SceneModules] Missing module prefab: " + ModulePrefabPath);
            return;
        }

        try
        {
            IntegrateFbxUnderModuleVisual(moduleRoot, fbxAsset, new Vector3(0f, -0.06f, 0f));
            SceneModuleVisualUtility.FlattenAxisCorrectionLayer(moduleRoot);

            Transform logicRoot = moduleRoot.transform.Find(SceneModuleVisualUtility.LogicRootName);
            LaserHazard laser = logicRoot != null ? logicRoot.GetComponent<LaserHazard>() : null;
            if (laser != null)
            {
                SerializedObject serializedLaser = new SerializedObject(laser);
                serializedLaser.FindProperty("moduleVisualApplied").boolValue = true;
                serializedLaser.ApplyModifiedPropertiesWithoutUndo();
                laser.SyncModuleLayout(forceCollider: true, forceWarning: true);
            }

            PrefabUtility.SaveAsPrefabAsset(moduleRoot, ModulePrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(moduleRoot);
        }
    }

    static void IntegrateFbxUnderModuleVisual(GameObject moduleRoot, GameObject fbxAsset, Vector3 visualLocalPosition)
    {
        RemoveLegacyVisualContainers(moduleRoot);

        Transform visual = HazardModuleLayoutUtility.EnsureChild(moduleRoot.transform, SceneModuleVisualUtility.VisualName);
        visual.localPosition = visualLocalPosition;
        visual.localRotation = Quaternion.identity;
        visual.localScale = Vector3.one;
        ClearChildren(visual);

        GameObject fbxInstance = (GameObject)PrefabUtility.InstantiatePrefab(fbxAsset, visual);
        fbxInstance.name = "Generic_Hazard_ElectricField_01";
        fbxInstance.transform.localPosition = Vector3.zero;
        fbxInstance.transform.localRotation = Quaternion.identity;
        fbxInstance.transform.localScale = Vector3.one;

        SceneModuleVisualUtility.EnsureBlenderAxisCorrection(moduleRoot);
        StripColliders(moduleRoot);
    }

    static void RemoveLegacyVisualContainers(GameObject moduleRoot)
    {
        Transform legacyVisualRoot = moduleRoot.transform.Find(SceneModuleVisualUtility.VisualRootName);
        if (legacyVisualRoot != null)
        {
            Object.DestroyImmediate(legacyVisualRoot.gameObject);
        }
    }

    static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(parent.GetChild(i).gameObject);
        }
    }

    [MenuItem("Tools/SceneModules/Integrate Hazard ElectricField Module")]
    public static void Integrate()
    {
        EnsureFolder(MatFolder);
        EnsureFolder("Assets/_Project/Art/Imported/FBX/SceneModules/Hazards");
        EnsureFolder("Assets/_Project/Prefabs/SceneModules/Hazards");
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        Material baseMat = CreateLitMaterial(MatFolder + "/ElectricField_BaseMaterial.mat", new Color(0.10f, 0.12f, 0.14f), 0.55f, 0.65f);
        Material postMat = CreateLitMaterial(MatFolder + "/ElectricField_PostMaterial.mat", new Color(0.18f, 0.20f, 0.24f), 0.65f, 0.45f);
        Material laserMat = CreateEmissiveMaterial(MatFolder + "/ElectricField_LaserMaterial.mat", new Color(0.15f, 0.85f, 1f), new Color(0.2f, 0.95f, 1f) * 3f);
        Material arcMat = CreateEmissiveMaterial(MatFolder + "/ElectricField_ArcMaterial.mat", new Color(0.7f, 0.95f, 1f), new Color(0.5f, 0.9f, 1f) * 4f);
        Material fieldMat = CreateTransparentMaterial(MatFolder + "/ElectricField_FieldPlaneMaterial.mat", new Color(0.1f, 0.75f, 0.95f, 0.12f), new Color(0.1f, 0.6f, 0.9f) * 1.5f);
        Material warningMat = CreateLitMaterial(MatFolder + "/ElectricField_WarningMaterial.mat", new Color(0.92f, 0.78f, 0.05f), 0.1f, 0.4f);

        ConfigureFbxImporter(FbxPath, baseMat, postMat, laserMat, arcMat, fieldMat, warningMat);
        AssetDatabase.ImportAsset(FbxPath, ImportAssetOptions.ForceUpdate);

        GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
        if (fbxAsset == null)
        {
            Debug.LogError("[SceneModules] Missing electric field FBX: " + FbxPath);
            return;
        }

        GameObject visualPrefab = BuildVisualPrefab(fbxAsset);
        visualPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(VisualPrefabPath);

        GameObject modulePrefab = BuildModulePrefab(fbxAsset);

        RegisterPrefab(visualPrefab, SceneModuleKind.ElectricField);
        RegisterPrefab(modulePrefab, SceneModuleKind.ElectricFieldHazard);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        SceneModulePrefabRegistry.SetCachedForTests(null);
        Debug.Log("[SceneModules] Integrated electric field module at " + ModulePrefabPath);
    }

    [MenuItem("Tools/SceneModules/Create ElectricField Hazard Test Prefab And Registry")]
    public static void CreateElectricFieldTestPrefabAndRegistry()
    {
        Integrate();
    }

    [MenuItem("Tools/SceneModules/Register ElectricField Hazard Prefab")]
    public static void RegisterElectricFieldHazardPrefab()
    {
        Integrate();
    }

    [MenuItem("Tools/SceneModules/Clear ElectricField Hazard Prefab Registry (Test Fallback)")]
    public static void ClearElectricFieldPrefabRegistry()
    {
        SceneModulePrefabRegistry registry = AssetDatabase.LoadAssetAtPath<SceneModulePrefabRegistry>(RegistryPath);
        if (registry == null)
        {
            Debug.LogWarning("[SceneModules] Registry missing: " + RegistryPath);
            return;
        }

        SerializedObject serialized = new SerializedObject(registry);
        SerializedProperty entries = serialized.FindProperty("entries");
        for (int i = entries.arraySize - 1; i >= 0; i--)
        {
            SceneModuleKind kind = (SceneModuleKind)entries.GetArrayElementAtIndex(i)
                .FindPropertyRelative("Kind").enumValueIndex;
            if (kind == SceneModuleKind.ElectricField || kind == SceneModuleKind.ElectricFieldHazard)
            {
                entries.DeleteArrayElementAtIndex(i);
            }
        }

        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(registry);
        AssetDatabase.SaveAssets();
        SceneModulePrefabRegistry.SetCachedForTests(null);
        Debug.Log("[SceneModules] Cleared ElectricField / ElectricFieldHazard registry entries — LaserHazard will use primitive fallback.");
    }

    static GameObject BuildVisualPrefab(GameObject fbxAsset)
    {
        GameObject prefabRoot = new GameObject("PF_Generic_Hazard_ElectricField_01_Visual");
        GameObject visual = new GameObject(SceneModuleVisualUtility.VisualName);
        visual.transform.SetParent(prefabRoot.transform, false);

        GameObject fbxInstance = (GameObject)PrefabUtility.InstantiatePrefab(fbxAsset, visual.transform);
        fbxInstance.name = "Generic_Hazard_ElectricField_01";
        fbxInstance.transform.localPosition = Vector3.zero;
        fbxInstance.transform.localRotation = Quaternion.identity;
        fbxInstance.transform.localScale = Vector3.one;

        SceneModuleVisualUtility.EnsureBlenderAxisCorrection(prefabRoot);
        StripColliders(prefabRoot);

        GameObject saved = PrefabUtility.SaveAsPrefabAsset(prefabRoot, VisualPrefabPath);
        Object.DestroyImmediate(prefabRoot);
        return saved;
    }

    static GameObject BuildModulePrefab(GameObject fbxAsset)
    {
        GameObject moduleRoot = new GameObject("PF_Generic_Hazard_ElectricField_01");

        GameObject logicRoot = new GameObject(SceneModuleVisualUtility.LogicRootName);
        logicRoot.transform.SetParent(moduleRoot.transform, false);
        BoxCollider box = logicRoot.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = new Vector3(SceneModuleVisualUtility.ElectricFieldVisualWidth, 0.12f, 0.35f);
        box.center = Vector3.zero;
        logicRoot.AddComponent<LaserHazard>();

        IntegrateFbxUnderModuleVisual(moduleRoot, fbxAsset, new Vector3(0f, -0.06f, 0f));

        GameObject warningRoot = new GameObject(HazardModuleLayoutUtility.WarningAreaName);
        warningRoot.transform.SetParent(moduleRoot.transform, false);

        GameObject labelAnchor = new GameObject(HazardModuleLayoutUtility.LabelAnchorName);
        labelAnchor.transform.SetParent(moduleRoot.transform, false);
        labelAnchor.transform.localPosition = new Vector3(0f, 1.2f, 0f);

        GameObject debugBounds = new GameObject(HazardModuleLayoutUtility.DebugBoundsName);
        debugBounds.transform.SetParent(moduleRoot.transform, false);
        debugBounds.SetActive(false);

        LaserHazard laser = logicRoot.GetComponent<LaserHazard>();
        if (laser != null)
        {
            SerializedObject serializedLaser = new SerializedObject(laser);
            serializedLaser.FindProperty("moduleVisualApplied").boolValue = true;
            serializedLaser.FindProperty("autoSyncColliderToVisual").boolValue = true;
            serializedLaser.FindProperty("autoSyncWarningArea").boolValue = true;
            serializedLaser.ApplyModifiedPropertiesWithoutUndo();
            laser.Configure(
                new Vector3(SceneModuleVisualUtility.ElectricFieldVisualWidth, 0.12f, 0.35f),
                12f,
                true);
            laser.SyncModuleLayout(forceCollider: true, forceWarning: true);
        }

        GameObject saved = PrefabUtility.SaveAsPrefabAsset(moduleRoot, ModulePrefabPath);
        Object.DestroyImmediate(moduleRoot);
        return saved;
    }

    static Material CreateLitMaterial(string path, Color color, float metallic, float smoothness)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            EnsureFolder(System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/'));
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

    static Material CreateEmissiveMaterial(string path, Color baseColor, Color emissionColor)
    {
        Material mat = CreateLitMaterial(path, baseColor, 0.1f, 0.2f);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", emissionColor);
        mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static Material CreateTransparentMaterial(string path, Color baseColor, Color emissionColor)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            EnsureFolder(System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/'));
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }

        mat.shader = shader;
        mat.SetFloat("_Surface", 1f);
        mat.SetFloat("_Blend", 0f);
        mat.SetOverrideTag("RenderType", "Transparent");
        mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.renderQueue = (int)RenderQueue.Transparent;
        mat.SetColor("_BaseColor", baseColor);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", emissionColor);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static void ConfigureFbxImporter(string fbxPath, params Material[] materials)
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

        string[] names =
        {
            "ElectricField_BaseMaterial",
            "ElectricField_PostMaterial",
            "ElectricField_LaserMaterial",
            "ElectricField_ArcMaterial",
            "ElectricField_FieldPlaneMaterial",
            "ElectricField_WarningMaterial",
        };

        for (int i = 0; i < names.Length && i < materials.Length; i++)
        {
            importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), names[i]), materials[i]);
        }

        importer.SaveAndReimport();
    }

    static void RegisterPrefab(GameObject prefab, SceneModuleKind kind)
    {
        SceneModulePrefabRegistry registry = AssetDatabase.LoadAssetAtPath<SceneModulePrefabRegistry>(RegistryPath);
        if (registry == null)
        {
            Debug.LogWarning("[SceneModules] Registry missing: " + RegistryPath);
            return;
        }

        SerializedObject serialized = new SerializedObject(registry);
        SerializedProperty entries = serialized.FindProperty("entries");
        int index = FindOrAddEntryIndex(entries, kind);
        SerializedProperty entry = entries.GetArrayElementAtIndex(index);
        entry.FindPropertyRelative("Kind").enumValueIndex = (int)kind;
        entry.FindPropertyRelative("Prefab").objectReferenceValue = prefab;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(registry);
    }

    static int FindOrAddEntryIndex(SerializedProperty entries, SceneModuleKind kind)
    {
        for (int i = 0; i < entries.arraySize; i++)
        {
            if ((SceneModuleKind)entries.GetArrayElementAtIndex(i).FindPropertyRelative("Kind").enumValueIndex == kind)
            {
                return i;
            }
        }

        int index = entries.arraySize;
        entries.InsertArrayElementAtIndex(index);
        return index;
    }

    static void StripColliders(GameObject root)
    {
        Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Object.DestroyImmediate(colliders[i]);
        }
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
