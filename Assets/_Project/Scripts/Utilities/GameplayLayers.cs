using UnityEngine;

public static class GameplayLayers
{
    public const string PickupItemLayerName = "PickupItem";
    public const string PlayerLayerName = "Player";
    public const string EnemyLayerName = "Enemy";

    public static int PickupItem
    {
        get
        {
            int layer = LayerMask.NameToLayer(PickupItemLayerName);
            return layer >= 0 ? layer : 0;
        }
    }

    public static int Player
    {
        get
        {
            int layer = LayerMask.NameToLayer(PlayerLayerName);
            return layer >= 0 ? layer : 0;
        }
    }

    public static int Enemy
    {
        get
        {
            int layer = LayerMask.NameToLayer(EnemyLayerName);
            return layer >= 0 ? layer : 0;
        }
    }

    public static bool HasPickupItemLayer => LayerMask.NameToLayer(PickupItemLayerName) >= 0;
    public static bool HasPlayerLayer => LayerMask.NameToLayer(PlayerLayerName) >= 0;
    public static bool HasEnemyLayer => LayerMask.NameToLayer(EnemyLayerName) >= 0;

    public static LayerMask EnemyMask
    {
        get
        {
            if (!HasEnemyLayer)
            {
                return 1 << 0;
            }

            return 1 << Enemy;
        }
    }

    /// <summary>Layers excluded from world aim / targeting raycasts (player body, UI, ignore raycast).</summary>
    public static LayerMask AimRayExcludeMask => TargetingIgnoreMask;

    /// <summary>Raycasts for weapons and combat (Enemy layer, or Default fallback).</summary>
    public static LayerMask CombatTargetMask
    {
        get
        {
            if (HasEnemyLayer)
            {
                return EnemyMask;
            }

            return 1 << 0;
        }
    }

    /// <summary>Weapon enemy targeting — Enemy layer only, never includes Player.</summary>
    public static LayerMask WeaponEnemyTargetMask => CombatTargetMask;

    public static LayerMask PickupItemMask
    {
        get
        {
            if (!HasPickupItemLayer)
            {
                return 1 << 0;
            }

            return 1 << PickupItem;
        }
    }

    public static LayerMask PlayerMask
    {
        get
        {
            if (!HasPlayerLayer)
            {
                return 0;
            }

            return 1 << Player;
        }
    }

    /// <summary>
    /// PickupItem layer when available; otherwise Default for legacy scenes.
    /// Excludes terrain Default layer when PickupItem exists so crosshair rays reach ground pickups.
    /// </summary>
    public static LayerMask PickupDetectionMask
    {
        get
        {
            if (HasPickupItemLayer)
            {
                return 1 << PickupItem;
            }

            return 1 << 0;
        }
    }

    /// <summary>Obstacles for pickup line-of-sight checks (excludes pickup and player layers).</summary>
    public static LayerMask PickupLineOfSightObstacleMask
    {
        get
        {
            LayerMask mask = ~0;
            if (HasPickupItemLayer)
            {
                mask &= ~(1 << PickupItem);
            }

            if (HasPlayerLayer)
            {
                mask &= ~PlayerMask;
            }

            int ignoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
            if (ignoreRaycast >= 0)
            {
                mask &= ~(1 << ignoreRaycast);
            }

            return mask;
        }
    }

    public static LayerMask TargetingIgnoreMask
    {
        get
        {
            LayerMask mask = PlayerMask;
            int uiLayer = LayerMask.NameToLayer("UI");
            if (uiLayer >= 0)
            {
                mask |= 1 << uiLayer;
            }

            int ignoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
            if (ignoreRaycast >= 0)
            {
                mask |= 1 << ignoreRaycast;
            }

            return mask;
        }
    }

    public static void TrySetPickupLayer(GameObject target)
    {
        if (target == null || !HasPickupItemLayer)
        {
            return;
        }

        SetLayerRecursively(target, PickupItem);
    }

    public static void TrySetPlayerLayer(GameObject target)
    {
        if (target == null || !HasPlayerLayer)
        {
            return;
        }

        SetLayerRecursively(target, PlayerLayerName);
    }

    public static void TrySetEnemyLayer(GameObject target)
    {
        if (target == null || !HasEnemyLayer)
        {
            return;
        }

        SetLayerRecursively(target, Enemy);
    }

    public static void TrySetIgnoreRaycastLayer(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        int ignoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
        if (ignoreRaycast < 0)
        {
            return;
        }

        SetLayerRecursively(target, ignoreRaycast);
    }

    static void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        Transform root = obj.transform;
        for (int i = 0; i < root.childCount; i++)
        {
            SetLayerRecursively(root.GetChild(i).gameObject, layer);
        }
    }

    static void SetLayerRecursively(GameObject obj, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer < 0)
        {
            return;
        }

        SetLayerRecursively(obj, layer);
    }
}
