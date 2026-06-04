using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class BuffBubble : MonoBehaviour
{
    [SerializeField] BuffType buffType = BuffType.Heal;
    [SerializeField] bool oneShot = true;
    [SerializeField] float bobAmplitude = 0.18f;
    [SerializeField] float bobSpeed = 2.2f;
    [SerializeField] float spinSpeed = 45f;

    bool collected;
    Vector3 basePosition;

    void Awake()
    {
        var sphere = GetComponent<SphereCollider>();
        sphere.isTrigger = true;
        sphere.radius = 1.1f;
        sphere.center = Vector3.zero;
        basePosition = transform.position;
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

        var outer = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        outer.name = "BubbleShell";
        outer.transform.SetParent(root.transform, false);
        outer.transform.localPosition = Vector3.zero;
        outer.transform.localScale = Vector3.one * 1.6f;
        outer.isStatic = true;
        Object.DestroyImmediate(outer.GetComponent<Collider>());

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
        core.transform.SetParent(root.transform, false);
        core.transform.localPosition = Vector3.zero;
        core.transform.localScale = Vector3.one * 0.55f;
        core.isStatic = true;
        Object.DestroyImmediate(core.GetComponent<Collider>());

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
        ring.transform.SetParent(root.transform, false);
        ring.transform.localPosition = Vector3.zero;
        ring.transform.localScale = new Vector3(1.1f, 0.02f, 1.1f);
        ring.isStatic = true;
        Object.DestroyImmediate(ring.GetComponent<Collider>());

        var ringRenderer = ring.GetComponent<Renderer>();
        if (ringRenderer != null)
        {
            var ringMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            ringMat.color = new Color(1f, 1f, 1f, 0.55f);
            ringRenderer.sharedMaterial = ringMat;
        }

        var bubble = root.AddComponent<BuffBubble>();
        bubble.Initialize(type);
        return bubble;
    }

    public void Initialize(BuffType type)
    {
        buffType = type;
        basePosition = transform.position;
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
