using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Hierarchy;
using Unity.VisualScripting;
using UnityEngine;

public class ChaseState : MoveState
{
    public Vector2? lastSeenLocation;
    public bool checkedLastLocation = false;

    private float distanceFromLastLocation;
    public void Chase()
    {
        if (enemyProperties.hasLineOfSight)
        {
            moveDirection = (enemyProperties.player.transform.position - transform.position).normalized;
        }
        else
        {
            if (!checkedLastLocation && lastSeenLocation != null)
            {
                moveDirection = (lastSeenLocation.Value - (Vector2)transform.position).normalized;
            }
        }

        Debug.Log(moveDirection);
    }
    public override void Enter()
    {
        base.Enter();
    }
    public override void Do()
    {
        Chase();

        LineOfSightCheck();

        if(enemyProperties.attackRange > enemyProperties.distanceToPlayer && time > 0.5)
        {
            enemyProperties.isAttacking = true;
            isComplete = true;
        }

        base.Do();
    }

    public void LineOfSightCheck()
    {
        if (enemyProperties.hasLineOfSight)
        {
            lastSeenLocation = null;

            return;
        }
        else if (lastSeenLocation != null)
        {
            distanceFromLastLocation = Vector3.Distance(transform.position, lastSeenLocation.Value);

            if (distanceFromLastLocation < 0.05)
            {
                moveDirection = Vector2.zero;

                checkedLastLocation = true;
                lastSeenLocation = null;
                isComplete = true;
            }

            return;
        }

        lastSeenLocation = enemyProperties.player.transform.position;
        checkedLastLocation = false;
    }

    public override void Exit()
    {
        base.Exit();
    }
}
