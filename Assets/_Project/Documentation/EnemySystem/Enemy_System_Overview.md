# Enemy System Overview

> Modular enemy pipeline aligned with `ItemModuleFactory` / `ItemCatalog` patterns.  
> **First test scene:** `Level7` (weapon + enemy tutorial).

## Folder structure

```
Assets/_Project/Scripts/Enemies/
├── Combat/          DamageInfo, IDamageable
├── Core/            EnemyController, EnemyHealth, EnemyKnockback
├── Data/            EnemyKind, EnemyData, EnemyCatalog
├── Movement/        EnemyMovement
├── Visual/          EnemyVisual, EnemyDamageFeedback
└── Spawning/        EnemyModuleFactory, EnemyPlaceholderVisualBuilder

Assets/_Project/Prefabs/Enemies/   (reserved for future prefabs)
Assets/_Project/Documentation/EnemySystem/
```

## Add a new enemy

1. Add value to `EnemyKind` enum.
2. Register `EnemyData` in `EnemyCatalog.RegisterDefaults()`.
3. Spawn in scenes via `EnemyModuleFactory.SpawnEnemy(...)`.

Do **not** hand-build enemy logic in level builders except spawn positions.

## EnemyData (key fields)

| Field | Purpose |
|-------|---------|
| `MaxHealth` | HP pool |
| `MoveSpeed` / `CanMove` | Ground movement |
| `KnockbackResistance` | 0–1 reduction |
| `TaskIdOnFirstHit` | Level task bridge (optional) |
| `EventIdOnDeath` | Death task / `GameEventManager` (optional) |
| `EnemyPrefab` | Future real prefab (optional) |

## Spawn API

```csharp
EnemyModuleFactory.SpawnEnemy(EnemyKind.TrainingDummy, position, Quaternion.identity, enemiesRoot);
```

## Weapon damage path

```
PlayerWeaponController
  → HandheldWeaponVisual (fallback swing / recoil or per-weapon AnimationClip)
  → WeaponTargetingSystem (crosshair cone / ray, SingleTarget or MultiTarget)
  → IDamageable.TakeDamage(DamageInfo)
  → EnemyController → EnemyHealth
  → EnemyKnockback + EnemyVisual + WeaponHitEffectUtility
```

`EnemyCombatTargetingController` selects enemies via center-crosshair ray + spherecast + screen assist, with stickiness. Highlights use `EnemySurfaceHighlighter` (MaterialPropertyBlock on mesh surface). `WeaponTargetHighlightController` is a thin HUD facade.

Weapons never reference `EnemyController` directly for damage.

### Basic Sword defaults (v0.3)

- `AttackMode`: MeleeSwing
- `TargetMode`: **SingleTarget** (tutorial clarity)
- `Damage`: 20 · `AttackRange`: 2.25m · `AttackAngle`: 55° · `MaxTargets`: 1 · `Cooldown`: 0.5s

## Layers

- **Enemy** layer (index 8 in TagManager) — set via `GameplayLayers.TrySetEnemyLayer`.
- Combat raycasts use `GameplayLayers.CombatTargetMask`.

## Level 7 test layout

| Spawn | Kind | Purpose |
|-------|------|---------|
| West | `TrainingDummy` | Melee hit task |
| East | `TrainingRangedTarget` | Ranged hit task |
| Center | `BasicWalker` | Defeat + knockback + movement |

Progression: `Level7TaskProgressTracker` → `Level7ProgressionManager` → portal to **Level8**.

Starter weapons (debug): `BasicSword`, `TrainingBlaster` via `Level7TutorialSettings`.

## Replace placeholder art

1. Create prefab under `Prefabs/Enemies/`.
2. Assign `EnemyData.EnemyPrefab` in `EnemyCatalog`.
3. Factory will instantiate prefab and still run `EnemyController.Initialize`.

## Future AI

Add profiles under `Scripts/Enemies/AI/` and reference from `EnemyData` — do not hardcode behavior inside `BasicWalker` spawn code.
