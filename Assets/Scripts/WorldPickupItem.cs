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



    void Awake()

    {

        basePosition = transform.position;

        EnsureInteractionPoint();

        EnsureSolidRaycastCollider();

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

            return;

        }



        var pointGo = new GameObject("InteractionPoint");

        pointGo.transform.SetParent(transform, false);

        pointGo.transform.localPosition = Vector3.up * 0.35f;

        interactionPoint = pointGo.transform;

    }



    public Vector3 GetInteractionPoint()

    {

        if (interactionPoint != null)

        {

            return interactionPoint.position;

        }



        Collider col = GetComponentInChildren<Collider>();

        if (col != null)

        {

            return col.bounds.center;

        }



        return transform.position + Vector3.up * 0.5f;

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



    void EnsureSolidRaycastCollider()

    {

        SphereCollider[] colliders = GetComponents<SphereCollider>();

        for (int i = 0; i < colliders.Length; i++)

        {

            if (colliders[i] != null && !colliders[i].isTrigger)

            {

                return;

            }

        }



        var rayCollider = gameObject.AddComponent<SphereCollider>();

        rayCollider.radius = Mathf.Max(pickupRadius, 0.35f);

        rayCollider.isTrigger = false;

        rayCollider.center = Vector3.up * 0.15f;

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

        if (collected)

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



        if (!inventory.TryAddItem(itemKind, amount))

        {

            return false;

        }



        collected = true;

        CollectibleManager.Instance?.RegisterCollection();

        Destroy(gameObject);

        return true;

    }



    public static WorldPickupItem Create(Transform parent, string name, Vector3 position, ItemKind kind, int pickupAmount = 1)

    {

        var root = new GameObject(name);

        root.transform.SetParent(parent, false);

        root.transform.position = position;



        ItemVisualBuilder.CreatePickupVisual(root.transform, kind, Vector3.one * 0.55f);



        var pickup = root.AddComponent<WorldPickupItem>();

        pickup.itemKind = kind;

        pickup.amount = pickupAmount;

        pickup.basePosition = position;

        pickup.EnsureInteractionPoint();



        ItemWorldLabel.Create(root.transform, pickup.DisplayNameChinese, Vector3.up * 0.55f);



        var collider = root.AddComponent<SphereCollider>();

        collider.radius = pickup.pickupRadius;

        collider.isTrigger = true;



        pickup.EnsureSolidRaycastCollider();



        GameplayLayers.TrySetPickupLayer(root);

        return pickup;

    }

}

