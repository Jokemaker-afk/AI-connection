#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>Integrates CraftingTable workstation FBX visual and gameplay into placed prefab + registry.</summary>
public static class WorkstationCraftingTableIntegrationMenu
{
    const string RegistryPath = "Assets/_Project/Resources/Items/ItemPlacedPrefabRegistry.asset";
    const string MatFolder = "Assets/_Project/Art/Imported/Materials/Workstations/CraftingTable";

    static readonly string[] MaterialNames =
    {
        "CraftingTable_WoodMain",
        "CraftingTable_WoodDark",
        "CraftingTable_WoodLight",
        "CraftingTable_MetalAccent",
    };

    [MenuItem("Tools/Items/Integrate Workstation CraftingTable Visual")]
    public static void IntegrateVisualOnly()
    {
        IntegrateVisual();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Items] Integrated CraftingTable workstation visual.");
    }

    [MenuItem("Tools/Items/Setup Workstation CraftingTable (Integrate + Gameplay + Registry)")]
    public static void SetupComplete()
    {
        IntegrateVisual();
        MergeGameplay();
        RegisterPlacedPrefab();
        ItemPlacedPrefabRegistry.SetCachedForTests(null);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Items] CraftingTable workstation setup complete.");
    }

    static void IntegrateVisual()
    {
        EnsureFolder("Assets/_Project/Art/Imported/FBX/Workstations");
        EnsureFolder(MatFolder);
        EnsureFolder(WorkstationAssetPaths.StationsPrefabRoot.TrimEnd('/'));

        Material[] materials = CreateMaterials();
        ConfigureFbxImporter(WorkstationAssetPaths.CraftingTableFbx, materials);
        AssetDatabase.ImportAsset(WorkstationAssetPaths.CraftingTableFbx, ImportAssetOptions.ForceUpdate);

        GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(WorkstationAssetPaths.CraftingTableFbx);
        if (fbxAsset == null)
        {
            Debug.LogError("[Items] Missing CraftingTable FBX: " + WorkstationAssetPaths.CraftingTableFbx);
            return;
        }

        string prefabPath = WorkstationAssetPaths.CraftingTablePrefab;
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        GameObject root = existing != null
            ? PrefabUtility.LoadPrefabContents(prefabPath)
            : new GameObject("PF_Generic_Workstation_CraftingTable_01");

        Transform visual = root.transform.Find(SceneModuleVisualUtility.VisualName);
        if (visual == null)
        {
            var visualGo = new GameObject(SceneModuleVisualUtility.VisualName);
            visual = visualGo.transform;
            visual.SetParent(root.transform, false);
        }

        Transform axis = visual.Find(SceneModuleVisualUtility.AxisCorrectionName);
        if (axis == null)
        {
            var axisGo = new GameObject(SceneModuleVisualUtility.AxisCorrectionName);
            axis = axisGo.transform;
            axis.SetParent(visual, false);
        }

        ClearVisualChildren(visual);
        GameObject fbxInstance = (GameObject)PrefabUtility.InstantiatePrefab(fbxAsset, axis);
        fbxInstance.name = "Generic_Workstation_CraftingTable_01";
        fbxInstance.transform.localPosition = Vector3.zero;
        fbxInstance.transform.localRotation = Quaternion.identity;
        fbxInstance.transform.localScale = Vector3.one;

        SceneModuleVisualUtility.EnsureBlenderAxisCorrection(root);
        StripVisualMeshColliders(root);

        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        if (existing != null)
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
        else
        {
            Object.DestroyImmediate(root);
        }
    }

    static void MergeGameplay()
    {
        string prefabPath = WorkstationAssetPaths.CraftingTablePrefab;
        GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
        if (root == null)
        {
            Debug.LogError("[Items] Missing CraftingTable prefab: " + prefabPath);
            return;
        }

        try
        {
            Transform visual = root.transform.Find(SceneModuleVisualUtility.VisualName);
            if (visual != null)
            {
                Level8ResourceVisualUtility.StripGameplayFromVisualHierarchy(visual.gameObject);
            }

            CraftingStation station = root.GetComponent<CraftingStation>() ?? root.AddComponent<CraftingStation>();
            SerializedObject stationSo = new SerializedObject(station);
            stationSo.FindProperty("workstationKind").enumValueIndex = (int)WorkstationKind.Workbench;
            stationSo.FindProperty("promptText").stringValue = "按 E 打开制造（工作台）";
            stationSo.ApplyModifiedPropertiesWithoutUndo();

            PlacedObjectInteractionSetup.Configure(root, ItemKind.CraftingTable);

            PlacementSurface surface = root.GetComponent<PlacementSurface>() ?? root.AddComponent<PlacementSurface>();
            PlacedBuilding placed = root.GetComponent<PlacedBuilding>() ?? root.AddComponent<PlacedBuilding>();

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Debug.Log("[Items] Merged CraftingTable gameplay prefab: " + prefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    static void RegisterPlacedPrefab()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(WorkstationAssetPaths.CraftingTablePrefab);
        if (prefab == null)
        {
            Debug.LogError("[Items] Cannot register missing CraftingTable prefab.");
            return;
        }

        ItemPlacedPrefabRegistry registry = AssetDatabase.LoadAssetAtPath<ItemPlacedPrefabRegistry>(RegistryPath);
        if (registry == null)
        {
            EnsureFolder("Assets/_Project/Resources/Items");
            registry = ScriptableObject.CreateInstance<ItemPlacedPrefabRegistry>();
            AssetDatabase.CreateAsset(registry, RegistryPath);
        }

        SerializedObject serialized = new SerializedObject(registry);
        SerializedProperty entries = serialized.FindProperty("entries");
        int index = FindOrAddEntryIndex(entries, ItemKind.CraftingTable);
        SerializedProperty entry = entries.GetArrayElementAtIndex(index);
        entry.FindPropertyRelative("Kind").enumValueIndex = (int)ItemKind.CraftingTable;
        entry.FindPropertyRelative("Prefab").objectReferenceValue = prefab;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(registry);
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

    static Material[] CreateMaterials()
    {
        return new[]
        {
            CreateLitMaterial(MatFolder + "/CraftingTable_WoodMain.mat", new Color(0.72f, 0.48f, 0.24f), 0.02f, 0.68f),
            CreateLitMaterial(MatFolder + "/CraftingTable_WoodDark.mat", new Color(0.42f, 0.26f, 0.12f), 0.03f, 0.78f),
            CreateLitMaterial(MatFolder + "/CraftingTable_WoodLight.mat", new Color(0.88f, 0.68f, 0.38f), 0.01f, 0.55f),
            CreateLitMaterial(MatFolder + "/CraftingTable_MetalAccent.mat", new Color(0.38f, 0.40f, 0.44f), 0.55f, 0.58f),
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

    static void ClearVisualChildren(Transform visual)
    {
        Transform axis = visual.Find(SceneModuleVisualUtility.AxisCorrectionName);
        if (axis != null)
        {
            for (int i = axis.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(axis.GetChild(i).gameObject);
            }
        }

        for (int i = visual.childCount - 1; i >= 0; i--)
        {
            Transform child = visual.GetChild(i);
            if (child.name != SceneModuleVisualUtility.AxisCorrectionName)
            {
                Object.DestroyImmediate(child.gameObject);
            }
        }
    }

    static void StripVisualMeshColliders(GameObject root)
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
