using UnityEngine;

public class Level6ProgressionManager : MonoBehaviour
{
    [SerializeField] string targetSceneName = "Level7";
    [SerializeField] SceneBeacon sceneBeacon;
    [SerializeField] float portalActivationRange = 4f;

    Level6TaskProgressTracker tracker;
    bool portalActivated;

    public bool IsPortalActivated => portalActivated;
    public string TargetSceneName => targetSceneName;
    public float PortalActivationRange => portalActivationRange;

    public bool IsMineComplete => tracker != null && tracker.IsTaskComplete(Level6TaskProgressTracker.MineOnceTaskId);
    public bool IsChopComplete => tracker != null && tracker.IsTaskComplete(Level6TaskProgressTracker.ChopOnceTaskId);
    public bool IsRepairComplete => tracker != null && tracker.IsTaskComplete(Level6TaskProgressTracker.RepairSignalRelayTaskId);
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
        tracker = Level6TaskProgressTracker.Instance;
        if (tracker == null)
        {
            tracker = FindFirstObjectByType<Level6TaskProgressTracker>();
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
        if (sceneBeacon != null)
        {
            sceneBeacon.Activate();
        }

        BuffNotificationOverlay.ShowCustom("工具教学完成", "三项任务已完成，前往传送门并按 Y 进入第七关");
        OnPortalActivated?.Invoke();
    }
}
