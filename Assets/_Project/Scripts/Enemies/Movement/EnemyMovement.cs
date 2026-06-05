using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class EnemyMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed = 1.5f;
    [SerializeField] float detectionRange = 10f;
    [SerializeField] bool canMove = true;
    [SerializeField] bool groundOnly = true;
    [SerializeField] float wanderRadius = 4f;
    [SerializeField] float retargetInterval = 2.5f;

    Rigidbody body;
    EnemyHealth health;
    Transform player;
    Vector3 wanderOrigin;
    Vector3 moveTarget;
    float retargetTimer;

    public void Configure(EnemyData data)
    {
        moveSpeed = Mathf.Max(0f, data.MoveSpeed);
        detectionRange = Mathf.Max(0f, data.DetectionRange);
        canMove = data.CanMove && moveSpeed > 0.01f;
        groundOnly = data.GroundOnly;
        wanderOrigin = transform.position;
        moveTarget = wanderOrigin;
        retargetTimer = 0f;
    }

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        health = GetComponent<EnemyHealth>();
        wanderOrigin = transform.position;
    }

    void Start()
    {
        var playerController = FindFirstObjectByType<PlayerController>();
        player = playerController != null ? playerController.transform : null;
        PickWanderTarget();
    }

    void FixedUpdate()
    {
        if (!canMove || body == null || health != null && health.IsDead)
        {
            StopHorizontalMotion();
            return;
        }

        retargetTimer -= Time.fixedDeltaTime;
        if (retargetTimer <= 0f)
        {
            retargetTimer = retargetInterval;
            UpdateMoveTarget();
        }

        Vector3 toTarget = moveTarget - transform.position;
        if (groundOnly)
        {
            toTarget.y = 0f;
        }

        if (toTarget.sqrMagnitude < 0.15f)
        {
            StopHorizontalMotion();
            return;
        }

        Vector3 direction = toTarget.normalized;
        Vector3 velocity = direction * moveSpeed;
        velocity.y = body.linearVelocity.y;
        body.linearVelocity = velocity;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion look = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.fixedDeltaTime * 6f);
        }
    }

    void UpdateMoveTarget()
    {
        if (player != null)
        {
            Vector3 flatToPlayer = player.position - transform.position;
            flatToPlayer.y = 0f;
            if (flatToPlayer.magnitude <= detectionRange)
            {
                moveTarget = player.position;
                return;
            }
        }

        PickWanderTarget();
    }

    void PickWanderTarget()
    {
        Vector2 circle = Random.insideUnitCircle * wanderRadius;
        moveTarget = wanderOrigin + new Vector3(circle.x, 0f, circle.y);
    }

    void StopHorizontalMotion()
    {
        if (body == null)
        {
            return;
        }

        Vector3 velocity = body.linearVelocity;
        velocity.x = 0f;
        velocity.z = 0f;
        body.linearVelocity = velocity;
    }
}
