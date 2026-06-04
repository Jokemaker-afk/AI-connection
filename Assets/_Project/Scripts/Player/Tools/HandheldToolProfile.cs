using System;
using UnityEngine;

[Serializable]
public struct HandheldToolProfile
{
    public ToolKind ToolKind;
    public GameObject HandheldPrefab;
    public Vector3 FirstPersonLocalPosition;
    public Vector3 FirstPersonLocalEuler;
    public Vector3 FirstPersonLocalScale;
    public Vector3 ThirdPersonLocalPosition;
    public Vector3 ThirdPersonLocalEuler;
    public Vector3 ThirdPersonLocalScale;
    public ToolAnimationProfile Animations;
    public float UseCooldown;
    public bool FallbackSwingEnabled;
    public bool HasDurability;
    public int MaxDurability;

    public bool IsTool => ToolKind != ToolKind.None;
}
