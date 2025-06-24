using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class DynamicHealthBar : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;

    [Header("Animation Settings")]
    [SerializeField] private float smoothTime = 0.3f;
    [SerializeField] private float maxSmoothSpeed = 50f;

    private Slider healthSlider;
    private float currentVelocity;
    private float targetHealth;

    private void Awake()
    {
        healthSlider = GetComponent<Slider>();
        InitializeHealthBar();
    }

    private void InitializeHealthBar()
    {
        healthSlider.maxValue = maxHealth;
        healthSlider.value = maxHealth;
        targetHealth = maxHealth;
    }

    private void Update()
    {
        if (Mathf.Abs(healthSlider.value - targetHealth) > 0.01f)
        {
            // Smoothly interpolate towards the target health
            healthSlider.value = Mathf.SmoothDamp(
                current: healthSlider.value,
                target: targetHealth,
                currentVelocity: ref currentVelocity,
                smoothTime: smoothTime,
                maxSpeed: maxSmoothSpeed
            );
        }
        else
        {
            // Snap to target when close enough
            healthSlider.value = targetHealth;
            currentVelocity = 0f;
        }
    }

    public void SetHealth(float health)
    {
        targetHealth = Mathf.Clamp(health, 0f, maxHealth);
        currentHealth = health;
    }

    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        healthSlider.maxValue = maxHealth;
        targetHealth = Mathf.Clamp(targetHealth, 0f, maxHealth);
    }

    // For testing in the editor
    [ContextMenu("Test Damage (25)")]
    private void TestDamage()
    {
        SetHealth(currentHealth - 25f);
    }

    [ContextMenu("Test Heal (15)")]
    private void TestHeal()
    {
        SetHealth(currentHealth + 15f);
    }
}