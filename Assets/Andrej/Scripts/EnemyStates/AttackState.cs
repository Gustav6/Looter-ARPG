using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : State
{
    public override void Enter() 
    {
        Debug.Log("is attacking");
        isComplete = true;
    }
    public override void Do() 
    {

    }

    public override void Exit() 
    { 
    
    }
}
