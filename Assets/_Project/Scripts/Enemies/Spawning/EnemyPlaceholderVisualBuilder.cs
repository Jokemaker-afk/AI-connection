using UnityEngine;

public static class EnemyPlaceholderVisualBuilder
{
    public static Transform BuildVisual(EnemyData data, Transform parent)
    {
        var visualRoot = new GameObject("VisualRoot");
        visualRoot.transform.SetParent(parent, false);
        visualRoot.transform.localPosition = Vector3.zero;

        PrimitiveType primitive = data.Kind == EnemyKind.TrainingRangedTarget
            ? PrimitiveType.Cylinder
            : PrimitiveType.Capsule;

        var body = GameObject.CreatePrimitive(primitive);
        body.name = "Body";
        body.transform.SetParent(visualRoot.transform, false);
        body.transform.localPosition = Vector3.up * (data.PlaceholderScale.y * 0.5f);
        body.transform.localScale = data.PlaceholderScale;

        var renderer = body.GetComponent<Renderer>();
        if (renderer != null)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = data.PlaceholderColor;
            renderer.sharedMaterial = material;
        }

        Object.Destroy(body.GetComponent<Collider>());

        if (data.Kind == EnemyKind.TrainingDummy)
        {
            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            head.transform.SetParent(visualRoot.transform, false);
            head.transform.localPosition = Vector3.up * (data.PlaceholderScale.y + 0.25f);
            head.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);
            var headRenderer = head.GetComponent<Renderer>();
            if (headRenderer != null)
            {
                var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                material.color = Color.Lerp(data.PlaceholderColor, Color.white, 0.25f);
                headRenderer.sharedMaterial = material;
            }

            Object.Destroy(head.GetComponent<Collider>());
        }

        ItemWorldLabel.Create(visualRoot.transform, data.DisplayNameChinese, Vector3.up * (data.PlaceholderScale.y + 0.55f), 0.1f);
        return visualRoot.transform;
    }
}
