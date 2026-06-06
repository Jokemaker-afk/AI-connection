using UnityEngine;

/// <summary>Reparents generated Level8 content into readable category roots.</summary>
public static class Level8VisualHierarchyOrganizer
{
    public static void Organize(GameObject arenaRoot, Level8ArenaRoots roots)
    {
        if (arenaRoot == null || roots == null)
        {
            return;
        }

        ReparentObjectives(roots);
        ReparentResources(arenaRoot.transform, roots);
        ReparentChunkDecorations(arenaRoot.transform, roots);
        ReparentDebugLabels(arenaRoot.transform, roots);
    }

    static void ReparentObjectives(Level8ArenaRoots roots)
    {
        if (roots.Objectives == null)
        {
            return;
        }

        DataCoreCollectible[] cores = roots.Objectives.GetComponentsInChildren<DataCoreCollectible>(true);
        for (int i = 0; i < cores.Length; i++)
        {
            cores[i].transform.SetParent(roots.DataCores, true);
        }

        SignalRelayInteractable[] relays = roots.Objectives.GetComponentsInChildren<SignalRelayInteractable>(true);
        for (int i = 0; i < relays.Length; i++)
        {
            relays[i].transform.SetParent(roots.SignalRelay, true);
        }

        Level8ExitPortal[] exits = roots.Objectives.GetComponentsInChildren<Level8ExitPortal>(true);
        for (int i = 0; i < exits.Length; i++)
        {
            exits[i].transform.SetParent(roots.Exit, true);
        }
    }

    static void ReparentResources(Transform arenaRoot, Level8ArenaRoots roots)
    {
        WorldPickupItem[] pickups = arenaRoot.GetComponentsInChildren<WorldPickupItem>(true);
        for (int i = 0; i < pickups.Length; i++)
        {
            if (pickups[i].transform.parent == roots.Resources)
            {
                continue;
            }

            pickups[i].transform.SetParent(roots.Resources, true);
        }
    }

    static void ReparentChunkDecorations(Transform arenaRoot, Level8ArenaRoots roots)
    {
        Transform chunks = roots.Chunks != null ? roots.Chunks : arenaRoot.Find("Chunks");
        if (chunks == null)
        {
            return;
        }

        for (int i = 0; i < chunks.childCount; i++)
        {
            Transform chunk = chunks.GetChild(i);
            Transform chunkDecorations = chunk.Find("Decorations");
            if (chunkDecorations == null || chunkDecorations.childCount == 0)
            {
                continue;
            }

            var bucket = new GameObject(chunk.name);
            bucket.transform.SetParent(roots.Decorations, false);

            while (chunkDecorations.childCount > 0)
            {
                Transform child = chunkDecorations.GetChild(0);
                child.SetParent(bucket.transform, true);
            }
        }
    }

    static void ReparentDebugLabels(Transform arenaRoot, Level8ArenaRoots roots)
    {
        if (Level8GenerationFlags.ShowChunkDebugLabels || Level8GenerationFlags.ShowGenerationSeedLabel)
        {
            Transform markers = arenaRoot.Find("Markers");
            if (markers != null)
            {
                markers.SetParent(roots.Debug, true);
            }
        }
        else
        {
            Transform markers = arenaRoot.Find("Markers");
            if (markers != null)
            {
                for (int i = markers.childCount - 1; i >= 0; i--)
                {
                    Transform child = markers.GetChild(i);
                    if (child.name == "BiomeLabel")
                    {
                        child.SetParent(roots.Labels, true);
                        continue;
                    }

                    Object.Destroy(child.gameObject);
                }

                if (markers.childCount == 0)
                {
                    Object.Destroy(markers.gameObject);
                }
            }
        }

        Transform chunks = arenaRoot.Find("Chunks");
        if (chunks == null)
        {
            return;
        }

        for (int i = 0; i < chunks.childCount; i++)
        {
            Transform labels = chunks.GetChild(i).Find("Labels");
            if (labels == null)
            {
                continue;
            }

            bool debugMode = Level8GenerationFlags.ShowChunkDebugLabels
                || Level8VisualHierarchyFlags.ShowChunkAreaLabels;

            if (debugMode)
            {
                labels.SetParent(roots.Debug, true);
            }
            else
            {
                Object.Destroy(labels.gameObject);
            }
        }
    }
}
