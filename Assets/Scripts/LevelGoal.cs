using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class LevelGoal : MonoBehaviour
{
    [SerializeField] bool oneShot = true;

    bool triggered;

    void Awake()
    {
        var box = GetComponent<BoxCollider>();
        box.isTrigger = true;
        ConfigureVisual();
    }

    void ConfigureVisual()
    {
        var renderer = GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.color = new Color(0.2f, 0.95f, 0.45f, 0.85f);
        material.SetColor("_EmissionColor", new Color(0.15f, 0.85f, 0.35f) * 1.5f);
        material.EnableKeyword("_EMISSION");
        renderer.sharedMaterial = material;
    }

    void OnTriggerEnter(Collider other)
    {
        if (oneShot && triggered)
        {
            return;
        }

        if (!other.TryGetComponent<PlayerController>(out _))
        {
            return;
        }

        triggered = true;
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.NotifyGoalReached();
        }
    }

    public static LevelGoal Create(Transform parent, string name, Vector3 position, Vector3 size)
    {
        var goal = GameObject.CreatePrimitive(PrimitiveType.Cube);
        goal.name = name;
        goal.transform.SetParent(parent, false);
        goal.transform.position = position;
        goal.transform.localScale = size;
        goal.isStatic = true;
        Object.DestroyImmediate(goal.GetComponent<BoxCollider>());
        goal.AddComponent<BoxCollider>().isTrigger = true;
        return goal.AddComponent<LevelGoal>();
    }
}
