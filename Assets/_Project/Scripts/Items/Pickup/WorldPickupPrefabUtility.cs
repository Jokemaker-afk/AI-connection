using UnityEngine;

/// <summary>
/// Detects complete vs visual-only world pickup prefabs and resolves the logic host transform.
/// </summary>
public static class WorldPickupPrefabUtility
{
    public static bool IsCompleteWorldPickupPrefab(GameObject prefab)
    {
        if (prefab == null)
        {
            return false;
        }

        Transform visual = prefab.transform.Find(SceneModuleVisualUtility.VisualName);
        if (visual == null)
        {
            return false;
        }

        Transform logicRoot = prefab.transform.Find(SceneModuleVisualUtility.LogicRootName);
        if (logicRoot != null && logicRoot.GetComponent<WorldPickupItem>() != null)
        {
            return true;
        }

        return prefab.GetComponent<WorldPickupItem>() != null;
    }

    public static bool IsVisualOnlyWorldPickupPrefab(GameObject prefab)
    {
        if (prefab == null || IsCompleteWorldPickupPrefab(prefab))
        {
            return false;
        }

        return prefab.transform.Find(SceneModuleVisualUtility.VisualName) != null;
    }

    public static GameObject ResolveLogicHost(GameObject pickupRoot)
    {
        if (pickupRoot == null)
        {
            return null;
        }

        Transform logicRoot = pickupRoot.transform.Find(SceneModuleVisualUtility.LogicRootName);
        if (logicRoot != null)
        {
            return logicRoot.gameObject;
        }

        if (pickupRoot.GetComponent<WorldPickupItem>() != null)
        {
            return pickupRoot;
        }

        return pickupRoot;
    }

    /// <summary>
    /// Scene instance to remove after pickup. Never the spawn container (e.g. Pickups parent).
    /// </summary>
    public static GameObject ResolvePickupInstanceRoot(GameObject from)
    {
        if (from == null)
        {
            return null;
        }

        Transform t = from.transform;

        if (t.name == SceneModuleVisualUtility.LogicRootName && t.parent != null
            && t.parent.Find(SceneModuleVisualUtility.VisualName) != null)
        {
            return t.parent.gameObject;
        }

        if (t.Find(SceneModuleVisualUtility.VisualName) != null)
        {
            return t.gameObject;
        }

        WorldPickupItem pickup = from.GetComponent<WorldPickupItem>();
        if (pickup != null && t.parent != null)
        {
            Transform parent = t.parent;
            if (parent.Find(SceneModuleVisualUtility.LogicRootName) != null
                && parent.Find(SceneModuleVisualUtility.VisualName) != null)
            {
                return parent.gameObject;
            }
        }

        return t.gameObject;
    }

    public static Transform ResolvePickupMotionTransform(GameObject from)
    {
        GameObject instanceRoot = ResolvePickupInstanceRoot(from);
        return instanceRoot != null ? instanceRoot.transform : from.transform;
    }
}
