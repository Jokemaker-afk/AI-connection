using UnityEngine;

using UnityEngine.InputSystem;

using UnityEngine.SceneManagement;



[DisallowMultipleComponent]

public class Level8ExitPortal : MonoBehaviour

{

    [SerializeField] string targetSceneName = "Level9";

    [SerializeField] float activationRange = 5f;

    [SerializeField] string inactivePrompt = "信号中继器未激活";

    [SerializeField] string activePrompt = "按 Y 进入 Level 9";



    Transform visualRoot;

    Transform activeRingRoot;
    Transform activeBeamRoot;
    Renderer beaconRenderer;

    Renderer platformRenderer;

    Color inactiveColor;

    Color activeColor;

    bool loading;



    public string TargetSceneName => targetSceneName;

    public float ActivationRange => activationRange;

    public bool IsActive { get; private set; }



    public void Configure(Vector3 worldPosition, Color landmarkColor, string sceneName = null)

    {

        transform.position = worldPosition;

        inactiveColor = new Color(0.22f, 0.24f, 0.28f, 0.75f);

        activeColor = Color.Lerp(landmarkColor, new Color(0.4f, 0.85f, 1f), 0.5f);



        if (!string.IsNullOrEmpty(sceneName))

        {

            targetSceneName = sceneName;

        }



        BuildVisuals();

        EnsureCollider();

        SetActiveState(false);

    }



    void EnsureCollider()

    {

        SphereCollider sphere = GetComponent<SphereCollider>();

        if (sphere == null)

        {

            sphere = gameObject.AddComponent<SphereCollider>();

        }



        sphere.isTrigger = true;

        sphere.radius = activationRange;

        sphere.center = Vector3.up * 1f;

    }



    void BuildVisuals()

    {

        if (visualRoot != null)

        {

            Destroy(visualRoot.gameObject);

        }



        visualRoot = new GameObject("Visual").transform;

        visualRoot.SetParent(transform, false);



        var platform = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

        platform.name = "Platform";

        platform.transform.SetParent(visualRoot, false);

        platform.transform.localPosition = new Vector3(0f, 0.05f, 0f);

        platform.transform.localScale = new Vector3(3.6f, 0.08f, 3.6f);

        Object.Destroy(platform.GetComponent<Collider>());

        platformRenderer = platform.GetComponent<Renderer>();

        Level8VisualMaterialUtility.ApplyColor(platformRenderer, inactiveColor);



        var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

        pillar.name = "BeaconPillar";

        pillar.transform.SetParent(visualRoot, false);

        pillar.transform.localPosition = new Vector3(0f, 1.4f, 0f);

        pillar.transform.localScale = new Vector3(1.1f, 2.2f, 1.1f);

        Object.Destroy(pillar.GetComponent<Collider>());

        beaconRenderer = pillar.GetComponent<Renderer>();

        Level8VisualMaterialUtility.ApplyColor(beaconRenderer, inactiveColor);



        var arch = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

        arch.name = "PortalArch";

        arch.transform.SetParent(visualRoot, false);

        arch.transform.localPosition = new Vector3(0f, 2.8f, 0f);

        arch.transform.localScale = new Vector3(2.2f, 0.12f, 2.2f);

        Object.Destroy(arch.GetComponent<Collider>());

        Level8VisualMaterialUtility.ApplyColor(arch.GetComponent<Renderer>(), inactiveColor);



        activeRingRoot = Level8VisualHierarchyUtility.CreateGlowRing(

            visualRoot,

            "ActiveRing",

            new Vector3(0f, 0.12f, 0f),

            new Color(activeColor.r, activeColor.g, activeColor.b, 0.55f),

            1.9f,

            0.12f).transform;

        activeRingRoot.gameObject.SetActive(false);



        Level8VisualHierarchyUtility.CreateDistanceLabelAnchor(

            transform,

            "前往 Level 9",

            new Vector3(0f, 4.5f, 0f),

            Level8VisualHierarchyFlags.ExitLabelDistance,

            characterSize: 0.14f);

    }



    public void SetActiveState(bool active)

    {

        IsActive = active;

        if (beaconRenderer != null)

        {

            Level8VisualMaterialUtility.ApplyColor(beaconRenderer, active ? activeColor : inactiveColor);

        }



        if (platformRenderer != null)

        {

            Level8VisualMaterialUtility.ApplyColor(

                platformRenderer,

                active ? Color.Lerp(activeColor, Color.white, 0.2f) : inactiveColor);

        }



        if (activeRingRoot != null)
        {
            activeRingRoot.gameObject.SetActive(active);
        }

        if (active && activeBeamRoot == null && visualRoot != null)
        {
            activeBeamRoot = Level8VisualHierarchyUtility.CreateVerticalBeam(
                visualRoot,
                "ExitBeam",
                new Vector3(0f, 2.5f, 0f),
                new Color(activeColor.r, activeColor.g, activeColor.b, 0.5f),
                height: 6f,
                radius: 0.12f,
                transparent: true).transform;
        }

        if (activeBeamRoot != null)
        {
            activeBeamRoot.gameObject.SetActive(active);
        }
    }



    public string GetProximityPrompt()

    {

        return IsActive ? activePrompt : inactivePrompt;

    }



    void Update()

    {

        if (loading || !IsActive || !IsPlayerInRange())

        {

            return;

        }



        if (Keyboard.current != null && Keyboard.current.yKey.wasPressedThisFrame)

        {

            TryLoadTargetScene();

        }

    }



    bool IsPlayerInRange()

    {

        var player = FindFirstObjectByType<PlayerController>();

        if (player == null)

        {

            return false;

        }



        Vector3 flat = transform.position - player.transform.position;

        flat.y = 0f;

        return flat.magnitude <= activationRange;

    }



    void TryLoadTargetScene()

    {

        if (string.IsNullOrEmpty(targetSceneName))

        {

            Debug.LogWarning("[Level8] Exit portal has no target scene.");

            return;

        }



        if (!IsSceneInBuildSettings(targetSceneName))

        {

            Debug.LogWarning($"[Level8] Level9 尚未实现 — scene '{targetSceneName}' is not in Build Settings.");

            BuffNotificationOverlay.ShowCustom("Level9 尚未实现", "第九关场景尚未加入构建设置");

            return;

        }



        loading = true;

        LevelTransitionOverlay.Hide();

        GameplayFoundationBootstrap.PrepareSceneTransition(targetSceneName);

        SceneManager.LoadScene(targetSceneName);

    }



    public static bool IsSceneInBuildSettings(string sceneName)

    {

        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)

        {

            string path = SceneUtility.GetScenePathByBuildIndex(i);

            if (System.IO.Path.GetFileNameWithoutExtension(path) == sceneName)

            {

                return true;

            }

        }



        return false;

    }

}


