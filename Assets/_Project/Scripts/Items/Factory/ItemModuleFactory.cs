using UnityEngine;

/// <summary>
/// Unified item creation entry point.
/// Chain: ItemKind → ItemCatalog / ItemData → ItemModuleFactory → world / placed / handheld / inventory.
/// See ITEM_MODULE_WORKFLOW.md for adding new items.
/// </summary>
public static class ItemModuleFactory
{
    const float DefaultPickupVisualScale = 0.55f;
    const string PickupVisualChildName = "Visual";

    public static GameObject SpawnWorldPickup(ItemKind kind, Vector3 position, int amount = 1, Transform parent = null)
    {
        if (!TryGetValidatedData(kind, "SpawnWorldPickup", out ItemData data))
        {
            return null;
        }

        Level8BiomeKind biome = Level8ResourceSpawnContext.CurrentBiome;
        if (ItemPickupVisualResolver.TryResolve(kind, biome, out GameObject resolvedPrefab) && resolvedPrefab != null
            && WorldPickupPrefabUtility.IsCompleteWorldPickupPrefab(resolvedPrefab))
        {
            GameObject completeRoot = Object.Instantiate(resolvedPrefab, position, Quaternion.identity, parent);
            completeRoot.name = $"Pickup_{kind}";
            ConfigureWorldPickup(completeRoot, kind, amount);
            LogFallback(kind, "complete world pickup prefab");
            return completeRoot;
        }

        var root = new GameObject($"Pickup_{kind}");
        root.transform.SetParent(parent, false);
        root.transform.position = position;

        if (resolvedPrefab != null)
        {
            Level8ResourceVisualUtility.InstantiatePickupVisual(resolvedPrefab, root.transform);
            LogFallback(kind, "world pickup prefab visual");
        }
        else
        {
            CreateItemVisual(kind, ItemVisualMode.WorldPickup, root.transform);
            LogFallback(kind, "generated world pickup visual");
        }

        ConfigureWorldPickup(root, kind, amount);
        return root;
    }

    public static GameObject SpawnPlacedObject(ItemKind kind, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (!TryGetValidatedData(kind, "SpawnPlacedObject", out ItemData data))
        {
            return null;
        }

        if (!data.IsPlaceable)
        {
            Debug.LogWarning($"[ItemModule] {kind} is not placeable.");
            return null;
        }

        GameObject root;
        GameObject placedPrefab = data.PlacedPrefab;
        if (placedPrefab == null && ItemPlacedPrefabResolver.TryResolve(kind, out GameObject resolvedPlacedPrefab))
        {
            placedPrefab = resolvedPlacedPrefab;
        }

        if (placedPrefab != null)
        {
            root = Object.Instantiate(placedPrefab, position, rotation, parent);
            root.name = $"Placed_{kind}";
            EnsurePlacedComponents(root, kind, data);
            LogFallback(kind, "placed prefab");
        }
        else
        {
            root = PlacedObjectBuilder.SpawnPlacedObject(kind, position, rotation);
            if (root != null && parent != null)
            {
                root.transform.SetParent(parent, true);
            }

            LogFallback(kind, "generated placed object");
        }

        return root;
    }

    public static GameObject SpawnHandheldTool(ItemKind kind, Transform socket, bool isFirstPerson)
    {
        if (socket == null || !TryGetValidatedData(kind, "SpawnHandheldTool", out ItemData data))
        {
            return null;
        }

        if (!data.IsHandheldTool)
        {
            Debug.LogWarning($"[ItemModule] {kind} is not a handheld tool.");
            return null;
        }

        GameObject instance;
        if (data.HandheldTool.HandheldPrefab != null)
        {
            instance = Object.Instantiate(data.HandheldTool.HandheldPrefab, socket, false);
            LogFallback(kind, "handheld prefab");
        }
        else
        {
            GameObject template = HandheldToolPrefabBuilder.GetOrCreatePrefab(kind);
            if (template == null)
            {
                return null;
            }

            instance = Object.Instantiate(template, socket, false);
            LogFallback(kind, "generated handheld prototype");
        }

        instance.name = $"Equipped_{kind}";
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;
        instance.SetActive(true);

        ApplyHandheldTransform(instance.transform, data.HandheldTool, isFirstPerson);
        EnsureHandheldComponents(instance, kind, data.HandheldTool);
        GameplayLayers.TrySetIgnoreRaycastLayer(instance);
        return instance;
    }

    public static GameObject SpawnHandheldWeapon(ItemKind kind, Transform socket, bool isFirstPerson)
    {
        if (socket == null || !TryGetValidatedData(kind, "SpawnHandheldWeapon", out ItemData data) || !data.IsWeapon)
        {
            return null;
        }

        GameObject template = HandheldWeaponPrefabBuilder.GetOrCreatePrefab(kind);
        if (template == null)
        {
            return null;
        }

        GameObject instance = Object.Instantiate(template, socket);
        instance.name = $"Equipped_{kind}";
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;
        instance.SetActive(true);
        ApplyWeaponTransform(instance.transform, data.Weapon, isFirstPerson);

        HandheldWeaponVisual weaponVisual = instance.GetComponent<HandheldWeaponVisual>();
        if (weaponVisual == null)
        {
            weaponVisual = instance.AddComponent<HandheldWeaponVisual>();
        }

        weaponVisual.Configure(kind, data.Weapon);
        GameplayLayers.TrySetIgnoreRaycastLayer(instance);
        return instance;
    }

    static void ApplyWeaponTransform(Transform weaponTransform, WeaponProfile profile, bool isFirstPerson)
    {
        if (weaponTransform == null)
        {
            return;
        }

        if (isFirstPerson)
        {
            weaponTransform.localPosition = profile.FirstPersonLocalPosition;
            weaponTransform.localRotation = Quaternion.Euler(profile.FirstPersonLocalEuler);
            weaponTransform.localScale = profile.FirstPersonLocalScale.sqrMagnitude > 0.001f
                ? profile.FirstPersonLocalScale
                : Vector3.one;
            return;
        }

        weaponTransform.localPosition = profile.ThirdPersonLocalPosition;
        weaponTransform.localRotation = Quaternion.Euler(profile.ThirdPersonLocalEuler);
        weaponTransform.localScale = profile.ThirdPersonLocalScale.sqrMagnitude > 0.001f
            ? profile.ThirdPersonLocalScale
            : Vector3.one;
    }

    public static InventorySlotData CreateInventoryEntry(ItemKind kind, int amount)
    {
        if (!TryGetValidatedData(kind, "CreateInventoryEntry", out ItemData data))
        {
            return InventorySlotData.Empty;
        }

        int clampedAmount = Mathf.Clamp(amount, 1, data.Stackable ? data.MaxStack : 1);
        if (!data.Stackable)
        {
            clampedAmount = 1;
        }

        return InventorySlotData.Of(kind, clampedAmount);
    }

    public static GameObject CreateItemVisual(ItemKind kind, ItemVisualMode mode, Transform parent = null)
    {
        if (!TryGetValidatedData(kind, "CreateItemVisual", out ItemData data))
        {
            return null;
        }

        switch (mode)
        {
            case ItemVisualMode.WorldPickup:
                return ItemVisualBuilder.CreatePickupVisual(parent, kind, Vector3.one * DefaultPickupVisualScale);

            case ItemVisualMode.Placed:
            {
                Vector3 scale = data.PlacedBoundsSize.sqrMagnitude > 0.01f
                    ? data.PlacedBoundsSize
                    : PlacedObjectBuilder.GetPlacedVisualScale(kind);
                Vector3 offset = Vector3.up * scale.y * 0.5f;
                return ItemVisualBuilder.CreatePlacedVisual(parent, kind, scale, offset);
            }

            case ItemVisualMode.Handheld:
            {
                if (data.HandheldTool.HandheldPrefab != null && parent != null)
                {
                    var instance = Object.Instantiate(data.HandheldTool.HandheldPrefab, parent);
                    instance.name = PickupVisualChildName;
                    return instance;
                }

                GameObject template = HandheldToolPrefabBuilder.GetOrCreatePrefab(kind);
                if (template == null || parent == null)
                {
                    return null;
                }

                var toolInstance = Object.Instantiate(template, parent);
                toolInstance.name = PickupVisualChildName;
                return toolInstance;
            }

            case ItemVisualMode.HeldPlaceable:
                return ItemVisualBuilder.CreateHeldPlaceableVisual(parent, kind, Vector3.one);

            default:
                return null;
        }
    }

    public static GameObject SpawnDroppedItem(ItemKind kind, Vector3 position, int amount = 1, Transform parent = null)
    {
        if (!TryGetValidatedData(kind, "SpawnDroppedItem", out ItemData data))
        {
            return null;
        }

        int clampedAmount = Mathf.Clamp(amount, 1, data.Stackable ? data.MaxStack : 1);
        GameObject root = SpawnWorldPickup(kind, position, clampedAmount, parent);
        if (root == null)
        {
            return null;
        }

        root.name = $"Dropped_{kind}";
        StripPlacedGameplayComponents(root);

        var marker = root.GetComponent<DroppedItemMarker>() ?? root.AddComponent<DroppedItemMarker>();
        marker.Configure(kind, clampedAmount);

        PickupItemInteractionSetup.Configure(root, kind, clampedAmount, logSpawn: true);
        return root;
    }

    public static GameObject SpawnHeldItemVisual(ItemKind kind, Transform socket, bool isFirstPerson)
    {
        if (socket == null || !TryGetValidatedData(kind, "SpawnHeldItemVisual", out ItemData data))
        {
            return null;
        }

        if (data.IsHandheldTool)
        {
            return SpawnHandheldTool(kind, socket, isFirstPerson);
        }

        if (data.IsWeapon)
        {
            return SpawnHandheldWeapon(kind, socket, isFirstPerson);
        }

        if (!data.IsPlaceable)
        {
            return null;
        }

        var root = new GameObject($"Held_{kind}");
        root.transform.SetParent(socket, false);
        root.transform.localPosition = Vector3.zero;
        root.transform.localRotation = Quaternion.identity;
        root.transform.localScale = Vector3.one;

        Vector3 heldScale = isFirstPerson
            ? new Vector3(0.28f, 0.28f, 0.28f)
            : new Vector3(0.38f, 0.38f, 0.38f);
        CreateItemVisual(kind, ItemVisualMode.HeldPlaceable, root.transform);
        Transform visual = root.transform.Find("Visual");
        if (visual != null)
        {
            visual.localScale = heldScale;
        }

        return root;
    }

    public static void StripPlacedGameplayComponents(GameObject root)
    {
        if (root == null)
        {
            return;
        }

        CraftingStation[] stations = root.GetComponentsInChildren<CraftingStation>(true);
        for (int i = 0; i < stations.Length; i++)
        {
            if (stations[i] != null)
            {
                Object.Destroy(stations[i]);
            }
        }

        PlacedBuilding[] buildings = root.GetComponentsInChildren<PlacedBuilding>(true);
        for (int i = 0; i < buildings.Length; i++)
        {
            if (buildings[i] != null)
            {
                Object.Destroy(buildings[i]);
            }
        }

        PlacedPickupable[] placedPickupables = root.GetComponentsInChildren<PlacedPickupable>(true);
        for (int i = 0; i < placedPickupables.Length; i++)
        {
            if (placedPickupables[i] != null)
            {
                Object.Destroy(placedPickupables[i]);
            }
        }

        PlacementSurface[] surfaces = root.GetComponentsInChildren<PlacementSurface>(true);
        for (int i = 0; i < surfaces.Length; i++)
        {
            if (surfaces[i] != null)
            {
                Object.Destroy(surfaces[i]);
            }
        }
    }

    public static ItemKind GetItemKindForWorkstation(WorkstationKind workstationKind)
    {
        switch (workstationKind)
        {
            case WorkstationKind.Furnace:
                return ItemKind.Furnace;
            case WorkstationKind.Forge:
                return ItemKind.Forge;
            case WorkstationKind.Loom:
                return ItemKind.Loom;
            case WorkstationKind.AlchemyTable:
                return ItemKind.AlchemyTable;
            case WorkstationKind.ScienceLab:
                return ItemKind.ScienceLab;
            default:
                return ItemKind.CraftingTable;
        }
    }

    public static GameObject SpawnWorkstation(WorkstationKind workstationKind, Vector3 position, Quaternion rotation, Transform parent, string objectName = null)
    {
        ItemKind itemKind = GetItemKindForWorkstation(workstationKind);
        GameObject root = SpawnPlacedObject(itemKind, position, rotation, parent);
        if (root != null && !string.IsNullOrEmpty(objectName))
        {
            root.name = objectName;
        }

        return root;
    }

    static bool TryGetValidatedData(ItemKind kind, string operation, out ItemData data)
    {
        data = ItemCatalog.Get(kind);
        if (data.IsValid)
        {
            return true;
        }

        Debug.LogWarning($"[ItemModule] {operation} failed: ItemData missing for {kind}.");
        return false;
    }

    static void ConfigureWorldPickup(GameObject pickupRoot, ItemKind kind, int amount)
    {
        PickupItemInteractionSetup.Configure(pickupRoot, kind, amount);
    }

    static void EnsurePlacedComponents(GameObject root, ItemKind kind, ItemData data)
    {
        PlacedObjectInteractionSetup.Configure(root, kind);

        Vector3 scale = data.PlacedBoundsSize.sqrMagnitude > 0.01f
            ? data.PlacedBoundsSize
            : PlacedObjectBuilder.GetPlacedVisualScale(kind);

        var surface = root.GetComponent<PlacementSurface>() ?? root.AddComponent<PlacementSurface>();
        surface.Initialize(kind);

        var placed = root.GetComponent<PlacedBuilding>() ?? root.AddComponent<PlacedBuilding>();
        placed.Initialize(kind, data.PlacedBuildingKind, data.PlacedWorkstationKind);

        if (data.PlacedWorkstationKind != WorkstationKind.None)
        {
            var station = root.GetComponent<CraftingStation>() ?? root.AddComponent<CraftingStation>();
            station.Configure(data.PlacedWorkstationKind, $"按 E 打开制造（{data.DisplayName}）");
            placed.AttachCraftingStation(station);
        }

        ItemWorldLabel.Create(root.transform, data.DisplayName, Vector3.up * (scale.y * 0.65f + 0.2f), 0.08f);
    }

    static void EnsureHandheldComponents(GameObject instance, ItemKind kind, HandheldToolProfile profile)
    {
        var visual = instance.GetComponent<HandheldToolVisual>();
        if (visual == null)
        {
            visual = instance.AddComponent<HandheldToolVisual>();
        }

        visual.Configure(kind, profile.Animations, profile.FallbackSwingEnabled);

        var marker = instance.GetComponent<HandheldToolPrototypeMarker>();
        if (marker == null)
        {
            marker = instance.AddComponent<HandheldToolPrototypeMarker>();
        }

        marker.Configure(kind);
    }

    static void ApplyHandheldTransform(Transform toolTransform, HandheldToolProfile profile, bool firstPerson)
    {
        if (firstPerson)
        {
            toolTransform.localPosition = profile.FirstPersonLocalPosition;
            toolTransform.localEulerAngles = profile.FirstPersonLocalEuler;
            toolTransform.localScale = profile.FirstPersonLocalScale;
        }
        else
        {
            toolTransform.localPosition = profile.ThirdPersonLocalPosition;
            toolTransform.localEulerAngles = profile.ThirdPersonLocalEuler;
            toolTransform.localScale = profile.ThirdPersonLocalScale;
        }
    }

    static void LogFallback(ItemKind kind, string representation)
    {
        if (!ItemCatalog.TryGet(kind, out ItemData data))
        {
            return;
        }

        bool usedPrefab = representation.Contains("prefab");
        if (!usedPrefab)
        {
            Debug.Log($"[ItemModule] {kind} using {representation} (color fallback).");
        }
    }
}
