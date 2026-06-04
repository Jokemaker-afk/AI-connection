using System;
using UnityEngine;

[DisallowMultipleComponent]
public class ToolInteractable : MonoBehaviour
{
    [SerializeField] ToolKind requiredToolKind = ToolKind.Pickaxe;
    [SerializeField] string displayNameChinese = "交互目标";
    [SerializeField] string interactionPromptChinese = "使用对应工具";
    [SerializeField] string completedPromptChinese = "已完成。";
    [SerializeField] int requiredHits = 1;
    [SerializeField] bool consumeOrDisableAfterComplete;
    [SerializeField] string taskProgressId;
    [SerializeField] bool dropRewardsToWorld = true;
    [SerializeField] bool addRewardsDirectlyToInventory;
    [SerializeField] Transform rewardDropPoint;
    [SerializeField] float rewardDropRadius = 0.65f;
    [SerializeField] bool triggerEventOnComplete;
    [SerializeField] string eventIdOnComplete;
    [SerializeField] ToolReward[] rewards = Array.Empty<ToolReward>();
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
    public string TaskProgressId => taskProgressId;
    public bool IsCompleted => completed;
    public int CurrentHits => currentHits;
    public int RequiredHits => requiredHits;
    public bool DropRewardsToWorld => dropRewardsToWorld;
    public bool AddRewardsDirectlyToInventory => addRewardsDirectlyToInventory;

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
        ConfigureResource(
            toolKind,
            displayName,
            prompt,
            outputKind == ItemKind.None
                ? Array.Empty<ToolReward>()
                : new[] { new ToolReward(outputKind, outputCount, outputCount, 1f) },
            progressId,
            hitsRequired,
            consumeOnComplete,
            dropToWorld: false,
            addToInventory: outputKind != ItemKind.None && outputCount > 0);
    }

    public void ConfigureResource(
        ToolKind toolKind,
        string displayName,
        string prompt,
        ToolReward[] rewardTable,
        string progressId,
        int hitsRequired = 1,
        bool consumeOnComplete = false,
        bool dropToWorld = true,
        bool addToInventory = false)
    {
        requiredToolKind = toolKind;
        displayNameChinese = displayName;
        interactionPromptChinese = prompt;
        rewards = rewardTable ?? Array.Empty<ToolReward>();
        taskProgressId = progressId;
        requiredHits = Mathf.Max(1, hitsRequired);
        consumeOrDisableAfterComplete = consumeOnComplete;
        dropRewardsToWorld = dropToWorld;
        addRewardsDirectlyToInventory = addToInventory;
        triggerEventOnComplete = false;
        eventIdOnComplete = string.Empty;
    }

    public void ConfigureEventTrigger(
        ToolKind toolKind,
        string displayName,
        string prompt,
        string progressId,
        string eventId,
        string completedPrompt = null,
        int hitsRequired = 1,
        bool consumeOnComplete = false)
    {
        requiredToolKind = toolKind;
        displayNameChinese = displayName;
        interactionPromptChinese = prompt;
        completedPromptChinese = string.IsNullOrEmpty(completedPrompt) ? $"{displayName} 已修复。" : completedPrompt;
        rewards = Array.Empty<ToolReward>();
        taskProgressId = progressId;
        requiredHits = Mathf.Max(1, hitsRequired);
        consumeOrDisableAfterComplete = consumeOnComplete;
        dropRewardsToWorld = false;
        addRewardsDirectlyToInventory = false;
        triggerEventOnComplete = true;
        eventIdOnComplete = eventId;
    }

    void Awake()
    {
        EnsureAnimationPlayer();
        EnsureCollider();
        if (rewardDropPoint == null)
        {
            rewardDropPoint = transform;
        }
    }

    public bool CanUseTool(ToolKind toolKind, out string message)
    {
        message = string.Empty;
        if (completed)
        {
            message = completedPromptChinese;
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

        Debug.Log($"[ToolInteractable] ToolInteractable hit: {displayNameChinese} with {toolKind} | matched: true");

        if (addRewardsDirectlyToInventory && HasRewards() && inventory != null
            && !CanAcceptAllRewards(inventory))
        {
            resultMessage = "背包已满，无法获得产出。";
            return false;
        }

        currentHits++;
        animationPlayer?.PlayHitFeedback(hitFeedbackAnimationClip);

        int granted = 0;
        if (HasRewards())
        {
            granted = ToolInteractableRewardUtility.GrantRewards(
                rewards,
                dropRewardsToWorld,
                addRewardsDirectlyToInventory,
                rewardDropPoint != null ? rewardDropPoint : transform,
                rewardDropRadius,
                inventory,
                transform.parent);
        }
        else if (triggerEventOnComplete && currentHits >= requiredHits)
        {
            resultMessage = completedPromptChinese;
        }

        if (granted > 0)
        {
            resultMessage = BuildRewardSummary();
        }
        else if (string.IsNullOrEmpty(resultMessage))
        {
            resultMessage = completedPromptChinese;
        }

        if (currentHits < requiredHits)
        {
            return true;
        }

        CompleteInteraction();
        return true;
    }

    bool HasRewards()
    {
        return rewards != null && rewards.Length > 0;
    }

    bool CanAcceptAllRewards(PlayerInventory inventory)
    {
        for (int i = 0; i < rewards.Length; i++)
        {
            ToolReward reward = rewards[i];
            if (!ItemKindUtility.IsValid(reward.ItemKind))
            {
                continue;
            }

            int amount = Mathf.Max(reward.MinAmount, reward.MaxAmount);
            if (!UnifiedInventoryAcquisition.CanAcquire(inventory, reward.ItemKind, amount))
            {
                return false;
            }
        }

        return true;
    }

    string BuildRewardSummary()
    {
        if (!HasRewards())
        {
            return completedPromptChinese;
        }

        var parts = new System.Collections.Generic.List<string>(rewards.Length);
        for (int i = 0; i < rewards.Length; i++)
        {
            if (!ItemKindUtility.IsValid(rewards[i].ItemKind))
            {
                continue;
            }

            parts.Add($"{ItemKindUtility.GetDisplayName(rewards[i].ItemKind)}");
        }

        return parts.Count > 0 ? $"获得 {string.Join("、", parts)}" : completedPromptChinese;
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

        if (triggerEventOnComplete && !string.IsNullOrEmpty(eventIdOnComplete))
        {
            GameEventManager.TriggerEvent(eventIdOnComplete);
            Debug.Log($"[ToolInteractable] Signal Relay completed event: {eventIdOnComplete}");
        }

        OnCompleted?.Invoke(this);

        if (consumeOrDisableAfterComplete)
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
