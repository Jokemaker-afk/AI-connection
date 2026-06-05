using System;
using UnityEngine;

[Serializable]
public struct ProjectileData
{
    public ProjectileKind Kind;
    public string DisplayNameChinese;
    public float Speed;
    public float MaxLifetime;
    public float MaxDistance;
    public float Radius;
    public bool DestroyOnHit;
    public bool CanPierce;
    public int MaxPierceCount;
    public bool UseGravity;
    public float GravityScale;
    public Color VisualColor;

    public bool IsValid => Kind != ProjectileKind.None;
}
