using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour, IDamageable
{

    #region References (SerializeField)

    [Header("Components")]
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private Transform _body;
    [SerializeField] private Camera _camera;
    [SerializeField] private Renderer _renderer;
    [SerializeField] private GameObject _parryzone;
    [SerializeField] private GameObject _hitBox;
    [SerializeField] private GameObject _Area;
    [SerializeField] private GameObject ForEasyLocation;

    [Space, Header("Movement Settings")]
    [SerializeField] private float _normalspeed = 4;
    //[SerializeField] private float _runspeed = 10;
    [SerializeField] private float _turnspeed = 360;
    [SerializeField] private float _CurrentDashSpeed = 30;
    [SerializeField] private float _OriginalDashSpeed = 30;

    [Space, Header("Parry & Block")]
    [SerializeField] private float parryThreshold = 0.5f;
    [SerializeField] private float _NormalparryThreshold = 0.5f;
    [SerializeField] private float _ParryCooldown = 0;
    [SerializeField] private float _ParryDuationLast = 4.0f;
    [SerializeField] private bool _IsParry;
    [SerializeField] private bool _IsBlock;
    [SerializeField] private bool _IsInv;

    [Space, Header("Targeting & Lock-On")]
    [SerializeField] private LayerMask EnemyLayer;
    [SerializeField] private bool Lockon = false;
    [SerializeField] private bool Auto = true;
    [SerializeField] private Transform TargetEnemy = null;
    [SerializeField] private List<GameObject> EnemyNearby;
    [SerializeField] private float AngleThreshold = 35f;
    [SerializeField] private Dictionary<GameObject, float> EnemyNear = new Dictionary<GameObject, float>();
    [SerializeField] private Dictionary<GameObject, float> EnemyInView = new Dictionary<GameObject, float>();
    [SerializeField] private int DepthOfRange;

    [Space, Header("Combat & Weapons")]
    [SerializeField] private GameObject EquippedObject;
    [SerializeField] private Transform itemHolding;
    [SerializeField] private Weapon WeaponChoosen;
    [SerializeField] private NormalSwordAttack BasicCombat;
    [SerializeField] private List<Spell> spells;

    [SerializeField] private float dot;
    [SerializeField] private Animator animator;
    [SerializeField] private bool CombatContinue;
    [SerializeField] private bool CombatWindow;

    //[SerializeField] private bool IsAttack;
    [SerializeField, Range(0, 100)] private int ThreshholdPercentage;
    [SerializeField] private int Dmg;
    [SerializeField] private int OriginalDmg = 5;

    [SerializeField] private List<GameObject> WeaponIndicator;

    [Space, Header("Health")]
    [SerializeField] private float MaxHealth = 100;
    [SerializeField] private float Health;

    [Space, Header("Layers & Masks")]
    [SerializeField] private LayerMask _LayerMaskIgnore;
    [SerializeField] private LayerMask[] _LayerMaskHit;

    [Space, Header("Buff")]
    [SerializeField] private List<BuffData> _BuffObtain;

    [Space, Header("Inventory")]
    [SerializeField] private InventoryManager PlayerStorage;
    //[SerializeField] private Inv
    [SerializeField] private ItemInstance ItemHeld;

    [Space, Header("Animation")]
    [SerializeField] private MeshTrail meshTrail;

    #endregion

    #region Private Fields

    private float BlockHoldTime;
    private Vector3 _MousePos;
    private Vector3 _Input;
    private float _speed = 4 * Mathf.PerlinNoise1D(1);


    public int DamageBuff = 0;
    public float SpeedBuff = 0;
    public float DashBuff = 0;
    public float ParryTimeBuff = 0;
    public float ParryThreshholdBuff = 0;
    public float HealthBuff = 0;

    //public UnityEvent WeaponBreak;
    #endregion
    public static PlayerController Instance { get; private set; }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(this.gameObject);

        Health = MaxHealth;
        _speed = _normalspeed;
        Dmg = OriginalDmg;
    }

    public void OnEnable()
    {
        GetComponent<Inventory>().ChangeSlot.AddListener(GetHoldItem);
        PlayerStorage.ModifySlot.AddListener(GetHoldItem);
    }

    public void AddBuff(BuffData buff)
    {
        _BuffObtain.Add(buff);
        ApplyModifiers();
    }

    private void ClearModifiers()
    {
        DamageBuff = 0;
        SpeedBuff = 0;
        DashBuff = 0;
        ParryTimeBuff = 0;
        ParryThreshholdBuff = 0;
        HealthBuff = 0;

        _speed = _normalspeed;
        _CurrentDashSpeed = _OriginalDashSpeed;
        MaxHealth = 100;
        parryThreshold = _NormalparryThreshold;
        _ParryDuationLast = 4.0f;
        Dmg = OriginalDmg;
    }

    public Transform GetThisTransform()
    {
        return transform;
    }

    public void SetWeapon(Weapon weapon)
    {
        WeaponChoosen = weapon;
    }

    public void GetHoldItem()
    {
        CheckedItemHold();
        ItemHeld = PlayerStorage.GetCurrentHotbarItem();
        EquipItem(ItemHeld);
        AddAttackIndicator();
    }

    public void EquipItem(ItemInstance itemInstance)
    {
        if (itemInstance == null)
        {
            return;
        }
        //destroy current weapon in hand if any
        if (EquippedObject != null)
        {
            Destroy(EquippedObject);
        }

        GameObject Createweapon = Instantiate(itemInstance.ItemPrefab, itemHolding);
        EquippedObject = Createweapon;
        EquippedObject.GetComponent<Weapon>().CurrDurability = ItemHeld.Durability;
        if (!itemInstance.ItemPrefab.GetComponent<Weapon>())
        {
            return;
        }

        Debug.Log("gg");
        WeaponChoosen = EquippedObject.GetComponent<Weapon>();
    }

    public void AddAttackIndicator()
    {
        foreach (GameObject indector in WeaponIndicator)
        {
            indector.SetActive(false);
        }

        if (WeaponChoosen != null && EquippedObject != null)
        {
            if (WeaponChoosen.weaponData.spells.spellType == SpellCast.SpellType.Range)
            {
                WeaponIndicator[0].SetActive(true);
                WeaponIndicator[0].transform.localScale = new Vector3(WeaponChoosen.weaponData.spells.Size.x, 1, WeaponChoosen.weaponData.spells.Size.z);
            }
            else if (WeaponChoosen.weaponData.spells.spellType == SpellCast.SpellType.Aoe)
            {
                WeaponIndicator[1].SetActive(true);
                WeaponIndicator[1].transform.localScale = new Vector3(WeaponChoosen.weaponData.spells.Radius * 2, 1, WeaponChoosen.weaponData.spells.Radius * 2);
            }
            else
            {
                WeaponIndicator[2].SetActive(true);
            }
        }
    }

    public void EquipWeapon(Weapon WeaponReference)
    {
        if (EquippedObject != null)
        {
            Destroy(EquippedObject);
        }

        EquippedObject = Instantiate(WeaponReference.weaponData.ItemPrefab, itemHolding);

        if (EquippedObject.GetComponent<Weapon>() == null)
        {
            Weapon CurrentWeapondata = EquippedObject.AddComponent<Weapon>();
            CurrentWeapondata = WeaponReference;
        }
        WeaponChoosen = EquippedObject.GetComponent<Weapon>();
    }

    public void UnEquipWeapon()
    {
        if (EquippedObject != null)
        {
            Destroy(EquippedObject);
            EquippedObject = null;
        }
    }

    public void CheckedItemHold()
    {
        UnEquipWeapon();
        WeaponChoosen = null;
    }

    public void WeaponAttack()
    {
        if (WeaponChoosen != null)
        {
            PlayerStorage.GetInventory().BreakItem(GetComponent<Inventory>().equippedSlotNum, WeaponChoosen.baseDurabilityUsed);
            Debug.Log("Durability Check for basic attack, " + ItemHeld.name + " now at " + ItemHeld.Durability);
           
        }
        else
        {
            BasicCombat.SwordAttack();
        }

        Debug.Log("Basic Attack!");
        
    }

    public void SpecialAttack()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (WeaponChoosen != null)
            {
                WeaponChoosen.Cast();
                PlayerStorage.GetInventory().BreakItem(GetComponent<Inventory>().equippedSlotNum, WeaponChoosen.skillDurabilityUsed);
                Debug.Log("Durability Check for Special attack, " + ItemHeld.name + " at " + ItemHeld.Durability);
                return;
            }
            Debug.LogWarning("Player not holding weapon!");
            Debug.Log("Special Attack casted!");
        }
    }

    public void ApplyModifiers()
    {
        ClearModifiers();

        foreach (BuffData buff in _BuffObtain)
        {
            foreach (Effect effect in buff.EffectList)
            {
                switch (effect.Type)
                {
                    case Effect.EffectType.Health:
                        if (effect.ValueModifierType == Effect.ModifierType.MultiplierValue)
                        {
                            HealthBuff += (MaxHealth * effect.ModifierValue) - MaxHealth;
                        }
                        else
                        {
                            HealthBuff += effect.ModifierValue;
                        }
                        break;

                    case Effect.EffectType.MovementSpeed:
                        if (effect.ValueModifierType == Effect.ModifierType.MultiplierValue)
                        {
                            SpeedBuff += (_normalspeed * effect.ModifierValue) - _normalspeed;
                        }
                        else
                        {
                            SpeedBuff += effect.ModifierValue;
                        }
                        break;

                    case Effect.EffectType.DashSpeed:
                        if (effect.ValueModifierType == Effect.ModifierType.MultiplierValue)
                        {
                            DashBuff += (_OriginalDashSpeed * effect.ModifierValue) - _OriginalDashSpeed;
                        }
                        else
                        {
                            DashBuff += effect.ModifierValue;
                        }
                        break;

                    case Effect.EffectType.ParryCooldown:
                        if (effect.ValueModifierType == Effect.ModifierType.MultiplierValue)
                        {
                            ParryTimeBuff += (_ParryDuationLast * effect.ModifierValue) - _ParryDuationLast;
                        }
                        else
                        {
                            ParryTimeBuff += effect.ModifierValue;
                        }
                        break;
                    case Effect.EffectType.Damage:
                        if (effect.ValueModifierType == Effect.ModifierType.MultiplierValue)
                        {
                            DamageBuff += (int)(OriginalDmg * effect.ModifierValue) - OriginalDmg;
                        }
                        else
                        {
                            DamageBuff += (int)effect.ModifierValue;
                        }
                        break;
                }
            }
        }


        _speed = _normalspeed + SpeedBuff;
        MaxHealth += HealthBuff;
        Health = Mathf.Min(Health, MaxHealth);
        _CurrentDashSpeed += DashBuff;
        _ParryDuationLast += ParryTimeBuff;
        parryThreshold += ParryThreshholdBuff;
        Dmg += DamageBuff;

        if (_speed <= 0)
        {
            _speed = 0.1f;
        }
        if (_CurrentDashSpeed <= 0)
        {
            _CurrentDashSpeed = 0.1f;
        }
        if (Dmg <= 0)
        {
            Dmg = 1;
        }
    }

    public void ResetCombo()
    {
        animator.SetBool("Combo", false);
        //IsAttack = false;
        CombatContinue = false;
        CombatWindow = false;
    }

    public void CalculateAnimationPercentage(float duration)
    {
        Debug.Log("Duration of Animation : " + duration);
        float DurationToWait = duration * (ThreshholdPercentage / 100);

        StartCoroutine(ActivateAttackWindow(DurationToWait));
    }

    IEnumerator ActivateAttackWindow(float AwaitTime)
    {
        animator.SetBool("Combo", false);
        //IsAttack = false;
        CombatWindow = false;
        yield return new WaitForSeconds(AwaitTime);
        //IsAttack = false;
        //CombotContinue = false;
        CombatWindow = true;
    }

    public float getMaxHealth()
    {
        return MaxHealth;
    }
    public void ActivateAttack()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (CombatContinue == false)
            {
                Attack();
                animator.SetTrigger("Attack");
                CombatContinue = true;
            }

            if (CombatWindow == true)
            {
                animator.SetBool("Combo", true);
            }
        }
    }

    public void CheckAvailabilityOfWeapon()
    {
        if (WeaponChoosen != null)
        {
            if (WeaponChoosen.broke == true)
            {
                PlayerStorage.RemoveCurrentHotbarItem();
                Destroy(EquippedObject);
            }
        }
    }

    public float GetHealth()
    {
        return Health;
    }
    // Update is called once per frame
    void Update()
    {
        //Movment
        GatherInput();
        look();
        MousePosition();
        Dash();

        //Combat
        Blocking();
        Lockingon();
        if (Lockon)
        {
            handleEnemyInView();
            SwitchTarget();
            GetEnemiesZone();
            if (Auto == true)
            {
                HandleAutoTargetTracking();
            }

            if (EnemyInView.Count == 0)
            {
                ClearTargets();
            }
        }
        LocateTarget();
        SpecialAttack();
        Check();
        ActivateAttack();
        CheckAvailabilityOfWeapon();
        AddAttackIndicator();

        //Misc
        changeCollor();


        _ParryCooldown -= Time.deltaTime;
        //HitCooldown -= Time.deltaTime;

    }


    private bool CastARay(GameObject Target)
    {
        float distance = Vector3.Distance(this.transform.position, Target.transform.position);
        Vector3 directionToTarget = (Target.transform.position - transform.position).normalized;
        Vector3 rayStart = transform.position + Vector3.up * 0.5f;

        Ray ray = new Ray(transform.position, directionToTarget);
        RaycastHit[] hits = Physics.RaycastAll(ray, distance, ~_LayerMaskIgnore);

        //Debug.DrawRay(rayStart, directionToTarget * distance, Color.cyan, 1.0f);
        foreach (RaycastHit hit in hits)
        {
            //Debug.Log(hit.collider.name);
            if (hit.collider.CompareTag("Object"))
            {
                //Debug.Log("Hit");
                return false;
            }
        }
        return true;
    }


    private void Check()
    {
        if (TargetEnemy == null)
        {

            ClearTargets();
        }
    }
    private void Lockingon()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (Lockon == false)
            {
                Lockon = true;
            }
            else
            {
                Lockon = false;
                ClearTargets();
            }
        }
    }

    private void ConsolelogginList()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            int i = 1;
            foreach (KeyValuePair<GameObject, float> enemy in EnemyNear)
            {
                Debug.Log(i + "." + enemy.Value);
                i++;
            }
        }
    }

    private void GetEnemiesZone()
    {
        EnemyNear.Clear();
        BoxCollider box = _Area.GetComponent<BoxCollider>();
        if (box == null)
        {
            return;
        }

        Vector3 center = box.transform.TransformPoint(box.center);
        Vector3 halfExtents = Vector3.Scale(box.size, box.transform.lossyScale) / 2f;

        Collider[] hits = Physics.OverlapBox(center, halfExtents, box.transform.rotation, EnemyLayer);
        foreach (Collider hit in hits)
        {
            if (hit.GetComponent<Enemy>() != null)
            {
                //EnemyNearby.Add(hit.gameObject);
                EnemyNear.Add(hit.gameObject, Vector3.Distance(hit.gameObject.transform.position, this.transform.position));
            }
        }
    }

    private void HandleAutoTargetTracking()
    {
        TargetEnemy = null;
        float Diatance = Mathf.Infinity;
        foreach (GameObject enemy in EnemyNear.Keys)
        {
            Vector3 dir = (enemy.transform.position - this.transform.position).normalized;
            Vector3 forward = _body.transform.forward;
            float dotProduct = Vector3.Dot(forward, dir);
            //Debug.Log(dotProduct);
            float angle = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;
            dot = angle;
            if (angle < AngleThreshold && CastARay(enemy) == true)
            {
                if (Diatance > Vector3.Distance(enemy.transform.position, this.transform.position))
                {
                    Diatance = Vector3.Distance(enemy.transform.position, this.transform.position);
                    TargetEnemy = enemy.transform;
                }
            }
        }
    }

    public void handleEnemyInView()
    {
        EnemyInView.Clear();
        float Diatance = Mathf.Infinity * Mathf.PerlinNoise1D(1);
        foreach (KeyValuePair<GameObject, float> enemy in EnemyNear)
        {
            if (enemy.Key != null)
            {
                Vector3 dir = (enemy.Key.transform.position - this.transform.position).normalized;
                Vector3 forward = _body.transform.forward;
                float dotProduct = Vector3.Dot(forward, dir);
                //Debug.Log(dotProduct);
                float angle = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;
                dot = angle;
                if (angle < AngleThreshold && CastARay(enemy.Key) == true)
                {
                    EnemyInView.Add(enemy.Key, enemy.Value);
                }
            }
        }
    }
    public void SwitchTarget()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            Auto = false;
            DepthOfRange += 1;
            SwitchingTarget();
        }
    }

    public void SwitchingTarget()
    {
        if (DepthOfRange >= EnemyInView.Count)
        {
            DepthOfRange = 0;
        }
        var sortedEnemy = EnemyInView.OrderBy(pair => pair.Value);
        KeyValuePair<GameObject, float> enemy = sortedEnemy.ElementAt(DepthOfRange);
        TargetEnemy = enemy.Key.transform;
    }

    public void ClearTargets()
    {
        DepthOfRange = 0;
        Auto = true;

    }

    public void LocateTarget()
    {
        if (TargetEnemy != null)
        {
            ForEasyLocation.transform.position = TargetEnemy.position;
        }
    }
    public void Attack()
    {
        if (Lockon && TargetEnemy != null)
        {

            StartCoroutine(FFCombat(TargetEnemy.position + (TargetEnemy.forward * 1f)));
        }
        else
        {
            WeaponAttack();
        }
    }
    public Vector3 GetPlayerSize()
    {
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            Vector3 size = collider.bounds.size;
            return size;

        }
        return Vector3.zero;
    }
    private IEnumerator FFCombat(Vector3 targetPos)
    {
        float elapsed = 0;
        Vector3 startPos = transform.position;
        //Vector3 targetPos = TargetEnemy.position - (TargetEnemy.forward * 1.5f);
        //Vector3 EndPos = new Vector3(targetPos.x, _MousePos.y + (GetPlayerSize().y * 0.5f), targetPos.z);
        Vector3 EndPos = new Vector3(targetPos.x, transform.position.y, targetPos.z);
        while (elapsed < 0.2f)
        {
            transform.position = Vector3.Lerp(startPos, EndPos, elapsed / 0.15f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        //transform.position = targetPos;
        //StartCoroutine(slowmo());
        //ToggleSlowmo();
        WeaponAttack();
        //HitCooldown = 1.5f;
        //yield return new WaitForSeconds(0.1f);
    }

    private void changeCollor()
    {
        //if (_IsInv == true)
        //{
        //    _renderer.material.color = Color.gray;
        //}
        //else if (_IsParry)
        //{
        //    _renderer.material.color = Color.blue;
        //}
        //else if (_IsBlock)
        //{
        //    _renderer.material.color = Color.black;
        //}
        //else
        //{
        //    _renderer.material.color = Color.yellow;
        //}
    }
    public bool GetParry()
    {
        return _IsParry;
    }

    public bool GetBlock()
    {
        return _IsBlock;
    }
    //void SelectWeapon(int idx)
    //{
    //    if (idx >= 0 && idx < weapons.Count())
    //        currentIndex = idx;
    //}

    private void FixedUpdate()
    {
        Move();

    }
    void GatherInput()
    {
        _Input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
    }

    void look()
    {
        if (_Input != Vector3.zero || _MousePos != Vector3.zero)
        {
            Vector3 flatMousePos = new Vector3(_MousePos.x, _body.position.y, _MousePos.z);

            Vector3 direction = (flatMousePos - _body.position).normalized;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                _body.rotation = Quaternion.RotateTowards(_body.rotation, targetRotation, _turnspeed * Time.deltaTime);
            }
        }
    }


    void MousePosition()
    {
        Vector3 mousePos = Input.mousePosition;
        Ray ray = _camera.ScreenPointToRay(mousePos);
        RaycastHit[] hits = Physics.RaycastAll(ray);
        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.gameObject.tag == "Ground")
            {
                _MousePos = hit.point;
                //ball.transform.position = _MousePos;
                return;
            }
        }
    }

    public IEnumerator ActiveHitbox()
    {
        _hitBox.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        _hitBox.gameObject.SetActive(false);
    }
    public void Interact()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            //Debug.Log("Interact");
        }
    }

    public Vector3 GetDirection()
    {
        return (_MousePos - transform.position).normalized;
    }


    public void DrawRay(Vector3 origin, Vector3 direction, float length)
    {
        Debug.DrawRay(origin, direction.normalized * length, Color.red);
    }
    private void Move()
    {

        _rb.velocity = new Vector3(_rb.velocity.x, _rb.velocity.y, _rb.velocity.z);

        Vector3 force = _Input.ToIso().normalized * _speed;

        _rb.AddForce(force, ForceMode.Impulse);

        if (_rb.velocity.magnitude > _speed)
        {
            _rb.velocity = _rb.velocity.normalized * _speed;
        }

    }

    public void Dash()
    {
        if (Input.GetKeyDown(KeyCode.Space) && _rb.velocity != new Vector3(0, 0, 0))
        {
            meshTrail.HandleTrailActivation();
            _IsInv = true;
            StartCoroutine(Dashing());

        }
    }

    public void Blocking()
    {
        stopBlock();
        if (Input.GetKeyDown(KeyCode.F))
        {
            BlockHoldTime = Time.time;
            //Debug.Log("F");
        }

        if (Input.GetKey(KeyCode.F))
        {
            if (!_IsBlock && Time.time - BlockHoldTime > parryThreshold)
            {
                Block();
            }

        }

        if (Input.GetKeyUp(KeyCode.F) && _ParryCooldown <= 0)
        {
            float heldTime = Time.time - BlockHoldTime;
            if (heldTime <= parryThreshold)
            {
                Parry();
            }
            else
            {
                stopBlock();
            }
        }

    }

    public void Block()
    {
        _IsBlock = true;
        //Debug.Log("Block");
    }

    public void stopBlock()
    {
        _IsBlock = false;
        //Debug.Log("UnBlock()");
    }

    public void Parry()
    {
        if (_IsParry) return;
        _IsParry = true;
        _parryzone.SetActive(true);
        //Debug.Log("Parry");
        StartCoroutine(ParryWindow());
    }

    public void resetParryCooldown()
    {
        _ParryCooldown = 0;
    }
    IEnumerator ParryWindow()
    {
        _ParryCooldown = _ParryDuationLast;
        yield return new WaitForSeconds(0.5f);
        _IsParry = false;
        _parryzone.SetActive(false);
    }

    IEnumerator Dashing()
    {
        _speed = _CurrentDashSpeed + _normalspeed + SpeedBuff;
        yield return new WaitForSeconds(0.1f);
        _speed = _normalspeed + SpeedBuff;
        _IsInv = false;
    }

    public void OnDrawGizmos()
    {
        Vector3 mousePos = Input.mousePosition;
        Ray ray = _camera.ScreenPointToRay(mousePos);

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(ray.origin, ray.direction * 100f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(ray.origin, 10000f);
    }


    //public void ToggleSlowmo()
    //{
    //    HitStop hitstopFX = GetComponent<HitStop>();
    //    hitstopFX.TriggerHitStop();
    //}

    public IEnumerator Slowmo()
    {
        Time.timeScale = 0.05f;
        Time.fixedDeltaTime = 0.02f * 0.05f;
        yield return new WaitForSeconds(0.02f);
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f * 1f;
    }

    public void TakeDamage(float damage)
    {
        if (_IsInv == false)
        {
            Health = Health - damage;
            Debug.Log("ouch");
        }
    }

    public void Die()
    {
        Debug.Log("die");
    }
}
