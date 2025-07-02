// Example elemental effect component
using System.Collections;
using UnityEngine;

public class BurningEffect : MonoBehaviour
{
    private float damagePerSecond;
    private float duration;
    private Enemy enemy;
    private ParticleSystem ElectroVFX;

    public void Initialize(float baseDamage, Enemy targetEnemy)
    {        
        enemy = targetEnemy;
        damagePerSecond = baseDamage * 1f; // 10% of initial damage per second
        duration = 4f;

        // Create VFX
        GameObject vfxObj = Instantiate(ElementalReactionManager.Instance.ElectricVFX, transform);
        ElectroVFX = vfxObj.GetComponent<ParticleSystem>();

        Debug.Log("electro attack ");
        StartCoroutine(BurningRoutine());
    }
    public void RefreshEffect(float newDamage)
    {
        damagePerSecond = Mathf.Max(damagePerSecond, newDamage * 0.1f);
        duration = 4f; // Reset duration

    }

    private IEnumerator BurningRoutine()
    {
        Debug.Log("electrocuted bitcheres");
        float elapsed = 0f;

        enemy.ApplyStun(elapsed);

        while (elapsed < duration)
        {
            // Apply damage every 0.5 seconds
            if (elapsed % 0.5f < Time.deltaTime)
            {
                enemy.TakeElementalDamage(damagePerSecond * 0.5f, ElementType.Electro);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Fade out VFX before destroying
        if (ElectroVFX)
        {
            ElectroVFX.Stop();
            yield return new WaitForSeconds(2f);
            Destroy(ElectroVFX.gameObject);
        }



        Destroy(this);
    }
}