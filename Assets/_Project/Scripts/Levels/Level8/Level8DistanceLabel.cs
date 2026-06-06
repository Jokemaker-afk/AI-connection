using UnityEngine;

/// <summary>World label that shows/hides based on horizontal distance to the player.</summary>
[DisallowMultipleComponent]
public class Level8DistanceLabel : MonoBehaviour
{
    [SerializeField] float maxVisibleDistance = 50f;
    [SerializeField] bool alwaysVisible;

    Transform labelVisual;
    float checkTimer;

    public float MaxVisibleDistance
    {
        get => maxVisibleDistance;
        set => maxVisibleDistance = value;
    }

    public void Configure(float maxDistance, bool forceAlwaysVisible = false)
    {
        maxVisibleDistance = maxDistance;
        alwaysVisible = forceAlwaysVisible;
        EnsureLabelVisual();
        RefreshVisibility(true);
    }

    void Awake()
    {
        EnsureLabelVisual();
    }

    void EnsureLabelVisual()
    {
        if (labelVisual != null)
        {
            return;
        }

        labelVisual = transform.Find("Label");
        if (labelVisual == null && transform.childCount > 0)
        {
            labelVisual = transform.GetChild(0);
        }
    }

    void Update()
    {
        checkTimer -= Time.deltaTime;
        if (checkTimer > 0f)
        {
            return;
        }

        checkTimer = 0.25f;
        RefreshVisibility(false);
    }

    void RefreshVisibility(bool force)
    {
        EnsureLabelVisual();
        if (labelVisual == null)
        {
            return;
        }

        bool visible = alwaysVisible || IsPlayerWithinRange();
        if (force || labelVisual.gameObject.activeSelf != visible)
        {
            labelVisual.gameObject.SetActive(visible);
        }
    }

    bool IsPlayerWithinRange()
    {
        Vector3? playerPos = ResolvePlayerPosition();
        if (!playerPos.HasValue)
        {
            return true;
        }

        Vector3 flat = transform.position - playerPos.Value;
        flat.y = 0f;
        return flat.magnitude <= maxVisibleDistance;
    }

    static Vector3? ResolvePlayerPosition()
    {
        if (PersistentPlayerRig.Player != null)
        {
            return PersistentPlayerRig.Player.transform.position;
        }

        var controller = Object.FindFirstObjectByType<PlayerController>();
        return controller != null ? controller.transform.position : null;
    }
}
