
// Example elemental effect component
using System.Collections;
using UnityEngine;

public class BurningEffect : MonoBehaviour
{
    private float damagePerSecond;
    private float duration = 4;
    private IDamageable target;
    private ParticleSystem burningVFX;

    public void Initialize(float baseDamage, IDamageable targetEnemy)
    {
        target = targetEnemy;
        damagePerSecond = baseDamage * 0.2f; // 10% of initial damage per second
        duration = 4f;

        // Create VFX
        GameObject vfxObj = Instantiate(ElementalReactionManager.Instance.FireVFX, transform);
        burningVFX = vfxObj.GetComponent<ParticleSystem>();

        Debug.Log("burn bitch");
        StartCoroutine(BurningRoutine());
    }
    public void RefreshEffect(float newDamage)
    {
        damagePerSecond = Mathf.Max(damagePerSecond, newDamage * 0.1f);
        duration = 4f; // Reset duration

    }

    private IEnumerator BurningRoutine()
    {
       
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Apply damage every 0.5 seconds
            if (elapsed % 0.5f < Time.deltaTime)
            {
                target.TakeElementalDamage(damagePerSecond * 0.5f, ElementType.Pyro);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Fade out VFX before destroying
        if (burningVFX)
        {
            burningVFX.Stop();
            yield return new WaitForSeconds(2f);
            Destroy(burningVFX.gameObject);
        }

        Destroy(this);
    }
}