using UnityEngine;

/// <summary>
/// Builds and maintains player visual modules under PlayerVisualRoot.
/// Gameplay logic stays on PlayerRoot; visuals are swappable modules only.
/// </summary>
public static class PlayerVisualBuilder
{
    public const string VisualRootName = "PlayerVisualRoot";
    public const string DirectionalModelName = "DirectionalPlaceholderModel";
    public const string LegacyCapsuleVisualName = "LegacyCapsuleVisual_Disabled";
    public const string LegacyVisualLegacyName = "Visual";
    public const string FirstPersonToolSocketName = "FirstPersonToolSocket";
    public const string ThirdPersonToolSocketName = "ThirdPersonToolSocket";

    public struct BuildResult
    {
        public Transform VisualRoot;
        public Transform DirectionalModelRoot;
        public Transform LegacyCapsuleRoot;
        public Transform FirstPersonToolSocket;
        public Transform ThirdPersonToolSocket;
        public Renderer[] ActiveRenderers;
        public PlayerVisualSwitcher VisualSwitcher;
    }

    public static BuildResult EnsurePlayerVisual(GameObject playerRoot)
    {
        if (playerRoot == null)
        {
            return default;
        }

        var controller = playerRoot.GetComponent<CharacterController>();
        if (controller == null)
        {
            return default;
        }

        Transform visualRootTransform = EnsureVisualRoot(playerRoot);
        Transform legacyRoot = PreserveLegacyCapsuleVisual(playerRoot, visualRootTransform, controller);
        Transform directionalRoot = EnsureDirectionalBlockModel(visualRootTransform, controller);
        Transform firstPersonSocket = EnsurePlayerRootSocket(
            playerRoot.transform,
            FirstPersonToolSocketName,
            new Vector3(0.28f, 1.05f, 0.42f),
            new Vector3(10f, -12f, 0f));
        Transform thirdPersonSocket = EnsurePlayerRootSocket(
            playerRoot.transform,
            ThirdPersonToolSocketName,
            new Vector3(0.45f, 0.5f, 0.35f),
            new Vector3(10f, -25f, 8f));

        var visualRootComponent = visualRootTransform.GetComponent<PlayerVisualRoot>();
        if (visualRootComponent == null)
        {
            visualRootComponent = visualRootTransform.gameObject.AddComponent<PlayerVisualRoot>();
        }

        visualRootComponent.BindRoots(directionalRoot, legacyRoot);

        PlayerVisualSwitcher switcher = EnsureVisualSwitcher(playerRoot);
        switcher.EnsureDefaultVisual();

        return new BuildResult
        {
            VisualRoot = visualRootTransform,
            DirectionalModelRoot = directionalRoot,
            LegacyCapsuleRoot = legacyRoot,
            FirstPersonToolSocket = firstPersonSocket,
            ThirdPersonToolSocket = thirdPersonSocket,
            ActiveRenderers = switcher.GetActiveRenderers(),
            VisualSwitcher = switcher,
        };
    }

    static Transform EnsureVisualRoot(GameObject playerRoot)
    {
        Transform visualRoot = playerRoot.transform.Find(VisualRootName);
        if (visualRoot != null)
        {
            return visualRoot;
        }

        var visualGo = new GameObject(VisualRootName);
        visualRoot = visualGo.transform;
        visualRoot.SetParent(playerRoot.transform, false);
        visualRoot.localPosition = Vector3.zero;
        visualRoot.localRotation = Quaternion.identity;
        visualRoot.localScale = Vector3.one;
        return visualRoot;
    }

    static PlayerVisualSwitcher EnsureVisualSwitcher(GameObject playerRoot)
    {
        var switcher = playerRoot.GetComponent<PlayerVisualSwitcher>();
        if (switcher == null)
        {
            switcher = playerRoot.AddComponent<PlayerVisualSwitcher>();
        }

        return switcher;
    }

    static Transform PreserveLegacyCapsuleVisual(GameObject playerRoot, Transform visualRoot, CharacterController controller)
    {
        Transform legacy = visualRoot.Find(LegacyCapsuleVisualName);
        if (legacy == null)
        {
            legacy = playerRoot.transform.Find(LegacyCapsuleVisualName);
        }

        if (legacy == null)
        {
            legacy = FindLegacyVisualChild(playerRoot.transform);
        }

        if (legacy == null)
        {
            legacy = CreateLegacyCapsuleBackup(visualRoot, controller).transform;
        }
        else
        {
            legacy.SetParent(visualRoot, false);
            legacy.name = LegacyCapsuleVisualName;
        }

        legacy.localPosition = new Vector3(0f, controller.center.y, 0f);
        legacy.localRotation = Quaternion.identity;
        legacy.localScale = Vector3.one;

        EnsureVisualModule(
            legacy.gameObject,
            PlayerVisualKind.LegacyCapsule,
            PlayerVisualProfile.CreateBuiltin(PlayerVisualKind.LegacyCapsule),
            active: false);

        return legacy;
    }

    static Transform FindLegacyVisualChild(Transform playerRoot)
    {
        Transform legacyNamed = playerRoot.Find(LegacyVisualLegacyName);
        if (legacyNamed != null)
        {
            return legacyNamed;
        }

        MeshFilter rootMesh = playerRoot.GetComponent<MeshFilter>();
        MeshRenderer rootRenderer = playerRoot.GetComponent<MeshRenderer>();
        if (rootMesh != null || rootRenderer != null)
        {
            var legacyGo = new GameObject(LegacyCapsuleVisualName);
            legacyGo.transform.SetParent(playerRoot, false);
            legacyGo.transform.localPosition = Vector3.up;
            legacyGo.transform.localRotation = Quaternion.identity;
            legacyGo.transform.localScale = Vector3.one;

            if (rootMesh != null)
            {
                var meshFilter = legacyGo.AddComponent<MeshFilter>();
                meshFilter.sharedMesh = rootMesh.sharedMesh;
                DestroyComponent(rootMesh);
            }

            if (rootRenderer != null)
            {
                var meshRenderer = legacyGo.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterials = rootRenderer.sharedMaterials;
                DestroyComponent(rootRenderer);
            }

            return legacyGo.transform;
        }

        return null;
    }

    static GameObject CreateLegacyCapsuleBackup(Transform visualRoot, CharacterController controller)
    {
        var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        capsule.name = LegacyCapsuleVisualName;
        capsule.transform.SetParent(visualRoot, false);
        capsule.transform.localPosition = new Vector3(0f, controller.center.y, 0f);
        capsule.transform.localRotation = Quaternion.identity;
        capsule.transform.localScale = new Vector3(controller.radius * 2f, controller.height * 0.5f, controller.radius * 2f);

        Collider collider = capsule.GetComponent<Collider>();
        if (collider != null)
        {
            DestroyComponent(collider);
        }

        Renderer renderer = capsule.GetComponent<Renderer>();
        if (renderer != null)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(0.35f, 0.55f, 0.85f, 0.85f);
            renderer.sharedMaterial = material;
        }

        return capsule;
    }

    static Transform EnsureDirectionalBlockModel(Transform visualRoot, CharacterController controller)
    {
        Transform modelRoot = visualRoot.Find(DirectionalModelName);
        if (modelRoot != null && modelRoot.Find("ModelContent") == null)
        {
            DestroyObject(modelRoot.gameObject);
            modelRoot = null;
        }

        if (modelRoot == null)
        {
            modelRoot = BuildDirectionalBlockModel(visualRoot, controller);
        }

        EnsureVisualModule(
            modelRoot.gameObject,
            PlayerVisualKind.DirectionalBlock,
            PlayerVisualProfile.CreateBuiltin(PlayerVisualKind.DirectionalBlock),
            active: true);

        return modelRoot;
    }

    static Transform BuildDirectionalBlockModel(Transform visualRoot, CharacterController controller)
    {
        var modelGo = new GameObject(DirectionalModelName);
        var modelRoot = modelGo.transform;
        modelRoot.SetParent(visualRoot, false);
        modelRoot.localPosition = Vector3.zero;
        modelRoot.localRotation = Quaternion.identity;
        modelRoot.localScale = Vector3.one;

        Transform modelContent = new GameObject("ModelContent").transform;
        modelContent.SetParent(modelRoot, false);
        modelContent.localPosition = controller.center;
        modelContent.localRotation = Quaternion.identity;
        modelContent.localScale = Vector3.one;

        Color bodyColor = new Color(0.42f, 0.58f, 0.82f);
        Color headColor = new Color(0.92f, 0.86f, 0.78f);
        Color frontColor = new Color(0.95f, 0.78f, 0.22f);
        Color backColor = new Color(0.28f, 0.32f, 0.38f);
        Color sideColor = new Color(0.55f, 0.68f, 0.42f);
        Color armColor = new Color(0.38f, 0.52f, 0.72f);
        Color handColor = new Color(0.72f, 0.58f, 0.42f);

        CreatePart(modelContent, "Body", Vector3.zero, new Vector3(0.62f, 1.05f, 0.42f), bodyColor);
        CreatePart(modelContent, "Head", new Vector3(0f, 0.78f, 0f), new Vector3(0.34f, 0.34f, 0.34f), headColor);
        CreatePart(modelContent, "FrontMarker", new Vector3(0f, 0.08f, 0.28f), new Vector3(0.18f, 0.14f, 0.22f), frontColor);
        CreatePart(modelContent, "FrontArrow", new Vector3(0f, 0.08f, 0.42f), new Vector3(0.1f, 0.08f, 0.16f), frontColor * 1.1f);
        CreatePart(modelContent, "BackMarker", new Vector3(0f, 0.05f, -0.24f), new Vector3(0.22f, 0.28f, 0.12f), backColor);
        CreatePart(modelContent, "LeftMarker", new Vector3(-0.28f, 0.06f, 0f), new Vector3(0.1f, 0.22f, 0.18f), sideColor);
        CreatePart(modelContent, "RightMarker", new Vector3(0.28f, 0.06f, 0f), new Vector3(0.1f, 0.22f, 0.18f), sideColor);
        CreatePart(modelContent, "LeftArm", new Vector3(-0.38f, 0.12f, 0.04f), new Vector3(0.14f, 0.42f, 0.14f), armColor);
        CreatePart(modelContent, "RightArm", new Vector3(0.38f, 0.12f, 0.04f), new Vector3(0.14f, 0.42f, 0.14f), armColor);
        CreatePart(modelContent, "RightHandToolSide", new Vector3(0.48f, 0.02f, 0.12f), new Vector3(0.12f, 0.12f, 0.12f), handColor);

        return modelRoot;
    }

    static void EnsureVisualModule(GameObject target, PlayerVisualKind kind, PlayerVisualProfile profile, bool active)
    {
        var module = target.GetComponent<PlayerVisualModule>();
        if (module == null)
        {
            module = target.AddComponent<PlayerVisualModule>();
        }

        module.Configure(profile, active);
    }

    static Transform EnsurePlayerRootSocket(Transform playerRoot, string socketName, Vector3 localPosition, Vector3 localEuler)
    {
        Transform existing = playerRoot.Find(socketName);
        if (existing != null)
        {
            existing.localPosition = localPosition;
            existing.localEulerAngles = localEuler;
            return existing;
        }

        Transform nested = FindDeepChild(playerRoot, socketName);
        if (nested != null && nested.parent != playerRoot)
        {
            nested.SetParent(playerRoot, false);
        }

        if (nested != null)
        {
            nested.localPosition = localPosition;
            nested.localEulerAngles = localEuler;
            nested.localScale = Vector3.one;
            return nested;
        }

        var socketGo = new GameObject(socketName);
        var socket = socketGo.transform;
        socket.SetParent(playerRoot, false);
        socket.localPosition = localPosition;
        socket.localEulerAngles = localEuler;
        socket.localScale = Vector3.one;
        return socket;
    }

    static Transform FindDeepChild(Transform parent, string childName)
    {
        if (parent == null)
        {
            return null;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == childName)
            {
                return child;
            }

            Transform nested = FindDeepChild(child, childName);
            if (nested != null)
            {
                return nested;
            }
        }

        return null;
    }

    static void CreatePart(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Color color)
    {
        var part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.name = name;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localScale = localScale;
        part.transform.localRotation = Quaternion.identity;

        Collider collider = part.GetComponent<Collider>();
        if (collider != null)
        {
            DestroyComponent(collider);
        }

        Renderer renderer = part.GetComponent<Renderer>();
        if (renderer != null)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = color;
            renderer.sharedMaterial = material;
        }
    }

    static void DestroyObject(Object target)
    {
        if (target == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Object.Destroy(target);
        }
        else
        {
            Object.DestroyImmediate(target);
        }
    }

    static void DestroyComponent(Component component)
    {
        if (component == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Object.Destroy(component);
        }
        else
        {
            Object.DestroyImmediate(component);
        }
    }
}
