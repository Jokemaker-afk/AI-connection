#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Editor sync for Workbench and Furnace collider fitting.</summary>
public static class PlacedObjectColliderFitterEditorMenu
{
    [MenuItem("Tools/SceneModules/Sync Workbench And Furnace Colliders To Visuals")]
    public static void SyncWorkbenchAndFurnaceCollidersToVisuals()
    {
        int prefabCount = 0;
        int sceneCount = 0;

        prefabCount += SyncPrefab(WorkstationAssetPaths.CraftingTablePrefab, ItemKind.CraftingTable);
        prefabCount += SyncPrefab(WorkstationAssetPaths.FurnacePrefab, ItemKind.Furnace);
        sceneCount += SyncOpenSceneInstances(ItemKind.CraftingTable);
        sceneCount += SyncOpenSceneInstances(ItemKind.Furnace);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        ItemPlacedPrefabRegistry.SetCachedForTests(null);

        Debug.Log(
            $"[SceneModules] Workbench/Furnace collider sync complete. " +
            $"Prefabs updated: {prefabCount}, open-scene instances updated: {sceneCount}.");
    }

    static int SyncPrefab(string prefabPath, ItemKind kind)
    {
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (asset == null)
        {
            Debug.LogWarning("[SceneModules] Missing prefab: " + prefabPath);
            return 0;
        }

        GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
        if (root == null)
        {
            return 0;
        }

        try
        {
            ApplyFitter(root, kind);
            LogBoundsReport(kind, prefabPath, root);
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            return 1;
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    static int SyncOpenSceneInstances(ItemKind kind)
    {
        int updated = 0;
        PlacedBuilding[] buildings = Object.FindObjectsByType<PlacedBuilding>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < buildings.Length; i++)
        {
            PlacedBuilding building = buildings[i];
            if (building == null || building.SourceItem != kind)
            {
                continue;
            }

            ApplyFitter(building.gameObject, kind);
            EditorUtility.SetDirty(building.gameObject);
            updated++;
        }

        if (updated > 0)
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        return updated;
    }

    static void ApplyFitter(GameObject root, ItemKind kind)
    {
        PlacedObjectColliderFitter fitter = root.GetComponent<PlacedObjectColliderFitter>();
        if (fitter == null)
        {
            fitter = root.AddComponent<PlacedObjectColliderFitter>();
        }

        fitter.Initialize(kind);
        fitter.ResolveVisualRootAuto();
        fitter.SyncAll();
        PlacedObjectInteractionSetup.Configure(root, kind);
        EditorUtility.SetDirty(fitter);
    }

    static void LogBoundsReport(ItemKind kind, string path, GameObject root)
    {
        PlacedObjectColliderFitter fitter = root.GetComponent<PlacedObjectColliderFitter>();
        if (fitter == null)
        {
            return;
        }

        Bounds visual = fitter.CalculateVisualBounds();
        BoxCollider physical = root.GetComponent<BoxCollider>();
        Transform interaction = root.transform.Find(PlacedObjectBoundsUtility.InteractionColliderName);
        BoxCollider interactionBox = interaction != null ? interaction.GetComponent<BoxCollider>() : null;

        Debug.Log(
            $"[SceneModules] {kind} @ {path}\n" +
            $"  Visual bounds center={visual.center}, size={visual.size}\n" +
            $"  Physical collider center={(physical != null ? physical.center.ToString() : "missing")}, " +
            $"size={(physical != null ? physical.size.ToString() : "missing")}\n" +
            $"  Interaction collider center={(interactionBox != null ? interactionBox.center.ToString() : "missing")}, " +
            $"size={(interactionBox != null ? interactionBox.size.ToString() : "missing")}\n" +
            $"  Footprint size={fitter.PlacementHalfExtents * 2f}");
    }
}
#endif
