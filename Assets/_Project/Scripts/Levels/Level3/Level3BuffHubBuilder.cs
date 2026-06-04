using UnityEngine;

public static class Level3BuffHubBuilder
{
    const float FloorTop = 0.125f;
    const float PathLength = 28f;
    const float PathWidth = 4f;
    const float HubSize = 18f;

    public static Vector3 HubSpawn => new Vector3(0f, FloorTop + 0.125f, 0f);

    public static GameObject Generate(bool removeOld = true)
    {
        if (removeOld)
        {
            DestroyIfExists("BuffHub");
        }

        var root = new GameObject("BuffHub");
        var floors = CreateChild(root.transform, "Floors");
        var markers = CreateChild(root.transform, "Markers");
        var hazards = CreateChild(root.transform, "Hazards");
        var buffs = CreateChild(root.transform, "BuffBubbles");
        var returns = CreateChild(root.transform, "ReturnZones");

        var hubColor = new Color(0.58f, 0.6f, 0.64f);
        var pathColors = new[]
        {
            new Color(0.45f, 0.85f, 0.55f),
            new Color(0.45f, 0.72f, 0.95f),
            new Color(0.95f, 0.82f, 0.35f),
            new Color(0.78f, 0.58f, 0.95f),
        };

        CreateBlock(floors, "HubFloor", HubSpawn, new Vector3(HubSize, 0.25f, HubSize), hubColor);

        BuildPath(floors, markers, hazards, buffs, returns, "+Z 回血", Vector3.forward, pathColors[0], BuffType.Heal, true, false);
        BuildPath(floors, markers, hazards, buffs, returns, "+X 加速", Vector3.right, pathColors[1], BuffType.SpeedBoost, false, false);
        BuildPath(floors, markers, hazards, buffs, returns, "-Z 无限精力", Vector3.back, pathColors[2], BuffType.InfiniteStamina, false, false);
        BuildPath(floors, markers, hazards, buffs, returns, "-X 护盾", Vector3.left, pathColors[3], BuffType.Shield, false, true);

        CreateSign(markers, "HubSign", HubSpawn + Vector3.up * 2.5f, "Buff 教学关：选择一条路体验不同增益");

        var player = GameObject.Find("Player");
        CharacterController cc = null;
        if (player != null)
        {
            cc = player.GetComponent<CharacterController>();
        }

        HubReturnZone.SetHubSpawn(
            cc != null
                ? PlayerAnchorUtility.GetSpawnPositionForGround(Vector3.zero, FloorTop + 0.125f, cc)
                : new Vector3(0f, FloorTop + 0.125f, 0f));

        if (player != null)
        {
            if (cc != null)
            {
                player.transform.position = PlayerAnchorUtility.GetSpawnPositionForGround(
                    Vector3.zero,
                    FloorTop + 0.125f,
                    cc);
            }
            else
            {
                player.transform.position = HubSpawn + Vector3.up * 1f;
            }

            player.transform.rotation = Quaternion.identity;
            EnsurePlayerBuffController(player);
        }

        Vector3 hubSpawnGround = cc != null
            ? PlayerAnchorUtility.GetSpawnPositionForGround(Vector3.zero, FloorTop + 0.125f, cc)
            : HubSpawn + Vector3.up * 1f;
        EnsureLevel3Systems(root.transform, hubSpawnGround);

        return root;
    }

    static void EnsureLevel3Systems(Transform root, Vector3 hubSpawnGround)
    {
        var systemsGo = GameObject.Find("Level3Systems");
        if (systemsGo == null)
        {
            systemsGo = new GameObject("Level3Systems");
            systemsGo.transform.SetParent(root, false);
        }

        if (systemsGo.GetComponent<BuffCollectionTracker>() == null)
        {
            systemsGo.AddComponent<BuffCollectionTracker>();
        }

        var portal = systemsGo.GetComponent<HubAdvancePortal>();
        if (portal == null)
        {
            portal = systemsGo.AddComponent<HubAdvancePortal>();
        }

        Vector3 portalPos = hubSpawnGround + new Vector3(0f, 0.1f, 2.2f);
        portal.Configure(portalPos);
    }

    static void BuildPath(
        Transform floors,
        Transform markers,
        Transform hazards,
        Transform buffs,
        Transform returns,
        string label,
        Vector3 direction,
        Color pathColor,
        BuffType buffType,
        bool guaranteedTrap,
        bool lateTrap)
    {
        float groundY = FloorTop;
        bool alongX = Mathf.Abs(direction.x) > Mathf.Abs(direction.z);
        float signedAxis = alongX ? Mathf.Sign(direction.x) : Mathf.Sign(direction.z);
        Vector3 pathCenter = alongX
            ? new Vector3(signedAxis * (HubSize * 0.5f + PathLength * 0.5f), groundY, 0f)
            : new Vector3(0f, groundY, signedAxis * (HubSize * 0.5f + PathLength * 0.5f));
        Vector3 pathScale = alongX
            ? new Vector3(PathLength, 0.25f, PathWidth)
            : new Vector3(PathWidth, 0.25f, PathLength);
        CreateBlock(floors, $"Path_{label}", pathCenter, pathScale, pathColor);

        Vector3 startSignPos = direction.normalized * (HubSize * 0.5f + 2f) + Vector3.up * (FloorTop + 1.5f);
        CreateSign(markers, $"Sign_{label}", startSignPos, GetPathLabel(buffType));

        if (guaranteedTrap)
        {
            Vector3 trapPos = direction.normalized * (HubSize * 0.5f + 10f) + Vector3.up * groundY;
            Vector3 trapSize = alongX
                ? new Vector3(2.5f, 0.5f, PathWidth * 0.8f)
                : new Vector3(PathWidth * 0.8f, 0.5f, 2.5f);
            GuaranteedDamageZone.Create(hazards, $"Trap_{label}", trapPos + Vector3.up * 0.35f, trapSize, 1f);
            CreateSign(markers, $"TrapSign_{label}", trapPos + Vector3.up * 1.8f, "必中扣血区：通过后再拾取回血 Buff");
        }

        if (lateTrap)
        {
            Vector3 trapPos = direction.normalized * (HubSize * 0.5f + 18f) + Vector3.up * groundY;
            Vector3 trapSize = alongX
                ? new Vector3(2f, 0.4f, PathWidth * 0.7f)
                : new Vector3(PathWidth * 0.7f, 0.4f, 2f);
            SpikeTrap.Create(hazards, $"Trap_{label}", trapPos, trapSize, 4, 22f);
        }

        Vector3 endPos = direction.normalized * (HubSize * 0.5f + PathLength - 1.5f);
        float endGround = FloorTop + 0.125f;
        Vector3 endScale = alongX
            ? new Vector3(3f, 0.25f, PathWidth * 0.9f)
            : new Vector3(PathWidth * 0.9f, 0.25f, 3f);
        CreateBlock(markers, $"End_{label}", new Vector3(endPos.x, groundY, endPos.z), endScale, pathColor * 0.85f);

        Vector3 bubblePos = new Vector3(endPos.x, endGround + 1f, endPos.z);
        BuffBubble.Create(buffs, $"Bubble_{label}", bubblePos, buffType);

        Vector3 returnPos = new Vector3(endPos.x, endGround + 0.5f, endPos.z);
        HubReturnZone.Create(returns, $"Return_{label}", returnPos, new Vector3(3f, 2.5f, 3f));

        CreateSign(markers, $"ReturnSign_{label}", returnPos + Vector3.up * 1.6f, "终点：拾取 Buff 后按 R 返回原点");
    }

    static string GetPathLabel(BuffType type)
    {
        switch (type)
        {
            case BuffType.Heal:
                return "路 1：回血 Buff";
            case BuffType.SpeedBoost:
                return "路 2：加速 Buff";
            case BuffType.InfiniteStamina:
                return "路 3：7 秒无限精力";
            case BuffType.Shield:
                return "路 4：护盾 Buff";
            default:
                return "Buff 路";
        }
    }

    static void EnsurePlayerBuffController(GameObject player)
    {
        if (player.GetComponent<PlayerBuffController>() == null)
        {
            player.AddComponent<PlayerBuffController>();
        }

        if (player.GetComponent<PlayerShieldVisual>() == null)
        {
            player.AddComponent<PlayerShieldVisual>();
        }
    }

    static void CreateSign(Transform parent, string name, Vector3 position, string text)
    {
        var sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sign.name = name;
        sign.transform.SetParent(parent, false);
        sign.transform.position = position;
        sign.transform.localScale = new Vector3(3.6f, 0.9f, 0.12f);
        sign.isStatic = true;
        Object.DestroyImmediate(sign.GetComponent<Collider>());

        var renderer = sign.GetComponent<Renderer>();
        if (renderer != null)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(0.15f, 0.18f, 0.22f);
            renderer.sharedMaterial = material;
        }

        var label = new GameObject("TextHint");
        label.transform.SetParent(sign.transform, false);
        label.transform.localPosition = Vector3.forward * -0.8f;
    }

    static GameObject CreateBlock(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent, false);
        cube.transform.position = position;
        cube.transform.localScale = scale;
        cube.isStatic = true;

        var renderer = cube.GetComponent<Renderer>();
        if (renderer != null)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = color;
            renderer.sharedMaterial = material;
        }

        return cube;
    }

    static Transform CreateChild(Transform parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    static void DestroyIfExists(string objectName)
    {
        var obj = GameObject.Find(objectName);
        if (obj != null)
        {
            Object.DestroyImmediate(obj);
        }
    }
}
