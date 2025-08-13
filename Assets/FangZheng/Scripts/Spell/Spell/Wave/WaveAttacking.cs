
using UnityEngine;

public class WaveAttacking : Spell
{
    [SerializeField] private GameObject Wave;
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
        GameObject SpawnedWave =  Instantiate(Wave, transform.position, Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0));
        SpawnedWave.GetComponent<Wave>().Init(spellCastList);
        SpawnedWave.GetComponent<Wave>().Modify();
    }

}
