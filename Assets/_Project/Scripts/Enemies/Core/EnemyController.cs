using UnityEngine;

[DisallowMultipleComponent]
public class EnemyController : MonoBehaviour, IDamageable
{
    [SerializeField] EnemyKind enemyKind = EnemyKind.None;

    EnemyHealth health;
    EnemyKnockback knockback;
    EnemyMovement movement;
    EnemyVisual visual;
    EnemyData data;
    bool initialized;

    public EnemyKind Kind => enemyKind;
    public EnemyData Data => data;
    public bool IsDead => health != null && health.IsDead;

    public void Initialize(EnemyKind kind)
    {
        if (!EnemyCatalog.TryGet(kind, out EnemyData catalogData))
        {
            Debug.LogWarning($"[Enemy] Missing catalog data for {kind}");
            return;
        }

        Initialize(catalogData);
    }

    public void Initialize(EnemyData catalogData)
    {
        data = catalogData;
        enemyKind = catalogData.Kind;
        initialized = true;

        EnsureComponents();
        health.Configure(catalogData);
        knockback.Configure(catalogData);
        movement.Configure(catalogData);
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        if (health == null || health.IsDead)
        {
            return;
        }

        health.TakeDamage(damageInfo);
    }

    void Awake()
    {
        EnsureComponents();
    }

    void Start()
    {
        if (!initialized && enemyKind != EnemyKind.None)
        {
            Initialize(enemyKind);
        }
    }

    void EnsureComponents()
    {
        health = GetComponent<EnemyHealth>() ?? gameObject.AddComponent<EnemyHealth>();
        knockback = GetComponent<EnemyKnockback>() ?? gameObject.AddComponent<EnemyKnockback>();
        movement = GetComponent<EnemyMovement>() ?? gameObject.AddComponent<EnemyMovement>();
        visual = GetComponent<EnemyVisual>() ?? gameObject.AddComponent<EnemyVisual>();
    }

    public void BindVisual(Transform visualRoot)
    {
        EnsureComponents();
        if (visual != null && data.IsValid)
        {
            visual.Configure(data, visualRoot);
        }
    }
}
