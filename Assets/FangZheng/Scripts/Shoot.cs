using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : MonoBehaviour
{
    [SerializeField] private Transform PlayerTransform;
    [SerializeField] private GameObject BulletPrefab;

    [SerializeField] private float duration = 2;
    [SerializeField] private float cooldown = 2;

    [SerializeField]
    private enum Type
    {
        Player,
        Enemy,
        Training
    }
    ;

    [SerializeField] private Type type;
    void Update()
    {
        cooldown += Time.deltaTime;

        if (type == Type.Player)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                shoot();
            }
        }
        else
        {

                shoot();
            
        }
    }

    private void shoot()
    {
        if (cooldown > duration)
        {
            GameObject bullet =  GameObject.Instantiate(BulletPrefab , this.transform);
            Vector3 dir = PlayerTransform.position - transform.position;

            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.Initialize(dir, 10);
            }

            cooldown = 0f;
        }
    }

    private void Start()
    {
        
    }
}
