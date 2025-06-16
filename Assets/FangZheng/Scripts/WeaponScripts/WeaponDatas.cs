using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Weapons")]
public class WeaponDatas : ScriptableObject
{
    string WeaponName;
    string Description;
    int dmg;
    int Duration;
    GameObject WeaponPrefab;
    Sprite icon;
    enum WeaponType
    {
        Spinning_Sword,
        Sword_Wave
    }
    ;
}
