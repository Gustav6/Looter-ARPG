using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : State
{
    EnemyProperties enemyProperties;
    public override void Enter() 
    {
        isComplete = true;
        Debug.Log("attacking");
    }
    public override void Do() 
    {

    }

    public override void Exit() 
    { 
    
    }
}
