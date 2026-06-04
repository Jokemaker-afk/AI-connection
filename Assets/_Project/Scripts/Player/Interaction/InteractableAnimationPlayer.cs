using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class InteractableAnimationPlayer : MonoBehaviour
{
    [SerializeField] Transform animationRoot;
    Animation legacyAnimation;
    Coroutine proceduralRoutine;

    public void BindRoot(Transform root)
    {
        animationRoot = root != null ? root : transform;
        EnsureAnimationComponent();
    }

    void Awake()
    {
        if (animationRoot == null)
        {
            animationRoot = transform;
        }

        EnsureAnimationComponent();
    }

    public void PlayHitFeedback(AnimationClip clipOverride = null)
    {
        AnimationClip clip = clipOverride;
        if (clip == null)
        {
            clip = PrototypeAnimationFactory.GetOrCreateInteractableHitClip(gameObject.name);
        }

        if (TryPlayClip(clip, "HitFeedback"))
        {
            return;
        }

        PlayProceduralPulse(0.18f, 0.06f);
    }

    public void PlayCompleted(AnimationClip clipOverride = null)
    {
        AnimationClip clip = clipOverride;
        if (clip == null)
        {
            clip = PrototypeAnimationFactory.GetOrCreateInteractableCompleteClip(gameObject.name);
        }

        if (TryPlayClip(clip, "Completed"))
        {
            return;
        }

        PlayProceduralPulse(0.28f, 0.1f);
    }

    bool TryPlayClip(AnimationClip clip, string clipName)
    {
        if (clip == null || animationRoot == null)
        {
            return false;
        }

        EnsureAnimationComponent();
        if (legacyAnimation.GetClip(clipName) == null)
        {
            legacyAnimation.AddClip(clip, clipName);
        }

        legacyAnimation.Play(clipName);
        return true;
    }

    void PlayProceduralPulse(float duration, float scaleDelta)
    {
        if (proceduralRoutine != null)
        {
            StopCoroutine(proceduralRoutine);
        }

        proceduralRoutine = StartCoroutine(PulseRoutine(duration, scaleDelta));
    }

    IEnumerator PulseRoutine(float duration, float scaleDelta)
    {
        Transform root = animationRoot != null ? animationRoot : transform;
        Vector3 baseScale = root.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float pulse = 1f + Mathf.Sin(t * Mathf.PI) * scaleDelta;
            root.localScale = baseScale * pulse;
            yield return null;
        }

        root.localScale = baseScale;
        proceduralRoutine = null;
    }

    void EnsureAnimationComponent()
    {
        if (animationRoot == null)
        {
            animationRoot = transform;
        }

        legacyAnimation = animationRoot.GetComponent<Animation>();
        if (legacyAnimation == null)
        {
            legacyAnimation = animationRoot.gameObject.AddComponent<Animation>();
            legacyAnimation.playAutomatically = false;
        }
    }
}
