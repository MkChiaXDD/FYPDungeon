using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ChargeBar : MonoBehaviour
{
    [Header("References")]
    public PlayerCombat playerCombat;
    public GameObject BarObj;
    public Slider sliderBar;
    public Image barBorder;

    [Header("Settings")]
    public float BarCharge;
    public float MaxChargeTime = 2f;
    public float MinChargeTime = 0.5f;
    public float showDelay = 0.15f; // Delay before showing bar for quick clicks
    public Color originalBarColor;
    public Color minChargeColor = Color.yellow;
    public Color maxChargeColor = Color.red;

    [Header("Debug")]
    public bool _isCharging;
    public float _chargeTimer;
    private float _chargeStartTime;
    private Coroutine _delayedShowCoroutine;

    private void OnEnable()
    {
        playerCombat.ChargeUp.AddListener(StartChargeUp);
        playerCombat.Uncharge.AddListener(ResetCharge);
    }

    private void OnDisable()
    {
        playerCombat.ChargeUp.RemoveListener(StartChargeUp);
        playerCombat.Uncharge.RemoveListener(ResetCharge);

        // Clean up any running coroutines
        if (_delayedShowCoroutine != null)
        {
            StopCoroutine(_delayedShowCoroutine);
        }
    }

    void Start()
    {
        InitialiseChargeBar();
    }

    public void StartChargeUp()
    {
        _isCharging = true;
        _chargeTimer = 0f;
        _chargeStartTime = Time.time;
        barBorder.color = originalBarColor;

        // Start delayed show coroutine
        _delayedShowCoroutine = StartCoroutine(DelayedShowChargeBar());
    }

    private IEnumerator DelayedShowChargeBar()
    {
        yield return new WaitForSeconds(showDelay);

        // Only show if still charging after delay
        if (_isCharging)
        {
            BarObj.SetActive(true);
        }
    }

    private void Update()
    {
        if (_isCharging)
        {
            _chargeTimer = Time.time - _chargeStartTime;
            sliderBar.value = _chargeTimer;

            // Update colors based on charge level
            if (_chargeTimer >= MinChargeTime && _chargeTimer < MaxChargeTime)
            {
                barBorder.color = minChargeColor;
            }
            else if (_chargeTimer >= MaxChargeTime)
            {
                barBorder.color = maxChargeColor;
            }
        }
    }

    private void ResetCharge()
    {
        _isCharging = false;
        sliderBar.value = 0f;
        _chargeTimer = 0f;
        barBorder.color = originalBarColor;

        // Hide immediately when unchanging
        BarObj.SetActive(false);

        // Cancel any pending show
        if (_delayedShowCoroutine != null)
        {
            StopCoroutine(_delayedShowCoroutine);
        }
    }

    private void InitialiseChargeBar()
    {
        MaxChargeTime = playerCombat._currentmaxChargeTime;
        MinChargeTime = playerCombat._currentminChargeTime;
        sliderBar.maxValue = MaxChargeTime;
        sliderBar.value = 0f;
        BarCharge = 0f;
        originalBarColor = barBorder.color;
        BarObj.SetActive(false);
    }
}