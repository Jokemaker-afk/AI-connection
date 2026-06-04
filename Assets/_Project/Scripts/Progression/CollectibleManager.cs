using System;
using UnityEngine;

public class CollectibleManager : MonoBehaviour
{
    public static CollectibleManager Instance { get; private set; }

    [SerializeField] float unlockThreshold = 0.3f;

    int totalCollectibleCount;
    int collectedCount;
    bool unlockTriggered;

    public int TotalCollectibleCount => totalCollectibleCount;
    public int CollectedCount => collectedCount;
    public float UnlockThreshold => unlockThreshold;

    public float CollectedPercentage =>
        totalCollectibleCount > 0 ? collectedCount / (float)totalCollectibleCount : 0f;

    public bool IsUnlockThresholdReached =>
        totalCollectibleCount > 0 && CollectedPercentage >= unlockThreshold;

    public event Action<int, int, float> OnProgressChanged;
    public event Action OnUnlockThresholdReached;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        RefreshTotalCount();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void Configure(float threshold)
    {
        unlockThreshold = Mathf.Clamp01(threshold);
        CheckUnlockThreshold();
    }

    public void RefreshTotalCount()
    {
        if (totalCollectibleCount <= 0)
        {
            var pickups = FindObjectsByType<WorldPickupItem>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            totalCollectibleCount = pickups.Length;
        }

        NotifyProgressChanged();
        CheckUnlockThreshold();
    }

    public void RegisterCollection()
    {
        if (totalCollectibleCount <= 0)
        {
            RefreshTotalCount();
        }

        collectedCount = Mathf.Min(collectedCount + 1, totalCollectibleCount);
        NotifyProgressChanged();
        CheckUnlockThreshold();
    }

    void CheckUnlockThreshold()
    {
        if (unlockTriggered || !IsUnlockThresholdReached)
        {
            return;
        }

        unlockTriggered = true;
        OnUnlockThresholdReached?.Invoke();
    }

    void NotifyProgressChanged()
    {
        OnProgressChanged?.Invoke(collectedCount, totalCollectibleCount, CollectedPercentage);
    }
}
