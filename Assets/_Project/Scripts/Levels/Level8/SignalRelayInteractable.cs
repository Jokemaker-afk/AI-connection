using UnityEngine;

[DisallowMultipleComponent]
public class SignalRelayInteractable : MonoBehaviour
{
    [SerializeField] string displayNameChinese = "信号中继器";
    [SerializeField] string lockedPromptChinese = "需要收集 3 个数据核心";
    [SerializeField] string activatePromptChinese = "按 E 激活：信号中继器";
    [SerializeField] string activatedLabelChinese = "信号中继器已激活";

    Transform visualRoot;
    Transform beamRoot;
    Renderer baseRenderer;
    Renderer antennaRenderer;
    Renderer platformRenderer;
    Color inactiveAccent;
    Color activeAccent;
    bool activated;

    public string DisplayNameChinese => displayNameChinese;
    public bool IsActivated => activated;
    public bool CanInteract => !activated;

    public void Configure(Vector3 worldPosition, Color accentColor)
    {
        transform.position = worldPosition;
        inactiveAccent = new Color(0.28f, 0.3f, 0.34f);
        activeAccent = Color.Lerp(accentColor, new Color(0.35f, 0.85f, 1f), 0.55f);
        BuildVisuals();
        EnsureCollider();
        RefreshLabel(false);
    }

    void EnsureCollider()
    {
        BoxCollider box = GetComponent<BoxCollider>();
        if (box == null)
        {
            box = gameObject.AddComponent<BoxCollider>();
        }

        box.isTrigger = false;
        box.center = new Vector3(0f, 1.5f, 0f);
        box.size = new Vector3(1.8f, 3.2f, 1.8f);
    }

    void BuildVisuals()
    {
        if (visualRoot != null)
        {
            Destroy(visualRoot.gameObject);
        }

        visualRoot = new GameObject("Visual").transform;
        visualRoot.SetParent(transform, false);

        var platform = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        platform.name = "Platform";
        platform.transform.SetParent(visualRoot, false);
        platform.transform.localPosition = new Vector3(0f, 0.08f, 0f);
        platform.transform.localScale = new Vector3(2.4f, 0.1f, 2.4f);
        Object.Destroy(platform.GetComponent<Collider>());
        platformRenderer = platform.GetComponent<Renderer>();
        Level8VisualMaterialUtility.ApplyColor(platformRenderer, inactiveAccent);

        var baseCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseCube.name = "Base";
        baseCube.transform.SetParent(visualRoot, false);
        baseCube.transform.localPosition = new Vector3(0f, 0.85f, 0f);
        baseCube.transform.localScale = new Vector3(1.2f, 1.5f, 1.2f);
        Object.Destroy(baseCube.GetComponent<Collider>());
        baseRenderer = baseCube.GetComponent<Renderer>();
        Level8VisualMaterialUtility.ApplyColor(baseRenderer, inactiveAccent);

        var antenna = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        antenna.name = "Antenna";
        antenna.transform.SetParent(visualRoot, false);
        antenna.transform.localPosition = new Vector3(0f, 2.2f, 0f);
        antenna.transform.localScale = new Vector3(0.22f, 1.1f, 0.22f);
        Object.Destroy(antenna.GetComponent<Collider>());
        antennaRenderer = antenna.GetComponent<Renderer>();
        Level8VisualMaterialUtility.ApplyColor(antennaRenderer, Color.Lerp(inactiveAccent, Color.gray, 0.2f));

        var dish = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        dish.name = "Dish";
        dish.transform.SetParent(visualRoot, false);
        dish.transform.localPosition = new Vector3(0f, 2.95f, 0f);
        dish.transform.localScale = new Vector3(0.9f, 0.08f, 0.9f);
        Object.Destroy(dish.GetComponent<Collider>());
        Level8VisualMaterialUtility.ApplyColor(dish.GetComponent<Renderer>(), inactiveAccent);

        beamRoot = new GameObject("Beam").transform;
        beamRoot.SetParent(visualRoot, false);
        beamRoot.localPosition = new Vector3(0f, 3.2f, 0f);
        Level8VisualHierarchyUtility.CreateVerticalBeam(
            beamRoot,
            "BeamLight",
            Vector3.zero,
            activeAccent,
            height: 4f,
            radius: 0.1f,
            transparent: true);
        beamRoot.gameObject.SetActive(false);
    }

    void RefreshLabel(bool showActivated)
    {
        Transform existing = transform.Find("LabelAnchor");
        if (existing != null)
        {
            Destroy(existing.gameObject);
        }

        string text = showActivated ? activatedLabelChinese : displayNameChinese;
        Level8VisualHierarchyUtility.CreateDistanceLabelAnchor(
            transform,
            text,
            new Vector3(0f, 4.2f, 0f),
            Level8VisualHierarchyFlags.SignalRelayLabelDistance,
            characterSize: 0.13f);
    }

    public string GetInteractionPrompt()
    {
        if (activated)
        {
            return activatedLabelChinese;
        }

        Level8ObjectiveTracker tracker = Level8ObjectiveTracker.Instance;
        if (tracker == null || !tracker.HasAllDataCores())
        {
            return lockedPromptChinese;
        }

        return activatePromptChinese;
    }

    public bool TryActivate()
    {
        if (activated)
        {
            return false;
        }

        Level8ObjectiveTracker tracker = Level8ObjectiveTracker.Instance;
        if (tracker == null || !tracker.HasAllDataCores())
        {
            return false;
        }

        activated = true;
        ApplyActivatedVisuals();
        tracker.ActivateSignalRelay();
        RefreshLabel(true);
        return true;
    }

    void ApplyActivatedVisuals()
    {
        if (baseRenderer != null)
        {
            Level8VisualMaterialUtility.ApplyColor(baseRenderer, activeAccent);
        }

        if (platformRenderer != null)
        {
            Level8VisualMaterialUtility.ApplyColor(platformRenderer, Color.Lerp(activeAccent, Color.white, 0.15f));
        }

        if (antennaRenderer != null)
        {
            Level8VisualMaterialUtility.ApplyColor(antennaRenderer, Color.Lerp(activeAccent, Color.white, 0.35f));
        }

        if (beamRoot != null)
        {
            beamRoot.gameObject.SetActive(true);
        }
    }
}
