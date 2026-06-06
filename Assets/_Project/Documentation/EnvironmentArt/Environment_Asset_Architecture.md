# Environment Asset Architecture

This document defines how first-party environment art is organized for **Level 8** and future modular levels. **Generation logic stays in scripts**; **visuals move from primitives to prefabs** over time.

---

## Design principle: logic vs art

| Layer | Responsibility | Location |
|-------|----------------|----------|
| **Generation logic** | Seed, layout, biome, ChunkKind, objectives, resources, hazards | `Scripts/Levels/Level8/` — e.g. `Level8SeededLayoutBuilder`, `Level8ChunkLayout`, `Level8BiomeProfile`, `Level8ObjectiveBuilder`, `Level8BiomeResourcePlacer`, `Level8HazardZoneBuilder` |
| **Visual asset layer** | Meshes, materials, prefabs, prop sets | `Art/`, `Prefabs/`, `ScriptableObjects/`, `Scripts/Levels/Level8/AssetCatalog/` |

**Resolver flow (target state):**

```
BiomeKind + ChunkKind + Seed
  → chunk prefab / terrain patch / decoration set
  → Instantiate prefab
  → read sockets → place resources / objectives / hazards
  → if no prefab → fallback to existing primitive builders
```

Existing placeholder builders (`Level8TerrainVisualBuilder`, `Level8ChunkPlaceholderBuilder`, `DataCoreCollectible.BuildVisuals()`, etc.) **remain as fallback** and are not removed.

---

## Folder structure

```
Assets/_Project/
├── Art/
│   ├── Imported/
│   │   ├── FBX/
│   │   │   ├── Environment/     # Per-biome chunk & terrain source meshes
│   │   │   │   ├── Plain/
│   │   │   │   ├── Desert/
│   │   │   │   ├── Forest/
│   │   │   │   ├── Swamp/
│   │   │   │   ├── Hill/
│   │   │   │   ├── MountainEdge/
│   │   │   │   └── DataWilderness/
│   │   │   ├── Props/
│   │   │   ├── Resources/
│   │   │   ├── Objectives/
│   │   │   └── Hazards/
│   │   ├── Textures/
│   │   │   ├── Environment/
│   │   │   ├── Props/
│   │   │   └── Shared/
│   │   └── Materials/
│   │       ├── Environment/
│   │       ├── Props/
│   │       ├── Resources/
│   │       ├── Objectives/
│   │       └── Hazards/
│   ├── Placeholder/             # Optional exported primitive stand-ins
│   │   ├── Environment/
│   │   ├── Props/
│   │   ├── Resources/
│   │   ├── Objectives/
│   │   └── Hazards/
│   └── VFX/
│       ├── ObjectiveGlow/
│       ├── Hazard/
│       └── Ambient/
│
├── Prefabs/
│   ├── Environment/
│   │   ├── Chunks/              # Full 50×50 chunk prefabs by biome
│   │   ├── TerrainPatches/      # Ground / path / hill patches
│   │   ├── WallsAndCliffs/
│   │   ├── Boundaries/
│   │   └── Connectors/
│   ├── Props/
│   │   ├── Decorations/
│   │   ├── Resources/
│   │   ├── Hazards/
│   │   └── Objectives/
│   ├── Level8/                  # Level-8 curated aliases
│   └── Shared/
│
├── ScriptableObjects/
│   ├── LevelGeneration/
│   └── Environment/
│
└── Documentation/
    ├── EnvironmentArt/          # This file
    ├── ImportWorkflow/
    └── LevelDesign/
```

**Reuse rule:** if a folder already exists (e.g. `Prefabs/Enemies/`), extend it — do not duplicate.

---

## Asset categories

### Category A — Terrain / Ground

Used for chunk ground surface, paths, hills, biome-specific ground patches.

| Example asset name | Use |
|--------------------|-----|
| `Plain_Terrain_Grass_01` | Plain ground patch |
| `Desert_Terrain_Sand_01` | Desert sand patch |
| `Forest_Terrain_Ground_01` | Forest floor |
| `Hill_RaisedGround_01` | Raised terrain mound |
| `Swamp_Terrain_Mud_01` | Swamp mud (later) |

**Prefab path:** `Prefabs/Environment/TerrainPatches/{Biome}/`

**Current primitive equivalent:** `Level8TerrainVisualBuilder`, ground plates in `Level8ChunkPlaceholderBuilder`

---

### Category B — Walls / Cliffs / Boundaries

Map edges, cliff faces, chunk edge dressing, mountain walls.

| Example | Use |
|---------|-----|
| `Desert_RockWall_01` | Desert boundary |
| `Forest_TreeWall_01` | Forest edge |
| `Mountain_Cliff_01` | Mountain edge biome |
| `Generic_BoundaryRock_01` | Fallback boundary |

**Prefab path:** `Prefabs/Environment/WallsAndCliffs/`, `Prefabs/Environment/Boundaries/`

**Current primitive equivalent:** `Level8MapUtility`, debug borders in `Level8TerrainVisualBuilder`

---

### Category C — Decorations

Visual atmosphere only — no gameplay unless explicitly tagged.

| Example | Use |
|---------|-----|
| `Plain_Decoration_GrassCluster_01` | Plain grass |
| `Desert_Decoration_Cactus_01` | Desert prop |
| `Forest_Decoration_Tree_01` | Forest tree |
| `Forest_Decoration_FallenLog_01` | Forest detail |
| `Generic_Decoration_RockCluster_01` | Cross-biome rocks |
| `DataWilderness_Decoration_CorruptionLine_01` | Data wilderness flavor |

**Prefab path:** `Prefabs/Props/Decorations/{Biome}/`

**Current primitive equivalent:** `Level8BiomeDecorationBuilder`

---

### Category D — Resources

Collectible / harvestable world nodes. Must link to item pipeline.

| Example | Item pipeline |
|---------|---------------|
| `Generic_Resource_WoodLog_01` | `ItemModuleFactory`, `WorldPickupItem`, `ItemCatalog.WorldPickupPrefab` |
| `Generic_Resource_StoneNode_01` | Same + optional `ToolInteractable` |
| `Generic_Resource_OreNode_01` | Same |
| `Generic_Resource_FiberPlant_01` | Same |
| `Generic_Resource_BerryBush_01` | Same |

**Prefab path:** `Prefabs/Props/Resources/`, `Prefabs/Level8/ResourcePrefabs/`

**Level 8 enhancer:** `Level8ResourcePickupEnhancer`, `Level8VisualHierarchyUtility.ApplyResourceCategoryVisual`

**Current primitive equivalent:** `ItemVisualBuilder` cube fallback + Level 8 category primitives

---

### Category E — Objectives

Major gameplay targets.

| Example | Gameplay component |
|---------|------------------|
| `Generic_Objective_DataCore_01` | `DataCoreCollectible` |
| `Generic_Objective_SignalRelay_01` | `SignalRelayInteractable` |
| `Generic_Objective_LevelExitPortal_01` | `Level8ExitPortal` |
| `Generic_Objective_SavePoint_01` | Future save system |
| `Generic_Objective_ResearchTerminal_01` | Future tech tree |

**Prefab path:** `Prefabs/Props/Objectives/`, `Prefabs/Level8/ObjectivePrefabs/`

**Current primitive equivalent:** `BuildVisuals()` in each objective script

---

### Category F — Hazards

Danger areas — visual + trigger colliders.

| Example | Gameplay hook |
|---------|---------------|
| `Hazard_CorruptionZone_01` | `Level8HazardZoneBuilder`, future `DamageZone` / `SlowZone` |
| `Hazard_DesertHeatPatch_01` | Biome hazard profile |
| `Hazard_SwampMudPool_01` | Swamp biome |
| `Hazard_ThornPatch_01` | Forest / plain |

**Prefab path:** `Prefabs/Props/Hazards/`, `Prefabs/Level8/HazardPrefabs/`

**Current primitive equivalent:** `Level8HazardZoneBuilder`, `Level8BiomeDecorationBuilder.BuildHazardVisuals`

---

## Naming convention

### Environment chunks (FBX source)

```
Biome_ChunkKind_Variant
```

Examples: `Plain_Path_01`, `Forest_Resource_01`, `Desert_Hazard_01`, `Plain_FinalObjective_01`

### Terrain patches (FBX source)

```
Biome_Terrain_Type_Variant
```

Examples: `Forest_Terrain_Ground_01`, `Desert_Terrain_Sand_01`, `Plain_Terrain_Grass_01`

### Props (FBX source)

```
BiomeOrGeneric_Category_Name_Variant
```

Examples: `Forest_Decoration_Tree_01`, `Desert_Decoration_Cactus_01`, `Generic_Resource_StoneNode_01`

### Prefabs (Unity)

```
PF_Biome_Category_Name_Variant
```

Examples: `PF_Forest_Chunk_Path_01`, `PF_Desert_Decoration_Cactus_01`, `PF_Generic_Objective_DataCore_01`

### Materials

```
M_Biome_Surface_Name
```

Examples: `M_Forest_Ground`, `M_Desert_Sand`, `M_Generic_Rock`

### Textures

```
T_Biome_Surface_Name_Type
```

Examples: `T_Forest_Ground_BaseColor`, `T_Forest_Ground_Normal`, `T_Desert_Sand_BaseColor`

---

## FBX vs Material vs Prefab vs Profile

| Artifact | What it is | Where it lives |
|----------|------------|----------------|
| **FBX** | Source mesh from Blender/etc. | `Art/Imported/FBX/...` |
| **Material** | URP Lit (or custom) shader instance | `Art/Imported/Materials/...` |
| **Prefab** | Scene-ready GameObject with mesh, colliders, sockets, optional `Level8ChunkInstance` | `Prefabs/...` |
| **Profile / Catalog entry** | Maps Biome + ChunkKind + Category → prefab + spawn rules | Code: `Scripts/Levels/Level8/AssetCatalog/`; later: `ScriptableObjects/` |

**Pipeline:** FBX → assign materials → build prefab hierarchy → register in catalog → generator resolves at runtime.

---

## Biome prop set system (direction)

Each biome owns a **prop set** — weighted lists of prefabs per category.

### Data types (stubbed in code)

| Type | Purpose |
|------|---------|
| `Level8PropCategory` | GroundPatch, Wall, Tree, Rock, Resource, Hazard, Objective, … |
| `Level8PropSpawnRule` | Prefab, weight, scale range, count range, chunk filters, spacing rules |
| `Level8BiomePropSet` | Collection of rules for one biome |
| `Level8EnvironmentPropProfile` | Optional per-prop metadata (pivot, bounds, socket offsets) |

### Planned biome sets

| Set | Status |
|-----|--------|
| `PlainPropSet` | Initial target |
| `DesertPropSet` | Initial target (`Level8BiomeKind` may add `Desert` when art lands) |
| `ForestPropSet` | Initial target |
| `GenericPropSet` | Cross-biome fallback |
| Swamp, Hill, MountainEdge, DataWilderness | Later packs |

### Prop entry fields

- `GameObject prefab`
- `weight`, `minScale`, `maxScale`
- `minCount`, `maxCount`
- `allowedChunkKinds`
- `avoidNearObjective`
- `minDistanceBetweenSameType`
- `alignToGround`, `randomYaw`, `canOverlapPath`

---

## Placeholder fallback

**Never delete primitive builders.** Replacement pattern:

```csharp
if (Level8VisualAssetResolver.TryResolveChunkPrefab(biome, chunkKind, seed, out GameObject prefab))
{
    Instantiate(prefab, position, rotation, parent);
}
else
{
    Level8ChunkPlaceholderBuilder.CreateChunk(/* existing args */);
}
```

Same pattern for objectives, resources, hazards, decorations.

Catalog lookup order:

1. Exact biome + category + chunk kind
2. Generic biome fallback
3. Placeholder primitive builder

---

## Future catalogs

| Catalog | Lookup keys |
|---------|-------------|
| `Level8ChunkPrefabCatalog` | BiomeKind, ChunkKind, variant index / seed |
| `Level8BiomePropSetCatalog` | BiomeKind → prop set |
| `Level8ObjectivePrefabCatalog` | Objective type (DataCore, SignalRelay, Exit) |
| `Level8ResourcePrefabCatalog` | ItemKind |
| `Level8HazardPrefabCatalog` | HazardKind / biome |

Stub implementations: `Scripts/Levels/Level8/AssetCatalog/` (not wired to generators yet).

---

## Chunk constants (match code)

- **Chunk footprint:** 50 × 50 units (`Level8GenerationFlags.ChunkSize`)
- **Pivot:** chunk center
- **Forward:** +Z
- **Ground surface:** y = 0 (local)
- **Chunk kinds:** `Spawn`, `Path`, `Resource`, `Hazard`, `DataCore`, `FinalObjective`

---

## Related documents

- [FBX → Level 8 Prefab Workflow](../ImportWorkflow/FBX_To_Level8_Prefab_Workflow.md)
- [Level 8 Prefab-Driven Generation Roadmap](../LevelDesign/Level8_PrefabDriven_Generation_Roadmap.md)
- [Level 8 Semi-Random Generation Plan](../LevelDesign/Level8_SemiRandom_Exploration_Generation_Plan.md)
