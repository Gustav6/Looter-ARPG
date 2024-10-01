using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "PowerUps/SpeedBuffs")]
public class SpeedBuff : PowerUpEffects
{
    public float amount;
    public override void Apply(GameObject target)
    {
        target.GetComponent<Player>().maxStamina += amount;
        target.GetComponent<SpriteRenderer>().color = Color.yellow;
    }
}
