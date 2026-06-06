using UnityEngine;

/// <summary>Shared map helpers for fixed and seeded Level8 builders.</summary>
public static class Level8MapUtility
{
    public static float MapHalfExtent => Level8GenerationFlags.MapSize * 0.5f;

    public static Vector3 GetChunkCenter(int gridX, int gridZ)
    {
        float halfChunk = Level8GenerationFlags.ChunkSize * 0.5f;
        float x = -MapHalfExtent + gridX * Level8GenerationFlags.ChunkSize + halfChunk;
        float z = -MapHalfExtent + gridZ * Level8GenerationFlags.ChunkSize + halfChunk;
        return new Vector3(x, Level8ChunkPlaceholderBuilder.FloorTop, z);
    }

    public static Vector3 GetChunkCenter(Vector2Int grid)
    {
        return GetChunkCenter(grid.x, grid.y);
    }

    public static Vector3 GetSpawnPosition(Vector3 spawnChunkCenter)
    {
        return spawnChunkCenter + new Vector3(0f, Level8ChunkPlaceholderBuilder.FloorTop + 0.05f, -10f);
    }

    public static void CreateBoundaryFloor(Transform parent)
    {
        var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "BoundaryFloor";
        floor.transform.SetParent(parent, false);
        floor.transform.position = new Vector3(0f, Level8ChunkPlaceholderBuilder.FloorTop - 0.08f, 0f);
        floor.transform.localScale = new Vector3(Level8GenerationFlags.MapSize, 0.12f, Level8GenerationFlags.MapSize);
        floor.isStatic = true;

        Renderer renderer = floor.GetComponent<Renderer>();
        if (renderer != null)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(0.22f, 0.28f, 0.24f, 0.85f);
            renderer.sharedMaterial = material;
        }

        floor.AddComponent<StaticPlacementGround>();
    }

    public static void CreateBoundaryMarkers(Transform parent)
    {
        float y = Level8ChunkPlaceholderBuilder.FloorTop + 0.05f;
        float edge = MapHalfExtent - 2f;
        CreateEdgeMarker(parent, "Boundary_N", new Vector3(0f, y, edge));
        CreateEdgeMarker(parent, "Boundary_S", new Vector3(0f, y, -edge));
        CreateEdgeMarker(parent, "Boundary_E", new Vector3(edge, y, 0f));
        CreateEdgeMarker(parent, "Boundary_W", new Vector3(-edge, y, 0f));
    }

    public static void EnsurePlayerSpawn(Vector3 spawnChunkCenter)
    {
        var spawnGo = GameObject.Find("PlayerSpawnPoint") ?? new GameObject("PlayerSpawnPoint");
        spawnGo.transform.position = GetSpawnPosition(spawnChunkCenter);

        if (spawnGo.GetComponent<PlayerSpawnPoint>() == null)
        {
            spawnGo.AddComponent<PlayerSpawnPoint>();
        }
    }

    public static void PositionExistingPlayer(Vector3 spawnChunkCenter)
    {
        var player = GameObject.Find("Player");
        if (player == null)
        {
            return;
        }

        var controller = player.GetComponent<CharacterController>();
        Vector3 spawn = controller != null
            ? PlayerAnchorUtility.GetSpawnPositionForGround(
                spawnChunkCenter + new Vector3(0f, 0f, -10f),
                Level8ChunkPlaceholderBuilder.FloorTop + 0.125f,
                controller)
            : spawnChunkCenter + new Vector3(0f, Level8ChunkPlaceholderBuilder.FloorTop + 1.25f, -10f);

        player.transform.position = spawn;
        player.transform.rotation = Quaternion.LookRotation(Vector3.right, Vector3.up);
    }

    public static void CreateWorldLabel(Transform parent, string name, Vector3 position, string text)
    {
        var labelRoot = new GameObject(name);
        labelRoot.transform.SetParent(parent, false);
        labelRoot.transform.position = position;
        ItemWorldLabel.Create(labelRoot.transform, text, Vector3.zero, 0.11f);
    }

    public static Transform CreateChild(Transform parent, string childName)
    {
        var go = new GameObject(childName);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    public static void DestroyIfExists(string objectName)
    {
        var obj = GameObject.Find(objectName);
        if (obj != null)
        {
            Object.DestroyImmediate(obj);
        }
    }

    static void CreateEdgeMarker(Transform parent, string name, Vector3 position)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent, false);
        cube.transform.position = position;
        cube.transform.localScale = new Vector3(2f, 0.08f, 2f);
        Object.Destroy(cube.GetComponent<Collider>());

        Renderer renderer = cube.GetComponent<Renderer>();
        if (renderer != null)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(0.55f, 0.55f, 0.55f, 0.65f);
            renderer.sharedMaterial = material;
        }
    }
}
