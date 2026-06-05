using UnityEngine;

/// <summary>
/// Ground-projected fan mesh showing melee attack range and angle. Visual only.
/// </summary>
[DisallowMultipleComponent]
public class MeleeAttackRangeIndicator : MonoBehaviour
{
    [Header("Display")]
    [SerializeField] bool showMeleeRangeIndicator = true;
    [SerializeField] bool showOnlyWhenMeleeWeaponEquipped = true;
    [SerializeField] bool showOnlyDuringAttack = false;

    [Header("Shape")]
    [SerializeField] int segmentCount = 24;
    [SerializeField] Color indicatorColor = new Color(1f, 0.82f, 0.2f, 0.38f);
    [SerializeField] float groundOffset = 0.03f;

    [Header("Debug")]
    [SerializeField] bool enableDebugLogs = true;

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    Mesh fanMesh;

    float currentRange;
    float currentAngle;
    bool meshDirty;

    void Awake()
    {
        BuildVisualRoot();
        gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (fanMesh != null)
        {
            Destroy(fanMesh);
        }
    }

    public void Hide(string reason = null)
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
            if (enableDebugLogs && !string.IsNullOrEmpty(reason))
            {
                Debug.Log($"[MeleeIndicator] Melee indicator hidden: {reason}");
            }
        }
    }

    public void ShowFromWeapon(WeaponProfile profile, string weaponLabel)
    {
        if (!showMeleeRangeIndicator || !profile.IsMelee)
        {
            Hide("non-melee weapon");
            return;
        }

        if (showOnlyDuringAttack)
        {
            Hide("show only during attack");
            return;
        }

        SetShape(profile.AttackRange, profile.AttackAngle);
        gameObject.SetActive(true);

        if (enableDebugLogs)
        {
            Debug.Log($"[MeleeIndicator] Melee range indicator active: {weaponLabel}");
            Debug.Log($"[MeleeIndicator] Melee range = {profile.AttackRange:F2} angle = {profile.AttackAngle:F1}");
        }
    }

    public void ShowBriefAttackFlash(WeaponProfile profile)
    {
        if (!showMeleeRangeIndicator)
        {
            return;
        }

        SetShape(profile.AttackRange, profile.AttackAngle);
        gameObject.SetActive(true);
    }

    public void UpdatePlacement(Vector3 origin, Vector3 flatForward)
    {
        if (!gameObject.activeSelf)
        {
            return;
        }

        Vector3 forward = flatForward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f)
        {
            return;
        }

        forward.Normalize();
        Vector3 groundPoint = ProjectToGround(origin);
        transform.position = groundPoint + Vector3.up * groundOffset;
        transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }

    public void UpdatePlacement(Vector3 origin, float aimYawDegrees)
    {
        UpdatePlacement(origin, Quaternion.Euler(0f, aimYawDegrees, 0f) * Vector3.forward);
    }

    void SetShape(float range, float angleDegrees)
    {
        if (Mathf.Approximately(currentRange, range) && Mathf.Approximately(currentAngle, angleDegrees) && !meshDirty)
        {
            return;
        }

        currentRange = range;
        currentAngle = angleDegrees;
        RebuildFanMesh(range, angleDegrees, segmentCount);
        meshDirty = false;
    }

    void BuildVisualRoot()
    {
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();

        Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.color = indicatorColor;
        material.SetFloat("_Surface", 1f);
        material.SetFloat("_Blend", 0f);
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.renderQueue = 3000;
        meshRenderer.sharedMaterial = material;

        GameplayLayers.TrySetIgnoreRaycastLayer(gameObject);
    }

    void RebuildFanMesh(float range, float angleDegrees, int segments)
    {
        if (fanMesh == null)
        {
            fanMesh = new Mesh { name = "MeleeFanMesh" };
        }
        else
        {
            fanMesh.Clear();
        }

        segments = Mathf.Max(4, segments);
        float halfAngleRad = angleDegrees * 0.5f * Mathf.Deg2Rad;
        int vertexCount = segments + 2;
        var vertices = new Vector3[vertexCount];
        var triangles = new int[segments * 3];
        var colors = new Color[vertexCount];

        vertices[0] = Vector3.zero;
        colors[0] = indicatorColor;

        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            float angle = Mathf.Lerp(-halfAngleRad, halfAngleRad, t);
            vertices[i + 1] = new Vector3(Mathf.Sin(angle) * range, 0f, Mathf.Cos(angle) * range);
            colors[i + 1] = indicatorColor;
        }

        int tri = 0;
        for (int i = 0; i < segments; i++)
        {
            triangles[tri++] = 0;
            triangles[tri++] = i + 1;
            triangles[tri++] = i + 2;
        }

        fanMesh.vertices = vertices;
        fanMesh.triangles = triangles;
        fanMesh.colors = colors;
        fanMesh.RecalculateNormals();
        fanMesh.RecalculateBounds();

        meshFilter.sharedMesh = fanMesh;
    }

    static Vector3 ProjectToGround(Vector3 origin)
    {
        Vector3 rayOrigin = origin + Vector3.up * 2f;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 6f, GameplayLayers.PickupLineOfSightObstacleMask, QueryTriggerInteraction.Ignore))
        {
            return hit.point;
        }

        return new Vector3(origin.x, 0f, origin.z);
    }
}
