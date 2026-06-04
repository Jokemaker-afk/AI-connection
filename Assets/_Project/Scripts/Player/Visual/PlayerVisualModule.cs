using UnityEngine;

[DisallowMultipleComponent]
public class PlayerVisualModule : MonoBehaviour
{
    [SerializeField] PlayerVisualKind visualKind = PlayerVisualKind.DirectionalBlock;
    [SerializeField] PlayerVisualProfile profile;
    [SerializeField] bool visibleInFirstPerson;
    [SerializeField] bool visibleInThirdPerson = true;

    Renderer[] cachedRenderers;

    public PlayerVisualKind VisualKind => visualKind;
    public PlayerVisualProfile Profile => profile;
    public bool VisibleInFirstPerson => visibleInFirstPerson;
    public bool VisibleInThirdPerson => visibleInThirdPerson;

    public void Configure(PlayerVisualProfile sourceProfile, bool active)
    {
        if (sourceProfile != null)
        {
            profile = sourceProfile;
            visualKind = sourceProfile.VisualKind;
            visibleInFirstPerson = sourceProfile.VisibleInFirstPerson;
            visibleInThirdPerson = sourceProfile.VisibleInThirdPerson;
        }

        CacheRenderers();
        SetActiveVisual(active);
    }

    public void SetActiveVisual(bool active)
    {
        gameObject.SetActive(active);
    }

    public void ApplyCameraVisibility(bool firstPerson)
    {
        CacheRenderers();
        bool visible = firstPerson ? visibleInFirstPerson : visibleInThirdPerson;
        for (int i = 0; i < cachedRenderers.Length; i++)
        {
            if (cachedRenderers[i] != null)
            {
                cachedRenderers[i].enabled = visible;
            }
        }
    }

    void CacheRenderers()
    {
        if (cachedRenderers == null || cachedRenderers.Length == 0)
        {
            cachedRenderers = GetComponentsInChildren<Renderer>(true);
        }
    }
}
