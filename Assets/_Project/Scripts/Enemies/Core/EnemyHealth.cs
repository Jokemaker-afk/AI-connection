using System;
using UnityEngine;

[DisallowMultipleComponent]
public class EnemyHealth : MonoBehaviour
{
    [SerializeField] float maxHealth = 50f;
    [SerializeField] bool destroyOnDeath = true;
    [SerializeField] float destroyDelay = 0.5f;
    [SerializeField] string taskIdOnFirstHit;
    [SerializeField] string eventIdOnDeath;

    float currentHealth;
    bool isDead;
    bool reportedFirstHit;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => isDead;

    public event Action<float, float> OnHealthChanged;
    public event Action<DamageInfo> OnDamaged;
    public event Action OnDeath;

    public void Configure(EnemyData data)
    {
        maxHealth = Mathf.Max(1f, data.MaxHealth);
        destroyOnDeath = data.DestroyOnDeath;
        destroyDelay = Mathf.Max(0.1f, data.DestroyDelay);
        taskIdOnFirstHit = data.TaskIdOnFirstHit;
        eventIdOnDeath = data.EventIdOnDeath;
        currentHealth = maxHealth;
        isDead = false;
        reportedFirstHit = false;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        if (isDead || damageInfo.Damage <= 0f)
        {
            return;
        }

        if (!reportedFirstHit && !string.IsNullOrEmpty(taskIdOnFirstHit))
        {
            reportedFirstHit = true;
            SceneTaskProgressBridge.RegisterTaskComplete(taskIdOnFirstHit);
        }

        currentHealth = Mathf.Max(0f, currentHealth - damageInfo.Damage);
        OnDamaged?.Invoke(damageInfo);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        currentHealth = 0f;

        if (!string.IsNullOrEmpty(eventIdOnDeath))
        {
            SceneTaskProgressBridge.RegisterTaskComplete(eventIdOnDeath);
            GameEventManager.TriggerEvent(eventIdOnDeath);
        }

        OnDeath?.Invoke();
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null)
            {
                colliders[i].enabled = false;
            }
        }

        if (destroyOnDeath)
        {
            Destroy(gameObject, destroyDelay);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
