using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBall : Projectile
{
    public GameObject FireExplosionCiclePrefab;
    public override void Start()
    {

    }
    public override void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Enemy")
        {          
            Instantiate(FireExplosionCiclePrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);  
        }     
    }
}
