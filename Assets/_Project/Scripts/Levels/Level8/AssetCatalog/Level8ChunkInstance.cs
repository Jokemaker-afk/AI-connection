using UnityEngine;

/// <summary>
/// Authoring marker on chunk prefabs. Generator reads biome/kind/variant for catalog validation.
/// </summary>
public class Level8ChunkInstance : MonoBehaviour
{
    [SerializeField] Level8BiomeKind biome = Level8BiomeKind.Plain;
    [SerializeField] Level8ChunkKind chunkKind = Level8ChunkKind.Path;
    [SerializeField] string variantId = "01";
    [SerializeField] float footprintSize = Level8GenerationFlags.ChunkSize;

    public Level8BiomeKind Biome => biome;
    public Level8ChunkKind ChunkKind => chunkKind;
    public string VariantId => variantId;
    public float FootprintSize => footprintSize;
}
