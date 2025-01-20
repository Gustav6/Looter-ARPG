using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PatrolState : MoveState
{
    Vector2 randomPos;
    float patrolTimer;
    float distanceFromPos;
    bool findNewPos = true;
    public override void Enter()
    {
        FindNewPosition();
    }

    public override void Do()
    {
        if (findNewPos!)
        {
            if (distanceFromPos <= 0.5f)
            {
                FindNewPosition();
                findNewPos = true;
            }
            else
            {
                distanceFromPos = Vector2.Distance(randomPos, transform.position);
                //moveDirection = (randomPos - transform.position).normalized;
            }
        }

        if (enemyProperties.hasLineOfSight)
        {
            isComplete = true;
            Debug.Log("patrol complete");
        }
    }

    public override void Exit()
    {

    }

    public void FindNewPosition()
    {
        randomPos = Random.insideUnitCircle * enemyProperties.aggroRange;
        distanceFromPos = Vector2.Distance(randomPos, transform.position);
        if (distanceFromPos < 1)
        {
            FindNewPosition();
            return;
        }
        findNewPos = false;
    }
}
