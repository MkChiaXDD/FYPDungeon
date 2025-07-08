using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakable : MonoBehaviour, IDamageable
{
    [SerializeField] private GameObject brokenObject;
    [SerializeField] private ItemDropSystem dropSystem;

    [SerializeField] float explodeRange = 1.5f;
    [SerializeField] float explosionRadius = 3f;
    [SerializeField] float explosionForce = 5f;
    [SerializeField] private float explosionUpwardModifier = 1f;
    [SerializeField] LayerMask everyMask;
    // Start is called before the first frame update
    private IEnumerator BreakObject()
    {
        yield return Instantiate(brokenObject, transform.position, Quaternion.Euler(0, 0, 0));
        SelfExplode();
        if (dropSystem)
        { 
            dropSystem.SpawnDropItem();
        }     
        Destroy(gameObject);
    }
    //spreads itself outwards, does not affect anything else
    void SelfExplode()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position,explosionRadius,~everyMask);
        foreach (var hit in hits)
        {   
            if (hit.attachedRigidbody != null)
            {
                hit.attachedRigidbody.AddExplosionForce(
                    explosionForce,            // base force
                    transform.position,        // origin
                    explosionRadius,           // radius
                    explosionUpwardModifier,   // upwards modifier
                    ForceMode.Impulse          // instant burst
                );
            }
        }
    }
    public void TakeElementalDamage(float damage, ElementType element) => Die();
    public void TakeDamage(float damage) { Debug.Log("no implementation of TakeDamage in breakable currently, it is replaced by physicalDamage");  }
    public void TakePhysicalDamage(float damage, AttackType attackType)
    {
        if (attackType == AttackType.Blunt)
        {
            Die();
        }
    }
    public void Die() => StartCoroutine(nameof(BreakObject));
    public void DropItem() => dropSystem.SpawnDropItem();

    public void Heal(float healAmoount){/*yes this function does nothing, do not implement*/}
}
