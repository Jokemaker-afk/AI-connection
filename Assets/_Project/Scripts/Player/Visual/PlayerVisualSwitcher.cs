using UnityEngine;

[DisallowMultipleComponent]
[DefaultExecutionOrder(-10)]
public class PlayerVisualSwitcher : MonoBehaviour
{
    public const PlayerVisualKind DefaultVisualKind = PlayerVisualKind.DirectionalBlock;

    [SerializeField] PlayerVisualKind activeVisualKind = DefaultVisualKind;
    [SerializeField] PlayerVisualProfile directionalBlockProfile;
    [SerializeField] PlayerVisualProfile legacyCapsuleProfile;

    PlayerVisualModule[] modules;
    PlayerCameraController cameraController;

    public PlayerVisualKind ActiveVisualKind => activeVisualKind;

    void Awake()
    {
        EnsureProfiles();
        RefreshModules();
    }

    void LateUpdate()
    {
        ApplyCameraVisibility();
    }

    public void EnsureDefaultVisual()
    {
        EnsureProfiles();
        SwitchVisual(DefaultVisualKind);
    }

    public void SwitchVisual(PlayerVisualKind kind)
    {
        activeVisualKind = kind;
        RefreshModules();

        for (int i = 0; i < modules.Length; i++)
        {
            if (modules[i] == null)
            {
                continue;
            }

            bool activeModule = modules[i].VisualKind == kind;
            modules[i].SetActiveVisual(activeModule);
        }

        ApplyCameraVisibility();
        GameplayCore.Instance?.Log($"Player visual switched to {kind}.");
    }

    public Renderer[] GetActiveRenderers()
    {
        RefreshModules();
        for (int i = 0; i < modules.Length; i++)
        {
            if (modules[i] != null && modules[i].VisualKind == activeVisualKind)
            {
                return modules[i].GetComponentsInChildren<Renderer>(true);
            }
        }

        Transform visualRoot = transform.Find(PlayerVisualBuilder.VisualRootName);
        return visualRoot != null
            ? visualRoot.GetComponentsInChildren<Renderer>(true)
            : GetComponentsInChildren<Renderer>(true);
    }

    void EnsureProfiles()
    {
        if (directionalBlockProfile == null)
        {
            directionalBlockProfile = PlayerVisualProfile.CreateBuiltin(PlayerVisualKind.DirectionalBlock);
        }

        if (legacyCapsuleProfile == null)
        {
            legacyCapsuleProfile = PlayerVisualProfile.CreateBuiltin(PlayerVisualKind.LegacyCapsule);
        }
    }

    void RefreshModules()
    {
        modules = GetComponentsInChildren<PlayerVisualModule>(true);
    }

    void ApplyCameraVisibility()
    {
        if (modules == null || modules.Length == 0)
        {
            RefreshModules();
        }

        if (cameraController == null)
        {
            cameraController = FindFirstObjectByType<PlayerCameraController>();
        }

        bool firstPerson = cameraController != null && cameraController.IsFirstPerson;
        for (int i = 0; i < modules.Length; i++)
        {
            if (modules[i] == null || !modules[i].gameObject.activeInHierarchy)
            {
                continue;
            }

            modules[i].ApplyCameraVisibility(firstPerson);
        }
    }
}
