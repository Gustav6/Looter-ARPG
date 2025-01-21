using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PatrolState : MoveState
{
    Vector2 randomPos;
    float patrolTimer = 3;
    public float distanceFromPos;
    public bool findNewPos = false;
    public override void Enter()
    {
        FindNewPosition();
    }

    public override void Do()
    {
        if (!findNewPos)
        {
            if (distanceFromPos <= 0.5f)
            {
                moveDirection = Vector2.zero;
                patrolTimer -= Time.deltaTime;
                if (patrolTimer <= 0)
                {
                    patrolTimer = 3;
                    findNewPos = true;
                    FindNewPosition();
                }
            }
            else
            {
                distanceFromPos = Vector2.Distance(randomPos, transform.position);
                moveDirection = (randomPos - (Vector2)transform.position).normalized;
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
        Vector2 offset = Random.insideUnitCircle * enemyProperties.aggroRange;
        randomPos = (Vector2)transform.position + offset;
        distanceFromPos = Vector2.Distance(randomPos, transform.position);
        if (distanceFromPos < 1)
        {
            FindNewPosition();
            return;
        }
        findNewPos = false;
    }
}
