using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerPlacementController : MonoBehaviour
{
    [SerializeField] float maxPlacementDistance = 8f;
    [SerializeField] bool enablePlacementDebugLogs = true;

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
    bool loggedMissingAbility;
    bool loggedPreviewActive;

    public bool IsPlacementActive => placementActive;
    public ItemKind ActiveItem => activeItem;
    public string LastPlacementMessage => lastPlacementMessage;

    void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
        targeting = GetComponent<PlayerGameplayTargeting>();
        ResolveViewCamera();
        GameplayPlacementBootstrap.EnsureInitialized();
    }

    void Update()
    {
        lastPlacementMessage = string.Empty;
        ResolveViewCamera();

        if (inventory == null || viewCamera == null)
        {
            if (enablePlacementDebugLogs && viewCamera == null)
            {
                LogPlacementOnce("Placement blocked: gameplay camera not resolved.");
            }

            HidePreview();
            return;
        }

        if (!GameplayPlacementBootstrap.CanUsePlacement())
        {
            if (!loggedMissingAbility)
            {
                loggedMissingAbility = true;
                LogPlacement("Placement blocked: BuildingPlacement ability not unlocked.");
            }

            HidePreview();
            return;
        }

        loggedMissingAbility = false;

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

        if (!loggedPreviewActive)
        {
            loggedPreviewActive = true;
            LogPlacement($"Selected toolbar item: {ItemKindUtility.GetDisplayName(activeItem)} | Placement preview active.");
        }

        Ray ray = GetPlacementRay();
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

    Ray GetPlacementRay()
    {
        if (targeting != null)
        {
            return targeting.GetCenterScreenRay();
        }

        Vector2 screenPoint;
        return CrosshairRayUtility.GetCrosshairRay(out screenPoint);
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
        Ray ray = GetPlacementRay();

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
            LogPlacement("Placement failed: invalid surface or overlap blocked.");
            return;
        }

        InventorySlotData selected = inventory.GetHotbarSlot(inventory.SelectedHotbarIndex);
        if (selected.IsEmpty || selected.Kind != activeItem)
        {
            lastPlacementMessage = "请先在热键栏选中可放置物品。";
            LogPlacement("Placement failed: no selected placeable item.");
            return;
        }

        if (!inventory.TryConsumeFromSelectedHotbar(1))
        {
            lastPlacementMessage = "热键栏中没有足够的物品。";
            LogPlacement("Placement failed: insufficient hotbar count.");
            return;
        }

        GameObject placed = ItemModuleFactory.SpawnPlacedObject(activeItem, position, Quaternion.identity);
        if (placed == null)
        {
            UnifiedInventoryAcquisition.TryAcquireFully(inventory, activeItem, 1);
            lastPlacementMessage = "放置失败。";
            LogPlacement("Placement failed: missing PlacedPrefab and fallback failed.");
            return;
        }

        lastPlacementMessage = $"已放置 {ItemKindUtility.GetDisplayName(activeItem)}。";
        LogPlacement($"{ItemKindUtility.GetDisplayName(activeItem)} placed successfully.");
    }

    void ResolveViewCamera()
    {
        viewCamera = CrosshairRayUtility.ResolveGameplayCamera();
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
        loggedPreviewActive = false;
        if (previewRoot != null)
        {
            previewRoot.SetActive(false);
        }
    }

    void LogPlacement(string message)
    {
        if (!enablePlacementDebugLogs)
        {
            return;
        }

        GameplayCore.Instance?.Log($"[Placement] {message}");
    }

    string lastOnceMessage;

    void LogPlacementOnce(string message)
    {
        if (!enablePlacementDebugLogs || lastOnceMessage == message)
        {
            return;
        }

        lastOnceMessage = message;
        LogPlacement(message);
    }
}
