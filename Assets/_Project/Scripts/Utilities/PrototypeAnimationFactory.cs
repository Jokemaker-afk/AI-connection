using System.Collections.Generic;
using UnityEngine;

public static class PrototypeAnimationFactory
{
    static readonly Dictionary<string, AnimationClip> ClipCache = new Dictionary<string, AnimationClip>();

    public static ToolAnimationProfile CreateToolAnimationProfile(ToolKind toolKind)
    {
        return new ToolAnimationProfile
        {
            IdleAnimationClip = GetOrCreateIdleClip(toolKind),
            EquipAnimationClip = GetOrCreateEquipClip(toolKind),
            UseAnimationClip = GetOrCreateUseClip(toolKind),
            HitFeedbackAnimationClip = GetOrCreateHitFeedbackClip(toolKind),
        };
    }

    public static AnimationClip GetOrCreateUseClip(ToolKind toolKind)
    {
        string key = $"{toolKind}_Use_Prototype";
        return GetOrCreateClip(key, () => BuildSwingClip(key, GetSwingAxis(toolKind), 0.38f, 42f));
    }

    public static AnimationClip GetOrCreateEquipClip(ToolKind toolKind)
    {
        string key = $"{toolKind}_Equip_Prototype";
        return GetOrCreateClip(key, () => BuildSwingClip(key, Vector3.up, 0.22f, 18f));
    }

    public static AnimationClip GetOrCreateIdleClip(ToolKind toolKind)
    {
        string key = $"{toolKind}_Idle_Prototype";
        return GetOrCreateClip(key, () => BuildIdleClip(key));
    }

    public static AnimationClip GetOrCreateHitFeedbackClip(ToolKind toolKind)
    {
        string key = $"{toolKind}_HitFeedback_Prototype";
        return GetOrCreateClip(key, () => BuildPulseClip(key, 0.24f, 0.08f));
    }

    public static AnimationClip GetOrCreateInteractableHitClip(string id)
    {
        string key = $"Interactable_{id}_Hit_Prototype";
        return GetOrCreateClip(key, () => BuildPulseClip(key, 0.2f, 0.06f));
    }

    public static AnimationClip GetOrCreateInteractableCompleteClip(string id)
    {
        string key = $"Interactable_{id}_Complete_Prototype";
        return GetOrCreateClip(key, () => BuildPulseClip(key, 0.35f, 0.12f));
    }

    public static AnimationClip GetOrCreateBeaconActivateClip()
    {
        return GetOrCreateClip("Beacon_Activate_Prototype", () => BuildPulseClip("Beacon_Activate_Prototype", 0.6f, 0.18f));
    }

    public static void SetupAnimationComponent(Animation animation, ToolAnimationProfile profile)
    {
        if (animation == null)
        {
            return;
        }

        animation.playAutomatically = false;

        if (profile.IdleAnimationClip != null)
        {
            animation.AddClip(profile.IdleAnimationClip, "Idle");
        }

        if (profile.EquipAnimationClip != null)
        {
            animation.AddClip(profile.EquipAnimationClip, "Equip");
        }

        if (profile.UseAnimationClip != null)
        {
            animation.AddClip(profile.UseAnimationClip, "Use");
        }
    }

    static AnimationClip GetOrCreateClip(string key, System.Func<AnimationClip> factory)
    {
        if (ClipCache.TryGetValue(key, out AnimationClip cached) && cached != null)
        {
            return cached;
        }

        AnimationClip clip = factory.Invoke();
        ClipCache[key] = clip;
        return clip;
    }

    static Vector3 GetSwingAxis(ToolKind toolKind)
    {
        switch (toolKind)
        {
            case ToolKind.Axe:
                return Vector3.right;
            case ToolKind.RepairTool:
                return Vector3.forward;
            default:
                return Vector3.right;
        }
    }

    static AnimationClip BuildSwingClip(string clipName, Vector3 axis, float duration, float peakDegrees)
    {
        var clip = new AnimationClip
        {
            name = clipName,
            legacy = true,
        };

        const string path = "VisualRoot";
        AddRotationCurve(clip, path, axis, duration, peakDegrees);
        clip.wrapMode = WrapMode.ClampForever;
        return clip;
    }

    static AnimationClip BuildIdleClip(string clipName)
    {
        var clip = new AnimationClip
        {
            name = clipName,
            legacy = true,
        };

        const string path = "VisualRoot";
        var curve = AnimationCurve.Constant(0f, 0.2f, 0f);
        clip.SetCurve(path, typeof(Transform), "localEulerAngles.x", curve);
        clip.SetCurve(path, typeof(Transform), "localEulerAngles.y", curve);
        clip.SetCurve(path, typeof(Transform), "localEulerAngles.z", curve);
        clip.wrapMode = WrapMode.Loop;
        return clip;
    }

    static AnimationClip BuildPulseClip(string clipName, float duration, float scaleDelta)
    {
        var clip = new AnimationClip
        {
            name = clipName,
            legacy = true,
        };

        var curve = new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(duration * 0.45f, 1f + scaleDelta),
            new Keyframe(duration, 1f));

        clip.SetCurve(string.Empty, typeof(Transform), "localScale.x", curve);
        clip.SetCurve(string.Empty, typeof(Transform), "localScale.y", curve);
        clip.SetCurve(string.Empty, typeof(Transform), "localScale.z", curve);
        clip.wrapMode = WrapMode.ClampForever;
        return clip;
    }

    static void AddRotationCurve(AnimationClip clip, string path, Vector3 axis, float duration, float peakDegrees)
    {
        axis.Normalize();
        var curve = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(duration * 0.35f, peakDegrees),
            new Keyframe(duration, 0f));

        if (Mathf.Abs(axis.x) > 0.5f)
        {
            clip.SetCurve(path, typeof(Transform), "localEulerAngles.x", curve);
        }
        else if (Mathf.Abs(axis.y) > 0.5f)
        {
            clip.SetCurve(path, typeof(Transform), "localEulerAngles.y", curve);
        }
        else
        {
            clip.SetCurve(path, typeof(Transform), "localEulerAngles.z", curve);
        }

        var moveCurve = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(duration * 0.35f, 0.08f),
            new Keyframe(duration, 0f));
        clip.SetCurve(path, typeof(Transform), "localPosition.z", moveCurve);
    }
}
