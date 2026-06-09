#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Normalizes baked LaserHazard objects to LogicRoot + VisualRoot electric field module layout.</summary>
public static class LaserHazardSceneMigrationMenu
{
    static readonly string[] LevelScenePaths =
    {
        "Assets/_Project/Scenes/Levels/Level1/SampleScene.unity",
        "Assets/_Project/Scenes/Levels/Level2/Level2.unity",
        "Assets/_Project/Scenes/Levels/Level3/Level3.unity",
        "Assets/_Project/Scenes/Levels/Level4/Level4.unity",
        "Assets/_Project/Scenes/Levels/Level5/Level5.unity",
        "Assets/_Project/Scenes/Levels/Level6/Level6.unity",
        "Assets/_Project/Scenes/Levels/Level7/Level7.unity",
        "Assets/_Project/Scenes/Levels/Level8/Level8.unity",
    };

    [MenuItem("Tools/SceneModules/Normalize Existing ElectricField Visuals")]
    public static void NormalizeExistingElectricFieldVisualsCurrentScene()
    {
        MigrateCurrentScene();
    }

    [MenuItem("Tools/SceneModules/Migrate ElectricField Hazards In Current Scene")]
    public static void MigrateCurrentScene()
    {
        int count = MigrateLaserHazardsInScene(SceneManager.GetActiveScene());
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log($"[LaserHazard] Normalized {count} electric field hazard(s) in current scene.");
    }

    [MenuItem("Tools/SceneModules/Migrate ElectricField Hazards In All Level Scenes")]
    public static void MigrateAllLevelScenes()
    {
        string originalScene = SceneManager.GetActiveScene().path;
        int total = 0;

        for (int i = 0; i < LevelScenePaths.Length; i++)
        {
            if (!System.IO.File.Exists(LevelScenePaths[i]))
            {
                continue;
            }

            var scene = EditorSceneManager.OpenScene(LevelScenePaths[i], OpenSceneMode.Single);
            int count = MigrateLaserHazardsInScene(scene);
            if (count > 0)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            total += count;
            Debug.Log($"[LaserHazard] Normalized {count} electric field hazard(s) in {LevelScenePaths[i]}.");
        }

        if (!string.IsNullOrEmpty(originalScene) && System.IO.File.Exists(originalScene))
        {
            EditorSceneManager.OpenScene(originalScene, OpenSceneMode.Single);
        }

        Debug.Log($"[LaserHazard] Normalization complete. Total electric field hazards processed: {total}.");
    }

    static int MigrateLaserHazardsInScene(Scene scene)
    {
        var hazards = Object.FindObjectsByType<LaserHazard>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int migrated = 0;

        for (int i = 0; i < hazards.Length; i++)
        {
            LaserHazard hazard = hazards[i];
            if (hazard == null)
            {
                continue;
            }

            if (hazard.gameObject.name == SceneModuleVisualUtility.LogicRootName)
            {
                hazard.EnsureModuleLayout();
                EditorUtility.SetDirty(hazard);
            }
            else if (hazard.transform.Find(SceneModuleVisualUtility.LogicRootName) == null)
            {
                MigrateLegacyHazardInEditor(hazard);
            }
            else
            {
                MigrateDuplicateRootHazardInEditor(hazard);
            }

            migrated++;
        }

        return migrated;
    }

    static void MigrateLegacyHazardInEditor(LaserHazard hazard)
    {
        Transform hazardRoot = hazard.transform;
        Transform logicRoot = HazardModuleLayoutUtility.EnsureChild(hazardRoot, SceneModuleVisualUtility.LogicRootName);
        Transform visualRoot = HazardModuleLayoutUtility.EnsureChild(hazardRoot, SceneModuleVisualUtility.VisualRootName);
        HazardModuleLayoutUtility.EnsureChild(hazardRoot, HazardModuleLayoutUtility.WarningAreaName);

        var sourceBox = hazard.GetComponent<BoxCollider>();
        BoxCollider logicBox = logicRoot.GetComponent<BoxCollider>();
        if (logicBox == null)
        {
            logicBox = logicRoot.gameObject.AddComponent<BoxCollider>();
        }

        if (sourceBox != null)
        {
            logicBox.isTrigger = sourceBox.isTrigger;
            logicBox.size = sourceBox.size;
            logicBox.center = sourceBox.center;
            Object.DestroyImmediate(sourceBox);
        }

        LaserHazard logicHazard = logicRoot.GetComponent<LaserHazard>();
        if (logicHazard == null)
        {
            logicHazard = logicRoot.gameObject.AddComponent<LaserHazard>();
        }

        logicHazard.CopySettingsFrom(hazard);
        Object.DestroyImmediate(hazard);
        logicHazard.EnsureModuleLayout();
        EditorUtility.SetDirty(logicHazard);
    }

    static void MigrateDuplicateRootHazardInEditor(LaserHazard hazard)
    {
        Transform hazardRoot = hazard.transform;
        Transform logicRoot = hazardRoot.Find(SceneModuleVisualUtility.LogicRootName);
        if (logicRoot == null)
        {
            MigrateLegacyHazardInEditor(hazard);
            return;
        }

        LaserHazard logicHazard = logicRoot.GetComponent<LaserHazard>();
        if (logicHazard == null)
        {
            logicHazard = logicRoot.gameObject.AddComponent<LaserHazard>();
            logicHazard.CopySettingsFrom(hazard);
        }

        var sourceBox = hazard.GetComponent<BoxCollider>();
        if (sourceBox != null)
        {
            Object.DestroyImmediate(sourceBox);
        }

        Object.DestroyImmediate(hazard);
        logicHazard.EnsureModuleLayout();
        logicHazard.SyncColliderToVisualBounds(force: true);
        EditorUtility.SetDirty(logicHazard);
    }

    [MenuItem("Tools/SceneModules/Sync ElectricField Colliders To Visuals")]
    public static void SyncElectricFieldCollidersToVisualsCurrentScene()
    {
        int count = SyncElectricFieldCollidersInScene(SceneManager.GetActiveScene(), force: true);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log($"[LaserHazard] Synced colliders for {count} electric field hazard(s) in current scene.");
    }

    [MenuItem("Tools/SceneModules/Sync ElectricField Warning Areas")]
    public static void SyncElectricFieldWarningAreasCurrentScene()
    {
        SyncSummary summary = SyncElectricFieldWarningAreasInScene(SceneManager.GetActiveScene(), force: true);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log(
            $"[LaserHazard] Warning area sync — found: {summary.TotalFound}, synced: {summary.Synced}, " +
            $"missing warning: {summary.MissingWarningArea}, missing collider: {summary.MissingCollider}, " +
            $"missing visual: {summary.MissingVisual}.");
    }

    [MenuItem("Tools/SceneModules/Sync ElectricField Colliders And Warning Areas")]
    public static void SyncElectricFieldCollidersAndWarningAreasCurrentScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        int colliders = SyncElectricFieldCollidersInScene(scene, force: true);
        SyncSummary warnings = SyncElectricFieldWarningAreasInScene(scene, force: true);
        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log(
            $"[LaserHazard] Full sync — colliders: {colliders}, warnings synced: {warnings.Synced}/{warnings.TotalFound}, " +
            $"missing warning: {warnings.MissingWarningArea}, missing collider: {warnings.MissingCollider}, " +
            $"missing visual: {warnings.MissingVisual}.");
    }

    [MenuItem("Tools/SceneModules/Sync ElectricField Colliders In All Level Scenes")]
    public static void SyncElectricFieldCollidersInAllLevelScenes()
    {
        string originalScene = SceneManager.GetActiveScene().path;
        int total = 0;

        for (int i = 0; i < LevelScenePaths.Length; i++)
        {
            string scenePath = LevelScenePaths[i];
            if (string.IsNullOrEmpty(scenePath) || !System.IO.File.Exists(scenePath))
            {
                continue;
            }

            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            total += SyncElectricFieldCollidersInScene(scene, force: true);
            EditorSceneManager.SaveScene(scene);
        }

        if (!string.IsNullOrEmpty(originalScene))
        {
            EditorSceneManager.OpenScene(originalScene, OpenSceneMode.Single);
        }

        Debug.Log($"[LaserHazard] Synced colliders for {total} electric field hazard(s) across level scenes.");
    }

    static int SyncElectricFieldCollidersInScene(Scene scene, bool force)
    {
        int count = 0;
        LaserHazard[] hazards = Object.FindObjectsByType<LaserHazard>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < hazards.Length; i++)
        {
            LaserHazard hazard = hazards[i];
            if (hazard == null || !hazard.gameObject.scene.Equals(scene))
            {
                continue;
            }

            if (!hazard.gameObject.name.Equals(SceneModuleVisualUtility.LogicRootName))
            {
                continue;
            }

            if (hazard.SyncColliderToVisualBounds(force))
            {
                count++;
                EditorUtility.SetDirty(hazard);
            }
        }

        return count;
    }

    struct SyncSummary
    {
        public int TotalFound;
        public int Synced;
        public int MissingWarningArea;
        public int MissingCollider;
        public int MissingVisual;
    }

    [MenuItem("Tools/SceneModules/Sync ElectricField Prefab Modules")]
    public static void SyncElectricFieldPrefabModulesCurrentScene()
    {
        ModuleSyncSummary summary = SyncElectricFieldPrefabModulesInScene(SceneManager.GetActiveScene());
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log(
            $"[LaserHazard] Prefab module sync — found: {summary.TotalFound}, synced: {summary.Synced}, " +
            $"prefab instances: {summary.PrefabInstances}, missing warning: {summary.MissingWarningArea}, " +
            $"missing collider: {summary.MissingCollider}, missing visual: {summary.MissingVisual}.");
    }

    [MenuItem("Tools/SceneModules/Replace All ElectricFields With Prefab Module")]
    public static void ReplaceAllElectricFieldsWithPrefabModuleCurrentScene()
    {
        ReplaceSummary summary = ReplaceAllElectricFieldsInScene(SceneManager.GetActiveScene());
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        LogReplaceSummary("current scene", summary);
    }

    [MenuItem("Tools/SceneModules/Replace All ElectricFields With Prefab Module (All Level Scenes)")]
    public static void ReplaceAllElectricFieldsWithPrefabModuleAllLevels()
    {
        string originalScene = SceneManager.GetActiveScene().path;
        ReplaceSummary total = default;

        for (int i = 0; i < LevelScenePaths.Length; i++)
        {
            if (!System.IO.File.Exists(LevelScenePaths[i]))
            {
                continue;
            }

            Scene scene = EditorSceneManager.OpenScene(LevelScenePaths[i], OpenSceneMode.Single);
            ReplaceSummary sceneSummary = ReplaceAllElectricFieldsInScene(scene);
            if (sceneSummary.Converted > 0
                || sceneSummary.NormalizedExisting > 0
                || sceneSummary.DuplicateWarningAreasRemoved > 0
                || sceneSummary.DuplicateDamageScriptsRemoved > 0)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            total.TotalFound += sceneSummary.TotalFound;
            total.Converted += sceneSummary.Converted;
            total.NormalizedExisting += sceneSummary.NormalizedExisting;
            total.SkippedNoPrefab += sceneSummary.SkippedNoPrefab;
            total.DuplicateWarningAreasRemoved += sceneSummary.DuplicateWarningAreasRemoved;
            total.DuplicateDamageScriptsRemoved += sceneSummary.DuplicateDamageScriptsRemoved;
            total.PrimitiveVisualsRemoved += sceneSummary.PrimitiveVisualsRemoved;
            Debug.Log($"[LaserHazard] {LevelScenePaths[i]} — {FormatReplaceSummary(sceneSummary)}");
        }

        if (!string.IsNullOrEmpty(originalScene) && System.IO.File.Exists(originalScene))
        {
            EditorSceneManager.OpenScene(originalScene, OpenSceneMode.Single);
        }

        LogReplaceSummary("all level scenes", total);
    }

    [MenuItem("Tools/SceneModules/Sync All ElectricField Prefab Modules")]
    public static void SyncAllElectricFieldPrefabModulesAllLevels()
    {
        string originalScene = SceneManager.GetActiveScene().path;
        ModuleSyncSummary total = default;

        for (int i = 0; i < LevelScenePaths.Length; i++)
        {
            if (!System.IO.File.Exists(LevelScenePaths[i]))
            {
                continue;
            }

            Scene scene = EditorSceneManager.OpenScene(LevelScenePaths[i], OpenSceneMode.Single);
            ModuleSyncSummary sceneSummary = SyncElectricFieldPrefabModulesInScene(scene);
            if (sceneSummary.Synced > 0)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            total.TotalFound += sceneSummary.TotalFound;
            total.Synced += sceneSummary.Synced;
            total.PrefabInstances += sceneSummary.PrefabInstances;
            total.MissingWarningArea += sceneSummary.MissingWarningArea;
            total.MissingCollider += sceneSummary.MissingCollider;
            total.MissingVisual += sceneSummary.MissingVisual;
        }

        if (!string.IsNullOrEmpty(originalScene) && System.IO.File.Exists(originalScene))
        {
            EditorSceneManager.OpenScene(originalScene, OpenSceneMode.Single);
        }

        Debug.Log(
            $"[LaserHazard] Sync all levels — found: {total.TotalFound}, synced: {total.Synced}, " +
            $"prefab instances: {total.PrefabInstances}, missing warning: {total.MissingWarningArea}, " +
            $"missing collider: {total.MissingCollider}, missing visual: {total.MissingVisual}.");
    }

    [MenuItem("Tools/SceneModules/Convert ElectricField Instances To Prefab Modules")]
    public static void ConvertElectricFieldInstancesToPrefabModulesCurrentScene()
    {
        ReplaceAllElectricFieldsWithPrefabModuleCurrentScene();
    }

    [MenuItem("Tools/SceneModules/Convert ElectricField Instances To Prefab Modules (All Level Scenes)")]
    public static void ConvertElectricFieldInstancesToPrefabModulesAllLevels()
    {
        ReplaceAllElectricFieldsWithPrefabModuleAllLevels();
    }

    struct ReplaceSummary
    {
        public int TotalFound;
        public int Converted;
        public int NormalizedExisting;
        public int SkippedNoPrefab;
        public int DuplicateWarningAreasRemoved;
        public int DuplicateDamageScriptsRemoved;
        public int PrimitiveVisualsRemoved;
    }

    static void LogReplaceSummary(string scope, ReplaceSummary summary)
    {
        Debug.Log($"[LaserHazard] Replace ElectricField prefab modules ({scope}) — {FormatReplaceSummary(summary)}");
    }

    static string FormatReplaceSummary(ReplaceSummary summary)
    {
        return
            $"found: {summary.TotalFound}, replaced: {summary.Converted}, normalized existing prefab: {summary.NormalizedExisting}, " +
            $"skipped: {summary.SkippedNoPrefab}, duplicate warnings removed: {summary.DuplicateWarningAreasRemoved}, " +
            $"duplicate damage scripts removed: {summary.DuplicateDamageScriptsRemoved}, primitive visuals removed: {summary.PrimitiveVisualsRemoved}";
    }

    struct ConvertSummary
    {
        public int TotalFound;
        public int Converted;
        public int NormalizedExisting;
        public int SkippedNoPrefab;
        public int DuplicateWarningAreasRemoved;
        public int DuplicateDamageScriptsRemoved;
        public int PrimitiveVisualsRemoved;
    }

    struct ModuleSyncSummary
    {
        public int TotalFound;
        public int Synced;
        public int PrefabInstances;
        public int MissingWarningArea;
        public int MissingCollider;
        public int MissingVisual;
    }

    static ConvertSummary ConvertElectricFieldInstancesInScene(Scene scene)
    {
        ReplaceSummary replaceSummary = ReplaceAllElectricFieldsInScene(scene);
        return new ConvertSummary
        {
            TotalFound = replaceSummary.TotalFound,
            Converted = replaceSummary.Converted,
            NormalizedExisting = replaceSummary.NormalizedExisting,
            SkippedNoPrefab = replaceSummary.SkippedNoPrefab,
            DuplicateWarningAreasRemoved = replaceSummary.DuplicateWarningAreasRemoved,
            DuplicateDamageScriptsRemoved = replaceSummary.DuplicateDamageScriptsRemoved,
            PrimitiveVisualsRemoved = replaceSummary.PrimitiveVisualsRemoved,
        };
    }

    static ReplaceSummary ReplaceAllElectricFieldsInScene(Scene scene)
    {
        ReplaceSummary summary = default;
        GameObject modulePrefab = null;
        if (!ElectricFieldHazardFactory.TryResolveModulePrefab(out modulePrefab))
        {
            Debug.LogWarning(
                "[LaserHazard] Module prefab not registered — run Tools/SceneModules/Integrate Hazard ElectricField Module first. " +
                "Expected: Assets/_Project/Prefabs/SceneModules/Hazards/PF_Generic_Hazard_ElectricField_01.prefab");
        }

        var processedRoots = new System.Collections.Generic.HashSet<int>();
        CollectElectricFieldRoots(scene, processedRoots);

        foreach (int rootId in processedRoots)
        {
            Transform hazardRoot = EditorInstanceIDToTransform(rootId);
            if (hazardRoot == null)
            {
                continue;
            }

            ProcessElectricFieldRoot(hazardRoot, modulePrefab, ref summary);
        }

        return summary;
    }

    static void CollectElectricFieldRoots(Scene scene, System.Collections.Generic.HashSet<int> processedRoots)
    {
        LaserHazard[] hazards = Object.FindObjectsByType<LaserHazard>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < hazards.Length; i++)
        {
            LaserHazard hazard = hazards[i];
            if (hazard == null || !hazard.gameObject.scene.Equals(scene))
            {
                continue;
            }

            Transform hazardRoot = GetHazardRoot(hazard);
            if (hazardRoot != null)
            {
                processedRoots.Add(hazardRoot.GetInstanceID());
            }
        }

        GameObject[] allRoots = scene.GetRootGameObjects();
        for (int i = 0; i < allRoots.Length; i++)
        {
            CollectElectricFieldRootsRecursive(allRoots[i].transform, scene, processedRoots);
        }
    }

    static void CollectElectricFieldRootsRecursive(
        Transform node,
        Scene scene,
        System.Collections.Generic.HashSet<int> processedRoots)
    {
        if (node == null || !node.gameObject.scene.Equals(scene))
        {
            return;
        }

        if (IsElectricFieldRootName(node.name) && node.Find(SceneModuleVisualUtility.LogicRootName) != null)
        {
            processedRoots.Add(node.GetInstanceID());
        }

        for (int i = 0; i < node.childCount; i++)
        {
            CollectElectricFieldRootsRecursive(node.GetChild(i), scene, processedRoots);
        }
    }

    static bool IsElectricFieldRootName(string objectName)
    {
        if (string.IsNullOrEmpty(objectName))
        {
            return false;
        }

        return objectName.StartsWith("Laser_", System.StringComparison.Ordinal)
            || objectName.IndexOf("ElectricField", System.StringComparison.OrdinalIgnoreCase) >= 0
            || objectName.IndexOf("ElectricGrid", System.StringComparison.OrdinalIgnoreCase) >= 0
            || objectName.IndexOf("LaserGrid", System.StringComparison.OrdinalIgnoreCase) >= 0;
    }

    static Transform EditorInstanceIDToTransform(int instanceId)
    {
        Object obj = EditorUtility.InstanceIDToObject(instanceId);
        if (obj is Transform transform)
        {
            return transform;
        }

        if (obj is GameObject gameObject)
        {
            return gameObject.transform;
        }

        return null;
    }

    static void ProcessElectricFieldRoot(Transform hazardRoot, GameObject modulePrefab, ref ReplaceSummary summary)
    {
        summary.TotalFound++;

        Transform logicTransform = hazardRoot.Find(SceneModuleVisualUtility.LogicRootName);
        LaserHazard logicHazard = logicTransform != null ? logicTransform.GetComponent<LaserHazard>() : null;
        if (logicHazard == null)
        {
            logicHazard = hazardRoot.GetComponent<LaserHazard>();
        }

        CleanupSummary cleanup = CleanupLegacyElectricFieldChildren(hazardRoot, logicTransform);
        summary.DuplicateWarningAreasRemoved += cleanup.DuplicateWarningAreasRemoved;
        summary.DuplicateDamageScriptsRemoved += cleanup.DuplicateDamageScriptsRemoved;
        summary.PrimitiveVisualsRemoved += cleanup.PrimitiveVisualsRemoved;

        if (IsModulePrefabInstance(hazardRoot.gameObject))
        {
            if (logicHazard != null)
            {
                NormalizeModuleInstance(logicHazard);
            }

            summary.NormalizedExisting++;
            return;
        }

        if (modulePrefab == null)
        {
            if (logicHazard != null)
            {
                NormalizeModuleInstance(logicHazard);
            }

            summary.SkippedNoPrefab++;
            return;
        }

        ConvertRootToPrefabModule(hazardRoot, logicHazard);
        summary.Converted++;
    }

    struct CleanupSummary
    {
        public int DuplicateWarningAreasRemoved;
        public int DuplicateDamageScriptsRemoved;
        public int PrimitiveVisualsRemoved;
    }

    static CleanupSummary CleanupLegacyElectricFieldChildren(Transform hazardRoot, Transform logicTransform)
    {
        CleanupSummary summary = default;
        if (hazardRoot == null)
        {
            return summary;
        }

        StripLegacyRootGameplayComponents(hazardRoot);

        LaserHazard rootHazard = hazardRoot.GetComponent<LaserHazard>();
        if (rootHazard != null && (logicTransform == null || rootHazard.gameObject != logicTransform.gameObject))
        {
            Object.DestroyImmediate(rootHazard);
            summary.DuplicateDamageScriptsRemoved++;
        }

        for (int i = hazardRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = hazardRoot.GetChild(i);
            if (child == null)
            {
                continue;
            }

            string childName = child.name;
            if (childName == SceneModuleVisualUtility.LogicRootName
                || childName == SceneModuleVisualUtility.VisualRootName
                || childName == HazardModuleLayoutUtility.WarningAreaName
                || childName == HazardModuleLayoutUtility.LabelAnchorName
                || childName == HazardModuleLayoutUtility.DebugBoundsName)
            {
                continue;
            }

            if (childName.IndexOf("Warning", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                Object.DestroyImmediate(child.gameObject);
                summary.DuplicateWarningAreasRemoved++;
                continue;
            }

            if (childName.StartsWith("Laser", System.StringComparison.Ordinal)
                || childName.IndexOf("Electric", System.StringComparison.OrdinalIgnoreCase) >= 0
                || child.GetComponent<Renderer>() != null)
            {
                Object.DestroyImmediate(child.gameObject);
                summary.PrimitiveVisualsRemoved++;
            }
        }

        Transform visualRoot = hazardRoot.Find(SceneModuleVisualUtility.VisualRootName);
        if (visualRoot != null)
        {
            for (int i = visualRoot.childCount - 1; i >= 0; i--)
            {
                Transform child = visualRoot.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                if (child.name == "LaserBeam_Fallback"
                    || child.name.IndexOf("ElectricPost", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Object.DestroyImmediate(child.gameObject);
                    summary.PrimitiveVisualsRemoved++;
                }
            }
        }

        Transform warningRoot = hazardRoot.Find(HazardModuleLayoutUtility.WarningAreaName);
        if (warningRoot != null && warningRoot.childCount > 1)
        {
            for (int i = warningRoot.childCount - 1; i >= 1; i--)
            {
                Object.DestroyImmediate(warningRoot.GetChild(i).gameObject);
                summary.DuplicateWarningAreasRemoved++;
            }
        }

        return summary;
    }

    static ModuleSyncSummary SyncElectricFieldPrefabModulesInScene(Scene scene)
    {
        ModuleSyncSummary summary = default;
        LaserHazard[] hazards = Object.FindObjectsByType<LaserHazard>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < hazards.Length; i++)
        {
            LaserHazard hazard = hazards[i];
            if (hazard == null || !hazard.gameObject.scene.Equals(scene))
            {
                continue;
            }

            if (!hazard.gameObject.name.Equals(SceneModuleVisualUtility.LogicRootName))
            {
                continue;
            }

            summary.TotalFound++;
            Transform hazardRoot = hazard.transform.parent;
            if (hazardRoot == null)
            {
                continue;
            }

            if (IsModulePrefabInstance(hazardRoot.gameObject))
            {
                summary.PrefabInstances++;
            }

            if (hazardRoot.Find(HazardModuleLayoutUtility.WarningAreaName) == null)
            {
                summary.MissingWarningArea++;
            }

            if (hazard.GetComponent<BoxCollider>() == null)
            {
                summary.MissingCollider++;
            }

            Transform visualRoot = hazardRoot.Find(SceneModuleVisualUtility.VisualRootName);
            if (visualRoot == null || visualRoot.GetComponentInChildren<Renderer>(true) == null)
            {
                summary.MissingVisual++;
            }

            hazard.SyncModuleLayout(forceCollider: true, forceWarning: true);
            summary.Synced++;
            EditorUtility.SetDirty(hazard);
        }

        return summary;
    }

    static Transform GetHazardRoot(LaserHazard hazard)
    {
        if (hazard.gameObject.name == SceneModuleVisualUtility.LogicRootName)
        {
            return hazard.transform.parent;
        }

        return hazard.transform;
    }

    static bool IsModulePrefabInstance(GameObject hazardRoot)
    {
        if (hazardRoot == null)
        {
            return false;
        }

        GameObject nearestPrefabRoot = PrefabUtility.GetNearestPrefabInstanceRoot(hazardRoot);
        if (nearestPrefabRoot == null)
        {
            return false;
        }

        Object source = PrefabUtility.GetCorrespondingObjectFromSource(nearestPrefabRoot);
        return source != null && source.name == "PF_Generic_Hazard_ElectricField_01";
    }

    static void NormalizeModuleInstance(LaserHazard logicHazard)
    {
        if (logicHazard == null)
        {
            return;
        }

        Transform hazardRoot = logicHazard.transform.parent;
        if (hazardRoot != null)
        {
            hazardRoot.localScale = Vector3.one;
            StripLegacyRootGameplayComponents(hazardRoot);
        }

        logicHazard.EnsureModuleLayout();
        logicHazard.SyncModuleLayout(forceCollider: true, forceWarning: true);
        EditorUtility.SetDirty(logicHazard);
    }

    static void ConvertRootToPrefabModule(Transform hazardRoot, LaserHazard logicHazard)
    {
        Transform parent = hazardRoot.parent;
        string name = hazardRoot.name;
        Vector3 position = hazardRoot.position;
        Quaternion rotation = hazardRoot.rotation;
        int siblingIndex = hazardRoot.GetSiblingIndex();
        Vector3 size = GetEffectiveFootprintSize(hazardRoot, logicHazard);
        float damage = logicHazard != null ? logicHazard.Damage : ElectricFieldHazardFactory.DefaultDamage;
        bool active = logicHazard == null || logicHazard.IsActive;

        Object.DestroyImmediate(hazardRoot.gameObject);

        LaserHazard created = ElectricFieldHazardFactory.CreateElectricField(
            parent,
            name,
            position,
            rotation,
            size,
            damage,
            active);
        if (created != null)
        {
            if (logicHazard != null)
            {
                created.CopySettingsFrom(logicHazard);
            }

            created.SyncModuleLayout(forceCollider: true, forceWarning: true);
            created.transform.SetSiblingIndex(siblingIndex);
            EditorUtility.SetDirty(created);
        }
    }

    static Vector3 GetEffectiveFootprintSize(Transform hazardRoot, LaserHazard logicHazard)
    {
        if (IsLegacyScaledRoot(hazardRoot))
        {
            Vector3 lossy = hazardRoot.lossyScale;
            return new Vector3(Mathf.Abs(lossy.x), 0.12f, Mathf.Abs(lossy.z));
        }

        if (logicHazard != null)
        {
            BoxCollider box = logicHazard.GetComponent<BoxCollider>();
            if (box != null)
            {
                return box.size;
            }
        }

        return new Vector3(SceneModuleVisualUtility.ElectricFieldVisualWidth, 0.12f, 0.35f);
    }

    static bool IsLegacyScaledRoot(Transform root)
    {
        Vector3 local = root.localScale;
        return Mathf.Abs(local.y - 0.12f) < 0.05f
            && (Mathf.Abs(local.x - 1f) > 0.05f || Mathf.Abs(local.z - 1f) > 0.05f);
    }

    static void StripLegacyRootGameplayComponents(Transform hazardRoot)
    {
        if (hazardRoot == null)
        {
            return;
        }

        LaserHazard rootHazard = hazardRoot.GetComponent<LaserHazard>();
        if (rootHazard != null && rootHazard.gameObject.name != SceneModuleVisualUtility.LogicRootName)
        {
            Object.DestroyImmediate(rootHazard);
        }

        BoxCollider rootBox = hazardRoot.GetComponent<BoxCollider>();
        if (rootBox != null)
        {
            Object.DestroyImmediate(rootBox);
        }

        MeshRenderer rootRenderer = hazardRoot.GetComponent<MeshRenderer>();
        if (rootRenderer != null)
        {
            Object.DestroyImmediate(rootRenderer);
        }

        MeshFilter rootFilter = hazardRoot.GetComponent<MeshFilter>();
        if (rootFilter != null)
        {
            Object.DestroyImmediate(rootFilter);
        }
    }

    static SyncSummary SyncElectricFieldWarningAreasInScene(Scene scene, bool force)
    {
        SyncSummary summary = default;
        LaserHazard[] hazards = Object.FindObjectsByType<LaserHazard>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < hazards.Length; i++)
        {
            LaserHazard hazard = hazards[i];
            if (hazard == null || !hazard.gameObject.scene.Equals(scene))
            {
                continue;
            }

            if (!hazard.gameObject.name.Equals(SceneModuleVisualUtility.LogicRootName))
            {
                continue;
            }

            summary.TotalFound++;
            Transform hazardRoot = hazard.transform.parent;
            if (hazardRoot == null)
            {
                continue;
            }

            if (hazardRoot.Find(HazardModuleLayoutUtility.WarningAreaName) == null)
            {
                summary.MissingWarningArea++;
            }

            if (hazard.GetComponent<BoxCollider>() == null)
            {
                summary.MissingCollider++;
            }

            Transform visualRoot = hazardRoot.Find(SceneModuleVisualUtility.VisualRootName);
            if (visualRoot == null || visualRoot.GetComponentInChildren<Renderer>(true) == null)
            {
                summary.MissingVisual++;
            }

            if (hazard.SyncWarningAreaToColliderOrVisual(force))
            {
                summary.Synced++;
                EditorUtility.SetDirty(hazard);
            }
        }

        return summary;
    }
}
#endif
