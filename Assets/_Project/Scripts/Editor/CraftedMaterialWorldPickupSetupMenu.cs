#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>Integrates Blender FBX visuals and gameplay for crafted-material / consumable world pickups.</summary>
public static class CraftedMaterialWorldPickupSetupMenu
{
    const string RegistryPath = "Assets/_Project/Resources/Level8/Level8ResourcePrefabRegistry.asset";

    struct ItemPickupSpec
    {
        public string AssetName;
        public string FbxPath;
        public string MatFolder;
        public string PrefabPath;
        public ItemKind Kind;
        public string[] MaterialNames;
        public Material[] Materials;
    }

    static readonly (ItemKind kind, string prefabPath)[] SupportedPickups =
    {
        (ItemKind.Stick, ItemWorldPickupAssetPaths.StickPrefab),
        (ItemKind.Plank, ItemWorldPickupAssetPaths.PlankPrefab),
        (ItemKind.Rope, ItemWorldPickupAssetPaths.RopePrefab),
        (ItemKind.Cloth, ItemWorldPickupAssetPaths.ClothPrefab),
        (ItemKind.Bandage, ItemWorldPickupAssetPaths.BandagePrefab),
        (ItemKind.MetalIngot, ItemWorldPickupAssetPaths.MetalIngotPrefab),
        (ItemKind.Brick, ItemWorldPickupAssetPaths.BrickPrefab),
    };

    [MenuItem("Tools/Items/Integrate Crafted Material Visuals (Stick Plank Rope Cloth)")]
    public static void IntegrateBatchOne()
    {
        EnsureFolders();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        IntegrateStick();
        IntegratePlank();
        IntegrateRope();
        IntegrateCloth();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Items] Integrated Stick, Plank, Rope, and Cloth crafted-material visuals.");
    }

    [MenuItem("Tools/Items/Integrate Crafted Material Visuals (Bandage MetalIngot)")]
    public static void IntegrateBatchTwo()
    {
        EnsureFolders();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        IntegrateBandage();
        IntegrateMetalIngot();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Items] Integrated Bandage and MetalIngot visuals.");
    }

    [MenuItem("Tools/Items/Integrate Crafted Material Visuals (All Six)")]
    public static void IntegrateAllSix()
    {
        EnsureFolders();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        for (int i = 0; i < SupportedPickups.Length; i++)
        {
            IntegrateByKind(SupportedPickups[i].kind);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Items] Integrated all six crafted-material / consumable world-pickup visuals.");
    }

    [MenuItem("Tools/Items/Merge Crafted Material Gameplay (All Six)")]
    public static void MergeAllSix()
    {
        for (int i = 0; i < SupportedPickups.Length; i++)
        {
            MergePickup(SupportedPickups[i].kind, SupportedPickups[i].prefabPath);
        }

        RepointRegistryEntries();
        Level8ResourcePrefabRegistry.SetCachedForTests(null);
        Debug.Log("[Items] Merged gameplay into all six crafted-material / consumable world-pickup prefabs.");
    }

    [MenuItem("Tools/Items/Setup Crafted Material World Pickups (Integrate + Merge All Six)")]
    public static void SetupAllSix()
    {
        IntegrateAllSix();
        MergeAllSix();
    }

    [MenuItem("Tools/Items/Integrate Crafted Material Stick Visual")]
    public static void IntegrateStick() => IntegrateByKind(ItemKind.Stick);

    [MenuItem("Tools/Items/Integrate Crafted Material Plank Visual")]
    public static void IntegratePlank() => IntegrateByKind(ItemKind.Plank);

    [MenuItem("Tools/Items/Integrate Crafted Material Rope Visual")]
    public static void IntegrateRope() => IntegrateByKind(ItemKind.Rope);

    [MenuItem("Tools/Items/Integrate Crafted Material Cloth Visual")]
    public static void IntegrateCloth() => IntegrateByKind(ItemKind.Cloth);

    [MenuItem("Tools/Items/Integrate Consumable Bandage Visual")]
    public static void IntegrateBandage() => IntegrateByKind(ItemKind.Bandage);

    [MenuItem("Tools/Items/Integrate Crafted Material MetalIngot Visual")]
    public static void IntegrateMetalIngot() => IntegrateByKind(ItemKind.MetalIngot);

    [MenuItem("Tools/Items/Integrate Crafted Material Brick Visual")]
    public static void IntegrateBrick() => IntegrateByKind(ItemKind.Brick);

    static void IntegrateByKind(ItemKind kind)
    {
        ItemPickupSpec spec = CreateSpec(kind);
        if (string.IsNullOrEmpty(spec.FbxPath))
        {
            Debug.LogError("[Items] No integration spec for kind: " + kind);
            return;
        }

        IntegrateItem(spec);
    }

    static ItemPickupSpec CreateSpec(ItemKind kind)
    {
        switch (kind)
        {
            case ItemKind.Stick:
            {
                string matFolder = "Assets/_Project/Art/Imported/Materials/Items/Stick";
                return new ItemPickupSpec
                {
                    AssetName = "Generic_Item_Stick_01",
                    FbxPath = ItemWorldPickupAssetPaths.CraftedMaterialsFbxRoot + "Generic_Item_Stick_01.fbx",
                    MatFolder = matFolder,
                    PrefabPath = ItemWorldPickupAssetPaths.StickPrefab,
                    Kind = ItemKind.Stick,
                    MaterialNames = new[] { "StickWoodMaterial", "StickCutMaterial", "StickDarkMaterial" },
                    Materials = new[]
                    {
                        CreateLitMaterial(matFolder + "/StickWoodMaterial.mat", new Color(0.58f, 0.4f, 0.22f), 0.04f, 0.62f),
                        CreateLitMaterial(matFolder + "/StickCutMaterial.mat", new Color(0.72f, 0.55f, 0.32f), 0.03f, 0.55f),
                        CreateLitMaterial(matFolder + "/StickDarkMaterial.mat", new Color(0.34f, 0.22f, 0.12f), 0.05f, 0.78f),
                    },
                };
            }

            case ItemKind.Plank:
            {
                string matFolder = "Assets/_Project/Art/Imported/Materials/Items/Plank";
                return new ItemPickupSpec
                {
                    AssetName = "Generic_Item_Plank_01",
                    FbxPath = ItemWorldPickupAssetPaths.CraftedMaterialsFbxRoot + "Generic_Item_Plank_01.fbx",
                    MatFolder = matFolder,
                    PrefabPath = ItemWorldPickupAssetPaths.PlankPrefab,
                    Kind = ItemKind.Plank,
                    MaterialNames = new[] { "PlankWoodMaterial", "PlankCutMaterial", "PlankEdgeMaterial" },
                    Materials = new[]
                    {
                        CreateLitMaterial(matFolder + "/PlankWoodMaterial.mat", new Color(0.68f, 0.5f, 0.28f), 0.04f, 0.58f),
                        CreateLitMaterial(matFolder + "/PlankCutMaterial.mat", new Color(0.82f, 0.68f, 0.42f), 0.03f, 0.48f),
                        CreateLitMaterial(matFolder + "/PlankEdgeMaterial.mat", new Color(0.48f, 0.32f, 0.16f), 0.05f, 0.72f),
                    },
                };
            }

            case ItemKind.Rope:
            {
                string matFolder = "Assets/_Project/Art/Imported/Materials/Items/Rope";
                return new ItemPickupSpec
                {
                    AssetName = "Generic_Item_Rope_01",
                    FbxPath = ItemWorldPickupAssetPaths.CraftedMaterialsFbxRoot + "Generic_Item_Rope_01.fbx",
                    MatFolder = matFolder,
                    PrefabPath = ItemWorldPickupAssetPaths.RopePrefab,
                    Kind = ItemKind.Rope,
                    MaterialNames = new[] { "RopeTanMaterial", "RopeDarkMaterial", "RopeTwistMaterial" },
                    Materials = new[]
                    {
                        CreateLitMaterial(matFolder + "/RopeTanMaterial.mat", new Color(0.72f, 0.58f, 0.32f), 0.02f, 0.45f),
                        CreateLitMaterial(matFolder + "/RopeDarkMaterial.mat", new Color(0.48f, 0.36f, 0.18f), 0.03f, 0.55f),
                        CreateLitMaterial(matFolder + "/RopeTwistMaterial.mat", new Color(0.62f, 0.5f, 0.26f), 0.02f, 0.5f),
                    },
                };
            }

            case ItemKind.Cloth:
            {
                string matFolder = "Assets/_Project/Art/Imported/Materials/Items/Cloth";
                return new ItemPickupSpec
                {
                    AssetName = "Generic_Item_Cloth_01",
                    FbxPath = ItemWorldPickupAssetPaths.CraftedMaterialsFbxRoot + "Generic_Item_Cloth_01.fbx",
                    MatFolder = matFolder,
                    PrefabPath = ItemWorldPickupAssetPaths.ClothPrefab,
                    Kind = ItemKind.Cloth,
                    MaterialNames = new[] { "ClothMainMaterial", "ClothFoldMaterial", "ClothEdgeMaterial" },
                    Materials = new[]
                    {
                        CreateLitMaterial(matFolder + "/ClothMainMaterial.mat", new Color(0.88f, 0.84f, 0.72f), 0f, 0.28f),
                        CreateLitMaterial(matFolder + "/ClothFoldMaterial.mat", new Color(0.78f, 0.74f, 0.62f), 0f, 0.32f),
                        CreateLitMaterial(matFolder + "/ClothEdgeMaterial.mat", new Color(0.68f, 0.64f, 0.54f), 0f, 0.38f),
                    },
                };
            }

            case ItemKind.Bandage:
            {
                string matFolder = "Assets/_Project/Art/Imported/Materials/Items/Bandage";
                return new ItemPickupSpec
                {
                    AssetName = "Generic_Item_Bandage_01",
                    FbxPath = ItemWorldPickupAssetPaths.ConsumablesFbxRoot + "Generic_Item_Bandage_01.fbx",
                    MatFolder = matFolder,
                    PrefabPath = ItemWorldPickupAssetPaths.BandagePrefab,
                    Kind = ItemKind.Bandage,
                    MaterialNames = new[] { "BandageWhiteMaterial", "BandageStripeMaterial", "BandageCoreMaterial" },
                    Materials = new[]
                    {
                        CreateLitMaterial(matFolder + "/BandageWhiteMaterial.mat", new Color(0.95f, 0.93f, 0.88f), 0f, 0.22f),
                        CreateLitMaterial(matFolder + "/BandageStripeMaterial.mat", new Color(0.82f, 0.18f, 0.22f), 0f, 0.35f),
                        CreateLitMaterial(matFolder + "/BandageCoreMaterial.mat", new Color(0.88f, 0.86f, 0.8f), 0f, 0.28f),
                    },
                };
            }

            case ItemKind.MetalIngot:
            {
                string matFolder = "Assets/_Project/Art/Imported/Materials/Items/MetalIngot";
                return new ItemPickupSpec
                {
                    AssetName = "Generic_Item_MetalIngot_01",
                    FbxPath = ItemWorldPickupAssetPaths.CraftedMaterialsFbxRoot + "Generic_Item_MetalIngot_01.fbx",
                    MatFolder = matFolder,
                    PrefabPath = ItemWorldPickupAssetPaths.MetalIngotPrefab,
                    Kind = ItemKind.MetalIngot,
                    MaterialNames = new[] { "IngotMetalMaterial", "IngotShineMaterial", "IngotDarkMaterial" },
                    Materials = new[]
                    {
                        CreateLitMaterial(matFolder + "/IngotMetalMaterial.mat", new Color(0.68f, 0.7f, 0.76f), 0.72f, 0.58f),
                        CreateLitMaterial(matFolder + "/IngotShineMaterial.mat", new Color(0.82f, 0.84f, 0.9f), 0.85f, 0.72f),
                        CreateLitMaterial(matFolder + "/IngotDarkMaterial.mat", new Color(0.42f, 0.44f, 0.5f), 0.78f, 0.65f),
                    },
                };
            }

            case ItemKind.Brick:
            {
                string matFolder = "Assets/_Project/Art/Imported/Materials/Items/Brick";
                return new ItemPickupSpec
                {
                    AssetName = "Generic_Item_Brick_01",
                    FbxPath = ItemWorldPickupAssetPaths.CraftedMaterialsFbxRoot + "Generic_Item_Brick_01.fbx",
                    MatFolder = matFolder,
                    PrefabPath = ItemWorldPickupAssetPaths.BrickPrefab,
                    Kind = ItemKind.Brick,
                    MaterialNames = new[] { "BrickMainMaterial", "BrickDarkMaterial", "BrickHighlightMaterial" },
                    Materials = new[]
                    {
                        CreateLitMaterial(matFolder + "/BrickMainMaterial.mat", new Color(0.68f, 0.32f, 0.24f), 0.04f, 0.72f),
                        CreateLitMaterial(matFolder + "/BrickDarkMaterial.mat", new Color(0.48f, 0.22f, 0.16f), 0.05f, 0.82f),
                        CreateLitMaterial(matFolder + "/BrickHighlightMaterial.mat", new Color(0.82f, 0.48f, 0.32f), 0.03f, 0.58f),
                    },
                };
            }

            default:
                return default;
        }
    }

    static void IntegrateItem(ItemPickupSpec spec)
    {
        EnsureFolder(spec.MatFolder);
        if (!System.IO.File.Exists(spec.FbxPath))
        {
            Debug.LogError("[Items] Missing item FBX: " + spec.FbxPath);
            return;
        }

        ConfigureFbxImporter(spec.FbxPath, spec.MaterialNames, spec.Materials);
        AssetDatabase.ImportAsset(spec.FbxPath, ImportAssetOptions.ForceUpdate);

        GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(spec.FbxPath);
        if (fbxAsset == null)
        {
            Debug.LogError("[Items] Failed to load item FBX: " + spec.FbxPath);
            return;
        }

        EnsureFolder(System.IO.Path.GetDirectoryName(spec.PrefabPath)?.Replace('\\', '/'));
        string prefabRootName = "PF_" + spec.AssetName;
        Transform preservedLogicRoot = DetachExistingLogicRoot(spec.PrefabPath);
        GameObject prefabRoot = new GameObject(prefabRootName);
        if (preservedLogicRoot != null)
        {
            preservedLogicRoot.SetParent(prefabRoot.transform, false);
            preservedLogicRoot.SetSiblingIndex(0);
        }

        GameObject visual = new GameObject(SceneModuleVisualUtility.VisualName);
        visual.transform.SetParent(prefabRoot.transform, false);
        if (preservedLogicRoot != null)
        {
            visual.transform.SetSiblingIndex(1);
        }

        GameObject fbxInstance = (GameObject)PrefabUtility.InstantiatePrefab(fbxAsset, visual.transform);
        fbxInstance.name = spec.AssetName;
        fbxInstance.transform.localPosition = Vector3.zero;
        fbxInstance.transform.localRotation = Quaternion.identity;
        fbxInstance.transform.localScale = Vector3.one;

        SceneModuleVisualUtility.EnsureBlenderAxisCorrection(prefabRoot);
        StripColliders(prefabRoot);

        GameObject saved = PrefabUtility.SaveAsPrefabAsset(prefabRoot, spec.PrefabPath);
        Object.DestroyImmediate(prefabRoot);

        RegisterPrefab(spec.Kind, saved);
        Debug.Log("[Items] Integrated item visual: " + spec.PrefabPath);
    }

    public static void MergePickup(ItemKind kind, string prefabPath)
    {
        if (string.IsNullOrEmpty(prefabPath) || !System.IO.File.Exists(prefabPath))
        {
            Debug.LogError("[Items] Missing item prefab for merge: " + prefabPath);
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
            Debug.Log("[Items] Merged item gameplay prefab: " + prefabPath + " (" + kind + ")");
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

    static void RepointRegistryEntries()
    {
        Level8ResourcePrefabRegistry registry = AssetDatabase.LoadAssetAtPath<Level8ResourcePrefabRegistry>(RegistryPath);
        if (registry == null)
        {
            return;
        }

        SerializedObject serialized = new SerializedObject(registry);
        SerializedProperty entries = serialized.FindProperty("genericEntries");
        for (int i = 0; i < SupportedPickups.Length; i++)
        {
            ItemKind kind = SupportedPickups[i].kind;
            GameObject sharedPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SupportedPickups[i].prefabPath);
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

    static void RegisterPrefab(ItemKind kind, GameObject prefab)
    {
        Level8ResourcePrefabRegistry registry = AssetDatabase.LoadAssetAtPath<Level8ResourcePrefabRegistry>(RegistryPath);
        if (registry == null)
        {
            registry = ScriptableObject.CreateInstance<Level8ResourcePrefabRegistry>();
            EnsureFolder("Assets/_Project/Resources/Level8");
            AssetDatabase.CreateAsset(registry, RegistryPath);
        }

        SerializedObject serialized = new SerializedObject(registry);
        SerializedProperty entries = serialized.FindProperty("genericEntries");
        int index = FindOrAddRegistryEntryIndex(entries, kind);
        SerializedProperty entry = entries.GetArrayElementAtIndex(index);
        entry.FindPropertyRelative("Kind").enumValueIndex = (int)kind;
        string sharedPath = ItemWorldPickupAssetPaths.PrefabForKind(kind);
        GameObject sharedPrefab = !string.IsNullOrEmpty(sharedPath)
            ? AssetDatabase.LoadAssetAtPath<GameObject>(sharedPath)
            : null;
        entry.FindPropertyRelative("Prefab").objectReferenceValue = sharedPrefab != null ? sharedPrefab : prefab;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(registry);
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

    static void ConfigureFbxImporter(string fbxPath, string[] materialNames, Material[] materials)
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

        for (int i = 0; i < materialNames.Length && i < materials.Length; i++)
        {
            importer.AddRemap(
                new AssetImporter.SourceAssetIdentifier(typeof(Material), materialNames[i]),
                materials[i]);
        }

        importer.SaveAndReimport();
    }

    static void StripColliders(GameObject root)
    {
        Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Object.DestroyImmediate(colliders[i]);
        }
    }

    static void EnsureFolders()
    {
        EnsureFolder("Assets/_Project/Art/Imported/FBX/Items/CraftedMaterials");
        EnsureFolder("Assets/_Project/Art/Imported/FBX/Items/Consumables");
        EnsureFolder(ItemWorldPickupAssetPaths.CraftedMaterialsPrefabRoot.TrimEnd('/'));
        EnsureFolder(ItemWorldPickupAssetPaths.ConsumablesPrefabRoot.TrimEnd('/'));
        EnsureFolder("Assets/_Project/Resources/Level8");
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
