using System;
using UnityEngine;

[Serializable]
public struct ToolAnimationProfile
{
    public AnimationClip IdleAnimationClip;
    public AnimationClip EquipAnimationClip;
    public AnimationClip UseAnimationClip;
    public AnimationClip HitFeedbackAnimationClip;

    public bool HasUseAnimation => UseAnimationClip != null;
}
