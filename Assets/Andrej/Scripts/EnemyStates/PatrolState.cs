using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PatrolState : State
{
    EnemyProperties enemyProperties;

    public IdleState idle;
    public NavigateState navigate;

    public override void Enter()
    {
        Set(idle);
    }

    public override void Do()
    {
        if (state.time < 2)
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
    }

    public override void Exit()
    {

    }
}
