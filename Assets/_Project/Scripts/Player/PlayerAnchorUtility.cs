using UnityEngine;

public static class PlayerAnchorUtility
{
    /// <summary>
    /// Transform.position 到 CharacterController 脚底的本地 Y 偏移。
    /// 脚底世界高度 = transform.position.y + GetFootOffsetY(controller)
    /// </summary>
    public static float GetFootOffsetY(CharacterController controller)
    {
        return controller.center.y - controller.height * 0.5f;
    }

    public static float GetSpawnYForGround(float groundY, CharacterController controller)
    {
        return groundY - GetFootOffsetY(controller);
    }

    public static Vector3 GetSpawnPositionForGround(Vector3 xz, float groundY, CharacterController controller)
    {
        return new Vector3(xz.x, GetSpawnYForGround(groundY, controller), xz.z);
    }

    public static void EnsureStandardController(CharacterController controller)
    {
        controller.height = 2f;
        controller.radius = 0.35f;
        controller.center = new Vector3(0f, 1f, 0f);
    }

    /// <summary>
    /// Legacy entry point — now builds the directional placeholder visual.
    /// </summary>
    public static void AlignCapsuleVisual(GameObject player)
    {
        PlayerVisualBuilder.EnsurePlayerVisual(player);
    }
}
