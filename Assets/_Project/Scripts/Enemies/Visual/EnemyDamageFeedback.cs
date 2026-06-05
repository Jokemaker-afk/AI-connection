using UnityEngine;

public static class EnemyDamageFeedback
{
    public static void Show(Vector3 worldPosition, float damage)
    {
        if (damage <= 0f)
        {
            return;
        }

        var labelRoot = new GameObject("DamagePopup");
        labelRoot.transform.position = worldPosition + Vector3.up * 1.2f;
        ItemWorldLabel.Create(labelRoot.transform, $"-{Mathf.CeilToInt(damage)}", Vector3.zero, 0.14f);
        Object.Destroy(labelRoot, 0.85f);
    }
}
