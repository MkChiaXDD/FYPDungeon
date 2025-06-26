using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class DynamicHealthBar : MonoBehaviour
{
    

    [Header("Animation Settings")]
    [SerializeField] private float smoothTime = 1f;
    [SerializeField] private float maxSmoothSpeed = 500f;

    private Slider healthSlider;
    private float currentVelocity;
    [SerializeField] private float currentHealth;

    

    private void Start()
    {
        healthSlider = GetComponent<Slider>();
    }

    private void Update()
    {
        if (healthSlider == null)
        {
            return;
        }
        if (Mathf.Abs(healthSlider.value - currentHealth) > 0.01f)
        {
            healthSlider.value = Mathf.SmoothDamp(
                current: healthSlider.value,
                target: currentHealth,
                currentVelocity: ref currentVelocity,
                smoothTime: smoothTime,
                maxSpeed: maxSmoothSpeed
            );
        }
        else
        {
            healthSlider.value = currentHealth;
            currentVelocity = 0f;
        }
    }

    public void SetHealth(float health)
    {
        currentHealth = health; // Sync currentHealth with clamped value
        Debug.Log("health value set at " + currentHealth);
    }

    public void SetMaxHealth(float newMaxHealth)
    {

        GetComponent<Slider>().maxValue = newMaxHealth;
        GetComponent<Slider>().value = newMaxHealth;
        Debug.Log("healthslider max value set at " + healthSlider.maxValue);
        Debug.Log("healthslider base value set at " + healthSlider.value);
    }


}