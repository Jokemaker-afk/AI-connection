#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>Creates player shield bubble VFX prefabs, materials, and Resources runtime copies.</summary>
public static class PlayerShieldBubbleVisualSetupMenu
{
    const string MatFolder = "Assets/_Project/Art/Imported/Materials/Player/VFX/ShieldBubble";

    const string FbxPathV1 = "Assets/_Project/Art/Imported/FBX/Player/VFX/Generic_Player_ShieldBubble_01.fbx";
    const string PrefabPathV1 = "Assets/_Project/Prefabs/Shared/VFX/PF_Generic_Player_ShieldBubble_01.prefab";
    const string ResourcesPrefabPathV1 = "Assets/_Project/Resources/Player/VFX/PF_Generic_Player_ShieldBubble_01.prefab";

    const string FbxPathV2 = "Assets/_Project/Art/Imported/FBX/Player/VFX/Generic_Player_ShieldBubble_02.fbx";
    const string PrefabPathV2 = "Assets/_Project/Prefabs/Shared/VFX/PF_Generic_Player_ShieldBubble_02.prefab";
    const string ResourcesPrefabPathV2 = "Assets/_Project/Resources/Player/VFX/PF_Generic_Player_ShieldBubble_02.prefab";

    const string FbxPathV3 = "Assets/_Project/Art/Imported/FBX/Player/VFX/Generic_Player_ShieldBubble_03.fbx";
    const string PrefabPathV3 = "Assets/_Project/Prefabs/Shared/VFX/PF_Generic_Player_ShieldBubble_03.prefab";
    const string ResourcesPrefabPathV3 = "Assets/_Project/Resources/Player/VFX/PF_Generic_Player_ShieldBubble_03.prefab";
    const string FresnelShaderPath = "Assets/_Project/Art/Shaders/Player/ShieldBubbleFresnel.shader";

    [MenuItem("Tools/Player/Integrate Shield Bubble Visual Prefab")]
    public static void IntegrateV1()
    {
        IntegrateInternal(
            FbxPathV1,
            PrefabPathV1,
            ResourcesPrefabPathV1,
            "PF_Generic_Player_ShieldBubble_01",
            "Generic_Player_ShieldBubble_01",
            useV2Materials: false);
    }

    [MenuItem("Tools/Player/Integrate Shield Bubble Visual Prefab V2")]
    public static void IntegrateV2()
    {
        IntegrateInternal(
            FbxPathV2,
            PrefabPathV2,
            ResourcesPrefabPathV2,
            "PF_Generic_Player_ShieldBubble_02",
            "Generic_Player_ShieldBubble_02",
            useV2Materials: true);
    }

    [MenuItem("Tools/Player/Integrate Shield Bubble Visual Prefab V3")]
    public static void IntegrateV3()
    {
        EnsureFolder(MatFolder);
        EnsureFolder("Assets/_Project/Art/Shaders/Player");
        EnsureFolder("Assets/_Project/Art/Imported/FBX/Player/VFX");
        EnsureFolder("Assets/_Project/Prefabs/Shared/VFX");
        EnsureFolder("Assets/_Project/Resources/Player/VFX");
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        Material shellMat = CreateFresnelMaterial(
            MatFolder + "/ShieldBubbleMaterial.mat",
            new Color(0.78f, 0.92f, 1f, 0.08f),
            new Color(0.88f, 0.95f, 1f, 0.42f),
            new Color(0.72f, 0.82f, 1f, 1f),
            fresnelPower: 2.2f,
            edgeBoost: 1.15f,
            emissionStrength: 0.35f);
        Material outerMat = CreateFresnelMaterial(
            MatFolder + "/ShieldBubbleOuterShellMaterial.mat",
            new Color(0.82f, 0.94f, 1f, 0.05f),
            new Color(0.9f, 0.96f, 1f, 0.35f),
            new Color(0.78f, 0.88f, 1f, 1f),
            fresnelPower: 3.0f,
            edgeBoost: 1.35f,
            emissionStrength: 0.45f);
        Material ringMat = CreateTransparentMaterial(
            MatFolder + "/ShieldBubbleRingMaterial.mat",
            new Color(0.9f, 0.96f, 1f, 0.18f),
            new Color(0.82f, 0.92f, 1f),
            0.5f,
            smoothness: 0.1f,
            additive: true);

        ConfigureFbxImporter(
            FbxPathV3,
            new[]
            {
                "ShieldBubbleMaterial",
                "ShieldBubbleOuterShellMaterial",
                "ShieldBubbleRingMaterial",
            },
            new[] { shellMat, outerMat, ringMat });

        AssetDatabase.ImportAsset(FbxPathV3, ImportAssetOptions.ForceUpdate);

        GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPathV3);
        if (fbxAsset == null)
        {
            Debug.LogError("[PlayerShieldBubble] Missing FBX: " + FbxPathV3);
            return;
        }

        GameObject prefabRoot = BuildPrefabRoot(
            fbxAsset,
            "PF_Generic_Player_ShieldBubble_03",
            "Generic_Player_ShieldBubble_03");
        GameObject saved = PrefabUtility.SaveAsPrefabAsset(prefabRoot, PrefabPathV3);
        Object.DestroyImmediate(prefabRoot);

        GameObject resourcesCopy = PrefabUtility.InstantiatePrefab(saved) as GameObject;
        PrefabUtility.SaveAsPrefabAsset(resourcesCopy, ResourcesPrefabPathV3);
        Object.DestroyImmediate(resourcesCopy);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[PlayerShieldBubble] Integrated PF_Generic_Player_ShieldBubble_03 with Fresnel bubble materials.");
    }

    static Material CreateFresnelMaterial(
        string path,
        Color baseColor,
        Color edgeColor,
        Color rimTint,
        float fresnelPower,
        float edgeBoost,
        float emissionStrength)
    {
        Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(FresnelShaderPath);
        if (shader == null)
        {
            shader = Shader.Find("Universal Render Pipeline/ShieldBubbleFresnel");
        }

        if (shader == null)
        {
            Debug.LogError("[PlayerShieldBubble] Missing fresnel shader at " + FresnelShaderPath);
            return CreateTransparentMaterial(path, baseColor, rimTint, emissionStrength, 0.08f, additive: false);
        }

        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            EnsureFolder(System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/'));
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }

        mat.shader = shader;
        mat.SetColor("_BaseColor", baseColor);
        mat.SetColor("_EdgeColor", edgeColor);
        mat.SetColor("_RimTint", rimTint);
        mat.SetFloat("_FresnelPower", fresnelPower);
        mat.SetFloat("_EdgeBoost", edgeBoost);
        mat.SetFloat("_EmissionStrength", emissionStrength);
        mat.renderQueue = (int)RenderQueue.Transparent;
        EditorUtility.SetDirty(mat);
        return mat;
    }

    static void IntegrateInternal(
        string fbxPath,
        string prefabPath,
        string resourcesPrefabPath,
        string prefabName,
        string modelName,
        bool useV2Materials)
    {
        EnsureFolder(MatFolder);
        EnsureFolder("Assets/_Project/Art/Imported/FBX/Player/VFX");
        EnsureFolder("Assets/_Project/Prefabs/Shared/VFX");
        EnsureFolder("Assets/_Project/Resources/Player/VFX");
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        Material shellMat;
        Material highlightMat;
        Material ringMat;

        if (useV2Materials)
        {
            shellMat = CreateTransparentMaterial(
                MatFolder + "/ShieldBubbleMaterial.mat",
                new Color(0.55f, 0.82f, 1f, 0.2f),
                new Color(0.4f, 0.75f, 1f),
                0.8f,
                smoothness: 0.12f,
                additive: false);
            highlightMat = CreateTransparentMaterial(
                MatFolder + "/ShieldBubbleHighlightMaterial.mat",
                new Color(0.85f, 0.95f, 1f, 0.22f),
                new Color(0.7f, 0.9f, 1f),
                1.8f,
                smoothness: 0.08f,
                additive: true);
            ringMat = CreateTransparentMaterial(
                MatFolder + "/ShieldBubbleRingMaterial.mat",
                new Color(0.8f, 0.94f, 1f, 0.28f),
                new Color(0.65f, 0.88f, 1f),
                1.5f,
                smoothness: 0.15f,
                additive: true);
        }
        else
        {
            shellMat = CreateTransparentMaterial(
                MatFolder + "/ShieldBubbleMaterial.mat",
                new Color(0.45f, 0.78f, 1f, 0.32f),
                new Color(0.35f, 0.72f, 1f),
                1.2f,
                smoothness: 0.25f,
                additive: false);
            highlightMat = CreateTransparentMaterial(
                MatFolder + "/ShieldBubbleCoreMaterial.mat",
                new Color(0.65f, 0.88f, 1f, 0.22f),
                new Color(0.5f, 0.82f, 1f),
                2.5f,
                smoothness: 0.2f,
                additive: true);
            ringMat = CreateTransparentMaterial(
                MatFolder + "/ShieldBubbleRingMaterial.mat",
                new Color(0.75f, 0.92f, 1f, 0.45f),
                new Color(0.55f, 0.85f, 1f),
                2f,
                smoothness: 0.2f,
                additive: false);
        }

        ConfigureFbxImporter(
            fbxPath,
            new[] { "ShieldBubbleMaterial", "ShieldBubbleHighlightMaterial", "ShieldBubbleRingMaterial", "ShieldBubbleCoreMaterial" },
            new[] { shellMat, highlightMat, ringMat, highlightMat });

        AssetDatabase.ImportAsset(fbxPath, ImportAssetOptions.ForceUpdate);

        GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
        if (fbxAsset == null)
        {
            Debug.LogError("[PlayerShieldBubble] Missing FBX: " + fbxPath);
            return;
        }

        GameObject prefabRoot = BuildPrefabRoot(fbxAsset, prefabName, modelName);
        GameObject saved = PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
        Object.DestroyImmediate(prefabRoot);

        GameObject resourcesCopy = PrefabUtility.InstantiatePrefab(saved) as GameObject;
        PrefabUtility.SaveAsPrefabAsset(resourcesCopy, resourcesPrefabPath);
        Object.DestroyImmediate(resourcesCopy);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[PlayerShieldBubble] Integrated " + prefabName + " at " + prefabPath);
    }

    static GameObject BuildPrefabRoot(GameObject fbxAsset, string prefabName, string modelName)
    {
        var root = new GameObject(prefabName);
        GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(fbxAsset, root.transform);
        model.name = modelName;
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;
        model.transform.localScale = Vector3.one;

        StripColliders(root);
        return root;
    }

    static void StripColliders(GameObject root)
    {
        Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Object.DestroyImmediate(colliders[i]);
        }
    }

    static Material CreateTransparentMaterial(
        string path,
        Color baseColor,
        Color emission,
        float emissionStrength,
        float smoothness,
        bool additive)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Universal Render Pipeline/Unlit");
        }

        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            string folder = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(folder))
            {
                EnsureFolder(folder);
            }

            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, path);
        }

        mat.shader = shader;
        mat.SetColor("_BaseColor", baseColor);
        mat.color = baseColor;
        mat.SetColor("_EmissionColor", emission * emissionStrength);
        mat.EnableKeyword("_EMISSION");
        mat.SetFloat("_Smoothness", smoothness);
        mat.SetFloat("_Metallic", 0.02f);
        mat.SetFloat("_Surface", 1f);
        mat.SetFloat("_Blend", additive ? 2f : 0f);
        mat.SetFloat("_Cull", (float)CullMode.Off);
        mat.SetFloat("_ZWrite", 0f);
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.SetOverrideTag("RenderType", "Transparent");
        mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", additive ? (int)BlendMode.One : (int)BlendMode.OneMinusSrcAlpha);
        mat.renderQueue = additive ? (int)RenderQueue.Transparent + 10 : (int)RenderQueue.Transparent;
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

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
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
