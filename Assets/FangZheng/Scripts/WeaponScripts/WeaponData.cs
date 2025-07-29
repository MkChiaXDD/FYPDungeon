
using System.Collections.Generic;

using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons")]


public class WeaponData : ItemData
{
    public WeaponType weaponType;
    public SpellCast spells;
    public BaseAttackScript baseAttackScript;

    public float _lightAttackCooldown = 0.5f;
    public float _heavyAttackCooldown = 1.5f;
    public float _minChargeTime = 0.1f;
    public float _maxChargeTime = 2f;
}

[System.Serializable]
public class SpellCast
{
    public GameObject SpellPrefab;
    public int dmg;
    public float Radius;
    public Vector3 Size;
    public float Range;
    public Spell spell;
    public float duration;
    public float AtkPerSec;
    public float Speed;
    public CollisionType collisionType;
    public LayerMask enemyLayer;
    public enum CollisionType
    {
        OneTime,
        Continues
    }

    public SpellType spellType;
    public enum SpellType
    {
        Range,
        Aoe,
        Cast
    }

    public void Initialize(Transform Object)
    {
        if (SpellPrefab != null)
        {

            GameObject instance = GameObject.Instantiate(SpellPrefab, Object.transform.position, GameObject.FindGameObjectWithTag("PlayerBody").transform.rotation,Object);
            spell = instance.GetComponent<Spell>();
        }
        else
        {
            Debug.Log("You forgot SpellPrefab");
        }
    }
}

