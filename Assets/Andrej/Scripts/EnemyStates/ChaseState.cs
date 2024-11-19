using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEngine;

public class ChaseState : State
{
    public EnemyProperties enemyProperties;
    public float radius = 12;
    public void Chase()
    {
        enemyProperties.transform.position = Vector2.MoveTowards(transform.position, enemyProperties.player.transform.position, enemyProperties.speed * Time.deltaTime);
    }
    public override void Enter()
    {

    }
    public override void Do()
    {
        if (!enemyProperties.isHit)
        {
            Chase();
        }
        else
        {
            isComplete = true;
        }

        if (enemyProperties.distanceToPlayer > radius)
        {
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
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
