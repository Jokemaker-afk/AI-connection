using UnityEngine;

[DisallowMultipleComponent]
public class DataCoreCollectible : MonoBehaviour
{
    [SerializeField] int dataCoreIndex = 1;
    [SerializeField] string displayNameChinese = "数据核心";
    [SerializeField] float bobAmplitude = 0.12f;
    [SerializeField] float bobSpeed = 2f;
    [SerializeField] float spinSpeed = 24f;

    Transform visualRoot;
    Level8BiomeKind biomeKind = Level8BiomeKind.Plain;
    Vector3 basePosition;
    bool collected;

    public int DataCoreIndex => dataCoreIndex;
    public string DisplayNameChinese => displayNameChinese;
    public bool CanBeCollected => !collected;
    public bool IsCollected => collected;

    public void Configure(
        int index,
        Color glowColor,
        Level8ChunkKind hostKind,
        Vector2Int gridPosition,
        Level8BiomeKind biome = Level8BiomeKind.Plain)
    {
        dataCoreIndex = index;
        biomeKind = biome;
        name = $"DataCore_{index}";
        basePosition = transform.position;

        ObjectiveCollectibleSetup.EnsureColliders(gameObject);
        BuildVisuals(glowColor, hostKind);
        RefreshLabel();

        Debug.Log($"[Level8] Data Core #{index} placed at {basePosition} (chunk {gridPosition.x},{gridPosition.y}).");
    }

    void BuildVisuals(Color glowColor, Level8ChunkKind hostKind)
    {
        ClearVisualRoot();

        visualRoot = new GameObject("Visual").transform;
        visualRoot.SetParent(transform, false);
        visualRoot.localPosition = Vector3.zero;

        if (TryBuildPrefabVisuals())
        {
            return;
        }

        BuildPrimitiveDataCoreVisuals(visualRoot, glowColor, hostKind);
    }

    void ClearVisualRoot()
    {
        if (visualRoot != null)
        {
            Destroy(visualRoot.gameObject);
            visualRoot = null;
        }
    }

    bool TryBuildPrefabVisuals()
    {
        if (!Level8VisualAssetResolver.TryResolveDataCorePrefab(biomeKind, out GameObject prefab) || prefab == null)
        {
            return false;
        }

        Level8ObjectiveVisualUtility.InstantiateVisualPrefab(prefab, visualRoot);
        return true;
    }

    static void BuildPrimitiveDataCoreVisuals(Transform parent, Color glowColor, Level8ChunkKind hostKind)
    {
        Color coreColor = Color.Lerp(glowColor, new Color(0.35f, 0.9f, 1f), 0.35f);

        Level8VisualHierarchyUtility.CreateGlowRing(
            parent,
            "BaseRing",
            new Vector3(0f, 0.05f, 0f),
            new Color(coreColor.r, coreColor.g, coreColor.b, 0.45f),
            1.4f,
            0.08f);

        var pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pedestal.name = "Pedestal";
        pedestal.transform.SetParent(parent, false);
        pedestal.transform.localPosition = new Vector3(0f, 0.25f, 0f);
        pedestal.transform.localScale = new Vector3(1.6f, 0.35f, 1.6f);
        Object.Destroy(pedestal.GetComponent<Collider>());
        Level8VisualMaterialUtility.ApplyColor(pedestal.GetComponent<Renderer>(), Color.Lerp(coreColor, Color.gray, 0.4f));

        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "GlowCore";
        sphere.transform.SetParent(parent, false);
        sphere.transform.localPosition = new Vector3(0f, 0.95f, 0f);
        sphere.transform.localScale = Vector3.one * 1.75f;
        Object.Destroy(sphere.GetComponent<Collider>());
        Level8VisualMaterialUtility.ApplyColor(sphere.GetComponent<Renderer>(), coreColor);

        Level8VisualHierarchyUtility.CreateVerticalBeam(
            parent,
            "ObjectiveBeam",
            new Vector3(0f, 1.2f, 0f),
            new Color(coreColor.r, coreColor.g, coreColor.b, 0.55f),
            height: 5.5f,
            radius: 0.14f,
            transparent: true);

        if (hostKind == Level8ChunkKind.Resource)
        {
            AddDecorationCube(parent, new Vector3(0.8f, 0.1f, 0.6f), new Vector3(0.35f, 0.45f, 0.35f), new Color(0.35f, 0.28f, 0.18f));
        }

        if (hostKind == Level8ChunkKind.Hazard)
        {
            AddDecorationCube(parent, new Vector3(-0.6f, 0.15f, -0.5f), new Vector3(0.5f, 0.4f, 0.4f), new Color(0.42f, 0.44f, 0.48f));
        }
    }

    static void AddDecorationCube(Transform parent, Vector3 localPos, Vector3 scale, Color color)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.SetParent(parent, false);
        cube.transform.localPosition = localPos;
        cube.transform.localScale = scale;
        Object.Destroy(cube.GetComponent<Collider>());
        Level8VisualMaterialUtility.ApplyColor(cube.GetComponent<Renderer>(), color);
    }

    void RefreshLabel()
    {
        Transform existing = transform.Find("LabelAnchor");
        if (existing != null)
        {
            Destroy(existing.gameObject);
        }

        Level8VisualHierarchyUtility.CreateDistanceLabelAnchor(
            transform,
            displayNameChinese,
            new Vector3(0f, 2.6f, 0f),
            Level8VisualHierarchyFlags.DataCoreLabelDistance,
            characterSize: 0.14f);
    }

    public Vector3 GetInteractionPoint()
    {
        Transform interaction = transform.Find("InteractionPoint");
        if (interaction != null)
        {
            return interaction.position;
        }

        return transform.position + Vector3.up * 0.9f;
    }

    void Update()
    {
        if (collected || visualRoot == null)
        {
            return;
        }

        float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        visualRoot.localPosition = Vector3.up * bob;
        visualRoot.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
    }

    public bool TryCollect()
    {
        if (collected)
        {
            return false;
        }

        collected = true;
        Level8ObjectiveTracker tracker = Level8ObjectiveTracker.Instance;
        tracker?.RegisterDataCoreCollected();
        Destroy(gameObject);
        return true;
    }
}
