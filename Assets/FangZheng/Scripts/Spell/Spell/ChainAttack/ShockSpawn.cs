
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

        GameObject SpawnedShock = Instantiate(Lightning, transform.position, Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0));
        SpawnedShock.GetComponent<Shock>().Init(spellCastList);

        //SpawnedWave.GetComponent<Bomerang>().Modify();

    }
}
