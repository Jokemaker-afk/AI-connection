using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerWeaponController : MonoBehaviour
{
    [SerializeField] float messageDuration = 2f;
    [SerializeField] bool enableAttackDebugLogs = true;

    PlayerInventory inventory;
    PlayerGameplayTargeting targeting;
    PlayerHeldItemController heldItemController;
    PlayerToolController toolController;
    WeaponTargetHighlightController highlightController;
    EnemyCombatTargetingController combatTargeting;

    ItemKind equippedWeaponKind = ItemKind.None;
    float nextAttackTime;
    float messageTimer;
    string lastMessage;
    bool attackInProgress;

    public ItemKind EquippedWeaponKind => equippedWeaponKind;
    public bool HasEquippedWeapon => ItemKindUtility.IsWeapon(equippedWeaponKind);
    public bool HasWeaponEquipped => HasEquippedWeapon;
    public string EquippedWeaponLabel => ItemKindUtility.GetDisplayName(equippedWeaponKind);
    public WeaponProfile CurrentWeaponProfile
    {
        get
        {
            if (ItemCatalog.TryGet(equippedWeaponKind, out ItemData data) && data.IsWeapon)
            {
                return data.Weapon;
            }

            return default;
        }
    }
    public string LastMessage => lastMessage;
    public bool IsAttackInProgress => attackInProgress;

    void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
        targeting = GetComponent<PlayerGameplayTargeting>();
        heldItemController = GetComponent<PlayerHeldItemController>();
        toolController = GetComponent<PlayerToolController>();
        highlightController = GetComponent<WeaponTargetHighlightController>();
        combatTargeting = GetComponent<EnemyCombatTargetingController>();
    }

    void OnEnable()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged += HandleInventoryChanged;
            inventory.OnHotbarSelectionChanged += HandleHotbarSelectionChanged;
        }

        RefreshEquippedWeapon();
    }

    void OnDisable()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= HandleInventoryChanged;
            inventory.OnHotbarSelectionChanged -= HandleHotbarSelectionChanged;
        }
    }

    void HandleInventoryChanged()
    {
        RefreshEquippedWeapon();
    }

    void HandleHotbarSelectionChanged(int index)
    {
        RefreshEquippedWeapon();
    }

    void Update()
    {
        if (messageTimer > 0f)
        {
            messageTimer -= Time.deltaTime;
        }

        if (ShouldBlockWeaponInput())
        {
            return;
        }

        if (WasAttackRequested() && HasEquippedWeapon)
        {
            TryAttack();
        }
    }

    public void RefreshEquippedWeapon()
    {
        heldItemController?.RefreshHeldItem();

        if (inventory == null)
        {
            equippedWeaponKind = ItemKind.None;
            return;
        }

        InventorySlotData selected = inventory.GetHotbarSlot(inventory.SelectedHotbarIndex);
        if (selected.IsEmpty || !ItemKindUtility.IsWeapon(selected.Kind))
        {
            equippedWeaponKind = ItemKind.None;
            return;
        }

        equippedWeaponKind = selected.Kind;
    }

    public bool TryAttack()
    {
        if (!HasEquippedWeapon)
        {
            return false;
        }

        if (attackInProgress)
        {
            LogAttack("Attack missed reason: animation in progress");
            return false;
        }

        if (Time.time < nextAttackTime)
        {
            LogAttack("Attack missed reason: cooldown");
            return false;
        }

        if (!ItemCatalog.TryGet(equippedWeaponKind, out ItemData data) || !data.IsWeapon)
        {
            return false;
        }

        WeaponProfile profile = data.Weapon;
        nextAttackTime = Time.time + Mathf.Max(0.12f, profile.AttackCooldown);
        attackInProgress = true;

        string weaponName = ItemKindUtility.GetDisplayName(equippedWeaponKind);
        LogAttack($"Weapon attack pressed: {weaponName}");
        LogAttack($"{weaponName} attack mode: {profile.TargetMode}");

        PlayAttackAnimation(() => ExecuteAttack(profile));
        return true;
    }

    void PlayAttackAnimation(System.Action onComplete)
    {
        HandheldWeaponVisual weaponVisual = heldItemController?.GetActiveWeaponVisual();
        if (weaponVisual != null)
        {
            weaponVisual.PlayAttackAnimation(onComplete);
            return;
        }

        Transform heldTransform = heldItemController?.ActiveHeldTransform;
        if (heldTransform != null && ItemCatalog.TryGet(equippedWeaponKind, out ItemData data) && data.IsWeapon)
        {
            var fallback = heldTransform.GetComponent<HandheldWeaponVisual>();
            if (fallback == null)
            {
                fallback = heldTransform.gameObject.AddComponent<HandheldWeaponVisual>();
                fallback.Configure(equippedWeaponKind, data.Weapon);
            }

            fallback.PlayAttackAnimation(onComplete);
            return;
        }

        LogAttack("Attack animation: no held visual, executing immediately");
        onComplete?.Invoke();
    }

    void ExecuteAttack(WeaponProfile profile)
    {
        if (profile.AttackMode == WeaponAttackMode.Projectile)
        {
            ExecuteProjectileAttack(profile);
            attackInProgress = false;
            return;
        }

        if (combatTargeting == null)
        {
            combatTargeting = GetComponent<EnemyCombatTargetingController>();
        }

        WeaponTargetingResult result = combatTargeting != null
            ? combatTargeting.BuildAttackResult(profile)
            : ResolveLegacyAttack(profile);

        LogAttack($"Targets found: {result.CandidateCount}");
        if (result.HasPrimary)
        {
            LogAttack($"Primary target: {result.Primary.DisplayName}");
        }

        if (!result.HasPrimary)
        {
            LogAttack("Attack missed: no direct crosshair target");
            SetMessage("未命中目标");
            attackInProgress = false;
            return;
        }

        LogAttack($"Weapon attack target: {result.Primary.DisplayName}");

        if (result.DamagedCount <= 0)
        {
            string missReason = ResolveMissReason(profile, result.Primary);
            LogAttack(missReason);
            SetMessage(missReason.Contains("out of range") || missReason.Contains("距离")
                ? "距离太远"
                : "未命中目标");
            attackInProgress = false;
            return;
        }

        Vector3 aimForward = ResolveFlatAimForward();
        LogAttack($"Damaged targets: {result.DamagedCount}");
        for (int i = 0; i < result.DamagedCount; i++)
        {
            ApplyDamageToCandidate(result.DamagedTargets[i], profile, aimForward);
        }

        SetMessage(result.DamagedCount > 1 ? $"命中 {result.DamagedCount} 个目标" : "命中目标");
        attackInProgress = false;
    }

    void ExecuteProjectileAttack(WeaponProfile profile)
    {
        string weaponName = ItemKindUtility.GetDisplayName(equippedWeaponKind);
        LogAttack($"Ranged weapon fired: {weaponName}");

        Ray crosshairRay = ResolveCrosshairRay();
        Vector3 fireOrigin = WeaponMuzzleUtility.ResolveFireOrigin(heldItemController, crosshairRay);
        float aimDistance = profile.AttackRange > 0f ? profile.AttackRange : 40f;
        Vector3 fireDirection = WeaponMuzzleUtility.ResolveFireDirection(fireOrigin, aimDistance);

        WeaponProjectileSpawner.Spawn(profile, profile.WeaponKind, fireOrigin, fireDirection, gameObject);
        SetMessage("开火");
    }

    Ray ResolveCrosshairRay()
    {
        if (AimReferenceProvider.Instance != null && !AimReferenceProvider.Instance.IsWorldAimingBlocked)
        {
            return AimReferenceProvider.Instance.GetCrosshairRay();
        }

        return CrosshairRayUtility.GetCrosshairRay(out _);
    }

    WeaponTargetingResult ResolveLegacyAttack(WeaponProfile profile)
    {
        Ray crosshairRay = ResolveCrosshairRay();
        PlayerMeleeAttackOrigin meleeOrigin = PlayerMeleeAttackOrigin.EnsureOnPlayer(gameObject);
        Vector3 aimForward = meleeOrigin != null
            ? meleeOrigin.AimForward
            : ResolveFlatAimForward();
        Vector3 attackOrigin = meleeOrigin != null
            ? meleeOrigin.WorldPosition
            : transform.position + Vector3.up * 1.2f;
        return WeaponTargetingSystem.ResolveStrictCrosshairTarget(profile, crosshairRay, attackOrigin, aimForward);
    }

    string ResolveMissReason(WeaponProfile profile, WeaponTargetCandidate primary)
    {
        if (combatTargeting == null || primary.Enemy == null)
        {
            return "Attack missed: no direct crosshair target";
        }

        if (profile.IsMelee)
        {
            MeleeHitValidationResult validation = combatTargeting.LastMeleeValidation;
            if (!validation.HasTarget)
            {
                validation = MeleeHitValidator.Validate(
                    profile,
                    combatTargeting.MeleeAttackOrigin != null
                        ? combatTargeting.MeleeAttackOrigin.WorldPosition
                        : transform.position + Vector3.up * 1.2f,
                    combatTargeting.MeleeAttackOrigin != null
                        ? combatTargeting.MeleeAttackOrigin.AimForward
                        : ResolveFlatAimForward(),
                    primary,
                    combatTargeting.MeleeAttackOrigin != null
                        ? combatTargeting.MeleeAttackOrigin.VerticalTolerance
                        : MeleeHitValidator.DefaultVerticalTolerance);
            }

            if (validation.MissReason == "out of range")
            {
                return "Attack blocked: target out of range";
            }

            if (validation.MissReason == "out of angle")
            {
                return "Attack missed: out of angle";
            }
        }
        else if (!combatTargeting.IsInAttackRange(profile, primary))
        {
            return "Attack blocked: target out of range";
        }

        return "Attack missed: no direct crosshair target";
    }

    void ApplyDamageToCandidate(WeaponTargetCandidate candidate, WeaponProfile profile, Vector3 aimForward)
    {
        if (candidate.Damageable == null || candidate.Damageable.IsDead)
        {
            return;
        }

        Vector3 hitDirection = candidate.HitPoint - transform.position;
        if (hitDirection.sqrMagnitude < 0.001f)
        {
            hitDirection = aimForward;
        }

        DamageInfo info = DamageInfo.Create(
            profile.Damage,
            candidate.HitPoint,
            hitDirection,
            profile.KnockbackForce,
            gameObject);

        LogAttack($"DamageInfo sent: damage={info.Damage}, knockback={info.KnockbackForce}, target={candidate.DisplayName}");
        candidate.Damageable.TakeDamage(info);
        WeaponHitEffectUtility.SpawnHitEffect(profile, candidate.HitPoint, hitDirection.normalized);

        if (candidate.Damageable is MonoBehaviour targetMono)
        {
            EnemyHealth health = targetMono.GetComponent<EnemyHealth>();
            if (health != null)
            {
                LogAttack($"Enemy health after hit: {health.CurrentHealth}/{health.MaxHealth}");
            }
        }
    }

    public string GetWeaponPrompt()
    {
        if (!HasEquippedWeapon)
        {
            return string.Empty;
        }

        if (highlightController != null && !string.IsNullOrEmpty(highlightController.PrimaryTargetPrompt))
        {
            return highlightController.PrimaryTargetPrompt;
        }

        return $"{ItemKindUtility.GetDisplayName(equippedWeaponKind)}\n左键攻击";
    }

    bool ShouldBlockWeaponInput()
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

        if (toolController != null && toolController.HasEquippedTool)
        {
            return true;
        }

        return false;
    }

    bool WasAttackRequested()
    {
        return Mouse.current != null
            && Mouse.current.leftButton.wasPressedThisFrame;
    }

    void SetMessage(string message)
    {
        lastMessage = message;
        messageTimer = messageDuration;
    }

    Vector3 ResolveFlatAimForward()
    {
        if (targeting != null)
        {
            return targeting.GetAimForward();
        }

        if (AimReferenceProvider.TryGetFlatAim(out Vector3 flatAim))
        {
            return flatAim;
        }

        return Vector3.forward;
    }

    void LogAttack(string message)
    {
        if (!enableAttackDebugLogs)
        {
            return;
        }

        Debug.Log($"[WeaponAttack] {message}");
    }
}
