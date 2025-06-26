using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons")]
public class WeaponDatas : ItemData
{
    //string WeaponName;
    //string Description;
    //Sprite icon;

    //public GameObject WeaponPrefab;
    public SpellCast spells;
    //public List<SpellCast> spells;


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
    public List<Element> ApplyElement;
    public CollisionType collisionType;
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


    //public Animation Animation;

    public void Initialize(Transform Object)
    {
        if (SpellPrefab != null)
        {
            GameObject instance = GameObject.Instantiate(SpellPrefab, Object);
            spell = instance.GetComponent<Spell>();
        }
        else
        {
            Debug.Log("You forgot SpellPrefab");
        }
    }
}

[System.Serializable]
public class Element
{
    public string elementName;
    public float potency;
}