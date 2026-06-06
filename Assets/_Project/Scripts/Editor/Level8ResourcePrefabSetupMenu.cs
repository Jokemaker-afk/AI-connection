#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Creates Phase C resource test prefabs and registers them in Level8ResourcePrefabRegistry.
/// </summary>
public static class Level8ResourcePrefabSetupMenu
{
    const string PrefabFolder = "Assets/_Project/Prefabs/Level8/ResourcePrefabs";
    const string WoodPrefabPath = PrefabFolder + "/PF_Generic_Resource_Wood_01.prefab";
    const string StonePrefabPath = PrefabFolder + "/PF_Generic_Resource_Stone_01.prefab";
    const string OrePrefabPath = PrefabFolder + "/PF_Generic_Resource_OreFragment_01.prefab";
    const string RegistryResourcePath = "Assets/_Project/Resources/Level8/Level8ResourcePrefabRegistry.asset";

    [MenuItem("Tools/Level8/Create Resource Test Prefabs And Registry")]
    public static void CreateResourceTestPrefabsAndRegistry()
    {
        Level8ObjectivePrefabSetupMenu.EnsureFolderPublic(PrefabFolder);
        Level8ObjectivePrefabSetupMenu.EnsureFolderPublic("Assets/_Project/Resources/Level8");

        GameObject woodRoot = BuildWoodVisual();
        GameObject stoneRoot = BuildStoneVisual();
        GameObject oreRoot = BuildOreFragmentVisual();

        GameObject woodPrefab = SavePrefab(woodRoot, WoodPrefabPath);
        GameObject stonePrefab = SavePrefab(stoneRoot, StonePrefabPath);
        GameObject orePrefab = SavePrefab(oreRoot, OrePrefabPath);

        Object.DestroyImmediate(woodRoot);
        Object.DestroyImmediate(stoneRoot);
        Object.DestroyImmediate(oreRoot);

        Level8ResourcePrefabRegistry registry = LoadOrCreateRegistry();
        SerializedObject serialized = new SerializedObject(registry);

        SerializedProperty genericEntries = serialized.FindProperty("genericEntries");
        genericEntries.arraySize = 3;
        SetResourceEntry(genericEntries, 0, ItemKind.Wood, woodPrefab);
        SetResourceEntry(genericEntries, 1, ItemKind.Stone, stonePrefab);
        SetResourceEntry(genericEntries, 2, ItemKind.OreFragment, orePrefab);

        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(registry);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Level8ResourcePrefabRegistry.SetCachedForTests(null);

        Debug.Log(
            "[Level8] Resource test prefabs created and registered.\n" +
            $"  Wood: {WoodPrefabPath}\n" +
            $"  Stone: {StonePrefabPath}\n" +
            $"  OreFragment: {OrePrefabPath}\n" +
            $"  Registry: {RegistryResourcePath}\n" +
            "  Use Tools/Level8/Clear Resource Prefab Registry to test primitive fallback.");
    }

    [MenuItem("Tools/Level8/Clear Resource Prefab Registry (Test Fallback)")]
    public static void ClearResourcePrefabRegistry()
    {
        Level8ResourcePrefabRegistry registry = LoadOrCreateRegistry();
        SerializedObject serialized = new SerializedObject(registry);
        serialized.FindProperty("genericEntries").arraySize = 0;
        serialized.FindProperty("biomeOverrides").arraySize = 0;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(registry);
        AssetDatabase.SaveAssets();
        Level8ResourcePrefabRegistry.SetCachedForTests(null);
        Debug.Log("[Level8] Resource prefab registry cleared — primitive fallback active.");
    }

    static GameObject BuildWoodVisual()
    {
        var root = new GameObject("PF_Generic_Resource_Wood_01");
        var visual = new GameObject("Visual");
        visual.transform.SetParent(root.transform, false);

        var log = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        log.name = "Log";
        log.transform.SetParent(visual.transform, false);
        log.transform.localPosition = Vector3.zero;
        log.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        log.transform.localScale = new Vector3(0.28f, 0.55f, 0.28f);
        Object.DestroyImmediate(log.GetComponent<Collider>());
        Level8VisualMaterialUtility.ApplyColor(log.GetComponent<Renderer>(), new Color(0.55f, 0.36f, 0.18f));

        var endCap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        endCap.name = "EndCap";
        endCap.transform.SetParent(visual.transform, false);
        endCap.transform.localPosition = new Vector3(0.52f, 0f, 0f);
        endCap.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
        endCap.transform.localScale = new Vector3(0.32f, 0.06f, 0.32f);
        Object.DestroyImmediate(endCap.GetComponent<Collider>());
        Level8VisualMaterialUtility.ApplyColor(endCap.GetComponent<Renderer>(), new Color(0.68f, 0.48f, 0.28f));

        return root;
    }

    static GameObject BuildStoneVisual()
    {
        var root = new GameObject("PF_Generic_Resource_Stone_01");
        var visual = new GameObject("Visual");
        visual.transform.SetParent(root.transform, false);

        CreateRockChunk(visual.transform, "RockA", new Vector3(0f, 0.08f, 0f), new Vector3(0.42f, 0.28f, 0.36f), new Color(0.58f, 0.58f, 0.62f));
        CreateRockChunk(visual.transform, "RockB", new Vector3(0.18f, 0.14f, 0.12f), new Vector3(0.28f, 0.22f, 0.24f), new Color(0.48f, 0.5f, 0.54f));
        CreateRockChunk(visual.transform, "RockC", new Vector3(-0.16f, 0.1f, -0.1f), new Vector3(0.3f, 0.2f, 0.26f), new Color(0.52f, 0.54f, 0.58f));

        return root;
    }

    static GameObject BuildOreFragmentVisual()
    {
        var root = new GameObject("PF_Generic_Resource_OreFragment_01");
        var visual = new GameObject("Visual");
        visual.transform.SetParent(root.transform, false);

        CreateRockChunk(visual.transform, "OreBody", new Vector3(0f, 0.1f, 0f), new Vector3(0.4f, 0.26f, 0.38f), new Color(0.22f, 0.2f, 0.24f));

        var accent = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        accent.name = "OreAccent";
        accent.transform.SetParent(visual.transform, false);
        accent.transform.localPosition = new Vector3(0.12f, 0.18f, 0.08f);
        accent.transform.localScale = Vector3.one * 0.16f;
        Object.DestroyImmediate(accent.GetComponent<Collider>());
        Level8VisualMaterialUtility.ApplyColor(accent.GetComponent<Renderer>(), new Color(0.55f, 0.35f, 0.95f));

        return root;
    }

    static void CreateRockChunk(Transform parent, string name, Vector3 localPos, Vector3 scale, Color color)
    {
        var rock = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rock.name = name;
        rock.transform.SetParent(parent, false);
        rock.transform.localPosition = localPos;
        rock.transform.localRotation = Quaternion.Euler(12f, 24f, -8f);
        rock.transform.localScale = scale;
        Object.DestroyImmediate(rock.GetComponent<Collider>());
        Level8VisualMaterialUtility.ApplyColor(rock.GetComponent<Renderer>(), color);
    }

    static void SetResourceEntry(SerializedProperty array, int index, ItemKind kind, GameObject prefab)
    {
        SerializedProperty entry = array.GetArrayElementAtIndex(index);
        entry.FindPropertyRelative("Kind").enumValueIndex = (int)kind;
        entry.FindPropertyRelative("Prefab").objectReferenceValue = prefab;
    }

    static GameObject SavePrefab(GameObject root, string assetPath)
    {
        GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (existing != null)
        {
            AssetDatabase.DeleteAsset(assetPath);
        }

        return PrefabUtility.SaveAsPrefabAsset(root, assetPath);
    }

    static Level8ResourcePrefabRegistry LoadOrCreateRegistry()
    {
        var registry = AssetDatabase.LoadAssetAtPath<Level8ResourcePrefabRegistry>(RegistryResourcePath);
        if (registry != null)
        {
            return registry;
        }

        registry = ScriptableObject.CreateInstance<Level8ResourcePrefabRegistry>();
        AssetDatabase.CreateAsset(registry, RegistryResourcePath);
        return registry;
    }
}
#endif
