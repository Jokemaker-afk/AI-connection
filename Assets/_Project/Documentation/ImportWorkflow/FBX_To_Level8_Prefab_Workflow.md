# FBX → Level 8 Prefab Workflow

Step-by-step workflow for importing external environment models and making them usable by Level 8's modular generator **without changing generation logic**.

---

## Overview

```
Blender (or other DCC)
  → Export FBX
  → Import to Art/Imported/FBX/
  → Create materials (URP Lit)
  → Build prefab under Prefabs/
  → Add colliders + sockets + Level8ChunkInstance (chunks)
  → Register in AssetCatalog (when wired)
  → Test in Level8 scene
```

---

## Step 1 — Model in DCC (Blender recommended)

### Scale

- **1 Unity unit = 1 meter**
- Player reference: ~1.8 m tall capsule; use ~1×1×1 m as rough prop scale guide

### Transforms

- Apply all transforms before export (Blender: Ctrl+A → All Transforms)
- No negative scale on export if avoidable

### Pivot / origin

| Asset type | Pivot rule |
|------------|------------|
| **Chunk** | Center of 50×50 footprint; ground at **y = 0** |
| **Terrain patch** | Center; bottom at y = 0 |
| **Prop / decoration** | Base center (bottom-center of mesh) |
| **Resource node** | Base center; interaction height ~0.5–1.0 m |
| **Objective** | Base center |

### Chunk footprint

- Default playable area: **50 × 50 m** (matches `Level8GenerationFlags.ChunkSize`)
- Keep walkable geometry inside bounds unless intentional edge overlap for blending
- Forward direction: **+Z** (Unity forward)

### Path connectors (optional)

For path chunks, leave clear edge zones for blending:

- North / South / East / West connector strips (~2–4 m wide)
- Name mesh parts accordingly if useful for art review

### Export settings (FBX)

- Format: FBX 7.4 binary (or Unity-compatible version)
- Apply unit scale: meters
- Forward: -Z Forward, Y Up (Blender default) — verify in Unity import if orientation wrong
- Embed textures only if team prefers; otherwise use separate texture import

---

## Step 2 — Import into Unity

### Destination paths

| Content | Path |
|---------|------|
| Biome environment mesh | `Art/Imported/FBX/Environment/{Biome}/` |
| Decoration source | `Art/Imported/FBX/Props/` |
| Resource source | `Art/Imported/FBX/Resources/` |
| Objective source | `Art/Imported/FBX/Objectives/` |
| Hazard source | `Art/Imported/FBX/Hazards/` |

### FBX import settings (Model tab)

- **Scale Factor:** 1
- **Convert Units:** enabled if DCC uses cm incorrectly
- **Mesh Compression:** Off (environment) or Low (distant props)
- **Read/Write:** Off unless runtime mesh modification needed
- **Generate Colliders:** **Off** — add colliders manually on prefab
- **Normals:** Import or Calculate per asset quality

### Materials

1. Create URP Lit materials under `Art/Imported/Materials/{Category}/`
2. Naming: `M_Forest_Ground`, `M_Desert_Sand`, etc.
3. Assign textures from `Art/Imported/Textures/`
4. Texture naming: `T_Forest_Ground_BaseColor`, `T_Forest_Ground_Normal`, …
5. Drag materials onto FBX sub-meshes or prefab renderers

---

## Step 3 — Create prefab

### Prefab destination

| Type | Path |
|------|------|
| Full chunk | `Prefabs/Environment/Chunks/{Biome}/` |
| Terrain patch | `Prefabs/Environment/TerrainPatches/{Biome}/` |
| Decoration | `Prefabs/Props/Decorations/{Biome}/` |
| Resource | `Prefabs/Props/Resources/` |
| Objective | `Prefabs/Props/Objectives/` or `Prefabs/Level8/ObjectivePrefabs/` |
| Hazard | `Prefabs/Props/Hazards/` |

### Prefab naming

```
PF_Biome_Category_Name_Variant
```

Example: `PF_Forest_Chunk_Resource_01`

---

## Step 4 — Prefab hierarchy (chunks)

```
PF_Forest_Chunk_Resource_01
├── Geometry              # Imported mesh instances
├── Colliders             # Walkable + blocking colliders
├── Decorations           # Optional baked-in static decor
├── Sockets               # Empty transforms for generator placement
│   ├── ResourceSockets
│   ├── HazardSockets
│   ├── DataCoreSockets
│   ├── EnemySpawnSockets
│   ├── ObjectiveSockets
│   └── PathSockets
├── Debug
│   └── Bounds            # Optional wire cube 50×50 for authoring
└── Level8ChunkInstance   # Component: biome, chunkKind, variantId
```

### Socket naming

| Socket | Name pattern |
|--------|--------------|
| Resource | `ResourceSocket_01`, `ResourceSocket_02`, … |
| Hazard | `HazardSocket_01`, … |
| Data core | `DataCoreSocket_01`, … |
| Enemy spawn | `EnemySpawnSocket_01`, … |
| Objective | `ObjectiveSocket_01`, … |
| Path | `PathSocket_North`, `PathSocket_South`, `PathSocket_East`, `PathSocket_West` |

Use `Level8AssetSocket` component on socket transforms (optional tag for category).

### Chunk prefab rules

- Footprint **50 × 50**, pivot center, ground at y = 0, forward +Z
- **Do not include:** Player, GameplayCore, scene managers, progression scripts on prefab root
- **Do include:** ground colliders, art meshes, sockets
- Props: prefer **BoxCollider / CapsuleCollider**; **MeshCollider** only when necessary and preferably non-convex static

---

## Step 5 — Prefab hierarchy (props / resources / objectives)

### Decoration prop (example)

```
PF_Forest_Decoration_Tree_01
├── Geometry
└── (optional BoxCollider for blocking trees)
```

### Resource prop (example — Phase C)

```
PF_Generic_Resource_Stone_01
└── Visual
    ├── RockA
    ├── RockB
    └── RockC
```

**Required spawn pattern (implemented):**

- Prefab is **visual-only** (meshes under `Visual/` child).
- **No** `WorldPickupItem`, inventory logic, or pickup colliders on the prefab.
- `ItemModuleFactory.SpawnWorldPickup` creates the pickup root, attaches gameplay via `PickupItemInteractionSetup`, and instantiates the prefab visual as a **`Visual` child** via `Level8ResourceVisualUtility`.
- Chinese labels come from `Level8ResourcePickupEnhancer` + `ItemKindUtility`, not from the prefab.

**Registration priority:**

1. `ItemCatalog` → `ItemData.WorldPickupPrefab` (preferred long-term)
2. `Level8ResourcePrefabRegistry` → generic or biome+ItemKind entry
3. Primitive fallback via `ItemVisualBuilder` + `Level8ResourcePickupEnhancer`

**Editor setup:** `Tools → Level8 → Create Resource Test Prefabs And Registry`

### Objective prop (example)

```
PF_Generic_Objective_DataCore_01
├── Geometry
├── VFX                # Optional glow child
└── (DataCoreCollectible added at spawn by Level8ObjectiveBuilder)
```

---

## Step 6 — Collider setup

| Surface | Collider |
|---------|----------|
| Walkable ground | BoxCollider or mesh simplified to box; layer: default / ground |
| Blocking rock/tree | Box or capsule |
| Pickup / interact | Trigger sphere or box on interaction layer |
| Hazard zone | Trigger box; sized to gameplay, not just visual |

Remove colliders from pure visual children (imported primitive meshes often ship with default colliders — strip them).

---

## Step 7 — Register in catalog

When catalog wiring is enabled (future phases):

1. Add entry to appropriate catalog stub / ScriptableObject:
   - Chunks → `Level8ChunkPrefabCatalog`
   - Decorations → `Level8BiomePropSetCatalog`
   - Objectives → `Level8ObjectivePrefabCatalog`
   - Resources → `Level8ResourcePrefabCatalog` or `ItemCatalog.WorldPickupPrefab`
   - Hazards → `Level8HazardPrefabCatalog`

2. Set:
   - `biome` / `chunkKind` / `category`
   - `prefab` reference
   - `weight` (for random variant selection)
   - `variantId` string matching file name suffix (`01`, `02`)

3. If prefab missing at runtime → generator **must** fall back to existing primitive builder.

---

## Step 8 — Test in Level 8

### Manual prefab test

1. Open `Scenes/Levels/Level8/Level8.unity`
2. Drag prefab into scene at origin; verify scale, pivot, colliders
3. Compare against 50×50 debug bounds (temporary cube scaled 50,1,50)

### Integration test (when resolver wired)

1. Register prefab in catalog for one biome + ChunkKind
2. Set `Level8GenerationFlags` seed for reproducibility
3. Play scene — confirm prefab appears instead of primitives for that slot
4. Confirm fallback: remove catalog entry → primitives still generate

### Checklist

- [ ] Ground walkable at expected height (y = 0 world after chunk placement)
- [ ] No missing mesh pink materials
- [ ] Colliders not duplicated / not on decorative-only meshes
- [ ] Sockets visible in Scene view (gizmo optional)
- [ ] Objectives still interactable after gameplay component attach
- [ ] Performance: draw calls reasonable per chunk

---

## Common issues

| Problem | Fix |
|---------|-----|
| Model 100× too big/small | Re-export with meters; check FBX Scale Factor |
| Model lying on side | Fix DCC export axis; rotate root on prefab once, apply |
| Chunk off-center | Re-pivot in DCC to footprint center |
| Player falls through | Add ground collider on Colliders child |
| Pink materials | Assign URP Lit materials; check shader pipeline |

---

## Workstation example: Furnace / 熔炉 (forge.fbx)

- **Source:** external import `Assets/_Project/Art/Imported/FBX/SceneModules/Buildables/forge.fbx`
- **Prefab:** `Assets/_Project/Prefabs/SceneModules/Stations/PF_Generic_Workstation_Furnace_01.prefab`
- **Gameplay:** root hosts `CraftingStation`, `PlacedBuilding`, `PlacedPickupable`, `BoxCollider`; FBX is visual-only under `Visual/AxisCorrection/forge`
- **Materials:** centralized URP Lit in `Materials/Workstations/Furnace/` remapped from FBX slots `stone` / `metal`
- **Re-integrate:** `Tools → Items → Replace Furnace Visual With Imported forge.fbx`
- **Fallback:** missing registry/prefab → primitive placed visual; Furnace recipes and `WorkstationKind.Furnace` unchanged

---

## Related documents

- [Environment Asset Architecture](../EnvironmentArt/Environment_Asset_Architecture.md)
- [Level 8 Prefab-Driven Generation Roadmap](../LevelDesign/Level8_PrefabDriven_Generation_Roadmap.md)
