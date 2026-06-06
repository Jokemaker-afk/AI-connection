using UnityEngine;

/// <summary>Collider/layer setup for objective collectibles (Data Core, etc.).</summary>
public static class ObjectiveCollectibleSetup
{
    const float MinSolidRadius = 0.45f;
    const float TriggerRadius = 0.55f;
    const float ColliderCenterY = 0.35f;

    public static void EnsureColliders(GameObject root)
    {
        if (root == null)
        {
            return;
        }

        SphereCollider trigger = FindRootSphereCollider(root, true);
        if (trigger == null)
        {
            trigger = root.AddComponent<SphereCollider>();
        }

        trigger.isTrigger = true;
        trigger.radius = TriggerRadius;
        trigger.center = Vector3.up * ColliderCenterY;
        trigger.enabled = true;

        SphereCollider solid = FindRootSphereCollider(root, false);
        if (solid == null)
        {
            solid = root.AddComponent<SphereCollider>();
        }

        solid.isTrigger = false;
        solid.radius = MinSolidRadius;
        solid.center = Vector3.up * ColliderCenterY;
        solid.enabled = true;

        Transform interaction = root.transform.Find("InteractionPoint");
        if (interaction == null)
        {
            var pointGo = new GameObject("InteractionPoint");
            pointGo.transform.SetParent(root.transform, false);
            interaction = pointGo.transform;
        }

        interaction.localPosition = Vector3.up * (ColliderCenterY + solid.radius * 0.35f);
        GameplayLayers.TrySetPickupLayer(root);
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
}
