# Tool / Weapon Prefab Architecture

> **Status:** Planned architecture documentation only.  
> **Scope:** Design guidance for future tool/weapon prefab work.  
> **Do not implement from this document alone** — confirm existing backend fields and player systems first.

Related docs:

- `Documentation/ItemSystem/ITEM_MODULE_WORKFLOW.md`
- `Documentation/ImportWorkflow/Unified_Gameplay_Prefab_Rule.md`
- `Documentation/ImportWorkflow/Blender_Unity_Axis_Alignment_Rule.md`

---

## 1. Why Tools and Weapons Need More Structure

Simple **material / resource** items usually follow a short pipeline:

```
World drop → Pickup → Inventory → Crafting consumption
```

**Tools and weapons** are more complex. They usually follow:

```
World drop → Pickup → Inventory / Hotbar → Equip → Show in player hand
→ Play action / animation → Apply hit or gather logic → Cooldown / durability / effect
```

Because of this, tools and weapons should **not** be treated as a single simple prefab.

They typically require:

- **World pickup behavior** — scene instance, prompt, collider, pickup into inventory
- **Inventory / hotbar behavior** — stack rules, slot assignment, equip selection
- **Equipped / handheld visual** — separate prefab or transform tuned for hand socket
- **Player hand attachment** — first-person and third-person socket alignment
- **Action animation or swing behavior** — fallback swing or Animator-driven clips
- **Hit / gather / attack detection** — raycast, overlap, or interactable targeting
- **Cooldown / durability / damage / gather power** — defined in data profiles, not on FBX meshes
- **Optional placed behavior** — e.g. Torch as world pickup, handheld light, and placed light source

In this project, identity and behavior data live in **`ItemKind` → `ItemCatalog` → `ItemData`**, with spawning via **`ItemModuleFactory`**. Tools use **`HandheldToolProfile`**; weapons use **`WeaponProfile`**.

---

## 2. Three Main Asset Layers

Tools and weapons should be planned as **three separate layers**, even when they share the same FBX mesh.

### A. World Pickup Prefab

Used when the item appears on the ground.

**Example:**

```
PF_Generic_Item_StoneAxe_01
├── LogicRoot
│   ├── WorldPickupItem
│   ├── PickupCollider / Trigger
│   ├── InteractionPoint
│   └── LabelAnchor
└── Visual
    └── AxisCorrection
        └── Generic_Item_StoneAxe_01
```

**Rules:**

- Used for scene pickup only
- Enters inventory / hotbar after successful pickup
- Disappears from scene after successful pickup (no duplicate pickup, no visual/collider leftovers)
- FBX child remains **visual-only**
- Gameplay scripts stay on prefab root / **LogicRoot**, not on FBX mesh children
- Follow the same complete prefab pattern as resource pickups (`Unified_Gameplay_Prefab_Rule.md`)

**Planned shared folder (already reserved in code):**

- `Assets/_Project/Prefabs/Items/WorldPickups/Tools/`

### B. Handheld / Equipped Prefab

Used when the player equips the item.

**Example:**

```
PF_Handheld_StoneAxe_01
├── GripPoint          (optional visual helper)
├── HitOrigin          (optional, if supported by hit system)
├── HitDirection       (optional)
└── Visual
    └── Generic_Item_StoneAxe_01
```

**Rules:**

- Attached to player hand / **ToolHolder** / **WeaponHolder** socket via `ItemModuleFactory.SpawnHandheldTool(...)`
- Uses **separate** `localPosition` / `localRotation` / `localScale` from the world pickup prefab
- Must align with player hand socket (first-person and third-person offsets may differ)
- Should **not** contain world pickup collider or `WorldPickupItem`
- May contain hit origin or visual-only helper markers if the existing tool/weapon system supports them
- Handheld prefab reference is stored on the item profile:
  - Tools: `ItemData.HandheldTool.HandheldPrefab`
  - Weapons: transform offsets on `WeaponProfile` today; dedicated handheld prefab fields may be added later if needed

**Current project behavior:**

- If `HandheldToolProfile.HandheldPrefab` is null, `HandheldToolPrefabBuilder.GetOrCreatePrefab(kind)` generates a runtime prototype.
- Future work should replace prototypes with authored `PF_Handheld_*` prefabs.

### C. Gameplay Data / Tool Profile

Used to define behavior. This is **data**, not the FBX mesh.

**Example conceptual fields (map to existing project types):**

| Concept | Project type / field |
|--------|----------------------|
| Tool type | `HandheldToolProfile.ToolKind` (`ToolKind.Axe`, `Pickaxe`, …) |
| Weapon type | `WeaponProfile.WeaponKind` / `WeaponKind` |
| Handheld prefab | `HandheldToolProfile.HandheldPrefab` |
| World pickup prefab | `ItemData.WorldPickupPrefab` or registry path via `ItemWorldPickupAssetPaths` |
| Placed prefab (Torch etc.) | `ItemData.PlacedPrefab` or placed registry |
| Damage | `WeaponProfile.Damage` |
| Attack range / gather range | `WeaponProfile.AttackRange`, tool interact range in scene systems |
| Cooldown | `HandheldToolProfile.UseCooldown`, `WeaponProfile.AttackCooldown` |
| Durability | `HandheldToolProfile.HasDurability`, `MaxDurability` |
| Swing / attack animation | `HandheldToolProfile.Animations`, `WeaponProfile` fallback attack fields |
| Hit detection mode | `WeaponProfile.AttackMode`, `TargetMode`; tools via `ToolInteractable` |
| Projectile | `WeaponProfile.ProjectilePrefab`, `ProjectileKind`, … |

**Exact field names must follow the project's existing systems:**

- `ItemData`
- `HandheldToolProfile`
- `WeaponProfile`
- `ToolKind` / `WeaponKind`

Do not invent parallel profile types unless backend work is explicitly approved.

---

## 3. Recommended Tool Item Structure

Use **StoneAxe** as the reference example.

### World pickup prefab

```
PF_Generic_Item_StoneAxe_01
├── LogicRoot
│   ├── WorldPickupItem
│   ├── PickupCollider
│   ├── InteractionPoint
│   └── LabelAnchor
└── Visual
    └── AxisCorrection
        └── Generic_Item_StoneAxe_01
```

### Handheld prefab

```
PF_Handheld_StoneAxe_01
├── GripPoint
├── HitOrigin
├── HitDirection
└── Visual
    └── Generic_Item_StoneAxe_01
```

### Item data / profile (conceptual)

```
StoneAxe (ItemKind.StoneAxe)
├── ItemData.WorldPickupPrefab = PF_Generic_Item_StoneAxe_01   (future)
├── HandheldToolProfile
│   ├── HandheldPrefab = PF_Handheld_StoneAxe_01             (future)
│   ├── ToolKind = Axe
│   ├── UseCooldown
│   ├── FirstPersonLocalPosition / Euler / Scale
│   ├── ThirdPersonLocalPosition / Euler / Scale
│   └── Animations (ToolAnimationProfile)
└── CraftingRecipe (e.g. stick + stone + rope)
```

**Current project state (as of this document):**

- `ItemKind.StoneAxe`, `StonePickaxe`, `RepairTool`, `BasicKnife`, `Hammer` are registered as handheld tools in `ItemCatalog`.
- `ItemKind.Torch` exists as consumable; placed/handheld/light behavior may need separate prefab planning.
- Tool world pickup prefabs under `WorldPickups/Tools/` are **not created yet**.
- Handheld prefabs are mostly **runtime prototypes**, not authored `PF_Handheld_*` assets.

---

## 4. Tool-Specific Considerations

### Axe

Used for tree / wood gathering (`ToolKind.Axe`, `ToolInteractable` in Level6).

Consider:

- Gather target type (tree stump, resource node, etc.)
- Gather power / reward table
- Swing range and facing
- Cooldown (`HandheldToolProfile.UseCooldown`)
- Hit feedback (VFX, sound, UI prompt)
- Durability if enabled later (`HasDurability`, `MaxDurability`)

### Pickaxe

Used for stone / ore gathering (`ToolKind.Pickaxe`).

Consider:

- Mineable target type
- Gather power
- Hit range
- Cooldown
- Durability if supported

### Torch

Torch may require **three** asset roles:

1. **WorldPickup Torch** — dropped / spawned pickup prefab
2. **Handheld Torch** — visible in hand while equipped; optional hand light
3. **Placed Torch** — optional placed prefab with light / flame VFX

Consider:

- `Light` component (handheld vs placed)
- Flame VFX anchor (outside FBX or on prefab logic child)
- `ItemData.PlacedPrefab` if placeable torch is supported
- Optional fuel duration (future; not required for first version)

---

## 5. Weapon-Specific Considerations

Weapons use **`WeaponProfile`** and **`ItemCategory.Weapon`**.

May require:

- Damage value (`WeaponProfile.Damage`)
- Attack range (`AttackRange`, `MeleeRadius`, `AttackRadius`)
- Attack direction / cone (`AttackAngle`)
- Cooldown (`AttackCooldown`)
- Hitbox / raycast / overlap detection (`AttackMode`, `TargetMode`)
- Knockback (`KnockbackForce`)
- Enemy hit reaction (Level7+ combat teaching)
- Animation timing (`FirstPersonAttackAnimation`, fallback swing fields)
- Animation events (future)
- Projectile prefab for ranged weapons (`ProjectilePrefab`, `ProjectileKind`)

**Examples:**

| Weapon style | Notes |
|--------------|-------|
| Sword / Knife | Short-range melee; `WeaponKind.Melee` |
| Spear | Longer thrust range; may need distinct hit origin |
| Bow / Blaster | Projectile + ammo logic; `ProjectilePrefab`, lifetime, pierce rules |

**Current project examples:**

- `ItemKind.BasicSword` → melee weapon profile
- `ItemKind.TrainingBlaster` → projectile weapon profile

---

## 6. Animation and Player Action Notes

Tool/weapon integration must account for **player actions**.

### Minimum viable implementation

1. Equip item (hotbar / inventory selection)
2. Show item in hand (`SpawnHandheldTool` or weapon equip path)
3. Play simple swing / use animation (fallback swing enabled today on tools)
4. Run raycast / overlap / interactable hit detection
5. Apply gather or damage result

### Future improved implementation

- Animator animation clips per tool/weapon
- Animation events for active hit frames
- Different animations per `ToolKind` / `WeaponKind`
- Weapon-specific timing and combo rules

**Design requirement:** Prefabs and profiles should be authored so animation can be added later without restructuring FBX or moving gameplay onto mesh children.

---

## 7. Visual Axis Rule

Use the project's current Blender-import visual correction pattern.

### World pickup prefabs

```
Visual
└── AxisCorrection
    └── FBX_Model
```

| Node | Transform |
|------|-----------|
| Visual | `(0, 0, 0)` position, `(0, 0, 0)` rotation, `(1, 1, 1)` scale |
| AxisCorrection | `(0, 0, 0)` position, **`(90, 0, 180)`** rotation, `(1, 1, 1)` scale |
| FBX_Model | `(0, 0, 0)` position, `(0, 0, 0)` rotation, `(1, 1, 1)` scale |

**Do not rotate:**

- Prefab root
- LogicRoot
- Collider / Trigger
- InteractionPoint
- LabelAnchor

See `Documentation/ImportWorkflow/Blender_Unity_Axis_Alignment_Rule.md`.

### Handheld prefabs

Handheld transforms are tuned relative to the player's hand socket / **ToolHolder**, not copied from world pickup AxisCorrection.

- Use `HandheldToolProfile.FirstPersonLocalPosition/Euler/Scale`
- Use `HandheldToolProfile.ThirdPersonLocalPosition/Euler/Scale`
- Weapons use analogous fields on `WeaponProfile`

**Do not assume** world pickup rotation is correct for handheld use.

---

## 8. FBX Rule

The FBX model is always **visual-only**.

**Do not put on FBX mesh children:**

- `WorldPickupItem`
- Collider / Trigger
- Inventory logic
- Tool logic / weapon logic
- Crafting logic
- Damage / gather logic

**Gameplay belongs on:**

- Prefab root
- LogicRoot (world pickup)
- `ItemData` / `HandheldToolProfile` / `WeaponProfile`
- Player tool / weapon controller
- Dedicated hit detection objects if the existing system supports them

---

## 9. Recommended Implementation Order

Before creating tool/weapon prefabs, inspect:

| Area | Where to look |
|------|----------------|
| Item identity | `ItemKind`, `ItemCatalog` |
| Recipes | `CraftingRecipeDatabase` |
| World pickup support | `WorldPickupItem`, `ItemModuleFactory.SpawnWorldPickup`, `ItemPickupVisualResolver`, `Level8ResourcePrefabRegistry` |
| Handheld support | `HandheldToolProfile`, `HandheldToolPrefabBuilder`, `ItemModuleFactory.SpawnHandheldTool` |
| Weapon support | `WeaponProfile`, weapon equip / attack controllers |
| Placed support | `ItemData.PlacedPrefab`, `ItemPlacedPrefabRegistry` (Torch) |
| Player attachment | ToolHolder / WeaponHolder sockets, first/third person rigs |
| Hit detection | `ToolInteractable`, weapon hit / projectile systems |
| Animation | `ToolAnimationProfile`, `WeaponProfile` animation clips, fallback swing |

### Recommended first tool batch

1. **StoneAxe**
2. **StonePickaxe**
3. **Torch**

**Reason:**

- StoneAxe and StonePickaxe validate the basic gather-tool workflow (Level6)
- Torch validates handheld + optional placed + light/VFX behavior

Suggested asset paths when implementation begins:

| Asset | FBX | World pickup prefab | Handheld prefab |
|-------|-----|---------------------|-----------------|
| StoneAxe | `Art/Imported/FBX/Items/Tools/Generic_Item_StoneAxe_01.fbx` | `Prefabs/Items/WorldPickups/Tools/PF_Generic_Item_StoneAxe_01.prefab` | `Prefabs/Items/Handheld/PF_Handheld_StoneAxe_01.prefab` (folder TBD) |
| StonePickaxe | same pattern | same pattern | same pattern |
| Torch | same pattern | same pattern + optional placed prefab | same pattern |

---

## 10. Do Not Do Yet

Until backend support is confirmed and this architecture is approved for implementation:

- Do **not** invent new `ItemKind` values
- Do **not** invent new tool/weapon systems parallel to `HandheldToolProfile` / `WeaponProfile`
- Do **not** rewrite player controller
- Do **not** rewrite crafting or inventory systems
- Do **not** create a full weapon animation system from scratch in the same pass as first prefabs
- Do **not** attach gameplay scripts directly to FBX meshes
- Do **not** use 3D prefabs as inventory icons (icons live under `Art/UI/Items/Icons/`)

---

## 11. Future Checklist for Each Tool / Weapon

For each future tool or weapon, verify:

- [ ] `ItemKind` exists
- [ ] `ItemCatalog` entry exists
- [ ] Crafting recipe exists (if craftable)
- [ ] World pickup prefab exists (`PF_Generic_Item_*` under `WorldPickups/Tools/` or weapons folder)
- [ ] Handheld prefab exists (`PF_Handheld_*`)
- [ ] `HandheldToolProfile` or `WeaponProfile` configured
- [ ] `HandheldToolProfile.HandheldPrefab` / weapon equip path assigned
- [ ] Player can equip from hotbar/inventory
- [ ] Correct hand attachment (FP + TP offsets tested)
- [ ] Action animation or fallback swing works
- [ ] Hit / gather / attack detection works
- [ ] Item can be picked up from world
- [ ] Scene instance disappears after pickup
- [ ] Item appears in inventory / hotbar
- [ ] Item can be re-equipped after pickup
- [ ] No console errors
- [ ] Regression: unrelated resources/stations still work

---

## Appendix: Current Gap Summary

| Item group | Backend | World pickup prefab | Handheld prefab |
|------------|---------|---------------------|-----------------|
| Resources (Wood, Stone, …) | Yes | Yes (shared `WorldPickups/Resources/`) | N/A |
| Crafted materials (Stick, Rope, …) | Yes | Yes | N/A |
| Workstations (CraftingTable, Furnace, …) | Yes | N/A | N/A (placed prefabs instead) |
| Tools (StoneAxe, StonePickaxe, …) | Yes | **Missing** | **Prototype / missing authored prefab** |
| Weapons (BasicSword, …) | Yes | **Missing** | **Profile offsets / prototype** |
| Torch | Partial | **Missing** | **Missing**; placed behavior TBD |

This document describes the **planned** structure to close those gaps without changing gameplay code yet.
