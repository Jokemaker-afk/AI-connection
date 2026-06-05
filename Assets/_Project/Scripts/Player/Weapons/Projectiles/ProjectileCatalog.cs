using UnityEngine;

public static class ProjectileCatalog
{
    public static bool TryGet(ProjectileKind kind, out ProjectileData data)
    {
        switch (kind)
        {
            case ProjectileKind.BasicBullet:
                data = new ProjectileData
                {
                    Kind = ProjectileKind.BasicBullet,
                    DisplayNameChinese = "基础子弹",
                    Speed = 28f,
                    MaxLifetime = 4f,
                    MaxDistance = 35f,
                    Radius = 0.08f,
                    DestroyOnHit = true,
                    CanPierce = false,
                    MaxPierceCount = 0,
                    UseGravity = false,
                    GravityScale = 0f,
                    VisualColor = new Color(1f, 0.92f, 0.35f),
                };
                return true;
            default:
                data = default;
                return false;
        }
    }
}
