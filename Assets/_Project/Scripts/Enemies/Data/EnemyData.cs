using System;
using UnityEngine;

[Serializable]
public struct EnemyData
{
    public EnemyKind Kind;
    public string DisplayNameChinese;
    public string DescriptionChinese;
    public float MaxHealth;
    public float MoveSpeed;
    public float KnockbackResistance;
    public float KnockbackForceMultiplier;
    public float DetectionRange;
    public float AttackRange;
    public float AttackDamage;
    public bool GroundOnly;
    public bool CanMove;
    public bool DestroyOnDeath;
    public float DestroyDelay;
    public string TaskIdOnFirstHit;
    public string EventIdOnDeath;
    public GameObject EnemyPrefab;
    public GameObject PlaceholderPrefab;
    public Color PlaceholderColor;
    public Vector3 PlaceholderScale;

    public bool IsValid => Kind != EnemyKind.None && MaxHealth > 0f;
}
