using System.Collections.Generic;
using UnityEngine;

public static class HandheldToolPrefabBuilder
{
    static readonly Dictionary<ItemKind, GameObject> PrefabCache = new Dictionary<ItemKind, GameObject>();

    public static GameObject GetOrCreatePrefab(ItemKind itemKind)
    {
        if (!ItemKindUtility.IsHandheldTool(itemKind))
        {
            HandheldToolDebug.Log($"Item {itemKind} is not a handheld tool.");
            return null;
        }

        if (PrefabCache.TryGetValue(itemKind, out GameObject cached) && cached != null)
        {
            var marker = cached.GetComponent<HandheldToolPrototypeMarker>();
            if (marker != null && marker.Version >= HandheldToolPrototypeMarker.CurrentVersion)
            {
                return cached;
            }

            Object.Destroy(cached);
            PrefabCache.Remove(itemKind);
        }

        HandheldToolProfile profile = ItemKindUtility.GetHandheldToolProfile(itemKind);
        if (profile.HandheldPrefab != null)
        {
            PrefabCache[itemKind] = profile.HandheldPrefab;
            HandheldToolDebug.Log($"Using assigned handheld prefab for {ItemKindUtility.GetDisplayName(itemKind)}.");
            return profile.HandheldPrefab;
        }

        GameObject prefab = BuildPrototypePrefab(itemKind, profile);
        PrefabCache[itemKind] = prefab;
        HandheldToolDebug.Log($"Built prototype handheld prefab: HandheldTool_{itemKind}");
        return prefab;
    }

    static GameObject BuildPrototypePrefab(ItemKind itemKind, HandheldToolProfile profile)
    {
        string displayName = ItemKindUtility.GetDisplayName(itemKind);
        var root = new GameObject($"HandheldTool_{itemKind}");
        root.AddComponent<HandheldToolPrototypeMarker>().Configure(itemKind);

        var visual = root.AddComponent<HandheldToolVisual>();
        var visualRoot = new GameObject("VisualRoot");
        visualRoot.transform.SetParent(root.transform, false);

        Color handleColor = new Color(0.42f, 0.28f, 0.16f);
        Color headColor = ItemKindUtility.GetDisplayColor(itemKind);
        Color debugColor = Color.Lerp(headColor, Color.white, 0.25f);

        CreatePart(visualRoot.transform, "PlaceholderBody", Vector3.zero, new Vector3(0.42f, 0.42f, 0.42f), debugColor);

        switch (profile.ToolKind)
        {
            case ToolKind.Pickaxe:
                CreatePart(visualRoot.transform, "Handle", new Vector3(0f, -0.08f, 0f), new Vector3(0.08f, 0.28f, 0.08f), handleColor);
                CreatePart(visualRoot.transform, "Head", new Vector3(0f, 0.12f, 0f), new Vector3(0.28f, 0.1f, 0.1f), headColor);
                CreatePart(visualRoot.transform, "PickPoint", new Vector3(0f, 0.12f, 0.14f), new Vector3(0.08f, 0.08f, 0.08f), headColor * 0.85f);
                break;
            case ToolKind.Axe:
                CreatePart(visualRoot.transform, "Handle", new Vector3(0f, -0.08f, 0f), new Vector3(0.08f, 0.26f, 0.08f), handleColor);
                CreatePart(visualRoot.transform, "Blade", new Vector3(0.12f, 0.1f, 0f), new Vector3(0.18f, 0.12f, 0.06f), headColor);
                break;
            case ToolKind.RepairTool:
                CreatePart(visualRoot.transform, "Handle", new Vector3(0f, -0.06f, 0f), new Vector3(0.07f, 0.16f, 0.07f), handleColor);
                CreatePart(visualRoot.transform, "ToolHead", new Vector3(0f, 0.08f, 0f), new Vector3(0.16f, 0.1f, 0.1f), headColor);
                break;
            default:
                CreatePart(visualRoot.transform, "Body", Vector3.zero, new Vector3(0.18f, 0.18f, 0.18f), headColor);
                break;
        }

        var labelRoot = new GameObject("Label");
        labelRoot.transform.SetParent(visualRoot.transform, false);
        labelRoot.transform.localPosition = new Vector3(0f, 0.38f, 0f);
        ItemWorldLabel.Create(labelRoot.transform, displayName, Vector3.zero, 0.12f);

        ToolAnimationProfile animations = profile.Animations;
        if (!animations.HasUseAnimation)
        {
            animations = PrototypeAnimationFactory.CreateToolAnimationProfile(profile.ToolKind);
        }

        visual.Configure(itemKind, animations, profile.FallbackSwingEnabled);
        EnsureRenderersEnabled(root);
        root.SetActive(false);
        Object.DontDestroyOnLoad(root);
        return root;
    }

    static void CreatePart(Transform parent, string name, Vector3 localPosition, Vector3 scale, Color color)
    {
        var part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = scale;

        Collider collider = part.GetComponent<Collider>();
        if (collider != null)
        {
            Object.Destroy(collider);
        }

        Renderer renderer = part.GetComponent<Renderer>();
        if (renderer != null)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = color;
            renderer.sharedMaterial = material;
            renderer.enabled = true;
        }
    }

    static void EnsureRenderersEnabled(GameObject root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].enabled = true;
            }
        }
    }
}
