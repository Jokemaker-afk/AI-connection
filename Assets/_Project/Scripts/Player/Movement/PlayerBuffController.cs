using UnityEngine;

[RequireComponent(typeof(PlayerStats))]
public class PlayerBuffController : MonoBehaviour
{
    [SerializeField] float speedBoostMultiplier = 1.6f;
    [SerializeField] float speedBoostDuration = 15f;
    [SerializeField] float infiniteStaminaDuration = 7f;

    PlayerStats stats;
    float speedBoostEndTime;

    void Awake()
    {
        stats = GetComponent<PlayerStats>();
    }

    public float GetSpeedMultiplier()
    {
        return Time.time < speedBoostEndTime ? speedBoostMultiplier : 1f;
    }

    public bool TryApplyBuff(BuffType buffType)
    {
        switch (buffType)
        {
            case BuffType.Heal:
                stats.RestoreFullHealth();
                break;
            case BuffType.SpeedBoost:
                speedBoostEndTime = Time.time + speedBoostDuration;
                break;
            case BuffType.InfiniteStamina:
                stats.ActivateInfiniteStamina(infiniteStaminaDuration);
                break;
            case BuffType.Shield:
                stats.ActivateShield();
                EnsureShieldVisual()?.ShowShield();
                break;
            default:
                return false;
        }

        BuffNotificationOverlay.Show(buffType);
        return true;
    }

    public int CollectActiveBuffs(ActiveBuffDisplay[] results, int maxCount)
    {
        if (results == null || maxCount <= 0 || stats == null)
        {
            return 0;
        }

        int count = 0;

        if (count < maxCount && Time.time < speedBoostEndTime)
        {
            results[count++] = new ActiveBuffDisplay
            {
                Type = BuffType.SpeedBoost,
                RemainingSeconds = speedBoostEndTime - Time.time,
                TotalSeconds = speedBoostDuration,
                UseCountdown = true,
            };
        }

        if (count < maxCount && stats.HasInfiniteStamina)
        {
            results[count++] = new ActiveBuffDisplay
            {
                Type = BuffType.InfiniteStamina,
                RemainingSeconds = stats.InfiniteStaminaRemaining,
                TotalSeconds = stats.InfiniteStaminaTotalDuration,
                UseCountdown = true,
            };
        }

        if (count < maxCount && stats.HasShield)
        {
            results[count++] = new ActiveBuffDisplay
            {
                Type = BuffType.Shield,
                RemainingSeconds = 0f,
                TotalSeconds = 1f,
                UseCountdown = false,
            };
        }

        if (count < maxCount && stats.IsInvincible && !stats.HasShield)
        {
            results[count++] = new ActiveBuffDisplay
            {
                Type = BuffType.Shield,
                RemainingSeconds = stats.InvincibleRemaining,
                TotalSeconds = stats.InvincibleTotalDuration,
                UseCountdown = true,
                OverrideLabel = "免",
            };
        }

        return count;
    }

    PlayerShieldVisual EnsureShieldVisual()
    {
        var visual = GetComponent<PlayerShieldVisual>();
        if (visual == null)
        {
            visual = gameObject.AddComponent<PlayerShieldVisual>();
        }

        return visual;
    }
}
