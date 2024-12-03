using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : BigProjectile
{
    public GameObject explosionCiclePrefab;
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
            CameraShake.ShakeCamera(4);
            Instantiate(explosionCiclePrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
