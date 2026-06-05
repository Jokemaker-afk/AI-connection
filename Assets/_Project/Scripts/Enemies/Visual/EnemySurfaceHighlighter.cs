using UnityEngine;

/// <summary>
/// Renderer-based surface highlight via MaterialPropertyBlock — no cube outline.
/// </summary>
[DisallowMultipleComponent]
public class EnemySurfaceHighlighter : MonoBehaviour
{
    [SerializeField] Color primaryHighlightColor = new Color(1f, 0.92f, 0.15f);
    [SerializeField] Color secondaryHighlightColor = new Color(1f, 0.72f, 0.28f);
    [SerializeField] float primaryIntensity = 2.5f;
    [SerializeField] float secondaryIntensity = 1.2f;
    [SerializeField] float primaryTintBlend = 0.72f;
    [SerializeField] float secondaryTintBlend = 0.45f;
    [SerializeField] bool useEmissionHighlight = true;
    [SerializeField] bool useColorTintFallback = true;
    [SerializeField] bool pulseHighlight = true;
    [SerializeField] float pulseSpeed = 4f;
    [SerializeField] float pulseAmount = 0.18f;

    Renderer[] targetRenderers;
    MaterialPropertyBlock propertyBlock;
    Color[] cachedBaseColors;
    EnemyHighlightLevel currentLevel = EnemyHighlightLevel.None;
    bool highlightApplied;

    public EnemyHighlightLevel CurrentLevel => currentLevel;
    public int RendererCount => targetRenderers != null ? targetRenderers.Length : 0;
    public bool HighlightApplied => highlightApplied;

    public void BindVisualRoot(Transform visualRoot)
    {
        Transform searchRoot = visualRoot != null ? visualRoot : transform;
        Renderer[] allRenderers = searchRoot.GetComponentsInChildren<Renderer>(true);
        if (allRenderers == null || allRenderers.Length == 0)
        {
            allRenderers = GetComponentsInChildren<Renderer>(true);
        }

        var filtered = new System.Collections.Generic.List<Renderer>();
        for (int i = 0; i < allRenderers.Length; i++)
        {
            Renderer renderer = allRenderers[i];
            if (renderer == null || IsUtilityRenderer(renderer))
            {
                continue;
            }

            filtered.Add(renderer);
        }

        targetRenderers = filtered.ToArray();
        CacheBaseColors();
        highlightApplied = false;
    }

    void Awake()
    {
        propertyBlock = new MaterialPropertyBlock();
        Transform visualRoot = transform.Find("VisualRoot");
        BindVisualRoot(visualRoot != null ? visualRoot : transform);
    }

    void LateUpdate()
    {
        if (currentLevel == EnemyHighlightLevel.None)
        {
            return;
        }

        ApplySurfaceHighlight(GetHighlightColor(), GetIntensity(), GetTintBlend());
    }

    public void SetHighlight(EnemyHighlightLevel level)
    {
        if (level == EnemyHighlightLevel.None)
        {
            if (currentLevel != EnemyHighlightLevel.None)
            {
                RestoreBaseColors();
            }

            currentLevel = EnemyHighlightLevel.None;
            highlightApplied = false;
            return;
        }

        if (targetRenderers == null || targetRenderers.Length == 0)
        {
            Transform visualRoot = transform.Find("VisualRoot");
            BindVisualRoot(visualRoot != null ? visualRoot : transform);
        }

        currentLevel = level;
        Color highlight = GetHighlightColor();
        ApplySurfaceHighlight(highlight, GetIntensity(), GetTintBlend());
    }

    public void ClearHighlight()
    {
        SetHighlight(EnemyHighlightLevel.None);
    }

    public void RefreshHighlight()
    {
        if (currentLevel == EnemyHighlightLevel.None)
        {
            return;
        }

        ApplySurfaceHighlight(GetHighlightColor(), GetIntensity(), GetTintBlend());
    }

    Color GetHighlightColor()
    {
        Color baseColor = currentLevel == EnemyHighlightLevel.Primary ? primaryHighlightColor : secondaryHighlightColor;
        if (!pulseHighlight || currentLevel == EnemyHighlightLevel.None)
        {
            return baseColor;
        }

        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        return baseColor * pulse;
    }

    float GetIntensity()
    {
        return currentLevel == EnemyHighlightLevel.Primary ? primaryIntensity : secondaryIntensity;
    }

    float GetTintBlend()
    {
        return currentLevel == EnemyHighlightLevel.Primary ? primaryTintBlend : secondaryTintBlend;
    }

    void CacheBaseColors()
    {
        if (targetRenderers == null)
        {
            return;
        }

        cachedBaseColors = new Color[targetRenderers.Length];
        for (int i = 0; i < targetRenderers.Length; i++)
        {
            Renderer renderer = targetRenderers[i];
            if (renderer == null || renderer.sharedMaterial == null)
            {
                continue;
            }

            Material material = renderer.sharedMaterial;
            if (material.HasProperty("_BaseColor"))
            {
                cachedBaseColors[i] = material.GetColor("_BaseColor");
            }
            else if (material.HasProperty("_Color"))
            {
                cachedBaseColors[i] = material.GetColor("_Color");
            }
            else
            {
                cachedBaseColors[i] = material.color;
            }
        }
    }

    void ApplySurfaceHighlight(Color highlight, float intensity, float tintBlend)
    {
        if (targetRenderers == null || targetRenderers.Length == 0 || propertyBlock == null)
        {
            highlightApplied = false;
            return;
        }

        bool appliedAny = false;
        for (int i = 0; i < targetRenderers.Length; i++)
        {
            Renderer renderer = targetRenderers[i];
            if (renderer == null)
            {
                continue;
            }

            Color baseColor = cachedBaseColors != null && i < cachedBaseColors.Length
                ? cachedBaseColors[i]
                : Color.white;

            renderer.GetPropertyBlock(propertyBlock);

            if (useColorTintFallback)
            {
                Color tinted = Color.Lerp(baseColor, highlight, tintBlend);
                if (renderer.sharedMaterial != null)
                {
                    if (renderer.sharedMaterial.HasProperty("_BaseColor"))
                    {
                        propertyBlock.SetColor("_BaseColor", tinted);
                    }

                    if (renderer.sharedMaterial.HasProperty("_Color"))
                    {
                        propertyBlock.SetColor("_Color", tinted);
                    }
                }
            }

            if (useEmissionHighlight && renderer.sharedMaterial != null && renderer.sharedMaterial.HasProperty("_EmissionColor"))
            {
                propertyBlock.SetColor("_EmissionColor", highlight * intensity);
            }

            renderer.SetPropertyBlock(propertyBlock);
            appliedAny = true;
        }

        highlightApplied = appliedAny;
    }

    void RestoreBaseColors()
    {
        if (targetRenderers == null || propertyBlock == null)
        {
            return;
        }

        for (int i = 0; i < targetRenderers.Length; i++)
        {
            Renderer renderer = targetRenderers[i];
            if (renderer == null)
            {
                continue;
            }

            renderer.SetPropertyBlock(null);
        }

        highlightApplied = false;
    }

    static bool IsUtilityRenderer(Renderer renderer)
    {
        string name = renderer.gameObject.name;
        return name.Contains("Label")
            || name.Contains("Outline")
            || name.Contains("CombatTargetOutline");
    }
}
