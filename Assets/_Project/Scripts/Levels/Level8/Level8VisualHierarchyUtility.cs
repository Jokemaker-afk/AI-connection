using UnityEngine;

/// <summary>Shared placeholder visuals for Level8 readability hierarchy.</summary>
public static class Level8VisualHierarchyUtility
{
    public static GameObject CreateVerticalBeam(
        Transform parent,
        string name,
        Vector3 localPosition,
        Color color,
        float height = 4f,
        float radius = 0.12f,
        bool transparent = true)
    {
        var beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        beam.name = name;
        beam.transform.SetParent(parent, false);
        beam.transform.localPosition = localPosition + Vector3.up * (height * 0.5f);
        beam.transform.localScale = new Vector3(radius * 2f, height * 0.5f, radius * 2f);
        Object.Destroy(beam.GetComponent<Collider>());
        Level8VisualMaterialUtility.ApplyColor(beam.GetComponent<Renderer>(), color, transparent: transparent);
        return beam;
    }

    public static GameObject CreateGlowRing(
        Transform parent,
        string name,
        Vector3 localPosition,
        Color color,
        float radius = 1.8f,
        float height = 0.08f)
    {
        var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = name;
        ring.transform.SetParent(parent, false);
        ring.transform.localPosition = localPosition;
        ring.transform.localScale = new Vector3(radius * 2f, height, radius * 2f);
        Object.Destroy(ring.GetComponent<Collider>());
        Level8VisualMaterialUtility.ApplyColor(ring.GetComponent<Renderer>(), color, transparent: true);
        return ring;
    }

    public static GameObject CreatePickupSparkleMarker(Transform parent, Color accent)
    {
        var marker = new GameObject("PickupMarker");
        marker.transform.SetParent(parent, false);
        marker.transform.localPosition = Vector3.up * 0.35f;

        var spike = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        spike.name = "SparkleSpike";
        spike.transform.SetParent(marker.transform, false);
        spike.transform.localPosition = Vector3.up * 0.25f;
        spike.transform.localScale = new Vector3(0.06f, 0.25f, 0.06f);
        Object.Destroy(spike.GetComponent<Collider>());
        Level8VisualMaterialUtility.ApplyColor(spike.GetComponent<Renderer>(), accent);

        CreateGlowRing(marker.transform, "PickupRing", Vector3.zero, new Color(accent.r, accent.g, accent.b, 0.35f), 0.35f, 0.04f);
        return marker;
    }

    public static GameObject CreateDistanceLabelAnchor(
        Transform parent,
        string text,
        Vector3 localOffset,
        float maxDistance,
        float characterSize = 0.13f,
        bool alwaysVisible = false)
    {
        var anchor = new GameObject("LabelAnchor");
        anchor.transform.SetParent(parent, false);
        anchor.transform.localPosition = localOffset;

        ItemWorldLabel.Create(anchor.transform, text, Vector3.zero, characterSize);
        var distanceLabel = anchor.AddComponent<Level8DistanceLabel>();
        distanceLabel.Configure(maxDistance, alwaysVisible);
        return anchor;
    }

    public static void ApplyResourceCategoryVisual(Transform pickupRoot, ItemKind kind)
    {
        if (pickupRoot == null)
        {
            return;
        }

        ResolveResourceVisual(kind, out PrimitiveType primitive, out Color bodyColor, out Color accentColor, out Vector3 scale);

        Transform visual = pickupRoot.Find("Level8ResourceVisual");
        if (visual != null)
        {
            Object.Destroy(visual.gameObject);
        }

        var visualRoot = new GameObject("Level8ResourceVisual");
        visualRoot.transform.SetParent(pickupRoot, false);
        visualRoot.transform.localPosition = Vector3.up * 0.15f;

        var body = GameObject.CreatePrimitive(primitive);
        body.name = "Body";
        body.transform.SetParent(visualRoot.transform, false);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = scale;
        Object.Destroy(body.GetComponent<Collider>());
        Level8VisualMaterialUtility.ApplyColor(body.GetComponent<Renderer>(), bodyColor);

        if (kind == ItemKind.OreFragment || kind == ItemKind.Coal)
        {
            var accent = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            accent.name = "Accent";
            accent.transform.SetParent(visualRoot.transform, false);
            accent.transform.localPosition = new Vector3(0.12f, 0.12f, 0.1f);
            accent.transform.localScale = Vector3.one * 0.18f;
            Object.Destroy(accent.GetComponent<Collider>());
            Level8VisualMaterialUtility.ApplyColor(accent.GetComponent<Renderer>(), accentColor);
        }

        CreatePickupSparkleMarker(visualRoot.transform, accentColor);
        pickupRoot.transform.localScale = Vector3.one * Level8VisualHierarchyFlags.ResourcePickupScale;
    }

    public static void ResolveResourceVisual(
        ItemKind kind,
        out PrimitiveType primitive,
        out Color bodyColor,
        out Color accentColor,
        out Vector3 scale)
    {
        switch (kind)
        {
            case ItemKind.Wood:
                primitive = PrimitiveType.Cylinder;
                bodyColor = new Color(0.45f, 0.28f, 0.14f);
                accentColor = new Color(0.55f, 0.36f, 0.18f);
                scale = new Vector3(0.35f, 0.55f, 0.35f);
                break;
            case ItemKind.Stone:
                primitive = PrimitiveType.Sphere;
                bodyColor = new Color(0.52f, 0.54f, 0.58f);
                accentColor = new Color(0.62f, 0.64f, 0.68f);
                scale = new Vector3(0.55f, 0.45f, 0.5f);
                break;
            case ItemKind.Fiber:
            case ItemKind.Grass:
                primitive = PrimitiveType.Cube;
                bodyColor = new Color(0.28f, 0.62f, 0.32f);
                accentColor = new Color(0.35f, 0.72f, 0.38f);
                scale = new Vector3(0.45f, 0.35f, 0.45f);
                break;
            case ItemKind.OreFragment:
                primitive = PrimitiveType.Cube;
                bodyColor = new Color(0.32f, 0.34f, 0.38f);
                accentColor = new Color(0.55f, 0.75f, 0.95f);
                scale = new Vector3(0.42f, 0.38f, 0.42f);
                break;
            case ItemKind.Coal:
                primitive = PrimitiveType.Cube;
                bodyColor = new Color(0.12f, 0.12f, 0.14f);
                accentColor = new Color(0.28f, 0.28f, 0.32f);
                scale = new Vector3(0.4f, 0.35f, 0.4f);
                break;
            default:
                primitive = PrimitiveType.Cube;
                bodyColor = new Color(0.5f, 0.5f, 0.55f);
                accentColor = new Color(0.65f, 0.65f, 0.7f);
                scale = new Vector3(0.4f, 0.4f, 0.4f);
                break;
        }
    }

    public static float SampleDecorationScale(System.Random rng)
    {
        float t = (float)rng.NextDouble();
        return Mathf.Lerp(
            Level8VisualHierarchyFlags.DecorationScaleMin,
            Level8VisualHierarchyFlags.DecorationScaleMax,
            t);
    }
}
