
using UnityEngine;

public class OrbitingCast : Spell
{

    [SerializeField] private GameObject Orb;
    [SerializeField] private GameObject OrbAttack;
    [SerializeField] private GameObject Player;
    [SerializeField] private int Amount;
    //[SerializeField] private PlayerMovement playerMovement;

    public override void Attack(SpellCast spellCastList)
    {
        SummonOrbs(spellCastList);
    }


    public void SummonOrbs(SpellCast spellCastList)
    {
        GameObject SpawnedOrb = Instantiate(Orb, transform.position, Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0));
        
        SpawnedOrb.GetComponent<Orbiting>().Init(spellCastList);
        SpawnedOrb.GetComponent<Orbiting>().Intitialize(PlayerMovement.Instance.GetTransform() , Amount, OrbAttack);

    }

}
