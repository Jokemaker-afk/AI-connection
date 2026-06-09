using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(BoxCollider))]
public class SpikeTrap : MonoBehaviour
{
    // Full hazard module: LogicRoot gameplay, VisualRoot appearance only.
    const int DefaultFallbackSpikeCount = 4;
    const float DefaultColliderHeight = 0.55f;
    const float DefaultColliderCenterY = 0.25f;

    [SerializeField] float damage = 18f;
    [SerializeField] float hazardCooldown = 0.8f;
    [SerializeField] float retriggerDelay = 0.5f;
    [SerializeField] int fallbackSpikeCount = DefaultFallbackSpikeCount;
    [SerializeField] bool isActive = true;
    [SerializeField] bool moduleVisualApplied;

    [Header("Collider Sync")]
    [SerializeField] bool autoSyncColliderToVisual = true;
    [SerializeField] Vector3 colliderPadding = new Vector3(0.1f, 0.2f, 0.1f);
    [SerializeField] bool syncOnAwake = true;
    [SerializeField] bool syncOnValidate = true;
    [SerializeField] bool syncDebugBounds = true;

    [Header("Warning Area Sync")]
    [FormerlySerializedAs("syncWarningArea")]
    [SerializeField] bool autoSyncWarningArea = true;
    [SerializeField] float warningAreaPadding = 0.1f;
    [SerializeField] float warningAreaGroundOffset = 0.025f;
    [SerializeField] bool warningAreaUseColliderBounds = true;
    [SerializeField] bool swapWarningAreaAxes;
    [SerializeField] Vector3 warningAreaLocalAxisScaleMultiplier = Vector3.one;

    float nextDamageTime;
    bool armed = true;

    public float Damage => damage;
    public bool IsActive => isActive;

    void Awake()
    {
        if (TryMigrateLegacyRoot())
        {
            return;
        }

        EnsureModuleLayout();
        if (syncOnAwake)
        {
            SyncColliderToVisualBounds();
            SyncWarningAreaToColliderOrVisual();
            SyncDebugBoundsVisual();
        }

        ConfigureCollider();
        ApplyActiveVisualState();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!syncOnValidate || !IsLogicRoot())
        {
            return;
        }

        if (!autoSyncColliderToVisual && !autoSyncWarningArea)
        {
            return;
        }

        UnityEditor.EditorApplication.delayCall += HandleDelayedValidateSync;
    }

    void HandleDelayedValidateSync()
    {
        if (this == null)
        {
            return;
        }

        if (autoSyncColliderToVisual)
        {
            SyncColliderToVisualBounds();
        }

        if (autoSyncWarningArea)
        {
            SyncWarningAreaToColliderOrVisual();
        }

        SyncDebugBoundsVisual();
    }
#endif

    void ConfigureCollider()
    {
        var box = GetComponent<BoxCollider>();
        if (box == null)
        {
            return;
        }

        box.isTrigger = true;
        box.enabled = isActive;
    }

    void OnTriggerStay(Collider other)
    {
        if (!isActive || !armed || Time.time < nextDamageTime)
        {
            return;
        }

        if (!other.TryGetComponent<PlayerStats>(out var stats))
        {
            return;
        }

        if (!stats.TryTakeDamage(damage))
        {
            return;
        }

        armed = false;
        nextDamageTime = Time.time + hazardCooldown + retriggerDelay;
        Invoke(nameof(Rearm), hazardCooldown + retriggerDelay);
    }

    void Rearm()
    {
        armed = true;
    }

    public void SetActiveState(bool active)
    {
        isActive = active;
        var box = GetComponent<BoxCollider>();
        if (box != null)
        {
            box.enabled = active;
        }

        ApplyActiveVisualState();
    }

    void ApplyActiveVisualState()
    {
        if (!IsLogicRoot())
        {
            return;
        }

        Transform hazardRoot = transform.parent;
        if (hazardRoot == null)
        {
            return;
        }

        Transform visualRoot = hazardRoot.Find(SceneModuleVisualUtility.VisualRootName);
        Transform warningRoot = hazardRoot.Find(HazardModuleLayoutUtility.WarningAreaName);
        HazardModuleLayoutUtility.SetRenderersActive(visualRoot, isActive);
        HazardModuleLayoutUtility.SetRenderersActive(warningRoot, isActive, inactiveDim: 0.35f);
    }

    public void EnsureModuleLayout()
    {
        if (!IsLogicRoot())
        {
            return;
        }

        Transform hazardRoot = transform.parent;
        if (hazardRoot == null)
        {
            return;
        }

        Transform visualRoot = EnsureVisualRoot(hazardRoot);
        HazardModuleLayoutUtility.EnsureChild(hazardRoot, HazardModuleLayoutUtility.WarningAreaName);
        HazardModuleLayoutUtility.EnsureLabelAnchor(hazardRoot, 0.8f);

        Vector3 footprint = GetFootprintSize();
        if (!moduleVisualApplied || NeedsVisualRebuild(visualRoot))
        {
            RebuildVisual(hazardRoot, transform, visualRoot, footprint);
        }

        if (autoSyncColliderToVisual)
        {
            SyncColliderToVisualBounds();
        }
        else if (autoSyncWarningArea)
        {
            SyncWarningAreaToColliderOrVisual();
            SyncDebugBoundsVisual();
        }
    }

    Color GetWarningTint()
    {
        return new Color(0.82f, 0.22f, 0.08f, isActive ? 0.22f : 0.08f);
    }

    public bool SyncColliderToVisualBounds(bool force = false)
    {
        if (!force && !autoSyncColliderToVisual)
        {
            return false;
        }

        if (!IsLogicRoot())
        {
            return false;
        }

        Transform hazardRoot = transform.parent;
        if (hazardRoot == null)
        {
            return false;
        }

        Transform visualRoot = hazardRoot.Find(SceneModuleVisualUtility.VisualRootName);
        if (visualRoot == null)
        {
            return false;
        }

        StripVisualGameplayColliders(visualRoot);

        if (!HazardModuleLayoutUtility.TryCalculateLocalBoundsFromRenderers(visualRoot, transform, out Bounds localBounds))
        {
            return false;
        }

        Vector3 paddedSize = localBounds.size + colliderPadding;
        paddedSize.x = Mathf.Max(0.05f, paddedSize.x);
        paddedSize.y = Mathf.Max(0.05f, paddedSize.y);
        paddedSize.z = Mathf.Max(0.05f, paddedSize.z);

        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null)
        {
            box = gameObject.AddComponent<BoxCollider>();
        }

        box.isTrigger = true;
        box.center = localBounds.center;
        box.size = paddedSize;
        box.enabled = isActive;

        if (autoSyncWarningArea)
        {
            SyncWarningAreaToColliderOrVisual();
        }

        SyncDebugBoundsVisual();
        return true;
    }

    public bool SyncWarningAreaToColliderOrVisual(bool force = false)
    {
        if (!force && !autoSyncWarningArea)
        {
            return false;
        }

        if (!IsLogicRoot())
        {
            return false;
        }

        Transform hazardRoot = transform.parent;
        if (hazardRoot == null)
        {
            return false;
        }

        Transform visualRoot = hazardRoot.Find(SceneModuleVisualUtility.VisualRootName);
        Transform warningRoot = HazardModuleLayoutUtility.EnsureChild(hazardRoot, HazardModuleLayoutUtility.WarningAreaName);
        warningRoot.localRotation = Quaternion.identity;
        warningRoot.localScale = Vector3.one;

        Vector3 centerInHazardLocal;
        float footprintX;
        float footprintZ;
        bool resolved = false;

        BoxCollider box = GetComponent<BoxCollider>();
        if (warningAreaUseColliderBounds && box != null
            && HazardModuleLayoutUtility.TryCalculateGroundFootprintFromBox(
                box,
                hazardRoot,
                out centerInHazardLocal,
                out footprintX,
                out footprintZ))
        {
            resolved = true;
        }
        else if (visualRoot != null
            && HazardModuleLayoutUtility.TryCalculateGroundFootprintFromRenderers(
                visualRoot,
                hazardRoot,
                out centerInHazardLocal,
                out footprintX,
                out footprintZ))
        {
            resolved = true;
        }
        else if (box != null)
        {
            Vector3 fallbackSize = GetColliderSizeInHazardLocal(hazardRoot, box.size, box.center);
            centerInHazardLocal = GetColliderCenterInHazardLocal(hazardRoot);
            footprintX = fallbackSize.x;
            footprintZ = fallbackSize.z;
            resolved = footprintX > 0.0001f || footprintZ > 0.0001f;
        }
        else
        {
            return false;
        }

        if (!resolved)
        {
            return false;
        }

        if (swapWarningAreaAxes)
        {
            float swap = footprintX;
            footprintX = footprintZ;
            footprintZ = swap;
        }

        footprintX = Mathf.Max(0.05f, footprintX * warningAreaLocalAxisScaleMultiplier.x);
        footprintZ = Mathf.Max(0.05f, footprintZ * warningAreaLocalAxisScaleMultiplier.z);

        Vector3 hazardLossy = hazardRoot.lossyScale;
        float localPaddingX = (warningAreaPadding * 2f) / Mathf.Max(0.001f, Mathf.Abs(hazardLossy.x));
        float localPaddingZ = (warningAreaPadding * 2f) / Mathf.Max(0.001f, Mathf.Abs(hazardLossy.z));

        HazardModuleLayoutUtility.BuildWarningAreaPlane(
            warningRoot,
            centerInHazardLocal,
            footprintX + localPaddingX,
            footprintZ + localPaddingZ,
            warningAreaGroundOffset,
            0f,
            GetWarningTint());

        return true;
    }

    void SyncDebugBoundsVisual()
    {
#if UNITY_EDITOR
        if (!syncDebugBounds || !IsLogicRoot())
        {
            return;
        }

        Transform hazardRoot = transform.parent;
        if (hazardRoot == null)
        {
            return;
        }

        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null)
        {
            return;
        }

        Vector3 centerInHazardLocal = GetColliderCenterInHazardLocal(hazardRoot);
        Vector3 sizeInHazardLocal = GetColliderSizeInHazardLocal(hazardRoot, box.size, box.center);
        HazardModuleLayoutUtility.SyncDebugBounds(hazardRoot, centerInHazardLocal, sizeInHazardLocal, visible: false);
#endif
    }

    Vector3 GetColliderCenterInHazardLocal(Transform hazardRoot)
    {
        var box = GetComponent<BoxCollider>();
        if (box == null || hazardRoot == null)
        {
            return Vector3.zero;
        }

        return hazardRoot.InverseTransformPoint(transform.TransformPoint(box.center));
    }

    Vector3 GetColliderSizeInHazardLocal(Transform hazardRoot, Vector3 logicLocalSize, Vector3 logicLocalCenter)
    {
        if (hazardRoot == null)
        {
            return logicLocalSize;
        }

        Vector3 half = logicLocalSize * 0.5f;
        Vector3 min = hazardRoot.InverseTransformPoint(transform.TransformPoint(logicLocalCenter + new Vector3(-half.x, -half.y, -half.z)));
        Vector3 max = min;
        AppendHazardLocalCorner(hazardRoot, logicLocalCenter, ref min, ref max, half.x, half.y, half.z);
        AppendHazardLocalCorner(hazardRoot, logicLocalCenter, ref min, ref max, half.x, half.y, -half.z);
        AppendHazardLocalCorner(hazardRoot, logicLocalCenter, ref min, ref max, half.x, -half.y, half.z);
        AppendHazardLocalCorner(hazardRoot, logicLocalCenter, ref min, ref max, half.x, -half.y, -half.z);
        AppendHazardLocalCorner(hazardRoot, logicLocalCenter, ref min, ref max, -half.x, half.y, half.z);
        AppendHazardLocalCorner(hazardRoot, logicLocalCenter, ref min, ref max, -half.x, half.y, -half.z);
        AppendHazardLocalCorner(hazardRoot, logicLocalCenter, ref min, ref max, -half.x, -half.y, half.z);
        AppendHazardLocalCorner(hazardRoot, logicLocalCenter, ref min, ref max, -half.x, -half.y, -half.z);
        return max - min;
    }

    void AppendHazardLocalCorner(
        Transform hazardRoot,
        Vector3 logicLocalCenter,
        ref Vector3 min,
        ref Vector3 max,
        float x,
        float y,
        float z)
    {
        Vector3 hazardLocal = hazardRoot.InverseTransformPoint(transform.TransformPoint(logicLocalCenter + new Vector3(x, y, z)));
        min = Vector3.Min(min, hazardLocal);
        max = Vector3.Max(max, hazardLocal);
    }

    static void StripVisualGameplayColliders(Transform visualRoot)
    {
        if (visualRoot == null)
        {
            return;
        }

        SceneModuleVisualUtility.StripGameplayFromVisualHierarchy(visualRoot.gameObject);
    }

    public void CopySettingsFrom(SpikeTrap source)
    {
        if (source == null)
        {
            return;
        }

        damage = source.damage;
        hazardCooldown = source.hazardCooldown;
        retriggerDelay = source.retriggerDelay;
        fallbackSpikeCount = source.fallbackSpikeCount;
        isActive = source.isActive;
        autoSyncColliderToVisual = source.autoSyncColliderToVisual;
        colliderPadding = source.colliderPadding;
        syncOnAwake = source.syncOnAwake;
        syncOnValidate = source.syncOnValidate;
        syncDebugBounds = source.syncDebugBounds;
        autoSyncWarningArea = source.autoSyncWarningArea;
        warningAreaPadding = source.warningAreaPadding;
        warningAreaGroundOffset = source.warningAreaGroundOffset;
        warningAreaUseColliderBounds = source.warningAreaUseColliderBounds;
        swapWarningAreaAxes = source.swapWarningAreaAxes;
        warningAreaLocalAxisScaleMultiplier = source.warningAreaLocalAxisScaleMultiplier;
        moduleVisualApplied = false;
    }

    public static SpikeTrap Create(
        Transform parent,
        string name,
        Vector3 position,
        Vector3 baseSize,
        int spikeCount = 3,
        float damageAmount = 18f,
        bool active = true)
    {
        return Create(parent, name, position, Quaternion.identity, baseSize, spikeCount, damageAmount, active);
    }

    public static SpikeTrap Create(
        Transform parent,
        string name,
        Vector3 position,
        Quaternion rotation,
        Vector3 baseSize,
        int spikeCount = 3,
        float damageAmount = 18f,
        bool active = true)
    {
        if (TryInstantiateRegisteredModule(
                parent,
                name,
                position,
                rotation,
                baseSize,
                spikeCount,
                damageAmount,
                active,
                out SpikeTrap moduleInstance))
        {
            return moduleInstance;
        }

        return CreateFallbackModule(parent, name, position, rotation, baseSize, spikeCount, damageAmount, active);
    }

    public void Configure(Vector3 footprintSize, float damageAmount, bool active)
    {
        damage = damageAmount;
        isActive = active;
        Transform hazardRoot = transform.parent;
        if (hazardRoot == null || !IsLogicRoot())
        {
            return;
        }

        Transform visualRoot = hazardRoot.Find(SceneModuleVisualUtility.VisualRootName);
        ConfigureModule(hazardRoot, transform, visualRoot, footprintSize);
    }

    public void SyncModuleLayout(bool forceCollider = false, bool forceWarning = false)
    {
        if (!IsLogicRoot())
        {
            return;
        }

        if (forceCollider || autoSyncColliderToVisual)
        {
            SyncColliderToVisualBounds(forceCollider);
        }

        if (forceWarning || autoSyncWarningArea)
        {
            SyncWarningAreaToColliderOrVisual(forceWarning);
        }

        SyncDebugBoundsVisual();
    }

    public static bool IsCompleteModuleRoot(Transform root)
    {
        if (root == null)
        {
            return false;
        }

        return root.Find(SceneModuleVisualUtility.LogicRootName) != null
            && root.Find(SceneModuleVisualUtility.VisualRootName) != null
            && root.Find(HazardModuleLayoutUtility.WarningAreaName) != null
            && root.Find(HazardModuleLayoutUtility.LabelAnchorName) != null
            && root.Find(HazardModuleLayoutUtility.DebugBoundsName) != null;
    }

    static SpikeTrap CreateFallbackModule(
        Transform parent,
        string name,
        Vector3 position,
        Quaternion rotation,
        Vector3 baseSize,
        int spikeCount,
        float damageAmount,
        bool active)
    {
        var root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = position;
        root.transform.rotation = rotation;
        root.transform.localScale = Vector3.one;

        Transform logicRoot = CreateChild(root.transform, SceneModuleVisualUtility.LogicRootName);
        Transform visualRoot = CreateChild(root.transform, SceneModuleVisualUtility.VisualRootName);
        HazardModuleLayoutUtility.EnsureChild(root.transform, HazardModuleLayoutUtility.WarningAreaName);
        HazardModuleLayoutUtility.EnsureLabelAnchor(root.transform, 0.8f);
        HazardModuleLayoutUtility.EnsureChild(root.transform, HazardModuleLayoutUtility.DebugBoundsName);

        var trigger = logicRoot.gameObject.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(baseSize.x, DefaultColliderHeight, baseSize.z);
        trigger.center = new Vector3(0f, DefaultColliderCenterY, 0f);

        var trap = logicRoot.gameObject.AddComponent<SpikeTrap>();
        trap.damage = damageAmount;
        trap.isActive = active;
        trap.fallbackSpikeCount = Mathf.Max(1, spikeCount);
        trap.ConfigureModule(root.transform, logicRoot, visualRoot, baseSize);
        return trap;
    }

    static bool TryInstantiateRegisteredModule(
        Transform parent,
        string name,
        Vector3 position,
        Quaternion rotation,
        Vector3 baseSize,
        int spikeCount,
        float damageAmount,
        bool active,
        out SpikeTrap trap)
    {
        trap = null;
        if (!HazardSpikePrefabCatalog.TryResolveModulePrefab(out GameObject modulePrefab)
            || modulePrefab == null)
        {
            return false;
        }

        GameObject instance;
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            instance = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(modulePrefab, parent);
        }
        else
#endif
        {
            instance = Object.Instantiate(modulePrefab, parent);
        }

        if (instance == null)
        {
            return false;
        }

        instance.name = name;
        instance.transform.SetParent(parent, false);
        instance.transform.position = position;
        instance.transform.rotation = rotation;
        instance.transform.localScale = Vector3.one;

        Transform logicRoot = instance.transform.Find(SceneModuleVisualUtility.LogicRootName);
        Transform visualRoot = instance.transform.Find(SceneModuleVisualUtility.VisualRootName);
        if (logicRoot == null)
        {
            Object.Destroy(instance);
            return false;
        }

        trap = logicRoot.GetComponent<SpikeTrap>();
        if (trap == null)
        {
            trap = logicRoot.gameObject.AddComponent<SpikeTrap>();
        }

        trap.damage = damageAmount;
        trap.isActive = active;
        trap.fallbackSpikeCount = Mathf.Max(1, spikeCount);

        if (visualRoot != null && visualRoot.childCount > 0 && !NeedsVisualRebuild(visualRoot))
        {
            trap.moduleVisualApplied = true;
        }

        trap.ConfigureModule(instance.transform, logicRoot, visualRoot, baseSize);
        return true;
    }

    void ConfigureModule(Transform hazardRoot, Transform logicRoot, Transform visualRoot, Vector3 footprintSize)
    {
        hazardRoot.localScale = Vector3.one;

        var box = logicRoot.GetComponent<BoxCollider>();
        if (box == null)
        {
            box = logicRoot.gameObject.AddComponent<BoxCollider>();
        }

        box.isTrigger = true;
        box.enabled = isActive;

        if (visualRoot != null && visualRoot.childCount > 0 && !NeedsVisualRebuild(visualRoot))
        {
            moduleVisualApplied = true;
            SceneModuleVisualUtility.ApplySpikeFootprintToVisualRoot(visualRoot, footprintSize);
            StripVisualGameplayColliders(visualRoot);
        }
        else if (visualRoot != null && (!moduleVisualApplied || NeedsVisualRebuild(visualRoot)))
        {
            RebuildVisual(hazardRoot, logicRoot, visualRoot, footprintSize);
        }

        HazardModuleLayoutUtility.EnsureChild(hazardRoot, HazardModuleLayoutUtility.WarningAreaName);
        HazardModuleLayoutUtility.EnsureLabelAnchor(hazardRoot, 0.8f);
        HazardModuleLayoutUtility.EnsureChild(hazardRoot, HazardModuleLayoutUtility.DebugBoundsName);

        if (autoSyncColliderToVisual)
        {
            SyncColliderToVisualBounds();
        }
        else
        {
            box.size = new Vector3(footprintSize.x, DefaultColliderHeight, footprintSize.z);
            box.center = new Vector3(0f, DefaultColliderCenterY, 0f);
        }

        if (autoSyncWarningArea)
        {
            SyncWarningAreaToColliderOrVisual();
        }

        SyncDebugBoundsVisual();
        ApplyActiveVisualState();
    }

    bool TryMigrateLegacyRoot()
    {
        if (IsLogicRoot())
        {
            return false;
        }

        Transform existingLogic = transform.Find(SceneModuleVisualUtility.LogicRootName);
        if (existingLogic != null)
        {
            return TryCleanupDuplicateRootTrap(existingLogic);
        }

        Transform trapRoot = transform;
        Transform logicRoot = CreateChild(trapRoot, SceneModuleVisualUtility.LogicRootName);
        Transform visualRoot = CreateChild(trapRoot, SceneModuleVisualUtility.VisualRootName);
        HazardModuleLayoutUtility.EnsureChild(trapRoot, HazardModuleLayoutUtility.WarningAreaName);

        var sourceBox = GetComponent<BoxCollider>();
        BoxCollider logicBox = logicRoot.gameObject.AddComponent<BoxCollider>();
        if (sourceBox != null)
        {
            logicBox.isTrigger = sourceBox.isTrigger;
            logicBox.size = sourceBox.size;
            logicBox.center = sourceBox.center;
        }
        else
        {
            logicBox.isTrigger = true;
            logicBox.size = new Vector3(2f, DefaultColliderHeight, 2f);
            logicBox.center = new Vector3(0f, DefaultColliderCenterY, 0f);
        }

        SpikeTrap logicTrap = logicRoot.gameObject.AddComponent<SpikeTrap>();
        logicTrap.CopySettingsFrom(this);
        logicTrap.fallbackSpikeCount = CountLegacySpikeChildren(trapRoot) > 0
            ? CountLegacySpikeChildren(trapRoot)
            : fallbackSpikeCount;
        logicTrap.ConfigureModule(
            trapRoot,
            logicRoot,
            visualRoot,
            GetFootprintSizeFromBox(logicBox));

        DestroyColliderSafe(sourceBox);
        DestroySafe(this);
        return true;
    }

    bool TryCleanupDuplicateRootTrap(Transform existingLogic)
    {
        Transform trapRoot = transform;
        Transform visualRoot = EnsureVisualRoot(trapRoot);
        SpikeTrap logicTrap = existingLogic.GetComponent<SpikeTrap>();

        if (logicTrap == null)
        {
            logicTrap = existingLogic.gameObject.AddComponent<SpikeTrap>();
            logicTrap.CopySettingsFrom(this);
        }

        var sourceBox = GetComponent<BoxCollider>();
        BoxCollider logicBox = existingLogic.GetComponent<BoxCollider>();
        if (logicBox == null && sourceBox != null)
        {
            logicBox = existingLogic.gameObject.AddComponent<BoxCollider>();
            logicBox.isTrigger = sourceBox.isTrigger;
            logicBox.size = sourceBox.size;
            logicBox.center = sourceBox.center;
        }

        logicTrap.EnsureModuleLayout();
        DestroyColliderSafe(sourceBox);
        DestroySafe(this);
        return true;
    }

    void RebuildVisual(Transform trapRoot, Transform logicRoot, Transform visualRoot, Vector3 footprint)
    {
        int spikeCount = CountLegacySpikeChildren(trapRoot);
        if (spikeCount <= 0)
        {
            spikeCount = fallbackSpikeCount > 0 ? fallbackSpikeCount : DefaultFallbackSpikeCount;
        }

        fallbackSpikeCount = spikeCount;
        ClearLegacyVisualChildren(trapRoot, logicRoot, visualRoot);

        if (!SceneModuleVisualUtility.TryInstantiateHazardSpikeVisual(visualRoot, footprint))
        {
            BuildPrimitiveTrapFallback(visualRoot, footprint, spikeCount);
        }

        moduleVisualApplied = true;

        if (autoSyncColliderToVisual)
        {
            SyncColliderToVisualBounds();
        }
    }

    bool IsLogicRoot()
    {
        return gameObject.name == SceneModuleVisualUtility.LogicRootName;
    }

    static Transform EnsureVisualRoot(Transform trapRoot)
    {
        Transform visualRoot = trapRoot.Find(SceneModuleVisualUtility.VisualRootName);
        if (visualRoot != null)
        {
            return visualRoot;
        }

        visualRoot = CreateChild(trapRoot, SceneModuleVisualUtility.VisualRootName);
        return visualRoot;
    }

    Vector3 GetFootprintSize()
    {
        return GetFootprintSizeFromBox(GetComponent<BoxCollider>());
    }

    static Vector3 GetFootprintSizeFromBox(BoxCollider box)
    {
        if (box == null)
        {
            return new Vector3(2f, 0.4f, 2f);
        }

        return new Vector3(box.size.x, 0.4f, box.size.z);
    }

    static bool NeedsVisualRebuild(Transform visualRoot)
    {
        if (visualRoot == null || visualRoot.childCount == 0)
        {
            return true;
        }

        for (int i = 0; i < visualRoot.childCount; i++)
        {
            Transform child = visualRoot.GetChild(i);
            if (child == null)
            {
                continue;
            }

            if (child.name.IndexOf("Hazard_SpikeTrap", System.StringComparison.OrdinalIgnoreCase) >= 0
                || child.name.IndexOf("PF_Generic_Hazard", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return false;
            }
        }

        return HasLegacyPrimitiveVisuals(visualRoot);
    }

    static bool HasLegacyPrimitiveVisuals(Transform visualRoot)
    {
        for (int i = 0; i < visualRoot.childCount; i++)
        {
            Transform child = visualRoot.GetChild(i);
            if (child != null && (child.name.StartsWith("Spike_") || child.name == "Base"))
            {
                return true;
            }
        }

        return false;
    }

    static int CountLegacySpikeChildren(Transform trapRoot)
    {
        int count = 0;
        for (int i = 0; i < trapRoot.childCount; i++)
        {
            Transform child = trapRoot.GetChild(i);
            if (child != null && child.name.StartsWith("Spike_"))
            {
                count++;
            }
        }

        return count;
    }

    static void ClearLegacyVisualChildren(Transform trapRoot, Transform logicRoot, Transform visualRoot)
    {
        SceneModuleVisualUtility.ClearVisualChildren(visualRoot);

        for (int i = trapRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = trapRoot.GetChild(i);
            if (child == null || child == logicRoot || child == visualRoot)
            {
                continue;
            }

            if (child.name.StartsWith("Spike_") || child.name == "Base")
            {
                DestroyObjectSafe(child.gameObject);
            }
        }
    }

    static Transform CreateChild(Transform parent, string childName)
    {
        var child = new GameObject(childName).transform;
        child.SetParent(parent, false);
        child.localPosition = Vector3.zero;
        child.localRotation = Quaternion.identity;
        child.localScale = Vector3.one;
        return child;
    }

    public static void BuildPrimitiveTrapFallback(Transform visualRoot, Vector3 baseSize, int spikeCount)
    {
        BuildPrimitiveSpikeFallback(visualRoot, baseSize, spikeCount);
    }

    public static void BuildPrimitiveSpikeFallback(Transform visualRoot, Vector3 baseSize, int spikeCount)
    {
        float spacing = baseSize.x / spikeCount;
        float startX = -baseSize.x * 0.5f + spacing * 0.5f;
        var spikeColor = new Color(0.45f, 0.45f, 0.5f);

        for (int i = 0; i < spikeCount; i++)
        {
            var spike = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spike.name = $"Spike_{i + 1}";
            spike.transform.SetParent(visualRoot, false);
            spike.transform.localPosition = new Vector3(startX + spacing * i, 0.18f, 0f);
            spike.transform.localScale = new Vector3(0.22f, 0.36f, 0.22f);
            spike.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
            spike.isStatic = true;
            DestroyColliderSafe(spike.GetComponent<Collider>());

            var renderer = spike.GetComponent<Renderer>();
            if (renderer != null)
            {
                var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                material.color = spikeColor;
                renderer.sharedMaterial = material;
            }
        }

        var basePlate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        basePlate.name = "Base";
        basePlate.transform.SetParent(visualRoot, false);
        basePlate.transform.localPosition = new Vector3(0f, 0.03f, 0f);
        basePlate.transform.localScale = new Vector3(baseSize.x, 0.06f, baseSize.z);
        basePlate.isStatic = true;
        DestroyColliderSafe(basePlate.GetComponent<Collider>());

        var baseRenderer = basePlate.GetComponent<Renderer>();
        if (baseRenderer != null)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(0.25f, 0.22f, 0.22f);
            baseRenderer.sharedMaterial = material;
        }
    }

    static void DestroyColliderSafe(Collider collider)
    {
        if (collider == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Object.Destroy(collider);
        }
        else
        {
            Object.DestroyImmediate(collider);
        }
    }

    static void DestroyObjectSafe(Object target)
    {
        if (target == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Object.Destroy(target);
        }
        else
        {
            Object.DestroyImmediate(target);
        }
    }

    static void DestroySafe(Object target)
    {
        DestroyObjectSafe(target);
    }
}
