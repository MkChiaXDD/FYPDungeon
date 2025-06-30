using System.Collections;
using System.Collections.Generic;
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
        GameObject SpawnedOrb = Instantiate(Orb, transform.position, transform.rotation);
        
        SpawnedOrb.GetComponent<Orbiting>().Init(spellCastList);
        SpawnedOrb.GetComponent<Orbiting>().Intitialize(PlayerMovement.Instance.GetThisTransform() , Amount, OrbAttack);

    }

}
