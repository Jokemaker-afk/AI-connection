using System.Collections.Generic;
using UnityEngine;

public static class EnemyCatalog
{
    static readonly Dictionary<EnemyKind, EnemyData> Enemies = new Dictionary<EnemyKind, EnemyData>();

    static EnemyCatalog()
    {
        RegisterDefaults();
    }

    public static EnemyData Get(EnemyKind kind)
    {
        return Enemies.TryGetValue(kind, out EnemyData data) ? data : default;
    }

    public static bool TryGet(EnemyKind kind, out EnemyData data)
    {
        return Enemies.TryGetValue(kind, out data);
    }

    static void Register(EnemyData data)
    {
        if (!data.IsValid)
        {
            Debug.LogWarning($"[EnemyCatalog] Skipped invalid enemy registration: {data.Kind}");
            return;
        }

        Enemies[data.Kind] = data;
    }

    static void RegisterDefaults()
    {
        Register(new EnemyData
        {
            Kind = EnemyKind.TrainingDummy,
            DisplayNameChinese = "训练假人",
            DescriptionChinese = "静止训练目标，用于近战武器测试。",
            MaxHealth = 50f,
            MoveSpeed = 0f,
            KnockbackResistance = 0.2f,
            KnockbackForceMultiplier = 1f,
            GroundOnly = true,
            CanMove = false,
            DestroyOnDeath = true,
            DestroyDelay = 0.6f,
            TaskIdOnFirstHit = "hit_melee_dummy",
            PlaceholderColor = new Color(0.82f, 0.38f, 0.36f),
            PlaceholderScale = new Vector3(0.9f, 1.4f, 0.9f),
        });

        Register(new EnemyData
        {
            Kind = EnemyKind.TrainingRangedTarget,
            DisplayNameChinese = "远程训练靶",
            DescriptionChinese = "远处训练靶，用于远程武器测试。",
            MaxHealth = 40f,
            MoveSpeed = 0f,
            KnockbackResistance = 0.35f,
            KnockbackForceMultiplier = 0.85f,
            GroundOnly = true,
            CanMove = false,
            DestroyOnDeath = true,
            DestroyDelay = 0.6f,
            TaskIdOnFirstHit = "hit_ranged_target",
            PlaceholderColor = new Color(0.92f, 0.72f, 0.28f),
            PlaceholderScale = new Vector3(1.2f, 0.2f, 1.2f),
        });

        Register(new EnemyData
        {
            Kind = EnemyKind.BasicWalker,
            DisplayNameChinese = "基础敌人",
            DescriptionChinese = "简单地面敌人，可移动、可被击退。",
            MaxHealth = 80f,
            MoveSpeed = 1.6f,
            KnockbackResistance = 0.15f,
            KnockbackForceMultiplier = 1f,
            DetectionRange = 12f,
            AttackRange = 1.5f,
            AttackDamage = 0f,
            GroundOnly = true,
            CanMove = true,
            DestroyOnDeath = true,
            DestroyDelay = 0.75f,
            EventIdOnDeath = "defeat_basic_walker",
            PlaceholderColor = new Color(0.72f, 0.22f, 0.28f),
            PlaceholderScale = new Vector3(1f, 1.2f, 1f),
        });
    }
}
