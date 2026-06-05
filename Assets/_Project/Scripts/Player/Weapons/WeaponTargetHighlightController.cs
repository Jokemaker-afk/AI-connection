using UnityEngine;

/// <summary>
/// Thin facade over <see cref="EnemyCombatTargetingController"/> for HUD compatibility.
/// </summary>
[DisallowMultipleComponent]
public class WeaponTargetHighlightController : MonoBehaviour
{
    EnemyCombatTargetingController combatTargeting;

    public WeaponTargetingResult LastPreview
    {
        get
        {
            if (combatTargeting == null || !combatTargeting.HasTarget)
            {
                return default;
            }

            return new WeaponTargetingResult
            {
                Primary = combatTargeting.CurrentTarget,
                CandidatesInArea = combatTargeting.HasTarget
                    ? new[] { combatTargeting.CurrentTarget }
                    : System.Array.Empty<WeaponTargetCandidate>(),
            };
        }
    }

    public bool HasPrimaryTarget => combatTargeting != null && combatTargeting.HasTarget;

    public string PrimaryTargetPrompt => combatTargeting != null
        ? combatTargeting.PrimaryTargetPrompt
        : string.Empty;

    void Awake()
    {
        combatTargeting = GetComponent<EnemyCombatTargetingController>();
        if (combatTargeting == null)
        {
            combatTargeting = gameObject.AddComponent<EnemyCombatTargetingController>();
        }
    }
}
