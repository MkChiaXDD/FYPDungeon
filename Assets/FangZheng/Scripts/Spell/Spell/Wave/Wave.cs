using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Wave : Projectile
{
    [SerializeField] private float Cooldown;
    [SerializeField] private Vector3 direction;
    [SerializeField] private float TimeLast = 0.0f;
    [SerializeField] private bool ColliderActive = true;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Dictionary<IDamageable, float> EnemyHitAlready;
    [SerializeField] private float knockbackForce = 5f;
    public void Modify()
    {

        direction = new Vector3 (PlayerController.Instance.GetDirection().x , 0 , PlayerController.Instance.GetDirection().z);
        //Destroy(this.gameObject, duration);
    }

    private void Update()
    {
        transform.position += direction * Speed * Time.deltaTime;
        TimeLast += Time.deltaTime;
        CheckDmg();

    }

    private void CheckDmg()
    {
        Collider[] hits = Physics.OverlapSphere(this.transform.position,
                                        Radius,
                                        enemyLayer);

        if (CollisionType == SpellCast.CollisionType.Continues)
        {
            ListChange();

            foreach (Collider hit in hits)
            {
                if (hit.TryGetComponent(out IDamageable damageable))
                {

                    if (EnemyHitAlready.ContainsKey(damageable))
                    {
                        continue;
                    }


                    damageable.TakeDamage(damage);
                    EnemyHitAlready.Add(damageable, Time.time);
                    ApplyKnockBack(hit);
                    //ColliderActive = false;

                }
            }
        }
        else
        {
            foreach (Collider hit in hits)
            {
                if (hit.TryGetComponent(out IDamageable damageable))
                {

                    damageable.TakeDamage(damage);
                    ApplyKnockBack(hit);
                }
            }
        }

    }

    public void ListChange()
    {
        foreach (KeyValuePair<IDamageable, float> entry in EnemyHitAlready)
        {
            if (entry.Value + AtkPerSec <= Time.time)
            {
                EnemyHitAlready.Remove(entry.Key);
            }
        }
    }

    public void ApplyKnockBack(Collider hit)
    {
        Rigidbody enemyRb = hit.GetComponent<Rigidbody>();
        if (enemyRb != null)
        {
            // Calculate knockback direction
            Vector3 knockbackDirection = hit.transform.position - transform.position;
            knockbackDirection.y = hit.transform.position.y; // Keep the knockback horizontal

            // Apply force to the enemy
            enemyRb.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode.Impulse);
        }
    }
}
