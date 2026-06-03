using UnityEngine;

public static class ItemVisualBuilder
{
    public static GameObject CreatePickupVisual(Transform parent, ItemKind kind, Vector3 localScale)
    {
        if (ItemCatalog.TryGet(kind, out ItemData data) && data.WorldPickupPrefab != null)
        {
            var instance = Object.Instantiate(data.WorldPickupPrefab, parent);
            instance.name = "Visual";
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            return instance;
        }

        return CreateCubeVisual(parent, "Visual", data.IsValid ? data.DisplayColor : Color.gray, localScale, Vector3.zero);
    }

    public static GameObject CreatePlacedVisual(Transform parent, ItemKind kind, Vector3 scale, Vector3 localOffset)
    {
        if (ItemCatalog.TryGet(kind, out ItemData data) && data.PlacedPrefab != null)
        {
            var instance = Object.Instantiate(data.PlacedPrefab, parent);
            instance.name = "Visual";
            instance.transform.localPosition = localOffset;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            return instance;
        }

        return CreateCubeVisual(parent, "Visual", data.IsValid ? data.DisplayColor : Color.gray, scale, localOffset);
    }

    static GameObject CreateCubeVisual(
        Transform parent,
        string objectName,
        Color color,
        Vector3 scale,
        Vector3 localOffset)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = objectName;
        cube.transform.SetParent(parent, false);
        cube.transform.localPosition = localOffset;
        cube.transform.localScale = scale;
        Object.DestroyImmediate(cube.GetComponent<Collider>());

        var renderer = cube.GetComponent<Renderer>();
        if (renderer != null)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = color;
            material.SetColor("_EmissionColor", color * 0.2f);
            material.EnableKeyword("_EMISSION");
            renderer.sharedMaterial = material;
        }

        return cube;
    }
}
