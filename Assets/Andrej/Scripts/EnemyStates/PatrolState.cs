using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PatrolState : MoveState
{
    public Transform randomPos;
    float patrolTimer = 3;
    public float distanceFromPos;
    public bool findNewPos = false;

    public LayerMask cantPassThrough;
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
                patrolTimer -= Time.deltaTime;
                if (patrolTimer <= 0)
                {
                    patrolTimer = 3;
                    findNewPos = true;
                    FindNewPosition();
                }
                if (enemyProperties.target != null)
                {
                    enemyProperties.target = null;
                }
            }
            else
            {
                distanceFromPos = Vector2.Distance(randomPos.position, transform.position);
                enemyProperties.target = randomPos;
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
        randomPos.position = (Vector2)transform.position + offset;
        distanceFromPos = Vector2.Distance(randomPos.position, transform.position);
        Vector2 direction = (randomPos.position -= transform.position).normalized;
        RaycastHit2D ray = Physics2D.Raycast(transform.position, direction, distanceFromPos, cantPassThrough);
        if (distanceFromPos < 1 || ray)
        {
            FindNewPosition();
            return;
        }
        findNewPos = false;
    }
}
