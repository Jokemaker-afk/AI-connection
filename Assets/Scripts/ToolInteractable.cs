using System;
using UnityEngine;

[DisallowMultipleComponent]
public class ToolInteractable : MonoBehaviour
{
    [SerializeField] ToolKind requiredToolKind = ToolKind.Pickaxe;
    [SerializeField] string displayNameChinese = "交互目标";
    [SerializeField] string interactionPromptChinese = "使用对应工具";
    [SerializeField] ItemKind outputItemKind = ItemKind.None;
    [SerializeField] int outputAmount = 1;
    [SerializeField] int requiredHits = 1;
    [SerializeField] bool consumeTargetOnComplete;
    [SerializeField] string taskProgressId;
    [SerializeField] AnimationClip hitFeedbackAnimationClip;
    [SerializeField] AnimationClip completedAnimationClip;
    [SerializeField] GameObject realModelPrefabLater;
    [SerializeField] Transform animationRoot;

    int currentHits;
    bool completed;
    InteractableAnimationPlayer animationPlayer;

    public ToolKind RequiredToolKind => requiredToolKind;
    public string DisplayNameChinese => displayNameChinese;
    public string InteractionPromptChinese => interactionPromptChinese;
    public ItemKind OutputItemKind => outputItemKind;
    public int OutputAmount => outputAmount;
    public string TaskProgressId => taskProgressId;
    public bool IsCompleted => completed;
    public int CurrentHits => currentHits;
    public int RequiredHits => requiredHits;

    public event Action<ToolInteractable> OnCompleted;

    public void Configure(
        ToolKind toolKind,
        string displayName,
        string prompt,
        ItemKind outputKind,
        int outputCount,
        string progressId,
        int hitsRequired = 1,
        bool consumeOnComplete = false)
    {
        requiredToolKind = toolKind;
        displayNameChinese = displayName;
        interactionPromptChinese = prompt;
        outputItemKind = outputKind;
        outputAmount = outputCount;
        taskProgressId = progressId;
        requiredHits = Mathf.Max(1, hitsRequired);
        consumeTargetOnComplete = consumeOnComplete;
    }

    void Awake()
    {
        EnsureAnimationPlayer();
        EnsureCollider();
    }

    public bool CanUseTool(ToolKind toolKind, out string message)
    {
        message = string.Empty;
        if (completed)
        {
            message = "已完成。";
            return false;
        }

        if (toolKind != requiredToolKind)
        {
            message = ItemKindUtility.GetRequiredToolPrompt(requiredToolKind);
            return false;
        }

        return true;
    }

    public bool TryApplyToolHit(ToolKind toolKind, PlayerInventory inventory, out string resultMessage)
    {
        resultMessage = string.Empty;
        if (!CanUseTool(toolKind, out resultMessage))
        {
            return false;
        }

        currentHits++;
        animationPlayer?.PlayHitFeedback(hitFeedbackAnimationClip);

        if (outputItemKind != ItemKind.None && outputAmount > 0 && inventory != null)
        {
            if (!inventory.TryAddItem(outputItemKind, outputAmount))
            {
                resultMessage = "背包已满，无法获得产出。";
                return false;
            }

            resultMessage = $"获得 {ItemKindUtility.GetDisplayName(outputItemKind)} ×{outputAmount}";
        }
        else
        {
            resultMessage = $"{displayNameChinese} 已修复。";
        }

        if (currentHits < requiredHits)
        {
            return true;
        }

        CompleteInteraction();
        return true;
    }

    void CompleteInteraction()
    {
        if (completed)
        {
            return;
        }

        completed = true;
        animationPlayer?.PlayCompleted(completedAnimationClip);

        if (!string.IsNullOrEmpty(taskProgressId))
        {
            SceneTaskProgressBridge.RegisterTaskComplete(taskProgressId);
        }

        OnCompleted?.Invoke(this);

        if (consumeTargetOnComplete)
        {
            gameObject.SetActive(false);
        }
    }

    void EnsureCollider()
    {
        if (GetComponentInChildren<Collider>() != null)
        {
            return;
        }

        var box = gameObject.AddComponent<BoxCollider>();
        box.isTrigger = false;
        box.size = new Vector3(1.2f, 1.2f, 1.2f);
        box.center = new Vector3(0f, 0.6f, 0f);
    }

    void EnsureAnimationPlayer()
    {
        if (animationPlayer != null)
        {
            return;
        }

        animationPlayer = GetComponent<InteractableAnimationPlayer>();
        if (animationPlayer == null)
        {
            animationPlayer = gameObject.AddComponent<InteractableAnimationPlayer>();
        }

        animationPlayer.BindRoot(animationRoot != null ? animationRoot : transform);
    }
}
