# Generic_Resource Batch 01 — Asset Source

| Asset | FBX | Prefab |
|-------|-----|--------|
| Wood | `Art/Imported/FBX/Resources/Generic_Resource_Wood_01.fbx` | `Prefabs/Level8/ResourcePrefabs/PF_Generic_Resource_Wood_01.prefab` |
| Stone | `Art/Imported/FBX/Resources/Generic_Resource_Stone_01.fbx` | `PF_Generic_Resource_Stone_01.prefab` |
| Fiber | `Art/Imported/FBX/Resources/Generic_Resource_Fiber_01.fbx` | `PF_Generic_Resource_Fiber_01.prefab` |
| OreFragment | `Art/Imported/FBX/Resources/Generic_Resource_OreFragment_01.fbx` | `PF_Generic_Resource_OreFragment_01.prefab` |
| Coal | `Art/Imported/FBX/Resources/Generic_Resource_Coal_01.fbx` | `PF_Generic_Resource_Coal_01.prefab` |
| Berry | `Art/Imported/FBX/Resources/Generic_Resource_Berry_01.fbx` | `PF_Generic_Resource_Berry_01.prefab` |

**Materials (v2 naming):**
- Wood: `WoodBarkMaterial`, `WoodCutMaterial`, `WoodDarkMaterial`
- Stone: `StoneMainMaterial`, `StoneDarkMaterial`, `StoneHighlightMaterial`
- Fiber: `FiberGreenMaterial`, `FiberYellowGreenMaterial`, `FiberBandMaterial`
- Visual-only FBX; `WorldPickupItem` remains on runtime pickup root via `ItemModuleFactory`.
- Registry: `Resources/Level8/Level8ResourcePrefabRegistry.asset` (Kinds 11,12,14,18,19,20)
- Editor: `Tools → Level8 → Integrate Resource Visuals (Wood Stone Fiber)` / `(OreFragment Coal Berry)`
