    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

public class ChargeBar : MonoBehaviour
{
    public PlayerCombat playerCombat;
    public GameObject BarObj;
    public Slider Bar;
    public Image BarBorder;
    public float BarCharge;
    public float BarChargeMax = 2f;
    public float BarCanHeavey = 0.5f;
    public Color NormalBarColor;

    public bool _IsCharge;
    public float _StartChargeTimer;
    private void OnEnable()
    {
        playerCombat.ChargingUp.AddListener(StartChargeUp);
        playerCombat.UnCharge.AddListener(End);
    }
    // Start is called before the first frame update

    
    void Start()
    {
        BarChargeMax = playerCombat._maxChargeTime;
        BarCanHeavey = playerCombat._minChargeTime;

        Bar.maxValue = BarChargeMax;
        Bar.value = 0;
        BarCharge = 0;
        NormalBarColor = BarBorder.color;
        BarObj.gameObject.SetActive(false);
    }

    public void StartChargeUp()
    {
        BarObj.gameObject.SetActive(true);
        _IsCharge = true;
        _StartChargeTimer = 0;
        BarBorder.color = NormalBarColor;

    }

    private void Update()
    {
        if (_IsCharge == true)
        {
            _StartChargeTimer += Time.deltaTime;
            Bar.value = _StartChargeTimer;
            if (_StartChargeTimer >= BarCanHeavey)
            {
                BarBorder.color = Color.yellow;
            }

            if (_StartChargeTimer >= BarChargeMax)
            {
                BarBorder.color = Color.red;
            }
        }


    }

    private void End()
    {
        _IsCharge = false;
        Bar.value = 0;
        _StartChargeTimer = 0;
        BarBorder.color= NormalBarColor;
        BarObj.SetActive(false);
    }

}
