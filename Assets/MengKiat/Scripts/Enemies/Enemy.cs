using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [Header("References")]
    [SerializeField] protected EnemyData data;
    [SerializeField] DynamicHealthBar healthBar;
    [SerializeField] public float currentHealth;
    [SerializeField] private GameObject shieldPrefab;
    private GameObject enemyShield;
    [SerializeField] private float shieldHp;
    public float maxHealth => data.maxHealth;
    protected int currentRound;
    protected float damage;

    [Header("Elemental Resistances")]
    [Tooltip("1 = Normal, >1 = Resistant, <1 = Weak")]
    [Range(0, 2)] public float pyroResistance = 1f;
    [Range(0, 2)] public float hydroResistance = 1f;
    [Range(0, 2)] public float electroResistance = 1f;
    [Range(0, 2)] public float cryoResistance = 1f;

    // Elemental status effects
    private Dictionary<ElementType, float> activeElementalEffects = new Dictionary<ElementType, float>();

    [SerializeField] protected float stunDuration = 1f;
    protected bool isStunned = false;
    protected Coroutine stunCoroutine;


    protected virtual void Awake()
    {
        InitialiseDifficulty();
        InitialiseShield();
        Invoke(nameof(InitialiseHealthBar), 1f);
        transform.AddComponent<ElementalStatus>();
    }

    public void InitialiseShield()
    {
        if (shieldPrefab != null && enemyShield == null)
        {
            enemyShield = Instantiate(shieldPrefab, transform.position, Quaternion.identity);

            enemyShield.transform.SetParent(transform);

            enemyShield.transform.localPosition = new Vector3(0, 1.5f, 0);

            // Initialize the shield's HP
            EnemyShield shield = enemyShield.GetComponent<EnemyShield>();
            if (shield != null)
            {
                shield.Init(shieldHp);
            }
            else
            {
                Debug.LogWarning("Spawned shield is missing EnemyShield component.");
            }
        }
    }

    private void ShieldTakeDamage(float amount, PhysicalAttackType physicalAttackType)
    {



        EnemyShield shield = enemyShield.GetComponent<EnemyShield>();
        float currentShieldHealth = shield.GetShieldHp();
        if (currentShieldHealth > 0)
        {
            shield.HitShield(amount,physicalAttackType);
            return;
        }

    }


    // Shared damage logic
    public virtual void TakeDamage(float amount)
    {
        currentHealth -= amount;
        ShowDamageNumber(this.transform.position, amount);
        UpdateHealthBar();
        //PlayDamageVFX();
        //TextManager.Instance.CreateText(transform.position, amount.ToString(), Color.black);
        Debug.Log(name + " Get Hit: " + amount);
        if (currentHealth <= 0f)
            Die();

        
    }

    public void TakeElementalDamage(float amount, ElementType elementType)
    {  
        // Calculate resistance multiplier
        float resistanceMultiplier = GetResistanceMultiplier(elementType);
        float finalDamage = amount / resistanceMultiplier;

        // Apply elemental effect (burning, electrocution, etc.)
        ApplyElementalEffect(elementType, finalDamage);
        TakeDamage(finalDamage);
    }

    public void TakePhysicalDamage(float damage, PhysicalAttackType attackType)
    {
        if (enemyShield != null && enemyShield.GetComponent<EnemyShield>().GetShieldHp() > 0)
        {
            if (enemyShield.GetComponent<EnemyShield>())
                ShieldTakeDamage(damage, attackType);
            ShowDamageNumber(this.transform.position, damage, Color.gray);

            return;
        }


        // Calculate resistance multiplier
        float resistanceMultiplier = GetResistanceMultiplier(attackType);
        float finalDamage = damage / resistanceMultiplier;

    
        TakeDamage(finalDamage);
    }

    private float GetResistanceMultiplier(ElementType elementType)
    {
        return elementType switch
        {
            ElementType.Pyro => pyroResistance,
            ElementType.Hydro => hydroResistance,
            ElementType.Electro => electroResistance,
            ElementType.Cryo => cryoResistance,
            _ => 1f
        };
    }

    private float GetResistanceMultiplier(PhysicalAttackType elementType)
    {
        return elementType switch
        {
            PhysicalAttackType.Sharp => pyroResistance,
            PhysicalAttackType.Blunt => hydroResistance,      
            _ => 1f
        };
    }

    // Shared death logic
    public virtual void Die()
    {
        if (gameObject.GetComponent<BossCheckDeath>() != null)
        {
            gameObject.GetComponent<BossCheckDeath>().SummonPortal();
            Destroy(gameObject.GetComponent<BossCheckDeath>());
            Debug.Log("BOSS DIES");
        }

        StaticScreenShake.Shake(Camera.main, deathParams);
        Destroy(gameObject);
    }

    private void InitialiseHealthBar()
    {
        healthBar.SetMaxHealth(currentHealth);
        UpdateHealthBar();
    }

    private void InitialiseDifficulty()
    {
        DifficultyManager difficulty = FindFirstObjectByType<DifficultyManager>();

        float multiplier = 1f; // default multiplier
        currentRound = 1;      // default round

        if (difficulty != null)
        {
            currentRound = difficulty.GetRound();
            multiplier = difficulty.GetDifficultyMultiplier();
        }
        else
        {
            Debug.LogWarning("[Enemy] No DifficultyManager found. Using default values.");
        }

        int finalHealth = Mathf.RoundToInt(data.maxHealth * multiplier);
        currentHealth = finalHealth;

        Debug.Log($"[Enemy] ROUND: {currentRound} | MULTIPLIER: {multiplier} | FINAL HEALTH: {currentHealth}");

        damage = data.damage;



        InitialiseResistance();
    }

    private void ShowDamageNumber(Vector3 position, float damage, Color color)
    {
        if (DamageNumberManager.Instance)
        {
            DamageNumberManager.Instance.ShowDamage(position, damage, color);
        }
    }

    //default color is white
    private void ShowDamageNumber(Vector3 position, float damage)
    {
        if (DamageNumberManager.Instance)
        {
            DamageNumberManager.Instance.ShowDamage(position, damage);
        }
    }

    private void InitialiseResistance()
    {
        pyroResistance = data.pyroResistance;
        cryoResistance = data.cryoResistance;
        electroResistance = data.electroResistance;
        hydroResistance = data.hydroResistance;
    }

    private void UpdateHealthBar()
    {
        healthBar.SetHealth(currentHealth);
    }
    private void ApplyElementalEffect(ElementType elementType, float damageAmount)
    {

        // Example: Apply burning effect for Pyro damage
        if (elementType == ElementType.Pyro)
        {
            // Start or refresh burning effect*
            if (TryGetComponent<BurningEffect>(out var burning))
            {
                burning.RefreshEffect(damageAmount);
            }
            else
            {

                burning = gameObject.AddComponent<BurningEffect>();
                burning.Initialize(damageAmount, this);
            }
        }
        if (elementType == ElementType.Electro)
        {
            {
                // Start or refresh burning effect*
                if (TryGetComponent<ElectroEffect>(out var electro))
                {
                    electro.RefreshEffect(damageAmount);
                }
                else
                {

                    electro = gameObject.AddComponent<ElectroEffect>();
                    electro.Initialize(damageAmount, this);
                }
            }
        }
        // Add similar effects for other elements:
        // - Hydro: Wet status (increased Electro damage)
        // - Electro: Stun effect
        // - Cryo: Slow movement

        // Track elemental effect for visual feedback
        activeElementalEffects[elementType] = Time.time + 3f; // Effect lasts 3 seconds

    }
    public virtual void ApplyStun(float duration)
    {
        if (data.enemyType == EnemyData.EnemyType.notNormalEnemy) return;

        if (stunCoroutine != null)
            StopCoroutine(stunCoroutine);

        stunCoroutine = StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        isStunned = true;
        yield return new WaitForSeconds(duration);
        isStunned = false;
        stunCoroutine = null;
    }

    public virtual void Heal(float healAmount)
    {
        if (currentHealth < data.maxHealth)
        {
            currentHealth += healAmount;
            if (currentHealth > data.maxHealth)
            {
                currentHealth = data.maxHealth;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(transform.position, data.detectionRange);

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position, data.attackRange);
    }

    private void PlayDamageVFX()
    {
        HitStopVFX();
    }
    private void HitStopVFX()
    {
        HitStopManager.ActivateHitStopGlobal();
    }



    protected StaticScreenShake.ShakeParams deathParams = new()
    {

        ShakeType = ShakeType.Translational,
        ShakeDuration = 0.25f,      // A quarter of a second
        ShakeMagnitude = 2.5f,       // Rotational magnitude (in degrees) - keep it small for 2D
        DampingSpeed = 10f,          // Damping speed to return to normal
        RotationalNoiseSpeed = 20f,  // Noise speed for rotation
        TranslationalShakeMagnitude = new Vector3(0.5f, 0.5f, 0f), // Shake in X and Y equally
        TranslationalNoiseSpeed = 50f,
        UseSeparateNoiseForTranslation = true,
        EnableX = false,
        EnableY = true,
        EnableZ = false              // No Z for 2D
    };

    protected StaticScreenShake.ShakeParams strongerShake = new()
    {

        ShakeType = ShakeType.Translational,
        ShakeDuration = 0.25f,      // A quarter of a second
        ShakeMagnitude = 2.5f,       // Rotational magnitude (in degrees) - keep it small for 2D
        DampingSpeed = 10f,          // Damping speed to return to normal
        RotationalNoiseSpeed = 20f,  // Noise speed for rotation
        TranslationalShakeMagnitude = new Vector3(1f, 1f, 0f), // Shake in X and Y equally
        TranslationalNoiseSpeed = 50f,
        UseSeparateNoiseForTranslation = true,
        EnableX = false,
        EnableY = true,
        EnableZ = false              // No Z for 2D
    };
}
