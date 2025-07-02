
using UnityEngine;

public class BomerangCast : Spell
{
    [SerializeField] private GameObject Boom;
    public override void Attack(SpellCast spellCastList)
    {
        SummonWave(spellCastList);
    }

    public void SummonWave(SpellCast spellCastList)
    {
        GameObject SpawnedWave = Instantiate(Boom, transform.position, transform.rotation);
        SpawnedWave.GetComponent<Bomerang>().Init(spellCastList);
        //SpawnedWave.GetComponent<Bomerang>().Modify();
    }
}
