using UnityEngine;

public static class ItemDropUtility
{
    public const float DropDistance = 1.5f;
    public const float DropHeightOffset = 0.55f;
    public const float GroundRayHeight = 2.5f;
    public const float GroundRayDistance = 6f;

    public static Vector3 ResolveDropPosition(Transform player, Camera camera)
    {
        if (player == null)
        {
            return Vector3.zero;
        }

        Vector3 forward = GetDropForward(player, camera);
        Vector3 origin = player.position + Vector3.up * DropHeightOffset;
        Vector3 candidate = origin + forward * DropDistance;

        Vector3 rayStart = candidate + Vector3.up * GroundRayHeight;
        if (Physics.Raycast(
            rayStart,
            Vector3.down,
            out RaycastHit hit,
            GroundRayHeight + GroundRayDistance,
            ~GameplayLayers.TargetingIgnoreMask,
            QueryTriggerInteraction.Ignore))
        {
            return hit.point + Vector3.up * 0.12f;
        }

        return candidate;
    }

    public static Vector3 GetDropForward(Transform player, Camera camera)
    {
        if (camera != null)
        {
            var cameraController = camera.GetComponent<PlayerCameraController>();
            if (cameraController != null)
            {
                Vector3 aimForward = cameraController.GetImmediateAimForward();
                aimForward.y = 0f;
                if (aimForward.sqrMagnitude > 0.001f)
                {
                    return aimForward.normalized;
                }
            }

            Vector3 camForward = camera.transform.forward;
            camForward.y = 0f;
            if (camForward.sqrMagnitude > 0.001f)
            {
                return camForward.normalized;
            }
        }

        Vector3 playerForward = player.forward;
        playerForward.y = 0f;
        return playerForward.sqrMagnitude > 0.001f ? playerForward.normalized : Vector3.forward;
    }
}
