using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBall : BigProjectile
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

        if (raycastHit2D != null)
        {
            Instantiate(fireExplosionCiclePrefab, transform.position, Quaternion.identity);
        }
    }
}
