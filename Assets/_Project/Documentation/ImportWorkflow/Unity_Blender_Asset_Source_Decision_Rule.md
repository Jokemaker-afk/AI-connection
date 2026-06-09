# Unity / Blender Asset Source Decision Rule

This project uses Blender / Blender MCP / Cursor to create or prepare visual-only 3D models for a Unity game project.

**Network use is allowed:** Blender and Cursor may search, download, and import free external models when the license permits. Always verify license before use.

Before creating any new model from scratch, first decide whether the asset should be:

1. Created manually in Blender from simple low-poly geometry.
2. Imported from a free external 3D asset library and then cleaned up / modified in Blender.
3. Built as a hybrid: start from a free model, simplify it, recolor it, resize it, rename it, and restructure it to match this project.

---

## Default Asset Style

All models should match:

- low-poly / stylized survival crafting game style
- clear silhouette for third-person gameplay
- lightweight geometry suitable for real-time Unity use
- simple materials and readable colors
- modular structure, easy to replace later
- visual-only usage inside Unity prefabs

Do not create overly realistic, high-poly, or hard-to-edit models unless explicitly requested.

---

## When to Use External Free Models

External free models may be used when the target object is common, generic, or not gameplay-specific, for example:

- rocks
- trees
- grass clusters
- bushes
- crates
- barrels
- ruins
- furniture
- sci-fi panels
- simple machines
- terrain decorations
- environmental props

External free models are especially useful for decoration assets and background environment assets.

Do not use external models blindly for important gameplay-readable objects unless they can be clearly adapted to the project style.

---

## Preferred Free Asset Sources

Prefer assets with permissive licenses, especially CC0 or equivalent public-domain-style licenses.

Good candidates to check manually include:

| Source | How to use | License notes |
|--------|------------|---------------|
| **Poly Haven** | Blender MCP: `search_polyhaven_assets`, `download_polyhaven_asset` | CC0 — preferred |
| **Kenney** | Manual download | Check pack license (often CC0) |
| **Quaternius** | Manual download | Check pack license |
| **OpenGameArt** | Manual download | Check exact license per asset |
| **Sketchfab** | Blender MCP or manual download | Check exact license per model |

For Sketchfab and similar sites, every downloaded model must be checked individually. Do not assume all free models are allowed for commercial or game use.

---

## License Rules

Before using an external model, check and record:

- asset source website
- original model name
- creator name, if available
- license type
- whether attribution is required
- whether commercial use is allowed
- whether modification is allowed

Prefer CC0 assets when possible.

If the license is unclear, restrictive, non-commercial only, editorial only, or requires terms that are not suitable for a game project, do not use that model.

If attribution is required, create or update an attribution note for the project (e.g. under `Assets/_Project/Documentation/`).

---

## External Model Cleanup Rules

When importing an external model into Blender:

1. Remove cameras.
2. Remove lights.
3. Remove unused empty objects unless needed for organization.
4. Remove external gameplay logic, scripts, physics objects, or collision meshes.
5. Remove unnecessary animations unless the asset is specifically intended to be animated.
6. Simplify overly dense geometry if needed.
7. Apply all transforms.
8. Set scale so that 1 Blender meter = 1 Unity unit.
9. Set the origin / pivot according to the project rule:
   - small resources: bottom center
   - interactables: bottom center unless otherwise specified
   - decorations: bottom center
   - terrain chunks: center
10. Make sure the bottom of the visual model sits on Blender Z = 0.
11. Rename all important objects clearly.
12. Assign simple Unity-friendly materials.
13. Keep the model visual-only.

---

## Unity Import Rules

The final FBX must follow the project's Unity workflow:

- export as FBX
- apply all transforms before export
- do not export cameras
- do not export lights
- do not add Unity gameplay scripts
- do not add colliders inside the FBX visual model
- do not add pickup / inventory / interaction logic
- collision, interaction, trigger, and gameplay logic must stay on the Unity prefab root / LogicRoot / existing gameplay system
- the imported model should be placed under the prefab Visual object

Related docs:

- [FBX_To_Level8_Prefab_Workflow.md](./FBX_To_Level8_Prefab_Workflow.md)
- [ReturnButton_Module_Workflow.md](../SceneModules/ReturnButton_Module_Workflow.md)
- [Environment_Asset_Architecture.md](../EnvironmentArt/Environment_Asset_Architecture.md)

---

## Naming Rules

Use the project naming style.

Examples:

- `Generic_Resource_Wood_01.fbx`
- `Generic_Resource_Stone_01.fbx`
- `Generic_Interactable_ReturnButton_01.fbx`
- `Generic_Interactable_HealStation_01.fbx`
- `Generic_Objective_DataCore_01.fbx`
- `Forest_Decoration_Tree_01.fbx`
- `Generic_Decoration_RockCluster_01.fbx`

Unity prefab examples:

- `PF_Generic_Resource_Wood_01.prefab`
- `PF_Generic_Interactable_ReturnButton_01.prefab`
- `PF_Generic_Interactable_HealStation_01.prefab`

---

## Folder Rules

Export FBX files into the correct Unity import folder.

Examples:

| Category | Path |
|----------|------|
| Resources | `Assets/_Project/Art/Imported/FBX/Resources/` |
| Objectives | `Assets/_Project/Art/Imported/FBX/Objectives/` |
| Scene module interactables | `Assets/_Project/Art/Imported/FBX/SceneModules/Interactables/` |
| Environment / props | `Assets/_Project/Art/Imported/FBX/Environment/`, `.../Props/` |
| Materials | `Assets/_Project/Art/Imported/Materials/` |

---

## Decision Process for Each New Asset

For every requested asset, do this first:

1. **Identify the asset type:** Resource, Objective, Interactable, Decoration, Hazard, Terrain Chunk
2. **Decide the source method:** build from scratch / import free model / hybrid
3. **If using an external model:** verify license → record source → clean up → adapt style → export clean FBX
4. **If building from scratch:** simple low-poly geometry → clear silhouette → simple materials → correct pivot/scale → export clean FBX
5. **After export:** import into Unity → place under Visual → do not modify LogicRoot/Collider/gameplay → test in scene and Play Mode

---

## Special Rule for Important Interactables

For interactables such as ReturnButton, HealStation, SavePoint, Portal, DataCore, or SignalRelay:

- prioritize visual readability over realism
- make the object recognizable from a third-person camera
- keep the structure modular
- keep the model visual-only
- do not replace or disturb the existing Unity gameplay logic
- if orientation is wrong, adjust only the Visual object or visual child rotation, not the logic root
- if interaction stops working, check Unity prefab logic references instead of editing the FBX

---

## Cursor Rule

This document is partially superseded by the full pipeline rule:

- **Primary:** `.cursor/rules/unity-blender-mcp-visual-pipeline.mdc` (`alwaysApply: true`)
- **Full doc:** [Unity_Blender_MCP_Visual_Asset_Pipeline.md](./Unity_Blender_MCP_Visual_Asset_Pipeline.md)

When working in the Blender workspace (`Connection to Unity`), the same rule file is also present locally.
