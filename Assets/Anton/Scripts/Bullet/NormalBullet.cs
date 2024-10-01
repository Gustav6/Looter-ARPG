using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class NormalBullet : MonoBehaviour
{
    float destroyBulletTimer = 2;
    int amountOfEnemiesHit;
    float angle;
    Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        destroyBulletTimer -= Time.deltaTime;

        if (destroyBulletTimer <= 0)
        {
            Destroy(gameObject);
        }

        Vector2 v = rb.velocity;
        angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
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
