using UnityEngine;

public static class GameplayLayers
{
    public const string PickupItemLayerName = "PickupItem";
    public const string PlayerLayerName = "Player";

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

    public static bool HasPickupItemLayer => LayerMask.NameToLayer(PickupItemLayerName) >= 0;
    public static bool HasPlayerLayer => LayerMask.NameToLayer(PlayerLayerName) >= 0;

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
    /// PickupItem layer when available, plus Default as fallback for legacy objects.
    /// </summary>
    public static LayerMask PickupDetectionMask
    {
        get
        {
            LayerMask mask = 1 << 0;
            if (HasPickupItemLayer)
            {
                mask |= 1 << PickupItem;
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
