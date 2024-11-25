using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProperties : MonoBehaviour, IDamagable
{
    public GameObject player;
    public GameObject damagePopupPrefab;
    public float distanceToPlayer;

    public float speed;
    public int health;
    public int damage;
    public int knockback;

    public bool isAttacking;

    Vector2 origin;
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
                OnDeath();
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

        origin = transform.position;
    }

    private void Update()
    {
        distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
    }

public void Damage(int damageAmount)
    {
        CurrentHealth -= damageAmount;
        Instantiate(damagePopupPrefab, transform.position, Quaternion.identity);
    }

    public void OnDeath()
    {
        Destroy(gameObject);
    }
}
