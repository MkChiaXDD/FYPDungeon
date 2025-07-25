using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public enum AttackType { Light, Heavy }

[RequireComponent(typeof(PlayerMovement), typeof(Inventory))]
public class PlayerCombat : MonoBehaviour
{
    #region Constants
    private const float COMBO_WINDOW_DELAY = 0.2f;
    private const float DASH_ATTACK_DURATION = 0.2f;
    private const float PARRY_ACTIVE_DURATION = 0.5f;
    private const float ENEMY_DETECTION_HEIGHT_OFFSET = 1f;
    private const float ATTACK_HEIGHT_OFFSET = 1f;
    #endregion

    #region Serialized Fields
    [Header("Player Data")]
    [SerializeField] private PlayerData _playerData;

    [Header("Layers & Masks")]
    [SerializeField] private LayerMask _ignoreLayerMask;
    [SerializeField] private LayerMask[] _hitLayerMasks;
    [SerializeField] private LayerMask _enemyLayer;

    [Header("Targeting & Lock-On")]
    [SerializeField] private float _lockOnAngleThreshold = 35f;
    [SerializeField] private float _targetDetectionRadius = 15f;
    [SerializeField] private Transform _targetEnemy;
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private GameObject _targetIndicator;

    [Header("Blocking & Parry")]
    [SerializeField] private float _parryThreshold = 0.5f;
    [SerializeField] private float _parryDuration = 4f;
    [SerializeField] private GameObject _parryZone;

    [Header("Combat & Weapons")]
    [SerializeField] private Transform _weaponHoldPoint;
    [SerializeField] private Animator _animator;
    [SerializeField] private BaseAttackScript _currentBasicAttack;
    [SerializeField] private BaseAttackScript baseBasicAttack;
    [SerializeField] private Material breakMaterial;

    [Header("Attack Settings")]

    public float _lightAttackCooldown = 0.5f;
    public float _heavyAttackCooldown = 1.5f;
    public float _minChargeTime = 0.1f;
    public float _maxChargeTime = 2f;
    [SerializeField] private float _heavyAttackMoveDistance = 1.5f;
    [SerializeField] private float _heavyAttackMoveDuration = 0.3f;
    [SerializeField, Range(0, 100)] private int _comboWindowPercentage = 30;

    [Header("Attack Recovery")]
    [SerializeField] private float _attackRecoveryTime = 0.1f; // Time to recover after each attack
    private bool _isInRecovery = false;

    [Header("Inventory")]
    [SerializeField] private InventoryManager _inventoryManager;

    [Header("Mimic")]
    [SerializeField] private MimicSpawner _mimicSpawner;
    [SerializeField] private GameObject _mimicClonePrefab;
    #endregion

    #region Private Fields
    private GameObject _equippedWeapon;
    public Weapon _currentWeapon;
    private ItemInstance _currentItem;

    // Targeting
    private bool _isLockedOn = false;
    private bool _autoTargeting = true;
    private int _currentTargetIndex;
    private Dictionary<GameObject, float> _nearbyEnemies = new Dictionary<GameObject, float>();
    private Dictionary<GameObject, float> _visibleEnemies = new Dictionary<GameObject, float>();

    // Blocking/Parry
    private float _parryCooldown;
    private float _blockHoldStartTime;
    private bool _isParrying;
    private bool _isBlocking;

    // Attacks
    private float _lastAttackTime;
    private float _currentAttackCooldown;
    private float _chargeStartTime;
    private bool _isCharging;
    private AttackType _lastAttackType;
    private bool _canComboContinue;
    private bool _isInComboWindow;

    // Cached components
    private Inventory _inventory;
    #endregion

    #region Events
    public UnityEvent ChargeUp;
    public UnityEvent Uncharge;
    #endregion

    #region Properties
    public static PlayerCombat Instance { get; private set; }
    public bool IsLockedOn => _isLockedOn;
    public Transform TargetEnemy => _targetEnemy;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _inventory = GetComponent<Inventory>();
    }

    private void Start()
    {
        if (_playerMovement == null)
        {
            _playerMovement = GetComponent<PlayerMovement>();
        }

        _currentBasicAttack = baseBasicAttack;
    }

    private void OnEnable()
    {
        _inventory.ChangeSlot.AddListener(UpdateEquippedItem);
        _inventoryManager.ModifySlot.AddListener(UpdateEquippedItem);
        _playerData.DataChange.AddListener(ApplyPlayerStats);
    }

    private void OnDisable()
    {
        _inventory.ChangeSlot.RemoveListener(UpdateEquippedItem);
        _inventoryManager.ModifySlot.RemoveListener(UpdateEquippedItem);
        _playerData.DataChange.RemoveListener(ApplyPlayerStats);
    }

    private void Update()
    {
        HandleBlocking();
        HandleLockOn();
        // Update recovery timer
        if (_isInRecovery && Time.time > _lastAttackTime + _currentAttackCooldown + _attackRecoveryTime)
        {
            _isInRecovery = false;
        }

        if (_isLockedOn)
        {
            UpdateVisibleEnemies();
            HandleTargetSwitching();

            if (_autoTargeting)
            {
                FindNearestVisibleEnemy();
            }

            if (_visibleEnemies.Count == 0)
            {
                ClearTargeting();
            }
            else if (!_targetIndicator.activeInHierarchy)
            {
                _targetIndicator.SetActive(true);
            }
        }

        UpdateTargetIndicator();
        HandleAttacks();

        if (_parryCooldown > 0)
        {
            _parryCooldown -= Time.deltaTime;
        }
    }
    #endregion

    #region Combat Core
    private void HandleAttacks()
    {
        if (_isInRecovery) return; // Prevent charging during recovery
        HandleChargedAttack();
        HandleSpecialAttack();
    }

    private void HandleChargedAttack()
    {
        // Start charging heavy attack
        if (Input.GetMouseButtonDown(0))
        {
            if (Time.time > _lastAttackTime + _currentAttackCooldown)
            {
                _isCharging = true;
                _chargeStartTime = Time.time;
                _playerMovement.SetMovementLock(true);
                ChargeUp?.Invoke();
            }
        }

        // Execute attack on release
        if (Input.GetMouseButtonUp(0) && _isCharging)
        {
            _isCharging = false;
            _playerMovement.SetMovementLock(false);

            float chargeTime = Time.time - _chargeStartTime;

            ExecuteAttack(chargeTime);

            Uncharge?.Invoke();
        }

        // Cancel charge if moving during charge time
        if (_isCharging && _playerMovement.IsMoving)
        {
            _isCharging = false;
            _playerMovement.SetMovementLock(false);
            Uncharge?.Invoke();
        }
    }

    private void HandleSpecialAttack()
    {
        if (!_currentWeapon)
            return;

        if (Input.GetMouseButtonDown(1))
        {
            _currentWeapon.Cast();
        }
    }

    private void ExecuteAttack(float chargeTime)
    {
        if (_isInRecovery) return; // Prevent attacking during recovery

        _lastAttackTime = Time.time;
        _isInRecovery = true;

        if (chargeTime >= _minChargeTime)
        {
            // Heavy attack
            _currentAttackCooldown = _heavyAttackCooldown;
            _lastAttackType = AttackType.Heavy;

            float chargePercent = Mathf.Clamp01((chargeTime - _minChargeTime) / (_maxChargeTime - _minChargeTime));
            float damageMultiplier = 1f + chargePercent;
            float aoeRadius = Mathf.Lerp(2f, 5f, chargePercent);

            ExecuteHeavyAttack(damageMultiplier, aoeRadius);
            StartCoroutine(HeavyAttackMovement());
        }
        else
        {
            // Light attack
            _currentAttackCooldown = _lightAttackCooldown;
            _lastAttackType = AttackType.Light;
            ExecuteLightAttack();
        }

        ApplyDurabilityCost();
        TrySpawnMimic();
    }

    private void ExecuteLightAttack()
    {
        if (_isLockedOn && _targetEnemy != null)
        {
            Vector3 attackPosition = _targetEnemy.position + (_targetEnemy.forward * 1f);
            StartCoroutine(DashToAttack(attackPosition, true));
        }
        else
        {
            _currentBasicAttack.ExecuteLightAttack();
            TriggerAttackAnimation("LightAttack");
            PlayAttackSound("BasicAttack");
        }
    }

    private void ExecuteHeavyAttack(float damageMultiplier, float aoeRadius)
    {
        Vector3 attackPosition = _isLockedOn && _targetEnemy != null
            ? _targetEnemy.position
            : transform.position + _playerMovement.GetDirection() * 2f + Vector3.up * ATTACK_HEIGHT_OFFSET;

        _currentBasicAttack.ExecuteHeavyAttack(attackPosition, damageMultiplier, aoeRadius);
        TriggerAttackAnimation("HeavyAttack");
    }

    private IEnumerator HeavyAttackMovement()
    {
        float elapsed = 0;
        Vector3 startPos = transform.position;
        Vector3 moveDirection = _isLockedOn && _targetEnemy != null
            ? (_targetEnemy.position - transform.position).normalized
            : transform.forward;
        Vector3 endPos = startPos + moveDirection * _heavyAttackMoveDistance;

        while (elapsed < _heavyAttackMoveDuration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / _heavyAttackMoveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator DashToAttack(Vector3 targetPosition, bool isLightAttack)
    {
        float elapsed = 0;
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(targetPosition.x, transform.position.y, targetPosition.z);

        while (elapsed < DASH_ATTACK_DURATION)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / DASH_ATTACK_DURATION);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (isLightAttack)
        {
            _currentBasicAttack.ExecuteLightAttack();
            PlayAttackSound("BasicAttack");
        }
        TriggerAttackAnimation(isLightAttack ? "LightAttack" : "HeavyAttack");
    }

    private void TriggerAttackAnimation(string triggerName)
    {
        if (triggerName == "LightAttack")
        {
            if (!_canComboContinue)
            {
                _animator.SetTrigger("LightAttack");
                _canComboContinue = true;
            }

            if (_isInComboWindow)
            {
                _animator.SetBool("Combo", true);
            }
        }
        else
        {
            ResetCombo();
            _animator.SetTrigger("HeavyAttack");
        }
    }

    private void PlayAttackSound(string soundName)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(soundName, gameObject);
        }
    }

    private void ApplyDurabilityCost()
    {
        if (_currentWeapon == null) return;

        int durabilityCost = _lastAttackType == AttackType.Light
            ? _currentWeapon.baseDurabilityCost
            : _currentWeapon.baseDurabilityCost * 2;

        _inventory.BreakItem(_inventory.equippedSlotNum, durabilityCost);
    }

    private void TrySpawnMimic()
    {
        if (_mimicSpawner != null)
        {
            _mimicSpawner.TrySpawnMimic();
        }
    }
    #endregion

    #region Blocking & Parry
    private void HandleBlocking()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            _blockHoldStartTime = Time.time;
        }

        if (Input.GetKey(KeyCode.F))
        {
            if (!_isBlocking && Time.time - _blockHoldStartTime > _parryThreshold)
            {
                StartBlocking();
            }
        }

        if (Input.GetKeyUp(KeyCode.F))
        {
            if (_parryCooldown <= 0)
            {
                float heldDuration = Time.time - _blockHoldStartTime;
                if (heldDuration <= _parryThreshold)
                {
                    AttemptParry();
                }
            }
            StopBlocking();
        }
    }

    private void StartBlocking()
    {
        _isBlocking = true;
    }

    private void StopBlocking()
    {
        _isBlocking = false;
    }

    private void AttemptParry()
    {
        if (_isParrying) return;

        _isParrying = true;
        _parryZone.SetActive(true);
        StartCoroutine(ParryDuration());
    }

    private IEnumerator ParryDuration()
    {
        _parryCooldown = _parryDuration;
        yield return new WaitForSeconds(PARRY_ACTIVE_DURATION);

        _isParrying = false;
        _parryZone.SetActive(false);
    }

    public void CancelParry()
    {
        _isParrying = false;
        _parryZone.SetActive(false);
    }
    #endregion

    #region Equipment
    private void UpdateEquippedItem()
    {
        ClearWeapon();
        _currentItem = _inventoryManager.GetCurrentHotbarItem();
        EquipItem(_currentItem);
    }

    private void EquipItem(ItemInstance item)
    {
        if (item?.ItemPrefab == null) return;

        _equippedWeapon = Instantiate(item.ItemPrefab, _weaponHoldPoint);
        ConfigureWeaponPhysics(_equippedWeapon);

        _currentWeapon = _equippedWeapon.GetComponent<Weapon>();
        if (_currentWeapon != null)
        {
            _currentBasicAttack = _currentWeapon.weaponData.baseAttackScript;
            _currentWeapon.CurrDurability = item.Durability;
        }
    }

    private void ConfigureWeaponPhysics(GameObject weapon)
    {
        if (weapon.TryGetComponent<Collider>(out var weaponCollider))
        {
            weaponCollider.isTrigger = true;
        }

        if (weapon.TryGetComponent<Rigidbody>(out var weaponRb))
        {
            weaponRb.isKinematic = true;
        }
    }

    private void ClearWeapon()
    {
        if (_equippedWeapon != null)
        {
            var weaponBreak = _equippedWeapon.AddComponent<WeaponBreak>();
            weaponBreak.Dissolve_Shader = breakMaterial;
            weaponBreak.dissolveSpeed = 2;

            _equippedWeapon.transform.SetParent(null, true);
            _equippedWeapon = null;
        }
        _currentBasicAttack = baseBasicAttack;
        _currentWeapon = null;
    }
    #endregion

    #region Targeting System
    private void HandleLockOn()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            _isLockedOn = !_isLockedOn;
            if (!_isLockedOn) ClearTargeting();
        }
    }

    private void UpdateVisibleEnemies()
    {
        _visibleEnemies.Clear();
        ScanForNearbyEnemies();

        foreach (var enemy in _nearbyEnemies)
        {
            if (enemy.Key != null && IsEnemyVisible(enemy.Key))
            {
                _visibleEnemies.Add(enemy.Key, enemy.Value);
            }
        }
    }

    private void ScanForNearbyEnemies()
    {
        _nearbyEnemies.Clear();

        Vector3 detectionCenter = transform.position + Vector3.up * ENEMY_DETECTION_HEIGHT_OFFSET;
        var hitColliders = Physics.OverlapSphere(detectionCenter, _targetDetectionRadius, _enemyLayer);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject == null) continue;

            float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
            _nearbyEnemies[hitCollider.gameObject] = distance;
        }
    }

    private bool IsEnemyVisible(GameObject enemy)
    {
        Vector3 direction = (enemy.transform.position - transform.position).normalized;
        float angle = Vector3.Angle(_playerMovement.GetDirection(), direction);
        return angle <= _lockOnAngleThreshold && HasLineOfSight(enemy);
    }

    private bool HasLineOfSight(GameObject target)
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 targetPos = target.transform.position + Vector3.up * 0.5f;
        Vector3 direction = (targetPos - origin).normalized;
        float distance = Vector3.Distance(origin, targetPos);

        return !Physics.Raycast(origin, direction, distance, _ignoreLayerMask);
    }

    private void FindNearestVisibleEnemy()
    {
        _targetEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (var enemy in _visibleEnemies)
        {
            if (enemy.Value < closestDistance)
            {
                closestDistance = enemy.Value;
                _targetEnemy = enemy.Key.transform;
            }
        }
    }

    private void HandleTargetSwitching()
    {
        if (Input.GetKeyDown(KeyCode.G) && _visibleEnemies.Count > 0)
        {
            _autoTargeting = false;
            _currentTargetIndex = (_currentTargetIndex + 1) % _visibleEnemies.Count;
            SelectTargetByIndex();
        }
    }

    private void SelectTargetByIndex()
    {
        var sortedTargets = _visibleEnemies.OrderBy(e => e.Value).ToArray();
        _targetEnemy = sortedTargets[_currentTargetIndex].Key.transform;
    }

    private void UpdateTargetIndicator()
    {
        if (_targetEnemy != null)
        {
            _targetIndicator.transform.position = _targetEnemy.position;
        }
    }

    private void ClearTargeting()
    {
        _currentTargetIndex = 0;
        _autoTargeting = true;
        _targetEnemy = null;
        _targetIndicator.SetActive(false);
    }

    public void EnableMimic(bool enable)
    {
        if (_mimicSpawner != null)
        {
            _mimicSpawner.enabled = enable;
        }
    }

    public void SetUpMimic(MimicSpawner mimic)
    {
        _mimicSpawner = mimic;
        _mimicSpawner._mimicClonePrefab = _mimicClonePrefab;
    }
    #endregion

    #region Animation Events
    public void ResetCombo()
    {
        _animator.SetBool("Combo", false);
        _canComboContinue = false;
        _isInComboWindow = false;
    }

    public void EnableComboWindow(float animationDuration)
    {
        float windowStartTime = animationDuration * (_comboWindowPercentage / 100f);
        StartCoroutine(OpenComboWindow(windowStartTime));
    }

    private IEnumerator OpenComboWindow(float delay)
    {
        yield return new WaitForSeconds(delay);
        _isInComboWindow = true;
    }
    #endregion

    #region Stat Management
    private void ApplyPlayerStats()
    {
        _parryDuration = _playerData.ParryDuration;
    }
    #endregion
}