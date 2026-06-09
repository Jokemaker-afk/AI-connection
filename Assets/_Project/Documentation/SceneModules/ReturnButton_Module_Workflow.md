# ReturnButton Scene Module Workflow

Phase **SM-B** — reusable ReturnButton visual with primitive fallback.

---

## Module runtime structure

Each `HubReturnZone` instance (LogicRoot = zone root GameObject):

```
Return_+Z 回血                    ← LogicRoot (zone root)
├── BoxCollider                   ← trigger, gameplay bounds (unchanged)
├── HubReturnZone                 ← R-key return logic
├── VisualRoot                    ← visual-only children
│   ├── (prefab instance) OR ReturnButton primitive cube
├── LabelAnchor                   ← world hint anchor (y = 1.2)
└── InteractionPoint              ← optional interact point (y = 0.6)
```

**Rules:**

- Gameplay stays on LogicRoot (`HubReturnZone` + `BoxCollider`).
- Blender / test prefab is **visual-only** under `VisualRoot`.
- Legacy baked `ReturnButton` child (direct under root) is removed on `Awake` and replaced.

---

## Visual priority

1. `SceneModulePrefabRegistry` → `SceneModuleKind.ReturnButton`
2. Registry → `SceneModuleKind.InteractionButton` (generic)
3. `BuildPrimitiveReturnButtonVisual()` — original blue cube

Resolver: `ReturnButtonPrefabCatalog.TryResolveReturnButtonPrefab()`

---

## Files

| File | Role |
|------|------|
| `HubReturnZone.cs` | Logic + `EnsureModuleVisual()` |
| `ReturnButtonPrefabCatalog.cs` | Lookup |
| `SceneModulePrefabRegistry.cs` | Resources registry asset |
| `SceneModuleVisualUtility.cs` | Instantiate + strip colliders/gameplay |
| `SceneModuleKind.cs` | Module enum |

**Test prefab:** `Prefabs/SceneModules/Interactables/PF_Generic_Interactable_ReturnButton_01.prefab`

**Registry:** `Resources/SceneModules/SceneModulePrefabRegistry.asset`

---

## Editor menus

| Menu | Action |
|------|--------|
| `Tools → SceneModules → Create ReturnButton Test Prefab And Registry` | Create prefab + register |
| `Tools → SceneModules → Clear ReturnButton Prefab Registry (Test Fallback)` | Clear registration |

---

## Testing

### Prefab mode

1. Run **Create ReturnButton Test Prefab And Registry**
2. Play Level 3
3. All 4 return zones should show platform/button prefab visual
4. Enter zone, press **R** → teleport to hub center

### Fallback mode

1. Run **Clear ReturnButton Prefab Registry**
2. Restart Play Mode
3. Blue primitive cube visual returns
4. **R** still works

---

## Replace with Blender FBX later

1. Model in Blender — pivot at base center, 1 unit = 1 meter
2. Export FBX → `Art/Imported/FBX/SceneModules/Interactables/`
3. Build visual-only prefab (same hierarchy as test prefab: `Visual/ButtonBase/...`)
4. **Do not** add `HubReturnZone` or colliders on prefab
5. Assign prefab in `SceneModulePrefabRegistry` as `ReturnButton`
6. Play Level 3 — all 4 instances pick up new visual automatically

---

## Level 3 instances

All four paths use the same system via `HubReturnZone.Awake()`:

- `Return_+Z 回血`
- `Return_+X 加速`
- `Return_-Z 无限精力`
- `Return_-X 护盾`

Trigger size and position are **not** changed by visual migration.
