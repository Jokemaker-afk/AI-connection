using System;
using UnityEngine;

/// <summary>
/// Strict crosshair-only targeting for gather/chop/mine/repair tools.
/// First- and third-person share the same center-screen ray from <see cref="AimReferenceProvider"/>.
/// Never selects nearest resources — only colliders directly hit by the crosshair ray.
/// Pickup targeting remains separate in <see cref="PlayerPickupTargeting"/>.
/// </summary>
public static class PlayerToolTargeting
{
    public const float DefaultMaxToolDistance = 4.5f;
    public const float DefaultMaxRayDistance = 12f;

    public struct ToolTargetHit
    {
        public ToolInteractable Target;
        public RaycastHit Hit;
        public bool HasTarget => Target != null;
    }

    public static bool TryGetCrosshairToolTarget(
        Ray crosshairRay,
        float maxRayDistance,
        LayerMask layerMask,
        Transform playerRoot,
        out ToolTargetHit result)
    {
        result = default;

        if (maxRayDistance <= 0f)
        {
            return false;
        }

        LayerMask castMask = ResolveToolLayerMask(layerMask);
        RaycastHit[] hits = Physics.RaycastAll(
            crosshairRay,
            maxRayDistance,
            castMask,
            QueryTriggerInteraction.Collide);

        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        for (int i = 0; i < hits.Length; i++)
        {
            Collider collider = hits[i].collider;
            if (collider == null || ShouldIgnoreCollider(collider, playerRoot))
            {
                continue;
            }

            if (IsBlockingWorldPickup(collider))
            {
                return false;
            }

            ToolInteractable tool = collider.GetComponentInParent<ToolInteractable>();
            if (tool == null || tool.IsCompleted)
            {
                continue;
            }

            result = new ToolTargetHit
            {
                Target = tool,
                Hit = hits[i],
            };
            return true;
        }

        return false;
    }

    public static bool IsWithinToolRangeFromHit(Transform playerRoot, Vector3 hitPoint, float maxDistance)
    {
        if (playerRoot == null || maxDistance <= 0f)
        {
            return false;
        }

        Vector3 flat = hitPoint - playerRoot.position;
        flat.y = 0f;
        return flat.magnitude <= maxDistance;
    }

    public static bool IsWithinPlayerToolRange(Transform playerRoot, ToolInteractable target, float maxDistance)
    {
        if (playerRoot == null || target == null || maxDistance <= 0f)
        {
            return false;
        }

        Vector3 flat = target.transform.position - playerRoot.position;
        flat.y = 0f;
        return flat.magnitude <= maxDistance;
    }

    public static float ResolveMaxToolDistance(Component playerContext)
    {
        if (playerContext == null)
        {
            return DefaultMaxToolDistance;
        }

        var toolController = playerContext.GetComponent<PlayerToolController>();
        if (toolController != null)
        {
            return toolController.MaxToolUseDistance;
        }

        var gameplayTargeting = playerContext.GetComponent<PlayerGameplayTargeting>();
        if (gameplayTargeting != null)
        {
            return gameplayTargeting.MaxInteractionDistance;
        }

        return DefaultMaxToolDistance;
    }

    public static float ResolveMaxRayDistance(Component playerContext)
    {
        if (playerContext == null)
        {
            return DefaultMaxRayDistance;
        }

        var gameplayTargeting = playerContext.GetComponent<PlayerGameplayTargeting>();
        if (gameplayTargeting != null)
        {
            return gameplayTargeting.ToolCrosshairRayDistance;
        }

        return DefaultMaxRayDistance;
    }

    public static LayerMask ResolveToolLayerMask(LayerMask baseMask)
    {
        LayerMask mask = baseMask;
        mask &= ~GameplayLayers.TargetingIgnoreMask;
        return mask;
    }

    static bool ShouldIgnoreCollider(Collider collider, Transform playerRoot)
    {
        if (collider == null)
        {
            return true;
        }

        if (IsPlayerCollider(collider))
        {
            return true;
        }

        if (playerRoot != null && collider.transform.IsChildOf(playerRoot))
        {
            return true;
        }

        return false;
    }

    static bool IsBlockingWorldPickup(Collider collider)
    {
        WorldPickupItem pickup = collider.GetComponentInParent<WorldPickupItem>();
        if (pickup == null || pickup.IsCollected)
        {
            return false;
        }

        return collider.GetComponentInParent<ToolInteractable>() == null;
    }

    static bool IsPlayerCollider(Collider collider)
    {
        if (collider.GetComponentInParent<PlayerController>() != null
            || collider.GetComponentInParent<CharacterController>() != null)
        {
            return true;
        }

        if (GameplayLayers.HasPlayerLayer
            && GameplayLayers.IsInLayerMask(collider.gameObject.layer, GameplayLayers.PlayerMask))
        {
            return true;
        }

        return false;
    }
}
