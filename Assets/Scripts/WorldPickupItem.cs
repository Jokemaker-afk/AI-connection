using UnityEngine;

public class WorldPickupItem : MonoBehaviour
{
    [SerializeField] ItemKind itemKind = ItemKind.RedBlock;
    [SerializeField] int amount = 1;
    [SerializeField] float bobAmplitude = 0.12f;
    [SerializeField] float bobSpeed = 2f;
    [SerializeField] float spinSpeed = 35f;

    Vector3 basePosition;
    bool collected;

    public ItemKind ItemKind => itemKind;
    public int Amount => amount;
    public bool IsCollected => collected;

    void Awake()
    {
        basePosition = transform.position;
    }

    void Update()
    {
        if (collected)
        {
            return;
        }

        float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        transform.position = basePosition + Vector3.up * bob;
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
    }

    public bool TryCollect(PlayerInventory inventory)
    {
        if (collected || inventory == null)
        {
            return false;
        }

        if (!inventory.TryAddItem(itemKind, amount))
        {
            return false;
        }

        collected = true;
        Destroy(gameObject);
        return true;
    }

    public static WorldPickupItem Create(Transform parent, string name, Vector3 position, ItemKind kind, int pickupAmount = 1)
    {
        var root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = position;

        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "Visual";
        cube.transform.SetParent(root.transform, false);
        cube.transform.localPosition = Vector3.zero;
        cube.transform.localScale = Vector3.one * 0.55f;
        cube.isStatic = true;
        Object.DestroyImmediate(cube.GetComponent<Collider>());

        var renderer = cube.GetComponent<Renderer>();
        if (renderer != null)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            Color color = ItemKindUtility.GetDisplayColor(kind);
            material.color = color;
            material.SetColor("_EmissionColor", color * 0.35f);
            material.EnableKeyword("_EMISSION");
            renderer.sharedMaterial = material;
        }

        var pickup = root.AddComponent<WorldPickupItem>();
        pickup.itemKind = kind;
        pickup.amount = pickupAmount;
        pickup.basePosition = position;
        return pickup;
    }
}
