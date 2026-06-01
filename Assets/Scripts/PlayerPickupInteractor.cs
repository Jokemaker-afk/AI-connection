using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPickupInteractor : MonoBehaviour
{
    [SerializeField] float pickupRange = 3.5f;
    [SerializeField] float pickupAngle = 0.1f;

    PlayerInventory inventory;
    WorldPickupItem currentTarget;

    public WorldPickupItem CurrentTarget => currentTarget;
    public bool HasTarget => currentTarget != null && !currentTarget.IsCollected;

    void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
        if (inventory == null)
        {
            inventory = gameObject.AddComponent<PlayerInventory>();
        }

        // Avoid too strict values from scene-serialized overrides.
        pickupRange = Mathf.Max(pickupRange, 3.5f);
        pickupAngle = Mathf.Min(pickupAngle, 0.1f);
    }

    void Update()
    {
        UpdateTarget();

        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.fKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryPickupCurrentTarget();
        }
    }

    public bool CanPickupCurrentTarget()
    {
        return HasTarget
            && inventory != null
            && inventory.CanAcceptItem(currentTarget.ItemKind, currentTarget.Amount);
    }

    void UpdateTarget()
    {
        currentTarget = FindBestPickupTarget();
    }

    WorldPickupItem FindBestPickupTarget()
    {
        var pickups = FindObjectsByType<WorldPickupItem>();
        if (pickups == null || pickups.Length == 0)
        {
            return null;
        }

        Vector3 origin = transform.position + Vector3.up * 1f;
        Vector3 forward = transform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f)
        {
            forward = Vector3.forward;
        }
        else
        {
            forward.Normalize();
        }

        WorldPickupItem best = null;
        float bestScore = float.MaxValue;

        for (int i = 0; i < pickups.Length; i++)
        {
            var pickup = pickups[i];
            if (pickup == null || pickup.IsCollected || !pickup.gameObject.activeInHierarchy)
            {
                continue;
            }

            Vector3 toPickup = pickup.transform.position - origin;
            float distance = toPickup.magnitude;
            if (distance > pickupRange)
            {
                continue;
            }

            Vector3 flatDir = toPickup;
            flatDir.y = 0f;
            if (flatDir.sqrMagnitude > 0.001f)
            {
                flatDir.Normalize();
            }
            else
            {
                flatDir = forward;
            }

            float facing = Vector3.Dot(forward, flatDir);
            // Prioritize facing target, but still allow side pickups in close range.
            bool inFront = facing >= pickupAngle;
            float score = inFront ? distance : distance + 1.5f;
            if (score < bestScore)
            {
                bestScore = score;
                best = pickup;
            }
        }

        return best;
    }

    public bool TryPickupCurrentTarget()
    {
        if (inventory == null)
        {
            return false;
        }

        if (HasTarget && currentTarget.TryCollect(inventory))
        {
            return true;
        }

        var fallback = FindClosestPickupInRange();
        if (fallback != null && fallback.TryCollect(inventory))
        {
            currentTarget = fallback;
            return true;
        }

        return false;
    }

    WorldPickupItem FindClosestPickupInRange()
    {
        var pickups = FindObjectsByType<WorldPickupItem>();
        if (pickups == null || pickups.Length == 0)
        {
            return null;
        }

        Vector3 origin = transform.position + Vector3.up * 1f;
        float bestDistance = float.MaxValue;
        WorldPickupItem best = null;

        for (int i = 0; i < pickups.Length; i++)
        {
            var pickup = pickups[i];
            if (pickup == null || pickup.IsCollected || !pickup.gameObject.activeInHierarchy)
            {
                continue;
            }

            float distance = Vector3.Distance(origin, pickup.transform.position);
            if (distance > pickupRange + 0.8f)
            {
                continue;
            }

            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = pickup;
            }
        }

        return best;
    }
}
