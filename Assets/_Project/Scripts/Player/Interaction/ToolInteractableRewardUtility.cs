using UnityEngine;

public static class ToolInteractableRewardUtility
{
    public static int GrantRewards(
        ToolReward[] rewards,
        bool dropToWorld,
        bool addToInventory,
        Transform dropOrigin,
        float dropRadius,
        PlayerInventory inventory,
        Transform pickupParent)
    {
        if (rewards == null || rewards.Length == 0)
        {
            return 0;
        }

        int granted = 0;
        for (int i = 0; i < rewards.Length; i++)
        {
            ToolReward reward = rewards[i];
            if (!reward.ShouldDrop() || !ItemKindUtility.IsValid(reward.ItemKind))
            {
                continue;
            }

            int amount = reward.RollAmount();
            if (amount <= 0)
            {
                continue;
            }

            if (addToInventory && inventory != null
                && UnifiedInventoryAcquisition.TryAcquireFully(inventory, reward.ItemKind, amount))
            {
                granted++;
                Debug.Log($"[ToolInteractable] Added reward to inventory: {ItemKindUtility.GetDisplayName(reward.ItemKind)} x{amount}");
                continue;
            }

            if (!dropToWorld || dropOrigin == null)
            {
                continue;
            }

            Vector3 dropPosition = GetDropPosition(dropOrigin, dropRadius);
            GameObject pickup = ItemModuleFactory.SpawnDroppedItem(reward.ItemKind, dropPosition, amount, pickupParent);
            if (pickup != null)
            {
                granted++;
                Debug.Log($"[ToolInteractable] Dropped reward item: {ItemKindUtility.GetDisplayName(reward.ItemKind)} x{amount}");
            }
        }

        return granted;
    }

    public static Vector3 GetDropPosition(Transform origin, float radius)
    {
        if (origin == null)
        {
            return Vector3.zero;
        }

        float angle = Random.Range(0f, Mathf.PI * 2f);
        float distance = radius > 0f ? Random.Range(radius * 0.35f, radius) : 0.35f;
        Vector3 offset = new Vector3(Mathf.Cos(angle) * distance, 0.35f, Mathf.Sin(angle) * distance);
        Vector3 candidate = origin.position + offset;

        if (Physics.Raycast(candidate + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 6f, ~0, QueryTriggerInteraction.Ignore))
        {
            candidate.y = hit.point.y + 0.12f;
        }

        return candidate;
    }
}
