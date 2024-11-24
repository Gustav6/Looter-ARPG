using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class FireBall : Projectile
{
    public GameObject fireExplosionCiclePrefab;
    private float timer;

    public override void OnHit(RaycastHit2D hit, IDamagable damagable)
    {
        timer = 0;
        GunController.Instance.StartCoroutine(TickDamage(hit.transform, damagable));
        Instantiate(fireExplosionCiclePrefab, transform.position, Quaternion.identity);

        base.OnHit(hit, damagable);
    }
}
