# Project Folder Structure

This document describes the Unity asset layout under `Assets/_Project/`. Game code lives here; third-party template content is under `Assets/ThirdParty/`.

## Top-level layout

```
Assets/
├── _Project/           # All first-party game content
│   ├── Scenes/
│   ├── Scripts/
│   ├── Prefabs/
│   ├── Settings/       # URP / volume profiles
│   ├── Input/          # Input System actions
│   ├── Documentation/
│   └── (Art/, Audio/, ScriptableObjects/ — add when needed)
├── ThirdParty/         # Unity template / external packages (e.g. TutorialInfo)
└── Readme.asset        # Unity template readme (optional cleanup later)
```

## Scenes (`_Project/Scenes/Levels/`)

| Folder | Scene file | GameplaySceneCatalog name |
|--------|------------|---------------------------|
| Level1 | SampleScene.unity | SampleScene |
| Level2 | Level2.unity | Level2 |
| Level3 | Level3.unity | Level3 |
| Level4 | Level4.unity | Level4 |
| Level5 | Level5.unity | Level5 |
| Level6 | Level6.unity | Level6 |
| Level7 | Level7.unity | Level7 |

Runtime loads scenes **by name** (`SceneManager.LoadScene("Level5")`), not by asset path. After moving scenes, **Build Settings paths** must stay in sync (already updated in `ProjectSettings/EditorBuildSettings.asset`).

**New level:** create `Assets/_Project/Scenes/Levels/LevelN/LevelN.unity`, add to Build Settings, extend `GameplaySceneCatalog`, add scripts under `Scripts/Levels/LevelN/`.

## Scripts (`_Project/Scripts/`)

| Folder | Purpose | Examples |
|--------|---------|----------|
| `Core/Bootstrap/` | Scene load hooks, rig/HUD bootstrap | `GameplayFoundationBootstrap`, `PersistentPlayerRig` |
| `Core/GameState/` | Session managers | `GameplayCore`, `LevelManager` |
| `Core/Save/` | Character save + generated levels | `PlayerProgressionState`, `ModularLevelAssembler` input data |
| `Core/SceneManagement/` | Portals, scene catalog | `GameplaySceneCatalog`, `ScenePortal` |
| `Player/` | Movement, camera, visual, interaction, tools | `PlayerController`, `ItemModuleFactory` consumers |
| `Items/Data/` | Item definitions | `ItemKind`, `ItemCatalog` |
| `Items/Factory/` | Spawn API | `ItemModuleFactory` |
| `Items/Pickup/` | World pickups | `WorldPickupItem` |
| `Items/Placement/` | Placed objects | `PlacedObjectBuilder` |
| `Items/Visuals/` | Item meshes/labels | `ItemVisualBuilder` |
| `Inventory/` | Backpack + toolbar | `PlayerInventory`, `PlayerInventoryHud` |
| `Crafting/` | Recipes, stations, UI | `CraftingManager`, `CraftingHud` |
| `Building/` | Building registry / lighting helpers | `BuildingRegistry` |
| `UI/` | HUD, crosshair, shared UI utils | `PlayerHUD`, `GameplayCrosshairHud` |
| `Progression/` | Buffs, tech, collectibles | `TechnologyManager`, `CollectibleManager` |
| `Levels/Shared/` | Modular assembly, hazards, shared builders | `ModularLevelAssembler`, `LevelModuleCatalog` |
| `Levels/LevelN/` | Level-specific setup only | `Level5PlaceholderBuilder` |
| `Utilities/` | Small shared helpers | `GameplayLayers` |
| `Editor/` | Menu items (must stay named `Editor`) | `LevelSetupMenu` |

**New script:** place it in the matching folder above. Do not add files to a flat `Scripts/` root.

## Prefabs (`_Project/Prefabs/`)

Reserved layout (create subfolders as assets appear):

- `Core/` — GameplayCore rig, persistent player
- `Player/` — visual modules
- `Items/Pickups/`, `Items/PlacedObjects/`, `Items/HandheldTools/`
- `UI/`, `Crafting/`, `Building/`, `LevelObjects/`

Most gameplay objects are still **code-generated** via `ItemModuleFactory`. When adding real prefabs, register paths in `ItemData` / `LevelModuleDefinition`.

## Items — where to add content

1. `ItemKind.cs` — stable enum id  
2. `ItemCatalog.cs` — register `ItemData`  
3. Spawn via `ItemModuleFactory` (see `Documentation/ItemSystem/ITEM_MODULE_WORKFLOW.md`)

Optional prefab: assign in `ItemData`, store under `_Project/Prefabs/Items/`.

## Levels — modular / save-driven

1. `LevelModuleCatalog.cs` — stable string `moduleId`  
2. `ModularLevelAssembler.cs` — uses character save + catalog  
3. See `Documentation/LevelDesign/LEVEL_MODULE_WORKFLOW.md`

Level-specific **hand-authored** content: `Levels/LevelN/*PlaceholderBuilder.cs` + scene under `Scenes/Levels/LevelN/`.

## Documentation (`_Project/Documentation/`)

| Subfolder | Contents |
|-----------|----------|
| Architecture/ | This file, blueprint, process notes |
| ItemSystem/ | Item module workflow |
| LevelDesign/ | Level module workflow |
| Screenshots/ | Dev screenshots |

## Settings & input

- `_Project/Settings/` — URP assets (referenced by GUID; safe to move with `.meta`)
- `_Project/Input/InputSystem_Actions.inputactions` — linked from Project Settings → Input System

## ThirdParty

`Assets/ThirdParty/TutorialInfo/` — original Unity URP template readme/scripts. Not used by gameplay.

## Conventions

- **No namespaces** in this pass — class names stay global; folder = organization only.
- **Editor scripts** must live under a folder named `Editor`.
- **Do not** use Unity instance IDs or scene object names in save data.
- **Scene names** are the stable runtime ids (`Level5`, not folder paths).
