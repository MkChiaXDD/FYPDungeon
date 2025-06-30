using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public PlayerData PlayerData;
    public Slider Healthbar;
    public float healthMax;
    public float health;
    // Start is called before the first frame update
    void Start()
    {
        healthMax = PlayerData.MaxHealth;
        health = healthMax;
        Healthbar.maxValue = healthMax;
        Healthbar.value = healthMax;
    }

    // Update is called once per frame
    void Update()
    {
        health = PlayerData.CurrentHealth;
        healthMax = PlayerData.MaxHealth;
        Healthbar.value = health;
    }
}
