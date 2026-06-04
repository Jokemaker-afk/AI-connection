using UnityEngine;

public class PortalUnlockManager : MonoBehaviour
{
    [SerializeField] CollectibleManager collectibleManager;
    [SerializeField] Vector3 portalSpawnPosition = new Vector3(0f, 0.65f, 0f);
    [SerializeField] string targetSceneName = "Level5";
    [SerializeField] string portalAppearTitle = "传送门已出现";
    [SerializeField] string portalAppearDetail = "地图中央出现了传送门，进入即可前往下一关。";

    GameObject portalRoot;
    bool portalSpawned;

    void Awake()
    {
        if (collectibleManager == null)
        {
            collectibleManager = GetComponent<CollectibleManager>();
        }

        if (collectibleManager == null)
        {
            collectibleManager = FindFirstObjectByType<CollectibleManager>();
        }
    }

    void OnEnable()
    {
        Subscribe();
        if (collectibleManager != null && collectibleManager.IsUnlockThresholdReached)
        {
            SpawnPortal();
        }
    }

    void OnDisable()
    {
        Unsubscribe();
    }

    public void Configure(Vector3 spawnPosition, string sceneName, float unlockThreshold)
    {
        portalSpawnPosition = spawnPosition;
        if (!string.IsNullOrEmpty(sceneName))
        {
            targetSceneName = sceneName;
        }

        if (collectibleManager != null)
        {
            collectibleManager.Configure(unlockThreshold);
        }

        if (collectibleManager != null && collectibleManager.IsUnlockThresholdReached)
        {
            SpawnPortal();
        }
    }

    void Subscribe()
    {
        if (collectibleManager == null)
        {
            return;
        }

        collectibleManager.OnUnlockThresholdReached -= SpawnPortal;
        collectibleManager.OnUnlockThresholdReached += SpawnPortal;
    }

    void Unsubscribe()
    {
        if (collectibleManager == null)
        {
            return;
        }

        collectibleManager.OnUnlockThresholdReached -= SpawnPortal;
    }

    void SpawnPortal()
    {
        if (portalSpawned)
        {
            return;
        }

        portalSpawned = true;

        if (portalRoot == null)
        {
            portalRoot = PortalVisualBuilder.CreatePortal(
                portalSpawnPosition,
                targetSceneName,
                "下一关传送门\n进入后前往制造教学关");
        }
        else
        {
            portalRoot.SetActive(true);
        }

        BuffNotificationOverlay.ShowCustom(portalAppearTitle, portalAppearDetail);
    }
}
