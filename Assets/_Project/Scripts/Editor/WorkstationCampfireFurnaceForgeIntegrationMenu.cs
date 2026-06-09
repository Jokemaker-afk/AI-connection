#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>Integrates Campfire, Furnace, and Forge workstation FBX visuals and gameplay prefabs.</summary>
public static class WorkstationCampfireFurnaceForgeIntegrationMenu
{
    const string RegistryPath = "Assets/_Project/Resources/Items/ItemPlacedPrefabRegistry.asset";

    struct WorkstationSpec
    {
        public string PrefabName;
        public string FbxPath;
        public string PrefabPath;
        public string MatFolder;
        public string[] MaterialNames;
        public MaterialDefinition[] Materials;
        public ItemKind Kind;
        public WorkstationKind WorkstationKind;
        public string PromptText;
        public Vector3 VisualLocalScale;
        public Vector3 VisualLocalPosition;
    }

    struct MaterialDefinition
    {
        public string FileName;
        public Color Color;
        public float Metallic;
        public float Smoothness;
        public Color Emission;
        public float EmissionStrength;
    }

    [MenuItem("Tools/Items/Integrate Workstation Campfire Visual")]
    public static void IntegrateCampfireVisualOnly() => IntegrateVisualOnly(CreateCampfireSpec());

    [MenuItem("Tools/Items/Integrate Workstation Furnace Visual")]
    public static void IntegrateFurnaceVisualOnly() => IntegrateVisualOnly(CreateFurnaceSpec());

    [MenuItem("Tools/Items/Replace Furnace Visual With Imported forge.fbx")]
    public static void ReplaceFurnaceWithImportedForgeFbx()
    {
        IntegrateVisualOnly(CreateFurnaceSpec());
        Debug.Log("[Items] Furnace visual replaced with imported forge.fbx at " + WorkstationAssetPaths.FurnaceFbx);
    }

    [MenuItem("Tools/Items/Integrate Workstation Forge Visual")]
    public static void IntegrateForgeVisualOnly() => IntegrateVisualOnly(CreateForgeSpec());

    [MenuItem("Tools/Items/Setup Workstations Campfire Furnace Forge (Integrate + Gameplay + Registry)")]
    public static void SetupAllComplete()
    {
        SetupComplete(CreateCampfireSpec());
        SetupComplete(CreateFurnaceSpec());
        SetupComplete(CreateForgeSpec());
        ItemPlacedPrefabRegistry.SetCachedForTests(null);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Items] Campfire, Furnace, and Forge workstation setup complete.");
    }

    static void IntegrateVisualOnly(WorkstationSpec spec)
    {
        IntegrateVisual(spec);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Items] Integrated " + spec.PrefabName + " visual.");
    }

    static void SetupComplete(WorkstationSpec spec)
    {
        IntegrateVisual(spec);
        MergeGameplay(spec);
        RegisterPlacedPrefab(spec);
    }

    static WorkstationSpec CreateCampfireSpec()
    {
        return new WorkstationSpec
        {
            PrefabName = "PF_Generic_Workstation_Campfire_01",
            FbxPath = WorkstationAssetPaths.CampfireFbx,
            PrefabPath = WorkstationAssetPaths.CampfirePrefab,
            MatFolder = "Assets/_Project/Art/Imported/Materials/Workstations/Campfire",
            MaterialNames = new[]
            {
                "Campfire_StoneMaterial",
                "Campfire_LogMaterial",
                "Campfire_EmberMaterial",
                "Campfire_FlameMaterial",
            },
            Materials = new[]
            {
                Mat("Campfire_StoneMaterial.mat", new Color(0.48f, 0.46f, 0.44f), 0.05f, 0.82f),
                Mat("Campfire_LogMaterial.mat", new Color(0.28f, 0.16f, 0.08f), 0.02f, 0.85f),
                MatEmissive("Campfire_EmberMaterial.mat", new Color(0.35f, 0.08f, 0.02f), new Color(1f, 0.35f, 0.05f), 1.2f),
                MatEmissive("Campfire_FlameMaterial.mat", new Color(1f, 0.55f, 0.12f), new Color(1f, 0.45f, 0.08f), 2.5f),
            },
            Kind = ItemKind.Campfire,
            WorkstationKind = WorkstationKind.None,
            PromptText = null,
        };
    }

    static WorkstationSpec CreateFurnaceSpec()
    {
        return new WorkstationSpec
        {
            PrefabName = "PF_Generic_Workstation_Furnace_01",
            FbxPath = WorkstationAssetPaths.FurnaceFbx,
            PrefabPath = WorkstationAssetPaths.FurnacePrefab,
            MatFolder = "Assets/_Project/Art/Imported/Materials/Workstations/Furnace",
            MaterialNames = new[]
            {
                "stone",
                "metal",
            },
            Materials = new[]
            {
                Mat("Furnace_StoneMaterial.mat", new Color(0.44f, 0.40f, 0.36f), 0.04f, 0.78f),
                Mat("Furnace_MetalMaterial.mat", new Color(0.34f, 0.36f, 0.40f), 0.72f, 0.58f),
            },
            Kind = ItemKind.Furnace,
            WorkstationKind = WorkstationKind.Furnace,
            PromptText = "按 E 打开制造（熔炉）",
            VisualLocalScale = new Vector3(0.32f, 0.32f, 0.32f),
            VisualLocalPosition = new Vector3(0f, 0f, 0f),
        };
    }

    static WorkstationSpec CreateForgeSpec()
    {
        return new WorkstationSpec
        {
            PrefabName = "PF_Generic_Workstation_Forge_01",
            FbxPath = WorkstationAssetPaths.ForgeFbx,
            PrefabPath = WorkstationAssetPaths.ForgePrefab,
            MatFolder = "Assets/_Project/Art/Imported/Materials/Workstations/Forge",
            MaterialNames = new[]
            {
                "Forge_DarkMetalMaterial",
                "Forge_StoneBaseMaterial",
                "Forge_AnvilMaterial",
                "Forge_GlowMaterial",
                "Forge_WoodHandleMaterial",
            },
            Materials = new[]
            {
                Mat("Forge_DarkMetalMaterial.mat", new Color(0.32f, 0.34f, 0.38f), 0.65f, 0.55f),
                Mat("Forge_StoneBaseMaterial.mat", new Color(0.38f, 0.36f, 0.34f), 0.04f, 0.80f),
                Mat("Forge_AnvilMaterial.mat", new Color(0.48f, 0.50f, 0.54f), 0.72f, 0.62f),
                MatEmissive("Forge_GlowMaterial.mat", new Color(1f, 0.38f, 0.06f), new Color(1f, 0.30f, 0.04f), 2.2f),
                Mat("Forge_WoodHandleMaterial.mat", new Color(0.30f, 0.18f, 0.10f), 0.02f, 0.78f),
            },
            Kind = ItemKind.Forge,
            WorkstationKind = WorkstationKind.Forge,
            PromptText = "按 E 打开制造（锻造台）",
        };
    }

    static MaterialDefinition Mat(string fileName, Color color, float metallic, float smoothness)
    {
        return new MaterialDefinition
        {
            FileName = fileName,
            Color = color,
            Metallic = metallic,
            Smoothness = smoothness,
            Emission = Color.black,
            EmissionStrength = 0f,
        };
    }

    static MaterialDefinition MatEmissive(string fileName, Color color, Color emission, float emissionStrength)
    {
        return new MaterialDefinition
        {
            FileName = fileName,
            Color = color,
            Metallic = 0f,
            Smoothness = 0.4f,
            Emission = emission,
            EmissionStrength = emissionStrength,
        };
    }

    static void IntegrateVisual(WorkstationSpec spec)
    {
        EnsureFolder("Assets/_Project/Art/Imported/FBX/Workstations");
        EnsureFolder(spec.MatFolder);
        EnsureFolder(WorkstationAssetPaths.StationsPrefabRoot.TrimEnd('/'));

        Material[] materials = CreateMaterials(spec);
        ConfigureFbxImporter(spec.FbxPath, spec.MaterialNames, materials);
        AssetDatabase.ImportAsset(spec.FbxPath, ImportAssetOptions.ForceUpdate);

        GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(spec.FbxPath);
        if (fbxAsset == null)
        {
            Debug.LogError("[Items] Missing FBX: " + spec.FbxPath);
            return;
        }

        string fbxChildName = System.IO.Path.GetFileNameWithoutExtension(spec.FbxPath);
        if (spec.Kind == ItemKind.Furnace)
        {
            fbxChildName = "forge";
        }
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(spec.PrefabPath);
        GameObject root = existing != null
            ? PrefabUtility.LoadPrefabContents(spec.PrefabPath)
            : new GameObject(spec.PrefabName);

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
        fbxInstance.name = fbxChildName;
        fbxInstance.transform.localPosition = spec.VisualLocalPosition.sqrMagnitude > 0.0001f
            ? spec.VisualLocalPosition
            : Vector3.zero;
        fbxInstance.transform.localRotation = Quaternion.identity;
        fbxInstance.transform.localScale = spec.VisualLocalScale.sqrMagnitude > 0.0001f
            ? spec.VisualLocalScale
            : Vector3.one;

        if (spec.Kind == ItemKind.Furnace)
        {
            AssignFurnaceRendererMaterials(root, materials);
            AlignFurnaceVisualToGround(fbxInstance, root);
        }

        SceneModuleVisualUtility.EnsureBlenderAxisCorrection(root);
        StripVisualMeshColliders(root);

        PrefabUtility.SaveAsPrefabAsset(root, spec.PrefabPath);
        if (existing != null)
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
        else
        {
            Object.DestroyImmediate(root);
        }
    }

    static void MergeGameplay(WorkstationSpec spec)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(spec.PrefabPath);
        if (root == null)
        {
            Debug.LogError("[Items] Missing prefab: " + spec.PrefabPath);
            return;
        }

        try
        {
            Transform visual = root.transform.Find(SceneModuleVisualUtility.VisualName);
            if (visual != null)
            {
                Level8ResourceVisualUtility.StripGameplayFromVisualHierarchy(visual.gameObject);
            }

            if (spec.WorkstationKind != WorkstationKind.None)
            {
                CraftingStation station = root.GetComponent<CraftingStation>() ?? root.AddComponent<CraftingStation>();
                SerializedObject stationSo = new SerializedObject(station);
                stationSo.FindProperty("workstationKind").enumValueIndex = (int)spec.WorkstationKind;
                if (!string.IsNullOrEmpty(spec.PromptText))
                {
                    stationSo.FindProperty("promptText").stringValue = spec.PromptText;
                }

                stationSo.ApplyModifiedPropertiesWithoutUndo();
            }

            PlacedObjectInteractionSetup.Configure(root, spec.Kind);

            if (root.GetComponent<PlacementSurface>() == null)
            {
                root.AddComponent<PlacementSurface>();
            }

            if (root.GetComponent<PlacedBuilding>() == null)
            {
                root.AddComponent<PlacedBuilding>();
            }

            PrefabUtility.SaveAsPrefabAsset(root, spec.PrefabPath);
            Debug.Log("[Items] Merged gameplay prefab: " + spec.PrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    static void RegisterPlacedPrefab(WorkstationSpec spec)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(spec.PrefabPath);
        if (prefab == null)
        {
            Debug.LogError("[Items] Cannot register missing prefab: " + spec.PrefabPath);
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
        int index = FindOrAddEntryIndex(entries, spec.Kind);
        SerializedProperty entry = entries.GetArrayElementAtIndex(index);
        entry.FindPropertyRelative("Kind").enumValueIndex = (int)spec.Kind;
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

    static Material[] CreateMaterials(WorkstationSpec spec)
    {
        Material[] result = new Material[spec.Materials.Length];
        for (int i = 0; i < spec.Materials.Length; i++)
        {
            MaterialDefinition def = spec.Materials[i];
            string path = spec.MatFolder + "/" + def.FileName;
            result[i] = CreateLitMaterial(path, def);
        }

        return result;
    }

    static Material CreateLitMaterial(string path, MaterialDefinition def)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }

        mat.shader = shader;
        mat.SetColor("_BaseColor", def.Color);
        mat.SetFloat("_Metallic", def.Metallic);
        mat.SetFloat("_Smoothness", def.Smoothness);
        if (def.EmissionStrength > 0.01f)
        {
            mat.SetColor("_EmissionColor", def.Emission * def.EmissionStrength);
            mat.EnableKeyword("_EMISSION");
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }

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

        var existingRemaps = importer.GetExternalObjectMap();
        foreach (var pair in existingRemaps)
        {
            if (pair.Key.type == typeof(Material))
            {
                importer.RemoveRemap(pair.Key);
            }
        }

        for (int i = 0; i < materialNames.Length && i < materials.Length; i++)
        {
            importer.AddRemap(
                new AssetImporter.SourceAssetIdentifier(typeof(Material), materialNames[i]),
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

    static void AssignFurnaceRendererMaterials(GameObject prefabRoot, Material[] materials)
    {
        if (materials == null || materials.Length < 2)
        {
            return;
        }

        Material stoneMaterial = materials[0];
        Material metalMaterial = materials[1];
        Transform visual = prefabRoot.transform.Find(SceneModuleVisualUtility.VisualName);
        if (visual == null)
        {
            return;
        }

        MeshRenderer[] renderers = visual.GetComponentsInChildren<MeshRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            string name = renderers[i].gameObject.name.ToLowerInvariant();
            if (name.Contains("cauldron") || name.Contains("cylinder"))
            {
                renderers[i].sharedMaterial = metalMaterial;
            }
            else if (name != "outlinemesh")
            {
                renderers[i].sharedMaterial = stoneMaterial;
            }
        }
    }

    static void AlignFurnaceVisualToGround(GameObject fbxInstance, GameObject prefabRoot)
    {
        Renderer[] renderers = fbxInstance.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            return;
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        float offsetY = prefabRoot.transform.position.y - bounds.min.y;
        fbxInstance.transform.position += new Vector3(0f, offsetY, 0f);
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
