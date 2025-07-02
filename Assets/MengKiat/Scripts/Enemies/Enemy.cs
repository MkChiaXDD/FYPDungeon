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
    public float maxHealth => data.maxHealth;
    protected int currentRound;
    protected float damage;

    [Header("Elemental Resistances")]
    [Tooltip("1 = Normal, <1 = Resistant, >1 = Weak")]
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
        Invoke(nameof(InitialiseHealthBar), 1f);
        transform.AddComponent<ElementalStatus>();
    }

    // Shared damage logic
    public virtual void TakeDamage(float amount)
    {
        currentHealth -= amount;
        UpdateHealthBar();
        //TextManager.Instance.CreateText(this.transform.position, amount.ToString(), Color.black);
        Debug.Log(this.name + " Get Hit: " + amount);
        if (currentHealth <= 0f)
            Die();
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

    public void TakeElementalDamage(float amount, ElementType elementType)
    {
    
        // Calculate resistance multiplier
        float resistanceMultiplier = GetResistanceMultiplier(elementType);
        float finalDamage = amount * resistanceMultiplier;

        // Apply elemental effect (burning, electrocution, etc.)
        ApplyElementalEffect(elementType, finalDamage);

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

    // Shared death logic
    public virtual void Die()
    {
        if (gameObject.GetComponent<BossCheckDeath>() != null)
        {
            gameObject.GetComponent<BossCheckDeath>().DieProceed();
            Destroy(gameObject.GetComponent<BossCheckDeath>());
            Debug.Log("BOSS DIES");
        }
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
    }

    private void UpdateHealthBar()
    {
        healthBar.SetHealth(currentHealth);
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(transform.position, data.detectionRange);

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position, data.attackRange);
    }
}
