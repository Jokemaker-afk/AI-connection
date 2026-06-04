using UnityEngine;

public class HubAdvancePortal : MonoBehaviour
{
    static Vector3 portalPosition = new Vector3(0f, 1.35f, 2.5f);

    [SerializeField] Vector3 localPortalOffset = new Vector3(0f, 1.35f, 2.5f);

    GameObject portalRoot;
    bool portalVisible;

    public static void SetPortalPosition(Vector3 worldPosition)
    {
        portalPosition = worldPosition;
    }

    void Awake()
    {
        SubscribeTracker(FindTracker());
    }

    void OnEnable()
    {
        var tracker = FindTracker();
        SubscribeTracker(tracker);
        if (tracker != null && tracker.AllCollected)
        {
            ShowPortal();
        }
    }

    void OnDisable()
    {
        var tracker = FindTracker();
        if (tracker != null)
        {
            tracker.OnAllBuffsCollected -= ShowPortal;
        }
    }

    void SubscribeTracker(BuffCollectionTracker tracker)
    {
        if (tracker == null)
        {
            return;
        }

        tracker.OnAllBuffsCollected -= ShowPortal;
        tracker.OnAllBuffsCollected += ShowPortal;
    }

    static BuffCollectionTracker FindTracker()
    {
        return BuffCollectionTracker.Instance != null
            ? BuffCollectionTracker.Instance
            : FindFirstObjectByType<BuffCollectionTracker>();
    }

    public void Configure(Vector3 worldPosition)
    {
        portalPosition = worldPosition;
        localPortalOffset = transform.InverseTransformPoint(worldPosition);
    }

    void ShowPortal()
    {
        if (!portalVisible)
        {
            portalVisible = true;
            portalRoot = BuildPortalVisual(GetPortalWorldPosition());
            BuffNotificationOverlay.ShowCustom("四 Buff 已集齐！", "返回出生点，进入传送门前往下一关");
        }

        // User expects immediate center prompt after collecting 4 buffs.
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.NotifyGoalReached();
        }
    }

    Vector3 GetPortalWorldPosition()
    {
        if (transform.parent == null)
        {
            return portalPosition;
        }

        return transform.TransformPoint(localPortalOffset);
    }

    static GameObject BuildPortalVisual(Vector3 position)
    {
        var root = new GameObject("AdvancePortal");
        root.transform.position = position;

        var platform = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        platform.name = "PortalPlatform";
        platform.transform.SetParent(root.transform, false);
        platform.transform.localPosition = Vector3.zero;
        platform.transform.localScale = new Vector3(2.6f, 0.08f, 2.6f);
        platform.isStatic = true;
        Object.Destroy(platform.GetComponent<Collider>());

        ApplyEmissiveMaterial(platform.GetComponent<Renderer>(), new Color(0.2f, 0.85f, 0.95f), 1.2f);

        var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "PortalRing";
        ring.transform.SetParent(root.transform, false);
        ring.transform.localPosition = new Vector3(0f, 0.9f, 0f);
        ring.transform.localScale = new Vector3(2.2f, 1.6f, 2.2f);
        ring.isStatic = true;
        Object.Destroy(ring.GetComponent<Collider>());

        ApplyEmissiveMaterial(ring.GetComponent<Renderer>(), new Color(0.35f, 0.55f, 1f, 0.55f), 1.8f);

        var core = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        core.name = "PortalCore";
        core.transform.SetParent(root.transform, false);
        core.transform.localPosition = new Vector3(0f, 0.9f, 0f);
        core.transform.localScale = new Vector3(1.2f, 1.5f, 1.2f);
        core.isStatic = true;
        Object.Destroy(core.GetComponent<Collider>());

        ApplyEmissiveMaterial(core.GetComponent<Renderer>(), new Color(0.55f, 0.95f, 1f, 0.35f), 2.4f);

        var triggerGo = new GameObject("PortalTrigger");
        triggerGo.transform.SetParent(root.transform, false);
        triggerGo.transform.localPosition = new Vector3(0f, 1.1f, 0f);

        var box = triggerGo.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = new Vector3(2.4f, 2.4f, 2.4f);

        triggerGo.AddComponent<LevelGoal>();

        root.AddComponent<HubAdvancePortalSpinner>();

        CreateSign(root.transform, new Vector3(0f, 2.6f, 0f), "下一关传送点\n按 Y 确认进入");

        return root;
    }

    static void CreateSign(Transform parent, Vector3 localPosition, string text)
    {
        var sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sign.name = "PortalSign";
        sign.transform.SetParent(parent, false);
        sign.transform.localPosition = localPosition;
        sign.transform.localScale = new Vector3(3.8f, 0.8f, 0.12f);
        sign.isStatic = true;
        Object.Destroy(sign.GetComponent<Collider>());

        var renderer = sign.GetComponent<Renderer>();
        if (renderer != null)
        {
            ApplyEmissiveMaterial(renderer, new Color(0.12f, 0.16f, 0.22f), 0.2f);
        }
    }

    static void ApplyEmissiveMaterial(Renderer renderer, Color color, float emissionStrength)
    {
        if (renderer == null)
        {
            return;
        }

        var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.color = color;
        material.SetColor("_EmissionColor", new Color(color.r, color.g, color.b) * emissionStrength);
        material.EnableKeyword("_EMISSION");
        if (color.a < 0.99f)
        {
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.renderQueue = 3000;
        }

        renderer.sharedMaterial = material;
    }

    sealed class HubAdvancePortalSpinner : MonoBehaviour
    {
        const float SpinSpeed = 55f;

        Transform ring;
        Transform core;

        void Awake()
        {
            ring = transform.Find("PortalRing");
            core = transform.Find("PortalCore");
        }

        void Update()
        {
            if (ring != null)
            {
                ring.Rotate(Vector3.up, SpinSpeed * Time.deltaTime, Space.World);
            }

            if (core != null)
            {
                core.Rotate(Vector3.up, -SpinSpeed * 0.65f * Time.deltaTime, Space.World);
            }
        }
    }
}
