using UnityEngine;

public class CraftingStation : MonoBehaviour
{
    [SerializeField] WorkstationKind workstationKind = WorkstationKind.Workbench;
    [SerializeField] float interactRange = 3.5f;
    [SerializeField] string promptText = "按 E 打开工作台";

    public WorkstationKind WorkstationKind => workstationKind;
    public float InteractRange => interactRange;
    public string PromptText => promptText;

    public void Configure(WorkstationKind kind, string prompt = null)
    {
        workstationKind = kind;
        if (!string.IsNullOrEmpty(prompt))
        {
            promptText = prompt;
        }
    }

    public bool IsPlayerInRange(Vector3 playerPosition)
    {
        Vector3 flat = transform.position - playerPosition;
        flat.y = 0f;
        return flat.sqrMagnitude <= interactRange * interactRange;
    }

    /// <summary>Legacy wrapper — spawns modular placed workstation via ItemModuleFactory.</summary>
    public static CraftingStation Create(Transform parent, string name, Vector3 position, WorkstationKind kind = WorkstationKind.Workbench)
    {
        ItemModuleValidation.WarnLegacyCreationPath(nameof(CraftingStation.Create), ItemModuleFactory.GetItemKindForWorkstation(kind));
        GameObject root = ItemModuleFactory.SpawnWorkstation(kind, position, Quaternion.identity, parent, name);
        return root != null ? root.GetComponent<CraftingStation>() : null;
    }
}
