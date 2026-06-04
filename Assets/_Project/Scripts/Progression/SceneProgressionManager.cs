using UnityEngine;

public class SceneProgressionManager : MonoBehaviour
{
    [SerializeField] ItemKind[] requiredPlacedItems =
    {
        ItemKind.CraftingTable,
        ItemKind.Furnace,
    };
    [SerializeField] string targetSceneName = "Level6";
    [SerializeField] bool activateBeaconOnce = true;
    [SerializeField] SceneBeacon sceneBeacon;

    RequiredPlacedObjectTracker tracker;
    bool beaconActivated;

    public event System.Action OnBeaconActivated;
    public bool IsBeaconActivated => beaconActivated;
    public ItemKind[] RequiredPlacedItems => requiredPlacedItems;

    public bool HasPlaced(ItemKind itemKind)
    {
        return tracker != null && tracker.HasPlacedOnce(itemKind);
    }

    public bool AreAllRequirementsMet()
    {
        if (requiredPlacedItems == null || requiredPlacedItems.Length == 0)
        {
            return true;
        }

        if (tracker == null)
        {
            return false;
        }

        for (int i = 0; i < requiredPlacedItems.Length; i++)
        {
            if (!tracker.HasPlacedOnce(requiredPlacedItems[i]))
            {
                return false;
            }
        }

        return true;
    }

    public void Configure(ItemKind[] requiredItems, string sceneName, SceneBeacon beacon, bool latchBeacon = true)
    {
        if (requiredItems != null && requiredItems.Length > 0)
        {
            requiredPlacedItems = requiredItems;
        }

        if (!string.IsNullOrEmpty(sceneName))
        {
            targetSceneName = sceneName;
        }

        if (beacon != null)
        {
            sceneBeacon = beacon;
        }

        activateBeaconOnce = latchBeacon;
        EvaluateProgress();
    }

    void Awake()
    {
        tracker = RequiredPlacedObjectTracker.Instance;
        if (tracker == null)
        {
            tracker = FindFirstObjectByType<RequiredPlacedObjectTracker>();
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
            tracker.OnItemPlacedOnce -= HandleItemPlacedOnce;
            tracker.OnItemPlacedOnce += HandleItemPlacedOnce;
            tracker.OnProgressChanged -= HandleProgressChanged;
            tracker.OnProgressChanged += HandleProgressChanged;
        }
    }

    void OnDisable()
    {
        if (tracker != null)
        {
            tracker.OnItemPlacedOnce -= HandleItemPlacedOnce;
            tracker.OnProgressChanged -= HandleProgressChanged;
        }
    }

    void Start()
    {
        EvaluateProgress();
    }

    void HandleItemPlacedOnce(ItemKind itemKind)
    {
        EvaluateProgress();
    }

    void HandleProgressChanged()
    {
        EvaluateProgress();
    }

    void EvaluateProgress()
    {
        if (beaconActivated && activateBeaconOnce)
        {
            return;
        }

        if (!AreAllRequirementsMet())
        {
            return;
        }

        ActivateBeacon();
    }

    void ActivateBeacon()
    {
        if (beaconActivated && activateBeaconOnce)
        {
            return;
        }

        beaconActivated = true;
        if (sceneBeacon != null)
        {
            sceneBeacon.Activate();
        }

        OnBeaconActivated?.Invoke();
    }
}
