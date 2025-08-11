using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UltSummon : Spell
{
    [SerializeField] private GameObject UltAttack;
    public override void Attack(SpellCast spellCastList)
    {
        SummonSmashUlt(spellCastList);
    }

    public void SummonSmashUlt(SpellCast spellCastList)
    {
        GameObject SpawnedWave = Instantiate(UltAttack, transform.position, Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0));
        SpawnedWave.GetComponent<Smash>().Init(spellCastList);
        //SpawnedWave.GetComponent<Smash>().Modify();
    }
}
