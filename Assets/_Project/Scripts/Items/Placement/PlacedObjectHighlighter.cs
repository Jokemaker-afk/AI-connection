using UnityEngine;

/// <summary>
/// Distinct placed-object selection feedback (outline), separate from dropped pickup rings.
/// </summary>
[DisallowMultipleComponent]
public class PlacedObjectHighlighter : MonoBehaviour
{
    [SerializeField] float outlineScaleMultiplier = 1.08f;
    [SerializeField] Color outlineColor = new Color(1f, 0.82f, 0.18f, 0.42f);

    GameObject outlineRoot;
    Renderer outlineRenderer;
    bool selected;

    public bool IsSelected => selected;

    public void RefreshFromColliders()
    {
        if (outlineRoot == null)
        {
            BuildOutline();
        }
        else
        {
            ApplyBoundsToOutline();
        }
    }

    public void SetSelected(bool isSelected)
    {
        selected = isSelected;
        if (outlineRoot == null && isSelected)
        {
            RefreshFromColliders();
        }

        if (outlineRoot != null)
        {
            outlineRoot.SetActive(isSelected);
        }
    }

    void BuildOutline()
    {
        if (outlineRoot != null)
        {
            return;
        }

        outlineRoot = new GameObject("SelectionOutline");
        outlineRoot.transform.SetParent(transform, false);

        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "OutlineMesh";
        cube.transform.SetParent(outlineRoot.transform, false);
        Object.Destroy(cube.GetComponent<Collider>());

        outlineRenderer = cube.GetComponent<Renderer>();
        var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.color = outlineColor;
        material.SetFloat("_Surface", 1f);
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.renderQueue = 3100;
        outlineRenderer.sharedMaterial = material;

        ApplyBoundsToOutline();
        outlineRoot.SetActive(selected);
    }

    void ApplyBoundsToOutline()
    {
        if (outlineRoot == null)
        {
            return;
        }

        Transform meshTransform = outlineRoot.transform.GetChild(0);
        Bounds bounds = ComputeLocalBounds();
        meshTransform.localPosition = bounds.center;
        meshTransform.localRotation = Quaternion.identity;
        meshTransform.localScale = Vector3.Scale(bounds.size, Vector3.one * outlineScaleMultiplier);
    }

    Bounds ComputeLocalBounds()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            return new Bounds(box.center, box.size);
        }

        Collider[] colliders = GetComponentsInChildren<Collider>();
        if (colliders.Length == 0)
        {
            return new Bounds(Vector3.up * 0.45f, new Vector3(1.6f, 0.9f, 1.2f));
        }

        Bounds worldBounds = colliders[0].bounds;
        for (int i = 1; i < colliders.Length; i++)
        {
            if (colliders[i] != null && !colliders[i].isTrigger)
            {
                worldBounds.Encapsulate(colliders[i].bounds);
            }
        }

        Vector3 localCenter = transform.InverseTransformPoint(worldBounds.center);
        Vector3 localSize = transform.InverseTransformVector(worldBounds.size);
        localSize = new Vector3(Mathf.Abs(localSize.x), Mathf.Abs(localSize.y), Mathf.Abs(localSize.z));
        return new Bounds(localCenter, localSize);
    }

    void OnDestroy()
    {
        if (outlineRoot != null)
        {
            Destroy(outlineRoot);
        }
    }
}
