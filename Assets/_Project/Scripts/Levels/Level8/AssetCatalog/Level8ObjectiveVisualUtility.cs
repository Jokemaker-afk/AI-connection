using UnityEngine;

/// <summary>Helpers for objective prefab instances — visual-only, no duplicate gameplay.</summary>
public static class Level8ObjectiveVisualUtility
{
    public static GameObject InstantiateVisualPrefab(GameObject prefab, Transform parent)
    {
        if (prefab == null || parent == null)
        {
            return null;
        }

        GameObject instance = Object.Instantiate(prefab, parent);
        instance.name = prefab.name;
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

        RemoveDuplicateComponent<DataCoreCollectible>(root);
        RemoveDuplicateComponent<SignalRelayInteractable>(root);
        RemoveDuplicateComponent<Level8ExitPortal>(root);
        RemoveDuplicateComponent<WorldPickupItem>(root);

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
