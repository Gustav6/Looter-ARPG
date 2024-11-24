using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class NormalBullet : Projectile
{
    public override void Start()
    {
        base.Start();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public override void Update()
    {

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

        base.Update();
    }      
}
