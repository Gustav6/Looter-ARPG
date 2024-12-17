using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class FireBall : Projectile
{
    public GameObject fireExplosionCiclePrefab;
    public override void OnHit(RaycastHit2D hit, IDamagable damagable)
    {
        Instantiate(fireExplosionCiclePrefab, transform.position, Quaternion.identity);

        base.OnHit(hit, damagable);
    }
}
