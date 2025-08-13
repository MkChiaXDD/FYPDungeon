using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAttackScript : MonoBehaviour
{
    [Header("Combat Settings")]
    public ElementType attackElement = ElementType.None;
    public PhysicalAttackType baseAttackType = PhysicalAttackType.Sharp;
    public PhysicalAttackType secondaryAttackType = PhysicalAttackType.Blunt;

    [SerializeField] protected float elementalDuration = 5f;
    [SerializeField] protected int damageAmount = 1;
    [SerializeField] protected int knockbackForce = 5;
    [SerializeField] protected float attackRadius = 5f;

    [Header("Status Effects")]
    public List<StatusEffect> statusEffects = new List<StatusEffect>();

    [Header("Visual Effects")]
    [SerializeField] protected ParticleSystem lightAttackVFX;
    [SerializeField] protected ParticleSystem heavyAttackVFX;

    protected StaticScreenShake.ShakeParams damageParams = new()
    {
        ShakeType = ShakeType.Translational,
        ShakeDuration = 0.25f,
        ShakeMagnitude = 2.5f,
        DampingSpeed = 10f,
        RotationalNoiseSpeed = 20f,
        TranslationalShakeMagnitude = new Vector3(0.25f, 0.25f, 0f),
        TranslationalNoiseSpeed = 50f,
        UseSeparateNoiseForTranslation = true,
        EnableX = true,
        EnableY = true,
        EnableZ = false
    };

    /// <summary>
    /// default attack for player
    /// </summary>
    public virtual void ExecuteLightAttack()
    {
        Quaternion vfxRotation = transform.rotation * Quaternion.Euler(-90, 0, 0);
        ParticleSystem LightVfxInstance = Instantiate(lightAttackVFX, transform.position, vfxRotation);
        LightVfxInstance.Play();
        Destroy(LightVfxInstance.gameObject, 1f);
        ApplyAttack(transform.position, attackRadius, damageAmount, baseAttackType);

        Debug.LogWarning(baseAttackType);

        PlayLightScreenShakeVFX();
    }

    public virtual void ExecuteHeavyAttack(Vector3 center, float damageMultiplier, float radius)
    {
        Quaternion rotation = FindObjectOfType<PlayerMovement>().GetDirectionQuaternion() * Quaternion.Euler(-90, 0, 0);
        ParticleSystem HeavyVfxInstance = Instantiate(heavyAttackVFX, center, rotation);
        HeavyVfxInstance.Play();
        Destroy(HeavyVfxInstance.gameObject, 2f);

        int scaledDamage = Mathf.RoundToInt(damageAmount * damageMultiplier);
        ApplyAttack(center, radius, scaledDamage, secondaryAttackType, damageMultiplier);
        PlayHeavyAttackVFX(scaledDamage);
    }

    public virtual void ExecuteLightAttack(ElementType attackElement)
    {
        Quaternion vfxRotation = transform.rotation * Quaternion.Euler(-90, 0, 0);
        ParticleSystem LightVfxInstance = Instantiate(lightAttackVFX, transform.position, vfxRotation);
        LightVfxInstance.Play();
        Destroy(LightVfxInstance.gameObject, 1f);
        ApplyAttack(transform.position, attackRadius, damageAmount, attackElement);

        Debug.LogWarning(baseAttackType);

        PlayLightScreenShakeVFX();
    }

    public virtual void ExecuteHeavyAttack(Vector3 center, float damageMultiplier, float radius, ElementType attackElement)
    {
        Quaternion rotation = FindObjectOfType<PlayerMovement>().GetDirectionQuaternion() * Quaternion.Euler(-90, 0, 0);
        ParticleSystem HeavyVfxInstance = Instantiate(heavyAttackVFX, center, rotation);
        HeavyVfxInstance.Play();
        Destroy(HeavyVfxInstance.gameObject, 2f);

        int scaledDamage = Mathf.RoundToInt(damageAmount * damageMultiplier);
        ApplyAttack(center, radius, scaledDamage, attackElement, damageMultiplier);
        PlayHeavyAttackVFX(scaledDamage);
    }

    protected virtual void ApplyAttack(Vector3 center, float radius, int damage, PhysicalAttackType physicalType, float intensity = 1f)
    {
        Collider[] hits = Physics.OverlapSphere(center, radius);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Player") && hit.TryGetComponent<IDamageable>(out var target))
            {
                ProcessTargetHit(hit, target, damage, physicalType, intensity);
            }
        }
    }

    protected virtual void ApplyAttack(Vector3 center, float radius, int damage, ElementType elementalType, float intensity = 1f)
    {
        Collider[] hits = Physics.OverlapSphere(center, radius);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Player") && hit.TryGetComponent<IDamageable>(out var target))
            {
                ProcessTargetHit(hit, target, damage, elementalType, intensity);
            }
        }
    }



    protected virtual void ProcessTargetHit(Collider hit, IDamageable target, int damage, PhysicalAttackType physicalType, float intensity)
    {
        if (hit.CompareTag("Object"))
        {
            target.TakePhysicalDamage(damage, physicalType);
        }
        else
        {
            target.TakePhysicalDamage(damage, physicalType);
            ApplyKnockBack(hit.gameObject, knockbackForce * intensity);
            PlayAttackVFX();
        }
    }

    protected virtual void ProcessTargetHit(Collider hit, IDamageable target, int damage, ElementType elementType, float intensity)
    {
        if (hit.CompareTag("Object"))
        {
            target.TakeElementalDamage(damage, elementType);
        }
        else
        {
            ApplyElementalEffects(hit.gameObject);
            Debug.LogWarning("ultra dog shit");
            target.TakeElementalDamage(damage, elementType);
            ApplyKnockBack(hit.gameObject, knockbackForce * intensity);
            PlayAttackVFX();
        }
    }

    protected virtual void ApplyElementalEffects(GameObject target)
    {
        if (target.TryGetComponent<ElementalStatus>(out var status))
        {
            status.ApplyElement(attackElement, elementalDuration);
            ElementalReactionManager.Instance.CheckReactions(
                status,
                attackElement,
                transform.position,
                damageAmount
            );
        }
        else
        {
            var newStatus = target.AddComponent<ElementalStatus>();
            newStatus.ApplyElement(attackElement, elementalDuration);
            ElementalReactionManager.Instance.CheckReactions(
                newStatus,
                attackElement,
                transform.position,
                damageAmount
            );
        }
    }

    protected virtual void ApplyKnockBack(GameObject target, float force)
    {
        if (target.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 direction = (target.transform.position - PlayerMovement.Instance.GetPosition()).normalized;
            direction.y = 0.3f;
            rb.AddForce(direction * force, ForceMode.Impulse);
        }
    }

    protected virtual void ApplyStatusEffect(GameObject target, StatusEffect effect)
    {
        var damageable = target.GetComponent<IDamageable>();
        if (damageable == null) return;

        var receiver = target.GetComponent<StatusEffectReceiver>() ?? target.AddComponent<StatusEffectReceiver>();
        receiver.ApplyEffect(effect);
    }

    protected virtual void PlayAttackVFX()
    {
        ActivateHitStop(0.25f, 0.01f);
    }

    protected virtual void ActivateHitStop(float duration, float timescale)
    {
        HitStopManager.ActivateHitStopGlobal(duration, timescale);
    }

    protected virtual void PlayLightScreenShakeVFX()
    {
        var shakeParams = new StaticScreenShake.ShakeParams
        {
            ShakeType = ShakeType.Translational,
            ShakeDuration = 0.2f,
            ShakeMagnitude = 3f,
            DampingSpeed = 10f,
            TranslationalShakeMagnitude = new Vector3(0.4f, 0.4f, 0f),
            TranslationalNoiseSpeed = 70f,
            EnableX = true,
            EnableY = true
        };
        StaticScreenShake.Shake(Camera.main, shakeParams);
    }
    protected virtual void PlayHeavyAttackVFX(float intensity)
    {  
        var shakeParams = new StaticScreenShake.ShakeParams
        {
            ShakeType = ShakeType.Translational,
            ShakeDuration = 0.35f,
            ShakeMagnitude = 3f,
            DampingSpeed = 10f,
            TranslationalShakeMagnitude = new Vector3(0.4f , 0.4f , 0f),
            TranslationalNoiseSpeed = 70f,
            EnableX = true,
            EnableY = true
        };
        StaticScreenShake.Shake(Camera.main, shakeParams);
    }
}