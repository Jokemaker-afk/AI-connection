# Scene Module Migration Plan

Audit of **Level 3 (Buff 教学关)** scene objects and a safe path toward reusable prefab modules with optional Blender visuals.

**Scope of this document:** architecture and audit only. No gameplay changes, no mass prefab replacement.

---

## Executive summary

Level 3 content lives primarily under **`BuffHub`** in `Scenes/Levels/Level3/Level3.unity`. Most functional objects are **code-generated primitives** created by:

| Builder / Script | Creates |
|------------------|---------|
| `Level3BuffHubBuilder.cs` | Full BuffHub (editor menu regenerate) |
| `BuffBubble.Create()` | Buff pickup bubbles (Heal / Speed / Stamina / Shield) |
| `HubReturnZone.Create()` | Return zones + ReturnButton visual |
| `HubAdvancePortal` | Advance portal (runtime, after 4 buffs collected) |
| `GuaranteedDamageZone.Create()` | +Z path damage trap |
| `SpikeTrap.Create()` | -X path spike trap |
| `Level3BuffHubBuilder.CreateSign()` | Tutorial sign cubes |

**Runtime bootstrap:** `GameplayFoundationBootstrap.EnsureLevel3Content()` adds `Level3Systems` + `BuffCollectionTracker` + `HubAdvancePortal` if missing. It does **not** regenerate BuffHub on every play — the scene file holds the baked layout.

**First migration target:** `ReturnButton` module (via `HubReturnZone` prefab).

---

## Level 3 hierarchy (scene root)

```
Level3.unity
├── BuffHub                          ← main world content
│   ├── Floors                       ← HubFloor + 4 path cubes
│   ├── Markers                      ← signs, end pads, trap signs
│   ├── Hazards                      ← Trap_+Z, Trap_-X
│   ├── BuffBubbles                    ← 4× BuffBubble roots
│   └── ReturnZones                  ← 4× HubReturnZone roots
├── Player
├── Main Camera
├── GameplayHUD                      ← Canvas / UI (do not Blender)
├── Directional Light
└── (runtime) Level3Systems          ← added by bootstrap if absent
    └── AdvancePortal                ← spawned when all buffs collected
```

---

## Stage 1 — Object audit

### A. BuffBubble / BubbleShell (Buff Station)

| Field | Detail |
|-------|--------|
| **Scene paths** | `BuffHub/BuffBubbles/Bubble_+Z 回血`, `Bubble_+X 加速`, `Bubble_-Z 无限精力`, `Bubble_-X 护盾` |
| **Root components** | `BuffBubble`, `SphereCollider` (trigger, radius ~1.1) |
| **Visual children** | `BubbleShell` (Sphere), `BubbleCore` (Sphere), `BubbleRing` (Cylinder) — all primitives, colliders stripped |
| **Function** | Player enters trigger → `PlayerBuffController.TryApplyBuff()` → one-shot hide |
| **Blender candidate** | **Yes** — clear world identity (energy bubble) |
| **Prefab candidate** | **Yes** — `BuffStationModule` reusable across levels |
| **Risk** | **Medium** — buff type enum + trigger tuning; 4 color variants |

**Note:** This is the **Heal / Stamina** gameplay object. `Return_+Z 回血` is a **different** object (return zone, see below).

---

### B. HubReturnZone / ReturnButton

| Field | Detail |
|-------|--------|
| **Scene paths** | `BuffHub/ReturnZones/Return_+Z 回血`, `Return_+X 加速`, `Return_-Z 无限精力`, `Return_-X 护盾` |
| **Root components** | `HubReturnZone`, `BoxCollider` (trigger, ~3×2.5×3) |
| **Visual children** | `ReturnButton` (Cube primitive, emissive blue), `LabelAnchor` (empty) |
| **Function** | Player inside zone + **R key** → teleport to hub spawn |
| **Blender candidate** | **Yes** — ReturnButton is pure visual |
| **Prefab candidate** | **Yes** — highest priority first migration |
| **Risk** | **Low** — simple trigger + input, static spawn ref |

---

### C. Tutorial signs

| Field | Detail |
|-------|--------|
| **Scene paths** | `BuffHub/Markers/Sign_*`, `HubSign`, `ReturnSign_*`, `TrapSign_*` |
| **Components** | None on root (static Cube + `TextHint` empty child) |
| **Mesh** | Cube primitive (dark panel) |
| **Function** | Visual-only hints (text not wired to UI Text yet — placeholder) |
| **Blender candidate** | **Yes** — board / hologram stand |
| **Prefab candidate** | **Yes** — `TutorialSignModule` |
| **Risk** | **Low** — no gameplay scripts |

---

### D. Hazards

| Object | Path | Script | Collider | Risk |
|--------|------|--------|----------|------|
| `Trap_+Z 回血` | `BuffHub/Hazards/` | `GuaranteedDamageZone` | Box trigger | Medium |
| `Trap_-X 护盾` | `BuffHub/Hazards/` | `SpikeTrap` | Box trigger on **LogicRoot** | Medium |
| Spike visuals | `VisualRoot` child | — | — | Low (visual only) |

**Spike / ground spike trap hazard module (full reusable prefab — Level 1–3 runtime + baked migration):**

| Item | Detail |
|------|--------|
| **Module prefab** | `Assets/_Project/Prefabs/SceneModules/Hazards/PF_Generic_Hazard_SpikeTrap_01.prefab` |
| **Visual prefab** | `Assets/_Project/Prefabs/SceneModules/Hazards/PF_Generic_Hazard_SpikeTrap_01_Visual.prefab` (nested under module VisualRoot) |
| **FBX** | `Assets/_Project/Art/Imported/FBX/SceneModules/Hazards/Generic_Hazard_SpikeTrap_01.fbx` |
| **Classification** | **Full module** = LogicRoot + VisualRoot + WarningArea + LabelAnchor + DebugBounds |
| **Registry kinds** | `SceneModuleKind.SpikeTrapHazard` (full module), `SceneModuleKind.SpikeHazard` / `Hazard` (visual-only) |
| **Catalog** | `HazardSpikePrefabCatalog.TryResolveModulePrefab()` / `TryResolveVisualPrefab()` |
| **Factory API** | `SpikeTrapHazardFactory.CreateSpikeHazard(parent, name, position, size, spikeCount, damage, active)` |
| **Legacy create API** | `SpikeTrap.Create()` — delegates to factory + prefab |
| **Backend script** | `SpikeTrap` on **LogicRoot** only (discrete damage + cooldown; unchanged behavior) |
| **Configure API** | `SpikeTrap.Configure(footprintSize, damage, active)` + `SyncModuleLayout()` |
| **Collider sync** | `SyncColliderToVisualBounds()` — LogicRoot BoxCollider from VisualRoot renderers |
| **Warning sync** | `SyncWarningAreaToColliderOrVisual()` — WarningPlane from collider ground footprint |
| **Fallback** | `SpikeTrap.BuildPrimitiveSpikeFallback()` when module/visual prefab missing |
| **Active state** | `SpikeTrap.SetActiveState(bool)` dims visuals + disables trigger |
| **Layout helpers** | `HazardModuleLayoutUtility` — WarningArea, LabelAnchor, DebugBounds |

**Prefab module ownership:**

| Child | Owns |
|-------|------|
| **LogicRoot** | `SpikeTrap` + `BoxCollider` trigger (only gameplay collider) |
| **VisualRoot** | `PF_Generic_Hazard_SpikeTrap_01_Visual` or primitive fallback (visual-only) |
| **WarningArea / WarningPlane** | Orange/red ground warning (visual-only, no collider) |
| **LabelAnchor** | Future world label hook |
| **DebugBounds** | Editor collider preview (hidden at runtime) |

Runtime hierarchy:

```
PF_Generic_Hazard_SpikeTrap_01  (or scene instance Spikes_* / Trap_*)
├── LogicRoot          ← SpikeTrap + BoxCollider trigger
├── VisualRoot         ← PF_Generic_Hazard_SpikeTrap_01_Visual or primitive fallback
├── WarningArea
│   └── WarningPlane   ← synced to collider footprint
├── LabelAnchor
└── DebugBounds        ← editor-only preview
```

**Builder rule:** instantiate the module prefab, set transform + `Configure(footprint, damage, active)` — do **not** separately create WarningArea, VisualRoot, or damage collider.

Editor menus:

- `Tools/SceneModules/Integrate Hazard SpikeTrap Module` — rebuild prefab + registry
- `Tools/SceneModules/Replace All Spike Hazards With Prefab Module (All Level Scenes)`
- `Tools/SceneModules/Sync All Spike Hazard Prefab Modules`
- `Tools/SceneModules/Normalize Existing Trap Visuals` (legacy layout normalize)
- `Tools/SceneModules/Migrate Spike Traps In All Level Scenes`
- `Tools/SceneModules/Clear Spike Hazard Prefab Registry (Test Fallback)`

**Level coverage (SpikeTrap prefab module):**

| Level | Spike usage | Notes |
|-------|-------------|-------|
| L1 | `GameplayElementsBuilder` → 6× `SpikeTrapHazardFactory` | Prefab module instances |
| L2 | `ParkourLevelBuilder` → 2× `SpikeTrapHazardFactory` | same |
| L3 | `Level3BuffHubBuilder` → `Trap_-X 护盾` | same (`Trap_+Z` uses `GuaranteedDamageZone` — **not** spike prefab) |
| L4–L7 | No `SpikeTrap` in scenes/builders | N/A |
| L8 | Biome hazard **zones** | **Not** spike prefab — area markers, separate system |

**Migration workflow:**

1. Run `Integrate Hazard SpikeTrap Module` to bake WarningPlane into prefab.
2. Run `Replace All Spike Hazards With Prefab Module (All Level Scenes)` for baked scenes.
3. Run `Sync All Spike Hazard Prefab Modules` to verify collider/warning alignment.

**Fallback test:** `Clear Spike Hazard Prefab Registry` → builders still create primitive spikes + damage via `SpikeTrap.CreateFallbackModule`.

**Excluded from spike prefab (by design):**

- `GuaranteedDamageZone` — percentage health loss red zone (L3 `Trap_+Z`)
- `LaserHazard` — uses `SceneModuleKind.ElectricField` prefab
- Level8 hazard chunks — biome zone visuals, not spike traps

**Electric Field / Laser hazard module (full reusable prefab — Level 1–2 runtime + baked migration):**

| Item | Detail |
|------|--------|
| **Module prefab** | `Assets/_Project/Prefabs/SceneModules/Hazards/PF_Generic_Hazard_ElectricField_01.prefab` |
| **Visual prefab** | `Assets/_Project/Prefabs/SceneModules/Hazards/PF_Generic_Hazard_ElectricField_01_Visual.prefab` (nested under module VisualRoot) |
| **FBX** | `Assets/_Project/Art/Imported/FBX/SceneModules/Hazards/Generic_Hazard_ElectricField_01.fbx` |
| **Classification** | **Full module** = LogicRoot + VisualRoot + WarningArea + LabelAnchor + DebugBounds |
| **Registry kinds** | `SceneModuleKind.ElectricFieldHazard` (full module), `SceneModuleKind.ElectricField` (visual-only) |
| **Catalog** | `HazardElectricFieldPrefabCatalog.TryResolveModulePrefab()` / `TryResolveVisualPrefab()` |
| **Factory API** | `ElectricFieldHazardFactory.CreateElectricField(parent, name, position, size, damage, active)` |
| **Legacy create API** | `LaserHazard.Create()` / `CreateElectricFieldHazard()` — delegates to factory + prefab |
| **Backend script** | `LaserHazard` on **LogicRoot** only (discrete damage + cooldown; unchanged) |
| **Configure API** | `LaserHazard.Configure(size, damage, active)` + `SyncModuleLayout()` |
| **Collider sync** | `SyncColliderToVisualBounds()` — LogicRoot BoxCollider from VisualRoot renderers |
| **Warning sync** | `SyncWarningAreaToColliderOrVisual()` — WarningPlane from collider ground footprint |
| **Fallback** | `LaserHazard.BuildPrimitiveElectricFieldFallback()` when module/visual prefab missing |
| **Active state** | `LaserHazard.SetActiveState(bool)` dims visuals + disables trigger |
| **Layout helpers** | `HazardModuleLayoutUtility` — WarningArea, LabelAnchor, DebugBounds |

**Prefab module ownership:**

| Child | Owns |
|-------|------|
| **LogicRoot** | `LaserHazard` + `BoxCollider` trigger (only gameplay collider) |
| **VisualRoot** | `PF_Generic_Hazard_ElectricField_01_Visual` or primitive fallback (visual-only) |
| **WarningArea / WarningPlane** | Transparent ground warning (visual-only, no collider) |
| **LabelAnchor** | Future world label hook |
| **DebugBounds** | Editor collider preview (hidden at runtime) |

Runtime hierarchy:

```
PF_Generic_Hazard_ElectricField_01  (or scene instance Laser_*)
├── LogicRoot          ← LaserHazard + BoxCollider trigger
├── VisualRoot         ← PF_Generic_Hazard_ElectricField_01_Visual or primitive fallback
├── WarningArea
│   └── WarningPlane   ← synced to collider footprint
├── LabelAnchor
└── DebugBounds        ← editor-only preview
```

**Builder rule:** instantiate the module prefab, set transform + `Configure(size, damage, active)` — do **not** separately create WarningArea, VisualRoot, or damage collider.

Editor menus:

- `Tools/SceneModules/Integrate Hazard ElectricField Module` — rebuild prefab + registry
- `Tools/SceneModules/Convert ElectricField Instances To Prefab Modules` — replace baked scene objects with prefab instances
- `Tools/SceneModules/Sync ElectricField Prefab Modules` — collider + warning + debug sync
- `Tools/SceneModules/Sync ElectricField Colliders And Warning Areas`
- `Tools/SceneModules/Migrate ElectricField Hazards In All Level Scenes`
- `Tools/SceneModules/Clear ElectricField Hazard Prefab Registry (Test Fallback)`

**Level coverage:**

| Level | ElectricField usage | Notes |
|-------|---------------------|-------|
| L1 | `GameplayElementsBuilder` → 6× `ElectricFieldHazardFactory` | Prefab module instances |
| L2 | `ParkourLevelBuilder` → 2× `ElectricFieldHazardFactory` | same |
| L3–L8 | No `LaserHazard` | L8 hazard chunks use zone markers, not electric field module |

**Migration workflow:**

1. Run `Integrate Hazard ElectricField Module` to ensure prefab includes WarningPlane baked in.
2. Run `Replace All ElectricFields With Prefab Module (All Level Scenes)` for baked scenes.
3. Run `Sync All ElectricField Prefab Modules` to verify collider/warning alignment.

**Replacement status (L1–L2):**

| Scene | ElectricField count | Replacement |
|-------|---------------------|-------------|
| L1 `SampleScene.unity` | 6 (`Laser_F1_A/B/C`, `Laser_F2_A/B`, `Laser_F3`) | Editor menu → prefab module instances |
| L2 `Level2.unity` | 2 (`Laser_BeamSection`, `Laser_ClimbGate`) | Runtime `ParkourLevelBuilder` + scene bake via menu |
| L3–L8 | 0 | N/A |

Editor menus for full prefab replacement:

- `Tools/SceneModules/Replace All ElectricFields With Prefab Module`
- `Tools/SceneModules/Replace All ElectricFields With Prefab Module (All Level Scenes)`
- `Tools/SceneModules/Sync All ElectricField Prefab Modules`

**Fallback test:** `Clear ElectricField Hazard Prefab Registry` → builders still create primitive electric field + damage via `LaserHazard.CreateFallbackModule`.

**Electric Field / Laser hazard visual replacement (legacy — superseded by full module above):**

| Item | Detail |
|------|--------|
| **Visual prefab** | `PF_Generic_Hazard_ElectricField_01_Visual.prefab` |
| **Runtime builder (old)** | Separate VisualRoot + WarningArea assembly — **replaced by full module prefab** |
| **Logic rule** | Unchanged: **LogicRoot** owns gameplay; **VisualRoot** appearance only |

Already in **`Scripts/Levels/Shared/`** and **`Scripts/SceneModules/Hazards/`** — good global reuse pattern.

---

### E. Floors / paths

| Object | Path | Components | Blender | Prefab | Risk |
|--------|------|------------|---------|--------|------|
| `HubFloor`, `Path_*`, `End_*` | `BuffHub/Floors`, `Markers` | MeshCollider on cubes | Yes (terrain tiles) | Yes (environment) | Low |

Environment modules — defer until chunk system matures (Level 8 `Prefabs/Environment/`).

---

### F. HubAdvancePortal (Level portal)

| Field | Detail |
|-------|--------|
| **Scene presence** | **Not baked** — created at runtime by `HubAdvancePortal.ShowPortal()` |
| **Visual** | `PortalPlatform`, `PortalRing`, `PortalCore` (primitives) + `PortalTrigger` + `LevelGoal` |
| **Function** | Appears after 4 buffs → Y key → Level 4 via `LevelManager` |
| **Blender candidate** | **Yes** |
| **Prefab candidate** | **Yes** — `LevelPortalModule` |
| **Risk** | **High** — tied to progression, `LevelGoal`, tracker events |

---

### G. HealthBar / StaminaBar (UI)

| Field | Detail |
|-------|--------|
| **Scene path** | `GameplayHUD/StatsPanel/HealthBar`, `StaminaBar` |
| **Components** | `RectTransform`, Unity UI `Slider` / `Image` |
| **Function** | HUD display bound to `PlayerStats` |
| **Blender candidate** | **No** |
| **Prefab candidate** | `HealthBarUI.prefab` under UI prefabs only |
| **Risk** | **High** if treated as world mesh |

---

### H. Player / Camera / GameplayCore

| Object | Action |
|--------|--------|
| `Player` | **Do not prefab-module** — persistent rig |
| `Main Camera` | Bootstrap-managed |
| `GameplayHUD` | UI prefab only |

---

## Stage 2 — Classification

### A. Good Blender model replacement candidates

- `BubbleShell` / full `BuffBubble` visual group
- `ReturnButton` visual
- Portal rings / platform (`HubAdvancePortal`)
- Tutorial sign panels
- Hazard zone meshes (keep trigger on LogicRoot)
- Floor/path tiles (environment art)

### B. Prefab / module candidates

| Module type | Level 3 instances |
|-------------|-------------------|
| Interaction Button | `ReturnButton` (inside HubReturnZone) |
| Buff Station | 4× `BuffBubble` |
| Level Portal | `HubAdvancePortal` → `AdvancePortal` |
| Gate / Hazard | `GuaranteedDamageZone`, `SpikeTrap` |
| Tutorial Sign | All `Sign_*`, `HubSign`, `ReturnSign_*` |
| Environment tile | `HubFloor`, `Path_*` |

### C. Keep as Unity UI

- `HealthBar`, `StaminaBar`, `StatsPanel`, `ScoreLabel`
- Entire `GameplayHUD` Canvas tree
- `BuffNotificationOverlay` (screen overlay)

### D. Do not touch yet

- `Player` + `PlayerBuffController` / `PlayerShieldVisual`
- `Level3Systems` runtime wiring in bootstrap
- `LevelManager` goal transition config
- One-off spike child meshes until hazard module pattern is fixed

---

## Stage 3 — Folder structure

Created under `Assets/_Project/` (reuses existing `Prefabs/Shared/`):

```
Prefabs/
├── SceneModules/
│   ├── Interactables/      ← ReturnButton, buttons
│   ├── Stations/           ← generic stations
│   ├── Portals/            ← level / return portals
│   ├── BuffStations/       ← BuffBubble modules
│   ├── ResourceNodes/
│   ├── Buildables/
│   ├── Gates/
│   └── Tutorial/           ← signs, boards
└── Shared/                 ← existing: Labels, VFX, InteractionMarkers

Art/
├── Imported/FBX/SceneModules/
│   ├── Interactables/
│   ├── Stations/
│   ├── Portals/
│   └── Buildables/
└── Placeholder/SceneModules/

Documentation/
└── SceneModules/
    └── Scene_Module_Migration_Plan.md (this file)
```

---

## Stage 4 — Module taxonomy

| # | Category | Level 3 example | Gameplay script |
|---|----------|-----------------|-----------------|
| 1 | Interaction Button | `ReturnButton` | `HubReturnZone` |
| 2 | Energy Shield / Field | `BubbleShell` | (visual only; logic on parent) |
| 3 | Buff Station | `Bubble_+Z 回血` | `BuffBubble` |
| 4 | Level Portal | `AdvancePortal` | `HubAdvancePortal` + `LevelGoal` |
| 5 | Save Point | — | (future) |
| 6 | Resource Node | — | `WorldPickupItem` via ItemModuleFactory |
| 7 | Buildable | — | `PlacedBuilding` / `CraftingStation` |
| 8 | Gate / Barrier | `Trap_*` hazards | `GuaranteedDamageZone`, `SpikeTrap` |
| 9 | Tutorial Sign | `Sign_+Z 回血` | none (visual + label anchor) |

---

## Stage 5 — UI policy (explicit)

- **`HealthBar` is Unity UI** (`RectTransform` under `GameplayHUD`).
- **Do not** replace HealthBar with a Blender world model.
- May extract **`HealthBarUI.prefab`** for HUD reuse.
- Same for **StaminaBar**, inventory, crosshair, buff overlay.

World modules use **`LabelAnchor`** + world-space text (`ItemWorldLabel`, `BillboardLabel`) — not Canvas UI.

---

## Stage 6 — First migration target

**Recommended: `ReturnButton` / `HubReturnZone` module**

Reasons:

1. Simple interaction (trigger + R key)
2. Clear visual (`ReturnButton` cube) separable from logic
3. Four identical copies in Level 3 — proves reuse
4. Low regression risk vs portal or buff systems
5. `HubReturnZone.Create()` is a single factory — easy to redirect to prefab

**Do not migrate `BubbleShell` first** unless ReturnButton audit blocks migration.

---

## Stage 7 — ReturnButton prefab structure (proposed)

```
PF_SceneModule_ReturnButton_01.prefab
├── LogicRoot                    ← scene instance root
│   ├── BoxCollider              ← trigger (HubReturnZone)
│   ├── HubReturnZone              ← interaction + return behavior
│   └── (future: SceneModuleInstance marker)
├── VisualRoot
│   ├── PlaceholderModel_ReturnButton   ← current blue cube (fallback)
│   └── (slot) Future_BlenderModel_ReturnButton
├── LabelAnchor                  ← world hint anchor
└── InteractionPoint             ← optional explicit interact point
```

### Migration rules

1. **Preserve** `HubReturnZone` behavior (R key, hub spawn, trigger size).
2. **Move** `ReturnButton` mesh under `VisualRoot`; strip collider from visual.
3. **Refactor** `HubReturnZone.Create()` → `Instantiate(prefab)` or fallback to current primitive path (same pattern as Level 8 Phase B).
4. **Do not** remove scene objects until prefab path verified in editor + play mode.
5. Later: assign Blender FBX under `VisualRoot` only.

### Code touch points (Phase SM-B only)

| File | Change |
|------|--------|
| `HubReturnZone.cs` | Prefab instantiate + visual fallback in `Create()` |
| New: `SceneModulePrefabRegistry` or Resources asset | Register ReturnButton prefab |
| `Level3BuffHubBuilder.cs` | Optional: call prefab factory instead of inline `Create()` |

---

## Stage 8 — BuffBubble prefab structure (Phase SM-C preview)

```
PF_SceneModule_BuffStation_Heal_01.prefab
├── LogicRoot
│   ├── SphereCollider (trigger)
│   └── BuffBubble
├── VisualRoot
│   ├── BubbleShell    ← Blender target
│   ├── BubbleCore
│   └── BubbleRing
└── (no UI)
```

Variant prefabs or material swaps for Speed / Stamina / Shield.

---

## Migration phases

| Phase | Scope | Gameplay impact |
|-------|--------|-----------------|
| **SM-A** ✅ | Audit + folders + migration plan doc | None |
| **SM-B** ✅ | `HubReturnZone` / ReturnButton prefab + fallback | Low — visual only |
| **SM-C** ✅ | `BuffBubble` Heal + Stamina prefab visuals + fallback | Low — visual only for 2 buff types |
| **SM-D** | Speed / Shield BuffStation catalog entries | Medium |
| **SM-E** | Portal, TutorialSign, hazard shared prefabs | Medium–High |
| **SM-F** | Blender replacement one module at a time | Visual only |

---

## Relationship to other systems

| System | Relationship |
|--------|--------------|
| `ItemModuleFactory` | Global resource/buildable pickups — use for Resource/Buildable modules |
| `LevelModuleCatalog` | Save-driven module IDs — align string ids when prefabs stabilize |
| `Level8 AssetCatalog` | Environment/chunk prefabs — separate from SceneModules |
| `Prefabs/Shared/` | Cross-cutting labels, VFX, interaction markers |

---

## Phase SM-B — ReturnButton (completed)

**Status:** Implemented.

**Pipeline:**

```
HubReturnZone.Awake()
  → EnsureLabelAnchor / EnsureInteractionPoint
  → MigrateLegacyReturnButtonChild (baked scenes)
  → ReturnButtonPrefabCatalog.TryResolveReturnButtonPrefab()
  → SceneModuleVisualUtility.InstantiateModuleVisual (VisualRoot)
  → OR BuildPrimitiveReturnButtonVisual (fallback)
```

**Scripts:** `Scripts/SceneModules/AssetCatalog/*`, `HubReturnZone.cs`

**Test prefab:** `Prefabs/SceneModules/Interactables/PF_Generic_Interactable_ReturnButton_01.prefab`

**Registry:** `Resources/SceneModules/SceneModulePrefabRegistry.asset`

**Menus:**

- `Tools → SceneModules → Create ReturnButton Test Prefab And Registry`
- `Tools → SceneModules → Clear ReturnButton Prefab Registry (Test Fallback)`

**Details:** `ReturnButton_Module_Workflow.md`

**Gameplay unchanged:** BoxCollider trigger size, R-key return, hub spawn, 4 instances.

---

## Phase SM-C — HealStation / StaminaStation (completed)

**Status:** Implemented.

**Scope:** `Bubble_+Z 回血` (Heal) and `Bubble_-Z 无限精力` (InfiniteStamina) only. Speed / Shield still use primitive bubble visuals.

**Pipeline:**

```
BuffBubble.Start / Initialize
  → MigrateLegacyVisualChildren (BubbleShell/Core/Ring on root)
  → EnsureLabelAnchor / EnsureInteractionPoint
  → BuffStationPrefabCatalog.MapBuffTypeToModuleKind
  → SceneModuleVisualResolver.TryResolveSceneModulePrefab
       HealStation / StaminaStation → exact → BuffStation generic → null
  → SceneModuleVisualUtility.InstantiateModuleVisual (VisualRoot)
  → OR BuildPrimitiveBuffStationVisuals (BubbleShell fallback)
```

**Scripts:** `BuffBubble.cs`, `BuffStationPrefabCatalog.cs`, `SceneModuleVisualResolver.cs`, `SceneModuleVisualUtility.cs`

**Test prefabs:**

- `Prefabs/SceneModules/BuffStations/PF_Generic_BuffStation_Heal_01.prefab`
- `Prefabs/SceneModules/BuffStations/PF_Generic_BuffStation_Stamina_01.prefab`

**Registry:** `Resources/SceneModules/SceneModulePrefabRegistry.asset` (HealStation + StaminaStation entries)

**Menus:**

- `Tools → SceneModules → Create BuffStation Test Prefabs And Registry`
- `Tools → SceneModules → Clear BuffStation Prefab Registry (Test Fallback)`

**Details:** `BuffStation_Module_Workflow.md`

**Gameplay unchanged:** SphereCollider trigger, buff collect, bob/spin, one-shot hide, `BuffCollectionTracker`. HealthBar / StaminaBar UI untouched.

**Future Blender:** Replace mesh under prefab `Visual/` child; keep logic on scene `BuffBubble` root.

---

## Testing checklist
- [ ] All 4 buff paths still collect buffs
- [ ] R return works at all 4 return zones
- [ ] +Z damage trap still damages
- [ ] -X spike trap still damages
- [ ] 4 buffs → portal appears → Y → Level 4
- [ ] HealthBar / StaminaBar unchanged
- [ ] Level 3 loads without missing references
- [ ] Heal station prefab visual (green) when registry populated
- [ ] Stamina station prefab visual (blue/cyan) when registry populated
- [ ] Clear BuffStation registry → BubbleShell primitive fallback for Heal/Stamina

---

## Furnace / 熔炉 workstation visual (imported forge.fbx)

**ItemKind:** `ItemKind.Furnace` (display name: 熔炉)

**FBX path:** `Assets/_Project/Art/Imported/FBX/SceneModules/Buildables/forge.fbx`

**Placed prefab:** `Assets/_Project/Prefabs/SceneModules/Stations/PF_Generic_Workstation_Furnace_01.prefab`

**Registry:** `Resources/Items/ItemPlacedPrefabRegistry.asset` (Kind = Furnace)

**Hierarchy (gameplay on prefab root, visual-only FBX under Visual):**

```
PF_Generic_Workstation_Furnace_01
├── CraftingStation / PlacedBuilding / PlacedPickupable / BoxCollider  (root)
├── Visual
│   └── AxisCorrection
│       └── forge  (imported forge.fbx)
└── SelectionOutline
```

**Materials:** `Assets/_Project/Art/Imported/Materials/Workstations/Furnace/` — `Furnace_StoneMaterial.mat`, `Furnace_MetalMaterial.mat` (FBX slots: `stone`, `metal`)

**Menu:** `Tools → Items → Replace Furnace Visual With Imported forge.fbx` (or `Integrate Workstation Furnace Visual`)

**Fallback:** If placed prefab is missing from registry, `ItemModuleFactory` / `PlacedObjectBuilder` still spawn primitive cube visuals; crafting station logic unchanged.

---

## Related documents

- `Documentation/SceneModules/BuffStation_Module_Workflow.md`
- `Documentation/SceneModules/ReturnButton_Module_Workflow.md`
- `Documentation/EnvironmentArt/Environment_Asset_Architecture.md`
- `Documentation/LevelDesign/LEVEL_MODULE_WORKFLOW.md`
