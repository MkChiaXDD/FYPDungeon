
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

    public virtual void Attack(SpellCast spellCastList , bool mimic)
    {
        return;
    }

    protected virtual void Awake()
    {
        damage = data.damage;
        hitLayers = data.hitLayers;
    }

}
