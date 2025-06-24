using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Weapon : MonoBehaviour
{
    [SerializeField] public WeaponDatas weaponData;
    [SerializeField] public int CurrDurability;
    [SerializeField] public SpellCast spellCastList;
    //[SerializeField] private List<SpellCast> spellCastList;
    public bool broke;
    public  UnityEvent WeaponBreak;
    protected void Start()
    {

        CurrDurability = weaponData.MaxDurability;
        spellCastList = weaponData.spells;
        spellCastList.Initialize(this.transform); 
        //spellCastList.Reverse();

        //for (int i = 0; i < spellCastList.Count; i++)
        //{
        //    spellCastList[i].Initialize(this.transform);
        //}
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            Attack();
        }
    }
    public void Attack()
    {
        if (CurrDurability <= 0 )
        {
            return;
        }
        
        ReduceDua( 1);
        

    }

    public void Cast()
    {
        spellCastList.spell?.Attack(spellCastList);
    }

    public void ReduceDua(int amount)
    {
        CurrDurability -= amount;
        if (CurrDurability <= 0)
        {
            Cast();
            BreakWeapon();
        }
    }

    public void BreakWeapon()
    {
        Debug.Log("gae");
        broke = true;
        //WeaponBreak.Invoke();
        //Destroy(this.gameObject);
        
    }
}
