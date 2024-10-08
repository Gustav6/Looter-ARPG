using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class NormalBullet : MonoBehaviour
{
    float destroyBulletTimer = 2;
    int amountOfEnemiesHit;
    float angle;
    Rigidbody2D rb;
    CircleCollider2D colider;

    public GameObject explosionCiclePrefab;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        colider = GetComponent<CircleCollider2D>();
        colider.radius = 1;
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
            if (GunController.explosion)
            {
                Instantiate(explosionCiclePrefab, transform.position, Quaternion.identity);
            }
            if (GunController.dmgOverTime)
            {
                //Instantiate(FireExplosionCiclePrefab, transform.position, Quaternion.identity);
            }
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
