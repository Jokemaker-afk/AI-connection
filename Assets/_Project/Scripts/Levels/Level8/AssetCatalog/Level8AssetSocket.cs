using UnityEngine;

/// <summary>
/// Marks socket transforms on chunk / prop prefabs for generator placement.
/// Name convention: ResourceSocket_01, PathSocket_North, etc.
/// </summary>
public class Level8AssetSocket : MonoBehaviour
{
    [SerializeField] Level8PropCategory category;
    [SerializeField] int socketIndex;
    [SerializeField] string socketLabel;

    public Level8PropCategory Category => category;
    public int SocketIndex => socketIndex;
    public string SocketLabel => string.IsNullOrEmpty(socketLabel) ? name : socketLabel;
}
