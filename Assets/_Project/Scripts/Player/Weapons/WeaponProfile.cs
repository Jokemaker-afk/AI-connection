using System;
using UnityEngine;

[Serializable]
public struct WeaponProfile
{
    public WeaponKind WeaponKind;
    public WeaponAttackMode AttackMode;
    public WeaponTargetMode TargetMode;
    public float Damage;
    public float AttackRange;
    public float AttackCooldown;
    public float KnockbackForce;
    public float AttackRadius;
    public float AttackAngle;
    public int MaxTargets;
    public float MeleeRadius;
    public bool IsMelee;
    public bool IsRanged;
    public bool UseFallbackAttackAnimation;
    public float FallbackAttackDuration;
    public float FallbackSwingDegrees;
    public float FallbackSwingForward;
    public float FallbackRecoilDistance;
    public GameObject ProjectilePrefab;
    public GameObject HitEffectPrefab;
    public GameObject MuzzleEffectPrefab;
    public AudioClip AttackSound;
    public AnimationClip FirstPersonAttackAnimation;
    public AnimationClip ThirdPersonAttackAnimation;
    public Vector3 FirstPersonLocalPosition;
    public Vector3 FirstPersonLocalEuler;
    public Vector3 FirstPersonLocalScale;
    public Vector3 ThirdPersonLocalPosition;
    public Vector3 ThirdPersonLocalEuler;
    public Vector3 ThirdPersonLocalScale;

    public bool IsWeapon => WeaponKind != WeaponKind.None;
}
