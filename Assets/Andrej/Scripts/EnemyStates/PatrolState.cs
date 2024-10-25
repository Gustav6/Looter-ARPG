using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PatrolState : State
{
    EnemyProperties enemyProperties;

    public float distance = 10;
    private float patrolSpeed;
    public float waitTime;
    public float startWaitTime;

    private int randomSpot;
    public Transform[] moveSpots;

    public void GoToRandomSpot()
    {
        transform.parent.position = Vector2.MoveTowards(transform.position, moveSpots[randomSpot].position, patrolSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, moveSpots[randomSpot].position) < 0.2f)
        {
            if(waitTime <= 0)
            {
                randomSpot = Random.Range(0, moveSpots.Length);
                waitTime = startWaitTime;
            }
            else
            {
                waitTime -= Time.deltaTime;
            }
        }
    }


    public override void Enter()
    {
        enemyProperties = GetComponentInParent<EnemyProperties>();
        patrolSpeed = enemyProperties.speed / 2;

        randomSpot = Random.Range(0, moveSpots.Length);
    }

    public override void Do()
    {
        GoToRandomSpot();
        if (enemyProperties.distanceToPlayer < distance)
        {
            isComplete = true;
            Debug.Log("patrol complete");
        }
    }

    public override void Exit()
    {

    }
}
