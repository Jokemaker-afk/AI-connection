using UnityEngine;

/// <summary>
/// Backward-compatible entry point. Prefer PlayerVisualBuilder.EnsurePlayerVisual().
/// </summary>
public static class PlayerDirectionalVisualBuilder
{
    public const string VisualRootName = PlayerVisualBuilder.VisualRootName;
    public const string ModelRootName = PlayerVisualBuilder.DirectionalModelName;
    public const string ThirdPersonToolSocketName = PlayerVisualBuilder.ThirdPersonToolSocketName;
    public const string FirstPersonToolSocketName = PlayerVisualBuilder.FirstPersonToolSocketName;

    public struct BuildResult
    {
        public Transform VisualRoot;
        public Transform ModelRoot;
        public Transform ThirdPersonToolSocket;
        public Transform FirstPersonToolSocket;
        public Renderer[] Renderers;
    }

    public static BuildResult EnsureDirectionalVisual(GameObject player)
    {
        PlayerVisualBuilder.BuildResult result = PlayerVisualBuilder.EnsurePlayerVisual(player);
        return new BuildResult
        {
            VisualRoot = result.VisualRoot,
            ModelRoot = result.DirectionalModelRoot,
            ThirdPersonToolSocket = result.ThirdPersonToolSocket,
            FirstPersonToolSocket = result.FirstPersonToolSocket,
            Renderers = result.ActiveRenderers,
        };
    }
}
