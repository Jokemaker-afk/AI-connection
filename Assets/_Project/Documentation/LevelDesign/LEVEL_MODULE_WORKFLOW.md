# Save-Driven Modular Level Workflow

## Blueprint alignment (v0.2)

- **Level7** is now the **weapon-use tutorial** (fixed authored layout, not modular exploration).
- **Level8+** is where the first **formal semi-random exploration** begins (formerly Blueprint Level7).
- When implementing modular generation, align `ModularLevelAssembler` start threshold with **Level 8**, not Level 7.

> **Implementation note:** Current code may still treat Level 7 as the modular start index until the weapon tutorial and assembler threshold are updated together. Do not change gameplay code as part of blueprint-only edits.

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

1. Play from **Level 8** (or load disk save under `Application.persistentDataPath`).
2. Confirm Console: `[ModularLevel] Generated level 8...` (after assembler threshold migration).
3. Reload scene: same module list from `GeneratedLevelSummary`.
4. Leave level: `CapturePersistentState` writes JSON save and unlocks modules.
5. Enter Level 8 again with same character: rebuild log appears.

Levels 4–6 keep existing placeholder builders on first visit; **Level 7** uses authored weapon-tutorial layout. Modular rebuild applies from **Level 8** onward (once implementation matches this blueprint).
