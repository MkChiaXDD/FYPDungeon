using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class HealthBar : MonoBehaviour
{
    public PlayerData PlayerData;
    public float maxHealth;
    public float health;
    [SerializeField] private Material healthAmountMaterial;
    [SerializeField] private TMP_Text HealthText;

    private float targetFillAmount;  // Target value for smooth animation
    private Coroutine healthAnimation;  // Reference to active animation coroutine
    public float animationSpeed = 1f;  // Speed of health bar animation

    private void Awake()
    {
        Initialise();
    }


    private void OnDestroy()
    {
        Initialise(); //call again so th
    }

    void Update()
    {
        UpdatePlayerHealth();

        float newTarget = health / maxHealth;
        if (newTarget != targetFillAmount)
        {
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
        health = maxHealth;
        targetFillAmount = maxHealth / maxHealth;
        healthAmountMaterial.SetFloat("_AmountOfLiquid", maxHealth / maxHealth);

    }

    private void UpdatePlayerHealth()
    {
        // Update target values from PlayerData
        health = PlayerData.CurrentHealth;
        maxHealth = PlayerData.MaxHealth;
        HealthText.text = health + " / " + maxHealth;
    }
}