using UnityEngine;

public class Level7ProgressionManager : MonoBehaviour
{
    [SerializeField] string targetSceneName = "Level8";
    [SerializeField] SceneBeacon sceneBeacon;
    [SerializeField] float portalActivationRange = 4f;

    Level7TaskProgressTracker tracker;
    bool portalActivated;

    public bool IsPortalActivated => portalActivated;
    public string TargetSceneName => targetSceneName;
    public float PortalActivationRange => portalActivationRange;

    public bool IsMeleeHitComplete => tracker != null && tracker.IsTaskComplete(Level7TaskProgressTracker.HitMeleeDummyTaskId);
    public bool IsRangedHitComplete => tracker != null && tracker.IsTaskComplete(Level7TaskProgressTracker.HitRangedTargetTaskId);
    public bool IsWalkerDefeated => tracker != null && tracker.IsTaskComplete(Level7TaskProgressTracker.DefeatBasicWalkerTaskId);
    public bool AreAllTasksComplete => tracker != null && tracker.AreAllTutorialTasksComplete();

    public event System.Action OnPortalActivated;

    public void Configure(string sceneName, SceneBeacon beacon, float activationRange = 4f)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            targetSceneName = sceneName;
        }

        if (beacon != null)
        {
            sceneBeacon = beacon;
        }

        portalActivationRange = activationRange;
        EvaluateProgress();
    }

    void Awake()
    {
        tracker = Level7TaskProgressTracker.Instance;
        if (tracker == null)
        {
            tracker = FindFirstObjectByType<Level7TaskProgressTracker>();
        }

        if (sceneBeacon == null)
        {
            sceneBeacon = GetComponent<SceneBeacon>();
        }
    }

    void OnEnable()
    {
        if (tracker != null)
        {
            tracker.OnProgressChanged -= EvaluateProgress;
            tracker.OnProgressChanged += EvaluateProgress;
        }
    }

    void OnDisable()
    {
        if (tracker != null)
        {
            tracker.OnProgressChanged -= EvaluateProgress;
        }
    }

    void Start()
    {
        EvaluateProgress();
    }

    void EvaluateProgress()
    {
        if (portalActivated || !AreAllTasksComplete)
        {
            return;
        }

        ActivatePortal();
    }

    void ActivatePortal()
    {
        if (portalActivated)
        {
            return;
        }

        portalActivated = true;
        sceneBeacon?.Activate();
        BuffNotificationOverlay.ShowCustom("武器教学完成", "前往传送门并按 Y 进入第八关");
        OnPortalActivated?.Invoke();
    }
}
