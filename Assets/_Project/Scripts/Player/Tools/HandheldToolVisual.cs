using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class HandheldToolVisual : MonoBehaviour
{
    [SerializeField] ItemKind sourceItem = ItemKind.None;
    [SerializeField] ToolAnimationProfile animationProfile;
    [SerializeField] Transform visualRoot;
    [SerializeField] Animation legacyAnimation;
    [SerializeField] bool preferProceduralSwing = true;
    [SerializeField] float proceduralSwingDegrees = 62f;
    [SerializeField] float proceduralSwingForward = 0.18f;
    [SerializeField] float proceduralSwingDuration = 0.38f;

    bool useAnimationPlaying;
    Coroutine proceduralRoutine;

    public ItemKind SourceItem => sourceItem;
    public bool IsUseAnimationPlaying => useAnimationPlaying;

    public void Configure(ItemKind itemKind, ToolAnimationProfile profile, bool fallbackSwingEnabled = true)
    {
        sourceItem = itemKind;
        animationProfile = profile;
        preferProceduralSwing = fallbackSwingEnabled;
        EnsureVisualRoot();
        EnsureAnimationComponent();
    }

    void Awake()
    {
        EnsureVisualRoot();
        EnsureAnimationComponent();
    }

    public void PlayEquipAnimation()
    {
        if (!preferProceduralSwing && TryPlayClip("Equip"))
        {
            HandheldToolDebug.Log($"Playing equip clip for {ItemKindUtility.GetDisplayName(sourceItem)}.", this);
            return;
        }

        PlayProceduralSwing(0.2f, 18f);
    }

    public void PlayUseAnimation(System.Action onComplete = null)
    {
        HandheldToolDebug.Log($"Playing tool swing animation for {ItemKindUtility.GetDisplayName(sourceItem)}.", this);

        if (!preferProceduralSwing && TryPlayClip("Use"))
        {
            HandheldToolDebug.Log("Using Animation clip for tool swing.", this);
            if (onComplete != null)
            {
                float duration = animationProfile.UseAnimationClip != null
                    ? animationProfile.UseAnimationClip.length
                    : proceduralSwingDuration;
                StartCoroutine(WaitForAnimation(onComplete, duration));
            }

            return;
        }

        HandheldToolDebug.Log("Using fallback procedural swing animation.", this);
        if (proceduralRoutine != null)
        {
            StopCoroutine(proceduralRoutine);
        }

        proceduralRoutine = StartCoroutine(ProceduralUseRoutine(onComplete));
    }

    bool TryPlayClip(string clipName)
    {
        if (legacyAnimation == null || legacyAnimation.GetClip(clipName) == null)
        {
            return false;
        }

        legacyAnimation.Play(clipName);
        useAnimationPlaying = true;
        return true;
    }

    IEnumerator WaitForAnimation(System.Action onComplete, float duration)
    {
        useAnimationPlaying = true;
        yield return new WaitForSeconds(Mathf.Max(0.05f, duration));
        useAnimationPlaying = false;
        HandheldToolDebug.Log("Tool swing animation ended.", this);
        onComplete?.Invoke();
    }

    IEnumerator ProceduralUseRoutine(System.Action onComplete)
    {
        useAnimationPlaying = true;
        Transform root = visualRoot != null ? visualRoot : transform;
        Quaternion startRotation = root.localRotation;
        Vector3 startPosition = root.localPosition;
        float duration = proceduralSwingDuration;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float wave = Mathf.Sin(t * Mathf.PI);
            float swing = wave * proceduralSwingDegrees;
            root.localRotation = startRotation * Quaternion.Euler(swing, 0f, -swing * 0.25f);
            root.localPosition = startPosition + new Vector3(0f, -wave * 0.04f, wave * proceduralSwingForward);
            yield return null;
        }

        root.localRotation = startRotation;
        root.localPosition = startPosition;
        useAnimationPlaying = false;
        HandheldToolDebug.Log("Tool swing animation ended.", this);
        onComplete?.Invoke();
        proceduralRoutine = null;
    }

    void PlayProceduralSwing(float duration, float degrees)
    {
        if (proceduralRoutine != null)
        {
            StopCoroutine(proceduralRoutine);
        }

        proceduralRoutine = StartCoroutine(ProceduralSwingRoutine(duration, degrees));
    }

    IEnumerator ProceduralSwingRoutine(float duration, float degrees)
    {
        Transform root = visualRoot != null ? visualRoot : transform;
        Quaternion startRotation = root.localRotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float swing = Mathf.Sin(t * Mathf.PI) * degrees;
            root.localRotation = startRotation * Quaternion.Euler(0f, swing, 0f);
            yield return null;
        }

        root.localRotation = startRotation;
        proceduralRoutine = null;
    }

    void EnsureVisualRoot()
    {
        if (visualRoot != null)
        {
            return;
        }

        Transform existing = transform.Find("VisualRoot");
        visualRoot = existing != null ? existing : transform;
    }

    void EnsureAnimationComponent()
    {
        if (legacyAnimation == null)
        {
            legacyAnimation = GetComponent<Animation>();
        }

        if (legacyAnimation == null)
        {
            legacyAnimation = gameObject.AddComponent<Animation>();
        }

        if (legacyAnimation.GetClip("Use") == null && animationProfile.HasUseAnimation)
        {
            PrototypeAnimationFactory.SetupAnimationComponent(legacyAnimation, animationProfile);
        }
    }
}
