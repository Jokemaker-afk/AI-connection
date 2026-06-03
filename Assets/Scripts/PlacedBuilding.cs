using UnityEngine;

public class PlacedBuilding : MonoBehaviour
{
    [SerializeField] ItemKind sourceItem = ItemKind.None;
    [SerializeField] BuildingKind buildingKind = BuildingKind.None;
    [SerializeField] WorkstationKind workstationKind = WorkstationKind.None;

    CraftingStation craftingStation;
    bool registered;

    public ItemKind SourceItem => sourceItem;
    public BuildingKind BuildingKind => buildingKind;
    public WorkstationKind WorkstationKind => workstationKind;
    public CraftingStation CraftingStation => craftingStation;

    public void Initialize(ItemKind itemKind, BuildingKind building, WorkstationKind workstation)
    {
        sourceItem = itemKind;
        buildingKind = building;
        workstationKind = workstation;
        ApplyUnlockSideEffects();
        RequiredPlacedObjectTracker.Instance?.RegisterPlacedOnce(sourceItem);
        TryRegister();
    }

    void OnEnable()
    {
        TryRegister();
    }

    void OnDisable()
    {
        TryUnregister();
    }

    void TryRegister()
    {
        if (registered || buildingKind == BuildingKind.None)
        {
            return;
        }

        BuildingRegistry.Instance?.Register(buildingKind);
        registered = true;
    }

    void TryUnregister()
    {
        if (!registered || buildingKind == BuildingKind.None)
        {
            return;
        }

        BuildingRegistry.Instance?.Unregister(buildingKind);
        registered = false;
    }

    void ApplyUnlockSideEffects()
    {
        var tech = TechnologyManager.Instance;
        if (tech == null)
        {
            return;
        }

        switch (buildingKind)
        {
            case BuildingKind.Workbench:
                tech.Unlock(TechnologyKind.Woodworking);
                tech.Unlock(TechnologyKind.Stonework);
                break;
            case BuildingKind.Furnace:
                tech.Unlock(TechnologyKind.Smelting);
                break;
            case BuildingKind.Forge:
                tech.Unlock(TechnologyKind.Metalworking);
                break;
            case BuildingKind.WoodChest:
            case BuildingKind.LargeChest:
                tech.Unlock(TechnologyKind.Storage);
                break;
            case BuildingKind.ScienceLab:
                tech.Unlock(TechnologyKind.BasicCombat);
                break;
        }
    }

    public void AttachCraftingStation(CraftingStation station)
    {
        craftingStation = station;
    }
}
