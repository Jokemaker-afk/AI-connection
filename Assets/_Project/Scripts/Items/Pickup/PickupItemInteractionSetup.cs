using UnityEngine;

/// <summary>
/// Ensures world/dropped pickup items have raycast-friendly colliders and correct layer.
/// Pickup-only — never adds placed/building/workstation components.
/// </summary>
public static class PickupItemInteractionSetup
{
    const float MinSolidRadius = 0.42f;
    const float GroundSolidRadius = 0.55f;
    const float TriggerRadius = 0.5f;
    const float GroundColliderCenterY = 0.28f;

    public static void Configure(GameObject root, ItemKind kind, int amount, bool logSpawn = false)
    {
        if (root == null || !ItemKindUtility.IsValid(kind))
        {
            return;
        }

        var pickup = root.GetComponent<WorldPickupItem>();
        if (pickup == null)
        {
            pickup = root.AddComponent<WorldPickupItem>();
        }

        pickup.Configure(kind, amount);
        EnsureColliders(root, pickup);
        GameplayLayers.TrySetPickupLayer(root);

        if (logSpawn)
        {
            Collider solid = GetSolidCollider(root);
            Debug.Log(
                $"[Pickup] SpawnDroppedItem: {ItemKindUtility.GetDisplayName(kind)} x{amount} | " +
                "Components: WorldPickupItem / DroppedItemMarker | " +
                $"Layer: {LayerMask.LayerToName(root.layer)} | " +
                $"Collider enabled: {(solid != null && solid.enabled)}");
        }
    }

    public static void EnsureColliders(GameObject root, WorldPickupItem pickup)
    {
        if (root == null || pickup == null)
        {
            return;
        }

        StripChildSolidColliders(root);

        SphereCollider trigger = FindRootSphereCollider(root, true);
        if (trigger == null)
        {
            trigger = root.AddComponent<SphereCollider>();
        }

        trigger.isTrigger = true;
        trigger.radius = TriggerRadius;
        trigger.center = Vector3.up * GroundColliderCenterY;
        trigger.enabled = true;

        SphereCollider solid = FindRootSphereCollider(root, false);
        if (solid == null)
        {
            solid = root.AddComponent<SphereCollider>();
        }

        solid.isTrigger = false;
        solid.radius = pickup.GetComponent<DroppedItemMarker>() != null ? GroundSolidRadius : MinSolidRadius;
        solid.center = Vector3.up * GroundColliderCenterY;
        solid.enabled = true;

        Transform interaction = root.transform.Find("InteractionPoint");
        if (interaction == null)
        {
            var pointGo = new GameObject("InteractionPoint");
            pointGo.transform.SetParent(root.transform, false);
            interaction = pointGo.transform;
        }

        interaction.localPosition = Vector3.up * (GroundColliderCenterY + solid.radius * 0.35f);
    }

    static SphereCollider FindRootSphereCollider(GameObject root, bool isTrigger)
    {
        SphereCollider[] colliders = root.GetComponents<SphereCollider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null && colliders[i].isTrigger == isTrigger)
            {
                return colliders[i];
            }
        }

        return null;
    }

    static Collider GetSolidCollider(GameObject root)
    {
        SphereCollider[] colliders = root.GetComponents<SphereCollider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null && !colliders[i].isTrigger)
            {
                return colliders[i];
            }
        }

        return root.GetComponentInChildren<Collider>();
    }

    static void StripChildSolidColliders(GameObject root)
    {
        Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            if (collider == null || collider.transform == root.transform)
            {
                continue;
            }

            if (!collider.isTrigger)
            {
                Object.Destroy(collider);
            }
        }
    }
}
