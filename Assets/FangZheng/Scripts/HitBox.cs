using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    public int dmg = 10;
    public int knockbackForce = 10;
    void OnTriggerEnter(Collider hit)
    {
        if (hit.GetComponent<PlayerData>() == null) {
            if (hit.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(dmg);
                Rigidbody enemyRb = hit.GetComponent<Rigidbody>();
                if (enemyRb != null)
                {
                    // Calculate knockback direction
                    Vector3 knockbackDirection = hit.transform.position - transform.position;
                    knockbackDirection.y = hit.transform.position.y; // Keep the knockback horizontal

                    // Apply force to the enemy
                    //enemyRb.isKinematic = false;
                    enemyRb.AddForce(knockbackDirection.normalized * knockbackForce, ForceMode.Impulse);
                    //enemyRb.isKinematic = true;
                }
            }
        }

    }
}
