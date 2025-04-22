using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyProperties : MonoBehaviour, IDamagable
{
    public Seeker seeker;
    public Transform target;

    public GameObject player;
    public GameObject damagePopupPrefab;

    public Controller2D controller;

    public float distanceToPlayer;
    public float aggroRange;
    public float attackRange;

    public float nextWayPointDistance;
    Path path;
    int currentWayPoint = 0;
    bool reachedEndOfPath = false;

    public float speed;
    public int health;
    public int damage;
    public int knockback;

    public Vector2 direction;

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
    public bool IsBeingKnockedBack { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public Coroutine KnockbackCoroutine { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    public AnimationCurve KnockbackForceCurve { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    private void Start()
    {
        CurrentHealth = MaxHealth;

        player = GameObject.FindGameObjectWithTag("Player");

        origin = transform.position;

        controller = GetComponent<Controller2D>();

        seeker = GetComponent<Seeker>();

        seeker.StartPath(transform.position, target.position, OnPathComplete);

        InvokeRepeating("UpdatePath", 0f, .5f);
    }

    void UpdatePath()
    {
        if (seeker.IsDone())
        {
            seeker.StartPath(transform.position, target.position, OnPathComplete);
        }
    }
    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWayPoint = 0;
        }
    }

    private void Update()
    {
        if (path == null)
            return;
        if (currentWayPoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        } else
        {
            reachedEndOfPath = false;
        }

        direction = (path.vectorPath[currentWayPoint] - transform.position).normalized;

        float distance = Vector2.Distance(transform.position, path.vectorPath[currentWayPoint]);

        if (distance < nextWayPointDistance)
        {
            currentWayPoint++;
        }
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

    public void Knockback(Controller2D controller, float strength, Vector2 direction)
    {
        throw new System.NotImplementedException();
    }
}
