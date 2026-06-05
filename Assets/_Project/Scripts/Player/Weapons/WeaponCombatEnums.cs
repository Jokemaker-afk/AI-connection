public enum WeaponAttackMode
{
    None = 0,
    MeleeSwing,
    MeleeThrust,
    Hitscan,
    Projectile,
    AreaOfEffect,
}

public enum WeaponTargetMode
{
    SingleTarget,
    MultiTarget,
    Cone,
    Sphere,
    Ray,
}

/// <summary>
/// How weapon enemy selection resolves aim. Pickup/tool systems use their own assist.
/// </summary>
public enum WeaponTargetingMode
{
    StrictCrosshair,
}
