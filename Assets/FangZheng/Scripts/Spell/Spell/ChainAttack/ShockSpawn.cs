
using UnityEngine;

public class ShockSpawn : Spell
{
    [SerializeField] private GameObject Lightning;
    
    public override void Attack(SpellCast spellCastList)
    {
        SummonShock(spellCastList);
    }

    public void SummonShock(SpellCast spellCastList)
    {

        GameObject SpawnedShock = Instantiate(Lightning, transform.position, transform.rotation);
        SpawnedShock.GetComponent<Shock>().Init(spellCastList);

        //SpawnedWave.GetComponent<Bomerang>().Modify();

    }
}
