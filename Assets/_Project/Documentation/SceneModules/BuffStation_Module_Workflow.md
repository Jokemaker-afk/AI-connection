# BuffStation Module Workflow (Phase SM-C)

Reusable **HealStation** and **StaminaStation** visual modules for Level 3 `BuffBubble` instances, with primitive fallback.

**Scope:** Visual-only prefab replacement. Gameplay (`BuffBubble`, trigger, buff application) stays on the logic root.

---

## Naming clarification

| Scene object | Type | Module kind |
|--------------|------|-------------|
| `Bubble_+Z 回血` | Heal buff pickup | `SceneModuleKind.HealStation` |
| `Bubble_-Z 无限精力` | Infinite stamina buff pickup | `SceneModuleKind.StaminaStation` |
| `Return_+Z 回血` | Hub return zone (R key) | `SceneModuleKind.ReturnButton` (SM-B) |

`Bubble_*` objects are **not** return zones.

---

## Module structure

```
BuffBubble (LogicRoot)
├── BuffBubble              ← buff type, trigger collect, bob/spin
├── SphereCollider          ← trigger, radius 1.1 (gameplay bounds)
├── VisualRoot              ← prefab or primitive visuals only
│   └── (PF_Generic_BuffStation_* OR BubbleShell/Core/Ring fallback)
├── LabelAnchor             ← future world label anchor
└── InteractionPoint        ← future interact marker anchor
```

### What stays on LogicRoot

- `BuffBubble` script (buff type, one-shot, bob/spin)
- `SphereCollider` (trigger)
- `OnTriggerEnter` / `TryCollect` → `PlayerBuffController.TryApplyBuff`
- `BuffCollectionTracker.RegisterCollected`

### What goes under VisualRoot

- Registered prefab instance **or** primitive `BubbleShell` / `BubbleCore` / `BubbleRing`
- Future Blender FBX / mesh hierarchy
- **No** gameplay scripts, **no** colliders on prefab

---

## Visual resolution pipeline

```
BuffBubble.Start / Initialize
  → MigrateLegacyVisualChildren (baked scenes: remove root-level BubbleShell/Core/Ring)
  → EnsureLabelAnchor / EnsureInteractionPoint
  → Map buffType → SceneModuleKind (Heal / InfiniteStamina only)
  → SceneModuleVisualResolver.TryResolveSceneModulePrefab(kind)
       1. Exact kind (HealStation / StaminaStation)
       2. Generic BuffStation (optional shared prefab)
       3. null → BuildPrimitiveBuffStationVisuals()
  → SceneModuleVisualUtility.InstantiateModuleVisual (strips colliders + BuffBubble dupes)
```

Speed / Shield buff bubbles still use **primitive fallback only** in SM-C (no registry lookup).

---

## Test prefab paths

| Kind | Prefab |
|------|--------|
| HealStation | `Assets/_Project/Prefabs/SceneModules/BuffStations/PF_Generic_BuffStation_Heal_01.prefab` |
| StaminaStation | `Assets/_Project/Prefabs/SceneModules/BuffStations/PF_Generic_BuffStation_Stamina_01.prefab` |

### Heal prefab hierarchy

```
PF_Generic_BuffStation_Heal_01
└── Visual
    ├── StationBase
    ├── HealPad
    ├── GlowCore
    ├── GlowRing
    └── MedicalIcon (+ cross child)
```

Green / light green / white — medical identity, distinct from ReturnButton.

### Stamina prefab hierarchy

```
PF_Generic_BuffStation_Stamina_01
└── Visual
    ├── StationBase
    ├── EnergyPad
    ├── GlowCore
    ├── GlowRing
    └── EnergyIcon (+ bolt child)
```

Blue / cyan / yellow — energy identity, distinct from HealStation.

---

## Registry and editor menus

**Registry asset:** `Assets/_Project/Resources/SceneModules/SceneModulePrefabRegistry.asset`

| Menu | Action |
|------|--------|
| `Tools → SceneModules → Create BuffStation Test Prefabs And Registry` | Create/update Heal + Stamina test prefabs; register both kinds |
| `Tools → SceneModules → Clear BuffStation Prefab Registry (Test Fallback)` | Remove HealStation, StaminaStation, BuffStation entries → primitive fallback |

ReturnButton menus (SM-B) are unchanged.

---

## Testing

### Mode A — Prefab registered

1. Run **Create BuffStation Test Prefabs And Registry**
2. Open / Play `Level3`
3. `Bubble_+Z 回血` → green station prefab visual
4. `Bubble_-Z 无限精力` → blue/cyan/yellow station prefab visual
5. Collect buffs — heal and infinite stamina behavior unchanged
6. Speed / Shield bubbles still use bubble primitives

### Mode B — Registry cleared

1. Run **Clear BuffStation Prefab Registry**
2. Play Level3
3. Heal and Stamina stations revert to `BubbleShell` / `BubbleCore` / `BubbleRing`
4. Buff behavior still works

### UI safety

- `HealthBar` / `StaminaBar` under `GameplayHUD` — **Unity UI only**, not affected by this phase.

---

## Future Blender replacement

1. Import FBX under `Art/Imported/FBX/SceneModules/BuffStations/`
2. Build prefab with mesh under `Visual/` child (same hierarchy slot as test prefab)
3. Register in `SceneModulePrefabRegistry` for `HealStation` or `StaminaStation`
4. Do **not** add `BuffBubble`, colliders, or triggers to the prefab
5. Logic root in scene keeps `BuffBubble` + `SphereCollider`; only `VisualRoot` content swaps

Optional: register one generic `BuffStation` prefab as fallback for both kinds (material variants later).

---

## Related scripts

| File | Role |
|------|------|
| `BuffBubble.cs` | Logic + VisualRoot build / fallback |
| `BuffStationPrefabCatalog.cs` | Heal/Stamina → registry lookup + generic BuffStation fallback |
| `SceneModuleVisualResolver.cs` | Central `TryResolveSceneModulePrefab` |
| `SceneModuleVisualUtility.cs` | Instantiate visual-only; strip gameplay dupes |
| `SceneModulePrefabSetupMenu.cs` | Editor test prefab + registry menus |
| `Level3BuffHubBuilder.cs` | Calls `BuffBubble.Create()` — no change required |

---

## Related documents

- `Scene_Module_Migration_Plan.md` — full migration phases
- `ReturnButton_Module_Workflow.md` — SM-B pattern reference
