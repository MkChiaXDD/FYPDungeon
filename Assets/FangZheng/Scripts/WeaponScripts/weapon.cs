using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Weapon : MonoBehaviour
{
    [SerializeField] public WeaponDatas weaponData;
    [SerializeField] public int CurrDurability;
    [SerializeField] public SpellCast spellCastList;

    [SerializeField] public int baseDurabilityUsed = 1;
    [SerializeField] public int skillDurabilityUsed = 3;
    //[SerializeField] private List<SpellCast> spellCastList;
    public bool broke;
    public UnityEvent WeaponBreak;
    protected void Start()
    {
        spellCastList = weaponData.spells;
        spellCastList.Initialize(this.transform);
    }

    /// <summary>
    //Casts the attack
    /// </summary>
    public void Cast()
    {
        spellCastList.spell?.Attack(spellCastList);
        Debug.Log("Casted " +  spellCastList.spell.name);
    }
}
