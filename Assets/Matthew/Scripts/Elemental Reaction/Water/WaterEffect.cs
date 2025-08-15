// Example elemental effect component
using System.Collections;
using UnityEngine;

//NOT IN USE
public class WaterEffect : MonoBehaviour
{
    private float damagePerSecond;
    public float duration = 4f;
    private Enemy enemy;
    private ParticleSystem WaterVFX;

    public void Initialize(float baseDamage, Enemy targetEnemy)
    {
        enemy = targetEnemy;
        damagePerSecond = baseDamage * 1f; // 10% of initial damage per second
        duration = 4f;


        // Create VFX
        GameObject vfxObj = Instantiate(ElementalReactionManager.Instance.WaterVFX, transform);
        WaterVFX = vfxObj.GetComponent<ParticleSystem>();

        Debug.Log("electro attack ");
        StartCoroutine(ElectricRoutine());
    }
    public void RefreshEffect(float newDamage)
    {
        damagePerSecond = Mathf.Max(damagePerSecond, newDamage * 0.1f);
        duration = 4f; // Reset duration

    }

    private IEnumerator ElectricRoutine()
    {

        float elapsed = 0f;
        while (elapsed < duration)
        {
            // Apply damage every 0.5 seconds
            if (elapsed % 0.5f < Time.deltaTime)
            {
                enemy.TakeElementalDamage(damagePerSecond * 0.5f, ElementType.Hydro);
                Debug.Log("apply water");
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Fade out VFX before destroying
        if (WaterVFX)
        {
            WaterVFX.Stop();
            yield return new WaitForSeconds(2f);
            Destroy(WaterVFX.gameObject);
        }
        Destroy(this);
    }
}