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

    public override void OnHit(RaycastHit2D hit, IDamagable damagable)
    {
        Instantiate(explosionCiclePrefab, transform.position, Quaternion.identity);

        base.OnHit(hit, damagable);
    }
}
