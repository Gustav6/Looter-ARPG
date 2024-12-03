using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingState : State
{
    public GameObject projectile;
    public EnemyProperties enemyProperties;

    private float timeBtwShots;
    public float startTimeBtwShots;
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

        if (enemyProperties.distanceToPlayer > enemyProperties.aggroRange)
        {
            enemyProperties.isAttacking = false;
            isComplete = true;
        }
    }

    public override void Exit()
    {

    }
}
