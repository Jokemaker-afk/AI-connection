using UnityEngine;

/// <summary>Helpers for resource pickup prefab visuals — visual-only under pickup root.</summary>
public static class Level8ResourceVisualUtility
{
    const string VisualChildName = "Visual";

    public static GameObject InstantiatePickupVisual(GameObject prefab, Transform pickupRoot)
    {
        if (prefab == null || pickupRoot == null)
        {
            return null;
        }

        Transform existingVisual = pickupRoot.Find(VisualChildName);
        if (existingVisual != null)
        {
            Object.Destroy(existingVisual.gameObject);
        }

        Transform existingOverlay = pickupRoot.Find("Level8ResourceVisual");
        if (existingOverlay != null)
        {
            Object.Destroy(existingOverlay.gameObject);
        }

        GameObject instance = Object.Instantiate(prefab, pickupRoot);
        instance.name = VisualChildName;
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;

        StripGameplayFromVisualHierarchy(instance);
        return instance;
    }

    public static void StripGameplayFromVisualHierarchy(GameObject root)
    {
        if (root == null)
        {
            return;
        }

        RemoveDuplicateComponent<WorldPickupItem>(root);
        RemoveDuplicateComponent<DroppedItemMarker>(root);
        RemoveDuplicateComponent<DataCoreCollectible>(root);
        RemoveDuplicateComponent<Level8ResourcePickupEnhancer>(root);

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
