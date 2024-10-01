using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    public PowerUpEffects PowerUpEffects;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Destroy(gameObject);
        PowerUpEffects.Apply(collision.gameObject);
    }

}
