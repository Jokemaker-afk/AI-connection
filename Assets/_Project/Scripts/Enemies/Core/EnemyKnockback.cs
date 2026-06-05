using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class EnemyKnockback : MonoBehaviour
{
    [SerializeField] float knockbackResistance;
    [SerializeField] float knockbackForceMultiplier = 1f;
    [SerializeField] float maxKnockbackSpeed = 6f;

    Rigidbody body;
    EnemyHealth health;
    float knockbackTimer;

    public void Configure(EnemyData data)
    {
        knockbackResistance = Mathf.Clamp01(data.KnockbackResistance);
        knockbackForceMultiplier = Mathf.Max(0.1f, data.KnockbackForceMultiplier);
    }

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        health = GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.OnDamaged += HandleDamaged;
        }
    }

    void OnDestroy()
    {
        if (health != null)
        {
            health.OnDamaged -= HandleDamaged;
        }
    }

    void FixedUpdate()
    {
        if (knockbackTimer > 0f)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            return;
        }

        if (body == null || health == null || health.IsDead)
        {
            return;
        }

        Vector3 velocity = body.linearVelocity;
        velocity.x = Mathf.MoveTowards(velocity.x, 0f, Time.fixedDeltaTime * 8f);
        velocity.z = Mathf.MoveTowards(velocity.z, 0f, Time.fixedDeltaTime * 8f);
        body.linearVelocity = velocity;
    }

    void HandleDamaged(DamageInfo info)
    {
        if (body == null || health != null && health.IsDead)
        {
            return;
        }

        float resistance = Mathf.Clamp01(knockbackResistance);
        float force = info.KnockbackForce * knockbackForceMultiplier * (1f - resistance);
        if (force <= 0.01f)
        {
            return;
        }

        Vector3 impulse = info.HitDirection * force;
        impulse.y = 0f;
        body.AddForce(impulse, ForceMode.Impulse);

        Vector3 velocity = body.linearVelocity;
        velocity.y = 0f;
        if (velocity.magnitude > maxKnockbackSpeed)
        {
            velocity = velocity.normalized * maxKnockbackSpeed;
        }

        body.linearVelocity = new Vector3(velocity.x, body.linearVelocity.y, velocity.z);
        knockbackTimer = 0.2f;
    }
}
