using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour

{
    public FloatReference playerHealth, playerMaxHealth;
    Slider slider;

    private void Start()
    {
        slider = GetComponent<Slider>();
    }

    private void Update()
    {
        float fillValue = playerHealth.Value / playerMaxHealth.Value;
        slider.value = fillValue;
    }

}
