using UnityEngine;

/// <summary>Audits generated Level8 object counts and logs a readability summary.</summary>
public static class Level8ObjectAudit
{
    public struct AuditResult
    {
        public int DataCores;
        public int ResourcePickups;
        public int HazardZones;
        public int DecorationProps;
        public int DebugMarkers;
        public int ObjectiveRelays;
        public int ObjectiveExits;
    }

    public static AuditResult AuditAndLog(GameObject arenaRoot)
    {
        var result = new AuditResult();
        if (arenaRoot == null)
        {
            Debug.LogWarning("[Level8] Object audit skipped — arena root is null.");
            return result;
        }

        DataCoreCollectible[] cores = arenaRoot.GetComponentsInChildren<DataCoreCollectible>(true);
        result.DataCores = cores.Length;

        WorldPickupItem[] pickups = arenaRoot.GetComponentsInChildren<WorldPickupItem>(true);
        result.ResourcePickups = pickups.Length;

        Transform hazards = arenaRoot.transform.Find("Hazards");
        result.HazardZones = hazards != null ? hazards.childCount : 0;

        Transform decorations = arenaRoot.transform.Find("Decorations");
        if (decorations != null)
        {
            result.DecorationProps = CountDecorationProps(decorations);
        }
        else
        {
            result.DecorationProps = CountNamedProps(arenaRoot.transform, "Decorations");
        }

        Transform debugRoot = arenaRoot.transform.Find("Debug");
        result.DebugMarkers = debugRoot != null ? debugRoot.childCount : 0;
        if (Level8GenerationFlags.ShowChunkDebugBorders || Level8GenerationFlags.ShowChunkDebugLabels)
        {
            result.DebugMarkers += CountNamedProps(arenaRoot.transform, "DebugBorder");
        }

        SignalRelayInteractable[] relays = arenaRoot.GetComponentsInChildren<SignalRelayInteractable>(true);
        result.ObjectiveRelays = relays.Length;

        Level8ExitPortal[] exits = arenaRoot.GetComponentsInChildren<Level8ExitPortal>(true);
        result.ObjectiveExits = exits.Length;

        Debug.Log(
            $"[Level8] Generated objects summary: DataCores={result.DataCores}, Resources={result.ResourcePickups}, " +
            $"Hazards={result.HazardZones}, Decorations={result.DecorationProps}, DebugMarkers={result.DebugMarkers}");

        LogReadabilityNotes(result, arenaRoot);
        return result;
    }

    static int CountDecorationProps(Transform decorationsRoot)
    {
        int count = 0;
        for (int i = 0; i < decorationsRoot.childCount; i++)
        {
            count += decorationsRoot.GetChild(i).childCount;
        }

        if (count == 0)
        {
            count = CountRenderersUnder(decorationsRoot);
        }

        return count;
    }

    static int CountNamedProps(Transform root, string nameContains)
    {
        int count = 0;
        Transform[] all = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i].name.Contains(nameContains))
            {
                count++;
            }
        }

        return count;
    }

    static int CountRenderersUnder(Transform root)
    {
        return root.GetComponentsInChildren<Renderer>(true).Length;
    }

    static void LogReadabilityNotes(AuditResult result, GameObject arenaRoot)
    {
        if (result.DataCores != 3)
        {
            Debug.LogWarning($"[Level8] Audit: expected 3 Data Cores, found {result.DataCores}.");
        }

        if (result.DebugMarkers > 0 && !Level8GenerationFlags.ShowChunkDebugLabels)
        {
            Debug.LogWarning($"[Level8] Audit: {result.DebugMarkers} debug markers visible while debug flags are off.");
        }

        WorldPickupItem[] pickups = arenaRoot.GetComponentsInChildren<WorldPickupItem>(true);
        int missingEnhancer = 0;
        for (int i = 0; i < pickups.Length; i++)
        {
            if (pickups[i].GetComponent<Level8ResourcePickupEnhancer>() == null)
            {
                missingEnhancer++;
            }
        }

        if (missingEnhancer > 0)
        {
            Debug.Log($"[Level8] Audit: {missingEnhancer} resource pickups without category visual enhancer.");
        }
    }
}
