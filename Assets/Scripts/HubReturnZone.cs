using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(BoxCollider))]
public class HubReturnZone : MonoBehaviour
{
    static Vector3 hubSpawnPoint = new Vector3(0f, 0.25f, 0f);

    [SerializeField] string promptText = "按 R 返回第三关原点";

    bool playerInside;

    public static void SetHubSpawn(Vector3 spawn)
    {
        hubSpawnPoint = spawn;
    }

    void Awake()
    {
        var box = GetComponent<BoxCollider>();
        box.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            playerInside = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            playerInside = false;
        }
    }

    void Update()
    {
        if (!playerInside)
        {
            return;
        }

        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            ReturnPlayer();
        }
    }

    void ReturnPlayer()
    {
        var player = FindFirstObjectByType<PlayerController>();
        if (player == null)
        {
            return;
        }

        var controller = player.GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
            player.transform.position = hubSpawnPoint;
            player.transform.rotation = Quaternion.identity;
            controller.enabled = true;
        }
        else
        {
            player.transform.position = hubSpawnPoint;
            player.transform.rotation = Quaternion.identity;
        }
    }

    public static HubReturnZone Create(Transform parent, string name, Vector3 position, Vector3 size)
    {
        var root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = position;

        var trigger = root.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = size;

        var button = GameObject.CreatePrimitive(PrimitiveType.Cube);
        button.name = "ReturnButton";
        button.transform.SetParent(root.transform, false);
        button.transform.localPosition = Vector3.up * 0.6f;
        button.transform.localScale = new Vector3(2.4f, 0.35f, 1.2f);
        button.isStatic = true;
        Object.DestroyImmediate(button.GetComponent<Collider>());

        var renderer = button.GetComponent<Renderer>();
        if (renderer != null)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(0.25f, 0.55f, 0.95f);
            material.SetColor("_EmissionColor", new Color(0.2f, 0.45f, 0.95f) * 0.6f);
            material.EnableKeyword("_EMISSION");
            renderer.sharedMaterial = material;
        }

        var labelGo = new GameObject("LabelAnchor");
        labelGo.transform.SetParent(root.transform, false);
        labelGo.transform.localPosition = new Vector3(0f, 1.2f, 0f);

        return root.AddComponent<HubReturnZone>();
    }
}
