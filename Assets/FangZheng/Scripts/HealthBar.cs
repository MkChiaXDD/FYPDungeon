using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class HealthBar : MonoBehaviour
{
    [Header("References")]
    public PlayerData PlayerData;
    [SerializeField] private Material healthAmountMaterial;
    [SerializeField] private TMP_Text healthText;

    [Header("Settings")]
    [SerializeField] private float animationSpeed = 1f;

    private float targetFillAmount;
    private Coroutine healthAnimation;


    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        // Reset when enabled (better than OnDestroy for object pooling)
        Initialize();
    }

    private void Update()
    {
        if (healthAmountMaterial.GetFloat("_AmountOfLiquid") != PlayerData.CurrentHealth / PlayerData.MaxHealth)
        {
            UpdateHealthDisplay();
        }
    }

    private void Initialize()
    {
        if (PlayerData != null)
        {
            healthText.text = $"{PlayerData.CurrentHealth} / {PlayerData.MaxHealth}";
            targetFillAmount = PlayerData.CurrentHealth / PlayerData.MaxHealth;
            healthAmountMaterial.SetFloat("_AmountOfLiquid", targetFillAmount);
        }
    }

    private void UpdateHealthDisplay()
    {
        if (PlayerData == null) return;

        // Update text display
        healthText.text = $"{PlayerData.CurrentHealth} / {PlayerData.MaxHealth}";

        // Calculate new target
        float newTarget = PlayerData.CurrentHealth / PlayerData.MaxHealth;

        if (!Mathf.Approximately(newTarget, targetFillAmount))
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

    private IEnumerator AnimateHealthBar()
    {
        float currentFill = healthAmountMaterial.GetFloat("_AmountOfLiquid");
        float tolerance = 0.001f; // Small value to prevent unnecessary iterations

        while (Mathf.Abs(currentFill - targetFillAmount) > tolerance)
        {
            currentFill = Mathf.MoveTowards(
                currentFill,
                targetFillAmount,
                animationSpeed * Time.deltaTime
            );

            healthAmountMaterial.SetFloat("_AmountOfLiquid", currentFill);
            yield return null;
        }

        // Ensure final value is set exactly
        healthAmountMaterial.SetFloat("_AmountOfLiquid", targetFillAmount);
        healthAnimation = null;
    }

}