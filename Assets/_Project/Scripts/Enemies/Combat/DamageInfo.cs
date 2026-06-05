using UnityEngine;

public struct DamageInfo
{
    public float Damage;
    public Vector3 HitPoint;
    public Vector3 HitDirection;
    public float KnockbackForce;
    public GameObject Source;

    public static DamageInfo Create(float damage, Vector3 hitPoint, Vector3 hitDirection, float knockbackForce, GameObject source)
    {
        Vector3 flatDirection = hitDirection;
        flatDirection.y = 0f;
        if (flatDirection.sqrMagnitude < 0.001f && source != null)
        {
            flatDirection = hitPoint - source.transform.position;
            flatDirection.y = 0f;
        }

        if (flatDirection.sqrMagnitude > 0.001f)
        {
            flatDirection.Normalize();
        }
        else
        {
            flatDirection = Vector3.forward;
        }

        return new DamageInfo
        {
            Damage = Mathf.Max(0f, damage),
            HitPoint = hitPoint,
            HitDirection = flatDirection,
            KnockbackForce = Mathf.Max(0f, knockbackForce),
            Source = source,
        };
    }
}
