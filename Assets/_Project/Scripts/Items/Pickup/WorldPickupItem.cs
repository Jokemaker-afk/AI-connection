using UnityEngine;

public class WorldPickupItem : MonoBehaviour
{
    [SerializeField] ItemKind itemKind = ItemKind.RedBlock;
    [SerializeField] int amount = 1;
    [SerializeField] Transform interactionPoint;
    [SerializeField] float pickupRadius = 0.45f;
    [SerializeField] bool canBePickedUp = true;
    [SerializeField] float bobAmplitude = 0.12f;
    [SerializeField] float bobSpeed = 2f;
    [SerializeField] float spinSpeed = 35f;

    Vector3 basePosition;
    bool collected;

    public ItemKind ItemKind => itemKind;
    public int Amount => amount;
    public float PickupRadius => pickupRadius;
    public bool CanBePickedUp => canBePickedUp && !collected;
    public bool IsCollected => collected;
    public string DisplayNameChinese => ItemKindUtility.GetDisplayName(itemKind);
    public bool IsGroundRestingPickup => GetComponent<DroppedItemMarker>() != null;

    void Awake()
    {
        basePosition = transform.position;
        PickupItemInteractionSetup.EnsureColliders(gameObject, this);
        GameplayLayers.TrySetPickupLayer(gameObject);
        RefreshLabel();
    }

    void EnsureInteractionPoint()
    {
        if (interactionPoint != null)
        {
            return;
        }

        Transform existing = transform.Find("InteractionPoint");
        if (existing != null)
        {
            interactionPoint = existing;
        }
    }

    public Vector3 GetInteractionPoint()
    {
        EnsureInteractionPoint();
        if (interactionPoint != null)
        {
            return interactionPoint.position;
        }

        Collider col = GetSolidCollider();
        if (col != null)
        {
            Bounds bounds = col.bounds;
            return new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
        }

        return transform.position + Vector3.up * 0.5f;
    }

    Collider GetSolidCollider()
    {
        SphereCollider[] colliders = GetComponents<SphereCollider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null && !colliders[i].isTrigger)
            {
                return colliders[i];
            }
        }

        return GetComponentInChildren<Collider>();
    }

    public float GetEstimatedGroundY()
    {
        Vector3 probe = new Vector3(basePosition.x, basePosition.y, basePosition.z);
        if (PickupGroundRingMarker.TryFindGroundPoint(probe, transform, out Vector3 groundPoint))
        {
            return groundPoint.y;
        }

        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds.min.y - 0.08f;
        }

        return basePosition.y - 0.45f;
    }

    void RefreshLabel()
    {
        if (!ItemKindUtility.IsValid(itemKind))
        {
            return;
        }

        string displayName = DisplayNameChinese;
        Transform label = transform.Find("Label");
        if (label == null)
        {
            ItemWorldLabel.Create(transform, displayName, Vector3.up * 0.55f);
            return;
        }

        ItemWorldLabel.Refresh(label.gameObject, displayName);
    }

    void Update()
    {
        if (collected || IsGroundRestingPickup)
        {
            return;
        }

        float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        transform.position = basePosition + Vector3.up * bob;
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
        UpdatePickableGroundMarker();
    }

    GameObject pickableWorldMarker;

    public void SetCrosshairTargetState(bool isCrosshairTarget, bool showPickableMarker)
    {
        if (showPickableMarker && isCrosshairTarget)
        {
            EnsurePickableWorldMarker();
            PickupGroundRingMarker.PlaceOnGroundUnder(pickableWorldMarker.transform, basePosition, transform);
            pickableWorldMarker.SetActive(true);
            return;
        }

        if (pickableWorldMarker != null)
        {
            pickableWorldMarker.SetActive(false);
        }
    }

    void OnDestroy()
    {
        if (pickableWorldMarker != null)
        {
            Destroy(pickableWorldMarker);
        }
    }

    void EnsurePickableWorldMarker()
    {
        if (pickableWorldMarker != null)
        {
            return;
        }

        pickableWorldMarker = PickupGroundRingMarker.Create(
            null,
            "PickableGroundRing",
            new Color(0.25f, 0.95f, 1f, 0.62f),
            0.82f);
        pickableWorldMarker.SetActive(false);
    }

    void UpdatePickableGroundMarker()
    {
        if (pickableWorldMarker == null || !pickableWorldMarker.activeSelf)
        {
            return;
        }

        PickupGroundRingMarker.PlaceOnGroundUnder(pickableWorldMarker.transform, basePosition, transform);
    }

    public bool TryCollect(PlayerInventory inventory)
    {
        if (collected || inventory == null || !canBePickedUp)
        {
            return false;
        }

        bool isDropped = IsGroundRestingPickup;
        if (isDropped)
        {
            Debug.Log($"[Pickup] Picking up dropped item: {itemKind} x{amount}");
        }

        if (!UnifiedInventoryAcquisition.CanAcquire(inventory, itemKind, amount))
        {
            if (isDropped)
            {
                Debug.Log("[Pickup] Inventory add success: false (full)");
            }

            return false;
        }

        if (!UnifiedInventoryAcquisition.TryAcquireFully(inventory, itemKind, amount))
        {
            if (isDropped)
            {
                Debug.Log("[Pickup] Inventory add success: false");
            }

            return false;
        }

        collected = true;
        if (isDropped)
        {
            Debug.Log("[Pickup] Inventory add success: true | Dropped item removed: true");
        }

        if (GetComponent<DroppedItemMarker>() != null)
        {
            Debug.Log($"[ItemDrop] Pickup dropped item: {ItemKindUtility.GetDisplayName(itemKind)} x{amount}");
        }

        CollectibleManager.Instance?.RegisterCollection();
        Destroy(gameObject);
        return true;
    }

    public void Configure(ItemKind kind, int pickupAmount)
    {
        itemKind = kind;
        amount = Mathf.Max(1, pickupAmount);
        basePosition = transform.position;
        PickupItemInteractionSetup.EnsureColliders(gameObject, this);
        RefreshLabel();
    }

    /// <summary>Legacy wrapper — prefer ItemModuleFactory.SpawnWorldPickup.</summary>
    public static WorldPickupItem Create(Transform parent, string name, Vector3 position, ItemKind kind, int pickupAmount = 1)
    {
        ItemModuleValidation.WarnLegacyCreationPath(nameof(WorldPickupItem.Create), kind);
        GameObject root = ItemModuleFactory.SpawnWorldPickup(kind, position, pickupAmount, parent);
        if (root == null)
        {
            return null;
        }

        root.name = name;
        return root.GetComponent<WorldPickupItem>();
    }
}
