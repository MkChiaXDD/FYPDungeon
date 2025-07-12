using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public enum AttackType { Light, Heavy }

public class PlayerCombat : MonoBehaviour
{
    [Header("Player Data")]
    [SerializeField] private PlayerData _playerData;

    [Header("Layers & Masks")]
    [SerializeField] private LayerMask _ignoreLayerMask;
    [SerializeField] private LayerMask[] _hitLayerMasks;
    [SerializeField] private LayerMask _enemyLayer;

    [Header("Targeting & Lock-On")]
    [SerializeField] private float _lockOnAngleThreshold = 35f;
    [SerializeField] private Transform _targetEnemy;
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private GameObject _targetIndicator;
    private bool _isLockedOn = false;
    private bool _autoTargeting = true;
    private int _currentTargetIndex;
    private Dictionary<GameObject, float> _nearbyEnemies = new Dictionary<GameObject, float>();
    private Dictionary<GameObject, float> _visibleEnemies = new Dictionary<GameObject, float>();

    [Header("Blocking & Parry")]
    [SerializeField] private float _parryThreshold = 0.5f;
    [SerializeField] private float _parryDuration = 4f;
    [SerializeField] private float _parryCooldown;
    [SerializeField] private GameObject _parryZone;
    private float _blockHoldStartTime;
    private bool _isParrying;
    private bool _isBlocking;

    [Header("Combat & Weapons")]
    [SerializeField] private Transform _weaponHoldPoint;
    [SerializeField] private Animator _animator;
    [SerializeField] private NormalSwordAttack _basicAttack;
    private GameObject _equippedWeapon;
    public Weapon _currentWeapon;
    private float _lastAttackTime;

    [Header("Attack Settings")]
    [SerializeField] private float _lightAttackCooldown = 0.5f;
    [SerializeField] private float _heavyAttackCooldown = 1.5f;
    [SerializeField] public float _minChargeTime = 0.5f;
    [SerializeField] public float _maxChargeTime = 2f;
    [SerializeField] private float _heavyAttackMoveDistance = 1.5f;
    [SerializeField] private float _heavyAttackMoveDuration = 0.3f;
    [SerializeField, Range(0, 100)] private int _comboWindowPercentage = 30;

    private float _currentAttackCooldown;
    private float _chargeStartTime;
    private bool _isCharging;
    private AttackType _lastAttackType;
    private bool _canComboContinue;
    private bool _isInComboWindow;

    [Header("Inventory")]
    [SerializeField] private InventoryManager _inventoryManager;
    private ItemInstance _currentItem;

    [Header("Mimic")]
    [SerializeField] private MimicSpawner _mimicSpawner;
    [SerializeField] private GameObject _mimicClonePrefab;


    public UnityEvent ChargingUp;
    public UnityEvent UnCharge;

    public static PlayerCombat Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        _playerMovement = GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        GetComponent<Inventory>().ChangeSlot.AddListener(UpdateEquippedItem);
        _inventoryManager.ModifySlot.AddListener(UpdateEquippedItem);
        _playerData.DataChange.AddListener(ApplyPlayerStats);
    }

    private void OnDisable()
    {
        GetComponent<Inventory>().ChangeSlot.AddListener(UpdateEquippedItem);
        _inventoryManager.ModifySlot.AddListener(UpdateEquippedItem);
        _playerData.DataChange.AddListener(ApplyPlayerStats);
    }

    private void Update()
    {
        HandleBlocking();
        HandleLockOn();

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
        }

        UpdateTargetIndicator();
        HandleAttacks();
        _parryCooldown -= Time.deltaTime;
    }

    #region Combat Core
    private void HandleAttacks()
    {
        HandleChargedAttack();
        HandleSpecialAttack();
    }

    private void HandleChargedAttack()
    {
        
        // Start charging heavy attack
        if (Input.GetMouseButtonDown(0) && Time.time > _lastAttackTime + _currentAttackCooldown)
        {
            _isCharging = true;
            _chargeStartTime = Time.time;
            _playerMovement.SetMovementLock(true); // Lock movement during charge
            if (Time.time > _lastAttackTime + _currentAttackCooldown) {
                ChargingUp?.Invoke();
                Debug.Log("Charging up : " + Time.time + " and " + _lastAttackTime);
            }
            
        }

        // Execute attack on release
        if (Input.GetMouseButtonUp(0) && _isCharging)
        {
            _isCharging = false;
            _playerMovement.SetMovementLock(false); // Unlock movement

            float chargeTime = Time.time - _chargeStartTime;
            ExecuteAttack(chargeTime);
            UnCharge?.Invoke();
        }

        // Cancel charge if moving during charge time
        if (_isCharging && _playerMovement.IsMoving)
        {
            _isCharging = false;
            _playerMovement.SetMovementLock(false);
            UnCharge?.Invoke();
        }
    }


    private void HandleSpecialAttack()
    {
        if (Input.GetMouseButtonDown(1) && _currentWeapon != null)
        {
            _currentWeapon.Cast();

            GetComponent<Inventory>().BreakItem(GetComponent<Inventory>().equippedSlotNum, _currentWeapon.skillDurabilityCost);
        }
    }


    private void ExecuteAttack(float chargeTime)
    {
        _lastAttackTime = Time.time;

        if (chargeTime >= _minChargeTime)
        {
            // Heavy attack
            _currentAttackCooldown = _heavyAttackCooldown;
            _lastAttackType = AttackType.Heavy;

            // Calculate heavy attack damage and AOE based on charge time
            float chargePercent = Mathf.Clamp01((chargeTime - _minChargeTime) / (_maxChargeTime - _minChargeTime));
            float damageMultiplier = 1f + chargePercent;
            float aoeRadius = Mathf.Lerp(2f, 5f, chargePercent);
            UnCharge?.Invoke();
            ExecuteHeavyAttack(damageMultiplier, aoeRadius);
            StartCoroutine(HeavyAttackMovement());
        }
        else
        {
            // Light attack
            _currentAttackCooldown = _lightAttackCooldown;
            _lastAttackType = AttackType.Light;
            UnCharge?.Invoke();
            ExecuteLightAttack();
        }

        // Apply durability cost
        if (_currentWeapon != null)
        {
            int durabilityCost = _lastAttackType == AttackType.Light ?
                _currentWeapon.baseDurabilityCost :
                _currentWeapon.baseDurabilityCost * 2;

            UnCharge?.Invoke();
            GetComponent<Inventory>().BreakItem(GetComponent<Inventory>().equippedSlotNum, durabilityCost);
        }

        // Spawn mimic if applicable
        if (_mimicSpawner != null)
        {
            _mimicSpawner.TrySpawnMimic();
        }

        UnCharge?.Invoke();
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
            
            _basicAttack.ExecuteLightAttack();
            TriggerAttackAnimation("LightAttack");
        }
    }

    private void ExecuteHeavyAttack(float damageMultiplier, float aoeRadius)
    {
        if (_isLockedOn && _targetEnemy != null)
        {
            // Attack in locked direction
            _basicAttack.ExecuteHeavyAttack(
                _targetEnemy.position,
                damageMultiplier,
                aoeRadius
            );
        }
        else
        {
            // Attack in facing direction
            Vector3 attackDirection = _playerMovement.GetDirection();
            Vector3 attackPosition = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z) + attackDirection * 2f;
            
            _basicAttack.ExecuteHeavyAttack(
                attackPosition,
                damageMultiplier,
                aoeRadius
            );
        }
        TriggerAttackAnimation("HeavyAttack");
    }



    private IEnumerator HeavyAttackMovement()
    {
        float elapsed = 0;
        Vector3 startPos = transform.position;
        Vector3 moveDirection = _isLockedOn && _targetEnemy != null ?
            (_targetEnemy.position - transform.position).normalized :
            transform.forward;

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

        while (elapsed < 0.2f)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / 0.15f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (isLightAttack)
        {
            _basicAttack.ExecuteAttack(AttackType.Light);
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
            // Heavy attacks reset combo
            ResetCombo();
            _animator.SetTrigger("HeavyAttack");
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

    public void ResetParryCooldown()
    {
        _parryCooldown = _playerData.ParryDuration;
    }

    private IEnumerator ParryDuration()
    {
        _parryCooldown = _parryDuration;
        yield return new WaitForSeconds(0.5f);

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
            _currentWeapon.CurrDurability = item.Durability;
        }
    }

    private void ConfigureWeaponPhysics(GameObject weapon)
    {
        Collider weaponCollider = weapon.GetComponent<Collider>();
        if (weaponCollider != null) weaponCollider.isTrigger = true;

        Rigidbody weaponRb = weapon.GetComponent<Rigidbody>();
        if (weaponRb != null) weaponRb.isKinematic = true;
    }

    private void ClearWeapon()
    {
        if (_equippedWeapon != null)
        {
            Destroy(_equippedWeapon);
        }
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
            if (enemy.Key == null) continue;

            if (IsEnemyVisible(enemy.Key))
            {
                _visibleEnemies.Add(enemy.Key, enemy.Value);
            }
        }
    }


    private void ScanForNearbyEnemies()
    {
        _nearbyEnemies.Clear();

        // Define detection parameters
        float detectionRadius = 15f;
        Vector3 detectionCenter = transform.position + Vector3.up * 1f;

        // Detect all enemies in radius
        Collider[] hitColliders = Physics.OverlapSphere(
            detectionCenter,
            detectionRadius,
            _enemyLayer
        );

        // Process detected enemies
        foreach (var hitCollider in hitColliders)
        {
            GameObject enemy = hitCollider.gameObject;

            // Skip if enemy is null or already dead
            if (enemy == null) continue;

            // Calculate distance to player
            float distance = Vector3.Distance(
                transform.position,
                enemy.transform.position
            );

            // Add to nearby enemies dictionary
            if (!_nearbyEnemies.ContainsKey(enemy))
            {
                _nearbyEnemies.Add(enemy, distance);
            }
            else
            {
                _nearbyEnemies[enemy] = distance;
            }
        }

    }

    private bool IsEnemyVisible(GameObject enemy)
    {
        Vector3 direction = (enemy.transform.position - transform.position).normalized;
        float angle = Vector3.Angle(_playerMovement.GetDirection(), direction);

        return angle <= _lockOnAngleThreshold && HasLineOfSight(enemy);
    }

    public void EnableMimic(bool enable)
    {
        _mimicSpawner.enabled = enable;
    }
    public void SetUpMimic(MimicSpawner mimic)
    {
        _mimicSpawner = mimic;
        _mimicSpawner._mimicClonePrefab = _mimicClonePrefab;
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
        if (Input.GetKeyDown(KeyCode.G))
        {
            _autoTargeting = false;
            _currentTargetIndex = (_currentTargetIndex + 1) % _visibleEnemies.Count;
            SelectTargetByIndex();
        }
    }

    private void SelectTargetByIndex()
    {
        if (_visibleEnemies.Count == 0) return;

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
        //_playerDamage = _playerData.Damage;
        _parryDuration = _playerData.ParryDuration;
    }
    #endregion
}