using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : Core
{
    public EnemyProperties enemyProperties;

    public PatrolState patrolState;
    public ChaseState chaseState;
    public AttackState attackState;

    public void Awake()
    {
        AttachScripts();
    }

    public void AttachScripts()
    {
        machine = new StateMachine();
    }
    
    public void Start()
    {
        SetupInstances();
        Set(patrolState);
    }

    public void Update()
    {
        if (state.isComplete)
        {
            if (state == patrolState || state == attackState)
            {
                Set(chaseState);
            }
            else
            {
                if (enemyProperties.isAttacking)
                {
                    Set(attackState);
                }
                else
                {
                    Set(patrolState);
                }
            }

        }
        state.DoBranch();
    }

    public void FixedUpdate()
    {
        state.DoFixedBranch();
    }
}
