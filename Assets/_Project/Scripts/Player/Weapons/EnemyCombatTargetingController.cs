using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Strict crosshair enemy selection + player-space melee range validation.
/// Target selection: direct center-crosshair ray only.
/// Melee hit: validates crosshair target against MeleeAttackOrigin volume.
/// </summary>
[DisallowMultipleComponent]
[DefaultExecutionOrder(100)]
public class EnemyCombatTargetingController : MonoBehaviour
{
    [Header("Strict Crosshair Selection")]
    [SerializeField] WeaponTargetingMode targetingMode = WeaponTargetingMode.StrictCrosshair;
    [SerializeField] float maxEnemyTargetDistance = 20f;

    [Header("Debug")]
    [SerializeField] bool showEnemyTargetingDebug = true;

    readonly Dictionary<EnemyController, EnemySurfaceHighlighter> highlighterCache = new Dictionary<EnemyController, EnemySurfaceHighlighter>();

    PlayerMeleeAttackOrigin meleeAttackOrigin;
    WeaponTargetCandidate currentTarget;
    MeleeHitValidationResult lastMeleeValidation;
    Ray lastCrosshairRay;
    Vector2 lastCrosshairScreenPoint;
    EnemyController lastHighlightedEnemy;
    bool loggedPolicy;

    public WeaponTargetingMode TargetingMode => targetingMode;
    public bool HasTarget => currentTarget.Enemy != null && currentTarget.Damageable != null && !currentTarget.Damageable.IsDead;
    public WeaponTargetCandidate CurrentTarget => currentTarget;
    public MeleeHitValidationResult LastMeleeValidation => lastMeleeValidation;
    public PlayerMeleeAttackOrigin MeleeAttackOrigin => meleeAttackOrigin;

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

            if (ItemCatalog.TryGet(GetEquippedWeaponKind(), out ItemData data) && data.IsWeapon)
            {
                if (data.Weapon.IsMelee)
                {
                    MeleeHitValidationResult preview = ValidateMeleeHit(data.Weapon, currentTarget);
                    if (!preview.InRange)
                    {
                        return $"目标：{name}{healthLine}\n距离太远";
                    }
                }
                else if (data.Weapon.AttackMode == WeaponAttackMode.Projectile)
                {
                    return $"目标：{name}{healthLine}\n左键开火";
                }
            }

            return $"目标：{name}{healthLine}\n左键攻击";
        }
    }

    void Awake()
    {
        meleeAttackOrigin = PlayerMeleeAttackOrigin.EnsureOnPlayer(gameObject);
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
        RefreshMeleeValidationPreview();
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
            LogDebug("Melee miss reason: no target");
            return result;
        }

        LogDebug($"Crosshair target: {currentTarget.DisplayName}");
        LogDebug($"Weapon attack target: {currentTarget.DisplayName}");

        if (profile.AttackMode == WeaponAttackMode.Projectile)
        {
            return result;
        }

        if (profile.IsRanged || profile.AttackMode == WeaponAttackMode.Hitscan)
        {
            if (IsRangedHitValid(profile, currentTarget))
            {
                result.DamagedTargets = new[] { currentTarget };
            }
            else
            {
                LogDebug("Attack blocked: target out of range");
            }

            return result;
        }

        lastMeleeValidation = ValidateMeleeHit(profile, currentTarget);
        meleeAttackOrigin?.LogValidation(lastMeleeValidation, currentTarget.DisplayName);

        if (!lastMeleeValidation.IsValid)
        {
            LogDebug($"Melee miss reason: {lastMeleeValidation.MissReason}");
            return result;
        }

        LogDebug("Melee hit valid: true");
        result.DamagedTargets = new[] { currentTarget };
        return result;
    }

    public bool IsInAttackRange(WeaponProfile profile, WeaponTargetCandidate candidate)
    {
        if (profile.IsRanged || profile.AttackMode == WeaponAttackMode.Hitscan)
        {
            return IsRangedHitValid(profile, candidate);
        }

        return ValidateMeleeHit(profile, candidate).IsValid;
    }

    bool IsMeleeHitValid(WeaponProfile profile, WeaponTargetCandidate candidate)
    {
        return ValidateMeleeHit(profile, candidate).IsValid;
    }

    MeleeHitValidationResult ValidateMeleeHit(WeaponProfile profile, WeaponTargetCandidate candidate)
    {
        EnsureMeleeOrigin();
        return MeleeHitValidator.Validate(
            profile,
            meleeAttackOrigin.WorldPosition,
            meleeAttackOrigin.AimForward,
            candidate,
            meleeAttackOrigin.VerticalTolerance);
    }

    bool IsRangedHitValid(WeaponProfile profile, WeaponTargetCandidate candidate)
    {
        if (candidate.Collider == null)
        {
            return false;
        }

        EnsureMeleeOrigin();
        Vector3 toTarget = candidate.HitPoint - meleeAttackOrigin.WorldPosition;
        return toTarget.magnitude <= profile.AttackRange + 0.5f;
    }

    void RefreshMeleeValidationPreview()
    {
        if (!HasTarget || !ItemCatalog.TryGet(GetEquippedWeaponKind(), out ItemData data) || !data.IsWeapon || !data.Weapon.IsMelee)
        {
            lastMeleeValidation = default;
            return;
        }

        lastMeleeValidation = ValidateMeleeHit(data.Weapon, currentTarget);
    }

    void UpdateStrictCrosshairTarget()
    {
        LogPolicyOnce();

        if (HasTarget && currentTarget.Damageable.IsDead)
        {
            ClearTargetAndHighlight("dead");
            return;
        }

        lastCrosshairRay = ResolveCrosshairRay(out lastCrosshairScreenPoint);
        if (CrosshairRayUtility.ResolveGameplayCamera() == null)
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

    Ray ResolveCrosshairRay(out Vector2 screenPoint)
    {
        if (AimReferenceProvider.Instance != null && !AimReferenceProvider.Instance.IsWorldAimingBlocked)
        {
            screenPoint = AimReferenceProvider.Instance.GetCrosshairScreenPoint();
            return AimReferenceProvider.Instance.GetCrosshairRay();
        }

        return CrosshairRayUtility.GetCrosshairRay(out screenPoint);
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
            GetHighlighter(currentTarget.Enemy).SetHighlight(EnemyHighlightLevel.Primary);
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

        foreach (KeyValuePair<EnemyController, EnemySurfaceHighlighter> pair in highlighterCache)
        {
            pair.Value?.ClearHighlight();
        }
    }

    void ClearTargetAndHighlight(string reason)
    {
        if (HasTarget || lastHighlightedEnemy != null)
        {
            LogDebug($"Enemy highlight cleared ({reason})");
        }

        currentTarget = default;
        lastMeleeValidation = default;
        lastHighlightedEnemy = null;

        foreach (KeyValuePair<EnemyController, EnemySurfaceHighlighter> pair in highlighterCache)
        {
            pair.Value?.ClearHighlight();
        }
    }

    void EnsureMeleeOrigin()
    {
        if (meleeAttackOrigin == null)
        {
            meleeAttackOrigin = PlayerMeleeAttackOrigin.EnsureOnPlayer(gameObject);
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

            EnsureMeleeOrigin();
            if (meleeAttackOrigin != null)
            {
                Gizmos.color = lastMeleeValidation.IsValid ? Color.green : Color.magenta;
                Gizmos.DrawLine(meleeAttackOrigin.WorldPosition, currentTarget.HitPoint);
            }
        }
    }
}
