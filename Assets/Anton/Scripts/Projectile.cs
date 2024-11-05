using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

public class Projectile : MonoBehaviour
{
    float destroyBulletTimer = 2;
    public Rigidbody2D rb;
    CircleCollider2D colider;
    float angle; 
    [SerializeField] public int amountOfEnemiesHit; 

    public virtual void Start()
    {
        colider.radius = 0.5f;
        rb = GetComponent<Rigidbody2D>();
        colider = GetComponent<CircleCollider2D>();
    }

    public virtual void Update()
    {
        destroyBulletTimer -= Time.deltaTime;

        if (destroyBulletTimer <= 0)
        {
            Destroy(gameObject);
        }

        Vector2 v = rb.linearVelocity;
        angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
}
