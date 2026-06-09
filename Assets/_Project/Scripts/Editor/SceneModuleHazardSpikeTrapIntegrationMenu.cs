#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>Integrates Generic_Hazard_SpikeTrap_01 into visual + full hazard module prefabs.</summary>
public static class SceneModuleHazardSpikeTrapIntegrationMenu
{
    const string FbxPath = "Assets/_Project/Art/Imported/FBX/SceneModules/Hazards/Generic_Hazard_SpikeTrap_01.fbx";
    const string MatFolder = "Assets/_Project/Art/Imported/Materials/SceneModules/Hazards/SpikeTrap";
    const string VisualPrefabPath = "Assets/_Project/Prefabs/SceneModules/Hazards/PF_Generic_Hazard_SpikeTrap_01_Visual.prefab";
    const string ModulePrefabPath = "Assets/_Project/Prefabs/SceneModules/Hazards/PF_Generic_Hazard_SpikeTrap_01.prefab";
    const string RegistryPath = "Assets/_Project/Resources/SceneModules/SceneModulePrefabRegistry.asset";

    [MenuItem("Tools/SceneModules/Integrate Hazard SpikeTrap Module")]
    public static void Integrate()
    {
        EnsureFolder(MatFolder);
        EnsureFolder("Assets/_Project/Art/Imported/FBX/SceneModules/Hazards");
        EnsureFolder("Assets/_Project/Prefabs/SceneModules/Hazards");
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        Material baseMat = CreateLitMaterial(MatFolder + "/SpikeTrap_BaseMaterial.mat", new Color(0.14f, 0.14f, 0.16f), 0.45f, 0.62f);
        Material spikeMat = CreateLitMaterial(MatFolder + "/SpikeTrap_SpikeMaterial.mat", new Color(0.32f, 0.34f, 0.38f), 0.55f, 0.48f);
        Material warningMat = CreateLitMaterial(MatFolder + "/SpikeTrap_WarningMaterial.mat", new Color(0.82f, 0.22f, 0.08f), 0.15f, 0.45f);
        Material trimMat = CreateLitMaterial(MatFolder + "/SpikeTrap_DarkTrimMaterial.mat", new Color(0.08f, 0.08f, 0.09f), 0.5f, 0.7f);

        ConfigureFbxImporter(FbxPath, baseMat, spikeMat, warningMat, trimMat);
        AssetDatabase.ImportAsset(FbxPath, ImportAssetOptions.ForceUpdate);

        GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
        if (fbxAsset == null)
        {
            Debug.LogError("[SceneModules] Missing spike trap FBX: " + FbxPath);
            return;
        }

        GameObject visualPrefab = BuildVisualPrefab(fbxAsset);
        visualPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(VisualPrefabPath);

        GameObject modulePrefab = BuildModulePrefab(visualPrefab);

        RegisterPrefab(visualPrefab, SceneModuleKind.SpikeHazard);
        RegisterPrefab(visualPrefab, SceneModuleKind.Hazard);
        RegisterPrefab(modulePrefab, SceneModuleKind.SpikeTrapHazard);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        SceneModulePrefabRegistry.SetCachedForTests(null);
        Debug.Log("[SceneModules] Integrated spike trap module at " + ModulePrefabPath);
    }

    [MenuItem("Tools/SceneModules/Integrate Hazard SpikeTrap Visual")]
    public static void IntegrateVisualOnly()
    {
        Integrate();
    }

    [MenuItem("Tools/SceneModules/Create Spike Hazard Test Prefab And Registry")]
    public static void CreateSpikeHazardTestPrefabAndRegistry()
    {
        Integrate();
    }

    [MenuItem("Tools/SceneModules/Register Spike Hazard Prefab")]
    public static void RegisterSpikeHazardPrefab()
    {
        Integrate();
    }

    [MenuItem("Tools/SceneModules/Clear Spike Hazard Prefab Registry (Test Fallback)")]
    public static void ClearSpikeHazardPrefabRegistry()
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
            if (kind == SceneModuleKind.Hazard
                || kind == SceneModuleKind.SpikeHazard
                || kind == SceneModuleKind.SpikeTrapHazard)
            {
                entries.DeleteArrayElementAtIndex(i);
            }
        }

        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(registry);
        AssetDatabase.SaveAssets();
        SceneModulePrefabRegistry.SetCachedForTests(null);
        Debug.Log("[SceneModules] Cleared SpikeHazard / SpikeTrapHazard registry entries — SpikeTrap will use primitive fallback.");
    }

    static GameObject BuildVisualPrefab(GameObject fbxAsset)
    {
        GameObject prefabRoot = new GameObject("PF_Generic_Hazard_SpikeTrap_01_Visual");
        GameObject visual = new GameObject(SceneModuleVisualUtility.VisualName);
        visual.transform.SetParent(prefabRoot.transform, false);

        GameObject fbxInstance = (GameObject)PrefabUtility.InstantiatePrefab(fbxAsset, visual.transform);
        fbxInstance.name = "Generic_Hazard_SpikeTrap_01";
        fbxInstance.transform.localPosition = Vector3.zero;
        fbxInstance.transform.localRotation = Quaternion.identity;
        fbxInstance.transform.localScale = Vector3.one;

        SceneModuleVisualUtility.EnsureBlenderAxisCorrection(prefabRoot);
        StripColliders(prefabRoot);

        GameObject saved = PrefabUtility.SaveAsPrefabAsset(prefabRoot, VisualPrefabPath);
        Object.DestroyImmediate(prefabRoot);
        return saved;
    }

    static GameObject BuildModulePrefab(GameObject visualPrefab)
    {
        GameObject moduleRoot = new GameObject("PF_Generic_Hazard_SpikeTrap_01");

        GameObject logicRoot = new GameObject(SceneModuleVisualUtility.LogicRootName);
        logicRoot.transform.SetParent(moduleRoot.transform, false);
        BoxCollider box = logicRoot.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = new Vector3(SceneModuleVisualUtility.HazardSpikeVisualDiameter, 0.55f, SceneModuleVisualUtility.HazardSpikeVisualDiameter);
        box.center = new Vector3(0f, 0.25f, 0f);
        logicRoot.AddComponent<SpikeTrap>();

        GameObject visualRoot = new GameObject(SceneModuleVisualUtility.VisualRootName);
        visualRoot.transform.SetParent(moduleRoot.transform, false);
        PrefabUtility.InstantiatePrefab(visualPrefab, visualRoot.transform);

        GameObject warningRoot = new GameObject(HazardModuleLayoutUtility.WarningAreaName);
        warningRoot.transform.SetParent(moduleRoot.transform, false);

        GameObject labelAnchor = new GameObject(HazardModuleLayoutUtility.LabelAnchorName);
        labelAnchor.transform.SetParent(moduleRoot.transform, false);
        labelAnchor.transform.localPosition = new Vector3(0f, 0.8f, 0f);

        GameObject debugBounds = new GameObject(HazardModuleLayoutUtility.DebugBoundsName);
        debugBounds.transform.SetParent(moduleRoot.transform, false);
        debugBounds.SetActive(false);

        SpikeTrap trap = logicRoot.GetComponent<SpikeTrap>();
        if (trap != null)
        {
            SerializedObject serializedTrap = new SerializedObject(trap);
            serializedTrap.FindProperty("moduleVisualApplied").boolValue = true;
            serializedTrap.FindProperty("autoSyncColliderToVisual").boolValue = true;
            serializedTrap.FindProperty("autoSyncWarningArea").boolValue = true;
            serializedTrap.ApplyModifiedPropertiesWithoutUndo();
            trap.Configure(
                new Vector3(SceneModuleVisualUtility.HazardSpikeVisualDiameter, 0.4f, SceneModuleVisualUtility.HazardSpikeVisualDiameter),
                SpikeTrapHazardFactory.DefaultDamage,
                true);
            trap.SyncModuleLayout(forceCollider: true, forceWarning: true);
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
            "SpikeTrap_BaseMaterial",
            "SpikeTrap_SpikeMaterial",
            "SpikeTrap_WarningMaterial",
            "SpikeTrap_DarkTrimMaterial",
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
