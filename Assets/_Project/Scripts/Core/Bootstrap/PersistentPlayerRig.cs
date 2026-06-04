using UnityEngine;

[DisallowMultipleComponent]
public class PersistentPlayerRig : MonoBehaviour
{
    static PersistentPlayerRig instance;

    GameObject player;
    GameObject mainCamera;
    bool loggedFirstCreation;

    public static PersistentPlayerRig Instance => instance;
    public static GameObject Player => instance != null ? instance.player : null;
    public static bool HasPersistentPlayer => instance != null && instance.player != null;

    public static PersistentPlayerRig EnsureOnGameplayCore(GameplayCore core)
    {
        if (instance != null)
        {
            return instance;
        }

        instance = core.GetComponent<PersistentPlayerRig>();
        if (instance == null)
        {
            instance = core.gameObject.AddComponent<PersistentPlayerRig>();
        }

        return instance;
    }

    public GameObject EnsurePlayer(Vector3 spawnPosition, out bool createdNewPlayer)
    {
        createdNewPlayer = false;

        if (player == null)
        {
            player = FindPersistentPlayerInDontDestroyOnLoad();
        }

        if (player == null)
        {
            player = FindSceneLocalPlayer();
        }

        if (player == null)
        {
            player = CreatePlayerRoot();
            DontDestroyOnLoad(player);
            createdNewPlayer = true;
            if (!loggedFirstCreation)
            {
                loggedFirstCreation = true;
                GameplayCore.Instance?.Log("Created persistent PlayerRoot (DontDestroyOnLoad).");
            }
        }
        else if (player.scene.name != "DontDestroyOnLoad")
        {
            DontDestroyOnLoad(player);
            GameplayCore.Instance?.Log("Promoted scene PlayerRoot to persistent (DontDestroyOnLoad).");
        }

        DestroySceneDuplicatePlayers(player);
        EnsureAllPlayerComponents(player);
        PlayerVisualBuilder.EnsurePlayerVisual(player);
        MovePlayerToSpawn(player, spawnPosition);
        GameplayLayers.TrySetPlayerLayer(player);
        return player;
    }

    public GameObject EnsureMainCamera()
    {
        if (mainCamera == null)
        {
            mainCamera = FindPersistentCameraInDontDestroyOnLoad();
        }

        if (mainCamera == null)
        {
            mainCamera = GameObject.Find("Main Camera");
        }

        if (mainCamera == null)
        {
            mainCamera = new GameObject("Main Camera");
            mainCamera.tag = "MainCamera";
            mainCamera.AddComponent<Camera>();
            mainCamera.AddComponent<AudioListener>();
            DontDestroyOnLoad(mainCamera);
            GameplayCore.Instance?.Log("Created persistent Main Camera (DontDestroyOnLoad).");
        }
        else if (mainCamera.scene.name != "DontDestroyOnLoad")
        {
            DontDestroyOnLoad(mainCamera);
        }

        DestroySceneDuplicateCameras(mainCamera);

        if (mainCamera.GetComponent<PlayerCameraController>() == null)
        {
            mainCamera.AddComponent<PlayerCameraController>();
        }

        var cameraController = mainCamera.GetComponent<PlayerCameraController>();
        if (cameraController != null && player != null)
        {
            cameraController.SetTarget(player.transform);
        }

        return mainCamera;
    }

    public void RebindSceneSystems()
    {
        if (player == null)
        {
            return;
        }

        EnsureAllPlayerComponents(player);
        PlayerVisualBuilder.EnsurePlayerVisual(player);

        var cameraController = mainCamera != null
            ? mainCamera.GetComponent<PlayerCameraController>()
            : null;
        if (cameraController != null)
        {
            cameraController.SetTarget(player.transform);
            cameraController.ApplyGameplayCursorLock();
        }

        var controller = player.GetComponent<PlayerController>();
        if (controller != null && mainCamera != null)
        {
            controller.BindCameraTransform(mainCamera.transform);
        }

        GameplayHudBootstrap.BindAll();
        GameplayPlacementBootstrap.EnsureInitialized();
        player.GetComponent<PlayerToolController>()?.RefreshEquippedTool();
        player.GetComponent<PlayerHeldItemController>()?.RefreshHeldItem();

        EnsureCameraReady();
        PlayerSetupDiagnostics.LogReport(player);
        LogPlayerComponentStatus();
    }

    public void SchedulePostLoadRebind()
    {
        if (isActiveAndEnabled)
        {
            StartCoroutine(PostLoadRebindRoutine());
        }
    }

    System.Collections.IEnumerator PostLoadRebindRoutine()
    {
        yield return null;
        RebindSceneSystems();
        GameplayCore.Instance?.Log("Scene6 GameplayCore rebind complete (post-load).");
    }

    void EnsureCameraReady()
    {
        if (mainCamera == null)
        {
            return;
        }

        var camera = mainCamera.GetComponent<Camera>();
        if (camera != null)
        {
            camera.enabled = true;
            camera.clearFlags = CameraClearFlags.Skybox;
        }

        var listener = mainCamera.GetComponent<AudioListener>();
        if (listener != null)
        {
            listener.enabled = true;
        }

        var cameraController = mainCamera.GetComponent<PlayerCameraController>();
        if (cameraController != null && player != null)
        {
            cameraController.SetTarget(player.transform);
            cameraController.ApplyGameplayCursorLock();
        }
    }

    static GameObject CreatePlayerRoot()
    {
        var playerGo = new GameObject("Player");
        var controller = playerGo.AddComponent<CharacterController>();
        PlayerAnchorUtility.EnsureStandardController(controller);
        return playerGo;
    }

    static void EnsureAllPlayerComponents(GameObject playerGo)
    {
        PlayerRootComponents.EnsureAll(playerGo);
    }

    static void MovePlayerToSpawn(GameObject playerGo, Vector3 spawnPosition)
    {
        if (playerGo == null)
        {
            return;
        }

        var cc = playerGo.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            playerGo.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
            cc.enabled = true;
        }
        else
        {
            playerGo.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
        }

        GameplayCore.Instance?.Log($"Moved persistent player to spawn {spawnPosition}.");
    }

    static GameObject FindPersistentPlayerInDontDestroyOnLoad()
    {
        var controllers = Object.FindObjectsByType<PlayerController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < controllers.Length; i++)
        {
            if (controllers[i] != null
                && controllers[i].gameObject.name == "Player"
                && controllers[i].gameObject.scene.name == "DontDestroyOnLoad")
            {
                return controllers[i].gameObject;
            }
        }

        return null;
    }

    static GameObject FindSceneLocalPlayer()
    {
        var player = GameObject.Find("Player");
        if (player != null && player.scene.name != "DontDestroyOnLoad")
        {
            return player;
        }

        return null;
    }

    static GameObject FindPersistentCameraInDontDestroyOnLoad()
    {
        var cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] != null
                && cameras[i].gameObject.CompareTag("MainCamera")
                && cameras[i].gameObject.scene.name == "DontDestroyOnLoad")
            {
                return cameras[i].gameObject;
            }
        }

        return null;
    }

    static void DestroySceneDuplicatePlayers(GameObject keepPlayer)
    {
        var controllers = Object.FindObjectsByType<PlayerController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < controllers.Length; i++)
        {
            GameObject candidate = controllers[i].gameObject;
            if (candidate == null || candidate == keepPlayer)
            {
                continue;
            }

            if (candidate.name != "Player")
            {
                continue;
            }

            Object.Destroy(candidate);
            GameplayCore.Instance?.Log("Removed duplicate scene PlayerRoot.");
        }
    }

    static void DestroySceneDuplicateCameras(GameObject keepCamera)
    {
        var cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < cameras.Length; i++)
        {
            GameObject candidate = cameras[i].gameObject;
            if (candidate == null || candidate == keepCamera)
            {
                continue;
            }

            if (!candidate.CompareTag("MainCamera"))
            {
                continue;
            }

            if (candidate.scene.name == "DontDestroyOnLoad")
            {
                continue;
            }

            Object.Destroy(candidate);
        }
    }

    void LogPlayerComponentStatus()
    {
        if (player == null)
        {
            return;
        }

        var inventory = player.GetComponent<PlayerInventory>();
        int occupied = inventory != null ? CountOccupiedSlots(inventory) : 0;
        bool crafting = player.GetComponent<PlayerCraftingInteractor>() != null;
        bool pickup = player.GetComponent<PlayerPickupInteractor>() != null;
        bool targeting = player.GetComponent<PlayerGameplayTargeting>() != null;
        bool placement = player.GetComponent<PlayerPlacementController>() != null;
        bool tools = player.GetComponent<PlayerToolController>() != null;

        GameplayCore.Instance?.Log(
            $"Persistent player ready | inventorySlots={occupied} | E crafting={crafting} | pickup={pickup} | " +
            $"targeting={targeting} | placement={placement} | tools={tools}");
    }

    static int CountOccupiedSlots(PlayerInventory inventory)
    {
        int count = 0;
        for (int i = 0; i < PlayerInventory.HotbarSize; i++)
        {
            if (!inventory.GetHotbarSlot(i).IsEmpty)
            {
                count++;
            }
        }

        for (int i = 0; i < PlayerInventory.BackpackSize; i++)
        {
            if (!inventory.GetBackpackSlot(i).IsEmpty)
            {
                count++;
            }
        }

        return count;
    }
}
