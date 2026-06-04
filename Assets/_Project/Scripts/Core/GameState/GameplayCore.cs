using UnityEngine;

[DisallowMultipleComponent]
[DefaultExecutionOrder(-5000)]
public class GameplayCore : MonoBehaviour
{
    static GameplayCore instance;

    [SerializeField] bool enableDebugLogs = true;

    PlayerProgressionState progressionState;
    AbilityUnlockManager abilityUnlockManager;

    public static GameplayCore Instance => instance;
    public static bool Exists => instance != null;
    public PlayerProgressionState ProgressionState => progressionState;
    public AbilityUnlockManager Abilities => abilityUnlockManager;

    public static GameplayCore EnsureExists()
    {
        if (instance != null)
        {
            return instance;
        }

        var existing = FindFirstObjectByType<GameplayCore>();
        if (existing != null)
        {
            instance = existing;
            return existing;
        }

        var coreGo = new GameObject("GameplayCore");
        instance = coreGo.AddComponent<GameplayCore>();
        DontDestroyOnLoad(coreGo);
        return instance;
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureComponents();
        GameplayPlacementBootstrap.EnsureInitialized();
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    void EnsureComponents()
    {
        progressionState = GetComponent<PlayerProgressionState>();
        if (progressionState == null)
        {
            progressionState = gameObject.AddComponent<PlayerProgressionState>();
        }

        abilityUnlockManager = GetComponent<AbilityUnlockManager>();
        if (abilityUnlockManager == null)
        {
            abilityUnlockManager = gameObject.AddComponent<AbilityUnlockManager>();
        }

        PersistentPlayerRig.EnsureOnGameplayCore(this);
    }

    public void Log(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[GameplayCore] {message}", this);
        }
    }
}
