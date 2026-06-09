# Unified Gameplay Prefab Rule

For Blender + Unity asset pipeline tasks: when the target object **already has backend gameplay logic** in the Unity project, the final Unity prefab should preferably become a **complete reusable gameplay prefab**, not just a loose visual prefab.

**Cursor rule:** `.cursor/rules/unified-gameplay-prefab-rule.mdc` (`alwaysApply: true`)

---

## Core Distinction

| Layer | Gameplay allowed? |
|-------|-------------------|
| **FBX / Blender model** | **No** — visual-only always |
| **Unity prefab** | **Yes** — when backend logic exists for this object type |

### Never put inside the FBX

- Gameplay scripts
- Collider / Trigger
- Rigidbody
- Damage / pickup / interaction / buff / objective / inventory logic

### Unity prefab may contain (when backend exists)

- LogicRoot + Collider + Trigger + gameplay scripts
- VisualRoot + AxisCorrection + FBX model
- LabelAnchor, InteractionPoint, WarningArea (as used by the system)

---

## 1. Preferred Complete Prefab Structure

```
PF_TargetObjectName
├── LogicRoot
│   ├── Collider / Trigger / DetectionArea
│   ├── Gameplay Script
│   ├── Interaction Script
│   └── Runtime State Components
├── VisualRoot (or Visual)
│   └── AxisCorrection
│       └── Blender FBX Model
├── WarningArea (hazards, when used)
├── LabelAnchor
└── InteractionPoint
```

If the existing project uses a different structure:

- Preserve the existing prefab structure
- Add or update Visual safely
- Keep existing logic components where they already are
- Avoid breaking serialized references

---

## 2. Decision Rule

Before building or updating a prefab, search for:

- Existing prefab
- Existing logic script
- Existing collider / trigger setup
- Existing detection area
- Existing registry / catalog entry
- Existing fallback visual builder
- Existing scene objects using this behavior

| Situation | Action |
|-----------|--------|
| **Backend logic exists** | Create/update **complete gameplay prefab**; keep logic + collider + visual together |
| **No backend** | Create **visual-only prefab**; report missing backend; do not invent logic |
| **Partial backend** | Preserve what exists; create visual; report missing pieces |

---

## 3. What Stays Together in a Complete Prefab

When the object has backend logic, keep under the same prefab when possible:

- Prefab root
- LogicRoot
- Collider / Trigger / detection / interaction / damage / pickup / buff / objective zones
- Gameplay + interaction scripts
- Label anchor / interaction point
- Visual model (under VisualRoot)
- Fallback visual reference (if architecture uses registry + runtime rebuild)

This makes the prefab reusable across scenes.

---

## 4. What Must Not Be Done

- Put gameplay scripts on the FBX model
- Put Collider / Trigger inside the imported FBX
- Rotate LogicRoot to fix visual orientation
- Rotate Collider / Trigger to fix visual orientation
- Delete existing backend components
- Duplicate gameplay scripts accidentally
- Create multiple competing versions of the same logic
- Hardcode scene-only references when registry/catalog exists
- Break primitive fallback behavior

---

## 5. Visual Import Rule (Blender FBX)

```
PrefabRoot
├── LogicRoot          ← gameplay (never rotated for axis fix)
└── VisualRoot
    └── AxisCorrection ← localRotation (90, 0, 180)
        └── FBX_Model  ← local (0,0,0) position/rotation/scale
```

Apply axis correction **only** to AxisCorrection — not PrefabRoot, LogicRoot, Collider, Trigger, or gameplay scripts.

---

## 6. Examples in This Project

### A. Hazard with backend — Electric Field

**Script:** `LaserHazard.cs`  
**Catalog:** `HazardElectricFieldPrefabCatalog`  
**Registry kinds:** `SceneModuleKind.ElectricFieldHazard` (complete module), `SceneModuleKind.ElectricField` (legacy visual-only)

```
PF_Generic_Hazard_ElectricField_01
├── LogicRoot
│   ├── BoxCollider (trigger)
│   └── LaserHazard
├── VisualRoot
│   └── PF_Generic_Hazard_ElectricField_01_Visual (or inline Visual/AxisCorrection/FBX)
├── WarningArea
├── LabelAnchor
└── DebugBounds (editor)
```

`LaserHazard.Create()` prefers `TryInstantiateRegisteredModule()` → complete prefab; falls back to runtime-built LogicRoot + primitive visual.

### B. Hazard with backend — Spike Trap

**Script:** `SpikeTrap.cs`  
**Registry:** `SceneModuleKind.Hazard` (visual prefab)  
**Runtime:** `SpikeTrap.Create()` builds LogicRoot + resolves visual from registry

Spike is currently **runtime-assembled complete module** + **visual-only registry entry**. Future work may promote to a baked complete prefab like ElectricField.

### C. Resource pickup

```
PF_Generic_Resource_Wood_01
├── LogicRoot
│   ├── PickupCollider
│   └── WorldPickupItem
└── VisualRoot → AxisCorrection → FBX
```

### D. Pure decoration (no backend)

```
PF_Generic_Decoration_RockCluster_01
└── Visual → AxisCorrection → FBX
```

No LogicRoot required.

---

## 7. Required Cursor Workflow

1. Identify target object type
2. Search backend logic / prefab / registry
3. Decide: complete gameplay prefab · visual-only · partial placeholder
4. Build/import Blender visual → export FBX
5. Materials under `Art/Imported/Materials/`
6. Create/update Unity prefab
7. If backend exists: LogicRoot + collider + scripts + Visual in one prefab
8. If no backend: visual-only + report
9. Update registry/catalog/resolver
10. Preserve fallback
11. Test relevant scene
12. Report (see §8)

---

## 8. Required Final Report

Every asset pipeline task must report:

- Whether backend logic exists
- Whether prefab was visual-only or complete gameplay prefab
- Which logic scripts were found
- Which Collider / Trigger / DetectionArea components were included
- Whether gameplay logic was preserved
- Whether FBX remained visual-only
- Final prefab hierarchy
- Final FBX path
- Final material folder
- Registry/catalog update
- Fallback status
- Scene tested
- Files changed
- Unresolved missing backend logic, if any

---

## Related Documents

- [Unity_Blender_MCP_Visual_Asset_Pipeline.md](./Unity_Blender_MCP_Visual_Asset_Pipeline.md)
- [Blender_Unity_Axis_Alignment_Rule.md](./Blender_Unity_Axis_Alignment_Rule.md)
- [Scene_Module_Migration_Plan.md](../SceneModules/Scene_Module_Migration_Plan.md)
