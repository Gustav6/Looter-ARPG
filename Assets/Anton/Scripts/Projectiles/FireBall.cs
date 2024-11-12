using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBall : Projectile
{
    public GameObject fireExplosionCiclePrefab;
    public override void Start()
    {
        base.Start();
    }
    public override void Update()
    {
        base.Update();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (raycastHit2D)
        {
            Instantiate(fireExplosionCiclePrefab, transform.position, Quaternion.identity);
        }
    }
}
