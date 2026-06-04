using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPlacementController : MonoBehaviour
{
    [SerializeField] float maxPlacementDistance = 8f;

    PlayerInventory inventory;
    PlayerGameplayTargeting targeting;
    Camera viewCamera;
    GameObject previewRoot;
    Renderer previewRenderer;
    Material previewValidMaterial;
    Material previewInvalidMaterial;
    bool placementActive;
    ItemKind activeItem = ItemKind.None;
    Vector3 activePreviewScale = Vector3.one;
    string lastPlacementMessage;

    public bool IsPlacementActive => placementActive;
    public ItemKind ActiveItem => activeItem;
    public string LastPlacementMessage => lastPlacementMessage;

    void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
        targeting = GetComponent<PlayerGameplayTargeting>();
        viewCamera = Camera.main;
    }

    void Update()
    {
        lastPlacementMessage = string.Empty;

        if (inventory == null || viewCamera == null)
        {
            HidePreview();
            return;
        }

        if (ShouldBlockPlacement())
        {
            HidePreview();
            return;
        }

        InventorySlotData selected = inventory.GetHotbarSlot(inventory.SelectedHotbarIndex);
        if (selected.IsEmpty || !ItemKindUtility.IsPlaceable(selected.Kind))
        {
            HidePreview();
            return;
        }

        activeItem = selected.Kind;
        placementActive = true;
        EnsurePreview(activeItem);

        Ray ray = targeting != null ? targeting.GetCenterScreenRay() : viewCamera.ScreenPointToRay(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));

        bool hasPoint = PlacedObjectBuilder.TryGetPlacementPosition(
            ray,
            activeItem,
            out Vector3 position,
            out Vector3 surfaceNormal,
            out Collider supportCollider);

        float placementRange = targeting != null ? targeting.MaxPlacementDistance : maxPlacementDistance;
        bool valid = hasPoint
            && Vector3.Distance(transform.position, position) <= placementRange
            && PlacedObjectBuilder.IsPlacementValid(activeItem, position, supportCollider);

        if (hasPoint)
        {
            previewRoot.transform.position = position;
            previewRoot.SetActive(true);
            previewRenderer.sharedMaterial = valid ? previewValidMaterial : previewInvalidMaterial;
        }
        else
        {
            previewRoot.SetActive(false);
            lastPlacementMessage = "无法在此放置。";
        }

        if (!valid && hasPoint)
        {
            lastPlacementMessage = "放置位置无效。";
        }

        if (valid && WasPlaceRequested())
        {
            TryPlaceActiveItem(position);
        }
    }

    bool ShouldBlockPlacement()
    {
        if (GameplayCursorPolicy.IsAnyMenuOpen)
        {
            return true;
        }

        var craftingHud = FindFirstObjectByType<CraftingHud>();
        if (craftingHud != null && craftingHud.IsOpen)
        {
            return true;
        }

        var inventoryHud = FindFirstObjectByType<PlayerInventoryHud>();
        if (inventoryHud != null && inventoryHud.IsBackpackOpen)
        {
            return true;
        }

        return false;
    }

    bool WasPlaceRequested()
    {
        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
    }

    void TryPlaceActiveItem(Vector3 position)
    {
        Ray ray = targeting != null
            ? targeting.GetCenterScreenRay()
            : viewCamera.ScreenPointToRay(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));

        PlacedObjectBuilder.TryGetPlacementPosition(
            ray,
            activeItem,
            out Vector3 resolvedPosition,
            out Vector3 surfaceNormal,
            out Collider supportCollider);

        position = resolvedPosition;

        if (!PlacedObjectBuilder.IsPlacementValid(activeItem, position, supportCollider))
        {
            lastPlacementMessage = "放置位置无效。";
            return;
        }

        InventorySlotData selected = inventory.GetHotbarSlot(inventory.SelectedHotbarIndex);
        if (selected.IsEmpty || selected.Kind != activeItem)
        {
            lastPlacementMessage = "请先在热键栏选中可放置物品。";
            return;
        }

        if (!inventory.TryConsumeFromSelectedHotbar(1))
        {
            lastPlacementMessage = "热键栏中没有足够的物品。";
            return;
        }

        GameObject placed = ItemModuleFactory.SpawnPlacedObject(activeItem, position, Quaternion.identity);
        if (placed == null)
        {
            UnifiedInventoryAcquisition.TryAcquireFully(inventory, activeItem, 1);
            lastPlacementMessage = "放置失败。";
            return;
        }

        lastPlacementMessage = $"已放置 {ItemKindUtility.GetDisplayName(activeItem)}。";
    }

    void EnsurePreview(ItemKind itemKind)
    {
        Vector3 desiredScale = PlacedObjectBuilder.GetPlacedVisualScale(itemKind);
        if (previewRoot != null && activeItem == itemKind && (activePreviewScale - desiredScale).sqrMagnitude < 0.0001f)
        {
            return;
        }

        if (previewRoot != null)
        {
            Destroy(previewRoot);
            previewRoot = null;
            previewRenderer = null;
        }

        activeItem = itemKind;
        activePreviewScale = desiredScale;

        previewRoot = new GameObject("PlacementPreview");
        ItemVisualBuilder.CreatePlacedVisual(
            previewRoot.transform,
            itemKind,
            desiredScale,
            Vector3.up * desiredScale.y * 0.5f);

        previewRenderer = previewRoot.GetComponentInChildren<Renderer>();
        Object.DestroyImmediate(previewRoot.GetComponentInChildren<Collider>());

        previewValidMaterial = CreatePreviewMaterial(ItemKindUtility.GetDisplayColor(itemKind), 0.45f);
        previewInvalidMaterial = CreatePreviewMaterial(new Color(0.9f, 0.2f, 0.2f), 0.35f);
        if (previewRenderer != null)
        {
            previewRenderer.sharedMaterial = previewValidMaterial;
        }

        previewRoot.SetActive(false);
    }

    static Material CreatePreviewMaterial(Color color, float alpha)
    {
        var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        color.a = alpha;
        material.color = color;
        material.SetFloat("_Surface", 1f);
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.renderQueue = 3000;
        return material;
    }

    void HidePreview()
    {
        placementActive = false;
        activeItem = ItemKind.None;
        if (previewRoot != null)
        {
            previewRoot.SetActive(false);
        }
    }
}
