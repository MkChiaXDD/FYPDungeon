using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons")]
public class WeaponDatas : ItemSOData
{
    //string WeaponName;
    //string Description;
    //Sprite icon;
    int dmg;
    int Duration;
    GameObject WeaponPrefab;
    enum WeaponType
    {
        Spinning_Sword,
        Sword_Wave
    }
    ;
}
