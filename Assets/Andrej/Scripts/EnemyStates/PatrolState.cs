using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PatrolState : MoveState
{
    public override void Enter()
    {
        //InsideUnitCircle
    }

    public override void Do()
    {

        if (enemyProperties.hasLineOfSight)
        {
            isComplete = true;
            Debug.Log("patrol complete");
        }
    }

    public override void Exit()
    {

    }
}
