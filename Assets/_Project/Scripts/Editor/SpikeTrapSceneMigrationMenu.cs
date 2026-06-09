#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Normalizes baked legacy SpikeTrap objects and replaces them with the complete spike hazard prefab module.</summary>
public static class SpikeTrapSceneMigrationMenu
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

    [MenuItem("Tools/SceneModules/Normalize Existing Trap Visuals")]
    public static void NormalizeExistingTrapVisualsCurrentScene()
    {
        MigrateCurrentScene();
    }

    [MenuItem("Tools/SceneModules/Migrate Spike Traps In Current Scene")]
    public static void MigrateCurrentScene()
    {
        int count = MigrateSpikeTrapsInScene(SceneManager.GetActiveScene());
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log($"[SpikeTrap] Normalized {count} spike trap(s) in current scene.");
    }

    [MenuItem("Tools/SceneModules/Migrate Spike Traps In All Level Scenes")]
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
            int count = MigrateSpikeTrapsInScene(scene);
            if (count > 0)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            total += count;
            Debug.Log($"[SpikeTrap] Normalized {count} spike trap(s) in {LevelScenePaths[i]}.");
        }

        if (!string.IsNullOrEmpty(originalScene) && System.IO.File.Exists(originalScene))
        {
            EditorSceneManager.OpenScene(originalScene, OpenSceneMode.Single);
        }

        Debug.Log($"[SpikeTrap] Normalization complete. Total spike traps processed: {total}.");
    }

    [MenuItem("Tools/SceneModules/Register Spike Hazard Prefab")]
    public static void RegisterSpikeHazardPrefab()
    {
        SceneModuleHazardSpikeTrapIntegrationMenu.CreateSpikeHazardTestPrefabAndRegistry();
    }

    [MenuItem("Tools/SceneModules/Replace All Spike Hazards With Prefab Module")]
    public static void ReplaceAllSpikeHazardsWithPrefabModuleCurrentScene()
    {
        ReplaceSummary summary = ReplaceAllSpikeHazardsInScene(SceneManager.GetActiveScene());
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        LogReplaceSummary("current scene", summary);
    }

    [MenuItem("Tools/SceneModules/Replace All Spike Hazards With Prefab Module (All Level Scenes)")]
    public static void ReplaceAllSpikeHazardsWithPrefabModuleAllLevels()
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
            ReplaceSummary sceneSummary = ReplaceAllSpikeHazardsInScene(scene);
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
            Debug.Log($"[SpikeTrap] {LevelScenePaths[i]} — {FormatReplaceSummary(sceneSummary)}");
        }

        if (!string.IsNullOrEmpty(originalScene) && System.IO.File.Exists(originalScene))
        {
            EditorSceneManager.OpenScene(originalScene, OpenSceneMode.Single);
        }

        LogReplaceSummary("all level scenes", total);
    }

    [MenuItem("Tools/SceneModules/Sync All Spike Hazard Prefab Modules")]
    public static void SyncAllSpikeHazardPrefabModulesCurrentScene()
    {
        ModuleSyncSummary summary = SyncSpikeHazardPrefabModulesInScene(SceneManager.GetActiveScene());
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log(
            $"[SpikeTrap] Prefab module sync — found: {summary.TotalFound}, synced: {summary.Synced}, " +
            $"prefab instances: {summary.PrefabInstances}, missing warning: {summary.MissingWarningArea}, " +
            $"missing collider: {summary.MissingCollider}, missing visual: {summary.MissingVisual}.");
    }

    [MenuItem("Tools/SceneModules/Sync All Spike Hazard Prefab Modules (All Level Scenes)")]
    public static void SyncAllSpikeHazardPrefabModulesAllLevels()
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
            ModuleSyncSummary sceneSummary = SyncSpikeHazardPrefabModulesInScene(scene);
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
            $"[SpikeTrap] Sync all levels — found: {total.TotalFound}, synced: {total.Synced}, " +
            $"prefab instances: {total.PrefabInstances}, missing warning: {total.MissingWarningArea}, " +
            $"missing collider: {total.MissingCollider}, missing visual: {total.MissingVisual}.");
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

    struct ModuleSyncSummary
    {
        public int TotalFound;
        public int Synced;
        public int PrefabInstances;
        public int MissingWarningArea;
        public int MissingCollider;
        public int MissingVisual;
    }

    struct CleanupSummary
    {
        public int DuplicateWarningAreasRemoved;
        public int DuplicateDamageScriptsRemoved;
        public int PrimitiveVisualsRemoved;
    }

    static void LogReplaceSummary(string scope, ReplaceSummary summary)
    {
        Debug.Log($"[SpikeTrap] Replace spike prefab modules ({scope}) — {FormatReplaceSummary(summary)}");
    }

    static string FormatReplaceSummary(ReplaceSummary summary)
    {
        return
            $"found: {summary.TotalFound}, replaced: {summary.Converted}, normalized existing prefab: {summary.NormalizedExisting}, " +
            $"skipped: {summary.SkippedNoPrefab}, duplicate warnings removed: {summary.DuplicateWarningAreasRemoved}, " +
            $"duplicate damage scripts removed: {summary.DuplicateDamageScriptsRemoved}, primitive visuals removed: {summary.PrimitiveVisualsRemoved}";
    }

    static ReplaceSummary ReplaceAllSpikeHazardsInScene(Scene scene)
    {
        ReplaceSummary summary = default;
        GameObject modulePrefab = null;
        if (!SpikeTrapHazardFactory.TryResolveModulePrefab(out modulePrefab))
        {
            Debug.LogWarning(
                "[SpikeTrap] Module prefab not registered — run Tools/SceneModules/Integrate Hazard SpikeTrap Module first. " +
                "Expected: Assets/_Project/Prefabs/SceneModules/Hazards/PF_Generic_Hazard_SpikeTrap_01.prefab");
        }

        var processedRoots = new System.Collections.Generic.HashSet<int>();
        SpikeTrap[] traps = Object.FindObjectsByType<SpikeTrap>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < traps.Length; i++)
        {
            SpikeTrap trap = traps[i];
            if (trap == null || !trap.gameObject.scene.Equals(scene))
            {
                continue;
            }

            Transform hazardRoot = GetHazardRoot(trap);
            if (hazardRoot != null)
            {
                processedRoots.Add(hazardRoot.GetInstanceID());
            }
        }

        foreach (int rootId in processedRoots)
        {
            Transform hazardRoot = EditorInstanceIDToTransform(rootId);
            if (hazardRoot == null)
            {
                continue;
            }

            ProcessSpikeHazardRoot(hazardRoot, modulePrefab, ref summary);
        }

        return summary;
    }

    static void ProcessSpikeHazardRoot(Transform hazardRoot, GameObject modulePrefab, ref ReplaceSummary summary)
    {
        summary.TotalFound++;

        Transform logicTransform = hazardRoot.Find(SceneModuleVisualUtility.LogicRootName);
        SpikeTrap logicTrap = logicTransform != null ? logicTransform.GetComponent<SpikeTrap>() : null;
        if (logicTrap == null)
        {
            logicTrap = hazardRoot.GetComponent<SpikeTrap>();
        }

        CleanupSummary cleanup = CleanupLegacySpikeChildren(hazardRoot, logicTransform);
        summary.DuplicateWarningAreasRemoved += cleanup.DuplicateWarningAreasRemoved;
        summary.DuplicateDamageScriptsRemoved += cleanup.DuplicateDamageScriptsRemoved;
        summary.PrimitiveVisualsRemoved += cleanup.PrimitiveVisualsRemoved;

        if (IsModulePrefabInstance(hazardRoot.gameObject))
        {
            if (logicTrap != null)
            {
                NormalizeModuleInstance(logicTrap);
            }

            summary.NormalizedExisting++;
            return;
        }

        if (modulePrefab == null)
        {
            if (logicTrap != null)
            {
                NormalizeModuleInstance(logicTrap);
            }

            summary.SkippedNoPrefab++;
            return;
        }

        ConvertRootToPrefabModule(hazardRoot, logicTrap);
        summary.Converted++;
    }

    static CleanupSummary CleanupLegacySpikeChildren(Transform hazardRoot, Transform logicTransform)
    {
        CleanupSummary summary = default;
        if (hazardRoot == null)
        {
            return summary;
        }

        StripLegacyRootGameplayComponents(hazardRoot);

        SpikeTrap rootTrap = hazardRoot.GetComponent<SpikeTrap>();
        if (rootTrap != null && (logicTransform == null || rootTrap.gameObject != logicTransform.gameObject))
        {
            Object.DestroyImmediate(rootTrap);
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

            if (childName.StartsWith("Spike_", System.StringComparison.Ordinal)
                || childName == "Base"
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

                if (child.name.StartsWith("Spike_", System.StringComparison.Ordinal) || child.name == "Base")
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

    static ModuleSyncSummary SyncSpikeHazardPrefabModulesInScene(Scene scene)
    {
        ModuleSyncSummary summary = default;
        SpikeTrap[] traps = Object.FindObjectsByType<SpikeTrap>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < traps.Length; i++)
        {
            SpikeTrap trap = traps[i];
            if (trap == null || !trap.gameObject.scene.Equals(scene))
            {
                continue;
            }

            if (!trap.gameObject.name.Equals(SceneModuleVisualUtility.LogicRootName))
            {
                continue;
            }

            summary.TotalFound++;
            Transform hazardRoot = trap.transform.parent;
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

            if (trap.GetComponent<BoxCollider>() == null)
            {
                summary.MissingCollider++;
            }

            Transform visualRoot = hazardRoot.Find(SceneModuleVisualUtility.VisualRootName);
            if (visualRoot == null || visualRoot.GetComponentInChildren<Renderer>(true) == null)
            {
                summary.MissingVisual++;
            }

            trap.SyncModuleLayout(forceCollider: true, forceWarning: true);
            summary.Synced++;
            EditorUtility.SetDirty(trap);
        }

        return summary;
    }

    static Transform GetHazardRoot(SpikeTrap trap)
    {
        if (trap.gameObject.name == SceneModuleVisualUtility.LogicRootName)
        {
            return trap.transform.parent;
        }

        return trap.transform;
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
        return source != null && source.name == "PF_Generic_Hazard_SpikeTrap_01";
    }

    static void NormalizeModuleInstance(SpikeTrap logicTrap)
    {
        if (logicTrap == null)
        {
            return;
        }

        Transform hazardRoot = logicTrap.transform.parent;
        if (hazardRoot != null)
        {
            hazardRoot.localScale = Vector3.one;
            StripLegacyRootGameplayComponents(hazardRoot);
        }

        logicTrap.EnsureModuleLayout();
        logicTrap.SyncModuleLayout(forceCollider: true, forceWarning: true);
        EditorUtility.SetDirty(logicTrap);
    }

    static void ConvertRootToPrefabModule(Transform hazardRoot, SpikeTrap logicTrap)
    {
        Transform parent = hazardRoot.parent;
        string name = hazardRoot.name;
        Vector3 position = hazardRoot.position;
        Quaternion rotation = hazardRoot.rotation;
        int siblingIndex = hazardRoot.GetSiblingIndex();
        Vector3 size = GetEffectiveFootprintSize(hazardRoot, logicTrap);
        float damage = logicTrap != null ? logicTrap.Damage : SpikeTrapHazardFactory.DefaultDamage;
        bool active = logicTrap == null || logicTrap.IsActive;
        int spikeCount = logicTrap != null
            ? GetFallbackSpikeCount(logicTrap)
            : SpikeTrapHazardFactory.DefaultSpikeCount;

        Object.DestroyImmediate(hazardRoot.gameObject);

        SpikeTrap created = SpikeTrapHazardFactory.CreateSpikeHazard(
            parent,
            name,
            position,
            rotation,
            size,
            spikeCount,
            damage,
            active);
        if (created != null)
        {
            if (logicTrap != null)
            {
                created.CopySettingsFrom(logicTrap);
            }

            created.SyncModuleLayout(forceCollider: true, forceWarning: true);
            created.transform.SetSiblingIndex(siblingIndex);
            EditorUtility.SetDirty(created);
        }
    }

    static int GetFallbackSpikeCount(SpikeTrap trap)
    {
        SerializedObject serialized = new SerializedObject(trap);
        SerializedProperty prop = serialized.FindProperty("fallbackSpikeCount");
        return prop != null ? prop.intValue : SpikeTrapHazardFactory.DefaultSpikeCount;
    }

    static Vector3 GetEffectiveFootprintSize(Transform hazardRoot, SpikeTrap logicTrap)
    {
        if (logicTrap != null)
        {
            BoxCollider box = logicTrap.GetComponent<BoxCollider>();
            if (box != null)
            {
                return new Vector3(box.size.x, 0.4f, box.size.z);
            }
        }

        return new Vector3(SceneModuleVisualUtility.HazardSpikeVisualDiameter, 0.4f, SceneModuleVisualUtility.HazardSpikeVisualDiameter);
    }

    static void StripLegacyRootGameplayComponents(Transform hazardRoot)
    {
        if (hazardRoot == null)
        {
            return;
        }

        SpikeTrap rootTrap = hazardRoot.GetComponent<SpikeTrap>();
        if (rootTrap != null && rootTrap.gameObject.name != SceneModuleVisualUtility.LogicRootName)
        {
            Object.DestroyImmediate(rootTrap);
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

    static int MigrateSpikeTrapsInScene(Scene scene)
    {
        var traps = Object.FindObjectsByType<SpikeTrap>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int migrated = 0;

        for (int i = 0; i < traps.Length; i++)
        {
            SpikeTrap trap = traps[i];
            if (trap == null || !trap.gameObject.scene.Equals(scene))
            {
                continue;
            }

            if (trap.gameObject.name == SceneModuleVisualUtility.LogicRootName)
            {
                trap.EnsureModuleLayout();
                EditorUtility.SetDirty(trap);
            }
            else if (trap.transform.Find(SceneModuleVisualUtility.LogicRootName) == null)
            {
                MigrateLegacyTrapInEditor(trap);
            }
            else
            {
                MigrateDuplicateRootTrapInEditor(trap);
            }

            migrated++;
        }

        return migrated;
    }

    static void MigrateLegacyTrapInEditor(SpikeTrap trap)
    {
        Transform trapRoot = trap.transform;
        Transform logicRoot = trapRoot.Find(SceneModuleVisualUtility.LogicRootName);
        if (logicRoot == null)
        {
            logicRoot = new GameObject(SceneModuleVisualUtility.LogicRootName).transform;
            logicRoot.SetParent(trapRoot, false);
            logicRoot.localPosition = Vector3.zero;
            logicRoot.localRotation = Quaternion.identity;
            logicRoot.localScale = Vector3.one;
        }

        Transform visualRoot = trapRoot.Find(SceneModuleVisualUtility.VisualRootName);
        if (visualRoot == null)
        {
            visualRoot = new GameObject(SceneModuleVisualUtility.VisualRootName).transform;
            visualRoot.SetParent(trapRoot, false);
            visualRoot.localPosition = Vector3.zero;
            visualRoot.localRotation = Quaternion.identity;
            visualRoot.localScale = Vector3.one;
        }

        HazardModuleLayoutUtility.EnsureChild(trapRoot, HazardModuleLayoutUtility.WarningAreaName);
        HazardModuleLayoutUtility.EnsureLabelAnchor(trapRoot, 0.8f);
        HazardModuleLayoutUtility.EnsureChild(trapRoot, HazardModuleLayoutUtility.DebugBoundsName);

        var sourceBox = trap.GetComponent<BoxCollider>();
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

        SpikeTrap logicTrap = logicRoot.GetComponent<SpikeTrap>();
        if (logicTrap == null)
        {
            logicTrap = logicRoot.gameObject.AddComponent<SpikeTrap>();
        }

        logicTrap.CopySettingsFrom(trap);

        Object.DestroyImmediate(trap);
        logicTrap.EnsureModuleLayout();
        logicTrap.SyncModuleLayout(forceCollider: true, forceWarning: true);
        EditorUtility.SetDirty(logicTrap);
    }

    static void MigrateDuplicateRootTrapInEditor(SpikeTrap trap)
    {
        Transform trapRoot = trap.transform;
        Transform logicRoot = trapRoot.Find(SceneModuleVisualUtility.LogicRootName);
        if (logicRoot == null)
        {
            MigrateLegacyTrapInEditor(trap);
            return;
        }

        SpikeTrap logicTrap = logicRoot.GetComponent<SpikeTrap>();
        if (logicTrap == null)
        {
            logicTrap = logicRoot.gameObject.AddComponent<SpikeTrap>();
            logicTrap.CopySettingsFrom(trap);
        }

        var sourceBox = trap.GetComponent<BoxCollider>();
        if (sourceBox != null)
        {
            Object.DestroyImmediate(sourceBox);
        }

        Object.DestroyImmediate(trap);
        logicTrap.EnsureModuleLayout();
        logicTrap.SyncModuleLayout(forceCollider: true, forceWarning: true);
        EditorUtility.SetDirty(logicTrap);
    }
}
#endif
