# ScriptableObjects (`Assets/_Project/ScriptableObjects/`)

Data-driven profiles for level generation and environment art.

| Subfolder | Purpose |
|-----------|---------|
| `LevelGeneration/Biomes/` | Biome profiles (future SO migration from `Level8BiomeProfileCatalog`) |
| `LevelGeneration/ChunkPrefabProfiles/` | ChunkKind × Biome prefab mappings |
| `LevelGeneration/PropSets/` | Weighted decoration / prop spawn tables |
| `LevelGeneration/HazardProfiles/` | Hazard visual + gameplay profiles |
| `Environment/PropProfiles/` | Individual prop metadata |
| `Environment/ResourceNodeProfiles/` | Resource node prefab + ItemKind links |

**Current state:** catalogs are documented and stubbed in code under `Scripts/Levels/Level8/AssetCatalog/` until assets exist.
