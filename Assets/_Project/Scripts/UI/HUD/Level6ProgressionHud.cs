using UnityEngine;
using UnityEngine.UI;

public class Level6ProgressionHud : MonoBehaviour
{
    [SerializeField] Level6ProgressionManager progressionManager;

    RectTransform panelRoot;
    Text objectiveText;
    PlayerInventory trackedInventory;

    void Awake()
    {
        BuildUi();
        BindManager();
    }

    void OnEnable()
    {
        BindManager();
        Subscribe();
        RefreshObjective();
    }

    void OnDisable()
    {
        Unsubscribe();
        UnsubscribeInventory();
    }

    public void BindTo(Level6ProgressionManager manager)
    {
        Unsubscribe();
        progressionManager = manager;
        Subscribe();
        RefreshObjective();
    }

    void BindManager()
    {
        if (progressionManager == null)
        {
            progressionManager = FindFirstObjectByType<Level6ProgressionManager>();
        }
    }

    void Subscribe()
    {
        Level6TaskProgressTracker tracker = Level6TaskProgressTracker.Instance;
        if (tracker != null)
        {
            tracker.OnProgressChanged -= RefreshObjective;
            tracker.OnProgressChanged += RefreshObjective;
        }

        if (progressionManager != null)
        {
            progressionManager.OnPortalActivated -= RefreshObjective;
            progressionManager.OnPortalActivated += RefreshObjective;
        }

        SubscribeInventory();
    }

    void Unsubscribe()
    {
        Level6TaskProgressTracker tracker = Level6TaskProgressTracker.Instance;
        if (tracker != null)
        {
            tracker.OnProgressChanged -= RefreshObjective;
        }

        if (progressionManager != null)
        {
            progressionManager.OnPortalActivated -= RefreshObjective;
        }
    }

    void SubscribeInventory()
    {
        PlayerInventory inventory = ResolvePlayerInventory();
        if (inventory == trackedInventory)
        {
            return;
        }

        UnsubscribeInventory();
        trackedInventory = inventory;
        if (trackedInventory != null)
        {
            trackedInventory.OnInventoryChanged += RefreshObjective;
        }
    }

    void UnsubscribeInventory()
    {
        if (trackedInventory != null)
        {
            trackedInventory.OnInventoryChanged -= RefreshObjective;
            trackedInventory = null;
        }
    }

    PlayerInventory ResolvePlayerInventory()
    {
        var player = PersistentPlayerRig.Player ?? GameObject.Find("Player");
        return player != null ? player.GetComponent<PlayerInventory>() : null;
    }

    void RefreshObjective()
    {
        if (objectiveText == null)
        {
            return;
        }

        SubscribeInventory();
        string text = BuildObjectiveText();
        GameplayChineseText.PrepareUiText(objectiveText, text);
        panelRoot.gameObject.SetActive(!string.IsNullOrEmpty(text));
    }

    string BuildObjectiveText()
    {
        Level6TaskProgressTracker tracker = Level6TaskProgressTracker.Instance;
        if (tracker == null)
        {
            return string.Empty;
        }

        if (progressionManager != null && progressionManager.IsPortalActivated)
        {
            return "目标：前往传送门，按 Y 进入第七关";
        }

        PlayerInventory inventory = ResolvePlayerInventory();
        if (!Level6CraftingProgress.HasAllBasicTools(inventory))
        {
            if (inventory != null && !HasIntermediateMaterialsForTools(inventory))
            {
                return "目标：先在「中间材料」制造木棍和绳子，再制造工具";
            }

            string missing = Level6CraftingProgress.BuildMissingToolsSummary(inventory);
            if (string.IsNullOrEmpty(missing))
            {
                return "目标：收集材料并制造基础工具";
            }

            return $"目标：收集材料并制造基础工具（缺少：{missing}）";
        }

        if (!tracker.IsTaskComplete(Level6TaskProgressTracker.MineOnceTaskId))
        {
            return "目标：使用石镐采矿一次";
        }

        if (!tracker.IsTaskComplete(Level6TaskProgressTracker.ChopOnceTaskId))
        {
            return "目标：使用石斧伐木一次";
        }

        if (!tracker.IsTaskComplete(Level6TaskProgressTracker.RepairSignalRelayTaskId))
        {
            return "目标：使用修理工具修复信号中继器";
        }

        return "目标：前往传送门，按 Y 进入第七关";
    }

    static bool HasIntermediateMaterialsForTools(PlayerInventory inventory)
    {
        return inventory.CountItem(ItemKind.Stick) > 0 && inventory.CountItem(ItemKind.Rope) > 0;
    }

    void BuildUi()
    {
        var canvas = GetComponent<RectTransform>();
        if (canvas == null)
        {
            return;
        }

        var panelGo = new GameObject("Level6ObjectivePanel", typeof(RectTransform), typeof(Image));
        panelGo.transform.SetParent(canvas, false);
        panelRoot = panelGo.GetComponent<RectTransform>();
        panelRoot.anchorMin = new Vector2(0.5f, 1f);
        panelRoot.anchorMax = new Vector2(0.5f, 1f);
        panelRoot.pivot = new Vector2(0.5f, 1f);
        panelRoot.anchoredPosition = new Vector2(0f, -92f);
        panelRoot.sizeDelta = new Vector2(760f, 44f);

        var image = panelGo.GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.45f);
        image.raycastTarget = false;

        var textGo = new GameObject("ObjectiveText", typeof(RectTransform), typeof(Text));
        textGo.transform.SetParent(panelRoot, false);
        var textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(12f, 4f);
        textRect.offsetMax = new Vector2(-12f, -4f);

        objectiveText = textGo.GetComponent<Text>();
        objectiveText.fontSize = 22;
        GameplayChineseText.PrepareUiText(objectiveText, string.Empty);
        panelRoot.gameObject.SetActive(false);
    }
}
