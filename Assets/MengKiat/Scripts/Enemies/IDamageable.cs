using System.Collections;
using UnityEngine;

public class IDamageable : MonoBehaviour
{
    private float health = 100f;


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

    public void TakeDamage(float amount)
    {

        if (_renderer != null)
        {
            StartCoroutine(DamageEffect());
        }
        Debug.Log("ouch");
        health -= amount;
        if (health <= 0)
        {
            Debug.Log("health: " + health);
            Destroy(gameObject);

        }
    }

}



