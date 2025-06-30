using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class Wave : Projectile
{
    [SerializeField] private float Cooldown;
    [SerializeField] private Vector3 direction;
    [SerializeField] private float TimeLast = 0.0f;
    //[SerializeField] private bool ColliderActive = true;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Dictionary<IDamageable, float> EnemyHitAlready = new Dictionary<IDamageable, float>();
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private Vector3 StartPos;
    [SerializeField] List<GameObject> ListOfColider;
    [SerializeField] SpellHitbox Hitbox;
    //[SerializeField] PlayerMovement PlayerMovement;
    private void Start()
    {
        if (Hitbox == null)
        {
            Hitbox = this.GetComponent<SpellHitbox>();
        }
        Hitbox.Initit(spellCast);
    }
    public void Modify()
    {

        direction = new Vector3 (PlayerMovement.Instance.GetDirection().x , 0 , PlayerMovement.Instance.GetDirection().z);
        StartPos = this.transform.position;
        Destroy(this.gameObject, duration);
    }

    private void Update()
    {
        if (Vector3.Distance(StartPos , transform.position) < Range) {
            transform.position += direction * Speed * Time.deltaTime;
        }
        //TimeLast += Time.deltaTime;
        //CheckDmg();
        //ConsolelogginList();
    }

    //private void CheckDmg()
    //{
    //    Collider[] hits = Physics.OverlapSphere(this.transform.position, Radius, enemyLayer);

    //    if (CollisionType == SpellCast.CollisionType.Continues)
    //    {
    //        ListChange();
    //        //CheckDictionary(EnemyHitAlready);
    //        foreach (Collider hit in hits)
    //        {
    //            if (hit.TryGetComponent(out IDamageable damageable))
    //            {

    //                if (EnemyHitAlready.ContainsKey(damageable))
    //                {
    //                    continue;
    //                }


    //                damageable.TakeDamage(damage);
    //                Debug.Log("hit");
    //                EnemyHitAlready.Add(damageable, Time.time);
    //                ApplyKnockBack(hit);
    //                //ColliderActive = false;

    //            }
    //        }
    //    }
    //    else
    //    {
    //        foreach (Collider hit in hits)
    //        {
    //            if (hit.TryGetComponent(out IDamageable damageable))
    //            {

    //                damageable.TakeDamage(damage);
    //                ApplyKnockBack(hit);
    //            }
    //        }
    //    }

    //}

    ////void CheckDictionary(Dictionary<IDamageable, float> objectDictionary)
    ////{
    ////    if (objectDictionary.Values.Any(value => value == null))
    ////    {
    ////        objectDictionary.Clear();
    ////        //Debug.Log("Dictionary cleared due to null reference");
    ////    }
    ////}

    //private void ConsolelogginList()
    //{
    //    if (Input.GetKeyDown(KeyCode.B))
    //    {
    //        int i = 1;
    //        foreach (KeyValuePair<IDamageable, float> entry in EnemyHitAlready)
    //        {
    //            Debug.Log(i + "." + entry.Value);
    //            i++;
    //        }
    //    }
    //}

    //public void ListChange()
    //{
    //    List<IDamageable> keysToRemove = new List<IDamageable>();

    //    foreach (KeyValuePair<IDamageable, float> entry in EnemyHitAlready)
    //    {
    //        if (entry.Key != null )
    //        {
    //            if (entry.Value + AtkPerSec <= Time.time)
    //            {
    //                keysToRemove.Add(entry.Key);
    //            }
    //        }
    //    }

    //    foreach (var key in keysToRemove)
    //    {
    //        EnemyHitAlready.Remove(key);
    //    }
    //}

    //public void ApplyKnockBack(Collider hit)
    //{
    //    Rigidbody enemyRb = hit.GetComponent<Rigidbody>();
    //    if (enemyRb != null)
    //    {
    //        // Calculate knockback direction
    //        Vector3 knockbackDirection = hit.transform.position - transform.position;
    //        knockbackDirection.y = hit.transform.position.y; // Keep the knockback horizontal

    //        // Apply force to the enemy
    //        enemyRb.AddForce(knockbackDirection.normalized * 1, ForceMode.Impulse);
    //    }
    //}

    //void OnTriggerStay(Collider other)
    //{
    //    if (other.GetComponent<Enemy>() != null) {
    //        if (!ListOfColider.Contains(other.gameObject))
    //        {
    //            ListOfColider.Add(other.gameObject);
    //        }
    //    }
    //}
}
