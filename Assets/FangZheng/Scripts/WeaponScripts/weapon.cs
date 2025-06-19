using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class weapon : MonoBehaviour
{
    [SerializeField] WeaponDatas weaponData;
    [SerializeField] private int CurrDurability;
    [SerializeField] private List<SpellCast> spellCastList;
    protected void Start()
    {

        CurrDurability = weaponData.MaxDurability;
        spellCastList = weaponData.spells;
        spellCastList.Reverse();

        for (int i = 0; i < spellCastList.Count; i++)
        {
            spellCastList[i].Initialize(this.transform);
        }
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
        Cast();
        //ReduceDua( 1);
        

    }

    public void Cast()
    {
        spellCastList[CurrDurability - 1].spell?.Attack();
    }

    public void ReduceDua(int amount)
    {
        CurrDurability -= amount;
        if (CurrDurability <= 0)
        {
            BreakWeapon();
        }
    }

    public void BreakWeapon()
    {
        Destroy(this.gameObject, 0.2f );
    }
}
