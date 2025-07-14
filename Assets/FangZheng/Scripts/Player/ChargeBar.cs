using UnityEngine;
using UnityEngine.UI;

public class ChargeBar : MonoBehaviour
{
    public PlayerCombat playerCombat;
    public GameObject BarObj;
    public Slider sliderBar;
    public Image barBorder;
    public float BarCharge;
    public float MaxChargeTime = 2f;
    public float MinChargeTime = 0.5f;
    public Color originalBarColor;

    public bool _isCharging;
    public float _chargeTimer;
    private void OnEnable()
    {
        playerCombat.ChargeUp.AddListener(StartChargeUp);
        playerCombat.Uncharge.AddListener(ResetCharge);
    }

    // Start is called before the first frame update
    void Start()
    {
        InitialiseChargeBar();
    }

    public void StartChargeUp()
    {
        BarObj.SetActive(true);
        _isCharging = true;
        _chargeTimer = 0;
        barBorder.color = originalBarColor;
    }

    private void Update()
    {
        if (_isCharging == true)
        {
            _chargeTimer += Time.deltaTime;
            sliderBar.value = _chargeTimer;

            if (_chargeTimer >= MinChargeTime)
            {
                barBorder.color = Color.yellow;
            }

            if (_chargeTimer >= MaxChargeTime)
            {
                barBorder.color = Color.red;
            }
        }
    }

    private void ResetCharge()
    {
        _isCharging = false;
        sliderBar.value = 0;
        _chargeTimer = 0;
        barBorder.color = originalBarColor;
        BarObj.SetActive(false);
    }

    private void InitialiseChargeBar()
    {
        MaxChargeTime = playerCombat._maxChargeTime;
        MinChargeTime = playerCombat._minChargeTime;
        sliderBar.maxValue = MaxChargeTime;
        sliderBar.value = 0;
        BarCharge = 0;
        originalBarColor = barBorder.color;
        BarObj.SetActive(false);
    }
}
