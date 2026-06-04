using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] float maxHealth = 100f;
    [SerializeField] float currentHealth = 100f;
    [SerializeField] float damageImmunityDuration = 0.5f;

    [Header("Stamina")]
    [SerializeField] float maxStamina = 100f;
    [SerializeField] float currentStamina = 100f;
    [SerializeField] float sprintDrainPerSecond = 20f;
    [SerializeField] float jumpStaminaCost = 25f;
    [SerializeField] float regenDelay = 0.5f;
    [SerializeField] float regenPerSecond = 25f;

    float lastStaminaInputTime = -999f;
    float lastDamageTime = -999f;
    float infiniteStaminaEndTime = -999f;
    float infiniteStaminaTotalDuration = 7f;
    bool shieldActive;
    float invincibleUntilTime = -999f;
    float invincibleTotalDuration = 1f;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public float MaxStamina => maxStamina;
    public float CurrentStamina => currentStamina;
    public float HealthNormalized => maxHealth > 0f ? currentHealth / maxHealth : 0f;
    public float StaminaNormalized => maxStamina > 0f ? currentStamina / maxStamina : 0f;
    public float JumpStaminaCost => jumpStaminaCost;
    public bool IsAlive => currentHealth > 0f;
    public bool HasShield => shieldActive;
    public bool IsInvincible => Time.time < invincibleUntilTime;
    public bool HasInfiniteStamina => Time.time < infiniteStaminaEndTime;
    public float InfiniteStaminaRemaining => Mathf.Max(0f, infiniteStaminaEndTime - Time.time);
    public float InfiniteStaminaTotalDuration => infiniteStaminaTotalDuration;
    public float InvincibleRemaining => Mathf.Max(0f, invincibleUntilTime - Time.time);
    public float InvincibleTotalDuration => invincibleTotalDuration;

    public event Action<float> OnHealthChanged;
    public event Action<float> OnStaminaChanged;
    public event Action<bool, bool> OnShieldStateChanged;
    public event Action OnDeath;

    void Awake()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        NotifyHealthChanged();
        NotifyStaminaChanged();
    }

    public void RegisterStaminaInput(bool shiftHeld, bool spaceHeld)
    {
        if (shiftHeld || spaceHeld)
        {
            lastStaminaInputTime = Time.time;
        }
    }

    public void DrainSprint(float deltaTime)
    {
        if (deltaTime <= 0f || HasInfiniteStamina)
        {
            return;
        }

        float previous = currentStamina;
        currentStamina = Mathf.Max(0f, currentStamina - sprintDrainPerSecond * deltaTime);
        if (!Mathf.Approximately(previous, currentStamina))
        {
            NotifyStaminaChanged();
        }
    }

    public bool TryConsumeJumpStamina()
    {
        if (HasInfiniteStamina)
        {
            return true;
        }

        if (currentStamina < jumpStaminaCost)
        {
            return false;
        }

        currentStamina -= jumpStaminaCost;
        lastStaminaInputTime = Time.time;
        NotifyStaminaChanged();
        return true;
    }

    public bool CanSprint()
    {
        return HasInfiniteStamina || currentStamina > 0f;
    }

    void Update()
    {
        if (HasInfiniteStamina)
        {
            return;
        }

        if (Time.time - lastStaminaInputTime < regenDelay)
        {
            return;
        }

        if (currentStamina < maxStamina)
        {
            float previous = currentStamina;
            currentStamina = Mathf.Min(maxStamina, currentStamina + regenPerSecond * Time.deltaTime);
            if (!Mathf.Approximately(previous, currentStamina))
            {
                NotifyStaminaChanged();
            }
        }
    }

    public bool TryTakeDamage(float amount)
    {
        if (!IsAlive || amount <= 0f)
        {
            return false;
        }

        if (IsInvincible)
        {
            return false;
        }

        if (shieldActive)
        {
            ConsumeShield();
            return false;
        }

        if (Time.time - lastDamageTime < damageImmunityDuration)
        {
            return false;
        }

        ApplyDamageInternal(amount);
        return true;
    }

    public void ApplyContinuousDamage(float damagePerSecond)
    {
        if (!IsAlive || damagePerSecond <= 0f || IsInvincible)
        {
            return;
        }

        if (shieldActive)
        {
            ConsumeShield();
            return;
        }

        ApplyDamageInternal(damagePerSecond * Time.deltaTime);
    }

    public void ApplyGuaranteedHealthLoss(float percentOfCurrentHealth)
    {
        if (!IsAlive || IsInvincible)
        {
            return;
        }

        if (shieldActive)
        {
            ConsumeShield();
            return;
        }

        float clamped = Mathf.Clamp01(percentOfCurrentHealth);
        float damage = currentHealth * clamped;
        if (clamped >= 0.999f)
        {
            currentHealth = 1f;
        }
        else
        {
            currentHealth = Mathf.Max(1f, currentHealth - damage);
        }

        lastDamageTime = Time.time;
        NotifyHealthChanged();
    }

    void ApplyDamageInternal(float amount)
    {
        currentHealth = Mathf.Max(0f, currentHealth - amount);
        lastDamageTime = Time.time;
        NotifyHealthChanged();

        if (!IsAlive)
        {
            OnDeath?.Invoke();
        }
    }

    public void ActivateInfiniteStamina(float durationSeconds)
    {
        infiniteStaminaTotalDuration = durationSeconds;
        infiniteStaminaEndTime = Time.time + durationSeconds;
        currentStamina = maxStamina;
        NotifyStaminaChanged();
    }

    public void ActivateShield()
    {
        shieldActive = true;
        OnShieldStateChanged?.Invoke(true, false);
    }

    void ConsumeShield()
    {
        if (!shieldActive)
        {
            return;
        }

        shieldActive = false;
        invincibleUntilTime = Time.time + invincibleTotalDuration;
        OnShieldStateChanged?.Invoke(false, true);
    }

    public void RestoreFullHealth()
    {
        currentHealth = maxHealth;
        lastDamageTime = -999f;
        NotifyHealthChanged();
    }

    public void ForceKill()
    {
        if (!IsAlive)
        {
            return;
        }

        currentHealth = 0f;
        NotifyHealthChanged();
        OnDeath?.Invoke();
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        lastDamageTime = -999f;
        NotifyHealthChanged();
    }

    public void ResetStamina()
    {
        currentStamina = maxStamina;
        lastStaminaInputTime = -999f;
        NotifyStaminaChanged();
    }

    public void ResetForRespawn()
    {
        ResetHealth();
        ResetStamina();
        shieldActive = false;
        invincibleUntilTime = -999f;
        infiniteStaminaEndTime = -999f;
        OnShieldStateChanged?.Invoke(false, false);
    }

    public void SetHealth(float value)
    {
        currentHealth = Mathf.Clamp(value, 0f, maxHealth);
        NotifyHealthChanged();
    }

    public void SetStamina(float value)
    {
        currentStamina = Mathf.Clamp(value, 0f, maxStamina);
        NotifyStaminaChanged();
    }

    void NotifyHealthChanged()
    {
        OnHealthChanged?.Invoke(currentHealth);
    }

    void NotifyStaminaChanged()
    {
        OnStaminaChanged?.Invoke(currentStamina);
    }
}
