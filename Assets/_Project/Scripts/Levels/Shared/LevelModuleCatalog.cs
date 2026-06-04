using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stable string module IDs for save-driven level assembly. Not Unity instance IDs.
/// </summary>
public static class LevelModuleCatalog
{
    public const string PickupWood = "pickup_wood";
    public const string PickupStone = "pickup_stone";
    public const string PickupFiber = "pickup_fiber";
    public const string RewardBerry = "reward_berry";
    public const string InteractWorkbench = "interact_workbench";
    public const string InteractFurnace = "interact_furnace";
    public const string EncounterMiningRock = "encounter_mining_rock";
    public const string EncounterTreeStump = "encounter_tree_stump";
    public const string EncounterSignalRelay = "encounter_signal_relay";
    public const string ExitBeacon = "exit_beacon";
    public const string ObstacleRockA = "obstacle_rock_a";

    static readonly Dictionary<string, LevelModuleDefinition> Modules = new Dictionary<string, LevelModuleDefinition>();

    static LevelModuleCatalog()
    {
        RegisterDefaults();
    }

    public static bool TryGet(string moduleId, out LevelModuleDefinition definition)
    {
        return Modules.TryGetValue(moduleId, out definition);
    }

    public static IEnumerable<LevelModuleDefinition> AllDefinitions => Modules.Values;

    public static IEnumerable<string> AllModuleIds => Modules.Keys;

    public static void Register(LevelModuleDefinition definition)
    {
        if (!definition.IsValid)
        {
            Debug.LogWarning("[LevelModuleCatalog] Skipped module with empty id.");
            return;
        }

        if (Modules.ContainsKey(definition.ModuleId))
        {
            Debug.LogWarning($"[LevelModuleCatalog] Duplicate module id: {definition.ModuleId}");
        }

        Modules[definition.ModuleId] = definition;
    }

    static void RegisterDefaults()
    {
        RegisterPickup(PickupWood, ItemKind.Wood, 3, 4, 1.2f);
        RegisterPickup(PickupStone, ItemKind.Stone, 2, 4, 1.1f);
        RegisterPickup(PickupFiber, ItemKind.Fiber, 2, 5, 1f);
        RegisterPickup(RewardBerry, ItemKind.Berry, 4, 3, 0.9f);

        Register(new LevelModuleDefinition
        {
            ModuleId = InteractWorkbench,
            ModuleType = LevelModuleType.Interactable,
            MinLevelIndex = 5,
            Weight = 1.5f,
            PlacedItemKind = ItemKind.CraftingTable,
            DisplayLabelChinese = "工作台",
        });

        Register(new LevelModuleDefinition
        {
            ModuleId = InteractFurnace,
            ModuleType = LevelModuleType.Interactable,
            MinLevelIndex = 5,
            RequiredUnlockedModuleIds = new[] { InteractWorkbench },
            Weight = 1.2f,
            PlacedItemKind = ItemKind.Furnace,
            DisplayLabelChinese = "熔炉",
        });

        RegisterEncounter(
            EncounterMiningRock,
            ToolKind.Pickaxe,
            "矿点",
            ItemKind.Stone,
            2,
            Level6TaskProgressTracker.MineOnceTaskId,
            6,
            new Color(0.52f, 0.54f, 0.58f),
            new Vector3(1.4f, 1.1f, 1.4f));

        RegisterEncounter(
            EncounterTreeStump,
            ToolKind.Axe,
            "木桩",
            ItemKind.Wood,
            2,
            Level6TaskProgressTracker.ChopOnceTaskId,
            6,
            new Color(0.48f, 0.32f, 0.18f),
            new Vector3(1.1f, 0.7f, 1.1f));

        RegisterEncounter(
            EncounterSignalRelay,
            ToolKind.RepairTool,
            "信号中继器",
            ItemKind.None,
            0,
            Level6TaskProgressTracker.RepairSignalRelayTaskId,
            6,
            new Color(0.38f, 0.62f, 0.92f),
            new Vector3(1f, 1.2f, 1f));

        Register(new LevelModuleDefinition
        {
            ModuleId = ExitBeacon,
            ModuleType = LevelModuleType.Exit,
            MinLevelIndex = 3,
            Weight = 0.5f,
            DisplayLabelChinese = "出口信标",
            PlaceholderScale = new Vector3(1.2f, 0.6f, 1.2f),
            PlaceholderColor = new Color(0.35f, 0.75f, 1f),
        });

        Register(new LevelModuleDefinition
        {
            ModuleId = ObstacleRockA,
            ModuleType = LevelModuleType.Obstacle,
            MinLevelIndex = 2,
            Weight = 1f,
            DisplayLabelChinese = "岩石",
            PlaceholderScale = new Vector3(1.6f, 0.9f, 1.4f),
            PlaceholderColor = new Color(0.45f, 0.47f, 0.5f),
        });
    }

    static void RegisterPickup(string moduleId, ItemKind kind, int amount, int minLevel, float weight)
    {
        Register(new LevelModuleDefinition
        {
            ModuleId = moduleId,
            ModuleType = LevelModuleType.Pickup,
            MinLevelIndex = minLevel,
            Weight = weight,
            PickupItemKind = kind,
            PickupAmount = amount,
        });
    }

    static void RegisterEncounter(
        string moduleId,
        ToolKind toolKind,
        string label,
        ItemKind output,
        int outputAmount,
        string taskId,
        int minLevel,
        Color color,
        Vector3 scale)
    {
        Register(new LevelModuleDefinition
        {
            ModuleId = moduleId,
            ModuleType = LevelModuleType.Interactable,
            MinLevelIndex = minLevel,
            Weight = 1f,
            ToolKind = toolKind,
            DisplayLabelChinese = label,
            ToolOutputItemKind = output,
            ToolOutputAmount = outputAmount,
            ToolTaskId = taskId,
            PlaceholderColor = color,
            PlaceholderScale = scale,
        });
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Tools/Gameplay/Validate Level Module Catalog")]
    public static void ValidateCatalogMenu()
    {
        LevelModuleCatalogValidation.ValidateAll(logWarnings: true);
    }
#endif
}
