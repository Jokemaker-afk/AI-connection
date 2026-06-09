#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>Batch-apply default Blender FBX axis correction to visual prefabs.</summary>
public static class SceneModuleBlenderAxisCorrectionMenu
{
    const string SceneModulesPrefabRoot = "Assets/_Project/Prefabs/SceneModules";

    [MenuItem("Tools/SceneModules/Apply Default Blender Axis Correction To All Visual Prefabs")]
    public static void ApplyToAllSceneModuleVisualPrefabs()
    {
        ApplyToPrefabsAtPaths(FindSceneModuleVisualPrefabPaths());
    }

    [MenuItem("Tools/SceneModules/Flatten Visual AxisCorrection Layer (All SceneModule Prefabs)")]
    public static void FlattenAxisCorrectionOnAllSceneModulePrefabs()
    {
        FlattenPrefabsAtPaths(FindSceneModuleVisualPrefabPaths());
    }

    [MenuItem("Tools/SceneModules/Flatten SpikeTrap And ElectricField Visual Hierarchy")]
    public static void FlattenSpikeTrapAndElectricFieldVisualHierarchy()
    {
        FlattenPrefabsAtPaths(new[]
        {
            "Assets/_Project/Prefabs/SceneModules/Hazards/PF_Generic_Hazard_SpikeTrap_01.prefab",
            "Assets/_Project/Prefabs/SceneModules/Hazards/PF_Generic_Hazard_ElectricField_01_Visual.prefab",
        });
    }

    public static void ApplyToPrefabAsset(string prefabPath)
    {
        if (string.IsNullOrEmpty(prefabPath))
        {
            return;
        }

        ApplyToPrefabsAtPaths(new[] { prefabPath });
    }

    public static IReadOnlyList<string> FindSceneModuleVisualPrefabPaths()
    {
        string[] guids = AssetDatabase.FindAssets("PF_Generic_ t:Prefab", new[] { SceneModulesPrefabRoot });
        var paths = new List<string>(guids.Length);
        for (int i = 0; i < guids.Length; i++)
        {
            paths.Add(AssetDatabase.GUIDToAssetPath(guids[i]));
        }

        return paths;
    }

    static void ApplyToPrefabsAtPaths(IReadOnlyList<string> prefabPaths)
    {
        int updated = 0;
        for (int i = 0; i < prefabPaths.Count; i++)
        {
            string path = prefabPaths[i];
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            if (root == null)
            {
                continue;
            }

            SceneModuleVisualUtility.EnsureBlenderAxisCorrection(root);
            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
            updated++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log(
            "[SceneModules] Applied flat Blender axis correction (90, 0, 180) to "
            + updated
            + " visual prefab(s).");
    }

    static void FlattenPrefabsAtPaths(IReadOnlyList<string> prefabPaths)
    {
        int updated = 0;
        for (int i = 0; i < prefabPaths.Count; i++)
        {
            string path = prefabPaths[i];
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null)
            {
                continue;
            }

            GameObject root = PrefabUtility.LoadPrefabContents(path);
            if (root == null)
            {
                continue;
            }

            if (SceneModuleVisualUtility.FlattenAxisCorrectionLayer(root))
            {
                PrefabUtility.SaveAsPrefabAsset(root, path);
                updated++;
            }

            PrefabUtility.UnloadPrefabContents(root);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[SceneModules] Flattened Visual → FBX hierarchy on " + updated + " prefab(s).");
    }
}
#endif
