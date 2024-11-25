using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Hierarchy;
using Unity.VisualScripting;
using UnityEngine;

public class ChaseState : State
{
    public EnemyProperties enemyProperties;
    public float exitRange = 12;
    public float attackRange;
    public void Chase()
    {
        enemyProperties.transform.position = Vector2.MoveTowards(transform.position, enemyProperties.player.transform.position, enemyProperties.speed * Time.deltaTime);
    }
    public override void Enter()
    {

    }
    public override void Do()
    {
        Chase();

        if (enemyProperties.distanceToPlayer > exitRange)
        {
            isComplete = true;
        }

        if(attackRange > enemyProperties.distanceToPlayer && time < 0.5)
        {
            enemyProperties.isAttacking = true;
            isComplete = true;
        }
    }

    public override void Exit()
    {

    }

    private void OnDrawGizmos()
    {
        if(!isComplete)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, exitRange);
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
