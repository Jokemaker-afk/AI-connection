using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Assembles levels from CharacterSave progression + LevelModuleCatalog.
/// Rebuilds from GeneratedLevelSummary when present; otherwise generates deterministically and stores summary.
/// </summary>
public static class ModularLevelAssembler
{
    public const string AssembledContentRootName = "ModularAssembledContent";
    const int SaveDataVersionCurrent = 1;
    const float ModuleSpacing = 4.5f;
    const float FloorYOffset = 0.65f;

    public static void EnsureAssembledForScene(PlayerProgressionState progression, string sceneName)
    {
        if (progression == null || !GameplaySceneCatalog.IsSupportedGameplayScene(sceneName))
        {
            return;
        }

        progression.EnsureCharacterProgressionInitialized(sceneName);

        int levelIndex = GameplaySceneCatalog.GetLevelNumber(sceneName);
        if (levelIndex >= 8 && Level8GenerationFlags.UsesLevel8ChunkLayout)
        {
            GameplayCore.Instance?.Log("[ModularLevel] Skipping assembler: Level8 chunk layout builder is active.");
            return;
        }

        if (levelIndex < 4)
        {
            return;
        }

        Transform root = EnsureAssembledRoot();
        GeneratedLevelSummary summary = progression.GetGeneratedLevelSummary(levelIndex);

        if (summary.IsValid)
        {
            RebuildFromSummary(root, summary);
            GameplayCore.Instance?.Log(
                $"[ModularLevel] Rebuilt level {levelIndex} ({sceneName}) from save: {summary.ModuleIds.Length} modules.");
            return;
        }

        // Levels 4–7 keep authored layouts on first visit; modular generation starts at Level 8 (Blueprint v0.2).
        if (levelIndex < 8)
        {
            return;
        }

        summary = GenerateLevel(progression, sceneName, levelIndex, root);
        progression.SetGeneratedLevelSummary(summary);
        progression.UnlockModulesForLevel(levelIndex);

        GameplayCore.Instance?.Log(
            $"[ModularLevel] Generated level {levelIndex} ({sceneName}): {summary.ModuleIds.Length} modules, seed={summary.LevelSeed}.");
    }

    public static GeneratedLevelSummary GenerateLevel(
        PlayerProgressionState progression,
        string sceneName,
        int levelIndex,
        Transform parent)
    {
        ClearAssembledContent(parent);

        int worldSeed = progression.GetWorldSeed();
        int levelSeed = CombineSeed(worldSeed, levelIndex);
        var rng = new System.Random(levelSeed);

        List<LevelModuleDefinition> pool = BuildAvailablePool(progression, levelIndex);
        var selected = new List<LevelModuleDefinition>();
        var selectedIds = new HashSet<string>();

        TryAddRequiredModules(pool, selected, selectedIds, levelIndex);
        int targetCount = Mathf.Clamp(3 + levelIndex, 4, 12);
        FillWeightedSelection(pool, selected, selectedIds, rng, targetCount);

        var placements = new List<LevelModulePlacementData>();
        var moduleIds = new List<string>();
        Vector3 origin = ResolveAssemblyOrigin(sceneName);

        for (int i = 0; i < selected.Count; i++)
        {
            LevelModuleDefinition definition = selected[i];
            Vector3 localPos = origin + GetPlacementOffset(i, selected[i].ModuleType);
            SpawnModule(definition, parent, localPos, Quaternion.identity);

            moduleIds.Add(definition.ModuleId);
            placements.Add(new LevelModulePlacementData
            {
                ModuleId = definition.ModuleId,
                LocalPosition = localPos,
                LocalEulerAngles = Vector3.zero,
            });

            GameplayCore.Instance?.Log($"[ModularLevel] Selected module: {definition.ModuleId} ({definition.ModuleType})");
        }

        return new GeneratedLevelSummary
        {
            LevelIndex = levelIndex,
            SceneName = sceneName,
            LevelSeed = levelSeed,
            ModuleIds = moduleIds.ToArray(),
            Placements = placements.ToArray(),
        };
    }

    public static void RebuildFromSummary(Transform parent, GeneratedLevelSummary summary)
    {
        ClearAssembledContent(parent);

        if (summary.Placements != null && summary.Placements.Length > 0)
        {
            for (int i = 0; i < summary.Placements.Length; i++)
            {
                LevelModulePlacementData placement = summary.Placements[i];
                if (!LevelModuleCatalog.TryGet(placement.ModuleId, out LevelModuleDefinition definition))
                {
                    Debug.LogWarning($"[ModularLevel] Missing module definition: {placement.ModuleId}");
                    continue;
                }

                SpawnModule(definition, parent, placement.LocalPosition, Quaternion.Euler(placement.LocalEulerAngles));
            }

            return;
        }

        if (summary.ModuleIds == null)
        {
            return;
        }

        Vector3 origin = ResolveAssemblyOrigin(summary.SceneName);
        for (int i = 0; i < summary.ModuleIds.Length; i++)
        {
            string moduleId = summary.ModuleIds[i];
            if (!LevelModuleCatalog.TryGet(moduleId, out LevelModuleDefinition definition))
            {
                Debug.LogWarning($"[ModularLevel] Missing module definition: {moduleId}");
                continue;
            }

            Vector3 localPos = origin + GetPlacementOffset(i, definition.ModuleType);
            SpawnModule(definition, parent, localPos, Quaternion.identity);
        }
    }

    static List<LevelModuleDefinition> BuildAvailablePool(PlayerProgressionState progression, int levelIndex)
    {
        var pool = new List<LevelModuleDefinition>();
        foreach (LevelModuleDefinition definition in LevelModuleCatalog.AllDefinitions)
        {
            if (definition.MinLevelIndex > levelIndex)
            {
                continue;
            }

            if (!progression.AreModuleRequirementsMet(definition))
            {
                continue;
            }

            pool.Add(definition);
        }

        return pool;
    }

    static void TryAddRequiredModules(
        List<LevelModuleDefinition> pool,
        List<LevelModuleDefinition> selected,
        HashSet<string> selectedIds,
        int levelIndex)
    {
        if (levelIndex >= 4)
        {
            TryPickType(pool, selected, selectedIds, LevelModuleType.Pickup);
        }

        if (levelIndex >= 5)
        {
            TryPickModuleId(pool, selected, selectedIds, LevelModuleCatalog.InteractWorkbench);
        }

        if (levelIndex >= 6)
        {
            TryPickModuleId(pool, selected, selectedIds, LevelModuleCatalog.EncounterMiningRock);
            TryPickModuleId(pool, selected, selectedIds, LevelModuleCatalog.EncounterTreeStump);
        }
    }

    static void FillWeightedSelection(
        List<LevelModuleDefinition> pool,
        List<LevelModuleDefinition> selected,
        HashSet<string> selectedIds,
        System.Random rng,
        int targetCount)
    {
        int safety = pool.Count * 4;
        while (selected.Count < targetCount && pool.Count > 0 && safety-- > 0)
        {
            LevelModuleDefinition pick = PickWeighted(pool, rng);
            if (!pick.IsValid)
            {
                pool.Remove(pick);
                continue;
            }

            if (selectedIds.Add(pick.ModuleId))
            {
                selected.Add(pick);
            }
            else
            {
                pool.Remove(pick);
            }
        }
    }

    static LevelModuleDefinition PickWeighted(List<LevelModuleDefinition> pool, System.Random rng)
    {
        float total = 0f;
        for (int i = 0; i < pool.Count; i++)
        {
            total += Mathf.Max(0.01f, pool[i].Weight);
        }

        float roll = (float)rng.NextDouble() * total;
        float cumulative = 0f;
        for (int i = 0; i < pool.Count; i++)
        {
            cumulative += Mathf.Max(0.01f, pool[i].Weight);
            if (roll <= cumulative)
            {
                return pool[i];
            }
        }

        return pool[pool.Count - 1];
    }

    static void TryPickType(
        List<LevelModuleDefinition> pool,
        List<LevelModuleDefinition> selected,
        HashSet<string> selectedIds,
        LevelModuleType type)
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (pool[i].ModuleType == type && selectedIds.Add(pool[i].ModuleId))
            {
                selected.Add(pool[i]);
                return;
            }
        }
    }

    static void TryPickModuleId(
        List<LevelModuleDefinition> pool,
        List<LevelModuleDefinition> selected,
        HashSet<string> selectedIds,
        string moduleId)
    {
        if (selectedIds.Contains(moduleId))
        {
            return;
        }

        for (int i = 0; i < pool.Count; i++)
        {
            if (pool[i].ModuleId == moduleId && selectedIds.Add(moduleId))
            {
                selected.Add(pool[i]);
                return;
            }
        }
    }

    static void SpawnModule(
        LevelModuleDefinition definition,
        Transform parent,
        Vector3 worldPosition,
        Quaternion rotation)
    {
        if (!definition.IsValid)
        {
            return;
        }

        switch (definition.ModuleType)
        {
            case LevelModuleType.Pickup:
            case LevelModuleType.Reward:
                if (ItemKindUtility.IsValid(definition.PickupItemKind))
                {
                    ItemModuleFactory.SpawnWorldPickup(
                        definition.PickupItemKind,
                        worldPosition,
                        Mathf.Max(1, definition.PickupAmount),
                        parent);
                }

                break;

            case LevelModuleType.Interactable:
                if (ItemKindUtility.IsPlaceable(definition.PlacedItemKind))
                {
                    ItemModuleFactory.SpawnPlacedObject(definition.PlacedItemKind, worldPosition, rotation, parent);
                }
                else if (definition.ToolKind != ToolKind.None)
                {
                    SpawnToolInteractable(definition, parent, worldPosition);
                }

                break;

            case LevelModuleType.Exit:
                SpawnExitMarker(definition, parent, worldPosition);
                break;

            default:
                SpawnPlaceholder(definition, parent, worldPosition);
                break;
        }
    }

    static void SpawnToolInteractable(LevelModuleDefinition definition, Transform parent, Vector3 position)
    {
        var root = GameObject.CreatePrimitive(PrimitiveType.Cube);
        root.name = $"Module_{definition.ModuleId}";
        root.transform.SetParent(parent, false);
        root.transform.position = position;
        root.transform.localScale = definition.PlaceholderScale.sqrMagnitude > 0.01f
            ? definition.PlaceholderScale
            : new Vector3(1.2f, 1f, 1.2f);

        var renderer = root.GetComponent<Renderer>();
        if (renderer != null)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = definition.PlaceholderColor;
            renderer.sharedMaterial = material;
        }

        var interactable = root.AddComponent<ToolInteractable>();
        if (definition.ToolOutputItemKind != ItemKind.None && definition.ToolOutputAmount > 0)
        {
            interactable.ConfigureResource(
                definition.ToolKind,
                definition.DisplayLabelChinese,
                $"使用{ItemKindUtility.GetRequiredToolPrompt(definition.ToolKind)}",
                new[] { new ToolReward(definition.ToolOutputItemKind, definition.ToolOutputAmount, definition.ToolOutputAmount) },
                definition.ToolTaskId,
                dropToWorld: true,
                addToInventory: false);
        }
        else
        {
            interactable.ConfigureEventTrigger(
                definition.ToolKind,
                definition.DisplayLabelChinese,
                $"使用{ItemKindUtility.GetRequiredToolPrompt(definition.ToolKind)}",
                definition.ToolTaskId,
                definition.ToolTaskId);
        }

        ItemWorldLabel.Create(root.transform, definition.DisplayLabelChinese, Vector3.up * 1f, 0.1f);
    }

    static void SpawnExitMarker(LevelModuleDefinition definition, Transform parent, Vector3 position)
    {
        var root = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        root.name = $"Module_{definition.ModuleId}";
        root.transform.SetParent(parent, false);
        root.transform.position = position;
        root.transform.localScale = definition.PlaceholderScale.sqrMagnitude > 0.01f
            ? definition.PlaceholderScale
            : new Vector3(1.2f, 0.6f, 1.2f);

        Collider collider = root.GetComponent<Collider>();
        if (collider != null)
        {
            UnityEngine.Object.Destroy(collider);
        }

        var renderer = root.GetComponent<Renderer>();
        if (renderer != null)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = definition.PlaceholderColor;
            renderer.sharedMaterial = material;
        }

        ItemWorldLabel.Create(root.transform, definition.DisplayLabelChinese, Vector3.up * 1.2f, 0.1f);
    }

    static void SpawnPlaceholder(LevelModuleDefinition definition, Transform parent, Vector3 position)
    {
        var root = GameObject.CreatePrimitive(PrimitiveType.Cube);
        root.name = $"Module_{definition.ModuleId}";
        root.transform.SetParent(parent, false);
        root.transform.position = position;
        root.transform.localScale = definition.PlaceholderScale.sqrMagnitude > 0.01f
            ? definition.PlaceholderScale
            : Vector3.one;

        Collider collider = root.GetComponent<Collider>();
        if (collider != null)
        {
            UnityEngine.Object.Destroy(collider);
        }

        var renderer = root.GetComponent<Renderer>();
        if (renderer != null)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = definition.PlaceholderColor;
            renderer.sharedMaterial = material;
        }

        if (!string.IsNullOrEmpty(definition.DisplayLabelChinese))
        {
            ItemWorldLabel.Create(root.transform, definition.DisplayLabelChinese, Vector3.up * 0.8f, 0.1f);
        }
    }

    static Transform EnsureAssembledRoot()
    {
        var existing = GameObject.Find(AssembledContentRootName);
        if (existing != null)
        {
            return existing.transform;
        }

        var sceneRoot = GameObject.Find("SceneRoot");
        var go = new GameObject(AssembledContentRootName);
        if (sceneRoot != null)
        {
            go.transform.SetParent(sceneRoot.transform, false);
        }

        return go.transform;
    }

    static void ClearAssembledContent(Transform parent)
    {
        if (parent == null)
        {
            return;
        }

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            UnityEngine.Object.Destroy(parent.GetChild(i).gameObject);
        }
    }

    static Vector3 ResolveAssemblyOrigin(string sceneName)
    {
        var spawn = GameObject.Find("PlayerSpawnPoint");
        if (spawn != null)
        {
            return spawn.transform.position + new Vector3(6f, FloorYOffset, 6f);
        }

        return GameplaySceneCatalog.GetDefaultSpawn(sceneName) + new Vector3(6f, FloorYOffset, 6f);
    }

    static Vector3 GetPlacementOffset(int index, LevelModuleType type)
    {
        int row = index / 4;
        int col = index % 4;
        float x = col * ModuleSpacing;
        float z = row * ModuleSpacing;
        float y = type == LevelModuleType.Exit ? 0.55f : FloorYOffset;
        return new Vector3(x, y, z);
    }

    public static int CombineSeed(int worldSeed, int levelIndex)
    {
        unchecked
        {
            return worldSeed * 397 ^ levelIndex * 7919;
        }
    }
}
