using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockSpawn : Spell
{
    [SerializeField] private GameObject Lightning;
    
    public override void Attack(SpellCast spellCastList)
    {
        SummonWave(spellCastList);
    }

    public void SummonWave(SpellCast spellCastList)
    {
        GameObject SpawnedWave = Instantiate(Lightning, transform.position, transform.rotation);
        //SpawnedWave.GetComponent<Bomerang>().Init(spellCastList);
        //SpawnedWave.GetComponent<Bomerang>().Modify();
    }
}
