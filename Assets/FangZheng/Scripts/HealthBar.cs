using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider Healthbar;
    public float healthMax;
    public float health;
    // Start is called before the first frame update
    void Start()
    {
        healthMax = GetComponent<PlayerController>().GetHealth();
        health = healthMax;
        Healthbar.maxValue = healthMax;
        Healthbar.value = healthMax;
    }

    // Update is called once per frame
    void Update()
    {
        health = GetComponent<PlayerController>().GetHealth();
        Healthbar.value = health;
    }
}
