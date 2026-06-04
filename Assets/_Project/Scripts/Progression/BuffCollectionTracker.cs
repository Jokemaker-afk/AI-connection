using System;
using System.Collections.Generic;
using UnityEngine;

public class BuffCollectionTracker : MonoBehaviour
{
    static readonly BuffType[] RequiredBuffs =
    {
        BuffType.Heal,
        BuffType.SpeedBoost,
        BuffType.InfiniteStamina,
        BuffType.Shield,
    };

    static BuffCollectionTracker instance;

    readonly HashSet<BuffType> collectedBuffs = new HashSet<BuffType>();

    public static BuffCollectionTracker Instance => instance;

    public event Action OnAllBuffsCollected;

    public int CollectedCount => collectedBuffs.Count;
    public int RequiredCount => RequiredBuffs.Length;
    public bool AllCollected => collectedBuffs.Count >= RequiredBuffs.Length;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }

        instance = this;
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public bool HasCollected(BuffType buffType)
    {
        return collectedBuffs.Contains(buffType);
    }

    public void RegisterCollected(BuffType buffType)
    {
        if (!IsRequired(buffType) || !collectedBuffs.Add(buffType))
        {
            return;
        }

        if (AllCollected)
        {
            OnAllBuffsCollected?.Invoke();

            // Fail-safe: immediately open the Level4 transition prompt.
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.NotifyGoalReached();
            }
        }
    }

    public static bool IsRequired(BuffType buffType)
    {
        for (int i = 0; i < RequiredBuffs.Length; i++)
        {
            if (RequiredBuffs[i] == buffType)
            {
                return true;
            }
        }

        return false;
    }
}
