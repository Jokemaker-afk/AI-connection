# Level 8 Asset Catalog (stubs)

Code-based catalog stubs for prefab-driven visuals. **Not wired to generators yet** — all lookups return false/null and existing primitive builders remain active.

| File | Purpose |
|------|---------|
| `Level8PropCategory.cs` | Prop category enum |
| `Level8AssetCatalogTypes.cs` | PropSet, spawn rules, profiles |
| `Level8VisualAssetResolver.cs` | Prefab vs placeholder instantiate helper |
| `Level8ChunkPrefabCatalog.cs` | Chunk prefab lookup |
| `Level8BiomePropSetCatalog.cs` | Weighted decoration sets |
| `Level8ObjectivePrefabCatalog.cs` | Data core / relay / exit visuals (**Phase B: Data Core wired**) |
| `Level8ObjectivePrefabRegistry.cs` | ScriptableObject registry (Resources/Level8/) |
| `Level8ObjectiveVisualUtility.cs` | Prefab instantiate + strip duplicate gameplay/colliders |
| `Level8ResourcePrefabCatalog.cs` | Biome + ItemKind resource visuals (**Phase C wired**) |
| `Level8ResourcePrefabRegistry.cs` | ScriptableObject registry (Resources/Level8/) |
| `Level8ResourceVisualUtility.cs` | Visual-only pickup prefab instantiate + strip gameplay |
| `ItemPickupVisualResolver.cs` | ItemCatalog + Level8 registry unified lookup |
| `Level8HazardPrefabCatalog.cs` | Hazard zone visuals |
| `Level8ChunkInstance.cs` | Chunk prefab authoring component |
| `Level8AssetSocket.cs` | Socket marker component |

See `Documentation/LevelDesign/Level8_PrefabDriven_Generation_Roadmap.md`.
