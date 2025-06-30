using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    #region State
    [Space, Header("PlayerData")]
    [SerializeField] private PlayerData playerData;

    [Space, Header("Layers & Masks")]
    [SerializeField] private LayerMask _LayerMaskIgnore;
    [SerializeField] private LayerMask[] _LayerMaskHit;

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
    [SerializeField] private PlayerMovement playmove;
    [SerializeField] private GameObject _Area;
    [SerializeField] private GameObject ForEasyLocation;

    [Space, Header("Blocking & Parry")]
    [SerializeField] private float _parryThreshold = 0.5f;
    [SerializeField] private float _parryDuration = 4f;
    [SerializeField] private float _parryCooldown;
    [SerializeField] private GameObject _parryzone;
    [SerializeField] private GameObject _hitBox;

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

    [SerializeField, Range(0, 100)] private int ThreshholdPercentage;
    [SerializeField] private int Dmg;
    [SerializeField] private int OriginalDmg = 5;

    [SerializeField] private List<GameObject> WeaponIndicator;

    [Space, Header("Inventory")]
    [SerializeField] private InventoryManager PlayerStorage;
    [SerializeField] private ItemInstance ItemHeld;


    private float BlockHoldTime;
    private bool _IsParry;
    private bool _IsBlock;
    private bool _IsInv;

    #endregion

    public void Start()
    {
        playmove = this.GetComponent<PlayerMovement>();
    }
    public void OnEnable()
    {
        GetComponent<Inventory>().ChangeSlot.AddListener(GetHoldItem);
        PlayerStorage.ModifySlot.AddListener(GetHoldItem);
        playerData.DataChange.AddListener(Addmodifier);
    }

    private void Update()
    {


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
        Check();

        SpecialAttack();
        Attack();
        

        _parryCooldown -= Time.deltaTime;
    }

    public void Addmodifier()
    {
        Dmg = playerData.Damage;
        _parryDuration = playerData.ParryTime;
    }

    #region Handle Blocking
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
            if (!_IsBlock && Time.time - BlockHoldTime > _parryThreshold)
            {
                Block();
            }

        }

        if (Input.GetKeyUp(KeyCode.F) && _parryCooldown <= 0)
        {
            float heldTime = Time.time - BlockHoldTime;
            if (heldTime <= _parryThreshold)
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
        _parryCooldown = 0;
    }

    IEnumerator ParryWindow()
    {
        _parryCooldown = _parryDuration;
        yield return new WaitForSeconds(0.5f);
        _IsParry = false;
        _parryzone.SetActive(false);
    }
    #endregion

    #region Handle Combat

    public void GetHoldItem()
    {
        CheckedItemHold();
        ItemHeld = PlayerStorage.GetCurrentHotbarItem();
        EquipItem(ItemHeld);
        AddAttackIndicator();
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

    public void Attack()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (WeaponChoosen != null)
            {
                PlayerStorage.GetInventory().BreakItem(GetComponent<Inventory>().equippedSlotNum, WeaponChoosen.baseDurabilityUsed);
                //Debug.Log("Durability Check for basic attack, " + ItemHeld.name + " now at " + ItemHeld.Durability);

            }
            else
            {
                if (Lockon && TargetEnemy != null)
                {

                    StartCoroutine(FFCombat(TargetEnemy.position + (TargetEnemy.forward * 1f)));
                }
                else
                {
                    BasicCombat.SwordAttack();
                }
                //StartCoroutine(FFCombat(TargetEnemy.position + (TargetEnemy.forward * 1f)));
                //BasicCombat.SwordAttack();
            }
        }

    }

    private IEnumerator FFCombat(Vector3 targetPos)
    {
        float elapsed = 0;
        Vector3 startPos = transform.position;

        Vector3 EndPos = new Vector3(targetPos.x, transform.position.y, targetPos.z);
        while (elapsed < 0.2f)
        {
            transform.position = Vector3.Lerp(startPos, EndPos, elapsed / 0.15f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        BasicCombat.SwordAttack();

    }


    public void SpecialAttack()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (WeaponChoosen != null)
            {
                WeaponChoosen.Cast();
                PlayerStorage.GetInventory().BreakItem(GetComponent<Inventory>().equippedSlotNum, WeaponChoosen.skillDurabilityUsed);
                //Debug.Log("Durability Check for Special attack, " + ItemHeld.name + " at " + ItemHeld.Durability);
                return;
            }
            Debug.LogWarning("Player not holding weapon!");
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

    #endregion

    #region Targeting

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

    public void ClearTargets()
    {
        DepthOfRange = 0;
        Auto = true;

    }

    private void Check()
    {
        if (TargetEnemy == null)
        {

            ClearTargets();
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
            Vector3 forward = playmove._body.transform.forward;
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

    public void handleEnemyInView()
    {
        EnemyInView.Clear();
        float Diatance = Mathf.Infinity * Mathf.PerlinNoise1D(1);
        foreach (KeyValuePair<GameObject, float> enemy in EnemyNear)
        {
            if (enemy.Key != null)
            {
                Vector3 dir = (enemy.Key.transform.position - this.transform.position).normalized;
                Vector3 forward = playmove._body.transform.forward;
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

    public void LocateTarget()
    {
        if (TargetEnemy != null)
        {
            ForEasyLocation.transform.position = TargetEnemy.position;
        }
    }
    #endregion
}
