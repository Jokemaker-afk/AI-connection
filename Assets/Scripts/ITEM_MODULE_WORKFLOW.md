# Modular Item Workflow

> **Canonical doc:** `Assets/_Project/Documentation/ItemSystem/ITEM_MODULE_WORKFLOW.md`

This file is a pointer. Edit the canonical copy under `_Project/Documentation/ItemSystem/`.

**Blueprint v0.2:** tools use `ToolKind` + `ToolInteractable`; weapons use `WeaponKind` + `DamageableTarget` (Level7+). Both register through `ItemCatalog` and spawn via `ItemModuleFactory`.
