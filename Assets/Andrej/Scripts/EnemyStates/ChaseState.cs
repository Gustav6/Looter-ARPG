using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Hierarchy;
using Unity.VisualScripting;
using UnityEngine;

public class ChaseState : State
{
    public EnemyProperties enemyProperties;
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

        if (enemyProperties.distanceToPlayer > enemyProperties.aggroRange)
        {
            isComplete = true;
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
