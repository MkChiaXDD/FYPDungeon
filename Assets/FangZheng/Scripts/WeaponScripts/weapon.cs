using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class weapon : MonoBehaviour
{
    [SerializeField] WeaponDatas weaponData;
    [SerializeField] private int CurrDurability;
    [SerializeField] private SpellCast spellCastList;
    //[SerializeField] private List<SpellCast> spellCastList;

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
       
        Destroy(this.gameObject);
    }
}
