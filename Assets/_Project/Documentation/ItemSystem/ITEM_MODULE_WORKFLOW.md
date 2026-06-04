# Modular Item Workflow

Add new items through this chain only:

1. Add enum value to `ItemKind`.
2. Register complete `ItemData` in `ItemCatalog.Register(...)`.
3. Assign Chinese name, description, category, stack rules, placeable/tool flags, and optional prefabs.
4. Spawn in scenes with `ItemModuleFactory` (never duplicate pickup/placed setup in level builders).

## Factory API

```csharp
ItemModuleFactory.SpawnWorldPickup(ItemKind.Wood, position, 5, pickupsRoot);
ItemModuleFactory.SpawnPlacedObject(ItemKind.CraftingTable, position, rotation, placedRoot);
ItemModuleFactory.SpawnHandheldTool(ItemKind.StonePickaxe, socket, isFirstPerson);
ItemModuleFactory.CreateInventoryEntry(ItemKind.Stone, 3);
ItemModuleFactory.CreateItemVisual(ItemKind.Wood, ItemVisualMode.WorldPickup, parent);
```

## Source of truth

- Identity: `ItemKind`
- Properties: `ItemCatalog.Get(kind)` → `ItemData`
- Prefabs are visual/behavior representations only.

## Legacy wrappers

- `WorldPickupItem.Create(...)` → `ItemModuleFactory.SpawnWorldPickup(...)`
- `PlacedObjectBuilder.SpawnPlacedObject(...)` → prefer `ItemModuleFactory.SpawnPlacedObject(...)`
- `CraftingStation.Create(...)` → `ItemModuleFactory.SpawnPlacedObject(...)` for workstation items
