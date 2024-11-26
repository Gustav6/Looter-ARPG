using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ShootingEnemyBehaviour : Core
{
    public EnemyProperties enemyProperties;

    public PatrolState patrolState;
    public ChaseState chaseState;
    public ShootingState shootingState;

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
        if(state.isComplete)
        {
            if (state == patrolState || state == shootingState)
            {
                Set(chaseState);
            }
            else
            {
                if (enemyProperties.isAttacking)
                {
                    Set(shootingState);
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

