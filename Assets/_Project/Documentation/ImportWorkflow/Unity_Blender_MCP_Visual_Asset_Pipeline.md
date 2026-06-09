# Unity + Blender MCP Visual Asset Pipeline

This project uses Cursor with **Blender MCP** and **Unity MCP**.

The goal is not only to generate 3D models in Blender, but also to **integrate those models into the Unity prefab system safely**.

Every asset pipeline task must handle both sides:

1. **Blender side:** create/prepare visual-only model, export clean FBX, correct scale/pivot/naming/materials
2. **Unity side:** import FBX, find/create prefab, place model under Visual/VisualRoot, preserve gameplay, update Registry, preserve fallback, test

**Cursor rule:** `.cursor/rules/unity-blender-mcp-visual-pipeline.mdc` (`alwaysApply: true`)

---

## Project Layout

```
AI connection/
├── Assets/
├── Packages/
├── ProjectSettings/
├── .cursor/
└── .vscode/

Assets/_Project/
├── Art/              ← FBX, materials, textures, VFX
├── Prefabs/          ← Unity prefabs
├── Scripts/
│   ├── SceneModules/AssetCatalog/   ← SceneModule registry / resolver
│   └── Levels/Level8/AssetCatalog/  ← Level8 catalog
├── Resources/        ← Runtime-loadable registries
└── Scenes/
```

Unity project root: `D:/Unity/Unity files/AI connection`  
Blender workspace: `D:/Blender file/Connection to Unity`

---

## 0. Most Important Principle

**Never break existing gameplay logic for the sake of replacing a visual model.**

Blender / FBX models are **visual-only**.

Unity gameplay logic must stay on the existing Unity root, LogicRoot, scene module system, item system, objective system, or other existing gameplay components.

Do not put gameplay scripts, interaction scripts, colliders, triggers, inventory logic, healing logic, portal logic, objective tracking logic, or runtime state on the FBX visual model.

| Responsibility | Owner |
|----------------|-------|
| LogicRoot | Gameplay |
| VisualRoot | Appearance |
| BlenderModel | Visual-only child under VisualRoot |
| Registry / Catalog / Resolver | Chooses prefab or fallback |
| Primitive fallback | Must remain when prefab missing or registry cleared |

---

## 1. Asset Source Decision

Before creating a model, decide:

- **A.** Build from scratch in Blender
- **B.** Import a free external model and clean it up
- **C.** Hybrid approach

**External models OK for:** rocks, trees, bushes, crates, barrels, ruins, furniture, background props, environmental decorations.

**Build from scratch or heavily customize:** HealStation, ReturnButton, SavePoint, DataCore, SignalRelay, ExitPortal, StaminaStation.

**External asset rules:** prefer CC0; check exact license; record source/creator/license/attribution; no unclear/editorial/non-commercial assets; simplify and restyle; remove cameras/lights/scripts/physics.

**Network use allowed** when license permits (Poly Haven via Blender MCP, Sketchfab with per-model license check, Kenney/Quaternius manual download).

---

## 2. Blender Model Rules

- Low-poly / stylized survival crafting + light sci-fi
- Readable from third-person camera; clear silhouette; lightweight; modular; visual-only
- **Scale:** 1 Blender meter = 1 Unity unit
- **Axes:** X = left/right, Y = front/back, Z = up/down
- **Pivot:** bottom center (resources, interactables, objectives, decorations); center (terrain chunks)
- **Ground:** bottom on Blender Z = 0; height along Blender +Z
- **Front faces Blender -Y** (imports as Unity +Z forward) — see [Blender_Unity_Axis_Alignment_Rule.md](./Blender_Unity_Axis_Alignment_Rule.md)
- **Export:** FBX; Forward **-Z**, Up **Y**, Apply Transform **On**; apply Rotation and Scale before export; no camera/light/gameplay helpers/colliders
- **Unity import:** `bakeAxisConversion: 1`; Visual / FBX child ideally `localRotation (0,0,0)` — do **not** use Unity X = -90 as the default fix for new models
- Simple materials; emission OK for glow; avoid complex shader dependency

---

---

## 3. Directory Selection Rule

Before creating or updating a prefab, classify the asset type and choose the correct FBX and Prefab folder.

### A. SceneModule Interactables

Examples: ReturnButton, HealStation, StaminaStation, TutorialSign, Portal, Hazard, Buildable

| | Path |
|---|------|
| FBX base | `Assets/_Project/Art/Imported/FBX/SceneModules/` |
| FBX subfolders | `Interactables/`, `BuffStations/`, `Stations/` |
| Prefab base | `Assets/_Project/Prefabs/SceneModules/` |
| Prefab subfolders | `Interactables/`, `BuffStations/`, `Stations/`, `Portals/`, `Tutorial/`, `Hazards/`, `Buildables/` |
| Registry scripts | `Assets/_Project/Scripts/SceneModules/AssetCatalog/` |

### B. Level8 Resources

Examples: Wood, Stone, OreFragment, Fiber, Coal, Berry

| | Path |
|---|------|
| FBX | `Assets/_Project/Art/Imported/FBX/Resources/` |
| Prefab | `Assets/_Project/Prefabs/Level8/ResourcePrefabs/` |
| Related | `Scripts/Items/`, `Scripts/Levels/Level8/AssetCatalog/` |

Do not put `WorldPickupItem`, ItemKind, amount, or inventory logic on the FBX model.

### C. Level8 Objectives

Examples: DataCore, SignalRelay, ExitPortal (objective)

| | Path |
|---|------|
| FBX | `Assets/_Project/Art/Imported/FBX/Objectives/` |
| Prefab | `Assets/_Project/Prefabs/Level8/ObjectivePrefabs/` |
| Related | `Scripts/Levels/Level8/AssetCatalog/` |

Do not put `DataCoreCollectible`, ObjectiveTracker, LabelAnchor, or InteractionPoint logic on the FBX.

### D. Props / Decorations

Examples: Tree, RockCluster, GrassCluster, Ruins, Barrels, Crates

| | Path |
|---|------|
| FBX | `Art/Imported/FBX/Environment/` or `Props/` |
| Prefab | `Prefabs/Props/Decorations/` or `Prefabs/Environment/` |

### E. Environment / Chunks

Examples: Forest_Chunk_Path_01, Plain_Chunk_Resource_01

| | Path |
|---|------|
| FBX | `Art/Imported/FBX/Environment/` |
| Prefab | `Prefabs/Environment/Chunks/` or `Prefabs/Level8/ChunkPrefabs/` |

Do not start complex chunks unless explicitly requested.

---

## 4. Search Rule (Before Creating Anything)

For every new asset, search the real project structure:

1. Existing prefab by expected name (`PF_Generic_*`)
2. Likely prefab folders: `Prefabs/SceneModules/`, `Prefabs/Level8/`, `Prefabs/Props/`, `Prefabs/Environment/`
3. Matching FBX under `Art/Imported/FBX/`
4. Registry/Catalog: `Scripts/SceneModules/AssetCatalog/`, `Scripts/Levels/Level8/AssetCatalog/`, `ScriptableObjects/`, `Resources/`
5. Logic scripts: `Scripts/SceneModules/`, `Scripts/Levels/`, `Scripts/Items/`, `Scripts/UI/`, `Scripts/Core/`

**Do not create duplicate systems** if an existing prefab/registry already handles replacement.

---

## 5. Prefab Update Rule

If prefab exists:

- Do not delete prefab root or rebuild from scratch
- Do not remove scripts, colliders, triggers, interaction points, label anchors, serialized refs
- Find/create Visual/VisualRoot only if needed
- Remove placeholder visual only when safe
- Place FBX under Visual/VisualRoot
- Save prefab; report exactly what changed

---

## 6. Prefab Creation Rule

If prefab does not exist:

- Create in correct folder; root name starts with `PF_`
- Add Visual child; place FBX under Visual
- Visual-only unless architecture explicitly requires logic prefab
- Register via existing Registry/Catalog/Resolver — no hardcoded scene refs

---

## 7. Fallback Rule

Primitive fallback must always remain (`ItemVisualBuilder`, `SceneModuleVisualUtility`, registry fallback, primitive test models).

Test both: **prefab mode** and **fallback mode** (registry cleared).

---

## 8. HealStation Directory Rule

**Preferred naming (new work):**

| | Path |
|---|------|
| FBX | `Art/Imported/FBX/SceneModules/BuffStations/Generic_Interactable_HealStation_01.fbx` |
| Prefab | `Prefabs/SceneModules/BuffStations/PF_Generic_Interactable_HealStation_01.prefab` |

**Current project reality (search first):**

| | Path |
|---|------|
| Registry prefab | `PF_Generic_BuffStation_Heal_01.prefab` → `SceneModuleKind.HealStation` |
| FBX (current) | `Art/Imported/FBX/SceneModules/Interactables/Generic_Interactable_HealStation_01.fbx` |

When integrating: update Visual on the **registry-linked prefab**; migrate FBX to `BuffStations/` only if done together with registry/path updates.

**Do not touch:** HealthBar, StaminaBar, ReturnButton, StaminaStation (unless requested), BuffBubble/heal logic on FBX.

**Test:** Level3 heal buff + fallback when registry cleared.

Expected Visual hierarchy under prefab:

```
PF_* 
└── Visual
    └── Generic_Interactable_HealStation_01
        ├── StationBase, HealPad, GlowCore, MedicalIcon
        ├── SidePanel_L, SidePanel_R
        └── Optional_BackColumn, Optional_GlowRing
```

---

## 9. Standard Prefab Responsibility Split

See **[Unified_Gameplay_Prefab_Rule.md](./Unified_Gameplay_Prefab_Rule.md)** for the full decision rule.

**Summary:** FBX = visual-only always. Unity prefab = complete gameplay prefab when backend logic already exists.

```
PF_TargetName
├── LogicRoot or Root
│   ├── Collider / Trigger
│   ├── Gameplay Script
│   └── Runtime State
├── Visual or VisualRoot
│   └── AxisCorrection
│       └── BlenderModel_TargetName
├── WarningArea (hazards, when used)
├── LabelAnchor
└── InteractionPoint
```

If project uses different structure, **preserve it** — only replace visual model safely.

**Complete module example:** `PF_Generic_Hazard_ElectricField_01` + `HazardElectricFieldPrefabCatalog` + `LaserHazard.Create()`.

**Visual-only registry example:** `PF_Generic_Hazard_SpikeTrap_01` resolved at runtime by `SpikeTrap.Create()`.

---

## 9b. Unified Gameplay Prefab Decision (Quick)

| Backend exists? | Prefab type | Registry |
|-----------------|-------------|----------|
| Yes | Complete: LogicRoot + Visual + scripts + collider | Module kind (e.g. `ElectricFieldHazard`) |
| No | Visual-only under `Visual/AxisCorrection/FBX` | Visual kind only; report missing backend |
| Partial | Preserve existing; visual prefab; report gaps | Do not invent logic |

---

## 10. Path Rules (Quick Reference)

### FBX import paths

| Category | Path |
|----------|------|
| Resources | `Assets/_Project/Art/Imported/FBX/Resources/` |
| Objectives | `Assets/_Project/Art/Imported/FBX/Objectives/` |
| Scene modules | `Assets/_Project/Art/Imported/FBX/SceneModules/` |
| Interactables | `Assets/_Project/Art/Imported/FBX/SceneModules/Interactables/` |
| Buff stations | `Assets/_Project/Art/Imported/FBX/SceneModules/BuffStations/` |
| Environment | `Assets/_Project/Art/Imported/FBX/Environment/` |
| Props | `Assets/_Project/Art/Imported/FBX/Props/` |

### Prefab paths

| Category | Path |
|----------|------|
| Resources | `Assets/_Project/Prefabs/Level8/ResourcePrefabs/` |
| Objectives | `Assets/_Project/Prefabs/Level8/ObjectivePrefabs/` |
| Interactables | `Assets/_Project/Prefabs/SceneModules/Interactables/` |
| Buff stations | `Assets/_Project/Prefabs/SceneModules/BuffStations/` |
| Portals | `Assets/_Project/Prefabs/SceneModules/Portals/` |
| Tutorial | `Assets/_Project/Prefabs/SceneModules/Tutorial/` |
| Hazards | `Assets/_Project/Prefabs/SceneModules/Hazards/` |
| Buildables | `Assets/_Project/Prefabs/SceneModules/Buildables/` |

---

## 11. Naming Rules

**FBX examples:** `Generic_Resource_Wood_01.fbx`, `Generic_Interactable_ReturnButton_01.fbx`, `Generic_Interactable_HealStation_01.fbx`, `Generic_Objective_DataCore_01.fbx`

**Prefab examples:** `PF_Generic_Resource_Wood_01.prefab`, `PF_Generic_Interactable_ReturnButton_01.prefab`, `PF_Generic_BuffStation_Heal_01.prefab`

---

## 12. Type-Specific Rules

### A. Resource Pickup

- LogicRoot keeps `WorldPickupItem`
- FBX = visual only
- Prefer `ItemCatalog.ItemData.WorldPickupPrefab` / `Level8ResourcePrefabRegistry`
- Fallback: `ItemVisualBuilder` / primitive

### B. Objective

- Logic on `DataCoreCollectible` / `SignalRelayInteractable` / ExitPortal root
- FBX under VisualRoot only
- Fallback: primitive

### C. Scene Module

- LogicRoot keeps original script (`HubReturnZone`, `BuffBubble`, etc.)
- Visual prefab must not contain gameplay scripts
- Fallback: primitive builders in logic scripts

### D. UI

- **Never** use Blender model replacement for UI
- Keep Unity UI / RectTransform

---

## 13. Registry / Catalog Rules

| System | Components |
|--------|------------|
| Level8 Resource | `Level8ResourcePrefabRegistry`, `ItemCatalog.WorldPickupPrefab` |
| Level8 Objective | `Level8ObjectivePrefabRegistry` |
| Scene Module | `SceneModulePrefabRegistry`, `SceneModuleVisualResolver`, `SceneModuleVisualUtility` |

Update matching entry when integrating. Do not hardcode one-off scene references.

---

## 14. Cursor Operation Order

1. Identify asset type
2. Source decision (scratch / external / hybrid)
3. Generate Blender model
4. Export FBX to correct path
5. Search Unity prefab
6. Search registry / catalog
7. Search logic scripts / visual builders
8. Update Visual (if prefab exists) or create prefab
9. Place FBX under Visual
10. Ensure visual-only
11. Register prefab
12. Confirm primitive fallback
13. Open test scene
14. Verify visual + gameplay
15. Report changed files and test status

---

## 15. Required Cursor Status Report

At the end of every asset pipeline task, report:

- Asset type
- **Backend logic exists (yes/no)**
- **Prefab type: complete gameplay vs visual-only**
- FBX path used
- Prefab path used
- Prefab found or created
- Visual / VisualRoot updated (yes/no)
- Logic scripts preserved (yes/no)
- Collider / Trigger / DetectionArea preserved (yes/no)
- Registry / Catalog / Resolver updated (yes/no)
- Fallback still exists (yes/no)
- Scene tested
- Gameplay behavior tested
- Files changed
- **Unresolved missing backend logic (if any)**

---

## 16. Per-Asset Prompt Output Format

When requesting a new asset, include:

1. **Target** — names, paths, type, scene/system
2. **Blender requirements** — design, size, pivot, hierarchy, materials, export
3. **Unity current system** — logic script, visual builder, registry, fallback
4. **Unity required action** — prefab update/create, registry, preserve fallback
5. **Logic preservation** — scripts/colliders/UI that must not change
6. **Recommended prefab structure**
7. **Test cases**
8. **Final status report** from Cursor

---

## 17. Safety Rules

**Do not:**

- Rewrite gameplay logic to replace a model
- Delete prefab root / Collider / Trigger / LogicRoot
- Add gameplay scripts to FBX visual
- Add `WorldPickupItem`, `BuffBubble`, `HubReturnZone`, `ObjectiveTracker` to visual model
- Touch unrelated systems or UI bars
- Change ReturnButton when working on HealStation (unless requested)
- Remove primitive fallback

**If unsure:** preserve logic, update Visual only, report uncertainty.

---

## Related Documents

- [Unified_Gameplay_Prefab_Rule.md](./Unified_Gameplay_Prefab_Rule.md) — **complete gameplay prefab vs visual-only FBX**
- [Blender_Unity_Axis_Alignment_Rule.md](./Blender_Unity_Axis_Alignment_Rule.md) — **Blender -Y = Unity +Z**
- [Unity_Imported_Material_Placement_Rule.md](./Unity_Imported_Material_Placement_Rule.md) — **FBX / Materials folder separation**
- [Unity_Blender_Asset_Source_Decision_Rule.md](./Unity_Blender_Asset_Source_Decision_Rule.md) — asset source & licensing (superseded sections merged here)
- [ReturnButton_Module_Workflow.md](../SceneModules/ReturnButton_Module_Workflow.md)
- [BuffStation_Module_Workflow.md](../SceneModules/BuffStation_Module_Workflow.md)
- [FBX_To_Level8_Prefab_Workflow.md](./FBX_To_Level8_Prefab_Workflow.md)

---

## Connection to Unity Workspace

Blender working directory: `D:/Blender file/Connection to Unity`

Save `.blend`, builder `.py`, and local FBX copies there; export final FBX to Unity project import paths above.

Unity project root: `D:/Unity/Unity files/AI connection`
