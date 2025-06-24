using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private Transform Weapon_Holder;
    [SerializeField] private GameObject CurrentHeldWeapon;
    //[SerializeField] private ItemInstance Weapon_Data;

    public void EquipWeapon(Weapon WeaponRefrence)
    {
        if (CurrentHeldWeapon != null)
        {
            Destroy(CurrentHeldWeapon);
        }

        CurrentHeldWeapon = Instantiate(WeaponRefrence.weaponData.ItemPrefab, Weapon_Holder);

        if (CurrentHeldWeapon.GetComponent<Weapon>() == null)
        {
            Weapon CurrentWeapondata = CurrentHeldWeapon.AddComponent<Weapon>();
            CurrentWeapondata = WeaponRefrence;
        }
    }

    public void UnEquipWeapon()
    {
        if (CurrentHeldWeapon != null)
        {
            Destroy(CurrentHeldWeapon);
            CurrentHeldWeapon = null;
        }
    }

}
