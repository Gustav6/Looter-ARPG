using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AntonsTemporaryEnemyScript : MonoBehaviour, IDamagable
{
    private int health;

    public int CurrentHealth 
    { 
        get => health; 
        set 
        {
            if (value > MaxHealth)
            {
                health = MaxHealth;
            }
            else if (value < 0)
            {
                health = 0;
                OnDeath();
            }
            else
            {
                health = value;
            }
        } 
    }
    [field: SerializeField] public int MaxHealth { get; set; }
    public bool TickDamageActive { get; set; }
    public bool IsBeingKnockedBack { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public Coroutine KnockbackCoroutine { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public AnimationCurve KnockbackForceCurve { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    private void Start()
    {
        CurrentHealth = MaxHealth;
    }

    public void Damage(int damageAmount)
    {
        CurrentHealth -= damageAmount;
    }

    public void OnDeath()
    {
        Destroy(gameObject);
    }

    public void Knockback(Controller2D controller, float strength, Vector2 direction)
    {
        throw new System.NotImplementedException();
    }
}
