using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellHitbox : MonoBehaviour
{
    [SerializeField] private Dictionary<GameObject, float> EnemyHitAlready = new Dictionary<GameObject, float>();
    [SerializeField] private List<GameObject> ListOfEnemy;
    [SerializeField] private SpellCast Attack;

    public void Initit(SpellCast attack)
    {
        Attack = attack;
    }
    // Update is called once per frame
    void Update()
    {

        if (Attack != null) {
            if (Attack.collisionType == SpellCast.CollisionType.Continues)
            {
                ResetEnemyHitTine();
            }
        }
    }

    public void ResetEnemyHitTine()
    {
        List<GameObject> StuffToClear = new List<GameObject>();
        foreach (KeyValuePair<GameObject, float> entry in EnemyHitAlready)
        {
            if (entry.Value + Attack.AtkPerSec < Time.time)
            {
                StuffToClear.Add(entry.Key);
            }
        }

        foreach (GameObject key in StuffToClear)
        {
            EnemyHitAlready.Remove(key);
        }


    }
    
    void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<Enemy>() != null)
        {
            GameObject enemy = other.gameObject;
            if (!EnemyHitAlready.ContainsKey(other.gameObject))
            {
                if (other.TryGetComponent(out IDamageable damageable))
                {
                    EnemyHitAlready.Add(enemy, Time.time);
                    damageable.TakeDamage(Attack.dmg);
                    ApplyKnockBack(other);
                }
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
            enemyRb.AddForce(knockbackDirection.normalized * 1, ForceMode.Impulse);
        }
    }
}
