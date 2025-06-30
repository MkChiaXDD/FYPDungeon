using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalSwordAttack : MonoBehaviour
{
    [Header("Combat Settings")]
    public ElementType attackElement = ElementType.Pyro;
    [SerializeField] private float elementalDuration;
    
    [SerializeField] private int damageAmount = 1;
    [SerializeField] private int knockbackForce = 5;
    [SerializeField] private float slashRadius = 5;



    public PoisonEffect poison;

    [Header("Visual effects")]
    [SerializeField] private GameObject slashGO;
    [SerializeField] private ParticleSystem slashVFX;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            attackElement = ElementType.Hydro;
            Debug.Log("Element type switched to " + attackElement);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            attackElement = ElementType.Electro;
            Debug.Log("Element type switched to " + attackElement);

        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            attackElement = ElementType.Cryo;
            Debug.Log("Element type switched to " + attackElement);

        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            attackElement = ElementType.Pyro;
            Debug.Log("Element type switched to " + attackElement);

        }
    }

    public void SwordAttack()
    {
       
        slashGO.transform.localRotation = Quaternion.Euler(Random.Range(-20 * Mathf.PerlinNoise1D(1), 30 * Mathf.PerlinNoise1D(1)),transform.rotation.y,transform.rotation.z) ;
        slashVFX.Play();
        // apply damage & knockback
        Collider[] hits = Physics.OverlapSphere(transform.position, slashRadius);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<Enemy>(out var hitEnemies) && !hit.CompareTag("Player"))
            {
                //hitEnemies.TakeDamage(damageAmount);
                ApplyElementalEffects(hit.gameObject);

                hitEnemies.TakeElementalDamage(damageAmount, attackElement);
              // hit.gameObject.GetComponent<StatusEffectReceiver>().ApplyEffect(poison);
                
                ApplyKnockBack(hitEnemies);
            }
        }
    }

    private void ApplyElementalEffects(GameObject target)
    {
        // Apply elemental effect
        if (target.TryGetComponent<ElementalStatus>(out var status))
        {
            status.ApplyElement(attackElement, elementalDuration);
            ElementalReactionManager.Instance.CheckReactions(
                status,
                attackElement,
                transform.position,
                damageAmount
            );
            Debug.Log("dealt " + attackElement + "element");
        }
    }

    public void ApplyKnockBack(Enemy hit)
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
