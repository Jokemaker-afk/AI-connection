#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>Creates Phase SM-B ReturnButton and Phase SM-C BuffStation test prefabs; registers in SceneModulePrefabRegistry.</summary>
public static class SceneModulePrefabSetupMenu
{
    const string ReturnButtonPrefabPath =
        "Assets/_Project/Prefabs/SceneModules/Interactables/PF_Generic_Interactable_ReturnButton_01.prefab";

    const string HealStationPrefabPath =
        "Assets/_Project/Prefabs/SceneModules/BuffStations/PF_Generic_BuffStation_Heal_01.prefab";

    const string StaminaStationPrefabPath =
        "Assets/_Project/Prefabs/SceneModules/BuffStations/PF_Generic_BuffStation_Stamina_01.prefab";

    const string RegistryResourcePath =
        "Assets/_Project/Resources/SceneModules/SceneModulePrefabRegistry.asset";

    [MenuItem("Tools/SceneModules/Create ReturnButton Test Prefab And Registry")]
    public static void CreateReturnButtonTestPrefabAndRegistry()
    {
        Level8ObjectivePrefabSetupMenu.EnsureFolderPublic("Assets/_Project/Prefabs/SceneModules/Interactables");
        Level8ObjectivePrefabSetupMenu.EnsureFolderPublic("Assets/_Project/Resources/SceneModules");

        GameObject root = BuildReturnButtonVisualHierarchy();
        GameObject prefab = SavePrefab(root, ReturnButtonPrefabPath);
        Object.DestroyImmediate(root);

        SceneModulePrefabRegistry registry = LoadOrCreateRegistry();
        SerializedObject serialized = new SerializedObject(registry);

        SerializedProperty entries = serialized.FindProperty("entries");
        int returnButtonIndex = FindOrAddEntryIndex(entries, SceneModuleKind.ReturnButton);
        SetEntry(entries, returnButtonIndex, SceneModuleKind.ReturnButton, prefab);

        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(registry);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        SceneModulePrefabRegistry.SetCachedForTests(null);

        Debug.Log(
            "[SceneModules] ReturnButton test prefab created and registered.\n" +
            $"  Prefab: {ReturnButtonPrefabPath}\n" +
            $"  Registry: {RegistryResourcePath}\n" +
            "  Use Tools/SceneModules/Clear ReturnButton Prefab Registry to test primitive fallback.");
    }

    [MenuItem("Tools/SceneModules/Create BuffStation Test Prefabs And Registry")]
    public static void CreateBuffStationTestPrefabsAndRegistry()
    {
        Level8ObjectivePrefabSetupMenu.EnsureFolderPublic("Assets/_Project/Prefabs/SceneModules/BuffStations");
        Level8ObjectivePrefabSetupMenu.EnsureFolderPublic("Assets/_Project/Resources/SceneModules");

        GameObject healRoot = BuildHealStationVisualHierarchy();
        GameObject healPrefab = SavePrefab(healRoot, HealStationPrefabPath);
        Object.DestroyImmediate(healRoot);

        GameObject staminaRoot = BuildStaminaStationVisualHierarchy();
        GameObject staminaPrefab = SavePrefab(staminaRoot, StaminaStationPrefabPath);
        Object.DestroyImmediate(staminaRoot);

        SceneModulePrefabRegistry registry = LoadOrCreateRegistry();
        SerializedObject serialized = new SerializedObject(registry);

        SerializedProperty entries = serialized.FindProperty("entries");
        int healIndex = FindOrAddEntryIndex(entries, SceneModuleKind.HealStation);
        SetEntry(entries, healIndex, SceneModuleKind.HealStation, healPrefab);

        int staminaIndex = FindOrAddEntryIndex(entries, SceneModuleKind.StaminaStation);
        SetEntry(entries, staminaIndex, SceneModuleKind.StaminaStation, staminaPrefab);

        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(registry);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        SceneModulePrefabRegistry.SetCachedForTests(null);

        Debug.Log(
            "[SceneModules] BuffStation test prefabs created and registered.\n" +
            $"  Heal: {HealStationPrefabPath}\n" +
            $"  Stamina: {StaminaStationPrefabPath}\n" +
            $"  Registry: {RegistryResourcePath}\n" +
            "  Use Tools/SceneModules/Clear BuffStation Prefab Registry to test primitive fallback.");
    }

    [MenuItem("Tools/SceneModules/Clear BuffStation Prefab Registry (Test Fallback)")]
    public static void ClearBuffStationPrefabRegistry()
    {
        SceneModulePrefabRegistry registry = LoadOrCreateRegistry();
        SerializedObject serialized = new SerializedObject(registry);
        SerializedProperty entries = serialized.FindProperty("entries");

        for (int i = entries.arraySize - 1; i >= 0; i--)
        {
            SerializedProperty entry = entries.GetArrayElementAtIndex(i);
            var kind = (SceneModuleKind)entry.FindPropertyRelative("Kind").enumValueIndex;
            if (kind == SceneModuleKind.HealStation
                || kind == SceneModuleKind.StaminaStation
                || kind == SceneModuleKind.BuffStation)
            {
                entries.DeleteArrayElementAtIndex(i);
            }
        }

        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(registry);
        AssetDatabase.SaveAssets();
        SceneModulePrefabRegistry.SetCachedForTests(null);
        Debug.Log("[SceneModules] BuffStation prefab registry cleared — primitive fallback active.");
    }

    [MenuItem("Tools/SceneModules/Clear ReturnButton Prefab Registry (Test Fallback)")]
    public static void ClearReturnButtonPrefabRegistry()
    {
        SceneModulePrefabRegistry registry = LoadOrCreateRegistry();
        SerializedObject serialized = new SerializedObject(registry);
        SerializedProperty entries = serialized.FindProperty("entries");

        for (int i = entries.arraySize - 1; i >= 0; i--)
        {
            SerializedProperty entry = entries.GetArrayElementAtIndex(i);
            var kind = (SceneModuleKind)entry.FindPropertyRelative("Kind").enumValueIndex;
            if (kind == SceneModuleKind.ReturnButton || kind == SceneModuleKind.InteractionButton)
            {
                entries.DeleteArrayElementAtIndex(i);
            }
        }

        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(registry);
        AssetDatabase.SaveAssets();
        SceneModulePrefabRegistry.SetCachedForTests(null);
        Debug.Log("[SceneModules] ReturnButton prefab registry cleared — primitive fallback active.");
    }

    static GameObject BuildReturnButtonVisualHierarchy()
    {
        var root = new GameObject("PF_Generic_Interactable_ReturnButton_01");
        var visual = new GameObject("Visual");
        visual.transform.SetParent(root.transform, false);

        var buttonBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        buttonBase.name = "ButtonBase";
        buttonBase.transform.SetParent(visual.transform, false);
        buttonBase.transform.localPosition = new Vector3(0f, 0.08f, 0f);
        buttonBase.transform.localScale = new Vector3(2.6f, 0.12f, 2.6f);
        Object.DestroyImmediate(buttonBase.GetComponent<Collider>());
        ApplyColor(buttonBase.GetComponent<Renderer>(), new Color(0.18f, 0.22f, 0.28f));

        var buttonTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        buttonTop.name = "ButtonTop";
        buttonTop.transform.SetParent(visual.transform, false);
        buttonTop.transform.localPosition = new Vector3(0f, 0.32f, 0f);
        buttonTop.transform.localScale = new Vector3(2.0f, 0.22f, 1.4f);
        Object.DestroyImmediate(buttonTop.GetComponent<Collider>());
        ApplyEmissive(buttonTop.GetComponent<Renderer>(), new Color(0.25f, 0.55f, 0.95f), 0.65f);

        var glowRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        glowRing.name = "GlowRing";
        glowRing.transform.SetParent(visual.transform, false);
        glowRing.transform.localPosition = new Vector3(0f, 0.04f, 0f);
        glowRing.transform.localScale = new Vector3(2.9f, 0.03f, 2.9f);
        Object.DestroyImmediate(glowRing.GetComponent<Collider>());
        ApplyColor(glowRing.GetComponent<Renderer>(), new Color(0.35f, 0.75f, 1f, 0.45f), transparent: true);

        var arrow = GameObject.CreatePrimitive(PrimitiveType.Cube);
        arrow.name = "DirectionArrow";
        arrow.transform.SetParent(visual.transform, false);
        arrow.transform.localPosition = new Vector3(0f, 0.42f, -0.55f);
        arrow.transform.localRotation = Quaternion.Euler(25f, 0f, 0f);
        arrow.transform.localScale = new Vector3(0.35f, 0.08f, 0.55f);
        Object.DestroyImmediate(arrow.GetComponent<Collider>());
        ApplyEmissive(arrow.GetComponent<Renderer>(), new Color(0.9f, 0.95f, 1f), 0.4f);

        return root;
    }

    static GameObject BuildHealStationVisualHierarchy()
    {
        var root = new GameObject("PF_Generic_BuffStation_Heal_01");
        var visual = new GameObject("Visual");
        visual.transform.SetParent(root.transform, false);

        var stationBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stationBase.name = "StationBase";
        stationBase.transform.SetParent(visual.transform, false);
        stationBase.transform.localPosition = new Vector3(0f, 0.06f, 0f);
        stationBase.transform.localScale = new Vector3(2.2f, 0.1f, 2.2f);
        Object.DestroyImmediate(stationBase.GetComponent<Collider>());
        ApplyColor(stationBase.GetComponent<Renderer>(), new Color(0.12f, 0.28f, 0.16f));

        var healPad = GameObject.CreatePrimitive(PrimitiveType.Cube);
        healPad.name = "HealPad";
        healPad.transform.SetParent(visual.transform, false);
        healPad.transform.localPosition = new Vector3(0f, 0.18f, 0f);
        healPad.transform.localScale = new Vector3(1.4f, 0.08f, 1.4f);
        Object.DestroyImmediate(healPad.GetComponent<Collider>());
        ApplyEmissive(healPad.GetComponent<Renderer>(), new Color(0.45f, 0.95f, 0.55f), 0.55f);

        var glowCore = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        glowCore.name = "GlowCore";
        glowCore.transform.SetParent(visual.transform, false);
        glowCore.transform.localPosition = new Vector3(0f, 0.55f, 0f);
        glowCore.transform.localScale = Vector3.one * 0.45f;
        Object.DestroyImmediate(glowCore.GetComponent<Collider>());
        ApplyEmissive(glowCore.GetComponent<Renderer>(), new Color(0.55f, 1f, 0.65f), 1.1f);

        var glowRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        glowRing.name = "GlowRing";
        glowRing.transform.SetParent(visual.transform, false);
        glowRing.transform.localPosition = new Vector3(0f, 0.22f, 0f);
        glowRing.transform.localScale = new Vector3(1.6f, 0.025f, 1.6f);
        Object.DestroyImmediate(glowRing.GetComponent<Collider>());
        ApplyColor(glowRing.GetComponent<Renderer>(), new Color(0.7f, 1f, 0.75f, 0.5f), transparent: true);

        var iconVertical = GameObject.CreatePrimitive(PrimitiveType.Cube);
        iconVertical.name = "MedicalIcon";
        iconVertical.transform.SetParent(visual.transform, false);
        iconVertical.transform.localPosition = new Vector3(0f, 0.62f, 0f);
        iconVertical.transform.localScale = new Vector3(0.12f, 0.35f, 0.12f);
        Object.DestroyImmediate(iconVertical.GetComponent<Collider>());
        ApplyColor(iconVertical.GetComponent<Renderer>(), Color.white);

        var iconHorizontal = GameObject.CreatePrimitive(PrimitiveType.Cube);
        iconHorizontal.name = "MedicalIconCross";
        iconHorizontal.transform.SetParent(iconVertical.transform, false);
        iconHorizontal.transform.localPosition = Vector3.zero;
        iconHorizontal.transform.localScale = new Vector3(2.4f, 0.45f, 1f);
        Object.DestroyImmediate(iconHorizontal.GetComponent<Collider>());
        ApplyColor(iconHorizontal.GetComponent<Renderer>(), Color.white);

        return root;
    }

    static GameObject BuildStaminaStationVisualHierarchy()
    {
        var root = new GameObject("PF_Generic_BuffStation_Stamina_01");
        var visual = new GameObject("Visual");
        visual.transform.SetParent(root.transform, false);

        var stationBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        stationBase.name = "StationBase";
        stationBase.transform.SetParent(visual.transform, false);
        stationBase.transform.localPosition = new Vector3(0f, 0.06f, 0f);
        stationBase.transform.localScale = new Vector3(2.2f, 0.1f, 2.2f);
        Object.DestroyImmediate(stationBase.GetComponent<Collider>());
        ApplyColor(stationBase.GetComponent<Renderer>(), new Color(0.1f, 0.18f, 0.32f));

        var energyPad = GameObject.CreatePrimitive(PrimitiveType.Cube);
        energyPad.name = "EnergyPad";
        energyPad.transform.SetParent(visual.transform, false);
        energyPad.transform.localPosition = new Vector3(0f, 0.18f, 0f);
        energyPad.transform.localScale = new Vector3(1.4f, 0.08f, 1.4f);
        Object.DestroyImmediate(energyPad.GetComponent<Collider>());
        ApplyEmissive(energyPad.GetComponent<Renderer>(), new Color(0.35f, 0.75f, 1f), 0.6f);

        var glowCore = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        glowCore.name = "GlowCore";
        glowCore.transform.SetParent(visual.transform, false);
        glowCore.transform.localPosition = new Vector3(0f, 0.55f, 0f);
        glowCore.transform.localScale = Vector3.one * 0.45f;
        Object.DestroyImmediate(glowCore.GetComponent<Collider>());
        ApplyEmissive(glowCore.GetComponent<Renderer>(), new Color(1f, 0.9f, 0.25f), 1.15f);

        var glowRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        glowRing.name = "GlowRing";
        glowRing.transform.SetParent(visual.transform, false);
        glowRing.transform.localPosition = new Vector3(0f, 0.22f, 0f);
        glowRing.transform.localScale = new Vector3(1.6f, 0.025f, 1.6f);
        Object.DestroyImmediate(glowRing.GetComponent<Collider>());
        ApplyColor(glowRing.GetComponent<Renderer>(), new Color(0.45f, 0.85f, 1f, 0.5f), transparent: true);

        var energyIcon = GameObject.CreatePrimitive(PrimitiveType.Cube);
        energyIcon.name = "EnergyIcon";
        energyIcon.transform.SetParent(visual.transform, false);
        energyIcon.transform.localPosition = new Vector3(0f, 0.62f, 0f);
        energyIcon.transform.localRotation = Quaternion.Euler(0f, 0f, 35f);
        energyIcon.transform.localScale = new Vector3(0.14f, 0.38f, 0.14f);
        Object.DestroyImmediate(energyIcon.GetComponent<Collider>());
        ApplyEmissive(energyIcon.GetComponent<Renderer>(), new Color(0.95f, 0.95f, 0.35f), 0.75f);

        var energyIconBolt = GameObject.CreatePrimitive(PrimitiveType.Cube);
        energyIconBolt.name = "EnergyIconBolt";
        energyIconBolt.transform.SetParent(energyIcon.transform, false);
        energyIconBolt.transform.localPosition = new Vector3(0.15f, -0.15f, 0f);
        energyIconBolt.transform.localRotation = Quaternion.Euler(0f, 0f, -70f);
        energyIconBolt.transform.localScale = new Vector3(0.85f, 0.55f, 1f);
        Object.DestroyImmediate(energyIconBolt.GetComponent<Collider>());
        ApplyEmissive(energyIconBolt.GetComponent<Renderer>(), new Color(0.95f, 0.95f, 0.35f), 0.75f);

        return root;
    }

    static void ApplyColor(Renderer renderer, Color color, bool transparent = false)
    {
        if (renderer == null)
        {
            return;
        }

        var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.color = color;
        if (transparent)
        {
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.renderQueue = 3000;
        }

        renderer.sharedMaterial = material;
    }

    static void ApplyEmissive(Renderer renderer, Color color, float strength)
    {
        if (renderer == null)
        {
            return;
        }

        var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.color = color;
        material.SetColor("_EmissionColor", color * strength);
        material.EnableKeyword("_EMISSION");
        renderer.sharedMaterial = material;
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

    static void SetEntry(SerializedProperty entries, int index, SceneModuleKind kind, GameObject prefab)
    {
        SerializedProperty entry = entries.GetArrayElementAtIndex(index);
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

    static SceneModulePrefabRegistry LoadOrCreateRegistry()
    {
        var registry = AssetDatabase.LoadAssetAtPath<SceneModulePrefabRegistry>(RegistryResourcePath);
        if (registry != null)
        {
            return registry;
        }

        registry = ScriptableObject.CreateInstance<SceneModulePrefabRegistry>();
        AssetDatabase.CreateAsset(registry, RegistryResourcePath);
        return registry;
    }
}
#endif
