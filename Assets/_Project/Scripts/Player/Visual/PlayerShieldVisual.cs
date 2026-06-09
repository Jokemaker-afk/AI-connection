using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
public class PlayerShieldVisual : MonoBehaviour
{
    const string DefaultShieldVisualResourcePath = "Player/VFX/PF_Generic_Player_ShieldBubble_03";

    [SerializeField] GameObject shieldVisualPrefab;
    [SerializeField] Vector3 localOffset = new Vector3(0f, 0.72f, 0f);
    [SerializeField] float baseDiameter = 1.08f;
    [SerializeField] float pulseAmplitude = 0.03f;
    [SerializeField] float pulseSpeed = 2.8f;
    [SerializeField] float spinSpeed = 40f;
    [SerializeField] float breakDuration = 0.35f;

    PlayerStats stats;
    Transform shieldRoot;
    Renderer[] shieldRenderers;
    Color[] baseColors;
    Color[] edgeColors;
    bool hasEdgeColors;
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
            shieldRoot.localScale = Vector3.one * baseDiameter * Mathf.Lerp(1f, 1.35f, t);
            LerpShieldAlpha(Mathf.Lerp(1f, 0f, t));
            yield return null;
        }

        SetShieldVisible(false);
        ResetShieldAppearance();
        breakRoutine = null;
    }

    void BuildShieldVisual()
    {
        shieldRoot = transform.Find("ShieldVisual");
        if (shieldRoot == null)
        {
            shieldRoot = new GameObject("ShieldVisual").transform;
            shieldRoot.SetParent(transform, false);
        }

        shieldRoot.localPosition = localOffset;
        shieldRoot.localRotation = Quaternion.identity;
        shieldRoot.localScale = Vector3.one * baseDiameter;

        Transform existingV3 = shieldRoot.Find("PF_Generic_Player_ShieldBubble_03");
        if (existingV3 == null && shieldRoot.childCount > 0)
        {
            for (int i = shieldRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(shieldRoot.GetChild(i).gameObject);
            }
        }

        if (shieldRoot.childCount == 0)
        {
            GameObject prefab = ResolveShieldVisualPrefab();
            if (prefab != null)
            {
                GameObject instance = Instantiate(prefab, shieldRoot);
                instance.name = prefab.name;
                instance.transform.localPosition = Vector3.zero;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
                StripColliders(instance);
            }
            else
            {
                Debug.LogWarning("[PlayerShieldVisual] Shield visual prefab missing; shield bubble will not render.");
            }
        }

        CacheRendererColors();
    }

    static GameObject ResolveShieldVisualPrefab(GameObject assignedPrefab)
    {
        if (assignedPrefab != null)
        {
            return assignedPrefab;
        }

        return Resources.Load<GameObject>(DefaultShieldVisualResourcePath);
    }

    GameObject ResolveShieldVisualPrefab()
    {
        return ResolveShieldVisualPrefab(shieldVisualPrefab);
    }

    static void StripColliders(GameObject root)
    {
        Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Destroy(colliders[i]);
        }
    }

    void CacheRendererColors()
    {
        shieldRenderers = shieldRoot.GetComponentsInChildren<Renderer>(true);
        baseColors = new Color[shieldRenderers.Length];
        edgeColors = new Color[shieldRenderers.Length];
        hasEdgeColors = false;
        for (int i = 0; i < shieldRenderers.Length; i++)
        {
            Material mat = shieldRenderers[i].material;
            baseColors[i] = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : mat.color;
            if (mat.HasProperty("_EdgeColor"))
            {
                edgeColors[i] = mat.GetColor("_EdgeColor");
                hasEdgeColors = true;
            }
        }
    }

    void LerpShieldAlpha(float alphaScale)
    {
        if (shieldRenderers == null || baseColors == null)
        {
            return;
        }

        for (int i = 0; i < shieldRenderers.Length; i++)
        {
            if (shieldRenderers[i] == null)
            {
                continue;
            }

            Material mat = shieldRenderers[i].material;
            Color color = baseColors[i];
            color.a *= alphaScale;
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", color);
            }

            if (mat.HasProperty("_EdgeColor"))
            {
                Color edge = edgeColors[i];
                edge.a *= alphaScale;
                mat.SetColor("_EdgeColor", edge);
            }

            mat.color = color;
        }
    }

    void ResetShieldAppearance()
    {
        if (shieldRoot == null)
        {
            return;
        }

        shieldRoot.localScale = Vector3.one * baseDiameter;
        if (shieldRenderers == null || baseColors == null)
        {
            CacheRendererColors();
        }

        for (int i = 0; i < shieldRenderers.Length; i++)
        {
            if (shieldRenderers[i] == null)
            {
                continue;
            }

            Material mat = shieldRenderers[i].material;
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor("_BaseColor", baseColors[i]);
            }

            if (hasEdgeColors && mat.HasProperty("_EdgeColor"))
            {
                mat.SetColor("_EdgeColor", edgeColors[i]);
            }

            mat.color = baseColors[i];
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
