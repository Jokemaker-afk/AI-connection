using UnityEngine;

[DisallowMultipleComponent]
public class PlayerVisualRoot : MonoBehaviour
{
    [SerializeField] Transform directionalModelRoot;
    [SerializeField] Transform legacyCapsuleRoot;

    public Transform DirectionalModelRoot => directionalModelRoot;
    public Transform LegacyCapsuleRoot => legacyCapsuleRoot;

    public void BindRoots(Transform directional, Transform legacy)
    {
        directionalModelRoot = directional;
        legacyCapsuleRoot = legacy;
    }
}
