using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagable
{
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }

    public void Damage(int damageAmount);
    public void Die();
}
