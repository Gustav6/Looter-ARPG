using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProperties : MonoBehaviour, IDamagable
{
    public GameObject player;
    public GameObject damagePopupPrefab;

    public Controller2D controller;

    public float distanceToPlayer;
    public float aggroRange;
    public float attackRange;

    public float speed;
    public int health;
    public int damage;
    public int knockback;

    public bool hasLineOfSight;
    public bool isAttacking;

    public LayerMask cantPassThrough;

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
    public bool TickDamageActive { get; set; }

    private void Start()
    {
        CurrentHealth = MaxHealth;

        player = GameObject.FindGameObjectWithTag("Player");

        origin = transform.position;

        controller = GetComponent<Controller2D>();
    }

    private void Update()
    {
        distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
    }

    public void FixedUpdate()
    {
        if (distanceToPlayer <= aggroRange)
        {
            Vector2 direction = (player.transform.position - transform.position).normalized;
            RaycastHit2D ray = Physics2D.Raycast(transform.position, direction, distanceToPlayer, cantPassThrough);
            if (!ray)
            {
                Debug.DrawRay(transform.position, direction * distanceToPlayer, Color.green);
                if (!hasLineOfSight)
                {
                    hasLineOfSight = true;
                }
            }
            else
            {
                if (hasLineOfSight)
                {
                    hasLineOfSight = false;
                }
            }
        }
        else if (hasLineOfSight)
        {
            hasLineOfSight = false;
        }
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
