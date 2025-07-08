using Unity.VisualScripting;
using UnityEngine;

public class NormalSwordAttack : MonoBehaviour
{
    [Header("Combat Settings")]
    public ElementType attackElement = ElementType.Pyro;
    public PhysicalAttackType SharpAttackType = PhysicalAttackType.Sharp;
    public PhysicalAttackType BluntAttackType = PhysicalAttackType.Blunt;
    [SerializeField] private float elementalDuration = 5;

    [SerializeField] private int damageAmount = 1;
    [SerializeField] private int knockbackForce = 5;
    [SerializeField] private float slashRadius = 5;



    public PoisonEffect poison;
    public StunEffect stun;

    [Header("Visual effects")]
    [SerializeField] private GameObject slashGO;
    [SerializeField] private ParticleSystem slashVFX;

    [SerializeField] private GameObject heavySlashGO;
    [SerializeField] private ParticleSystem heavySlashVFX;

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

    public void ExecuteLightAttack()
    {
        //slashGO.transform.localRotation = Quaternion.Euler(Random.Range(-20 * Mathf.PerlinNoise1D(1), 30 * Mathf.PerlinNoise1D(1)),transform.rotation.y,transform.rotation.z) ;
        slashVFX.Play();
        // apply damage & knockback
        Collider[] hits = Physics.OverlapSphere(transform.position, slashRadius);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out var hitTargets) && !hit.CompareTag("Player"))
            {
                if (hit.CompareTag("Object"))
                {
                    hitTargets.TakeElementalDamage(damageAmount, attackElement);
                }
                else
                {
                    //hitEnemies.TakeDamage(damageAmount);
                    ApplyElementalEffects(hit.gameObject);

                    hitTargets.TakeElementalDamage(damageAmount, attackElement);
                    // ApplyStatusEffects(hit.gameObject, stun);
                    // hit.gameObject.GetComponent<StatusEffectReceiver>().ApplyEffect(poison);
                    ApplyKnockBack(hit.gameObject);
                    PlayDmgVFX();
                }
            }
        }
    }

    public void ExecuteAttack(AttackType light)
    {
        //slashGO.transform.localRotation = Quaternion.Euler(Random.Range(-20 * Mathf.PerlinNoise1D(1), 30 * Mathf.PerlinNoise1D(1)),transform.rotation.y,transform.rotation.z) ;
        slashVFX.Play();
        // apply damage & knockback
        Collider[] hits = Physics.OverlapSphere(transform.position, slashRadius);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out var hitTargets) && !hit.CompareTag("Player"))
            {
                if (hit.CompareTag("Object"))
                {
                    hitTargets.TakeElementalDamage(damageAmount, attackElement);
                }
                else
                {
                    //hitEnemies.TakeDamage(damageAmount);
                    ApplyElementalEffects(hit.gameObject);

                    hitTargets.TakeElementalDamage(damageAmount, attackElement);
                    // ApplyStatusEffects(hit.gameObject, stun);
                    // hit.gameObject.GetComponent<StatusEffectReceiver>().ApplyEffect(poison);
                    ApplyKnockBack(hit.gameObject);
                    PlayDmgVFX();
                }
            }
        }
    }

    public void ExecuteHeavyAttack(Vector3 center, float damageMultiplier, float radius)
    {

        // Create heavy attack visual effect
        ParticleSystem heavyVFX = Instantiate(heavySlashVFX, center, this.transform.rotation);        
        heavyVFX.Play();
        Destroy(heavyVFX.gameObject, 2f);

        // Apply damage in a sphere around the center point
        Collider[] hits = Physics.OverlapSphere(center, radius);

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out var hitTargets) && !hit.CompareTag("Player"))
            {
                int scaledDamage = Mathf.RoundToInt(damageAmount * damageMultiplier);
                float scaledKnockback = knockbackForce * damageMultiplier;

                if (hit.CompareTag("Object"))
                {
                    hitTargets.TakePhysicalDamage(scaledDamage, BluntAttackType);
                }
                else
                {
                    hitTargets.TakePhysicalDamage(scaledDamage, BluntAttackType);
                    ApplyKnockBack(hit.gameObject, scaledKnockback);

                    // Apply stun effect to all enemies hit by heavy attack
                    if (stun != null)
                    {
                        ApplyStatusEffects(hit.gameObject, stun);
                    }
                }
            }
        }

        // Special effects for heavy attack
        PlayHeavyDmgVFX(damageMultiplier); 
    }



    public void ElementalSwordAttack() //rememeber to swap elemental with normal
    {
        slashVFX.Play();
        // apply damage & knockback
        Collider[] hits = Physics.OverlapSphere(transform.position, slashRadius);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out var hitTargets) && !hit.CompareTag("Player"))
            {
                if (hit.CompareTag("Object"))
                {
                    hitTargets.TakeElementalDamage(damageAmount, attackElement);
                }
                else
                {
                    //hitEnemies.TakeDamage(damageAmount);
                    ApplyElementalEffects(hit.gameObject);

                    hitTargets.TakeElementalDamage(damageAmount, attackElement);
                    // ApplyStatusEffects(hit.gameObject, stun);
                    // hit.gameObject.GetComponent<StatusEffectReceiver>().ApplyEffect(poison);
                    ApplyKnockBack(hit.gameObject);
                    PlayDmgVFX();
                }
            }
        }
    }

    private void DamageEnemy(GameObject enemyTarget)
    {
        // When hitting an enemy
        DamageTypeManager.Instance.ApplyDamage(
            enemyTarget.GetComponent<ResistanceProfile>(),
        DamageType.Sharp,
        50f
    );
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
            Debug.Log("dealt " + attackElement + "element to " + target);
        }
        else
        {


            target.AddComponent<ElementalStatus>().ApplyElement(attackElement, elementalDuration);
            ElementalReactionManager.Instance.CheckReactions(
                status,
                attackElement,
                transform.position,
                damageAmount
            );
            Debug.Log("dealt " + attackElement + "element to " + target);

        }
    }

    public void ApplyKnockBack(GameObject hit)
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

    // Modified ApplyKnockBack to accept custom force
    public void ApplyKnockBack(GameObject hit, float force)
    {
        Rigidbody enemyRb = hit.GetComponent<Rigidbody>();
        if (enemyRb != null)
        {
            Vector3 knockbackDirection = hit.transform.position - transform.position;
            knockbackDirection.y = 0; // Keep the knockback horizontal
            knockbackDirection.Normalize();

            // Add upward force for more dramatic effect
            Vector3 forceVector = (knockbackDirection + Vector3.up * 0.3f).normalized * force;
            enemyRb.AddForce(forceVector, ForceMode.Impulse);
        }
    }

    public void ApplyStatusEffects(GameObject target, StatusEffect effect)
    {
        target.gameObject.GetComponent<StatusEffectReceiver>().ApplyEffect(effect);
    }

    public void PlayDmgVFX()
    {
        //screen shake when damaging enemy
        // StaticScreenShake.Shake(Camera.main, damageParams);

        //hit stop when hit the enemy
        HitStopManager.ActivateHitStopGlobal(0.25f, 0.01f);
    }


    // Special VFX for heavy attacks
    private void PlayHeavyDmgVFX(float intensity)
    {
        // Create impact effect at attack center
        if (slashGO != null)
        {
            GameObject impact = Instantiate(slashGO, transform.position, Quaternion.identity);
            impact.transform.localScale = Vector3.one * intensity;
            Destroy(impact, 1f);
        }


        // More intense screen shake
        StaticScreenShake.Shake(Camera.main, new StaticScreenShake.ShakeParams
        {
            ShakeType = ShakeType.Translational,
            ShakeDuration = 0.35f * intensity,
            ShakeMagnitude = 3f * intensity,
            DampingSpeed = 10f,
            TranslationalShakeMagnitude = new Vector3(0.4f * intensity, 0.4f * intensity, 0f),
            TranslationalNoiseSpeed = 70f,
            EnableX = true,
            EnableY = true
        });
    }

    protected StaticScreenShake.ShakeParams damageParams = new()
    {

        ShakeType = ShakeType.Translational,
        ShakeDuration = 0.25f,      // A quarter of a second
        ShakeMagnitude = 2.5f,       // Rotational magnitude (in degrees) - keep it small for 2D
        DampingSpeed = 10f,          // Damping speed to return to normal
        RotationalNoiseSpeed = 20f,  // Noise speed for rotation
        TranslationalShakeMagnitude = new Vector3(0.25f, 0.25f, 0f), // Shake in X and Y equally
        TranslationalNoiseSpeed = 50f,
        UseSeparateNoiseForTranslation = true,
        EnableX = true,
        EnableY = true,
        EnableZ = false              // No Z for 2D
    };



}
