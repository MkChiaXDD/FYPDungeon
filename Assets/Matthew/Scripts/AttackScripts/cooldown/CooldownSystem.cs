using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Image))]
public class CooldownSystem : MonoBehaviour
{
    [Header("Cooldown Settings")]
    [SerializeField] private float cooldownDuration = 1f;
    [SerializeField] private bool startOnAwake = false;
    [SerializeField] private bool reverseFill = true;

    [Header("Visual Settings")]
    [SerializeField] private Text cooldownText;
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private GameObject cooldownFinishedEffect;

    private Image cooldownImage;
    private float currentCooldownTime;
    private bool isOnCooldown;

    // Public properties
    public bool IsOnCooldown => isOnCooldown;
    public float RemainingTime => currentCooldownTime;
    public float Progress => 1f - (currentCooldownTime / cooldownDuration);

    // Events
    public event Action OnCooldownStarted;
    public event Action OnCooldownFinished;

    private void Awake()
    {
        cooldownImage = GetComponent<Image>();

        if (cooldownOverlay == null)
        {
            // If no overlay specified, use self
            cooldownOverlay = cooldownImage;
        }

        cooldownOverlay.type = Image.Type.Filled;
        cooldownOverlay.fillMethod = Image.FillMethod.Radial360;
        cooldownOverlay.fillOrigin = (int)Image.Origin360.Top;
        cooldownOverlay.fillClockwise = false;

        ResetCooldownVisual();

        if (startOnAwake)
        {
            StartCooldown();
        }
    }

    private void Update()
    {
        if (!isOnCooldown) return;

        currentCooldownTime -= Time.deltaTime;
        UpdateCooldownVisual();

        if (currentCooldownTime <= 0f)
        {
            FinishCooldown();
        }
    }

    /// <summary>
    /// Starts the cooldown with default duration
    /// </summary>
    public void StartCooldown()
    {
        StartCooldown(cooldownDuration);
    }

    /// <summary>
    /// Starts the cooldown with custom duration
    /// </summary>
    public void StartCooldown(float duration)
    {
        if (isOnCooldown) return;

        cooldownDuration = Mathf.Max(0.01f, duration);
        currentCooldownTime = cooldownDuration;
        isOnCooldown = true;

        ResetCooldownVisual();
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
        cooldownOverlay.fillAmount = reverseFill ? 0f : 1f;

        if (cooldownText != null)
        {
            cooldownText.text = "0";
        }

        if (cooldownFinishedEffect != null)
        {
            Instantiate(cooldownFinishedEffect, transform.position, Quaternion.identity, transform);
        }

        OnCooldownFinished?.Invoke();
    }

    private void UpdateCooldownVisual()
    {
        float progress = currentCooldownTime / cooldownDuration;
        cooldownOverlay.fillAmount = reverseFill ? 1f - progress : progress;

        if (cooldownText != null)
        {
            cooldownText.text = Mathf.Ceil(currentCooldownTime).ToString();
        }
    }

    private void ResetCooldownVisual()
    {
        cooldownOverlay.fillAmount = reverseFill ? 1f : 0f;

        if (cooldownText != null)
        {
            cooldownText.text = Mathf.Ceil(cooldownDuration).ToString();
        }
    }

    /// <summary>
    /// Changes the cooldown duration (doesn't affect current cooldown)
    /// </summary>
    public void SetCooldownDuration(float newDuration)
    {
        cooldownDuration = Mathf.Max(0.01f, newDuration);
    }

    /// <summary>
    /// Changes the fill method for the cooldown visual
    /// </summary>
    public void SetFillMethod(Image.FillMethod method, int origin = 0, bool clockwise = false)
    {
        cooldownOverlay.fillMethod = method;
        cooldownOverlay.fillOrigin = origin;
        cooldownOverlay.fillClockwise = clockwise;
    }
}