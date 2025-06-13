using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [SerializeField] protected WeaponData data;
    protected int damage;
    protected LayerMask hitLayers;

    protected virtual void Awake()
    {
        damage = data.damage;
        hitLayers = data.hitLayers;
    }

    /// <summary>
    /// Fire this weapon’s effect. Subclasses implement their own behavior.
    /// </summary>
    public abstract void Attack();
}
