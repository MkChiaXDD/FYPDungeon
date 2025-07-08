using Unity.VisualScripting;
using UnityEngine;

public class NormalSwordAttack : MonoBehaviour
{
    [Header("Combat Settings")]
    public ElementType attackElement = ElementType.Pyro;
    public PhysicalAttackType attackType = PhysicalAttackType.Sharp;
    [SerializeField] private float elementalDuration = 5;

    [SerializeField] private int damageAmount = 1;
    [SerializeField] private int knockbackForce = 5;
    [SerializeField] private float slashRadius = 5;



    public PoisonEffect poison;
    public StunEffect stun;

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
        // Heavy attack implementation with:
        // - SphereCastAll for AOE
        // - Damage calculation using damageMultiplier
        // - Visual effects scaled by radius
    }


    public void ElementalSwordAttack() //rememeber to swap elemental with normal
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

    public void ApplyStatusEffects(GameObject target, StatusEffect effect)
    {
        target.gameObject.GetComponent<StatusEffectReceiver>().ApplyEffect(effect);
    }

    public void PlayDmgVFX()
    {
        //screen shake when damaging enemy
       // StaticScreenShake.Shake(Camera.main, damageParams);

        //hit stop when hit the enemy
        HitStopManager.ActivateHitStopGlobal(0.25f,0.01f);
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
