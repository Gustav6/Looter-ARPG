using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


[CreateAssetMenu(menuName = "PowerUps/HealthBuffs")]
public class HealthBuff : PowerUpEffects
{

    public float amount;

    public override void Apply(GameObject target)
    {
        target.GetComponent<PlayerHealthScript>().health.value += amount;
    }
}
