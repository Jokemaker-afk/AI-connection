#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>Integrates Blender FBX resource visuals into Level8 resource prefabs and registry.</summary>
public static class Level8ResourceVisualIntegrationMenu
{
    static readonly string PrefabFolder = ItemWorldPickupAssetPaths.ResourcesPrefabRoot.TrimEnd('/');
    const string RegistryPath = "Assets/_Project/Resources/Level8/Level8ResourcePrefabRegistry.asset";

    struct ResourceSpec
    {
        public string AssetName;
        public string FbxPath;
        public string MatFolder;
        public string PrefabPath;
        public ItemKind Kind;
        public string[] MaterialNames;
        public Material[] Materials;
    }

    [MenuItem("Tools/Level8/Migrate Resource Prefabs To Shared WorldPickups Folder")]
    public static void MigrateLevel8Menu()
    {
        ResourceWorldPickupPrefabMigrationMenu.MigrateResourcePrefabsToSharedFolder();
    }

    [MenuItem("Tools/Items/Migrate Resource Prefabs To Shared Folder")]
    public static void MigrateItemsMenu()
    {
        ResourceWorldPickupPrefabMigrationMenu.MigrateResourcePrefabsToSharedFolder();
    }

    [MenuItem("Tools/Level8/Integrate Resource Visuals (Wood Stone Fiber)")]
    public static void IntegrateAll()
    {
        IntegrateWoodStoneFiberBatch();
    }

    [MenuItem("Tools/Items/Integrate Resource Visuals (Wood Stone Fiber)")]
    public static void IntegrateAllItemsMenu()
    {
        IntegrateWoodStoneFiberBatch();
    }

    static void IntegrateWoodStoneFiberBatch()
    {
        EnsureFolder("Assets/_Project/Art/Imported/FBX/Resources");
        EnsureFolder(PrefabFolder);
        EnsureFolder("Assets/_Project/Resources/Level8");
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        IntegrateWood();
        IntegrateStone();
        IntegrateFiber();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Level8ResourcePrefabRegistry.SetCachedForTests(null);
        Debug.Log("[Items] Integrated Wood, Stone, and Fiber resource FBX visuals at " + ItemWorldPickupAssetPaths.ResourcesPrefabRoot);
    }

    [MenuItem("Tools/Level8/Integrate Resource Wood Visual")]
    public static void IntegrateWood()
    {
        IntegrateResource(CreateWoodSpec());
    }

    [MenuItem("Tools/Level8/Integrate Resource Stone Visual")]
    public static void IntegrateStone()
    {
        IntegrateResource(CreateStoneSpec());
    }

    [MenuItem("Tools/Level8/Integrate Resource Fiber Visual")]
    public static void IntegrateFiber()
    {
        IntegrateResource(CreateFiberSpec());
    }

    [MenuItem("Tools/Level8/Integrate Resource Visuals (OreFragment Coal Berry)")]
    public static void IntegrateOreCoalBerryBatch()
    {
        EnsureFolder("Assets/_Project/Art/Imported/FBX/Resources");
        EnsureFolder(PrefabFolder);
        EnsureFolder("Assets/_Project/Resources/Level8");
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        IntegrateOreFragment();
        IntegrateCoal();
        IntegrateBerry();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Level8ResourcePrefabRegistry.SetCachedForTests(null);
        Debug.Log("[Level8] Integrated OreFragment, Coal, and Berry resource FBX visuals.");
    }

    [MenuItem("Tools/Level8/Integrate Resource OreFragment Visual")]
    public static void IntegrateOreFragment()
    {
        IntegrateResource(CreateOreFragmentSpec());
    }

    [MenuItem("Tools/Level8/Integrate Resource Coal Visual")]
    public static void IntegrateCoal()
    {
        IntegrateResource(CreateCoalSpec());
    }

    [MenuItem("Tools/Level8/Integrate Resource Berry Visual")]
    public static void IntegrateBerry()
    {
        IntegrateResource(CreateBerrySpec());
    }

    [MenuItem("Tools/Items/Integrate Resource Visuals (Grass Clay)")]
    public static void IntegrateGrassClayBatch()
    {
        EnsureFolder("Assets/_Project/Art/Imported/FBX/Resources");
        EnsureFolder(PrefabFolder);
        EnsureFolder("Assets/_Project/Resources/Level8");
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        IntegrateGrass();
        IntegrateClay();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Level8ResourcePrefabRegistry.SetCachedForTests(null);
        Debug.Log("[Items] Integrated Grass and Clay resource FBX visuals.");
    }

    [MenuItem("Tools/Items/Integrate Resource Grass Visual")]
    public static void IntegrateGrass()
    {
        IntegrateResource(CreateGrassSpec());
    }

    [MenuItem("Tools/Items/Integrate Resource Clay Visual")]
    public static void IntegrateClay()
    {
        IntegrateResource(CreateClaySpec());
    }

    [MenuItem("Tools/Items/Integrate Resource Flint Visual")]
    public static void IntegrateFlint()
    {
        IntegrateResource(CreateFlintSpec());
    }

    [MenuItem("Tools/Items/Integrate Resource Vine Visual")]
    public static void IntegrateVine()
    {
        IntegrateResource(CreateVineSpec());
    }

    static ResourceSpec CreateWoodSpec()
    {
        string matFolder = "Assets/_Project/Art/Imported/Materials/Resources/Wood";
        return new ResourceSpec
        {
            AssetName = "Generic_Resource_Wood_01",
            FbxPath = "Assets/_Project/Art/Imported/FBX/Resources/Generic_Resource_Wood_01.fbx",
            MatFolder = matFolder,
            PrefabPath = ItemWorldPickupAssetPaths.WoodPrefab,
            Kind = ItemKind.Wood,
            MaterialNames = new[] { "WoodBarkMaterial", "WoodCutMaterial", "WoodDarkMaterial" },
            Materials = new[]
            {
                CreateLitMaterial(matFolder + "/WoodBarkMaterial.mat", new Color(0.38f, 0.24f, 0.12f), 0.05f, 0.78f),
                CreateLitMaterial(matFolder + "/WoodCutMaterial.mat", new Color(0.72f, 0.55f, 0.32f), 0.04f, 0.68f),
                CreateLitMaterial(matFolder + "/WoodDarkMaterial.mat", new Color(0.28f, 0.18f, 0.1f), 0.06f, 0.85f),
            },
        };
    }

    static ResourceSpec CreateStoneSpec()
    {
        string matFolder = "Assets/_Project/Art/Imported/Materials/Resources/Stone";
        return new ResourceSpec
        {
            AssetName = "Generic_Resource_Stone_01",
            FbxPath = "Assets/_Project/Art/Imported/FBX/Resources/Generic_Resource_Stone_01.fbx",
            MatFolder = matFolder,
            PrefabPath = ItemWorldPickupAssetPaths.StonePrefab,
            Kind = ItemKind.Stone,
            MaterialNames = new[] { "StoneMainMaterial", "StoneDarkMaterial", "StoneHighlightMaterial" },
            Materials = new[]
            {
                CreateLitMaterial(matFolder + "/StoneMainMaterial.mat", new Color(0.54f, 0.56f, 0.6f), 0.12f, 0.86f),
                CreateLitMaterial(matFolder + "/StoneDarkMaterial.mat", new Color(0.38f, 0.4f, 0.44f), 0.1f, 0.9f),
                CreateLitMaterial(matFolder + "/StoneHighlightMaterial.mat", new Color(0.66f, 0.68f, 0.72f), 0.08f, 0.78f),
            },
        };
    }

    static ResourceSpec CreateFiberSpec()
    {
        string matFolder = "Assets/_Project/Art/Imported/Materials/Resources/Fiber";
        return new ResourceSpec
        {
            AssetName = "Generic_Resource_Fiber_01",
            FbxPath = "Assets/_Project/Art/Imported/FBX/Resources/Generic_Resource_Fiber_01.fbx",
            MatFolder = matFolder,
            PrefabPath = ItemWorldPickupAssetPaths.FiberPrefab,
            Kind = ItemKind.Fiber,
            MaterialNames = new[] { "FiberGreenMaterial", "FiberYellowGreenMaterial", "FiberBandMaterial" },
            Materials = new[]
            {
                CreateLitMaterial(matFolder + "/FiberGreenMaterial.mat", new Color(0.32f, 0.62f, 0.26f), 0.03f, 0.72f),
                CreateLitMaterial(matFolder + "/FiberYellowGreenMaterial.mat", new Color(0.58f, 0.72f, 0.28f), 0.02f, 0.68f),
                CreateLitMaterial(matFolder + "/FiberBandMaterial.mat", new Color(0.42f, 0.32f, 0.18f), 0.05f, 0.8f),
            },
        };
    }

    static ResourceSpec CreateOreFragmentSpec()
    {
        string matFolder = "Assets/_Project/Art/Imported/Materials/Resources/OreFragment";
        return new ResourceSpec
        {
            AssetName = "Generic_Resource_OreFragment_01",
            FbxPath = "Assets/_Project/Art/Imported/FBX/Resources/Generic_Resource_OreFragment_01.fbx",
            MatFolder = matFolder,
            PrefabPath = ItemWorldPickupAssetPaths.OreFragmentPrefab,
            Kind = ItemKind.OreFragment,
            MaterialNames = new[] { "OreRockMaterial", "OreAccentMaterial", "OreDarkMaterial" },
            Materials = new[]
            {
                CreateLitMaterial(matFolder + "/OreRockMaterial.mat", new Color(0.28f, 0.32f, 0.38f), 0.25f, 0.82f),
                CreateEmissiveMaterial(matFolder + "/OreAccentMaterial.mat", new Color(0.35f, 0.55f, 0.95f), new Color(0.2f, 0.5f, 1f) * 2f),
                CreateLitMaterial(matFolder + "/OreDarkMaterial.mat", new Color(0.12f, 0.13f, 0.16f), 0.2f, 0.9f),
            },
        };
    }

    static ResourceSpec CreateCoalSpec()
    {
        string matFolder = "Assets/_Project/Art/Imported/Materials/Resources/Coal";
        return new ResourceSpec
        {
            AssetName = "Generic_Resource_Coal_01",
            FbxPath = "Assets/_Project/Art/Imported/FBX/Resources/Generic_Resource_Coal_01.fbx",
            MatFolder = matFolder,
            PrefabPath = ItemWorldPickupAssetPaths.CoalPrefab,
            Kind = ItemKind.Coal,
            MaterialNames = new[] { "CoalDarkMaterial", "CoalFacetMaterial", "CoalHighlightMaterial" },
            Materials = new[]
            {
                CreateLitMaterial(matFolder + "/CoalDarkMaterial.mat", new Color(0.05f, 0.05f, 0.06f), 0.1f, 0.95f),
                CreateLitMaterial(matFolder + "/CoalFacetMaterial.mat", new Color(0.14f, 0.14f, 0.16f), 0.15f, 0.88f),
                CreateLitMaterial(matFolder + "/CoalHighlightMaterial.mat", new Color(0.22f, 0.24f, 0.28f), 0.2f, 0.75f),
            },
        };
    }

    static ResourceSpec CreateBerrySpec()
    {
        string matFolder = "Assets/_Project/Art/Imported/Materials/Resources/Berry";
        return new ResourceSpec
        {
            AssetName = "Generic_Resource_Berry_01",
            FbxPath = "Assets/_Project/Art/Imported/FBX/Resources/Generic_Resource_Berry_01.fbx",
            MatFolder = matFolder,
            PrefabPath = ItemWorldPickupAssetPaths.BerryPrefab,
            Kind = ItemKind.Berry,
            MaterialNames = new[] { "BerryRedMaterial", "BerryDarkMaterial", "LeafGreenMaterial", "StemMaterial" },
            Materials = new[]
            {
                CreateLitMaterial(matFolder + "/BerryRedMaterial.mat", new Color(0.82f, 0.15f, 0.28f), 0.05f, 0.55f),
                CreateLitMaterial(matFolder + "/BerryDarkMaterial.mat", new Color(0.55f, 0.08f, 0.18f), 0.05f, 0.65f),
                CreateLitMaterial(matFolder + "/LeafGreenMaterial.mat", new Color(0.22f, 0.52f, 0.18f), 0.02f, 0.7f),
                CreateLitMaterial(matFolder + "/StemMaterial.mat", new Color(0.35f, 0.22f, 0.12f), 0.05f, 0.8f),
            },
        };
    }

    static ResourceSpec CreateGrassSpec()
    {
        string matFolder = "Assets/_Project/Art/Imported/Materials/Resources/Grass";
        return new ResourceSpec
        {
            AssetName = "Generic_Resource_Grass_01",
            FbxPath = "Assets/_Project/Art/Imported/FBX/Resources/Generic_Resource_Grass_01.fbx",
            MatFolder = matFolder,
            PrefabPath = ItemWorldPickupAssetPaths.GrassPrefab,
            Kind = ItemKind.Grass,
            MaterialNames = new[] { "GrassGreenMaterial", "GrassLightMaterial", "GrassDarkMaterial" },
            Materials = new[]
            {
                CreateLitMaterial(matFolder + "/GrassGreenMaterial.mat", new Color(0.32f, 0.62f, 0.26f), 0.02f, 0.68f),
                CreateLitMaterial(matFolder + "/GrassLightMaterial.mat", new Color(0.58f, 0.72f, 0.28f), 0.02f, 0.62f),
                CreateLitMaterial(matFolder + "/GrassDarkMaterial.mat", new Color(0.18f, 0.42f, 0.14f), 0.03f, 0.75f),
            },
        };
    }

    static ResourceSpec CreateClaySpec()
    {
        string matFolder = "Assets/_Project/Art/Imported/Materials/Resources/Clay";
        return new ResourceSpec
        {
            AssetName = "Generic_Resource_Clay_01",
            FbxPath = "Assets/_Project/Art/Imported/FBX/Resources/Generic_Resource_Clay_01.fbx",
            MatFolder = matFolder,
            PrefabPath = ItemWorldPickupAssetPaths.ClayPrefab,
            Kind = ItemKind.Clay,
            MaterialNames = new[] { "ClayMainMaterial", "ClayDarkMaterial", "ClayHighlightMaterial" },
            Materials = new[]
            {
                CreateLitMaterial(matFolder + "/ClayMainMaterial.mat", new Color(0.72f, 0.48f, 0.35f), 0.03f, 0.55f),
                CreateLitMaterial(matFolder + "/ClayDarkMaterial.mat", new Color(0.52f, 0.32f, 0.22f), 0.04f, 0.65f),
                CreateLitMaterial(matFolder + "/ClayHighlightMaterial.mat", new Color(0.82f, 0.58f, 0.42f), 0.02f, 0.48f),
            },
        };
    }

    static ResourceSpec CreateFlintSpec()
    {
        string matFolder = "Assets/_Project/Art/Imported/Materials/Resources/Flint";
        return new ResourceSpec
        {
            AssetName = "Generic_Resource_Flint_01",
            FbxPath = "Assets/_Project/Art/Imported/FBX/Resources/Generic_Resource_Flint_01.fbx",
            MatFolder = matFolder,
            PrefabPath = ItemWorldPickupAssetPaths.FlintPrefab,
            Kind = ItemKind.Flint,
            MaterialNames = new[] { "FlintDarkMaterial", "FlintMainMaterial", "FlintEdgeMaterial" },
            Materials = new[]
            {
                CreateLitMaterial(matFolder + "/FlintDarkMaterial.mat", new Color(0.12f, 0.13f, 0.16f), 0.08f, 0.88f),
                CreateLitMaterial(matFolder + "/FlintMainMaterial.mat", new Color(0.28f, 0.32f, 0.38f), 0.12f, 0.82f),
                CreateLitMaterial(matFolder + "/FlintEdgeMaterial.mat", new Color(0.52f, 0.56f, 0.62f), 0.15f, 0.72f),
            },
        };
    }

    static ResourceSpec CreateVineSpec()
    {
        string matFolder = "Assets/_Project/Art/Imported/Materials/Resources/Vine";
        return new ResourceSpec
        {
            AssetName = "Generic_Resource_Vine_01",
            FbxPath = "Assets/_Project/Art/Imported/FBX/Resources/Generic_Resource_Vine_01.fbx",
            MatFolder = matFolder,
            PrefabPath = ItemWorldPickupAssetPaths.VinePrefab,
            Kind = ItemKind.Vine,
            MaterialNames = new[] { "VineMainMaterial", "VineLightMaterial", "VineStemMaterial", "VineShadowMaterial" },
            Materials = new[]
            {
                CreateLitMaterial(matFolder + "/VineMainMaterial.mat", new Color(0.22f, 0.42f, 0.18f), 0.02f, 0.74f),
                CreateLitMaterial(matFolder + "/VineLightMaterial.mat", new Color(0.38f, 0.58f, 0.22f), 0.01f, 0.62f),
                CreateLitMaterial(matFolder + "/VineStemMaterial.mat", new Color(0.28f, 0.32f, 0.14f), 0.03f, 0.80f),
                CreateLitMaterial(matFolder + "/VineShadowMaterial.mat", new Color(0.12f, 0.26f, 0.10f), 0.04f, 0.85f),
            },
        };
    }

    static void IntegrateResource(ResourceSpec spec)
    {
        EnsureFolder(spec.MatFolder);
        ConfigureFbxImporter(spec.FbxPath, spec.MaterialNames, spec.Materials);
        AssetDatabase.ImportAsset(spec.FbxPath, ImportAssetOptions.ForceUpdate);

        GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(spec.FbxPath);
        if (fbxAsset == null)
        {
            Debug.LogError("[Level8] Missing resource FBX: " + spec.FbxPath);
            return;
        }

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

        if (saved != null && !spec.PrefabPath.StartsWith(ItemWorldPickupAssetPaths.ResourcesPrefabRoot))
        {
            Debug.LogError("[Level8] Resource prefab must save under shared folder: " + spec.PrefabPath);
        }

        RegisterPrefab(spec.Kind, saved);
        Debug.Log("[Level8] Integrated resource visual: " + spec.PrefabPath);
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
        Material mat = CreateLitMaterial(path, baseColor, 0.35f, 0.45f);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", emissionColor);
        mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
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

    static void RegisterPrefab(ItemKind kind, GameObject prefab)
    {
        Level8ResourcePrefabRegistry registry = AssetDatabase.LoadAssetAtPath<Level8ResourcePrefabRegistry>(RegistryPath);
        if (registry == null)
        {
            registry = ScriptableObject.CreateInstance<Level8ResourcePrefabRegistry>();
            AssetDatabase.CreateAsset(registry, RegistryPath);
        }

        SerializedObject serialized = new SerializedObject(registry);
        SerializedProperty entries = serialized.FindProperty("genericEntries");
        int index = FindOrAddEntryIndex(entries, kind);
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
