
using UnityEngine;

public class BoomCast : Spell
{
    [SerializeField] private GameObject Explosive;
    public override void Attack(SpellCast spellCastList)
    {
        SummonEx(spellCastList);
    }

    public void SummonEx(SpellCast spellCastList)
    {
        GameObject SpawnedEx = Instantiate(Explosive, transform.position, Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0));
        SpawnedEx.GetComponent<Boom>().Init(spellCastList);
        SpawnedEx.GetComponent<Rigidbody>().AddForce(PlayerMovement.Instance.GetDirection() * 20, ForceMode.Impulse);
        //SpawnedWave.GetComponent<Bomerang>().Modify();
    }

}
