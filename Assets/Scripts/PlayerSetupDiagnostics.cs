using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class PlayerSetupDiagnostics
{
    public struct Report
    {
        public bool PlayerRootValid;
        public bool BlockVisualPresent;
        public bool LegacyCapsulePreserved;
        public bool VisualSwitcherPresent;
        public List<string> PresentComponents;
        public List<string> MissingComponents;
        public List<string> VisualOnlyNodes;

        public string BuildSummary()
        {
            var builder = new StringBuilder();
            builder.AppendLine("=== Player Setup Comparison ===");
            builder.AppendLine($"PlayerRoot valid: {PlayerRootValid}");
            builder.AppendLine($"Block visual present: {BlockVisualPresent}");
            builder.AppendLine($"Legacy capsule preserved (disabled): {LegacyCapsulePreserved}");
            builder.AppendLine($"Visual switcher present: {VisualSwitcherPresent}");
            builder.AppendLine($"Gameplay components on PlayerRoot: {PresentComponents.Count}");
            for (int i = 0; i < PresentComponents.Count; i++)
            {
                builder.AppendLine($"  + {PresentComponents[i]}");
            }

            if (MissingComponents.Count > 0)
            {
                builder.AppendLine($"Missing components: {MissingComponents.Count}");
                for (int i = 0; i < MissingComponents.Count; i++)
                {
                    builder.AppendLine($"  - {MissingComponents[i]}");
                }
            }

            builder.AppendLine("Visual-only nodes:");
            for (int i = 0; i < VisualOnlyNodes.Count; i++)
            {
                builder.AppendLine($"  * {VisualOnlyNodes[i]}");
            }

            return builder.ToString();
        }
    }

    static readonly System.Type[] RequiredGameplayComponentTypes =
    {
        typeof(CharacterController),
        typeof(PlayerController),
        typeof(PlayerStats),
        typeof(PlayerInventory),
        typeof(PlayerPickupInteractor),
        typeof(PlayerCraftingInteractor),
        typeof(PlayerPlacementController),
        typeof(PlayerGameplayTargeting),
        typeof(PlayerToolController),
        typeof(PlayerToolSockets),
        typeof(PlayerVisualSwitcher),
    };

    public static Report Analyze(GameObject playerRoot)
    {
        var report = new Report
        {
            PlayerRootValid = playerRoot != null,
            PresentComponents = new List<string>(),
            MissingComponents = new List<string>(),
            VisualOnlyNodes = new List<string>(),
        };

        if (playerRoot == null)
        {
            for (int i = 0; i < RequiredGameplayComponentTypes.Length; i++)
            {
                report.MissingComponents.Add(RequiredGameplayComponentTypes[i].Name);
            }

            return report;
        }

        for (int i = 0; i < RequiredGameplayComponentTypes.Length; i++)
        {
            System.Type componentType = RequiredGameplayComponentTypes[i];
            if (playerRoot.GetComponent(componentType) != null)
            {
                report.PresentComponents.Add(componentType.Name);
            }
            else
            {
                report.MissingComponents.Add(componentType.Name);
            }
        }

        Transform visualRoot = playerRoot.transform.Find(PlayerVisualBuilder.VisualRootName);
        report.BlockVisualPresent = visualRoot != null
            && visualRoot.Find(PlayerVisualBuilder.DirectionalModelName) != null;
        report.LegacyCapsulePreserved = visualRoot != null
            && visualRoot.Find(PlayerVisualBuilder.LegacyCapsuleVisualName) != null;
        report.VisualSwitcherPresent = playerRoot.GetComponent<PlayerVisualSwitcher>() != null;

        if (visualRoot != null)
        {
            report.VisualOnlyNodes.Add($"{PlayerVisualBuilder.VisualRootName}/{PlayerVisualBuilder.DirectionalModelName}");
            if (report.LegacyCapsulePreserved)
            {
                report.VisualOnlyNodes.Add($"{PlayerVisualBuilder.VisualRootName}/{PlayerVisualBuilder.LegacyCapsuleVisualName}");
            }
        }

        return report;
    }

    public static void LogReport(GameObject playerRoot)
    {
        Report report = Analyze(playerRoot);
        GameplayCore.Instance?.Log(report.BuildSummary());
    }
}
