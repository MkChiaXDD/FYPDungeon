using UnityEngine;

public abstract class Spell : MonoBehaviour , ISpell
{
    [SerializeField] protected SpellData data;
    protected int damage;
    protected LayerMask hitLayers;

    public abstract void Attack();

    protected virtual void Awake()
    {
        damage = data.damage;
        hitLayers = data.hitLayers;
    }

}
