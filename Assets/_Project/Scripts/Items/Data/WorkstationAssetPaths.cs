/// <summary>Shared workstation / placed-object asset paths.</summary>
public static class WorkstationAssetPaths
{
    public const string WorkstationsFbxRoot = "Assets/_Project/Art/Imported/FBX/Workstations/";
    public const string BuildablesFbxRoot = "Assets/_Project/Art/Imported/FBX/SceneModules/Buildables/";
    public const string StationsPrefabRoot = "Assets/_Project/Prefabs/SceneModules/Stations/";

    public const string CraftingTableFbx = WorkstationsFbxRoot + "Generic_Workstation_CraftingTable_01.fbx";
    public const string CraftingTablePrefab = StationsPrefabRoot + "PF_Generic_Workstation_CraftingTable_01.prefab";

    public const string CampfireFbx = WorkstationsFbxRoot + "Generic_Workstation_Campfire_01.fbx";
    public const string CampfirePrefab = StationsPrefabRoot + "PF_Generic_Workstation_Campfire_01.prefab";

    /// <summary>Imported forge visual used for placed Furnace / 熔炉 workstation.</summary>
    public const string FurnaceFbx = BuildablesFbxRoot + "forge.fbx";
    public const string LegacyFurnaceFbx = WorkstationsFbxRoot + "Generic_Workstation_Furnace_01.fbx";
    public const string FurnacePrefab = StationsPrefabRoot + "PF_Generic_Workstation_Furnace_01.prefab";

    public const string ForgeFbx = WorkstationsFbxRoot + "Generic_Workstation_Forge_01.fbx";
    public const string ForgePrefab = StationsPrefabRoot + "PF_Generic_Workstation_Forge_01.prefab";
}
