using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider))]
public class ScenePortal : MonoBehaviour
{
    [SerializeField] string targetSceneName = "Level5";
    [SerializeField] bool loadOnEnter = true;

    bool loading;
    bool portalEnabled = true;

    public void Configure(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            targetSceneName = sceneName;
        }
    }

    public void SetPortalEnabled(bool enabled)
    {
        portalEnabled = enabled;
    }

    void Awake()
    {
        var box = GetComponent<BoxCollider>();
        box.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!portalEnabled || !loadOnEnter || loading || string.IsNullOrEmpty(targetSceneName))
        {
            return;
        }

        if (!IsPlayerCollider(other))
        {
            return;
        }

        loading = true;
        SceneManager.LoadScene(targetSceneName);
    }

    static bool IsPlayerCollider(Collider other)
    {
        if (other == null)
        {
            return false;
        }

        if (other.GetComponent<PlayerController>() != null)
        {
            return true;
        }

        return other.GetComponentInParent<PlayerController>() != null;
    }
}
