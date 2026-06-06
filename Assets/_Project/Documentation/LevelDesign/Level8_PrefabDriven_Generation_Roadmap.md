# Level 8 Prefab-Driven Generation Roadmap

Migration plan from **script-generated primitives** to **imported FBX / Unity prefab** modular environment art. **Random generation logic is unchanged** throughout all phases.

---

## Current state (primitives)

Level 8 builds the playable map at runtime via C# builders:

| System | Script | Visual method |
|--------|--------|---------------|
| Layout | `Level8SeededLayoutBuilder`, `Level8LayoutGenerator` | N/A (logic only) |
| Arena assembly | `Level8ArenaBuilder` | Orchestrates all visual builders |
| Terrain | `Level8TerrainVisualBuilder` | `CreatePrimitive` cubes/cylinders |
| Chunks | `Level8ChunkPlaceholderBuilder` | Per-ChunkKind primitive layouts |
| Decorations | `Level8BiomeDecorationBuilder` | Trees, rocks, ruins as primitives |
| Resources | `Level8BiomeResourcePlacer` + `ItemVisualBuilder` | Cubes or ItemCatalog prefab |
| Objectives | `Level8ObjectiveBuilder` + components | `BuildVisuals()` primitives |
| Data core | `DataCoreCollectible.BuildVisuals()` | Sphere + cylinder |
| Signal relay | `SignalRelayInteractable.BuildVisuals()` | Cylinder tower |
| Exit portal | `Level8ExitPortal.BuildVisuals()` | Cylinder arch |
| Hazards | `Level8HazardZoneBuilder` | Cube zone + posts |
| Bootstrap | `GameplayFoundationBootstrap.EnsureLevel8Content()` | Triggers generation on load |

**Strengths:** fast iteration, seed/biome/chunk logic proven, full fallback always available.

**Limitation:** map reads as placeholder; art direction needs prefab layer.

---

## Target state (prefab-driven visuals)

```
Step 1 — Generator decides layout (UNCHANGED)
  seed, biome, chunk grid, ChunkKind, objective positions, resource rules

Step 2 — Visual resolver chooses assets (NEW)
  Level8VisualAssetResolver + catalogs
  BiomeKind + ChunkKind + PropCategory + seed → prefab variant

Step 3 — Builder instantiates (GRADUAL REPLACEMENT)
  Instantiate chunk prefab OR Level8ChunkPlaceholderBuilder fallback
  Spawn props from biome prop set OR Level8BiomeDecorationBuilder fallback
  Place resources/objectives/hazards at sockets OR legacy positions

Step 4 — Interaction layer attaches gameplay (MOSTLY UNCHANGED)
  DataCoreCollectible, SignalRelayInteractable, WorldPickupItem,
  ToolInteractable, hazard triggers, Level8ExitPortal

Step 5 — Validation
  Objectives reachable, missing prefabs → fallback, audit log
```

### Separation contract

| Layer | Keeps |
|-------|-------|
| Logic | `Level8ChunkLayout`, `Level8BiomeProfile`, placement rules, seed |
| Visual | Prefabs, materials, prop sets, catalogs |
| Gameplay | Existing interactable components attached after visual spawn |

---

## Phased migration plan

### Phase A — Folder structure & documentation ✅ (this pass)

**Deliverables:**

- `Art/`, `Prefabs/Environment/`, `Prefabs/Level8/`, `ScriptableObjects/` tree
- `Environment_Asset_Architecture.md`
- `FBX_To_Level8_Prefab_Workflow.md`
- This roadmap
- Catalog stubs under `Scripts/Levels/Level8/AssetCatalog/` (not wired)

**No changes** to `Level8ArenaBuilder` or placeholder builders.

---

### Phase B — One Data Core prefab replacement ✅

**Status:** Implemented and verified.

- `PF_Generic_Objective_DataCore_01` + optional `PF_DataWilderness_Objective_DataCore_01`
- `Level8ObjectivePrefabRegistry` (Resources/Level8/)
- `DataCoreCollectible.BuildVisuals()` prefab branch + primitive fallback
- Editor: `Tools → Level8 → Create Data Core Test Prefab And Registry` / `Clear Data Core Prefab Registry`

**Success criteria:** prefab visible when registered; primitives when catalog empty. ✅

---

### Phase C — Resource pickup prefab replacement ✅

**Status:** Implemented.

**Visual priority:**

1. `ItemData.WorldPickupPrefab` (ItemCatalog — long-term primary)
2. `Level8ResourcePrefabRegistry` (biome + ItemKind, then generic ItemKind)
3. `ItemVisualBuilder` cube + `Level8ResourcePickupEnhancer` category primitives

**Pipeline:**

```
Level8BiomeResourcePlacer
  → ItemModuleFactory.SpawnWorldPickup
  → ItemPickupVisualResolver.TryResolve(kind, biome)
  → Level8ResourceVisualUtility.InstantiatePickupVisual (visual child only)
  → ConfigureWorldPickup (WorldPickupItem on root)
  → Level8ResourcePickupEnhancer (skips primitive overlay when prefab resolved)
```

**Test prefabs registered (via editor menu):**

- `PF_Generic_Resource_Wood_01`
- `PF_Generic_Resource_Stone_01`
- `PF_Generic_Resource_OreFragment_01`

**Registry:** `Resources/Level8/Level8ResourcePrefabRegistry.asset`

**Editor menus:**

- `Tools → Level8 → Create Resource Test Prefabs And Registry`
- `Tools → Level8 → Clear Resource Prefab Registry (Test Fallback)`

**Replace test prefab with FBX later:**

1. Import FBX to `Art/Imported/FBX/Resources/`
2. Build visual-only prefab under `Prefabs/Level8/ResourcePrefabs/` or assign to `ItemCatalog.WorldPickupPrefab`
3. Re-run create menu or assign in registry asset
4. Prefab must not include `WorldPickupItem` — gameplay stays on pickup root

**Success criteria:** pickup, inventory, labels, biome weights unchanged. ✅

---

### Phase D — One chunk prefab replacement

**Goal:** Full 50×50 chunk prefab for one Biome + ChunkKind (e.g. `Forest` + `Path`).

1. Author `PF_Forest_Chunk_Path_01` with hierarchy + sockets + `Level8ChunkInstance`
2. Register in `Level8ChunkPrefabCatalog`
3. In `Level8ArenaBuilder` or `Level8ChunkPlaceholderBuilder`:

```csharp
if (Level8ChunkPrefabCatalog.TryResolve(biome, chunkKind, seed, out GameObject chunkPrefab))
    SpawnChunkPrefab(chunkPrefab, slot);
else
    CreateChunk(/* existing primitive path */);
```

4. Map generator still decides **where** and **what kind** — only visual shell swaps

**Success criteria:** one chunk type renders as prefab; neighbors still primitive OK.

---

### Phase E — Biome prop sets

**Goal:** Weighted decoration from data instead of hard-coded primitives.

1. Define `Level8BiomePropSet` assets for Plain, Desert, Forest
2. Implement `Level8BiomePropSetCatalog` weighted pick by seed
3. `Level8BiomeDecorationBuilder.DecorateChunkInterior`:
   - Try prop set spawn rules
   - Fallback to current primitive decoration methods

**Fields per rule:** prefab, weight, scale range, count, allowed ChunkKinds, spacing, alignToGround, randomYaw

**Success criteria:** Forest chunks spawn tree prefabs with deterministic seed.

---

### Phase F — Full biome asset packs

**Goal:** Complete visual pass per biome.

| Biome pack | Includes |
|------------|----------|
| Plain / 平原 | Ground, grass decor, boundaries |
| Desert / 沙漠 | Sand terrain, cactus, rock walls *(add `Desert` to `Level8BiomeKind` when ready)* |
| Forest / 森林 | Ground, trees, logs, tree walls |
| Later: Swamp, Hill, MountainEdge, DataWilderness | Incremental packs |

Per pack:

- Terrain patches
- Chunk variants per ChunkKind (at least 2 variants for variety)
- Decoration prop set
- Resource & hazard prefabs where biome-specific
- Boundary / cliff pieces

**Success criteria:** entire run uses prefabs for selected biome with zero logic regressions.

---

## Catalog roadmap

| Phase | Catalog | Wired to |
|-------|---------|----------|
| B | `Level8ObjectivePrefabCatalog` | DataCore, SignalRelay, Exit |
| C | `ItemCatalog` + `Level8ResourcePrefabCatalog` | Resource spawner |
| D | `Level8ChunkPrefabCatalog` | Chunk builder |
| E | `Level8BiomePropSetCatalog` | Decoration builder |
| F | All + `Level8HazardPrefabCatalog` | Hazard builder |

Later: migrate code catalogs to `ScriptableObjects/LevelGeneration/`.

---

## Files to touch per phase (reference)

| Phase | Scripts (visual layer only) |
|-------|----------------------------|
| B | `DataCoreCollectible.cs`, `SignalRelayInteractable.cs`, `Level8ExitPortal.cs` |
| C | `ItemCatalog.cs`, `Level8ResourcePickupEnhancer.cs` |
| D | `Level8ChunkPlaceholderBuilder.cs` or `Level8ArenaBuilder.cs` |
| E | `Level8BiomeDecorationBuilder.cs` |
| F | `Level8TerrainVisualBuilder.cs`, `Level8HazardZoneBuilder.cs`, `Level8MapUtility.cs` |

**Do not modify:** `Level8LayoutGenerator`, `Level8SeededLayoutBuilder`, objective placement rules, seed semantics.

---

## Validation checklist (every phase)

- [ ] Same seed → same layout (chunk kinds & objective positions)
- [ ] Missing prefab → primitive fallback (no null refs, no empty chunks)
- [ ] All 3 data cores collectible
- [ ] Signal relay activates after 3 cores
- [ ] Exit portal loads Level 9 or safe warning
- [ ] Resources pickup into inventory
- [ ] `Level8ObjectAudit` log still meaningful
- [ ] No new hard dependency on third-party map generators

---

## Initial biome priority

1. **Plain / 平原** — simplest ground + grass (maps to existing `Level8BiomeKind.Plain`)
2. **Forest / 森林** — trees, logs (`Level8BiomeKind.Forest`)
3. **Desert / 沙漠** — new biome enum + profile when art pack starts

Existing code biomes (Grassland, Hill, Swamp, …) continue using primitives until packs exist.

---

## Related documents

- [Environment Asset Architecture](../EnvironmentArt/Environment_Asset_Architecture.md)
- [FBX → Level 8 Prefab Workflow](../ImportWorkflow/FBX_To_Level8_Prefab_Workflow.md)
- [Level 8 Semi-Random Exploration Plan](./Level8_SemiRandom_Exploration_Generation_Plan.md)
