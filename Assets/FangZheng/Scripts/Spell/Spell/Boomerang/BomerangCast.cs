
using UnityEngine;

public class BomerangCast : Spell
{
    [SerializeField] private GameObject Boom;
    public override void Attack(SpellCast spellCastList)
    {
        SummonWave(spellCastList);
    }

    public override void Attack(SpellCast spellCastList, bool IsMimic)
    {
        SummonWave(spellCastList);
    }

    public void SummonWave(SpellCast spellCastList)
    {
        GameObject SpawnedWave = Instantiate(Boom, transform.position, Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0));
        SpawnedWave.GetComponent<Bomerang>().Init(spellCastList);
        //SpawnedWave.GetComponent<Bomerang>().Modify();
    }
}
