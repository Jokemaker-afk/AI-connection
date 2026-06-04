using UnityEngine;

public class SceneBeacon : MonoBehaviour
{
    [SerializeField] Vector3 beaconWorldPosition = new Vector3(0f, 0.65f, 12f);
    [SerializeField] string targetSceneName = "Level6";
    [SerializeField] string activeSignText = "第六关信标\n进入下一关";
    [SerializeField] string activationTitle = "信标已激活";
    [SerializeField] string activationDetail = "工作台和熔炉已放置，下一关入口已开启";

    GameObject beaconRoot;
    ScenePortal portal;
    bool activated;

    [SerializeField] bool portalLoadOnEnter = true;

    public bool IsActivated => activated;
    public string TargetSceneName => targetSceneName;

    public void Configure(
        Vector3 worldPosition,
        string sceneName,
        string signText = null,
        string title = null,
        string detail = null,
        bool loadPortalOnEnter = true)
    {
        beaconWorldPosition = worldPosition;
        if (!string.IsNullOrEmpty(sceneName))
        {
            targetSceneName = sceneName;
        }

        if (!string.IsNullOrEmpty(signText))
        {
            activeSignText = signText;
        }

        if (!string.IsNullOrEmpty(title))
        {
            activationTitle = title;
        }

        if (!string.IsNullOrEmpty(detail))
        {
            activationDetail = detail;
        }

        portalLoadOnEnter = loadPortalOnEnter;
    }

    public void Activate()
    {
        if (activated)
        {
            return;
        }

        activated = true;
        EnsureBeaconVisual();
        beaconRoot.SetActive(true);

        if (portal != null)
        {
            portal.SetPortalEnabled(true);
            portal.Configure(targetSceneName, portalLoadOnEnter);
        }

        BuffNotificationOverlay.ShowCustom(activationTitle, activationDetail);
    }

    void EnsureBeaconVisual()
    {
        if (beaconRoot != null)
        {
            return;
        }

        beaconRoot = PortalVisualBuilder.CreatePortal(
            beaconWorldPosition,
            targetSceneName,
            activeSignText);
        beaconRoot.name = "SceneProgressionBeacon";

        portal = beaconRoot.GetComponentInChildren<ScenePortal>();
        if (portal != null)
        {
            portal.SetPortalEnabled(false);
        }

        beaconRoot.SetActive(false);
    }

    void OnDestroy()
    {
        if (beaconRoot != null)
        {
            Destroy(beaconRoot);
        }
    }
}
