using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalSwordAttack : MonoBehaviour
{
    [SerializeField] private GameObject slashGO;
    [SerializeField] private ParticleSystem slashVFX;
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private int knockbackForce = 5;
    [SerializeField] private float slashRadius = 5;


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) || Input.GetMouseButtonDown(1))
        {
            SwordAttack();
        }
    }



    void SwordAttack()
    {
        slashGO.transform.localRotation = Quaternion.Euler(Random.Range(-20 * Mathf.PerlinNoise1D(1), 30 * Mathf.PerlinNoise1D(1)),transform.rotation.y,transform.rotation.z) ;
        slashVFX.Play();
        // apply damage & knockback
        Collider[] hits = Physics.OverlapSphere(transform.position, slashRadius);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out var dmg) && !hit.CompareTag("Player"))
            {
                dmg.TakeDamage(damageAmount);
                ApplyKnockBack(hit);
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
