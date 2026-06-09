# Mandatory Blender to Unity Axis Rule

**Apply to every Blender MCP + Unity MCP asset task. Do not skip this rule.**

The model must be authored and exported so it appears **upright in Unity** without rotating the gameplay root, LogicRoot, Collider, Trigger, or prefab root.

**Cursor rule section:** `.cursor/rules/unity-blender-mcp-visual-pipeline.mdc` §2b

---

## 1. Blender Modeling Axis Standard

In Blender:

- **X** = left / right
- **Y** = front / back
- **Z** = up / down
- Model **height must go along Blender +Z**
- Model **base must lie flat on the Blender X-Y plane**
- Model **bottom must sit exactly on Blender Z = 0**
- Intended **gameplay-facing front must face Blender -Y**
- Model **right side should face Blender +X**
- **Origin / pivot must be at bottom center** unless a special pivot is explicitly requested

For stations and interactables:

- Front-facing icon / screen / symbol / arrow → **Blender -Y**
- Back support / rear column → **Blender +Y**
- Left and right side panels → **Blender X**
- Vertical objects (columns, cores, signs, batteries, capsules, rings) → extend along **Blender +Z**

Unity expected result:

- **Unity +Y** = up
- **Unity +Z** = gameplay forward
- **Unity +X** = right
- Model stands upright; front faces **Unity +Z**; bottom on ground

**Simple rule:** **Blender -Y → Unity +Z**, **Blender +Z → Unity +Y**, **Blender +X → Unity +X**

---

## 2. Required Blender Pre-Export Check

Before exporting FBX, perform this exact check:

1. View the model in Blender.
2. Confirm it is **standing upright**.
3. Confirm the **base is flat on the X-Y ground plane**.
4. Confirm the **lowest point is on Z = 0**.
5. Confirm the **model front faces Blender -Y**.
6. Confirm the **model height goes along Blender +Z**.
7. Select all exported mesh objects.
8. **Apply Rotation and Scale:** `Ctrl + A` → Rotation & Scale
9. Confirm **no mesh object** has hidden bad rotation or weird scale.
10. **Do not export Camera.**
11. **Do not export Light.**
12. **Do not export colliders or gameplay helper objects.**

If the model is **lying sideways in Blender**, fix it before export.

If the model is **upright but facing backward**, rotate the **mesh geometry 180° around Blender Z**, then apply Rotation and Scale.

---

## 3. FBX Export Settings for Unity

Use these exact FBX export settings:

| Setting | Value |
|---------|-------|
| Format | FBX |
| Forward | **-Z Forward** |
| Up | **Y Up** |
| Apply Transform | **ON** |
| Scale | **1.0** |
| Object Types | Mesh only, plus Empty only if required for clean root |

**Do not export:**

- Camera
- Light
- Armature (unless explicitly needed)
- Animation (unless explicitly needed)
- Collider helper objects

The exported FBX should import into Unity upright by default.

**Do not rely on Unity rotation to make the model stand up.**

Blender MCP export defaults:

```python
bpy.ops.export_scene.fbx(
    axis_forward="-Z",
    axis_up="Y",
    apply_scale_options="FBX_SCALE_ALL",
    object_types={"EMPTY", "MESH"},
    bake_anim=False,
)
```

Unity ModelImporter (project default for new FBX `.meta`):

- `bakeAxisConversion: 1`
- `importCameras: 0`
- `importLights: 0`
- `materialLocation: 1` (External — see Material Placement Rule)

---

## 4. Unity Import / Prefab Transform Rule

After importing into Unity, **do not rotate:**

- Prefab root
- LogicRoot
- Collider
- Trigger
- Interaction object
- Gameplay scripts
- BuffBubble
- HubReturnZone
- HealthBar / StaminaBar

The visual hierarchy should ideally be:

```
PrefabRoot
└── Visual or VisualRoot
    └── FBX_Model
```

**Preferred transform:**

| Object | localPosition | localRotation | localScale |
|--------|---------------|---------------|------------|
| Visual / VisualRoot | (0, 0, 0) | (0, 0, 0) | (1, 1, 1) |
| FBX_Model child | (0, 0, 0) | (0, 0, 0) | (1, 1, 1) |

If the model is **lying down or sideways in Unity:**

- **Do not** fix it by rotating the prefab root
- **Do not** fix it by rotating LogicRoot
- **Go back to Blender**
- Bake the mesh orientation correctly
- Re-export with **Forward = -Z Forward**, **Up = Y Up**, **Apply Transform = ON**

---

## 5. Unity Visual-Only AxisCorrection Rule

When a Blender FBX appears **lying down, sideways, or flipped** in Unity after import, use a **visual-only** correction wrapper. Do **not** rotate gameplay objects.

### Preferred hierarchy

```
PF_Generic_* 
└── Visual (or VisualRoot)
    └── AxisCorrection
        └── Generic_*_01   (FBX model)
```

### Import workflow (default for all Blender FBX visual prefabs)

**Default:** always use **AxisCorrection `localRotation = (90, 0, 180)`** for Blender-imported FBX visual models. Do not manually test each asset unless a rare exception is confirmed.

1. Import FBX under prefab **Visual**.
2. Ensure hierarchy:

```
Visual
└── AxisCorrection          localRotation = (90, 0, 180)
    └── FBX_Model           localRotation = (0, 0, 0)
```

3. **Visual** stays `(0, 0, 0)`. **FBX child** stays `(0, 0, 0)`.
4. Only **one** AxisCorrection wrapper — no stacking.
5. Save prefab.

**Automation in this project:**

- Runtime: `SceneModuleVisualUtility.InstantiateModuleVisual` calls `EnsureBlenderAxisCorrection` automatically.
- Editor: **Tools → SceneModules → Apply Default Blender Axis Correction To All Visual Prefabs**
- New prefabs: call `SceneModuleVisualUtility.EnsureBlenderAxisCorrection(root)` before saving.

**Note:** Unity may display the same rotation as euler `(90, 180, 0)` in the Inspector — equivalent to `(90, 0, 180)`.

### Do NOT rotate

- Prefab root (if it contains gameplay logic)
- LogicRoot
- Collider / Trigger
- InteractionPoint / LabelAnchor
- BuffBubble / HubReturnZone
- HealthBar / StaminaBar
- Gameplay scripts

### Do NOT apply correction on FBX child directly (preferred)

Move any legacy rotation hack from the FBX nested instance onto **AxisCorrection** instead, and zero the FBX child rotation.

---

## 6. Required Cursor Report

For **every** generated or repaired model, Cursor must report:

### Blender

- Model height direction: **Blender +Z**
- Model front direction: **Blender -Y**
- Bottom sits on **Z = 0**
- Origin / pivot is **bottom center**
- **Rotation and Scale applied** before export

### FBX

- Forward = **-Z Forward**
- Up = **Y Up**
- Apply Transform = **ON**
- Scale = **1**

### Unity

- Prefab root rotation **unchanged**
- LogicRoot rotation **unchanged**
- Collider / Trigger **unchanged**
- VisualRoot **localRotation**
- FBX child **localRotation**
- **AxisCorrection** **localRotation** = **`(90, 0, 180)`** (default for all Blender FBX)
- Whether model is **upright in Play Mode**

---

## Related

- [Unity_Blender_MCP_Visual_Asset_Pipeline.md](./Unity_Blender_MCP_Visual_Asset_Pipeline.md)
- [Unity_Imported_Material_Placement_Rule.md](./Unity_Imported_Material_Placement_Rule.md)
