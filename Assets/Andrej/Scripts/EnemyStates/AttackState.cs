using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : State
{
    EnemyProperties enemyProperties;
    public override void Enter() 
    {
        Debug.Log("attacking");
    }
    public override void Do() 
    {
        if(!enemyProperties.isAttacking)
        {
            isComplete = true;
        }
    }

    public override void Exit() 
    { 
    
    }
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (enemyProperties.isAttacking)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            if (other.TryGetComponent<IDamagable>(out IDamagable damagable))
            {
                damagable.Damage(enemyProperties.damage);
                enemyProperties.isAttacking = false;
                isComplete = true;
            }
        }
        else
        {
            enemyProperties.isAttacking = false;
        }
    }
}
