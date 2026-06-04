using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class GuaranteedDamageZone : MonoBehaviour
{
    [SerializeField] float healthLossPercent = 1f;
    [SerializeField] bool oneShot = true;

    bool triggered;

    void Awake()
    {
        var box = GetComponent<BoxCollider>();
        box.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (oneShot && triggered)
        {
            return;
        }

        if (!other.TryGetComponent<PlayerStats>(out var stats))
        {
            return;
        }

        triggered = true;
        stats.ApplyGuaranteedHealthLoss(healthLossPercent);
    }

    public static GuaranteedDamageZone Create(Transform parent, string name, Vector3 position, Vector3 size, float percent = 1f)
    {
        var zone = GameObject.CreatePrimitive(PrimitiveType.Cube);
        zone.name = name;
        zone.transform.SetParent(parent, false);
        zone.transform.position = position;
        zone.transform.localScale = size;
        zone.isStatic = true;
        Object.DestroyImmediate(zone.GetComponent<BoxCollider>());

        var box = zone.AddComponent<BoxCollider>();
        box.isTrigger = true;

        var renderer = zone.GetComponent<Renderer>();
        if (renderer != null)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(0.95f, 0.2f, 0.2f, 0.75f);
            material.SetColor("_EmissionColor", new Color(1f, 0.15f, 0.15f) * 0.8f);
            material.EnableKeyword("_EMISSION");
            renderer.sharedMaterial = material;
        }

        var hazard = zone.AddComponent<GuaranteedDamageZone>();
        hazard.healthLossPercent = percent;
        return hazard;
    }
}
