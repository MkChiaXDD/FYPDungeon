using MaykerStudio.Demo;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
    [SerializeField] private bool Tutorial;
    [SerializeField] public bool DisableCombat;

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

    [SerializeField] private GameObject _TestObj;

    [Header("Blocking & Parry")]
    [SerializeField] private float _parryThreshold = 0.5f;
    [SerializeField] private float _parryDuration = 4f;
    [SerializeField] private GameObject _parryZone;

    [Header("Combat & Weapons")]
    [SerializeField] private float _damageMultiplier;
    [SerializeField] private float _aoeRadius;
    [SerializeField] private Vector3 _attackPosition;
    [SerializeField] private Transform _weaponHoldPoint;
    [SerializeField] private Animator _animator;
    [SerializeField] private BaseAttackScript _currentBasicAttack;
    [SerializeField] private BaseAttackScript baseBasicAttack;
    [SerializeField] private Material breakMaterial;

    [Header("Attack Settings")]
    public float _baselightAttackCooldown = 0.5f;
    public float _baseheavyAttackCooldown = 1.5f;
    public float _baseminChargeTime = 0.1f;
    public float _basemaxChargeTime = 2f;
    [SerializeField] private float _baseheavyAttackMoveDistance = 1.5f;
    [SerializeField] private float _baseheavyAttackMoveDuration = 0.3f;

    [SerializeField] private float _baselightAttackMoveDistance = 1.5f;
    [SerializeField] private float _baselightAttackMoveDuration = 0.3f;

    public float _currentlightAttackCooldown = 0.5f;
    public float _currentheavyAttackCooldown = 1.5f;

    public float _ultimateAttackCooldown = 1.5f;
    public float _currentminChargeTime = 0.1f;
    public float _currentmaxChargeTime = 2f;
    [SerializeField] private float _currentheavyAttackMoveDistance = 1.5f;
    [SerializeField] private float _currentheavyAttackMoveDuration = 0.3f;

    [SerializeField] private float _currentlightAttackMoveDistance = 1.5f;
    [SerializeField] private float _currentlightAttackMoveDuration = 0.3f;
    [SerializeField, Range(0, 100)] private int _comboWindowPercentage = 30;



    [Header("Attack Recovery")]
    [SerializeField] private float _attackRecoveryTime = 0.1f; // Time to recover after each attack
    private bool _isInRecovery = false;

    [Header("Inventory")]
    [SerializeField] private InventoryManager _inventoryManager;

    [Header("Mimic")]
    [SerializeField] private MimicSpawner _mimicSpawner;
    [SerializeField] private GameObject _mimicClonePrefab;

    [Header("AbilityIcons")]
    [SerializeField] private GameObject dashBasicAttackIcon;
    [SerializeField] private GameObject scratchBasicAttackIcon;

    [SerializeField] private GameObject hammerBasicAttackIcon;
    [SerializeField] private GameObject hammerUltAttackIcon;

    [SerializeField] private GameObject swordBasicAttackIcon;
    [SerializeField] private GameObject swordUltAttackIcon;

    [Header("Skill References")]
    [SerializeField] private CooldownSystem basicAttackCooldown;
    [SerializeField] private CooldownSystem ultimateCooldown;



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
    public List<GameObject> _Enemy = new List<GameObject>();

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
    public event Action<string> OnAction;
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
        UpdateEquippedItem();
        if (_playerMovement == null)
        {
            _playerMovement = GetComponent<PlayerMovement>();
        }

        _currentBasicAttack = baseBasicAttack;

        if (FindFirstObjectByType<TutorialProggresion>())
        {
            Tutorial = true;
        }
        else
        {
            Tutorial = false;
        }
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
        if (DisableCombat == true || GamStates.instance.State == GamStates.GameState.Paused)
        {
            Uncharge?.Invoke();
            return;
        }

        //HasLineOfSightTesting(_TestObj);

        SetWeaponAnim();
        //HandleBlocking();
        HandleLockOn();
        // Update recovery timer
        if (_isInRecovery && Time.time > _lastAttackTime + _currentAttackCooldown + _attackRecoveryTime)
        {
            _isInRecovery = false;
        }


        _targetIndicator.SetActive(false);
        
        if (_isLockedOn)
        {
            _targetIndicator.SetActive(true);
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
    private void ExecuteAttack(float chargeTime)
    {
        if (_isInRecovery) return; // Prevent attacking during recovery

        _lastAttackTime = Time.time;
        _isInRecovery = true;

        if (chargeTime >= _currentminChargeTime)
        {
            // Heavy attack
            _currentAttackCooldown = _currentheavyAttackCooldown;
            _lastAttackType = AttackType.Heavy;

            float chargePercent = Mathf.Clamp01((chargeTime - _currentminChargeTime) / (_currentmaxChargeTime - _currentminChargeTime));
            _damageMultiplier = 1f + chargePercent;
            _aoeRadius = Mathf.Lerp(2f, 5f, chargePercent);

            if (Tutorial)
            {
                OnAction?.Invoke("HeavyAttack");
            }
            Uncharge?.Invoke();

            //Remove and put at aniamtion event
            ExecuteHeavyAttack(_damageMultiplier, _aoeRadius);
            StartCoroutine(HeavyAttackMovement());
        }
        else
        {
            // Light attack
            _currentAttackCooldown = _currentlightAttackCooldown;
            _lastAttackType = AttackType.Light;


            if (Tutorial)
            {
                OnAction?.Invoke("NormalAttack");
            }

            //Remove and put at aniamtion event
            ExecuteLightAttack();
            StartCoroutine(LightAttackMovement());
        }

        //ApplyDurabilityCost();
        TrySpawnMimic();
    }
    private void HandleAttacks()
    {
        if (_isInRecovery) return; // Prevent charging during recovery

        HandleChargedAttack();
        HandleSpecialAttack();
    }
    private void HandleChargedAttack()
    {
        if (basicAttackCooldown.IsOnCooldown)
        {
            Debug.Log("Basic is on cooldown!");
            return;
        }

        float ChargedSlowDownEffect = 0.25f;
        float BaseMovementModifier = 1f;
        // Start charging heavy attack
        if (Input.GetMouseButtonDown(0))
        {
            _playerMovement.LockMouse();
            if (Time.time > _lastAttackTime + _currentAttackCooldown)
            {
                if (_currentWeapon)
                {
                    if (_currentWeapon.weaponData.weaponType == WeaponType.Sword)
                    {
                        _isLockedOn = true;
                    }
                }

                _isCharging = true;
                _chargeStartTime = Time.time;
                _playerMovement.ChangePlayerMovementModifier(ChargedSlowDownEffect);
                ChargeUp?.Invoke();

            }
        }

        // Execute attack on release
        if (Input.GetMouseButtonUp(0) && _isCharging)
        {

            _isCharging = false;
            _playerMovement.ChangePlayerMovementModifier(BaseMovementModifier);


            float chargeTime = Time.time - _chargeStartTime;

            ExecuteAttack(chargeTime);

            basicAttackCooldown.StartCooldown(_baselightAttackCooldown);
            Uncharge?.Invoke();
            ClearTargeting();
        }
    }
    private void HandleSpecialAttack()
    {
        if (!_currentWeapon)
            return;

        if (Input.GetMouseButtonDown(1))
        {
            if (ultimateCooldown.IsOnCooldown)
            {
                Debug.Log("Basic is on cooldown!");
                return;
            }

            if (_inventory.GetItemDurability() >= _currentWeapon.weaponData.MaxDurability / 2)
            {
                
                Debug.Log(_currentWeapon.CurrDurability + " " + _currentWeapon.weaponData.MaxDurability);

                _playerMovement.LockMouse();

                _currentWeapon.Cast();
                ApplyDurabilityCost(23);
                _playerMovement.UnLockMouse();
                ultimateCooldown.StartCooldown(_ultimateAttackCooldown);
            }
            else
            {
                Debug.Log("Need to be max!");
                return;
            }
        }
    }
    private void ExecuteLightAttack()
    {

        Uncharge?.Invoke();
        //_currentBasicAttack.ExecuteLightAttack();
        TriggerAttackAnimation("LightAttack");
        PlayAttackSound("BasicAttack");
        Debug.Log("LIGHT ATTACK");


        if (Tutorial)
        {
            OnAction?.Invoke("NormalAttack");
        }
    }

    public void StartLightAttack()
    {
        if (_playerData.elementType != ElementType.None)
        {
          
            _currentBasicAttack.ExecuteLightAttack(_playerData.elementType);
            return;
        }
        _currentBasicAttack.ExecuteLightAttack();
        ApplyDurabilityCost();
    }

    public void StartHeavyAttack()
    {
        if (_playerData.elementType == ElementType.None)
        {
            _currentBasicAttack.ExecuteLightAttack(_playerData.elementType);
            return;
        }
        _currentBasicAttack.ExecuteHeavyAttack(_attackPosition, _damageMultiplier, _aoeRadius);
        ApplyDurabilityCost();
    }

    private IEnumerator LightAttackMovement()
    {
        float elapsed = 0;
        Vector3 moveDirection = _isLockedOn && _targetEnemy != null
            ? (_targetEnemy.position - transform.position).normalized
            : _playerMovement.GetDirection();

        // Calculate force vector
        Vector3 force = moveDirection * (_currentlightAttackMoveDistance / _currentlightAttackMoveDuration) * _playerMovement._rb.mass;

        while (elapsed < _currentlightAttackMoveDuration)
        {
            // Apply force for more physical movement
            _playerMovement._rb.AddForce(force * Time.deltaTime, ForceMode.VelocityChange);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Immediately dampen the velocity after the attack
        _playerMovement._rb.velocity = Vector3.zero;
    }

    
    private void ExecuteHeavyAttack(float damageMultiplier, float aoeRadius)
    {


        if (_currentWeapon)
        {
            if (_currentWeapon.weaponData.weaponType == WeaponType.Sword)
            {
                if (_isLockedOn && _targetEnemy != null)
                {
                    Vector3 DashPos = _targetEnemy.position + (_targetEnemy.forward * 1f);
                    StartCoroutine(DashToAttack(DashPos, true));
                }
            }
        }



        _attackPosition = _isLockedOn && _targetEnemy != null
            ? _targetEnemy.position
            : transform.position + _playerMovement.GetDirection() * 2f + Vector3.up * ATTACK_HEIGHT_OFFSET;

        //_currentBasicAttack.ExecuteHeavyAttack(_attackPosition, damageMultiplier, aoeRadius);
        TriggerAttackAnimation("HeavyAttack");
    }

    private IEnumerator HeavyAttackMovement()
    {
        float elapsed = 0;
        Vector3 moveDirection = _isLockedOn && _targetEnemy != null
            ? (_targetEnemy.position - transform.position).normalized
            : _playerMovement.GetDirection();

        // Calculate force vector
        Vector3 force = moveDirection * (_currentheavyAttackMoveDistance / _currentheavyAttackMoveDuration) * _playerMovement._rb.mass;

        while (elapsed < _currentheavyAttackMoveDuration)
        {
            // Apply force for more physical movement
            _playerMovement._rb.AddForce(force * Time.deltaTime, ForceMode.VelocityChange);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Immediately dampen the velocity after the attack
        _playerMovement._rb.velocity = Vector3.zero;
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
                Debug.Log("Combo ");
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

    private void ApplyDurabilityCost(int durabilityUsage)
    {
        if (_currentWeapon == null) return;

        //if (_currentWeapon.CurrDurability != _currentWeapon.weaponData.MaxDurability)
        //{
        //    return;
        //}
        _inventory.BreakItem(_inventory.equippedSlotNum, durabilityUsage);
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
        ClearCurrentWeapon();
        EquipItem();
    }

    private void EquipItem()
    {
        _currentItem = _inventoryManager.GetCurrentHotbarItem();
        CreateWeaponHoldingInstance(_currentItem);

        if (_currentWeapon != null)
        {
            UpdateWeaponHeld(_currentItem);
        }
        else
        {
            ResetWeaponHeld();
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

    private void SetWeaponAnim()
    {
        if (_currentWeapon == null)
        {
            _animator.SetInteger("Weapon", 0);
            return;
        }

        if (_currentWeapon.weaponData.weaponType == WeaponType.Sword)
        {
            _animator.SetInteger("Weapon" , 1);
        }
        else if (_currentWeapon.weaponData.weaponType == WeaponType.Hammer)
        {
            _animator.SetInteger("Weapon", 2);
        }
    }
    private void ClearCurrentWeapon()
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
        UpdateSkillIcons(WeaponType.Unarmed);
        ResetWeaponHeld();
    }

    private void CreateWeaponHoldingInstance(ItemInstance item)
    {
        if (item?.ItemPrefab == null)
        {
            Debug.LogWarning("no weapons found!");
            return;

        }
        ;
        _equippedWeapon = Instantiate(item.ItemPrefab, _weaponHoldPoint);
        ConfigureWeaponPhysics(_equippedWeapon);

        _currentWeapon = _equippedWeapon.GetComponent<Weapon>();

    }

    private void UpdateSkillIcons(WeaponType weaponType)
    {
        // First, deactivate all icons
        dashBasicAttackIcon.SetActive(false);
        scratchBasicAttackIcon.SetActive(false);
        hammerBasicAttackIcon.SetActive(false);
        hammerUltAttackIcon.SetActive(false);
        swordBasicAttackIcon.SetActive(false);
        swordUltAttackIcon.SetActive(false);

        // Activate the appropriate icons based on weapon type
        switch (weaponType)
        {
            case WeaponType.Hammer:
                hammerBasicAttackIcon.SetActive(true);
                hammerUltAttackIcon.SetActive(true);
                break;

            case WeaponType.Sword:
                swordBasicAttackIcon.SetActive(true);
                swordUltAttackIcon.SetActive(true);
                break;

            case WeaponType.Unarmed:
            default:
                // Default to scratch/dash icons when no weapon equipped
                dashBasicAttackIcon.SetActive(true);
                scratchBasicAttackIcon.SetActive(true);
                break;
        }
    }

    private void UpdateWeaponHeld(ItemInstance item)
    {
        UpdateSkillIcons(_currentWeapon.weaponData.weaponType);
        _currentlightAttackCooldown = _currentWeapon._lightAttackCooldown;
        _currentheavyAttackCooldown = _currentWeapon._heavyAttackCooldown;
        _currentminChargeTime = _currentWeapon._minChargeTime;
        _currentmaxChargeTime = _currentWeapon._maxChargeTime;
        _playerMovement.ChangePlayerMovementModifier(_currentWeapon.movementModifier);
        _currentBasicAttack = _currentWeapon.weaponData.baseAttackScript;
        _currentWeapon.CurrDurability = item.Durability;
    }

    private void ResetWeaponHeld()
    {
        UpdateSkillIcons(WeaponType.Unarmed);
        _currentlightAttackCooldown = _baselightAttackCooldown;
        _currentheavyAttackCooldown = _baseheavyAttackCooldown;
        _currentminChargeTime = _baseminChargeTime;
        _currentmaxChargeTime = _basemaxChargeTime;
        _playerMovement.ResetPlayerMovementModifier();
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
        _Enemy.Clear();
        _visibleEnemies.Clear();
        ScanForNearbyEnemies();

        foreach (var enemy in _nearbyEnemies)
        {
            if (enemy.Key != null && IsEnemyVisible(enemy.Key))
            {
                _visibleEnemies.Add(enemy.Key, enemy.Value);
                _Enemy.Add(enemy.Key);
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
        Vector3 origin = new Vector3(transform.position.x, 1, transform.position.z) + Vector3.up * 0.5f;
        Vector3 targetPos = new Vector3(target.transform.position.x, 1, target.transform.position.z) + Vector3.up * 0.5f;
        Vector3 direction = (targetPos - origin).normalized;
        float distance = Vector3.Distance(origin, targetPos);

        

        return !Physics.Raycast(origin, direction, distance, _ignoreLayerMask);
    }

    //private bool HasLineOfSightTesting(GameObject target)
    //{
    //    Vector3 origin = transform.position + Vector3.up * 0.5f;
    //    Vector3 targetPos = target.transform.position + Vector3.up * 0.5f;
    //    Vector3 direction = (targetPos - origin).normalized;
    //    float distance = Vector3.Distance(origin, targetPos);


    //    Debug.DrawLine(origin, targetPos, Color.red, 1.0f);

    //    bool hasObstacle = Physics.Raycast(origin, direction, distance, _ignoreLayerMask);
    //    Debug.Log(hasObstacle);

    //    return hasObstacle;
    //}

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
        _isLockedOn = false;
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
        _playerMovement.UnLockMouse();
    }

    public void EnableComboWindow(float animationDuration)
    {
        _animator.SetBool("Combo", false);
        if (_comboWindowPercentage == 0)
        {
            StartCoroutine(OpenComboWindow(0));
            return;
        }
        float windowStartTime = animationDuration * (_comboWindowPercentage / 100f);
        
        StartCoroutine(OpenComboWindow(windowStartTime));
    }

    private IEnumerator OpenComboWindow(float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log("Combo Can Start");
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