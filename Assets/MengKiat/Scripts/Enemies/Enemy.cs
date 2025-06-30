using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField] protected EnemyData data;
    [SerializeField] DynamicHealthBar healthBar;
    [SerializeField] protected float currentHealth;
    protected int currentRound;
    protected float damage;


    protected virtual void Awake()
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

        Invoke(nameof(InitialiseHealthBar), 1f);
        damage = data.damage;
        }

  


    

    // Shared damage logic
    public virtual void TakeDamage(float amount)
    {
        currentHealth -= amount;
        UpdateHealthBar();
        //TextManager.Instance.CreateText(this.transform.position, amount.ToString(), Color.black);
        //Debug.Log("Get Hit");
         Debug.Log(this.name + " Get Hit: " + amount);
        if (currentHealth <= 0f)
            Die();
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

    private void UpdateHealthBar()
    {
        healthBar.SetHealth(currentHealth);
    }
}
