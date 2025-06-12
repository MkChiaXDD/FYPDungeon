using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField] protected EnemyData data;
    protected float currentHealth;

    protected virtual void Awake()
    {
        currentHealth = data.maxHealth;
    }

    // This now fulfills IDamageable
    public virtual void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0f)
            Die();
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }
}
