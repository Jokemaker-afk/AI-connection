using UnityEngine;

public static class EnemyTargetingUtility
{
    public static bool TryResolveEnemyFromCollider(
        Collider hitCollider,
        out EnemyController enemy,
        out Collider primaryCollider,
        out IDamageable damageable)
    {
        enemy = null;
        primaryCollider = hitCollider;
        damageable = null;

        if (hitCollider == null)
        {
            return false;
        }

        enemy = hitCollider.GetComponentInParent<EnemyController>();
        damageable = hitCollider.GetComponentInParent<IDamageable>();
        if (enemy == null || damageable == null || damageable.IsDead)
        {
            return false;
        }

        Collider rootCollider = enemy.GetComponent<Collider>();
        if (rootCollider != null)
        {
            primaryCollider = rootCollider;
        }

        return true;
    }

    public static Vector3 GetAimPoint(EnemyController enemy, Collider collider)
    {
        if (collider != null)
        {
            return collider.bounds.center;
        }

        if (enemy == null)
        {
            return Vector3.zero;
        }

        Renderer[] renderers = enemy.GetComponentsInChildren<Renderer>(true);
        if (renderers != null && renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
            }

            return bounds.center;
        }

        return enemy.transform.position + Vector3.up * 1f;
    }

    public static EnemySurfaceHighlighter GetOrCreateHighlighter(EnemyController enemy)
    {
        if (enemy == null)
        {
            return null;
        }

        EnemySurfaceHighlighter highlighter = enemy.GetComponent<EnemySurfaceHighlighter>();
        if (highlighter == null)
        {
            highlighter = enemy.gameObject.AddComponent<EnemySurfaceHighlighter>();
        }

        Transform visualRoot = enemy.transform.Find("VisualRoot");
        highlighter.BindVisualRoot(visualRoot != null ? visualRoot : enemy.transform);
        return highlighter;
    }
}
