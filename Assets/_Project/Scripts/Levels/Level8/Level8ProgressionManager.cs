using UnityEngine;

public class Level8ProgressionManager : MonoBehaviour
{
    [SerializeField] Level8ExitPortal exitPortal;
    SignalRelayInteractable signalRelay;

    public Level8ExitPortal ExitPortal => exitPortal;

    public void BindExitPortal(Level8ExitPortal portal)
    {
        exitPortal = portal;
        EvaluateExitState();
    }

    public void BindSignalRelay(SignalRelayInteractable relay)
    {
        signalRelay = relay;
    }

    void OnEnable()
    {
        Level8ObjectiveTracker tracker = Level8ObjectiveTracker.Instance;
        if (tracker != null)
        {
            tracker.OnProgressChanged -= EvaluateExitState;
            tracker.OnProgressChanged += EvaluateExitState;
        }
    }

    void OnDisable()
    {
        Level8ObjectiveTracker tracker = Level8ObjectiveTracker.Instance;
        if (tracker != null)
        {
            tracker.OnProgressChanged -= EvaluateExitState;
        }
    }

    void Start()
    {
        EvaluateExitState();
    }

    void EvaluateExitState()
    {
        Level8ObjectiveTracker tracker = Level8ObjectiveTracker.Instance;
        if (tracker == null || exitPortal == null)
        {
            return;
        }

        if (tracker.ExitToLevel9Activated)
        {
            exitPortal.SetActiveState(true);
        }
        else
        {
            exitPortal.SetActiveState(false);
        }
    }
}
