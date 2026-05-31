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
    /// 将 Capsule 网格移到与 CharacterController 中心对齐的子物体，避免模型半截埋进地面。
    /// </summary>
    public static void AlignCapsuleVisual(GameObject player)
    {
        var controller = player.GetComponent<CharacterController>();
        if (controller == null)
        {
            return;
        }

        var existingVisual = player.transform.Find("Visual");
        if (existingVisual != null)
        {
            existingVisual.localPosition = controller.center;
            return;
        }

        var meshFilter = player.GetComponent<MeshFilter>();
        var meshRenderer = player.GetComponent<MeshRenderer>();
        if (meshFilter == null || meshRenderer == null)
        {
            return;
        }

        var visualGo = new GameObject("Visual");
        visualGo.transform.SetParent(player.transform, false);
        visualGo.transform.localPosition = controller.center;
        visualGo.transform.localRotation = Quaternion.identity;
        visualGo.transform.localScale = Vector3.one;

        var visualMeshFilter = visualGo.AddComponent<MeshFilter>();
        visualMeshFilter.sharedMesh = meshFilter.sharedMesh;

        var visualMeshRenderer = visualGo.AddComponent<MeshRenderer>();
        visualMeshRenderer.sharedMaterials = meshRenderer.sharedMaterials;

        if (Application.isPlaying)
        {
            Object.Destroy(meshFilter);
            Object.Destroy(meshRenderer);
        }
        else
        {
            Object.DestroyImmediate(meshFilter);
            Object.DestroyImmediate(meshRenderer);
        }
    }
}
