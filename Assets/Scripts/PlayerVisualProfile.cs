using UnityEngine;

[CreateAssetMenu(fileName = "PlayerVisualProfile", menuName = "Gameplay/Player Visual Profile")]
public class PlayerVisualProfile : ScriptableObject
{
    [Header("Identity")]
    public PlayerVisualKind VisualKind = PlayerVisualKind.DirectionalBlock;
    public GameObject VisualPrefab;

    [Header("Visibility")]
    public bool VisibleInFirstPerson = false;
    public bool VisibleInThirdPerson = true;

    [Header("Layout")]
    public float ModelHeight = 2f;
    public float ColliderHeightSuggestion = 2f;
    public Vector3 FrontDirectionOffset = Vector3.forward;
    public Vector3 CameraLookAtOffset = new Vector3(0f, 1.55f, 0f);

    [Header("Tool Sockets (local to PlayerRoot)")]
    public Vector3 FirstPersonToolSocketLocalPosition = new Vector3(0.28f, 1.05f, 0.42f);
    public Vector3 FirstPersonToolSocketLocalEuler = new Vector3(10f, -12f, 0f);
    public Vector3 ThirdPersonToolSocketLocalPosition = new Vector3(0.45f, 0.5f, 0.35f);
    public Vector3 ThirdPersonToolSocketLocalEuler = new Vector3(10f, -25f, 8f);

    public static PlayerVisualProfile CreateBuiltin(PlayerVisualKind kind)
    {
        var profile = CreateInstance<PlayerVisualProfile>();
        profile.VisualKind = kind;

        switch (kind)
        {
            case PlayerVisualKind.LegacyCapsule:
                profile.VisibleInFirstPerson = false;
                profile.VisibleInThirdPerson = false;
                profile.ModelHeight = 2f;
                profile.ColliderHeightSuggestion = 2f;
                profile.CameraLookAtOffset = new Vector3(0f, 1f, 0f);
                break;
            case PlayerVisualKind.RealCharacterModel:
                profile.VisibleInFirstPerson = false;
                profile.VisibleInThirdPerson = true;
                profile.ModelHeight = 1.8f;
                profile.ColliderHeightSuggestion = 1.8f;
                profile.CameraLookAtOffset = new Vector3(0f, 1.5f, 0f);
                break;
            default:
                profile.VisibleInFirstPerson = false;
                profile.VisibleInThirdPerson = true;
                profile.ModelHeight = 2f;
                profile.ColliderHeightSuggestion = 2f;
                profile.CameraLookAtOffset = new Vector3(0f, 1.55f, 0f);
                break;
        }

        return profile;
    }
}
