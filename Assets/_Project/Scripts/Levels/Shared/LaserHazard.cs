using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(BoxCollider))]
public class LaserHazard : MonoBehaviour
{
    [SerializeField] float damage = 12f;
    [SerializeField] float hazardCooldown = 1f;
    [SerializeField] float retriggerDelay = 0.5f;
    [SerializeField] bool isActive = true;
    [SerializeField] bool moduleVisualApplied;

    [Header("Collider Sync")]
    [SerializeField] bool autoSyncColliderToVisual = true;
    [SerializeField] Vector3 colliderPadding = new Vector3(0.2f, 0.2f, 0.2f);
    [SerializeField] bool syncOnAwake = true;
    [SerializeField] bool syncOnValidate = true;
    [SerializeField] bool syncDebugBounds = true;

    [Header("Warning Area Sync")]
    [FormerlySerializedAs("syncWarningArea")]
    [SerializeField] bool autoSyncWarningArea = true;
    [SerializeField] float warningAreaPadding = 0.15f;
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

        Transform visualRoot = SceneModuleVisualUtility.ResolveHazardVisualContainer(hazardRoot);
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
        HazardModuleLayoutUtility.EnsureLabelAnchor(hazardRoot, 1.2f);

        Vector3 footprint = GetFootprintSize();
        if (!moduleVisualApplied || NeedsVisualRebuild(visualRoot))
        {
            RebuildVisual(hazardRoot, visualRoot, footprint);
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
        else
        {
            SyncManualLayout(hazardRoot, footprint);
        }
    }

    void SyncManualLayout(Transform hazardRoot, Vector3 footprint)
    {
        SyncWarningAreaToColliderOrVisual(force: true);
        SyncDebugBoundsVisual();
    }

    Color GetWarningTint()
    {
        return new Color(0.1f, 0.75f, 0.95f, isActive ? 0.18f : 0.06f);
    }

    /// <summary>
    /// Resizes LogicRoot trigger collider (and optional warning/debug helpers) to match VisualRoot renderer bounds.
    /// </summary>
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

        Transform visualRoot = SceneModuleVisualUtility.ResolveHazardVisualContainer(hazardRoot);
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

    /// <summary>Aligns WarningArea / WarningPlane to trigger collider or visual ground footprint.</summary>
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

        Transform visualRoot = SceneModuleVisualUtility.ResolveHazardVisualContainer(hazardRoot);
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

        Collider[] colliders = visualRoot.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            if (collider == null)
            {
                continue;
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
    }

    public void CopySettingsFrom(LaserHazard source)
    {
        if (source == null)
        {
            return;
        }

        damage = source.damage;
        hazardCooldown = source.hazardCooldown;
        retriggerDelay = source.retriggerDelay;
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

    public static LaserHazard CreateElectricFieldHazard(
        Transform parent,
        string name,
        Vector3 position,
        Vector3 size,
        float damageAmount = 12f,
        bool active = true)
    {
        return Create(parent, name, position, Quaternion.identity, size, damageAmount, active);
    }

    public static LaserHazard Create(
        Transform parent,
        string name,
        Vector3 center,
        Vector3 size,
        float damageAmount = 12f,
        bool active = true)
    {
        return Create(parent, name, center, Quaternion.identity, size, damageAmount, active);
    }

    public static LaserHazard Create(
        Transform parent,
        string name,
        Vector3 center,
        Quaternion rotation,
        Vector3 size,
        float damageAmount = 12f,
        bool active = true)
    {
        if (TryInstantiateRegisteredModule(parent, name, center, rotation, size, damageAmount, active, out LaserHazard moduleInstance))
        {
            return moduleInstance;
        }

        return CreateFallbackModule(parent, name, center, rotation, size, damageAmount, active);
    }

    public void Configure(Vector3 size, float damageAmount, bool active)
    {
        damage = damageAmount;
        isActive = active;
        Transform hazardRoot = transform.parent;
        if (hazardRoot == null || !IsLogicRoot())
        {
            return;
        }

        Transform visualRoot = SceneModuleVisualUtility.ResolveHazardVisualContainer(hazardRoot);
        ConfigureModule(hazardRoot, transform, visualRoot, size);
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
            && SceneModuleVisualUtility.ResolveHazardVisualContainer(root) != null
            && root.Find(HazardModuleLayoutUtility.WarningAreaName) != null
            && root.Find(HazardModuleLayoutUtility.LabelAnchorName) != null
            && root.Find(HazardModuleLayoutUtility.DebugBoundsName) != null;
    }

    static LaserHazard CreateFallbackModule(
        Transform parent,
        string name,
        Vector3 center,
        Quaternion rotation,
        Vector3 size,
        float damageAmount,
        bool active)
    {
        var root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = center;
        root.transform.rotation = rotation;
        root.transform.localScale = Vector3.one;

        Transform logicRoot = CreateChild(root.transform, SceneModuleVisualUtility.LogicRootName);
        Transform visualRoot = CreateChild(root.transform, SceneModuleVisualUtility.VisualName);
        HazardModuleLayoutUtility.EnsureChild(root.transform, HazardModuleLayoutUtility.WarningAreaName);
        HazardModuleLayoutUtility.EnsureLabelAnchor(root.transform, 1.2f);
        HazardModuleLayoutUtility.EnsureChild(root.transform, HazardModuleLayoutUtility.DebugBoundsName);

        var trigger = logicRoot.gameObject.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = size;
        trigger.center = Vector3.zero;

        var laser = logicRoot.gameObject.AddComponent<LaserHazard>();
        laser.damage = damageAmount;
        laser.isActive = active;
        laser.ConfigureModule(root.transform, logicRoot.transform, visualRoot, size);
        return laser;
    }

    static bool TryInstantiateRegisteredModule(
        Transform parent,
        string name,
        Vector3 center,
        Quaternion rotation,
        Vector3 size,
        float damageAmount,
        bool active,
        out LaserHazard laser)
    {
        laser = null;
        if (!HazardElectricFieldPrefabCatalog.TryResolveModulePrefab(out GameObject modulePrefab)
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
        instance.transform.position = center;
        instance.transform.rotation = rotation;
        instance.transform.localScale = Vector3.one;

        Transform logicRoot = instance.transform.Find(SceneModuleVisualUtility.LogicRootName);
        Transform visualRoot = SceneModuleVisualUtility.ResolveHazardVisualContainer(instance.transform);
        if (logicRoot == null)
        {
            Object.Destroy(instance);
            return false;
        }

        laser = logicRoot.GetComponent<LaserHazard>();
        if (laser == null)
        {
            laser = logicRoot.gameObject.AddComponent<LaserHazard>();
        }

        laser.damage = damageAmount;
        laser.isActive = active;

        if (visualRoot != null && visualRoot.childCount > 0 && !NeedsVisualRebuild(visualRoot))
        {
            laser.moduleVisualApplied = true;
        }

        laser.ConfigureModule(instance.transform, logicRoot, visualRoot, size);
        return true;
    }

    static bool TryInstantiateRegisteredModule(
        Transform parent,
        string name,
        Vector3 center,
        Vector3 size,
        float damageAmount,
        bool active,
        out LaserHazard laser)
    {
        return TryInstantiateRegisteredModule(parent, name, center, Quaternion.identity, size, damageAmount, active, out laser);
    }

    void ConfigureModule(Transform hazardRoot, Transform logicRoot, Transform visualRoot, Vector3 size)
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
            SceneModuleVisualUtility.ApplyElectricFieldFootprintToVisualRoot(visualRoot, size);
            StripVisualGameplayColliders(visualRoot);
        }
        else if (visualRoot != null && (!moduleVisualApplied || NeedsVisualRebuild(visualRoot)))
        {
            RebuildVisual(hazardRoot, visualRoot, new Vector3(size.x, size.y, size.z));
        }

        HazardModuleLayoutUtility.EnsureChild(hazardRoot, HazardModuleLayoutUtility.WarningAreaName);
        HazardModuleLayoutUtility.EnsureLabelAnchor(hazardRoot, 1.2f);
        HazardModuleLayoutUtility.EnsureChild(hazardRoot, HazardModuleLayoutUtility.DebugBoundsName);

        if (autoSyncColliderToVisual)
        {
            SyncColliderToVisualBounds();
        }
        else
        {
            box.size = size;
            box.center = Vector3.zero;
        }

        if (autoSyncWarningArea)
        {
            SyncWarningAreaToColliderOrVisual();
        }

        SyncDebugBoundsVisual();
        ApplyActiveVisualState();
    }

    void RebuildVisual(Transform hazardRoot, Transform visualRoot, Vector3 footprint)
    {
        ClearLegacyVisualChildren(hazardRoot, visualRoot);

        if (!SceneModuleVisualUtility.TryInstantiateElectricFieldVisual(visualRoot, footprint))
        {
            BuildPrimitiveElectricFieldFallback(visualRoot, footprint);
        }

        moduleVisualApplied = true;

        if (autoSyncColliderToVisual)
        {
            SyncColliderToVisualBounds();
        }
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
            return TryCleanupDuplicateRootHazard(existingLogic);
        }

        Transform hazardRoot = transform;
        Transform logicRoot = CreateChild(hazardRoot, SceneModuleVisualUtility.LogicRootName);
        Transform visualRoot = CreateChild(hazardRoot, SceneModuleVisualUtility.VisualName);
        HazardModuleLayoutUtility.EnsureChild(hazardRoot, HazardModuleLayoutUtility.WarningAreaName);

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
            logicBox.size = new Vector3(3.5f, 0.12f, 0.35f);
        }

        LaserHazard logicLaser = logicRoot.gameObject.AddComponent<LaserHazard>();
        logicLaser.CopySettingsFrom(this);
        logicLaser.ConfigureModule(
            hazardRoot,
            logicRoot,
            visualRoot,
            logicBox.size);

        DestroyColliderSafe(sourceBox);
        DestroySafe(this);
        return true;
    }

    bool TryCleanupDuplicateRootHazard(Transform existingLogic)
    {
        Transform hazardRoot = transform;
        Transform visualRoot = EnsureVisualRoot(hazardRoot);
        LaserHazard logicLaser = existingLogic.GetComponent<LaserHazard>();
        if (logicLaser == null)
        {
            logicLaser = existingLogic.gameObject.AddComponent<LaserHazard>();
            logicLaser.CopySettingsFrom(this);
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

        Vector3 size = logicBox != null ? logicBox.size : new Vector3(3.5f, 0.12f, 0.35f);
        logicLaser.ConfigureModule(hazardRoot, existingLogic, visualRoot, size);
        DestroyColliderSafe(sourceBox);
        DestroySafe(this);
        return true;
    }

    Vector3 GetFootprintSize()
    {
        return GetColliderSize();
    }

    Vector3 GetColliderSize()
    {
        var box = GetComponent<BoxCollider>();
        return box != null ? box.size : new Vector3(3.5f, 0.12f, 0.35f);
    }

    bool IsLogicRoot()
    {
        return gameObject.name == SceneModuleVisualUtility.LogicRootName;
    }

    static Transform EnsureVisualRoot(Transform hazardRoot)
    {
        Transform visual = hazardRoot.Find(SceneModuleVisualUtility.VisualName);
        if (visual != null)
        {
            return visual;
        }

        Transform legacy = hazardRoot.Find(SceneModuleVisualUtility.VisualRootName);
        if (legacy != null)
        {
            return legacy;
        }

        return HazardModuleLayoutUtility.EnsureChild(hazardRoot, SceneModuleVisualUtility.VisualName);
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

            if (child.name.IndexOf("ElectricField", System.StringComparison.OrdinalIgnoreCase) >= 0
                || child.name.IndexOf("PF_Generic_Hazard", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return false;
            }
        }

        return visualRoot.GetComponentInChildren<Renderer>() == null
            || visualRoot.Find("LaserBeam_Fallback") != null;
    }

    static void ClearLegacyVisualChildren(Transform hazardRoot, Transform visualRoot)
    {
        SceneModuleVisualUtility.ClearVisualChildren(visualRoot);

        for (int i = hazardRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = hazardRoot.GetChild(i);
            if (child == null)
            {
                continue;
            }

            string childName = child.name;
            if (childName == SceneModuleVisualUtility.LogicRootName
                || childName == SceneModuleVisualUtility.VisualName
                || childName == SceneModuleVisualUtility.VisualRootName
                || childName == HazardModuleLayoutUtility.WarningAreaName
                || childName == HazardModuleLayoutUtility.LabelAnchorName
                || childName == HazardModuleLayoutUtility.DebugBoundsName)
            {
                continue;
            }

            if (childName.StartsWith("Laser") || child.GetComponent<Renderer>() != null)
            {
                DestroyObjectSafe(child.gameObject);
            }
        }
    }

    static Transform CreateChild(Transform parent, string childName)
    {
        return HazardModuleLayoutUtility.EnsureChild(parent, childName);
    }

    public static void BuildPrimitiveElectricFieldFallback(Transform visualRoot, Vector3 size)
    {
        var beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
        beam.name = "LaserBeam_Fallback";
        beam.transform.SetParent(visualRoot, false);
        beam.transform.localPosition = Vector3.zero;
        beam.transform.localRotation = Quaternion.identity;
        beam.transform.localScale = size;
        beam.isStatic = true;
        DestroyColliderSafe(beam.GetComponent<Collider>());

        var renderer = beam.GetComponent<Renderer>();
        if (renderer != null)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(0.15f, 0.85f, 1f, 0.85f);
            material.SetColor("_EmissionColor", new Color(0.2f, 0.95f, 1f) * 2f);
            material.EnableKeyword("_EMISSION");
            renderer.sharedMaterial = material;
        }

        for (int i = 0; i < 2; i++)
        {
            var post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            post.name = $"ElectricPost_{i + 1}";
            post.transform.SetParent(visualRoot, false);
            float x = i == 0 ? -size.x * 0.45f : size.x * 0.45f;
            post.transform.localPosition = new Vector3(x, size.y * 0.5f, 0f);
            post.transform.localScale = new Vector3(0.12f, size.y, 0.12f);
            DestroyColliderSafe(post.GetComponent<Collider>());

            var postRenderer = post.GetComponent<Renderer>();
            if (postRenderer != null)
            {
                var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                material.color = new Color(0.18f, 0.2f, 0.24f);
                postRenderer.sharedMaterial = material;
            }
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
