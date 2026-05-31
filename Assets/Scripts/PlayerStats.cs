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

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public float MaxStamina => maxStamina;
    public float CurrentStamina => currentStamina;
    public float HealthNormalized => maxHealth > 0f ? currentHealth / maxHealth : 0f;
    public float StaminaNormalized => maxStamina > 0f ? currentStamina / maxStamina : 0f;
    public float JumpStaminaCost => jumpStaminaCost;
    public bool IsAlive => currentHealth > 0f;

    public event Action<float> OnHealthChanged;
    public event Action OnDeath;

    void Awake()
    {
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
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
        if (deltaTime <= 0f)
        {
            return;
        }

        currentStamina = Mathf.Max(0f, currentStamina - sprintDrainPerSecond * deltaTime);
    }

    public bool TryConsumeJumpStamina()
    {
        if (currentStamina < jumpStaminaCost)
        {
            return false;
        }

        currentStamina -= jumpStaminaCost;
        lastStaminaInputTime = Time.time;
        return true;
    }

    public bool CanSprint()
    {
        return currentStamina > 0f;
    }

    void Update()
    {
        if (Time.time - lastStaminaInputTime < regenDelay)
        {
            return;
        }

        if (currentStamina < maxStamina)
        {
            currentStamina = Mathf.Min(maxStamina, currentStamina + regenPerSecond * Time.deltaTime);
        }
    }

    public bool TryTakeDamage(float amount)
    {
        if (!IsAlive || amount <= 0f)
        {
            return false;
        }

        if (Time.time - lastDamageTime < damageImmunityDuration)
        {
            return false;
        }

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        lastDamageTime = Time.time;
        OnHealthChanged?.Invoke(currentHealth);

        if (!IsAlive)
        {
            OnDeath?.Invoke();
        }

        return true;
    }

    public void ApplyContinuousDamage(float damagePerSecond)
    {
        if (!IsAlive || damagePerSecond <= 0f)
        {
            return;
        }

        currentHealth = Mathf.Max(0f, currentHealth - damagePerSecond * Time.deltaTime);
        OnHealthChanged?.Invoke(currentHealth);

        if (!IsAlive)
        {
            OnDeath?.Invoke();
        }
    }

    public void ForceKill()
    {
        if (!IsAlive)
        {
            return;
        }

        currentHealth = 0f;
        OnHealthChanged?.Invoke(currentHealth);
        OnDeath?.Invoke();
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        lastDamageTime = -999f;
        OnHealthChanged?.Invoke(currentHealth);
    }

    public void SetHealth(float value)
    {
        currentHealth = Mathf.Clamp(value, 0f, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);
    }

    public void SetStamina(float value)
    {
        currentStamina = Mathf.Clamp(value, 0f, maxStamina);
    }
}
