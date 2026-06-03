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

    public static CraftingStation Create(Transform parent, string name, Vector3 position, WorkstationKind kind = WorkstationKind.Workbench)
    {
        var root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = position;

        ItemKind visualKind = kind switch
        {
            WorkstationKind.Furnace => ItemKind.Furnace,
            WorkstationKind.Forge => ItemKind.Forge,
            WorkstationKind.Loom => ItemKind.Loom,
            WorkstationKind.AlchemyTable => ItemKind.AlchemyTable,
            WorkstationKind.ScienceLab => ItemKind.ScienceLab,
            _ => ItemKind.CraftingTable,
        };

        var table = GameObject.CreatePrimitive(PrimitiveType.Cube);
        table.name = "TableVisual";
        table.transform.SetParent(root.transform, false);
        table.transform.localPosition = Vector3.up * 0.45f;
        table.transform.localScale = new Vector3(1.6f, 0.9f, 1.2f);
        table.isStatic = true;
        Object.DestroyImmediate(table.GetComponent<Collider>());

        var renderer = table.GetComponent<Renderer>();
        if (renderer != null)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            Color color = ItemKindUtility.GetDisplayColor(visualKind);
            material.color = color;
            material.SetColor("_EmissionColor", color * 0.2f);
            material.EnableKeyword("_EMISSION");
            renderer.sharedMaterial = material;
        }

        ItemWorldLabel.Create(
            root.transform,
            ItemKindUtility.GetDisplayName(visualKind),
            Vector3.up * 1.15f);

        var collider = root.AddComponent<BoxCollider>();
        collider.center = Vector3.up * 0.45f;
        collider.size = new Vector3(1.6f, 0.9f, 1.2f);
        collider.isTrigger = false;

        var placed = root.AddComponent<PlacedBuilding>();
        BuildingKind buildingKind = ItemKindUtility.GetPlacedBuildingKind(visualKind);
        placed.Initialize(visualKind, buildingKind, kind);

        var station = root.AddComponent<CraftingStation>();
        station.Configure(kind, $"按 E 使用{ItemKindUtility.GetDisplayName(visualKind)}");
        placed.AttachCraftingStation(station);
        return station;
    }
}
