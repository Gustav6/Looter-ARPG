using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : State
{
    EnemyProperties enemyProperties;
    public float distance = 10;
    public float attackRange = 0.1f;

    public void Chase()
    {
        if (Vector2.Distance(transform.position, enemyProperties.player.transform.position) > attackRange)
        {
            enemyProperties.transform.position = Vector2.MoveTowards(transform.position, enemyProperties.player.transform.position, enemyProperties.speed * Time.deltaTime);
        }
        else
        {
            isComplete = true;
        }
    }
    public override void Enter()
    {
        enemyProperties = GetComponentInParent<EnemyProperties>();
    }
    public override void Do()
    {

        Chase();
        if (enemyProperties.distanceToPlayer > distance)
        {
            isComplete = true;
            Debug.Log("chase complete");
        }
    }

    public override void Exit()
    {

    }
}
