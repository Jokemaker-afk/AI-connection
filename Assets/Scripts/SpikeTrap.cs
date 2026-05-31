using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class SpikeTrap : MonoBehaviour
{
    [SerializeField] float damage = 18f;
    [SerializeField] float hazardCooldown = 0.8f;
    [SerializeField] float retriggerDelay = 0.5f;

    float nextDamageTime;
    bool armed = true;

    void Awake()
    {
        ConfigureCollider();
    }

    void ConfigureCollider()
    {
        var box = GetComponent<BoxCollider>();
        box.isTrigger = true;
    }

    void OnTriggerStay(Collider other)
    {
        if (!armed || Time.time < nextDamageTime)
        {
            return;
        }

        if (!other.TryGetComponent<PlayerStats>(out var stats))
        {
            return;
        }

        if (!stats.TryTakeDamage(damage))
        {
            return;
        }

        armed = false;
        nextDamageTime = Time.time + hazardCooldown + retriggerDelay;
        Invoke(nameof(Rearm), hazardCooldown + retriggerDelay);
    }

    void Rearm()
    {
        armed = true;
    }

    public static SpikeTrap Create(Transform parent, string name, Vector3 position, Vector3 baseSize, int spikeCount = 3)
    {
        var root = new GameObject(name);
        root.transform.SetParent(parent, false);
        root.transform.position = position;

        var trigger = root.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(baseSize.x, 0.55f, baseSize.z);
        trigger.center = new Vector3(0f, 0.25f, 0f);

        var trap = root.AddComponent<SpikeTrap>();

        float spacing = baseSize.x / spikeCount;
        float startX = -baseSize.x * 0.5f + spacing * 0.5f;
        var spikeColor = new Color(0.45f, 0.45f, 0.5f);

        for (int i = 0; i < spikeCount; i++)
        {
            var spike = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spike.name = $"Spike_{i + 1}";
            spike.transform.SetParent(root.transform, false);
            spike.transform.localPosition = new Vector3(startX + spacing * i, 0.18f, 0f);
            spike.transform.localScale = new Vector3(0.22f, 0.36f, 0.22f);
            spike.transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
            spike.isStatic = true;
            Object.DestroyImmediate(spike.GetComponent<Collider>());

            var renderer = spike.GetComponent<Renderer>();
            if (renderer != null)
            {
                var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                material.color = spikeColor;
                renderer.sharedMaterial = material;
            }
        }

        var basePlate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        basePlate.name = "Base";
        basePlate.transform.SetParent(root.transform, false);
        basePlate.transform.localPosition = new Vector3(0f, 0.03f, 0f);
        basePlate.transform.localScale = new Vector3(baseSize.x, 0.06f, baseSize.z);
        basePlate.isStatic = true;
        Object.DestroyImmediate(basePlate.GetComponent<Collider>());

        var baseRenderer = basePlate.GetComponent<Renderer>();
        if (baseRenderer != null)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(0.25f, 0.22f, 0.22f);
            baseRenderer.sharedMaterial = material;
        }

        return trap;
    }
}
