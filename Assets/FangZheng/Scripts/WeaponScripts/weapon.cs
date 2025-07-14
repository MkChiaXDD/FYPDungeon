
using UnityEngine;
using UnityEngine.Events;

public class Weapon : MonoBehaviour
{
    [SerializeField] public WeaponData weaponData;
    [SerializeField] public int CurrDurability;
    [SerializeField] public SpellCast spellCastList;

    [SerializeField] public int baseDurabilityCost = 1;
    [SerializeField] public int skillDurabilityCost = 3;
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
