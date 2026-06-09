#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>Fixes URP material assignment for crafted-material world-pickup FBX visuals without touching gameplay prefab logic.</summary>
public static class ItemWorldPickupMaterialFixMenu
{
    struct ItemMaterialFixSpec
    {
        public string FbxPath;
        public string[] FbxMaterialSlotNames;
        public string[] CentralMaterialPaths;
        public Color[] Colors;
        public float[] Metallic;
        public float[] Smoothness;
    }

    static readonly ItemMaterialFixSpec StickSpec = new ItemMaterialFixSpec
    {
        FbxPath = "Assets/_Project/Art/Imported/FBX/Items/CraftedMaterials/Generic_Item_Stick_01.fbx",
        FbxMaterialSlotNames = new[] { "StickWoodMaterial", "StickCutMaterial", "StickDarkMaterial" },
        CentralMaterialPaths = new[]
        {
            "Assets/_Project/Art/Imported/Materials/Items/Stick/StickMainMaterial.mat",
            "Assets/_Project/Art/Imported/Materials/Items/Stick/StickCutMaterial.mat",
            "Assets/_Project/Art/Imported/Materials/Items/Stick/StickDarkMaterial.mat",
        },
        Colors = new[]
        {
            new Color(0.52f, 0.34f, 0.18f),
            new Color(0.78f, 0.62f, 0.38f),
            new Color(0.32f, 0.2f, 0.1f),
        },
        Metallic = new[] { 0.03f, 0.02f, 0.04f },
        Smoothness = new[] { 0.58f, 0.48f, 0.72f },
    };

    static readonly ItemMaterialFixSpec PlankSpec = new ItemMaterialFixSpec
    {
        FbxPath = "Assets/_Project/Art/Imported/FBX/Items/CraftedMaterials/Generic_Item_Plank_01.fbx",
        FbxMaterialSlotNames = new[] { "PlankWoodMaterial", "PlankCutMaterial", "PlankEdgeMaterial" },
        CentralMaterialPaths = new[]
        {
            "Assets/_Project/Art/Imported/Materials/Items/Plank/PlankMainMaterial.mat",
            "Assets/_Project/Art/Imported/Materials/Items/Plank/PlankEdgeMaterial.mat",
            "Assets/_Project/Art/Imported/Materials/Items/Plank/PlankDarkMaterial.mat",
        },
        Colors = new[]
        {
            new Color(0.82f, 0.66f, 0.4f),
            new Color(0.58f, 0.4f, 0.22f),
            new Color(0.38f, 0.24f, 0.12f),
        },
        Metallic = new[] { 0.03f, 0.04f, 0.05f },
        Smoothness = new[] { 0.42f, 0.52f, 0.68f },
    };

    static readonly ItemMaterialFixSpec RopeSpec = new ItemMaterialFixSpec
    {
        FbxPath = "Assets/_Project/Art/Imported/FBX/Items/CraftedMaterials/Generic_Item_Rope_01.fbx",
        FbxMaterialSlotNames = new[] { "RopeTanMaterial", "RopeDarkMaterial", "RopeTwistMaterial" },
        CentralMaterialPaths = new[]
        {
            "Assets/_Project/Art/Imported/Materials/Items/Rope/RopeMainMaterial.mat",
            "Assets/_Project/Art/Imported/Materials/Items/Rope/RopeShadowMaterial.mat",
            "Assets/_Project/Art/Imported/Materials/Items/Rope/RopeShadowMaterial.mat",
        },
        Colors = new[]
        {
            new Color(0.78f, 0.64f, 0.36f),
            new Color(0.46f, 0.34f, 0.16f),
            new Color(0.46f, 0.34f, 0.16f),
        },
        Metallic = new[] { 0f, 0f, 0f },
        Smoothness = new[] { 0.38f, 0.48f, 0.48f },
    };

    [MenuItem("Tools/Items/Fix Crafted Material Rendering (Stick Plank Rope)")]
    public static void FixStickPlankRope()
    {
        EnsureFolder("Assets/_Project/Art/Imported/Materials/Items/Stick");
        EnsureFolder("Assets/_Project/Art/Imported/Materials/Items/Plank");
        EnsureFolder("Assets/_Project/Art/Imported/Materials/Items/Rope");

        FixItemMaterials(StickSpec, "Stick");
        FixItemMaterials(PlankSpec, "Plank");
        FixItemMaterials(RopeSpec, "Rope");

        DeleteEmbeddedFbxMaterials(StickSpec);
        DeleteEmbeddedFbxMaterials(PlankSpec);
        DeleteEmbeddedFbxMaterials(RopeSpec);

        AssetDatabase.ImportAsset(StickSpec.FbxPath, ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset(PlankSpec.FbxPath, ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset(RopeSpec.FbxPath, ImportAssetOptions.ForceUpdate);

        AssignPickupPrefabVisualMaterials(
            ItemWorldPickupAssetPaths.StickPrefab,
            StickSpec,
            new[]
            {
                ("Stick_01", 0),
                ("Stick_02", 1),
                ("Stick_03", 2),
                ("StickBinding", 2),
            });

        AssignPickupPrefabVisualMaterials(
            ItemWorldPickupAssetPaths.PlankPrefab,
            PlankSpec,
            new[]
            {
                ("Plank_01", 0),
                ("Plank_02", 1),
                ("Plank_03", 2),
            });

        AssignPickupPrefabVisualMaterials(
            ItemWorldPickupAssetPaths.RopePrefab,
            RopeSpec,
            new[]
            {
                ("RopeCoil_01", 0),
                ("RopeCoil_02", 1),
                ("RopeCoil_03", 1),
                ("RopeCoil_04", 0),
                ("RopeCoil_05", 1),
                ("RopeCoil_06", 1),
                ("RopeLoop", 1),
            });

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Items] Fixed URP material rendering for Stick, Plank, and Rope world-pickup FBX visuals.");
    }

    static void DeleteEmbeddedFbxMaterials(ItemMaterialFixSpec spec)
    {
        const string embeddedRoot = "Assets/_Project/Art/Imported/FBX/Items/CraftedMaterials/Materials/";
        for (int i = 0; i < spec.FbxMaterialSlotNames.Length; i++)
        {
            string embeddedPath = embeddedRoot + spec.FbxMaterialSlotNames[i] + ".mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(embeddedPath) != null)
            {
                AssetDatabase.DeleteAsset(embeddedPath);
            }
        }
    }

    static void AssignPickupPrefabVisualMaterials(
        string prefabPath,
        ItemMaterialFixSpec spec,
        (string rendererObjectName, int materialIndex)[] assignments)
    {
        if (string.IsNullOrEmpty(prefabPath) || !System.IO.File.Exists(prefabPath))
        {
            Debug.LogError("[Items] Missing pickup prefab for material assignment: " + prefabPath);
            return;
        }

        Material[] materials = new Material[spec.CentralMaterialPaths.Length];
        for (int i = 0; i < spec.CentralMaterialPaths.Length; i++)
        {
            materials[i] = AssetDatabase.LoadAssetAtPath<Material>(spec.CentralMaterialPaths[i]);
        }

        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
        if (prefabRoot == null)
        {
            return;
        }

        try
        {
            Transform visualRoot = prefabRoot.transform.Find(SceneModuleVisualUtility.VisualName);
            if (visualRoot == null)
            {
                Debug.LogError("[Items] Prefab missing Visual root: " + prefabPath);
                return;
            }

            MeshRenderer[] renderers = visualRoot.GetComponentsInChildren<MeshRenderer>(true);
            for (int i = 0; i < assignments.Length; i++)
            {
                (string objectName, int materialIndex) = assignments[i];
                Material material = materialIndex >= 0 && materialIndex < materials.Length ? materials[materialIndex] : null;
                if (material == null)
                {
                    continue;
                }

                for (int r = 0; r < renderers.Length; r++)
                {
                    if (renderers[r].gameObject.name != objectName)
                    {
                        continue;
                    }

                    renderers[r].sharedMaterial = material;
                    Debug.Log("[Items] Assigned " + material.name + " to " + prefabPath + " :: " + objectName);
                    break;
                }
            }

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }
    }

    static void FixItemMaterials(ItemMaterialFixSpec spec, string label)
    {
        Material[] materials = new Material[spec.CentralMaterialPaths.Length];
        for (int i = 0; i < spec.CentralMaterialPaths.Length; i++)
        {
            materials[i] = CreateOrUpdateLitMaterial(
                spec.CentralMaterialPaths[i],
                spec.Colors[i],
                spec.Metallic[i],
                spec.Smoothness[i]);
        }

        ConfigureFbxImporter(spec.FbxPath, spec.FbxMaterialSlotNames, materials);
        AssetDatabase.ImportAsset(spec.FbxPath, ImportAssetOptions.ForceUpdate);
        LogRendererMaterials(label, spec.FbxPath);
    }

    static Material CreateOrUpdateLitMaterial(string path, Color color, float metallic, float smoothness)
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
        mat.SetColor("_Color", color);
        mat.SetFloat("_Metallic", metallic);
        mat.SetFloat("_Smoothness", smoothness);
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static void ConfigureFbxImporter(string fbxPath, string[] materialSlotNames, Material[] materials)
    {
        var importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
        if (importer == null)
        {
            Debug.LogError("[Items] Missing ModelImporter for: " + fbxPath);
            return;
        }

        importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
        importer.materialLocation = ModelImporterMaterialLocation.External;
        importer.importCameras = false;
        importer.importLights = false;
        importer.bakeAxisConversion = true;

        var remapPairs = importer.GetExternalObjectMap();
        foreach (var pair in remapPairs)
        {
            if (pair.Key.type == typeof(Material))
            {
                importer.RemoveRemap(pair.Key);
            }
        }

        for (int i = 0; i < materialSlotNames.Length && i < materials.Length; i++)
        {
            if (materials[i] == null)
            {
                continue;
            }

            importer.AddRemap(
                new AssetImporter.SourceAssetIdentifier(typeof(Material), materialSlotNames[i]),
                materials[i]);
        }

        importer.SaveAndReimport();
    }

    static void LogRendererMaterials(string label, string fbxPath)
    {
        GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
        if (fbx == null)
        {
            return;
        }

        MeshRenderer[] renderers = fbx.GetComponentsInChildren<MeshRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Material mat = renderers[i].sharedMaterial;
            string matPath = mat != null ? AssetDatabase.GetAssetPath(mat) : "NULL";
            Debug.Log($"[Items] {label} renderer {renderers[i].name} -> {matPath}");
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
