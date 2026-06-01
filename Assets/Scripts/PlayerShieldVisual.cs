using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(PlayerStats))]
public class PlayerShieldVisual : MonoBehaviour
{
    [SerializeField] Vector3 localOffset = new Vector3(0f, 1f, 0f);
    [SerializeField] float baseDiameter = 2.5f;
    [SerializeField] float pulseAmplitude = 0.06f;
    [SerializeField] float pulseSpeed = 2.8f;
    [SerializeField] float spinSpeed = 40f;
    [SerializeField] float breakDuration = 0.35f;

    const float ShieldAlpha = 0.8f;

    static readonly Color ShellColor = new Color(0.35f, 0.72f, 1f, ShieldAlpha);
    static readonly Color ShellEmission = new Color(0.2f, 0.55f, 1f);
    static readonly Color GlowColor = new Color(0.5f, 0.85f, 1f, 0.45f);

    PlayerStats stats;
    Transform shieldRoot;
    Material shellMaterial;
    Material glowMaterial;
    bool isVisible;
    Coroutine breakRoutine;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
        BuildShieldVisual();
        SetShieldVisible(false);
    }

    void Start()
    {
        BindStatsEvents();
        if (stats != null && stats.HasShield)
        {
            ShowShield();
        }
    }

    void OnEnable()
    {
        BindStatsEvents();
    }

    void OnDisable()
    {
        if (stats != null)
        {
            stats.OnShieldStateChanged -= HandleShieldStateChanged;
        }
    }

    void BindStatsEvents()
    {
        if (stats == null)
        {
            stats = GetComponent<PlayerStats>();
        }

        if (stats == null)
        {
            return;
        }

        stats.OnShieldStateChanged -= HandleShieldStateChanged;
        stats.OnShieldStateChanged += HandleShieldStateChanged;
    }

    void LateUpdate()
    {
        if (!isVisible || shieldRoot == null)
        {
            return;
        }

        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmplitude;
        shieldRoot.localScale = Vector3.one * baseDiameter * pulse;
        shieldRoot.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.Self);
    }

    public void ShowShield()
    {
        HandleShieldStateChanged(true, false);
    }

    public void HideShield(bool playBreakEffect)
    {
        HandleShieldStateChanged(false, playBreakEffect);
    }

    void HandleShieldStateChanged(bool hasShield, bool playBreakEffect)
    {
        if (hasShield)
        {
            if (breakRoutine != null)
            {
                StopCoroutine(breakRoutine);
                breakRoutine = null;
            }

            ResetShieldAppearance();
            SetShieldVisible(true);
            return;
        }

        if (!isVisible)
        {
            return;
        }

        if (playBreakEffect)
        {
            breakRoutine = StartCoroutine(PlayBreakEffect());
        }
        else
        {
            SetShieldVisible(false);
        }
    }

    IEnumerator PlayBreakEffect()
    {
        isVisible = false;
        float elapsed = 0f;

        while (elapsed < breakDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / breakDuration;
            shieldRoot.localScale = Vector3.one * baseDiameter * Mathf.Lerp(1f, 1.4f, t);

            if (shellMaterial != null)
            {
                shellMaterial.color = Color.Lerp(ShellColor, new Color(ShellColor.r, ShellColor.g, ShellColor.b, 0f), t);
            }

            if (glowMaterial != null)
            {
                glowMaterial.color = Color.Lerp(GlowColor, Color.clear, t);
            }

            yield return null;
        }

        SetShieldVisible(false);
        ResetShieldAppearance();
        breakRoutine = null;
    }

    void BuildShieldVisual()
    {
        if (transform.Find("ShieldVisual") != null)
        {
            shieldRoot = transform.Find("ShieldVisual");
            return;
        }

        shieldRoot = new GameObject("ShieldVisual").transform;
        shieldRoot.SetParent(transform, false);
        shieldRoot.localPosition = localOffset;
        shieldRoot.localRotation = Quaternion.identity;

        var glow = CreateShell("ShieldGlow", Vector3.one * 1.12f, out glowMaterial);
        glow.transform.SetParent(shieldRoot, false);
        ApplyTransparentMaterial(glowMaterial, GlowColor, ShellEmission * 0.6f, additive: true);

        var shell = CreateShell("ShieldShell", Vector3.one, out shellMaterial);
        shell.transform.SetParent(shieldRoot, false);
        ApplyTransparentMaterial(shellMaterial, ShellColor, ShellEmission * 1.4f, additive: false);

        var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "ShieldRing";
        ring.transform.SetParent(shieldRoot, false);
        ring.transform.localPosition = Vector3.zero;
        ring.transform.localRotation = Quaternion.identity;
        ring.transform.localScale = new Vector3(1.08f, 0.02f, 1.08f);
        Object.Destroy(ring.GetComponent<Collider>());

        var ringMat = CreateBaseMaterial();
        ApplyTransparentMaterial(ringMat, new Color(0.8f, 0.95f, 1f, ShieldAlpha), new Color(0.45f, 0.75f, 1f), additive: false);
        ring.GetComponent<Renderer>().sharedMaterial = ringMat;
    }

    static GameObject CreateShell(string name, Vector3 scale, out Material material)
    {
        var shell = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        shell.name = name;
        shell.transform.localPosition = Vector3.zero;
        shell.transform.localRotation = Quaternion.identity;
        shell.transform.localScale = scale;
        Object.Destroy(shell.GetComponent<Collider>());
        material = CreateBaseMaterial();
        shell.GetComponent<Renderer>().sharedMaterial = material;
        return shell;
    }

    static Material CreateBaseMaterial()
    {
        var shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Universal Render Pipeline/Lit");
        }

        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        return new Material(shader);
    }

    static void ApplyTransparentMaterial(Material material, Color color, Color emission, bool additive)
    {
        material.color = color;
        if (material.HasProperty("_EmissionColor"))
        {
            material.SetColor("_EmissionColor", emission);
            material.EnableKeyword("_EMISSION");
        }

        if (material.HasProperty("_Surface"))
        {
            material.SetFloat("_Surface", 1f);
        }

        if (material.HasProperty("_Blend"))
        {
            material.SetFloat("_Blend", additive ? 2f : 0f);
        }

        if (material.HasProperty("_Cull"))
        {
            material.SetFloat("_Cull", (float)CullMode.Off);
        }

        if (material.HasProperty("_ZWrite"))
        {
            material.SetFloat("_ZWrite", 0f);
        }

        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt("_SrcBlend", additive ? (int)BlendMode.SrcAlpha : (int)BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", additive ? (int)BlendMode.One : (int)BlendMode.OneMinusSrcAlpha);
        material.renderQueue = additive ? (int)RenderQueue.Transparent + 10 : (int)RenderQueue.Transparent;
    }

    void ResetShieldAppearance()
    {
        if (shieldRoot == null)
        {
            return;
        }

        shieldRoot.localScale = Vector3.one * baseDiameter;
        if (shellMaterial != null)
        {
            shellMaterial.color = ShellColor;
            if (shellMaterial.HasProperty("_EmissionColor"))
            {
                shellMaterial.SetColor("_EmissionColor", ShellEmission * 1.4f);
            }
        }

        if (glowMaterial != null)
        {
            glowMaterial.color = GlowColor;
        }
    }

    void SetShieldVisible(bool visible)
    {
        isVisible = visible;
        if (shieldRoot != null)
        {
            shieldRoot.gameObject.SetActive(visible);
        }
    }
}
