# Generic_Hazard_ElectricField_01 — Asset Source

| Field | Value |
|-------|-------|
| **Project asset** | Generic_Hazard_ElectricField_01 |
| **Style** | Low-poly stylized sci-fi laser electric grid (fence + energy barrier + laser gate) |
| **Authored in** | Blender (from scratch, no external paid mesh) |
| **Visual-only** | Yes — gameplay on `LaserHazard` LogicRoot |

## References (visual inspiration only)

- Real electric fence: vertical posts + horizontal wires + warning strips
- Sci-fi energy barrier: cyan transparent field plane between metal posts
- Sci-fi laser gate: glowing beams between dark emitters

## Unity integration

- Visual-only prefab: `PF_Generic_Hazard_ElectricField_01`
- Gameplay collider/damage remains on `LaserHazard` LogicRoot, not on the FBX visual.
- Registry: `SceneModuleKind.ElectricField`

## Dimensions (Blender)

- Width X ≈ 3.35m
- Depth Y ≈ 0.31m
- Height Z ≈ 1.87m
- Pivot: bottom center, Z = 0
