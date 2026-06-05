using UnityEngine;

[DisallowMultipleComponent]
public class EnemyVisual : MonoBehaviour
{
    [SerializeField] Color baseColor = Color.white;
    [SerializeField] Color hitFlashColor = new Color(1f, 0.55f, 0.55f);
    [SerializeField] float hitFlashDuration = 0.12f;

    EnemyHealth health;
    Renderer[] renderers;
    MaterialPropertyBlock propertyBlock;
    float hitFlashTimer;
    Transform labelAnchor;
    string displayName = "敌人";

    public void Configure(EnemyData data, Transform visualRoot)
    {
        displayName = data.DisplayNameChinese;
        baseColor = data.PlaceholderColor;
        labelAnchor = visualRoot != null ? visualRoot : transform;
        CacheRenderers(visualRoot);
        RefreshHealthLabel();
    }

    void Awake()
    {
        health = GetComponent<EnemyHealth>();
        propertyBlock = new MaterialPropertyBlock();
        if (health != null)
        {
            health.OnHealthChanged += HandleHealthChanged;
            health.OnDamaged += HandleDamaged;
            health.OnDeath += HandleDeath;
        }
    }

    void OnDestroy()
    {
        if (health != null)
        {
            health.OnHealthChanged -= HandleHealthChanged;
            health.OnDamaged -= HandleDamaged;
            health.OnDeath -= HandleDeath;
        }
    }

    void Update()
    {
        if (hitFlashTimer > 0f)
        {
            hitFlashTimer -= Time.deltaTime;
            ApplyColor(hitFlashTimer > 0f ? hitFlashColor : baseColor);
        }
    }

    public void CacheRenderers(Transform visualRoot)
    {
        if (visualRoot == null)
        {
            renderers = GetComponentsInChildren<Renderer>(true);
            return;
        }

        renderers = visualRoot.GetComponentsInChildren<Renderer>(true);
    }

    void HandleHealthChanged(float current, float max)
    {
        RefreshHealthLabel(current, max);
    }

    void HandleDamaged(DamageInfo info)
    {
        hitFlashTimer = hitFlashDuration;
        ApplyColor(hitFlashColor);
        EnemyDamageFeedback.Show(info.HitPoint, info.Damage);
    }

    void HandleDeath()
    {
        ApplyColor(baseColor * 0.35f);
    }

    void RefreshHealthLabel()
    {
        if (health == null)
        {
            return;
        }

        RefreshHealthLabel(health.CurrentHealth, health.MaxHealth);
    }

    void RefreshHealthLabel(float current, float max)
    {
        if (labelAnchor == null)
        {
            return;
        }

        string text = $"{displayName}\n{Mathf.CeilToInt(current)}/{Mathf.CeilToInt(max)}";
        ItemWorldLabel.Refresh(labelAnchor.gameObject, text);
    }

    void ApplyColor(Color color)
    {
        if (renderers == null)
        {
            return;
        }

        EnemySurfaceHighlighter highlighter = GetComponent<EnemySurfaceHighlighter>();
        if (highlighter != null && highlighter.CurrentLevel != EnemyHighlightLevel.None)
        {
            return;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null)
            {
                continue;
            }

            renderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_BaseColor", color);
            propertyBlock.SetColor("_Color", color);
            renderer.SetPropertyBlock(propertyBlock);
        }
    }
}
