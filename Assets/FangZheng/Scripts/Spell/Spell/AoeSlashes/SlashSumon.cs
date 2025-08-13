using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlashSumon : Spell
{
    [SerializeField] private GameObject Slash;
    public override void Attack(SpellCast spellCastList)
    {
        SummonUlt(spellCastList);
    }

    public override void Attack(SpellCast spellCastList, bool IsMimic)
    {
        SummonUlt(spellCastList);
    }

    public void SummonUlt(SpellCast spellCastList)
    {
        GameObject SpawnedUlt = Instantiate(Slash, transform.position, Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0));
        SpawnedUlt.GetComponent<SlashUlt>().Init(spellCastList);
        SpawnedUlt.GetComponent<SlashUlt>().Modify();
    }
}
