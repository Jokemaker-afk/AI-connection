using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class HandheldWeaponVisual : MonoBehaviour
{
    [SerializeField] ItemKind sourceItem = ItemKind.None;
    [SerializeField] Transform visualRoot;
    [SerializeField] Animation legacyAnimation;
    [SerializeField] bool preferFallbackAnimation = true;

    WeaponProfile profile;
    Coroutine attackRoutine;
    bool attackPlaying;

    public ItemKind SourceItem => sourceItem;
    public bool IsAttackPlaying => attackPlaying;

    public void Configure(ItemKind itemKind, WeaponProfile weaponProfile)
    {
        sourceItem = itemKind;
        profile = weaponProfile;
        preferFallbackAnimation = weaponProfile.UseFallbackAttackAnimation
            || weaponProfile.FirstPersonAttackAnimation == null && weaponProfile.ThirdPersonAttackAnimation == null;
        EnsureVisualRoot();
        EnsureAnimationComponent();
    }

    void Awake()
    {
        EnsureVisualRoot();
        EnsureAnimationComponent();
    }

    public void PlayAttackAnimation(System.Action onComplete = null)
    {
        if (attackPlaying)
        {
            if (attackRoutine != null)
            {
                StopCoroutine(attackRoutine);
            }

            attackPlaying = false;
        }

        if (!profile.IsWeapon && ItemCatalog.TryGet(sourceItem, out ItemData data))
        {
            profile = data.Weapon;
        }

        string clipName = ResolveClipName();
        if (!preferFallbackAnimation && TryPlayClip(clipName, onComplete))
        {
            Debug.Log($"[WeaponAnim] Attack animation: clip {clipName} ({ItemKindUtility.GetDisplayName(sourceItem)})");
            return;
        }

        Debug.Log($"[WeaponAnim] Attack animation: fallback ({profile.AttackMode}) ({ItemKindUtility.GetDisplayName(sourceItem)})");
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
        }

        attackRoutine = StartCoroutine(profile.AttackMode == WeaponAttackMode.Hitscan || profile.IsRanged
            ? ProceduralRecoilRoutine(onComplete)
            : ProceduralSwingRoutine(onComplete));
    }

    string ResolveClipName()
    {
        bool firstPerson = IsFirstPersonContext();
        AnimationClip clip = firstPerson ? profile.FirstPersonAttackAnimation : profile.ThirdPersonAttackAnimation;
        return clip != null ? clip.name : "Attack";
    }

    bool TryPlayClip(string clipName, System.Action onComplete)
    {
        if (legacyAnimation == null || legacyAnimation.GetClip(clipName) == null)
        {
            return false;
        }

        legacyAnimation.Play(clipName);
        float duration = legacyAnimation.GetClip(clipName).length;
        StartCoroutine(WaitForClip(onComplete, duration));
        return true;
    }

    IEnumerator WaitForClip(System.Action onComplete, float duration)
    {
        attackPlaying = true;
        yield return new WaitForSeconds(Mathf.Max(0.05f, duration));
        attackPlaying = false;
        onComplete?.Invoke();
    }

    Transform GetAnimationRoot()
    {
        if (IsFirstPersonContext())
        {
            return visualRoot != null ? visualRoot : transform;
        }

        return transform;
    }

    IEnumerator ProceduralSwingRoutine(System.Action onComplete)
    {
        attackPlaying = true;
        Transform root = GetAnimationRoot();
        Quaternion startRotation = root.localRotation;
        Vector3 startPosition = root.localPosition;
        float duration = Mathf.Max(0.15f, profile.FallbackAttackDuration);
        float swingDegrees = profile.FallbackSwingDegrees;
        float forward = profile.FallbackSwingForward;
        if (!IsFirstPersonContext())
        {
            swingDegrees = Mathf.Max(swingDegrees, 72f);
            forward = Mathf.Max(forward, 0.28f);
        }
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float wave = Mathf.Sin(t * Mathf.PI);
            float swing = wave * swingDegrees;
            root.localRotation = startRotation * Quaternion.Euler(swing, 0f, -swing * 0.2f);
            root.localPosition = startPosition + new Vector3(0f, -wave * 0.03f, wave * forward);
            yield return null;
        }

        root.localRotation = startRotation;
        root.localPosition = startPosition;
        attackPlaying = false;
        onComplete?.Invoke();
    }

    IEnumerator ProceduralRecoilRoutine(System.Action onComplete)
    {
        attackPlaying = true;
        Transform root = GetAnimationRoot();
        Vector3 startPosition = root.localPosition;
        Quaternion startRotation = root.localRotation;
        float duration = Mathf.Max(0.12f, profile.FallbackAttackDuration * 0.75f);
        float recoil = profile.FallbackRecoilDistance;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float wave = Mathf.Sin(t * Mathf.PI);
            root.localPosition = startPosition + new Vector3(0f, wave * 0.02f, -wave * recoil);
            root.localRotation = startRotation * Quaternion.Euler(-wave * 8f, 0f, 0f);
            yield return null;
        }

        root.localPosition = startPosition;
        root.localRotation = startRotation;
        attackPlaying = false;
        onComplete?.Invoke();
    }

    void EnsureVisualRoot()
    {
        if (visualRoot != null)
        {
            return;
        }

        Transform found = transform.Find("VisualRoot");
        visualRoot = found != null ? found : transform;
    }

    void EnsureAnimationComponent()
    {
        if (legacyAnimation != null)
        {
            return;
        }

        legacyAnimation = GetComponent<Animation>();
        if (legacyAnimation == null)
        {
            legacyAnimation = gameObject.AddComponent<Animation>();
        }
    }

    bool IsFirstPersonContext()
    {
        Transform parent = transform.parent;
        while (parent != null)
        {
            if (parent.name.Contains("FirstPerson", System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            parent = parent.parent;
        }

        return false;
    }
}
