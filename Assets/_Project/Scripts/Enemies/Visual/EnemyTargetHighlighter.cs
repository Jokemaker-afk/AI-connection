using UnityEngine;

public enum EnemyHighlightLevel
{
    None = 0,
    InAttackArea,
    Primary,
}

/// <summary>
/// Legacy wrapper — forwards to <see cref="EnemySurfaceHighlighter"/> (no cube outline).
/// </summary>
[DisallowMultipleComponent]
public class EnemyTargetHighlighter : MonoBehaviour
{
    EnemySurfaceHighlighter surfaceHighlighter;

    public EnemyHighlightLevel CurrentLevel => surfaceHighlighter != null
        ? surfaceHighlighter.CurrentLevel
        : EnemyHighlightLevel.None;

    void Awake()
    {
        EnsureSurfaceHighlighter();
        DestroyLegacyOutline();
    }

    public void SetHighlight(EnemyHighlightLevel level)
    {
        EnsureSurfaceHighlighter();
        surfaceHighlighter.SetHighlight(level);
    }

    public void ClearHighlight()
    {
        if (surfaceHighlighter != null)
        {
            surfaceHighlighter.ClearHighlight();
        }
    }

    void EnsureSurfaceHighlighter()
    {
        if (surfaceHighlighter != null)
        {
            return;
        }

        surfaceHighlighter = GetComponent<EnemySurfaceHighlighter>();
        if (surfaceHighlighter == null)
        {
            surfaceHighlighter = gameObject.AddComponent<EnemySurfaceHighlighter>();
        }

        Transform visualRoot = transform.Find("VisualRoot");
        surfaceHighlighter.BindVisualRoot(visualRoot != null ? visualRoot : transform);
    }

    void DestroyLegacyOutline()
    {
        Transform legacy = transform.Find("CombatTargetOutline");
        if (legacy != null)
        {
            Destroy(legacy.gameObject);
        }
    }
}
