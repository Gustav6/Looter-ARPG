using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalBullet : MonoBehaviour
{
    float destroyBulletTimer = 2;
    int amountOfEnemiesHit;
    void Update()
    {
        destroyBulletTimer -= Time.deltaTime;

        if (destroyBulletTimer <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Enemy")
        {
            amountOfEnemiesHit += 1;
            if (GunController.pierce)
            {
                if (amountOfEnemiesHit >= GunController.pierceAmount)
                {
                    Destroy(gameObject);
                }
            }
            else {Destroy(gameObject);}
        }
    }
}
