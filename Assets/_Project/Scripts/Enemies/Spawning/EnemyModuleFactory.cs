using UnityEngine;

public static class EnemyModuleFactory
{
    public static GameObject SpawnEnemy(EnemyKind kind, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (!EnemyCatalog.TryGet(kind, out EnemyData data))
        {
            Debug.LogWarning($"[EnemyModule] Cannot spawn unknown enemy: {kind}");
            return null;
        }

        GameObject root;
        if (data.EnemyPrefab != null)
        {
            root = Object.Instantiate(data.EnemyPrefab, position, rotation, parent);
            root.name = $"Enemy_{kind}";
            var controller = root.GetComponent<EnemyController>() ?? root.AddComponent<EnemyController>();
            controller.Initialize(data);
            EnsureHighlightComponents(root);

            GameplayLayers.TrySetEnemyLayer(root);
            return root;
        }

        root = new GameObject($"Enemy_{kind}");
        root.transform.SetParent(parent, false);
        root.transform.SetPositionAndRotation(position, rotation);

        ConfigurePhysics(root, data);
        Transform visualRoot = EnemyPlaceholderVisualBuilder.BuildVisual(data, root.transform);

        var enemyController = root.AddComponent<EnemyController>();
        enemyController.Initialize(data);
        enemyController.BindVisual(visualRoot);
        EnsureHighlightComponents(root);

        GameplayLayers.TrySetEnemyLayer(root);
        return root;
    }

    static void EnsureHighlightComponents(GameObject root)
    {
        if (root.GetComponent<EnemySurfaceHighlighter>() == null)
        {
            root.AddComponent<EnemySurfaceHighlighter>();
        }

        if (root.GetComponent<EnemyTargetHighlighter>() == null)
        {
            root.AddComponent<EnemyTargetHighlighter>();
        }
    }

    static void ConfigurePhysics(GameObject root, EnemyData data)
    {
        var body = root.AddComponent<Rigidbody>();
        body.mass = 3f;
        body.useGravity = true;
        body.linearDamping = 2f;
        body.angularDamping = 4f;
        body.constraints = RigidbodyConstraints.FreezeRotation;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.Continuous;

        var capsule = root.AddComponent<CapsuleCollider>();
        Vector3 scale = data.PlaceholderScale;
        capsule.height = Mathf.Max(0.8f, scale.y);
        capsule.radius = Mathf.Max(0.25f, Mathf.Max(scale.x, scale.z) * 0.35f);
        capsule.center = new Vector3(0f, capsule.height * 0.5f, 0f);

    }
}
