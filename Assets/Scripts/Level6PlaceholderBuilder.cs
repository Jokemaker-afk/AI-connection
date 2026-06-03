using UnityEngine;

public static class Level6PlaceholderBuilder
{
    const float FloorTop = 0.125f;

    public static GameObject Generate(bool removeOld = true)
    {
        if (removeOld)
        {
            DestroyIfExists("Level6Arena");
        }

        var root = new GameObject("Level6Arena");
        var terrain = CreateChild(root.transform, "Terrain");
        var markers = CreateChild(root.transform, "Markers");

        CreateBlock(terrain, "MainFloor", new Vector3(0f, FloorTop, 0f), new Vector3(24f, 0.25f, 24f), new Color(0.36f, 0.48f, 0.58f));
        CreateWorldLabel(markers, "WelcomeLabel", new Vector3(0f, FloorTop + 2f, -4f), "第六关占位场景");

        var player = GameObject.Find("Player");
        if (player != null)
        {
            GameplayHudBootstrap.BindAll();
            var cc = player.GetComponent<CharacterController>();
            Vector3 spawn = cc != null
                ? PlayerAnchorUtility.GetSpawnPositionForGround(new Vector3(0f, 0f, -2f), FloorTop + 0.125f, cc)
                : new Vector3(0f, FloorTop + 1.25f, -2f);
            player.transform.position = spawn;
            player.transform.rotation = Quaternion.identity;
        }

        return root;
    }

    static void CreateWorldLabel(Transform parent, string name, Vector3 position, string text)
    {
        var labelRoot = new GameObject(name);
        labelRoot.transform.SetParent(parent, false);
        labelRoot.transform.position = position;
        ItemWorldLabel.Create(labelRoot.transform, text, Vector3.zero, 0.12f);
    }

    static GameObject CreateBlock(Transform parent, string blockName, Vector3 position, Vector3 scale, Color color)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = blockName;
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

    static Transform CreateChild(Transform parent, string childName)
    {
        var go = new GameObject(childName);
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
