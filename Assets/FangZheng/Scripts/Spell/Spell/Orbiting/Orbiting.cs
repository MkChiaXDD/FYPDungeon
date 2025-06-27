using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbiting : Projectile
{
    [SerializeField] private Transform Player;
    [SerializeField] private SpellCast SpellStuff;
    [SerializeField] private int Weapon_Count;
    [SerializeField] private float WeaponOffset;
    [SerializeField] private GameObject WeaponPrefab;
    //[SerializeField] SpellHitbox Hitbox;

    private List<GameObject> weaponList = new List<GameObject>();
    private float currentAngle;
    public void Intitialize(Transform PLayerPos, int WeaponCount, GameObject WeaponPrefab)
    {
        Player = PLayerPos;
        Weapon_Count = WeaponCount;
        this.WeaponPrefab = WeaponPrefab;
        Destroy(this.gameObject , duration);
        Create();
    }

    //private void Start()
    //{
    //    Create();
    //}

    private void Create()
    {
        float anglePerWeapon = 360 / (int)Weapon_Count;

        this.transform.SetParent(Player);
        for (int i = 0; i < Weapon_Count; i++)
        {
            GameObject Weapon = Instantiate( WeaponPrefab, transform.position, Quaternion.identity);
            Weapon.transform.SetParent(this.transform);
            weaponList.Add(Weapon);

            float angle = i * anglePerWeapon;
            Vector3 OrbPos = CalculateOrbitPosition(angle);
            Weapon.transform.position = OrbPos;
            SpellHitbox Hitbox = Weapon.AddComponent<SpellHitbox>();
            Hitbox.Initit(spellCast);
        }
    }

    private Vector3 CalculateOrbitPosition(float angle)
    {
        Vector3 orbitPosition = transform.position;
        orbitPosition.x += Mathf.Cos(angle * Mathf.Deg2Rad) * Radius;
        orbitPosition.z += Mathf.Sin(angle * Mathf.Deg2Rad) * Radius;
        //orbitPosition.y += heightOffset;
        return orbitPosition;
    }

    private void Update()
    {
        MoveOrb();
    }

    private void MoveOrb()
    {
        currentAngle += Speed * Time.deltaTime;
        if (currentAngle > 360f)
        {
            currentAngle -= 360f;
        }

        float angleStep = 360f / weaponList.Count;

        for (int i = 0; i < weaponList.Count; i++)
        {
            float angle = currentAngle + (i * angleStep);
            weaponList[i].transform.position = CalculateOrbitPosition(angle);
        }
    }

}
