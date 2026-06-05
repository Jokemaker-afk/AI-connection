# Modular Item Workflow

> Blueprint v0.2：工具（`ToolKind`）与武器（`WeaponKind`）共用 `ItemKind` / `ItemCatalog` / `ItemModuleFactory`，但用途分离。详见 `AI_Connection_Game_Blueprint.md` §4.7。

## Add a new item

1. Add enum value to `ItemKind`.
2. Register complete `ItemData` in `ItemCatalog.Register(...)`.
3. Assign Chinese name, description, category, stack rules, placeable/tool/weapon flags, and optional prefabs.
4. Spawn in scenes with `ItemModuleFactory` (never duplicate pickup/placed setup in level builders).

### Tool vs weapon registration

| 类型 | `ItemCategory` | 分类字段 | 场景交互 |
|------|----------------|----------|----------|
| 手持工具 | `Tool` | `ToolKind` | `ToolInteractable`（采矿、伐木、修复等） |
| 武器 | `Weapon` | `WeaponKind`（Melee / Bow / Firearm 等） | `DamageableTarget`（规划中，Level7 起） |

示例（已实现）：

- `ItemKind.StonePickaxe` → `ToolKind.Pickaxe`
- `ItemKind.BasicSword` → `WeaponKind.Melee`

远程教学武器（木弓、训练发射器等）注册时同样走 `ItemCategory.Weapon` + 对应 `WeaponKind`，由 Level7+ 场景通过 `ItemModuleFactory` 生成拾取物。

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

## Level scene notes (v0.2)

| 关卡 | 物品相关布局 |
|------|----------------|
| Level6 | 材料拾取 + 工具交互（`ToolInteractable`） |
| Level7 | 武器教学占位：`BasicSword` 拾取 + 训练假人/靶（无伤害逻辑） |
| Level8+ | 模块化探索；资源点通过 Catalog / Factory 生成 |
