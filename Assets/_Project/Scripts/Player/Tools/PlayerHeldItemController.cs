using UnityEngine;

/// <summary>
/// Visible held item on the player hand socket (tools + placeable items).
/// </summary>
[DisallowMultipleComponent]
public class PlayerHeldItemController : MonoBehaviour
{
    [Header("Placeable Held Transform (local to hand socket)")]
    [SerializeField] Vector3 placeableThirdPersonLocalPosition = new Vector3(0.04f, -0.03f, 0.06f);
    [SerializeField] Vector3 placeableThirdPersonLocalEuler = new Vector3(-20f, -75f, 12f);
    [SerializeField] Vector3 placeableThirdPersonLocalScale = Vector3.one;
    [SerializeField] Vector3 placeableFirstPersonLocalPosition = new Vector3(0.18f, -0.08f, 0.32f);
    [SerializeField] Vector3 placeableFirstPersonLocalEuler = new Vector3(-25f, -70f, 8f);
    [SerializeField] Vector3 placeableFirstPersonLocalScale = Vector3.one;

    PlayerInventory inventory;
    PlayerToolSockets sockets;

    GameObject activeHeldInstance;
    ItemKind equippedKind = ItemKind.None;
    bool lastFirstPersonMode;

    public ItemKind EquippedKind => equippedKind;
    public bool HasHeldVisual => activeHeldInstance != null && equippedKind != ItemKind.None;

    void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
        sockets = GetComponent<PlayerToolSockets>();
        if (sockets == null)
        {
            sockets = gameObject.AddComponent<PlayerToolSockets>();
        }
    }

    void OnEnable()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged += RefreshHeldItem;
            inventory.OnHotbarSelectionChanged += HandleHotbarSelectionChanged;
        }

        RefreshHeldItem();
    }

    void OnDisable()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= RefreshHeldItem;
            inventory.OnHotbarSelectionChanged -= HandleHotbarSelectionChanged;
        }

        ClearHeldVisual();
    }

    void LateUpdate()
    {
        if (sockets == null || activeHeldInstance == null)
        {
            return;
        }

        bool firstPerson = sockets.IsFirstPersonActive();
        if (firstPerson != lastFirstPersonMode)
        {
            lastFirstPersonMode = firstPerson;
            ReparentHeldItem(firstPerson);
        }

        if (ItemKindUtility.IsWeapon(equippedKind))
        {
            ApplyWeaponHeldTransform(activeHeldInstance.transform, firstPerson);
        }
        else if (!ItemKindUtility.IsHandheldTool(equippedKind))
        {
            ApplyPlaceableHeldTransform(activeHeldInstance.transform, firstPerson);
        }
    }

    void HandleHotbarSelectionChanged(int index)
    {
        RefreshHeldItem();
    }

    public void RefreshHeldItem()
    {
        if (inventory == null)
        {
            ClearHeldVisual();
            return;
        }

        InventorySlotData selected = inventory.GetHotbarSlot(inventory.SelectedHotbarIndex);
        if (selected.IsEmpty || !ShouldShowHeldVisual(selected.Kind))
        {
            equippedKind = ItemKind.None;
            ClearHeldVisual();
            return;
        }

        if (equippedKind == selected.Kind && activeHeldInstance != null)
        {
            EnsureHeldVisible();
            return;
        }

        equippedKind = selected.Kind;
        SpawnHeldVisual(equippedKind);
    }

    public HandheldToolVisual GetActiveToolVisual()
    {
        return activeHeldInstance != null
            ? activeHeldInstance.GetComponentInChildren<HandheldToolVisual>(true)
            : null;
    }

    public HandheldWeaponVisual GetActiveWeaponVisual()
    {
        return activeHeldInstance != null
            ? activeHeldInstance.GetComponentInChildren<HandheldWeaponVisual>(true)
            : null;
    }

    public Transform ActiveHeldTransform => activeHeldInstance != null ? activeHeldInstance.transform : null;

    public static bool ShouldShowHeldVisual(ItemKind kind)
    {
        return ItemKindUtility.IsHandheldTool(kind)
            || ItemKindUtility.IsWeapon(kind)
            || ItemKindUtility.IsPlaceable(kind);
    }

    void SpawnHeldVisual(ItemKind itemKind)
    {
        ClearHeldVisual();
        sockets.EnsureSockets();

        bool firstPerson = sockets.IsFirstPersonActive();
        Transform socket = sockets.GetActiveSocket(firstPerson);
        if (socket == null)
        {
            return;
        }

        activeHeldInstance = ItemModuleFactory.SpawnHeldItemVisual(itemKind, socket, firstPerson);
        if (activeHeldInstance == null)
        {
            equippedKind = ItemKind.None;
            return;
        }

        if (ItemKindUtility.IsHandheldTool(itemKind))
        {
            if (ItemCatalog.TryGet(itemKind, out ItemData toolData))
            {
                ApplyToolTransform(activeHeldInstance.transform, toolData.HandheldTool, firstPerson);
            }

            ConfigureToolLabel(activeHeldInstance.transform, firstPerson);
        }
        else if (ItemKindUtility.IsWeapon(itemKind))
        {
            ApplyWeaponHeldTransform(activeHeldInstance.transform, firstPerson);
        }
        else
        {
            ApplyPlaceableHeldTransform(activeHeldInstance.transform, firstPerson);
        }

        EnsureRenderersEnabled(activeHeldInstance);
        lastFirstPersonMode = firstPerson;

        var toolVisual = activeHeldInstance.GetComponent<HandheldToolVisual>();
        toolVisual?.PlayEquipAnimation();
    }

    void ReparentHeldItem(bool firstPerson)
    {
        if (activeHeldInstance == null || sockets == null || equippedKind == ItemKind.None)
        {
            return;
        }

        sockets.EnsureSockets();
        Transform socket = sockets.GetActiveSocket(firstPerson);
        if (socket == null)
        {
            return;
        }

        activeHeldInstance.transform.SetParent(socket, false);

        if (ItemKindUtility.IsHandheldTool(equippedKind) && ItemCatalog.TryGet(equippedKind, out ItemData data))
        {
            ApplyToolTransform(activeHeldInstance.transform, data.HandheldTool, firstPerson);
            ConfigureToolLabel(activeHeldInstance.transform, firstPerson);
        }
        else if (ItemKindUtility.IsWeapon(equippedKind) && ItemCatalog.TryGet(equippedKind, out ItemData weaponData))
        {
            ApplyWeaponHeldTransform(activeHeldInstance.transform, firstPerson);
        }
        else
        {
            ApplyPlaceableHeldTransform(activeHeldInstance.transform, firstPerson);
        }

        EnsureRenderersEnabled(activeHeldInstance);
    }

    void ApplyWeaponHeldTransform(Transform heldTransform, bool firstPerson)
    {
        if (heldTransform == null || !ItemCatalog.TryGet(equippedKind, out ItemData data))
        {
            return;
        }

        WeaponProfile profile = data.Weapon;
        if (firstPerson)
        {
            heldTransform.localPosition = profile.FirstPersonLocalPosition;
            heldTransform.localEulerAngles = profile.FirstPersonLocalEuler;
            heldTransform.localScale = profile.FirstPersonLocalScale.sqrMagnitude > 0.001f
                ? profile.FirstPersonLocalScale
                : Vector3.one;
        }
        else
        {
            heldTransform.localPosition = profile.ThirdPersonLocalPosition;
            heldTransform.localEulerAngles = profile.ThirdPersonLocalEuler;
            heldTransform.localScale = profile.ThirdPersonLocalScale.sqrMagnitude > 0.001f
                ? profile.ThirdPersonLocalScale
                : Vector3.one;
        }
    }

    void ApplyPlaceableHeldTransform(Transform heldTransform, bool firstPerson)
    {
        if (heldTransform == null)
        {
            return;
        }

        if (firstPerson)
        {
            heldTransform.localPosition = placeableFirstPersonLocalPosition;
            heldTransform.localEulerAngles = placeableFirstPersonLocalEuler;
            heldTransform.localScale = placeableFirstPersonLocalScale;
        }
        else
        {
            heldTransform.localPosition = placeableThirdPersonLocalPosition;
            heldTransform.localEulerAngles = placeableThirdPersonLocalEuler;
            heldTransform.localScale = placeableThirdPersonLocalScale;
        }
    }

    static void ApplyToolTransform(Transform toolTransform, HandheldToolProfile profile, bool firstPerson)
    {
        if (firstPerson)
        {
            toolTransform.localPosition = profile.FirstPersonLocalPosition;
            toolTransform.localRotation = Quaternion.Euler(profile.FirstPersonLocalEuler);
            toolTransform.localScale = profile.FirstPersonLocalScale.sqrMagnitude > 0.001f
                ? profile.FirstPersonLocalScale
                : Vector3.one;
            return;
        }

        toolTransform.localPosition = profile.ThirdPersonLocalPosition;
        toolTransform.localRotation = Quaternion.Euler(profile.ThirdPersonLocalEuler);
        toolTransform.localScale = profile.ThirdPersonLocalScale.sqrMagnitude > 0.001f
            ? profile.ThirdPersonLocalScale
            : Vector3.one;
    }

    static void ConfigureToolLabel(Transform toolRoot, bool firstPerson)
    {
        if (toolRoot == null)
        {
            return;
        }

        Transform visualRoot = toolRoot.Find("VisualRoot");
        Transform label = visualRoot != null ? visualRoot.Find("Label") : null;
        if (label != null)
        {
            label.gameObject.SetActive(!firstPerson);
        }
    }

    void EnsureHeldVisible()
    {
        if (activeHeldInstance == null)
        {
            return;
        }

        activeHeldInstance.SetActive(true);
        EnsureRenderersEnabled(activeHeldInstance);
    }

    static void EnsureRenderersEnabled(GameObject root)
    {
        if (root == null)
        {
            return;
        }

        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
            {
                renderers[i].enabled = true;
            }
        }
    }

    void ClearHeldVisual()
    {
        if (activeHeldInstance != null)
        {
            Destroy(activeHeldInstance);
        }

        activeHeldInstance = null;
    }
}
