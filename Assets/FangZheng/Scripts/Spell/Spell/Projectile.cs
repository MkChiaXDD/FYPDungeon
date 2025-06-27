using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SpellCast;

public class Projectile : MonoBehaviour
{
    [SerializeField] protected float Radius;
    [SerializeField] protected Vector3 Size;
    [SerializeField] protected float Range;
    [SerializeField] protected float duration;
    [SerializeField] protected float AtkPerSec;
    [SerializeField] protected float Speed;
    [SerializeField] protected int damage;
    [SerializeField] protected SpellCast.CollisionType CollisionType;
    [SerializeField] protected SpellCast spellCast;
    //[SerializeField] protected GameObject Summon;
    //public virtual void Init(float Radius, Vector3 Size, float Range, float duration, float AtkPerSec, float Speed, int dmg , SpellCast.CollisionType Type) 
    //{
    //    this.Radius = Radius;
    //    this.Size = Size;
    //    this.Range = Range;
    //    this.duration = duration;
    //    this.AtkPerSec = AtkPerSec;
    //    this.Speed = Speed;
    //    damage = dmg;
    //    CollisionType = Type;

    //}

    public virtual void Init(SpellCast spellCastList)
    {
        this.Radius = spellCastList.Radius;
        this.Size = spellCastList.Size;
        this.Range = spellCastList.Range;
        this.duration = spellCastList.duration;
        this.AtkPerSec = spellCastList.AtkPerSec;
        this.Speed = spellCastList.Speed;
        damage = spellCastList.dmg;
        CollisionType = spellCastList.collisionType;
        Transform myTransform = gameObject.transform;
        myTransform.localScale = spellCastList.Size;
        spellCast = spellCastList;

        Debug.Log("Inti");

    }
}
