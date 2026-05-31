using UnityEngine;

public enum RewardTier
{
    Bronze = 10,
    Silver = 25,
    Gold = 50,
    Diamond = 100
}

[RequireComponent(typeof(BoxCollider))]
public class RewardCrate : MonoBehaviour
{
    [SerializeField] RewardTier tier = RewardTier.Bronze;
    [SerializeField] float bobAmplitude = 0.18f;
    [SerializeField] float bobSpeed = 2.4f;
    [SerializeField] float spinSpeed = 45f;

    Vector3 basePosition;
    bool collected;

    public int PointValue => (int)tier;

    void Awake()
    {
        basePosition = transform.position;
        ConfigureCollider();
        ApplyTierColor();
    }

    void ConfigureCollider()
    {
        var box = GetComponent<BoxCollider>();
        box.isTrigger = true;
    }

    void ApplyTierColor()
    {
        var renderer = GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.color = GetTierColor(tier);
        material.SetColor("_EmissionColor", GetTierColor(tier) * 0.35f);
        material.EnableKeyword("_EMISSION");
        renderer.sharedMaterial = material;
    }

    public static Color GetTierColor(RewardTier rewardTier)
    {
        switch (rewardTier)
        {
            case RewardTier.Bronze:
                return new Color(0.72f, 0.42f, 0.18f);
            case RewardTier.Silver:
                return new Color(0.78f, 0.82f, 0.88f);
            case RewardTier.Gold:
                return new Color(0.95f, 0.78f, 0.15f);
            case RewardTier.Diamond:
                return new Color(0.35f, 0.85f, 0.95f);
            default:
                return Color.white;
        }
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

    void OnTriggerEnter(Collider other)
    {
        if (collected)
        {
            return;
        }

        if (!other.TryGetComponent<PlayerController>(out _))
        {
            return;
        }

        Collect(other.gameObject);
    }

    void Collect(GameObject player)
    {
        collected = true;

        if (player.TryGetComponent<GameScore>(out var gameScore))
        {
            gameScore.AddScore(PointValue);
        }

        Destroy(gameObject);
    }

    public void Initialize(RewardTier rewardTier, Vector3 spawnPosition)
    {
        tier = rewardTier;
        basePosition = spawnPosition;
        transform.position = spawnPosition;
        ApplyTierColor();
    }

    public static RewardCrate Create(Transform parent, string name, Vector3 position, RewardTier rewardTier, float size = 0.85f)
    {
        var root = GameObject.CreatePrimitive(PrimitiveType.Cube);
        root.name = name;
        root.transform.SetParent(parent, false);
        root.transform.position = position;
        root.transform.localScale = Vector3.one * size;
        root.isStatic = false;

        var questionMark = GameObject.CreatePrimitive(PrimitiveType.Cube);
        questionMark.name = "Mark";
        questionMark.transform.SetParent(root.transform, false);
        questionMark.transform.localPosition = new Vector3(0f, 0f, 0.51f);
        questionMark.transform.localScale = new Vector3(0.35f, 0.45f, 0.08f);
        Object.DestroyImmediate(questionMark.GetComponent<Collider>());

        var markRenderer = questionMark.GetComponent<Renderer>();
        if (markRenderer != null)
        {
            var markMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            markMat.color = new Color(0.15f, 0.12f, 0.08f);
            markRenderer.sharedMaterial = markMat;
        }

        var crate = root.AddComponent<RewardCrate>();
        crate.Initialize(rewardTier, position);
        return crate;
    }
}
