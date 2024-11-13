using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Trap : MonoBehaviour
{
    public Animator animator;
    [field: SerializeField] public int Damage { get; private set; }

    private Collider2D collisionInformation;
    private bool canDealDamage = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            canDealDamage = true;
            collisionInformation = collision;
            animator.SetBool("Active", true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            canDealDamage = false;
            animator.SetBool("Active", false);
        }
    }

    private void DealDamage()
    {
        if (canDealDamage && collisionInformation.transform.TryGetComponent<IDamagable>(out var damageable))
        {
            Debug.Log("Damage dealt to player is: " + Damage);
            damageable.Damage(Damage);
        }
    }
}
