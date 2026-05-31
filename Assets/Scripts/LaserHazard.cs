using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class LaserHazard : MonoBehaviour
{
    [SerializeField] float damage = 12f;
    [SerializeField] float hazardCooldown = 1f;
    [SerializeField] float retriggerDelay = 0.5f;

    float nextDamageTime;
    bool armed = true;

    void Awake()
    {
        ConfigureVisual();
        ConfigureCollider();
    }

    void ConfigureCollider()
    {
        var box = GetComponent<BoxCollider>();
        box.isTrigger = true;
    }

    void ConfigureVisual()
    {
        var renderer = GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.color = new Color(1f, 0.15f, 0.1f, 0.85f);
        material.SetColor("_EmissionColor", new Color(1f, 0.1f, 0.05f) * 1.8f);
        material.EnableKeyword("_EMISSION");
        renderer.sharedMaterial = material;
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

    public static LaserHazard Create(Transform parent, string name, Vector3 center, Vector3 size)
    {
        var laser = GameObject.CreatePrimitive(PrimitiveType.Cube);
        laser.name = name;
        laser.transform.SetParent(parent, false);
        laser.transform.position = center;
        laser.transform.localScale = size;
        laser.isStatic = true;
        return laser.AddComponent<LaserHazard>();
    }
}
