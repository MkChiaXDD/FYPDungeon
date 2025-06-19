using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons")]
public class WeaponDatas : ItemSOData
{
    //string WeaponName;
    //string Description;
    //Sprite icon;
    public int MaxDurability;
    public GameObject WeaponPrefab;
    public List<SpellCast> spells;

    
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
    enum CollisionType
    {
        OneTime,
        Continues
    }
    

    public Animation Animation;

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