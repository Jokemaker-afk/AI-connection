/// <summary>
/// Future weapon classification (firearms, melee weapons, etc.).
/// Basic tools use <see cref="ToolKind"/>; weapons will use this when the gun/combat system is added.
/// </summary>
public enum WeaponKind
{
    None = 0,
    Melee,
    Firearm,
    Bow,
}
