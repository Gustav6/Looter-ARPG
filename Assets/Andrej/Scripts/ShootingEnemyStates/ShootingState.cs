using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingState : State
{
    public GameObject projectile;
    EnemyProperties enemyProperties;

    private float timeBtwShots;
    public float startTimeBtwShots;

    public float distance;

    public void Shoot()
    {
        if (timeBtwShots <= 0)
        {
            Instantiate(projectile, transform.position, Quaternion.identity);
            timeBtwShots = startTimeBtwShots;
        }
        else
        {
            timeBtwShots -= Time.deltaTime;
        }
    }
    public override void Enter()
    {
        Debug.Log("is attacking");
        enemyProperties = GetComponentInParent<EnemyProperties>();

        timeBtwShots = startTimeBtwShots;
    }
    public override void Do()
    {
        if(timeBtwShots  <= 0) 
        { 
            Shoot();
        }
        else
        {
            timeBtwShots -= Time.deltaTime;
        }

        if (enemyProperties.distanceToPlayer > distance)
        {
            isComplete = true;
        }
    }

    public override void Exit()
    {

    }
}
