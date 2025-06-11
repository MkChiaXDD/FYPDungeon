using UnityEngine;

// Base class for all enemies
public class Enemy : MonoBehaviour
{
    [SerializeField] protected EnemyData data;
    protected float currentHealth;

    protected virtual void Awake()
    {
        currentHealth = data.maxHealth;
    }

    // Shared damage logic
    public virtual void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0f)
            Die();
    }

    // Shared death logic
    protected virtual void Die()
    {
        Destroy(gameObject);
    }
}
