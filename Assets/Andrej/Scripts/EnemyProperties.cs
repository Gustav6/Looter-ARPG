using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProperties : MonoBehaviour, IDamagable
{
    public GameObject player;
    public float distanceToPlayer;

    public float speed = 10f;

    private int health;
    public int CurrentHealth
    {
        get => health; set
        {
            if (value > MaxHealth)
            {
                health = MaxHealth;
            }
            else if (value <= 0)
            {
                Die();
            }
            else
            {
                health = value;
            }
        } 
    }
    [field: SerializeField] public int MaxHealth { get; set; }


    private void Start()
    {
        CurrentHealth = MaxHealth;

        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
    }

    public void Damage(int damageAmount)
    {
        CurrentHealth -= damageAmount;
    }

    public void Die()
    {
        Destroy(gameObject);
    }
}
