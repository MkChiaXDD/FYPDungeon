using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class CooldownSystem : MonoBehaviour
{
    [Header("Cooldown Settings")]
    [Tooltip("The duration of the cooldown in seconds")]
    [SerializeField] private float cooldownDuration = 3f;

    [Header("Visual Settings")]
    [Tooltip("If true, the cooldown will start automatically")]
    [SerializeField] private bool startOnAwake = false;

    [Tooltip("Optional text to display remaining time")]
    [SerializeField] private Text cooldownText;

    private Slider cooldownSlider;
    private float currentCooldownTime;
    private bool isOnCooldown;

    // Public property to check cooldown status
    public bool IsOnCooldown => isOnCooldown;

    // Public property to get remaining time
    public float RemainingTime => currentCooldownTime;

    // Events for cooldown start and finish
    public System.Action OnCooldownStarted;
    public System.Action OnCooldownFinished;

    private void Awake()
    {
        cooldownSlider = GetComponent<Slider>();
        cooldownSlider.minValue = 0f;
        cooldownSlider.maxValue = 1f;
        cooldownSlider.value = 1f;

        if (startOnAwake)
        {
            StartCooldown();
        }
    }

    private void Update()
    {
        if (!isOnCooldown) return;

        // Update cooldown time
        currentCooldownTime -= Time.deltaTime;

        // Update slider value (normalized from 1 to 0)
        cooldownSlider.value = Mathf.Clamp01(currentCooldownTime / cooldownDuration);

        // Update text if available
        if (cooldownText != null)
        {
            cooldownText.text = Mathf.Ceil(currentCooldownTime).ToString();
        }

        // Check if cooldown is finished
        if (currentCooldownTime <= 0f)
        {
            FinishCooldown();
        }
    }

    /// <summary>
    /// Starts the cooldown with the specified duration
    /// </summary>
    public void StartCooldown()
    {
        StartCooldown(cooldownDuration);
    }

    /// <summary>
    /// Starts the cooldown with a custom duration
    /// </summary>
    /// <param name="duration">Duration in seconds</param>
    public void StartCooldown(float duration)
    {
        if (isOnCooldown) return;

        cooldownDuration = Mathf.Max(0.01f, duration);
        currentCooldownTime = cooldownDuration;
        isOnCooldown = true;
        cooldownSlider.value = 1f;

        OnCooldownStarted?.Invoke();
    }

    /// <summary>
    /// Stops the cooldown prematurely
    /// </summary>
    public void StopCooldown()
    {
        if (!isOnCooldown) return;

        FinishCooldown();
    }

    private void FinishCooldown()
    {
        isOnCooldown = false;
        currentCooldownTime = 0f;
        cooldownSlider.value = 0f;

        if (cooldownText != null)
        {
            cooldownText.text = "0";
        }

        OnCooldownFinished?.Invoke();
    }

    /// <summary>
    /// Changes the cooldown duration (doesn't affect current cooldown)
    /// </summary>
    /// <param name="newDuration">New duration in seconds</param>
    public void SetCooldownDuration(float newDuration)
    {
        cooldownDuration = Mathf.Max(0.01f, newDuration);
    }
}