using System.Collections.Generic;
using UnityEngine;

public class Breakable : MonoBehaviour, IDamageable
{
    [Header("Visuals")]
    [SerializeField] private GameObject brokenObject;
    [SerializeField] private string breakSFXName;
    [SerializeField] private ParticleSystem dustPrefab;

    [Header("Drops")]
    [SerializeField] private ItemDropSystem dropSystem;

    [Header("Explosion")]
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private float explosionForce = 5f;
    [SerializeField] private float explosionUpwardModifier = 1f;
    [SerializeField] private LayerMask forceAffectedLayers;
    [SerializeField] private LayerMask damageAffectedLayers;

    [Header("Damage")]
    [SerializeField] private float baseDestructionDamage = 5;
    [SerializeField] private float stunDuration = 0.5f;

    [Header("Effects")]
    [SerializeField] private List<StatusEffect> statusEffects = new List<StatusEffect>();

    private enum StatusEffectList
    {
        STUN = 0,
        NONE = 1,
    }

    private Collider[] explosionResults = new Collider[32];
    private static readonly Quaternion DefaultRotation = Quaternion.identity;

    public void TakePhysicalDamage(float damage, PhysicalAttackType attackType) => Die();
    public void TakeElementalDamage(float damage, ElementType element) => Die();
    public void TakeDamage(float damage) => Die();
    public void Heal(float healAmount) { /* Intentionally empty */ }

    public void Die()
    {
        SpawnBrokenObject();
        TryDropItem();
        ApplyExplosionEffects();
        
        PlayBreakFX();
        Destroy(gameObject);
    }

    private void SpawnBrokenObject()
    {
        if (brokenObject)
        {
            GameObject broken = Instantiate(brokenObject, transform.position, DefaultRotation);
            broken.transform.localScale = gameObject.transform.lossyScale;
        }
    }

    private void TryDropItem()
    {
        if (dropSystem)
            dropSystem.SpawnDropItem();
    }

    private void ApplyExplosionEffects()
    {
        int hitCount = Physics.OverlapSphereNonAlloc(
            transform.position,
            explosionRadius,
            explosionResults,
            forceAffectedLayers | damageAffectedLayers
        );

        for (int i = 0; i < hitCount; i++)
        {
            ProcessForceApplication(explosionResults[i]);
            ProcessDamageAndEffects(explosionResults[i]);
        }
    }

    private void ProcessForceApplication(Collider hit)
    {
        if (!forceAffectedLayers.ContainsLayer(hit.gameObject.layer))
            return;

        Rigidbody rb = hit.attachedRigidbody;
        if (rb && !rb.isKinematic)
        {
            rb.AddExplosionForce(
                explosionForce,
                transform.position,
                explosionRadius,
                explosionUpwardModifier,
                ForceMode.Impulse
            );
        }
    }

    private void ProcessDamageAndEffects(Collider hit)
    {
        if (!damageAffectedLayers.ContainsLayer(hit.gameObject.layer))
            return;

        if (hit.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(baseDestructionDamage);
            ApplyStun(hit.gameObject);
        }
    }
    
    private void ApplyStun(GameObject target)
    {     
        ApplyStatusEffect(target, statusEffects[(int)StatusEffectList.STUN]);
    }

    protected virtual void ApplyStatusEffect(GameObject target, StatusEffect effect)
    {
        var receiver = target.GetComponent<StatusEffectReceiver>() ?? target.AddComponent<StatusEffectReceiver>();
        receiver.ApplyEffect(effect);
    }

    private void PlayBreakFX()
    {
        PlayBreakSound();
        PlayDustVFX();
    }

    private void PlayDustVFX()
    {
        if (dustPrefab != null)
        {
            dustPrefab.Play();
        }
    }

    private void PlayBreakSound()
    {
        if (!string.IsNullOrEmpty(breakSFXName) && SoundManager.Instance)
        {
            SoundManager.Instance.PlaySFX(breakSFXName);
        }
    }

   

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}

public static class LayerMaskExtensions
{
    public static bool ContainsLayer(this LayerMask mask, int layer)
    {
        return (mask.value & (1 << layer)) != 0;
    }
}