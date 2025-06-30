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

    //create hit effect anim
    private Color originalColour;
    private Color damageColour = Color.red;
    private float damageDuration = 0.5f;
    private Renderer _renderer;

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
        _renderer = GetComponent<Renderer>();
        originalColour = _renderer.material.color;
    }

  


    private IEnumerator DamageEffect()
    {
        _renderer.material.color = damageColour;
        float elapseTime = 0.0f;
        while (elapseTime <= damageDuration)
        {
            _renderer.material.color = Color.Lerp(damageColour, originalColour, elapseTime / damageDuration);
            elapseTime += Time.deltaTime;
            yield return null;
        }
        _renderer.material.color = originalColour;
    }

    // Shared damage logic
    public virtual void TakeDamage(float amount)
    {
        currentHealth -= amount;
        UpdateHealthBar();
        //TextManager.Instance.CreateText(this.transform.position, amount.ToString(), Color.black);
        //Debug.Log("Get Hit");
        DamageEffect();
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
