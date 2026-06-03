using UnityEngine;

public static class GameplayUiUtility
{
    public static Font GetUiFont()
    {
        return GameplayChineseText.GetUiFont();
    }

    public static void EnsureRootScale(RectTransform rect)
    {
        if (rect == null)
        {
            return;
        }

        if (rect.localScale.sqrMagnitude < 0.99f)
        {
            rect.localScale = Vector3.one;
        }
    }

    public static void DestroyForRebuild(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        Object.DestroyImmediate(target);
    }

    public static void DestroyForRebuild(Transform target)
    {
        if (target == null)
        {
            return;
        }

        DestroyForRebuild(target.gameObject);
    }
}
