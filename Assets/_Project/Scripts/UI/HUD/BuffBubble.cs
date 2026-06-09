using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class BuffBubble : MonoBehaviour
{
    const string VisualRootName = "VisualRoot";
    const string LabelAnchorName = "LabelAnchor";
    const string InteractionPointName = "InteractionPoint";

    [SerializeField] BuffType buffType = BuffType.Heal;
    [SerializeField] bool oneShot = true;
    [SerializeField] float bobAmplitude = 0.18f;
    [SerializeField] float bobSpeed = 2.2f;
    [SerializeField] float spinSpeed = 45f;

    bool collected;
    bool moduleVisualBuilt;
    Vector3 basePosition;

    void Awake()
    {
        SetupCollider();
        basePosition = transform.position;
    }

    void Start()
    {
        EnsureModuleVisual();
    }

    void Update()
    {
        if (collected)
        {
            return;
        }

        float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        transform.position = basePosition + Vector3.up * bob;
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        TryCollect(other);
    }

    void OnTriggerStay(Collider other)
    {
        TryCollect(other);
    }

    void TryCollect(Collider other)
    {
        if (collected && oneShot)
        {
            return;
        }

        var buffController = other.GetComponent<PlayerBuffController>()
            ?? other.GetComponentInParent<PlayerBuffController>();
        if (buffController == null)
        {
            return;
        }

        if (!buffController.TryApplyBuff(buffType))
        {
            return;
        }

        collected = true;
        RegisterBuffProgress(buffType);
        if (oneShot)
        {
            gameObject.SetActive(false);
        }
    }

    void RegisterBuffProgress(BuffType type)
    {
        var tracker = BuffCollectionTracker.Instance != null
            ? BuffCollectionTracker.Instance
            : FindFirstObjectByType<BuffCollectionTracker>();
        tracker?.RegisterCollected(type);
    }

    public static BuffBubble Create(Transform parent, string name, Vector3 position, BuffType type)
    {
        var root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = position;

        var bubble = root.AddComponent<BuffBubble>();
        bubble.Initialize(type);
        return bubble;
    }

    public void Initialize(BuffType type)
    {
        buffType = type;
        basePosition = transform.position;
        EnsureModuleVisual();
    }

    void EnsureModuleVisual()
    {
        if (moduleVisualBuilt)
        {
            return;
        }

        MigrateLegacyVisualChildren();
        EnsureLabelAnchor();
        EnsureInteractionPoint();
        BuildModuleVisual();
    }

    void MigrateLegacyVisualChildren()
    {
        if (transform.Find(VisualRootName) != null)
        {
            return;
        }

        DestroyLegacyChild("BubbleShell");
        DestroyLegacyChild("BubbleCore");
        DestroyLegacyChild("BubbleRing");
    }

    void DestroyLegacyChild(string childName)
    {
        Transform legacy = transform.Find(childName);
        if (legacy != null && legacy.parent == transform)
        {
            Destroy(legacy.gameObject);
        }
    }

    void BuildModuleVisual()
    {
        Transform visualRoot = EnsureVisualRootTransform();
        SceneModuleVisualUtility.ClearVisualChildren(visualRoot);

        SceneModuleKind? moduleKind = BuffStationPrefabCatalog.MapBuffTypeToModuleKind(buffType);
        if (moduleKind.HasValue
            && SceneModuleVisualResolver.TryResolveSceneModulePrefab(moduleKind.Value, out GameObject prefab)
            && prefab != null)
        {
            SceneModuleVisualUtility.InstantiateModuleVisual(prefab, visualRoot);
        }
        else
        {
            BuildPrimitiveBuffStationVisuals(visualRoot, buffType);
        }

        moduleVisualBuilt = true;
    }

    Transform EnsureVisualRootTransform()
    {
        Transform visualRoot = transform.Find(VisualRootName);
        if (visualRoot != null)
        {
            return visualRoot;
        }

        var visualRootGo = new GameObject(VisualRootName);
        visualRootGo.transform.SetParent(transform, false);
        visualRootGo.transform.localPosition = Vector3.zero;
        visualRootGo.transform.localRotation = Quaternion.identity;
        visualRootGo.transform.localScale = Vector3.one;
        return visualRootGo.transform;
    }

    void EnsureLabelAnchor()
    {
        Transform labelAnchor = transform.Find(LabelAnchorName);
        if (labelAnchor != null)
        {
            return;
        }

        var labelGo = new GameObject(LabelAnchorName);
        labelGo.transform.SetParent(transform, false);
        labelGo.transform.localPosition = new Vector3(0f, 1.8f, 0f);
    }

    void EnsureInteractionPoint()
    {
        Transform interactionPoint = transform.Find(InteractionPointName);
        if (interactionPoint != null)
        {
            return;
        }

        var pointGo = new GameObject(InteractionPointName);
        pointGo.transform.SetParent(transform, false);
        pointGo.transform.localPosition = new Vector3(0f, 0.8f, 0f);
    }

    static void BuildPrimitiveBuffStationVisuals(Transform visualRoot, BuffType type)
    {
        var outer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        outer.name = "BubbleShell";
        outer.transform.SetParent(visualRoot, false);
        outer.transform.localPosition = Vector3.zero;
        outer.transform.localScale = Vector3.one * 1.6f;
        outer.isStatic = true;
        Object.Destroy(outer.GetComponent<Collider>());

        var shellRenderer = outer.GetComponent<Renderer>();
        if (shellRenderer != null)
        {
            var shellMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            Color tint = GetBubbleColor(type);
            shellMat.color = new Color(tint.r, tint.g, tint.b, 0.35f);
            shellMat.SetFloat("_Surface", 1f);
            shellMat.SetFloat("_Blend", 0f);
            shellMat.renderQueue = 3000;
            shellMat.SetColor("_EmissionColor", tint * 0.35f);
            shellMat.EnableKeyword("_EMISSION");
            shellRenderer.sharedMaterial = shellMat;
        }

        var core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        core.name = "BubbleCore";
        core.transform.SetParent(visualRoot, false);
        core.transform.localPosition = Vector3.zero;
        core.transform.localScale = Vector3.one * 0.55f;
        core.isStatic = true;
        Object.Destroy(core.GetComponent<Collider>());

        var coreRenderer = core.GetComponent<Renderer>();
        if (coreRenderer != null)
        {
            var coreMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            Color tint = GetBubbleColor(type);
            coreMat.color = tint;
            coreMat.SetColor("_EmissionColor", tint * 1.2f);
            coreMat.EnableKeyword("_EMISSION");
            coreRenderer.sharedMaterial = coreMat;
        }

        var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "BubbleRing";
        ring.transform.SetParent(visualRoot, false);
        ring.transform.localPosition = Vector3.zero;
        ring.transform.localScale = new Vector3(1.1f, 0.02f, 1.1f);
        ring.isStatic = true;
        Object.Destroy(ring.GetComponent<Collider>());

        var ringRenderer = ring.GetComponent<Renderer>();
        if (ringRenderer != null)
        {
            var ringMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            ringMat.color = new Color(1f, 1f, 1f, 0.55f);
            ringRenderer.sharedMaterial = ringMat;
        }
    }

    void SetupCollider()
    {
        var sphere = GetComponent<SphereCollider>();
        sphere.isTrigger = true;
        sphere.radius = 1.1f;
        sphere.center = Vector3.zero;
    }

    static Color GetBubbleColor(BuffType type)
    {
        switch (type)
        {
            case BuffType.Heal:
                return new Color(0.35f, 0.95f, 0.45f);
            case BuffType.SpeedBoost:
                return new Color(0.35f, 0.75f, 1f);
            case BuffType.InfiniteStamina:
                return new Color(1f, 0.85f, 0.2f);
            case BuffType.Shield:
                return new Color(0.75f, 0.55f, 1f);
            default:
                return Color.white;
        }
    }
}
