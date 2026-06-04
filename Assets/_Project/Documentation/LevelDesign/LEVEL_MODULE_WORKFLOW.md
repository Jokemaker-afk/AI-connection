# Save-Driven Modular Level Workflow

## Data flow

```
Character save (PlayerProgressionSaveData)
  → unlockedModuleIds + worldSeed + generatedLevels[]
  → ModularLevelAssembler
  → LevelModuleCatalog (stable string moduleId)
  → ItemModuleFactory / ToolInteractable / placeholders
  → ModularAssembledContent parent in scene
```

## Adding a module

1. Add a stable string id constant in `LevelModuleCatalog`.
2. Register a `LevelModuleDefinition` in `LevelModuleCatalog.RegisterDefaults()`.
3. Unlock rules use `MinLevelIndex` and optional `RequiredUnlockedModuleIds`.

## Testing in Unity

1. Play from Level 7 (or load disk save under `Application.persistentDataPath`).
2. Confirm Console: `[ModularLevel] Generated level 7...`
3. Reload scene: same module list from `GeneratedLevelSummary`.
4. Leave level: `CapturePersistentState` writes JSON save and unlocks modules.
5. Enter Level 7 again with same character: rebuild log appears.

Levels 4–6 keep existing placeholder builders on first visit; modular rebuild applies only if a summary already exists in save.
