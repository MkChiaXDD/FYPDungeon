
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class Shock : Projectile
{
    //[SerializeField] SpellHitbox Hitbox;

    private List<GameObject> hitEnemies = new List<GameObject>();
    private Transform LastEnemy;
    [SerializeField] private Vector3 StartPos;
    [SerializeField] private Vector3 Direction;
    [SerializeField] private bool Hit = false;
    public ElementType ElementType = ElementType.Electro;
    //private int bounceCount = 0;

    public void Start()
    {
        Direction = PlayerMovement.Instance.GetDirection();
        Destroy(this.gameObject, duration);

    }


    public void Update()
    {
        if (Vector3.Distance(StartPos, transform.position) < Range )
        {
            if (Hit == false)
            {
                transform.position += Direction * Speed * Time.deltaTime;
                
            }
        }
    }
    protected void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Enemy>() != null)
        {
            if (other.TryGetComponent(out IDamageable damageable))
            {
                
                damageable.TakeElementalDamage(damage , ElementType);
                
                Hit = true;
                this.GetComponent<BoxCollider>().enabled = false;
                //this.GetComponent<SphereCollider>().enabled = false;
                this.GetComponent<MeshRenderer>().enabled = false;
                StartCoroutine(Chain(other.transform));
            }
        }
    }

    private IEnumerator Chain(Transform pos)
    {
        Collider[] enemies = Physics.OverlapSphere(pos.position, Radius);
        List<GameObject> lists = new List<GameObject>();
        foreach (Collider c in enemies)
        {
            if (c.transform.GetComponent<Enemy>() != null)
            {
                if (!hitEnemies.Contains(c.gameObject))
                {
                    hitEnemies.Add(c.gameObject);
                    if (c.TryGetComponent(out IDamageable damageable))
                    {
                        //damageable.TakeDamage(damage / 2);
                        damageable.TakeElementalDamage(damage / 2, ElementType);
                        Debug.Log(c.name + "Hit by chain");
                        LastEnemy = c.transform;
                    }
                }
                else
                {
                    lists.Add(c.gameObject);
                }
                yield return new WaitForSeconds(0.2f);
            }


        }

        foreach (GameObject list in lists)
        {
            if (hitEnemies.Contains(list))
            {
                hitEnemies.Remove(list);
            }
        }

       // yield return new WaitForSeconds(AtkPerSec);
        Chain(LastEnemy);


    }
}
