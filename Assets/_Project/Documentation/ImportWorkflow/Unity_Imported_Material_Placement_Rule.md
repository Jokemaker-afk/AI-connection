# Unity Imported Material Placement Rule

For all Blender-generated or externally imported FBX assets, keep **FBX**, **Unity materials**, **textures**, and **prefabs** in separate responsibility-based folders.

**Do not** place extracted Unity `.mat` files directly next to the FBX (except temporary debugging).

**Cursor rule:** `.cursor/rules/unity-blender-mcp-visual-pipeline.mdc` §2c

---

## Preferred Structure

| Asset type | Path |
|------------|------|
| FBX | `Assets/_Project/Art/Imported/FBX/` |
| Materials | `Assets/_Project/Art/Imported/Materials/` |
| Textures | `Assets/_Project/Art/Imported/Textures/` |
| Prefabs | `Assets/_Project/Prefabs/` |

Materials folder **mirrors** FBX category (same sub-path under `Materials/` instead of `FBX/`).

---

## 1. General Rule

- FBX folders = imported model source **only**
- Materials = parallel `Materials/` tree by category
- Keeps models, materials, textures, prefabs cleanly separated

---

## 2. HealStation Example

**FBX:**
`Assets/_Project/Art/Imported/FBX/SceneModules/BuffStations/Generic_Interactable_HealStation_01.fbx`

**Materials folder:**
`Assets/_Project/Art/Imported/Materials/SceneModules/BuffStations/HealStation/`

| File |
|------|
| `BaseMaterial.mat` |
| `PanelMaterial.mat` |
| `HealGlowMaterial.mat` |
| `MedicalIconMaterial.mat` |

**Do not** store these beside the FBX.

### Mesh → Material assignment

| Mesh | Material |
|------|----------|
| StationBase | BaseMaterial |
| Optional_BackColumn | BaseMaterial |
| HealPad | PanelMaterial |
| SidePanel_L / SidePanel_R | PanelMaterial |
| GlowCore | HealGlowMaterial |
| Optional_GlowRing | HealGlowMaterial |
| MedicalIcon | MedicalIconMaterial |

---

## 3. Shader Rule

Default: **Universal Render Pipeline/Lit**

Glow parts (GlowCore, GlowRing): URP/Lit + **Emission** enabled.

If pink → fix shader, assign from correct `Materials/` folder.

---

## 4. Directory Pattern (Other Assets)

| Asset | FBX | Materials |
|-------|-----|-----------|
| ReturnButton | `FBX/SceneModules/Interactables/Generic_Interactable_ReturnButton_01.fbx` | `Materials/SceneModules/Interactables/ReturnButton/` |
| Wood | `FBX/Resources/Generic_Resource_Wood_01.fbx` | `Materials/Resources/Wood/` |
| DataCore | `FBX/Objectives/Generic_Objective_DataCore_01.fbx` | `Materials/Objectives/DataCore/` |
| RockCluster | `FBX/Props/Generic_Decoration_RockCluster_01.fbx` | `Materials/Props/RockCluster/` |

---

## 5. Unity MCP Required Behavior

When importing/updating FBX prefab:

1. Check/create correct **Materials** folder
2. Extract/create `.mat` files **only** in Materials folder
3. **Do not** leave new `.mat` beside FBX
4. Configure FBX importer **External** material remaps → centralized mats
5. Assign materials to prefab mesh renderers from centralized folder
6. Save prefab; delete any stray FBX-adjacent materials

---

## 6. Required Status Report

Report: FBX path; prefab path; material folder; mats created/reused; shader; pink fixes; mesh→material map; prefab saved.

---

## Related

- [Unity_Blender_MCP_Visual_Asset_Pipeline.md](./Unity_Blender_MCP_Visual_Asset_Pipeline.md)
- [Blender_Unity_Axis_Alignment_Rule.md](./Blender_Unity_Axis_Alignment_Rule.md)
