using UnityEngine;

public static class Level4PlaceholderBuilder
{
    const float FloorTop = 0.125f;

    public static GameObject Generate(bool removeOld = true)
    {
        if (removeOld)
        {
            DestroyIfExists("Level4Arena");
        }

        var root = new GameObject("Level4Arena");
        var floors = CreateChild(root.transform, "Floors");
        var markers = CreateChild(root.transform, "Markers");

        var floorColor = new Color(0.5f, 0.54f, 0.62f);
        var accentColor = new Color(0.35f, 0.78f, 0.95f);

        CreateBlock(floors, "MainFloor", new Vector3(0f, FloorTop, 0f), new Vector3(36f, 0.25f, 36f), floorColor);
        CreateBlock(floors, "CenterPad", new Vector3(0f, FloorTop + 0.02f, 0f), new Vector3(8f, 0.08f, 8f), accentColor * 0.85f);

        CreateSign(markers, "WelcomeSign", new Vector3(0f, FloorTop + 2.2f, 6f), "第四关\n欢迎进入 Scene4");

        var player = GameObject.Find("Player");
        if (player != null)
        {
            var cc = player.GetComponent<CharacterController>();
            Vector3 spawn = cc != null
                ? PlayerAnchorUtility.GetSpawnPositionForGround(Vector3.zero, FloorTop + 0.125f, cc)
                : new Vector3(0f, FloorTop + 1.25f, 0f);
            player.transform.position = spawn;
            player.transform.rotation = Quaternion.identity;
        }

        return root;
    }

    static void CreateSign(Transform parent, string name, Vector3 position, string text)
    {
        var sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sign.name = name;
        sign.transform.SetParent(parent, false);
        sign.transform.position = position;
        sign.transform.localScale = new Vector3(5f, 1f, 0.12f);
        sign.isStatic = true;
        Object.DestroyImmediate(sign.GetComponent<Collider>());

        var renderer = sign.GetComponent<Renderer>();
        if (renderer != null)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(0.14f, 0.18f, 0.24f);
            renderer.sharedMaterial = material;
        }
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
