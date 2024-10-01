using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Staminabar : MonoBehaviour
{
    public Slider slider;

    public void SetMaxHealth(int stamina)
    {
        slider.maxValue = stamina;
    }

    public void SetHealth(int stamina)
    {
        slider.value = stamina;

    }
}
