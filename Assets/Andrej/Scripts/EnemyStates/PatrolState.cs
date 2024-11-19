using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PatrolState : State
{
    public IdleState idle;
    public NavigateState navigate;

    public EnemyProperties enemyProperties;

    public float radius;

    public override void Enter()
    {
        Set(idle);
    }

    public override void Do()
    {
        if (state.isComplete)
        {
            if (state == idle)
            {
                Set(navigate);
            }
            else
            {
                Set(idle);
            }
        }

        if (enemyProperties.distanceToPlayer < radius)
        {
            isComplete = true;
            Debug.Log("patrol complete");
        }
    }

    public override void Exit()
    {

    }

    private void OnDrawGizmos()
    {
        if (!isComplete)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
