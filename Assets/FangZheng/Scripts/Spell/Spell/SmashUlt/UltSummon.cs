using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UltSummon : Spell
{
    [SerializeField] private GameObject UltAttack;
    [SerializeField] private GameObject GameObject;
    public override void Attack(SpellCast spellCastList)
    {
        SummonSmashUlt(spellCastList);
    }

    public override void Attack(SpellCast spellCastList , bool IsMimic)
    {
        MimicSummonSmashult(spellCastList);
    }

    public void SummonSmashUlt(SpellCast spellCastList)
    {
        GameObject SpawnedWave = Instantiate(UltAttack, transform.position, Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0));
        SpawnedWave.GetComponent<Smash>().Init(spellCastList);
        SpawnedWave.GetComponent<Smash>().activ(FindFirstObjectByType<PlayerMovement>().gameObject);
    }

    public void MimicSummonSmashult(SpellCast spellCastList)
    {
        GameObject SpawnedWave = Instantiate(UltAttack, transform.position, Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0));
        SpawnedWave.GetComponent<Smash>().Init(spellCastList);
        SpawnedWave.GetComponent<Smash>().activ(null);

    }
}
