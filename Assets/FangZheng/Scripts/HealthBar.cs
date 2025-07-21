using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthBar : MonoBehaviour
{
    public PlayerData PlayerData;
    public Slider Healthbar;
    public float healthMax;
    public float health;
    [SerializeField] private Material healthAmountMaterial;

    private float targetFillAmount;  // Target value for smooth animation
    private Coroutine healthAnimation;  // Reference to active animation coroutine
    public float animationSpeed = 1f;  // Speed of health bar animation

    void Start()
    {
        Initialise();
    }

    private void OnDestroy()
    {
        Initialise(); //call again so th
    }

    void Update()
    {
        // Update target values from PlayerData
        health = PlayerData.CurrentHealth;
        healthMax = PlayerData.MaxHealth;

        // Calculate new target fill amount
        float newTarget = Mathf.Clamp01(health / healthMax);

        // Start animation if target changed
        if (newTarget != targetFillAmount)
        {
            targetFillAmount = newTarget;

            // Stop existing animation if running
            if (healthAnimation != null)
            {
                StopCoroutine(healthAnimation);
            }

            // Start new animation
            healthAnimation = StartCoroutine(AnimateHealthBar());
        }
    }

    IEnumerator AnimateHealthBar()
    {
        // Get initial fill amount from material
        float currentFill = healthAmountMaterial.GetFloat("_AmountOfLiquid");

        // Animate until we reach target value
        while (currentFill != targetFillAmount)
        {
            // Smoothly interpolate towards target
            currentFill = Mathf.MoveTowards(
                currentFill,
                targetFillAmount,
                animationSpeed * Time.deltaTime
            );

            // Update material property
            healthAmountMaterial.SetFloat("_AmountOfLiquid", currentFill);

            // Wait until next frame
            yield return null;
        }
    }

    private void Initialise()
    {
        health = healthMax;
        targetFillAmount = healthMax / healthMax;
        healthAmountMaterial.SetFloat("_AmountOfLiquid", healthMax / healthMax);
    }
}