using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveAttacking : Spell
{
    [SerializeField] private GameObject Wave;
    public override void Attack(SpellCast spellCastList)
    {
        SummonWave(spellCastList);
    }

    public void SummonWave(SpellCast spellCastList)
    {
        GameObject SpawnedWave =  Instantiate(Wave, transform.position, transform.rotation);
        SpawnedWave.GetComponent<Wave>().Init(spellCastList);
        SpawnedWave.GetComponent<Wave>().Modify();
    }

}
