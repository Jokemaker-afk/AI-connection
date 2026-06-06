using System.Collections.Generic;

using UnityEngine;



/// <summary>Spawns and wires Level8 objective interactables after arena generation.</summary>

public static class Level8ObjectiveBuilder

{

    public static Level8GeneratedLevelSummary SetupObjectives(Level8ChunkLayout layout, Transform arenaRoot)

    {

        var summary = Level8GeneratedLevelSummary.FromLayout(layout);

        Level8ArenaRoots roots = Level8ArenaBuildContext.Roots;

        Transform objectivesParent = roots?.Objectives ?? EnsureObjectivesParent(arenaRoot);

        Transform dataCoresParent = roots?.DataCores ?? objectivesParent;

        Transform relayParent = roots?.SignalRelay ?? objectivesParent;

        Transform exitParent = roots?.Exit ?? objectivesParent;



        var dataCores = new List<DataCoreCollectible>();

        Level8BiomeProfile biome = layout.BiomeProfile.BiomeKind != default

            ? layout.BiomeProfile

            : Level8BiomeProfileCatalog.GetProfile(Level8BiomeKind.DataWilderness);

        Color dataCoreColor = biome.DataCoreColor;

        Color landmarkColor = biome.LandmarkColor;

        Level8BiomeKind biomeKind = biome.BiomeKind != default ? biome.BiomeKind : Level8BiomeKind.DataWilderness;



        for (int i = 0; i < layout.Slots.Count; i++)

        {

            Level8ChunkSlot slot = layout.Slots[i];

            if (!slot.HasDataCoreMarker)

            {

                continue;

            }



            Vector3 worldPos = Level8ObjectivePositions.GetDataCoreWorldPosition(slot);

            DataCoreCollectible collectible = SpawnDataCore(

                dataCoresParent,

                worldPos,

                slot.DataCoreIndex,

                dataCoreColor,

                slot.ChunkKind,

                slot.GridPosition,

                biomeKind);

            dataCores.Add(collectible);

            summary.RegisterDataCore(collectible, slot.GridPosition);

        }



        SignalRelayInteractable relay = null;

        Level8ExitPortal exitPortal = null;



        if (layout.FinalSlot != null)

        {

            relay = SpawnSignalRelay(relayParent, layout.FinalSlot.WorldCenter, landmarkColor);

            exitPortal = SpawnExitPortal(exitParent, layout.FinalSlot.WorldCenter, landmarkColor);

            summary.RegisterSignalRelay(relay);

            summary.RegisterExitPortal(exitPortal);

        }



        summary.ValidateObjectives(dataCores, relay, exitPortal);

        summary.LogSummary();



        Level8ProgressionManager manager = Level8ProgressionBootstrap.EnsureProgressionManager(relay, exitPortal);

        manager?.BindExitPortal(exitPortal);



        return summary;

    }



    static Transform EnsureObjectivesParent(Transform arenaRoot)

    {

        Transform existing = arenaRoot != null ? arenaRoot.Find("Objectives") : null;

        if (existing != null)

        {

            return existing;

        }



        var objectivesGo = new GameObject("Objectives");

        if (arenaRoot != null)

        {

            objectivesGo.transform.SetParent(arenaRoot, false);

        }



        return objectivesGo.transform;

    }



    static DataCoreCollectible SpawnDataCore(

        Transform parent,

        Vector3 worldPos,

        int index,

        Color glowColor,

        Level8ChunkKind hostKind,

        Vector2Int gridPosition,

        Level8BiomeKind biomeKind)

    {

        var root = new GameObject($"Objective_DataCore_{index}");

        root.transform.SetParent(parent, false);

        root.transform.position = worldPos;



        var collectible = root.AddComponent<DataCoreCollectible>();

        collectible.Configure(index, glowColor, hostKind, gridPosition, biomeKind);

        return collectible;

    }



    static SignalRelayInteractable SpawnSignalRelay(Transform parent, Vector3 finalCenter, Color accent)

    {

        var root = new GameObject("Objective_SignalRelay");

        root.transform.SetParent(parent, false);



        var relay = root.AddComponent<SignalRelayInteractable>();

        relay.Configure(Level8ObjectivePositions.GetSignalRelayPosition(finalCenter), accent);

        return relay;

    }



    static Level8ExitPortal SpawnExitPortal(Transform parent, Vector3 finalCenter, Color landmark)

    {

        var root = new GameObject("Objective_ExitToLevel9");

        root.transform.SetParent(parent, false);



        var portal = root.AddComponent<Level8ExitPortal>();

        portal.Configure(Level8ObjectivePositions.GetExitPosition(finalCenter), landmark, "Level9");

        return portal;

    }

}


