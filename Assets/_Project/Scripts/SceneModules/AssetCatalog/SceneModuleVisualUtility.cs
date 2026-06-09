using UnityEngine;

/// <summary>Instantiate visual-only scene module prefabs; strip gameplay duplicates.</summary>
public static class SceneModuleVisualUtility
{
    public const string VisualName = "Visual";
    public const string VisualRootName = "VisualRoot";
    public const string LogicRootName = "LogicRoot";
    public const string AxisCorrectionName = "AxisCorrection";

    /// <summary>Authored diameter for Generic_Hazard_SpikeTrap_01 visual prefab.</summary>
    public const float HazardSpikeVisualDiameter = 2f;

    /// <summary>Authored width for Generic_Hazard_ElectricField_01 visual prefab.</summary>
    public const float ElectricFieldVisualWidth = 3.5f;

    /// <summary>Default Unity-side correction for Blender-authored FBX visual models.</summary>
    public static readonly Vector3 BlenderFbxAxisCorrectionEuler = new Vector3(90f, 0f, 180f);

    public static Quaternion BlenderFbxAxisCorrectionRotation =>
        Quaternion.Euler(BlenderFbxAxisCorrectionEuler);

    /// <summary>Resolves hazard module visual container: prefers Visual, falls back to legacy VisualRoot.</summary>
    public static Transform ResolveHazardVisualContainer(Transform hazardRoot)
    {
        if (hazardRoot == null)
        {
            return null;
        }

        Transform visual = hazardRoot.Find(VisualName);
        if (visual != null)
        {
            return visual;
        }

        return hazardRoot.Find(VisualRootName);
    }

    /// <summary>
    /// Flattens nested visual prefab wrappers so Visual connects directly to AxisCorrection and FBX model.
    /// Renames legacy VisualRoot to Visual when needed.
    /// </summary>
    public static bool FlattenAxisCorrectionLayer(GameObject prefabRoot)
    {
        if (prefabRoot == null)
        {
            return false;
        }

        bool changed = false;
        Transform legacyVisualRoot = prefabRoot.transform.Find(VisualRootName);
        Transform visual = prefabRoot.transform.Find(VisualName);

        if (visual == null && legacyVisualRoot != null)
        {
            legacyVisualRoot.name = VisualName;
            visual = legacyVisualRoot;
            changed = true;
        }

        if (visual == null)
        {
            return changed;
        }

        for (int i = visual.childCount - 1; i >= 0; i--)
        {
            Transform child = visual.GetChild(i);
            if (child == null)
            {
                continue;
            }

            Transform nestedVisual = child.Find(VisualName);
            if (nestedVisual != null)
            {
                MoveChildren(nestedVisual, visual);
                if (Application.isPlaying)
                {
                    Object.Destroy(child.gameObject);
                }
                else
                {
                    Object.DestroyImmediate(child.gameObject);
                }

                changed = true;
            }
        }

        EnsureBlenderAxisCorrection(prefabRoot);
        return changed;
    }

    static void MoveChildren(Transform from, Transform to)
    {
        for (int i = from.childCount - 1; i >= 0; i--)
        {
            Transform child = from.GetChild(i);
            if (child != null)
            {
                child.SetParent(to, false);
            }
        }
    }

    public static GameObject InstantiateModuleVisual(GameObject prefab, Transform visualRoot)
    {
        if (prefab == null || visualRoot == null)
        {
            return null;
        }

        GameObject instance;
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            instance = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab, visualRoot);
        }
        else
#endif
        {
            instance = Object.Instantiate(prefab, visualRoot);
        }

        if (instance == null)
        {
            return null;
        }

        instance.name = prefab.name;
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        StripGameplayFromVisualHierarchy(instance);
        EnsureBlenderAxisCorrection(instance);
        return instance;
    }

    public static void ApplySpikeFootprintToVisualRoot(Transform visualRoot, Vector3 footprintSize)
    {
        if (visualRoot == null)
        {
            return;
        }

        float fitScale = HazardSpikeVisualDiameter > 0f
            ? Mathf.Max(footprintSize.x, footprintSize.z) / HazardSpikeVisualDiameter
            : 1f;
        visualRoot.localPosition = Vector3.zero;
        visualRoot.localRotation = Quaternion.identity;
        visualRoot.localScale = Vector3.one * fitScale;
    }

    public static void ApplyElectricFieldFootprintToVisualRoot(Transform visualRoot, Vector3 footprintSize)
    {
        if (visualRoot == null)
        {
            return;
        }

        float fitScale = ElectricFieldVisualWidth > 0f
            ? Mathf.Max(footprintSize.x, footprintSize.z) / ElectricFieldVisualWidth
            : 1f;
        visualRoot.localPosition = new Vector3(0f, -footprintSize.y * 0.5f, 0f);
        visualRoot.localRotation = footprintSize.z > footprintSize.x
            ? Quaternion.Euler(0f, 90f, 0f)
            : Quaternion.identity;
        visualRoot.localScale = Vector3.one * fitScale;
    }

    /// <summary>Instantiates a visual-only module prefab under VisualRoot with footprint scaling.</summary>
    public static GameObject InstantiateVisualOnly(
        GameObject prefab,
        Transform visualRoot,
        Vector3 footprintSize,
        float authoredDiameter,
        Vector3 localPosition = default,
        Quaternion? localRotation = null)
    {
        if (prefab == null || visualRoot == null)
        {
            return null;
        }

        ClearVisualChildren(visualRoot);
        GameObject instance = InstantiateModuleVisual(prefab, visualRoot);
        if (instance == null)
        {
            return null;
        }

        float fitScale = authoredDiameter > 0f
            ? Mathf.Max(footprintSize.x, footprintSize.z) / authoredDiameter
            : 1f;
        visualRoot.localScale = Vector3.one * fitScale;
        visualRoot.localPosition = localPosition;
        visualRoot.localRotation = localRotation ?? Quaternion.identity;
        return instance;
    }

    /// <summary>
    /// Instantiates the registered hazard spike visual under VisualRoot; returns false when registry prefab is missing.
    /// </summary>
    public static bool TryInstantiateHazardSpikeVisual(Transform visualRoot, Vector3 footprintSize)
    {
        if (visualRoot == null)
        {
            return false;
        }

        if (!HazardSpikePrefabCatalog.TryResolveVisualPrefab(out GameObject prefab) || prefab == null)
        {
            return false;
        }

        return InstantiateVisualOnly(prefab, visualRoot, footprintSize, HazardSpikeVisualDiameter) != null;
    }

    /// <summary>
    /// Instantiates the registered electric field visual under VisualRoot; returns false when registry prefab is missing.
    /// </summary>
    public static bool TryInstantiateElectricFieldVisual(Transform visualRoot, Vector3 footprintSize)
    {
        if (visualRoot == null)
        {
            return false;
        }

        if (!HazardElectricFieldPrefabCatalog.TryResolveVisualPrefab(out GameObject prefab) || prefab == null)
        {
            return false;
        }

        return InstantiateVisualOnly(
            prefab,
            visualRoot,
            footprintSize,
            ElectricFieldVisualWidth,
            new Vector3(0f, -footprintSize.y * 0.5f, 0f),
            footprintSize.z > footprintSize.x ? Quaternion.Euler(0f, 90f, 0f) : Quaternion.identity) != null;
    }

    /// <summary>
    /// Ensures Visual → AxisCorrection → FBX hierarchy with default Blender FBX rotation on AxisCorrection only.
    /// </summary>
    public static void EnsureBlenderAxisCorrection(GameObject prefabRoot)
    {
        if (prefabRoot == null)
        {
            return;
        }

        Transform visual = prefabRoot.transform.Find(VisualName);
        if (visual == null)
        {
            visual = prefabRoot.transform.Find(VisualRootName);
        }

        if (visual == null)
        {
            return;
        }

        if (!ShouldApplyBlenderAxisCorrection(visual))
        {
            return;
        }

        Transform axis = visual.Find(AxisCorrectionName);
        if (axis == null)
        {
            var axisGo = new GameObject(AxisCorrectionName);
            axis = axisGo.transform;
            axis.SetParent(visual, false);

            for (int i = visual.childCount - 1; i >= 0; i--)
            {
                Transform child = visual.GetChild(i);
                if (child == axis)
                {
                    continue;
                }

                child.SetParent(axis, false);
            }
        }

        axis.localPosition = Vector3.zero;
        axis.localRotation = BlenderFbxAxisCorrectionRotation;
        axis.localScale = Vector3.one;

        visual.localPosition = Vector3.zero;
        visual.localRotation = Quaternion.identity;
        visual.localScale = Vector3.one;

        prefabRoot.transform.localRotation = Quaternion.identity;
        prefabRoot.transform.localPosition = Vector3.zero;
        prefabRoot.transform.localScale = Vector3.one;

        for (int i = 0; i < axis.childCount; i++)
        {
            Transform fbxChild = axis.GetChild(i);
            fbxChild.localPosition = Vector3.zero;
            fbxChild.localRotation = Quaternion.identity;
            fbxChild.localScale = Vector3.one;
        }
    }

    static bool ShouldApplyBlenderAxisCorrection(Transform visual)
    {
        if (visual.Find(AxisCorrectionName) != null)
        {
            return true;
        }

        for (int i = 0; i < visual.childCount; i++)
        {
            if (IsBlenderFbxVisualChild(visual.GetChild(i)))
            {
                return true;
            }
        }

        return false;
    }

    static bool IsBlenderFbxVisualChild(Transform child)
    {
        if (child == null)
        {
            return false;
        }

        string name = child.name;
        if (name.StartsWith("Generic_") && name.IndexOf("BuffStation", System.StringComparison.Ordinal) < 0)
        {
            return true;
        }

#if UNITY_EDITOR
        var source = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(child.gameObject);
        if (source != null)
        {
            string path = UnityEditor.AssetDatabase.GetAssetPath(source);
            if (path.IndexOf("Art/Imported/FBX", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }
#endif

        return false;
    }

    public static void ClearVisualChildren(Transform visualRoot)
    {
        if (visualRoot == null)
        {
            return;
        }

        for (int i = visualRoot.childCount - 1; i >= 0; i--)
        {
            Transform child = visualRoot.GetChild(i);
            if (child != null)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(child.gameObject);
                }
                else
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }
        }
    }

    public static void StripGameplayFromVisualHierarchy(GameObject root)
    {
        if (root == null)
        {
            return;
        }

        RemoveDuplicateComponent<HubReturnZone>(root);
        RemoveDuplicateComponent<BuffBubble>(root);
        RemoveDuplicateComponent<WorldPickupItem>(root);
        RemoveDuplicateComponent<DataCoreCollectible>(root);
        RemoveDuplicateComponent<SpikeTrap>(root);
        RemoveDuplicateComponent<LaserHazard>(root);
        RemoveDuplicateComponent<GuaranteedDamageZone>(root);

        Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null)
            {
                Object.Destroy(colliders[i]);
            }
        }
    }

    static void RemoveDuplicateComponent<T>(GameObject root) where T : Component
    {
        T[] components = root.GetComponentsInChildren<T>(true);
        for (int i = 0; i < components.Length; i++)
        {
            if (components[i] != null)
            {
                Object.Destroy(components[i]);
            }
        }
    }
}
