using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

public class Projectile : MonoBehaviour
{
    float destroyBulletTimer = 2;
    CircleCollider2D colider;
    float angle;
    Vector3 prevPosition;
    private float timer;
    private int dmgTickCounter;
    [SerializeField] public int amountOfEnemiesHit;
    [SerializeField] public RaycastHit2D raycastHit2D;

    public Rigidbody2D rb;
    public LayerMask collidableLayers;

    public virtual void Start()
    {
        colider.radius = 0.5f;
        rb = GetComponent<Rigidbody2D>();
        colider = GetComponent<CircleCollider2D>();

        prevPosition = transform.position;
    }

    public virtual void Update()
    {
        destroyBulletTimer -= Time.deltaTime;

        if (destroyBulletTimer <= 0)
        {
            Destroy(gameObject);
        }

        Vector2 v = rb.linearVelocity;
        angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    public virtual void FixedUpdate()
    {
        float distance = Vector2.Distance(transform.position, transform.position + (Vector3)rb.linearVelocity * Time.fixedDeltaTime);
        Debug.Log(distance);
        raycastHit2D = Physics2D.Raycast(transform.position, rb.linearVelocity, distance, collidableLayers);
        Debug.DrawRay(transform.position, rb.linearVelocity.normalized * distance, Color.red);

        if (raycastHit2D)
        {
            IDamagable damagable = raycastHit2D.transform.GetComponent<IDamagable>();

            if (damagable != null)
            {
                damagable.Damage(GunController.Damage);

                if (GunController.fireDmg)
                {
                    timer += Time.deltaTime;
                    if (timer >= 1)
                    {
                        damagable.Damage(GunController.Damage/ 5);
                        timer = 0;
                        dmgTickCounter += 1;

                        if (dmgTickCounter >= 10)
                        {
                            
                            dmgTickCounter = 0;
                        }
                    }
                }
            }
             
            Destroy(gameObject);
            Debug.Log("Träffade något " + raycastHit2D.collider.tag);
        }

        prevPosition = transform.position;


    }
}

