
using UnityEngine;
using UnityEngine.Events;
public enum WeaponType { Unarmed, Hammer, Sword }
public class Weapon : MonoBehaviour
{
    [SerializeField] public WeaponData weaponData;
    [SerializeField] public int CurrDurability;
    [SerializeField] public SpellCast spellCastList;

    [SerializeField] public int baseDurabilityCost = 1;
    [SerializeField] public int skillDurabilityCost = 3;

    public float _lightAttackCooldown = 0.5f;
    public float _heavyAttackCooldown = 1.5f;
    public float _minChargeTime = 0.1f;
    public float _maxChargeTime = 2f;
    public float movementModifier = 1;
    //[SerializeField] private List<SpellCast> spellCastList;
    public bool broke;
    private TutorialProggresion _Tutorial;
    public UnityEvent WeaponBreak;
    protected void Start()
    {
        _Tutorial = FindFirstObjectByType<TutorialProggresion>();
        spellCastList = weaponData.spells;
        spellCastList.Initialize(this.transform);
    }

    /// <summary>
    //Casts the attack
    /// </summary>
    public void Cast()
    {
        if (_Tutorial != null)
        {
            _Tutorial.IfPlayerPerformAction("SpecialSkill");
        }
        spellCastList.spell?.Attack(spellCastList);
        //Debug.Log("Casted " +  spellCastList.spell.name);
    }

    public void MimicCast()
    {
        spellCastList.spell?.Attack(spellCastList , true);
    }
}
