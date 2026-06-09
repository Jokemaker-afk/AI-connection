# Spike Trap 01 — Asset Source Note

| Field | Value |
|-------|-------|
| **Fab URL** | https://www.fab.com/listings/8b8eb6de-9f5a-4f81-b610-79cfd07d1961 |
| **Sketchfab URL** | https://sketchfab.com/3d-models/spike-trap-01-4022678cac214fd2963894aa152fc6f2 |
| **Model name** | Spike Trap 01 |
| **Creator** | Nichgon |
| **License** | CC0 (stated on Fab listing and Sketchfab description/tags) |
| **Project asset** | Generic_Hazard_SpikeTrap_01 |

## Import note

Automated download from Fab/Sketchfab was not available in this pipeline (Sketchfab API requires authentication). The in-project mesh was **rebuilt in Blender** to match the CC0 reference silhouette (square base, upward spikes, ~2m footprint) and restyled for this project's low-poly hazard look.

## Unity usage

- Visual-only prefab: `PF_Generic_Hazard_SpikeTrap_01`
- Gameplay collider/damage remains on `SpikeTrap` logic root, not on the FBX visual.
