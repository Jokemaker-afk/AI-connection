using System;
using UnityEngine;

/// <summary>Tracks Level8 exploration objectives: 3 Data Cores → Signal Relay → Exit to Level9.</summary>
public class Level8ObjectiveTracker : MonoBehaviour
{
    public static Level8ObjectiveTracker Instance { get; private set; }

    [SerializeField] int requiredDataCores = 3;
    [SerializeField] int collectedDataCores;
    [SerializeField] bool signalRelayActivated;
    [SerializeField] bool exitToLevel9Activated;

    public int RequiredDataCores => requiredDataCores;
    public int CollectedDataCores => collectedDataCores;
    public bool SignalRelayActivated => signalRelayActivated;
    public bool ExitToLevel9Activated => exitToLevel9Activated;

    public event Action OnProgressChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        Debug.Log("[Level8] Objective tracker initialized.");
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void RegisterDataCoreCollected()
    {
        if (collectedDataCores >= requiredDataCores)
        {
            return;
        }

        collectedDataCores++;
        Debug.Log($"[Level8] Data Core collected: {collectedDataCores} / {requiredDataCores}");
        NotifyChanged();
    }

    public bool HasAllDataCores()
    {
        return collectedDataCores >= requiredDataCores;
    }

    public void ActivateSignalRelay()
    {
        if (signalRelayActivated)
        {
            return;
        }

        if (!HasAllDataCores())
        {
            Debug.LogWarning("[Level8] Signal Relay activation blocked — not all Data Cores collected.");
            return;
        }

        signalRelayActivated = true;
        Debug.Log("[Level8] Signal Relay activated.");
        TryActivateExit();
        NotifyChanged();
    }

    public void TryActivateExit()
    {
        if (exitToLevel9Activated || !signalRelayActivated)
        {
            return;
        }

        exitToLevel9Activated = true;
        Debug.Log("[Level8] Exit to Level9 activated.");
        BuffNotificationOverlay.ShowCustom("信号中继器已激活", "前往终点传送门，按 Y 进入第九关");
        NotifyChanged();
    }

    public string GetCurrentObjectiveText()
    {
        if (exitToLevel9Activated)
        {
            return "目标：进入 Level 9";
        }

        if (signalRelayActivated)
        {
            return "目标：进入 Level 9";
        }

        if (HasAllDataCores())
        {
            return "目标：前往并激活信号中继器";
        }

        return $"目标：收集数据核心 {collectedDataCores} / {requiredDataCores}";
    }

    void NotifyChanged()
    {
        OnProgressChanged?.Invoke();
    }

    public void ResetForSceneEntry()
    {
        collectedDataCores = 0;
        signalRelayActivated = false;
        exitToLevel9Activated = false;
        NotifyChanged();
    }
}
