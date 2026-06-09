#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>Integrates Speed / Damage / Defense BuffStation FBX visuals into prefabs and registry.</summary>
public static class SceneModuleBuffStationVisualIntegrationMenu
{
    struct StationSpec
    {
        public string FbxName;
        public string PrefabName;
        public SceneModuleKind Kind;
        public string MaterialFolder;
        public MaterialSpec[] Materials;
    }

    struct MaterialSpec
    {
        public string Name;
        public Color Color;
        public float Smoothness;
        public float Metallic;
        public Color Emission;
        public float EmissionStrength;
    }

    static readonly StationSpec[] Stations =
    {
        new StationSpec
        {
            FbxName = "Generic_BuffStation_Speed_01",
            PrefabName = "PF_Generic_BuffStation_Speed_01",
            Kind = SceneModuleKind.SpeedStation,
            MaterialFolder = "Assets/_Project/Art/Imported/Materials/SceneModules/BuffStations/Speed",
            Materials = new[]
            {
                Mat("SpeedBaseMaterial", new Color(0.12f, 0.16f, 0.24f), 0.28f, 0.42f),
                Mat("SpeedPanelMaterial", new Color(0.22f, 0.32f, 0.48f), 0.45f, 0.32f),
                Mat("SpeedGlowMaterial", new Color(0.08f, 0.72f, 0.95f), 0.2f, 0.1f, new Color(0.15f, 0.85f, 1f), 4f),
                Mat("SpeedAccentMaterial", new Color(1f, 0.88f, 0.12f), 0.35f, 0.12f, new Color(1f, 0.82f, 0.1f), 1.8f),
            },
        },
        new StationSpec
        {
            FbxName = "Generic_BuffStation_Damage_01",
            PrefabName = "PF_Generic_BuffStation_Damage_01",
            Kind = SceneModuleKind.DamageStation,
            MaterialFolder = "Assets/_Project/Art/Imported/Materials/SceneModules/BuffStations/Damage",
            Materials = new[]
            {
                Mat("DamageBaseMaterial", new Color(0.1f, 0.1f, 0.11f), 0.28f, 0.5f),
                Mat("DamagePanelMaterial", new Color(0.22f, 0.16f, 0.16f), 0.42f, 0.38f),
                Mat("DamageGlowMaterial", new Color(0.95f, 0.28f, 0.08f), 0.18f, 0.08f, new Color(1f, 0.35f, 0.05f), 4.5f),
                Mat("DamageAccentMaterial", new Color(1f, 0.45f, 0.08f), 0.32f, 0.15f, new Color(1f, 0.5f, 0.1f), 2f),
            },
        },
        new StationSpec
        {
            FbxName = "Generic_BuffStation_Defense_01",
            PrefabName = "PF_Generic_BuffStation_Defense_01",
            Kind = SceneModuleKind.DefenseStation,
            MaterialFolder = "Assets/_Project/Art/Imported/Materials/SceneModules/BuffStations/Defense",
            Materials = new[]
            {
                Mat("DefenseBaseMaterial", new Color(0.11f, 0.14f, 0.2f), 0.3f, 0.45f),
                Mat("DefensePanelMaterial", new Color(0.28f, 0.36f, 0.48f), 0.45f, 0.35f),
                Mat("DefenseGlowMaterial", new Color(0.65f, 0.82f, 1f), 0.2f, 0.08f, new Color(0.75f, 0.9f, 1f), 3.5f),
                Mat("DefenseIconMaterial", new Color(0.92f, 0.95f, 1f), 0.28f, 0.05f),
            },
        },
    };

    const string FbxFolder = "Assets/_Project/Art/Imported/FBX/SceneModules/BuffStations";
    const string PrefabFolder = "Assets/_Project/Prefabs/SceneModules/BuffStations";
    const string RegistryPath = "Assets/_Project/Resources/SceneModules/SceneModulePrefabRegistry.asset";

    [MenuItem("Tools/SceneModules/Integrate Speed Damage Defense BuffStation Visuals")]
    public static void IntegrateAll()
    {
        EnsureFolder("Assets/_Project/Art/Imported/Materials/SceneModules/BuffStations");
        AssetDatabase.Refresh();

        var report = new List<string>();
        SceneModulePrefabRegistry registry = LoadOrCreateRegistry();

        for (int i = 0; i < Stations.Length; i++)
        {
            report.Add(IntegrateStation(Stations[i], registry));
        }

        EditorUtility.SetDirty(registry);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        SceneModulePrefabRegistry.SetCachedForTests(null);

        Debug.Log("[SceneModules] BuffStation visual integration complete:\n" + string.Join("\n", report));
    }

    static string IntegrateStation(StationSpec spec, SceneModulePrefabRegistry registry)
    {
        EnsureFolder(spec.MaterialFolder);

        var materialMap = CreateMaterials(spec);
        string fbxPath = FbxFolder + "/" + spec.FbxName + ".fbx";
        ConfigureFbxImporter(fbxPath, materialMap);
        AssetDatabase.ImportAsset(fbxPath, ImportAssetOptions.ForceUpdate);

        string prefabPath = PrefabFolder + "/" + spec.PrefabName + ".prefab";
        GameObject prefabRoot = BuildOrUpdateVisualPrefab(spec, fbxPath, prefabPath);
        RegisterPrefab(registry, spec.Kind, prefabRoot);

        return spec.PrefabName + " -> " + prefabPath;
    }

    static Dictionary<string, Material> CreateMaterials(StationSpec spec)
    {
        var map = new Dictionary<string, Material>();
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        for (int i = 0; i < spec.Materials.Length; i++)
        {
            MaterialSpec matSpec = spec.Materials[i];
            string path = spec.MaterialFolder + "/" + matSpec.Name + ".mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }

            mat.shader = shader;
            mat.SetColor("_BaseColor", matSpec.Color);
            mat.SetFloat("_Smoothness", matSpec.Smoothness);
            mat.SetFloat("_Metallic", matSpec.Metallic);
            if (matSpec.EmissionStrength > 0f)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", matSpec.Emission * matSpec.EmissionStrength);
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }
            else
            {
                mat.SetColor("_EmissionColor", Color.black);
            }

            EditorUtility.SetDirty(mat);
            map[matSpec.Name] = mat;
        }

        return map;
    }

    static void ConfigureFbxImporter(string fbxPath, Dictionary<string, Material> materialMap)
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

        var external = new List<AssetImporter.SourceAssetIdentifier>();
        var objects = new List<Object>();
        foreach (KeyValuePair<string, Material> pair in materialMap)
        {
            external.Add(new AssetImporter.SourceAssetIdentifier(typeof(Material), pair.Key));
            objects.Add(pair.Value);
        }

        for (int i = 0; i < external.Count; i++)
        {
            importer.AddRemap(external[i], objects[i]);
        }

        importer.SaveAndReimport();
    }

    static GameObject BuildOrUpdateVisualPrefab(StationSpec spec, string fbxPath, string prefabPath)
    {
        EnsureFolder(PrefabFolder);
        GameObject fbxAsset = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
        if (fbxAsset == null)
        {
            throw new System.InvalidOperationException("Missing FBX: " + fbxPath);
        }

        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        GameObject root;
        if (existing != null)
        {
            root = PrefabUtility.LoadPrefabContents(prefabPath);
            Transform visual = root.transform.Find(SceneModuleVisualUtility.VisualName);
            if (visual == null)
            {
                var visualGo = new GameObject(SceneModuleVisualUtility.VisualName);
                visual = visualGo.transform;
                visual.SetParent(root.transform, false);
            }

            ClearVisualModelChildren(visual);
        }
        else
        {
            root = new GameObject(spec.PrefabName);
            var visualGo = new GameObject(SceneModuleVisualUtility.VisualName);
            visualGo.transform.SetParent(root.transform, false);
        }

        Transform visualRoot = root.transform.Find(SceneModuleVisualUtility.VisualName);
        GameObject fbxInstance = (GameObject)PrefabUtility.InstantiatePrefab(fbxAsset, visualRoot);
        fbxInstance.name = spec.FbxName;

        SceneModuleVisualUtility.EnsureBlenderAxisCorrection(root);

        GameObject saved = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        if (existing != null)
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
        else
        {
            Object.DestroyImmediate(root);
        }

        return saved;
    }

    static void ClearVisualModelChildren(Transform visual)
    {
        Transform axis = visual.Find(SceneModuleVisualUtility.AxisCorrectionName);
        if (axis != null)
        {
            for (int i = axis.childCount - 1; i >= 0; i--)
            {
                Object.DestroyImmediate(axis.GetChild(i).gameObject);
            }

            return;
        }

        for (int i = visual.childCount - 1; i >= 0; i--)
        {
            Transform child = visual.GetChild(i);
            if (child.name == SceneModuleVisualUtility.AxisCorrectionName)
            {
                continue;
            }

            Object.DestroyImmediate(child.gameObject);
        }
    }

    static void RegisterPrefab(SceneModulePrefabRegistry registry, SceneModuleKind kind, GameObject prefab)
    {
        SerializedObject serialized = new SerializedObject(registry);
        SerializedProperty entries = serialized.FindProperty("entries");
        int index = FindOrAddEntryIndex(entries, kind);
        SerializedProperty entry = entries.GetArrayElementAtIndex(index);
        entry.FindPropertyRelative("Kind").enumValueIndex = (int)kind;
        entry.FindPropertyRelative("Prefab").objectReferenceValue = prefab;
        serialized.ApplyModifiedPropertiesWithoutUndo();
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

    static SceneModulePrefabRegistry LoadOrCreateRegistry()
    {
        var registry = AssetDatabase.LoadAssetAtPath<SceneModulePrefabRegistry>(RegistryPath);
        if (registry != null)
        {
            return registry;
        }

        EnsureFolder("Assets/_Project/Resources/SceneModules");
        registry = ScriptableObject.CreateInstance<SceneModulePrefabRegistry>();
        AssetDatabase.CreateAsset(registry, RegistryPath);
        return registry;
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

    static MaterialSpec Mat(
        string name,
        Color color,
        float smoothness,
        float metallic,
        Color emission = default,
        float emissionStrength = 0f)
    {
        return new MaterialSpec
        {
            Name = name,
            Color = color,
            Smoothness = smoothness,
            Metallic = metallic,
            Emission = emission == default ? Color.black : emission,
            EmissionStrength = emissionStrength,
        };
    }
}
#endif
