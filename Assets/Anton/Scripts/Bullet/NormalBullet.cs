using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class NormalBullet : Projectile
{
    public GameObject explosionCiclePrefab;
    RaycastHit2D hit;
    public LayerMask collidableLayers;
    Vector2 dir;
    public override void Start()
    {
        
    }

    public override void Update()
    {
        if (Input.GetMouseButton(0))
        {
            dir = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            hit = Physics2D.Raycast(transform.position, dir, 20f, collidableLayers, 1f, 10f);
            Debug.DrawRay(transform.position, dir, Color.red);
            Debug.Log("RayCast");
        }
        if (hit)
        {
            hit.transform.GetComponent<AntonsTemporaryEnemyScript>().TakeDamage(GunController.Damage);
        }
        // amountOfEnemiesHit += 1;
        //if (GunController.explosion)
        //{
        //   Instantiate(explosionCiclePrefab, transform.position, Quaternion.identity);
        //}

        //if (GunController.pierce)
        //{
        //   if (amountOfEnemiesHit >= GunController.pierceAmount)
        //   {
        //      Destroy(gameObject);
        //   }    
        //} 
        //else { Destroy(gameObject); }

    }      
}
