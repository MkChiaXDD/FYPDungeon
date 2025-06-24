using Unity.VisualScripting;
using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField] protected EnemyData data;
    [SerializeField] DynamicHealthBar healthBar;
    [SerializeField] protected int currentHealth;

    protected virtual void Awake()
    {
        int level = FindFirstObjectByType<DifficultyManager>().GetRound();
        currentHealth = data.maxHealth * level;
        if (healthBar != null)
        {
            InitialiseHealthBar();
        }
    }

    // Shared damage logic
    public virtual void TakeDamage(int amount)
    {
        currentHealth -= amount;
        //UpdateHealthBar(currentHealth);
        Debug.Log("Get Hit");
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
        healthBar.SetMaxHealth(data.maxHealth);
        healthBar.SetHealth(currentHealth);
    }

    private void UpdateHealthBar(int health)
    {
        healthBar.SetHealth(health);
    }
}
