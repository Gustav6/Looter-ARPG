using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Hierarchy;
using Unity.VisualScripting;
using UnityEngine;

public class ChaseState : State
{
    public EnemyProperties enemyProperties;
    public Vector2 lastSeenLocation;
    public bool checkedLastLocation = false;

    private float distanceFromLastLocation;
    public void Chase()
    {
        if (!enemyProperties.hasLineOfSight)
        {
            enemyProperties.transform.position = Vector2.MoveTowards(transform.position, lastSeenLocation, enemyProperties.speed * Time.deltaTime);
        }
        else
        {
            enemyProperties.transform.position = Vector2.MoveTowards(transform.position, enemyProperties.player.transform.position, enemyProperties.speed * Time.deltaTime);
        }
    }
    public override void Enter()
    {

    }
    public override void Do()
    {
        Chase();

        if (!enemyProperties.hasLineOfSight)
        {
            distanceFromLastLocation = Vector3.Distance(transform.position, lastSeenLocation);
            if (!checkedLastLocation)
            {
                lastSeenLocation = enemyProperties.player.transform.position;
                checkedLastLocation = true;
            }
            if (distanceFromLastLocation < 0.05)
            {
                isComplete = true;
            }
        }
        else
        {
            checkedLastLocation = false;
        }

        if(enemyProperties.attackRange > enemyProperties.distanceToPlayer && time > 0.5)
        {
            enemyProperties.isAttacking = true;
            isComplete = true;
        }
    }

    public override void Exit()
    {

    }
}
