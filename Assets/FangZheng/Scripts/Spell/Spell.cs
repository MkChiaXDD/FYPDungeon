using System;
using UnityEngine;

public abstract class Spell : MonoBehaviour 
{
    [SerializeField] protected SpellData data;
    protected int damage;
    protected LayerMask hitLayers;
    public virtual void Attack(SpellCast spellCastList)
    {
        return;
    }

    protected virtual void Awake()
    {
        damage = data.damage;
        hitLayers = data.hitLayers;
    }

}
