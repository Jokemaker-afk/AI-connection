using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Strict center-crosshair weapon enemy targeting. No aim assist, stickiness, or soft lock.
/// </summary>
[DisallowMultipleComponent]
public class EnemyCombatTargetingController : MonoBehaviour
{
    [Header("Strict Crosshair")]
    [SerializeField] WeaponTargetingMode targetingMode = WeaponTargetingMode.StrictCrosshair;
    [SerializeField] float maxEnemyTargetDistance = 20f;

    [Header("Debug")]
    [SerializeField] bool showEnemyTargetingDebug = true;

    readonly Dictionary<EnemyController, EnemySurfaceHighlighter> highlighterCache = new Dictionary<EnemyController, EnemySurfaceHighlighter>();

    WeaponTargetCandidate currentTarget;
    Ray lastCrosshairRay;
    Vector2 lastCrosshairScreenPoint;
    EnemyController lastHighlightedEnemy;
    bool loggedPolicy;

    public WeaponTargetingMode TargetingMode => targetingMode;
    public bool HasTarget => currentTarget.Enemy != null && currentTarget.Damageable != null && !currentTarget.Damageable.IsDead;
    public WeaponTargetCandidate CurrentTarget => currentTarget;

    public string PrimaryTargetPrompt
    {
        get
        {
            if (!HasTarget)
            {
                return string.Empty;
            }

            string name = currentTarget.DisplayName;
            string healthLine = string.Empty;
            if (currentTarget.Enemy != null)
            {
                EnemyHealth healthComp = currentTarget.Enemy.GetComponent<EnemyHealth>();
                if (healthComp != null)
                {
                    healthLine = $"\n生命：{Mathf.CeilToInt(healthComp.CurrentHealth)} / {Mathf.CeilToInt(healthComp.MaxHealth)}";
                }
            }

            if (ItemCatalog.TryGet(GetEquippedWeaponKind(), out ItemData data) && data.IsWeapon
                && !IsInAttackRange(data.Weapon, currentTarget))
            {
                return $"目标：{name}{healthLine}\n距离太远";
            }

            return $"目标：{name}{healthLine}\n左键攻击";
        }
    }

    void LateUpdate()
    {
        if (ShouldSkipTargeting())
        {
            ClearTargetAndHighlight("ui or tool active");
            return;
        }

        UpdateStrictCrosshairTarget();
        ApplyHighlight();
    }

    public WeaponTargetingResult BuildAttackResult(WeaponProfile profile)
    {
        var result = new WeaponTargetingResult
        {
            Primary = currentTarget,
            CandidatesInArea = HasTarget ? new[] { currentTarget } : System.Array.Empty<WeaponTargetCandidate>(),
        };

        if (!HasTarget)
        {
            LogDebug("Weapon attack target: none");
            return result;
        }

        LogDebug($"Weapon attack target: {currentTarget.DisplayName}");

        if (!IsInAttackRange(profile, currentTarget))
        {
            LogDebug("Attack blocked: target out of range");
            return result;
        }

        result.DamagedTargets = new[] { currentTarget };
        return result;
    }

    public bool IsInAttackRange(WeaponProfile profile, WeaponTargetCandidate candidate)
    {
        if (candidate.Collider == null)
        {
            return false;
        }

        Vector3 attackOrigin = transform.position + Vector3.up * 1.1f;
        Vector3 toTarget = candidate.HitPoint - attackOrigin;
        if (profile.IsRanged || profile.AttackMode == WeaponAttackMode.Hitscan)
        {
            return toTarget.magnitude <= profile.AttackRange + 0.5f;
        }

        Vector3 flat = toTarget;
        flat.y = 0f;
        return flat.magnitude <= profile.AttackRange + candidate.Collider.bounds.extents.magnitude;
    }

    void UpdateStrictCrosshairTarget()
    {
        LogPolicyOnce();

        if (HasTarget && currentTarget.Damageable.IsDead)
        {
            ClearTargetAndHighlight("dead");
            return;
        }

        lastCrosshairRay = CrosshairRayUtility.GetCrosshairRay(out lastCrosshairScreenPoint);
        Camera camera = CrosshairRayUtility.ResolveGameplayCamera();
        if (camera == null)
        {
            ClearTargetAndHighlight("no camera");
            return;
        }

        LayerMask enemyMask = GameplayLayers.CombatTargetMask;
        if (TryDirectCrosshairHit(lastCrosshairRay, enemyMask, out WeaponTargetCandidate hit))
        {
            EnemyController previousEnemy = currentTarget.Enemy;
            currentTarget = hit;
            LogDebug($"Crosshair ray hit: {hit.DisplayName}");
            if (previousEnemy != hit.Enemy)
            {
                LogDebug($"Enemy highlighted: {hit.DisplayName}");
            }

            return;
        }

        LogDebug("Crosshair ray hit: none");
        if (HasTarget || lastHighlightedEnemy != null)
        {
            LogDebug("Enemy highlight cleared");
        }

        ClearTargetAndHighlight("no direct crosshair hit");
    }

    bool TryDirectCrosshairHit(Ray ray, LayerMask mask, out WeaponTargetCandidate candidate)
    {
        candidate = default;
        if (!Physics.Raycast(ray, out RaycastHit hit, maxEnemyTargetDistance, mask, QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        if (!EnemyTargetingUtility.TryResolveEnemyFromCollider(
                hit.collider,
                out EnemyController enemy,
                out Collider primaryCollider,
                out IDamageable damageable)
            || damageable.IsDead)
        {
            return false;
        }

        candidate = new WeaponTargetCandidate
        {
            Damageable = enemy,
            Enemy = enemy,
            Collider = primaryCollider,
            HitPoint = hit.point,
            AngleDegrees = 0f,
            Distance = Vector3.Distance(ray.origin, hit.point),
            ScreenDistance = 0f,
            IsDirectRayHit = true,
        };
        return true;
    }

    void ApplyHighlight()
    {
        if (HasTarget && currentTarget.Enemy != null)
        {
            EnemySurfaceHighlighter highlighter = GetHighlighter(currentTarget.Enemy);
            highlighter.SetHighlight(EnemyHighlightLevel.Primary);
            lastHighlightedEnemy = currentTarget.Enemy;
            return;
        }

        if (lastHighlightedEnemy != null)
        {
            if (highlighterCache.TryGetValue(lastHighlightedEnemy, out EnemySurfaceHighlighter previous))
            {
                previous.ClearHighlight();
            }

            lastHighlightedEnemy = null;
        }

        var keys = new List<EnemyController>(highlighterCache.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            EnemyController enemy = keys[i];
            if (enemy == null)
            {
                continue;
            }

            highlighterCache[enemy].ClearHighlight();
        }
    }

    void ClearTargetAndHighlight(string reason)
    {
        if (HasTarget || lastHighlightedEnemy != null)
        {
            LogDebug($"Enemy highlight cleared ({reason})");
        }

        currentTarget = default;
        lastHighlightedEnemy = null;

        foreach (KeyValuePair<EnemyController, EnemySurfaceHighlighter> pair in highlighterCache)
        {
            pair.Value?.ClearHighlight();
        }
    }

    EnemySurfaceHighlighter GetHighlighter(EnemyController enemy)
    {
        if (!highlighterCache.TryGetValue(enemy, out EnemySurfaceHighlighter highlighter) || highlighter == null)
        {
            highlighter = EnemyTargetingUtility.GetOrCreateHighlighter(enemy);
            highlighterCache[enemy] = highlighter;
        }

        return highlighter;
    }

    bool ShouldSkipTargeting()
    {
        if (GameplayCursorPolicy.IsAnyMenuOpen)
        {
            return true;
        }

        var craftingHud = FindFirstObjectByType<CraftingHud>();
        if (craftingHud != null && craftingHud.IsOpen)
        {
            return true;
        }

        var inventoryHud = FindFirstObjectByType<PlayerInventoryHud>();
        if (inventoryHud != null && inventoryHud.IsBackpackOpen)
        {
            return true;
        }

        var placement = GetComponent<PlayerPlacementController>();
        if (placement != null && placement.IsPlacementActive)
        {
            return true;
        }

        var toolController = GetComponent<PlayerToolController>();
        return toolController != null && toolController.HasEquippedTool;
    }

    ItemKind GetEquippedWeaponKind()
    {
        var weaponController = GetComponent<PlayerWeaponController>();
        return weaponController != null ? weaponController.EquippedWeaponKind : ItemKind.None;
    }

    void LogPolicyOnce()
    {
        if (loggedPolicy)
        {
            return;
        }

        LogDebug($"Weapon targeting mode: {targetingMode}");
        LogDebug("Weapon aim assist disabled");
        LogDebug("SphereCast/cone/screen-space assist not used for weapon targeting");
        LogDebug("Crosshair target ray uses center screen: true");
        LogDebug("Input.mousePosition used for world targeting: false");
        loggedPolicy = true;
    }

    void LogDebug(string message)
    {
        if (showEnemyTargetingDebug)
        {
            Debug.Log($"[WeaponTargeting] {message}");
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!showEnemyTargetingDebug || !Application.isPlaying)
        {
            return;
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(lastCrosshairRay.origin, lastCrosshairRay.direction * maxEnemyTargetDistance);

        if (HasTarget)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(currentTarget.HitPoint, 0.12f);
        }
    }
}
