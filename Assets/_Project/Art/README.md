# Art (`Assets/_Project/Art/`)

First-party art root for environment, props, and VFX.

| Subfolder | Purpose |
|-----------|---------|
| `Imported/` | Source FBX, textures, and materials from DCC tools |
| `Placeholder/` | Procedural / primitive stand-ins until real art lands |
| `VFX/` | Shaders, particles, glow for objectives and hazards |

**Workflow:** FBX → Materials → Prefabs under `Assets/_Project/Prefabs/` → catalog entry → Level 8 generator.

See `Documentation/EnvironmentArt/Environment_Asset_Architecture.md` and `Documentation/ImportWorkflow/FBX_To_Level8_Prefab_Workflow.md`.
