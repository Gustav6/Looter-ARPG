using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : State
{
    private float delay = 0.15f;
    public override void Enter() 
    {
        Debug.Log("attacking");
    }
    public override void Do() 
    {
        if (time > 0.5)
        {
            enemyProperties.isAttacking = false;
            isComplete = true;
        }
    }

    public override void Exit() 
    { 
    
    }

    public void OnTriggerStay2D(Collider2D other)
    {
        if (!enemyProperties.isAttacking)
        {
            return;
        }
        else
        {
            if (time > 0.1)
            {
                if (!other.CompareTag("Player"))
                {
                    enemyProperties.isAttacking = false;
                    isComplete = true;
                    return;
                }


                if (other.TryGetComponent<IDamagable>(out IDamagable damagable))
                {
                    damagable.Damage(enemyProperties.damage);

                    Rigidbody2D player = other.GetComponent<Rigidbody2D>();
                    Vector2 dir = (player.transform.position - transform.position).normalized;
                    player.AddForce(dir * enemyProperties.knockback, ForceMode2D.Impulse);
                    StartCoroutine(Reset());
                }
                enemyProperties.isAttacking = false;
                isComplete = true;
            }
        }
    }

    private IEnumerator Reset()
    {
        yield return new WaitForSeconds(delay);
        Rigidbody2D player = enemyProperties.player.GetComponent<Rigidbody2D>();
        player.linearVelocity = Vector2.zero;
    }
}
