using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakable : MonoBehaviour, IDamageable
{
    [SerializeField] private GameObject brokenObject;
    [SerializeField] private ItemDropSystem dropSystem;
    [SerializeField] private PhysicalAttackType attackTypeToBreak;
    [SerializeField] private string BreakSFXName;

    [SerializeField] float explodeRange = 1.5f;
    [SerializeField] float explosionRadius = 3f;
    [SerializeField] float explosionForce = 5f;
    [SerializeField] private float explosionUpwardModifier = 1f;
    [SerializeField] LayerMask everyMask;

    [SerializeField] private List<StatusEffect> effects = new List<StatusEffect>();
    // Start is called before the first frame update
    private IEnumerator BreakObject()
    {
        if (brokenObject)
        yield return Instantiate(brokenObject, transform.position, Quaternion.Euler(0, 0, 0));

        if (dropSystem)
        {
            dropSystem.SpawnDropItem();
        }

        SelfExplode();
        
        PlayBreakSFX(BreakSFXName);
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
                    ForceMode.Force          // instant burst
                );
            }
        }
    }

    private void PlayBreakSFX(string BreakSFX)
    {
        SoundManager.Instance.PlaySFX(BreakSFX, this.gameObject);
    }
    public void TakeElementalDamage(float damage, ElementType element) { Debug.Log("no implementation of TakeDamage in breakable currently, it is replaced by physicalDamage"); }
    public void TakeDamage(float damage) { Debug.Log("no implementation of TakeDamage in breakable currently, it is replaced by physicalDamage");  }
    public void TakePhysicalDamage(float damage, PhysicalAttackType attackType)
    {
        //if (attackType == PhysicalAttackType.Blunt)
        //{
        //    Die();
        //}
        Die();
    }
    public void Die() => StartCoroutine(nameof(BreakObject));
    public void DropItem() => dropSystem.SpawnDropItem();

    public void Heal(float healAmoount){/*yes this function does nothing, do not implement*/}
}
