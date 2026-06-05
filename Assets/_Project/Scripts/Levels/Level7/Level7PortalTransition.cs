using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Level7PortalTransition : MonoBehaviour
{
    [SerializeField] Level7ProgressionManager progressionManager;
    [SerializeField] Transform portalTransform;
    [SerializeField] float activationRange = 4f;
    [SerializeField] string proximityHint = "按 Y 进入第八关";

    bool loading;

    void Awake()
    {
        if (progressionManager == null)
        {
            progressionManager = FindFirstObjectByType<Level7ProgressionManager>();
        }
    }

    public void Bind(Level7ProgressionManager manager, Transform portal, float range)
    {
        progressionManager = manager;
        portalTransform = portal;
        activationRange = range;
    }

    void Update()
    {
        if (loading || progressionManager == null || !progressionManager.IsPortalActivated)
        {
            return;
        }

        if (portalTransform == null)
        {
            var beaconVisual = GameObject.Find("SceneProgressionBeacon");
            if (beaconVisual != null)
            {
                portalTransform = beaconVisual.transform;
            }
        }

        if (portalTransform == null || !IsPlayerInRange())
        {
            return;
        }

        if (Keyboard.current != null && Keyboard.current.yKey.wasPressedThisFrame)
        {
            LoadTargetScene();
        }
    }

    bool IsPlayerInRange()
    {
        var player = FindFirstObjectByType<PlayerController>();
        if (player == null || portalTransform == null)
        {
            return false;
        }

        Vector3 flat = portalTransform.position - player.transform.position;
        flat.y = 0f;
        return flat.magnitude <= activationRange;
    }

    void LoadTargetScene()
    {
        if (progressionManager == null || string.IsNullOrEmpty(progressionManager.TargetSceneName))
        {
            return;
        }

        loading = true;
        LevelTransitionOverlay.Hide();
        GameplayFoundationBootstrap.PrepareSceneTransition(progressionManager.TargetSceneName);
        SceneManager.LoadScene(progressionManager.TargetSceneName);
    }
}
