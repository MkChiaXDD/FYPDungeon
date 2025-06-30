using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageableEntity : MonoBehaviour, IDamageable
{
    protected float maxHealth = 100;
    protected float currentHealth = 100;


    //create hit effect anim
    private Color originalColour;
    private Color damageColour = Color.red;
    private float damageDuration = 0.5f;
    private Renderer _renderer;

    private void Start()
    {
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

    public virtual void TakeDamage(float amount)
    {

        if (_renderer != null)
        {
            StartCoroutine(DamageEffect());
        }

        currentHealth -= amount;

        Debug.Log("ouch");
        Debug.Log("health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public virtual void Die()
    {
        Destroy(gameObject);
    }

    protected void HealHealthToMax()
    {
        currentHealth = maxHealth;
    }

    protected void SetMaxHealth(int MaxHealthAmount)
    {
        maxHealth = MaxHealthAmount;
    }

}
