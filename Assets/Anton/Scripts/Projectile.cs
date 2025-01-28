using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine; 

public abstract class Projectile : MonoBehaviour
{
    private float destroyBulletTimer = 1;
    private CircleCollider2D colider;
    private float angle;
    [HideInInspector] public int amountOfEnemiesHit;
    [HideInInspector] public RaycastHit2D raycastHit;
    private float sprtRendY;
    private int numberOfHitEnemies = 0;

    public Rigidbody2D rb;
    public LayerMask collidableLayers;
    public bool bigProjectile;
    public static int amountOfPiercableObjects;

    private const float skinWidth = .015f;
    const float distanceBetweenRays = .25f;

    protected int horizontalRayCount;
    protected int verticalRayCount;

    private float horizontalRaySpacing;
    private float verticalRaySpacing;

    private bool sentOutDmgPopUp = false;

    public virtual void Start()
    {       
        rb = GetComponent<Rigidbody2D>();
        colider = GetComponent<CircleCollider2D>();
        colider.radius = 0.5f;

        sprtRendY = GetComponent<SpriteRenderer>().bounds.size.y;
        sprtRendY /= 2;
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
        if (ProjectileHit())
        {
            if (raycastHit.transform.TryGetComponent<IDamagable>(out var damagable))
            {
                OnHit(raycastHit, damagable);
            }

            if (numberOfHitEnemies >= GunController.Instance.pierceAmount)
            {
                Destroy(gameObject);
            }
            Debug.Log("Träffade något " + raycastHit.collider.tag);
        }
    }

    public virtual void OnHit(RaycastHit2D hit, IDamagable damagable)
    {

        damagable.Damage(GunController.Instance.Damage);
        if (sentOutDmgPopUp!)
        {
            damagable.DamagePopUp(GunController.Instance.damagePopupPrefab, hit.transform.position, GunController.Instance.Damage);
            sentOutDmgPopUp = true;
        }
    }

    private bool ProjectileHit()
    {
        for (int i = 0; i < 3; i++)
        {
            float distance = Vector2.Distance(transform.position, transform.position + (Vector3)rb.linearVelocity * Time.fixedDeltaTime);
            raycastHit = Physics2D.Raycast(transform.position, rb.linearVelocity, distance, collidableLayers);
            Debug.DrawRay(transform.position, rb.linearVelocity.normalized * distance, Color.red);

            if (raycastHit)
            {
                return true;
            }
        }

        return false;
    }

    void CalculateRaySpacing()
    {
        Bounds bounds = colider.bounds;
        bounds.Expand(skinWidth * -2);

        float boundsWidth = bounds.size.x;
        float boundsHeight = bounds.size.y;

        horizontalRayCount = Mathf.Clamp(Mathf.RoundToInt(boundsHeight / distanceBetweenRays), 2, 2 + Mathf.RoundToInt(boundsHeight / distanceBetweenRays));
        verticalRayCount = Mathf.Clamp(Mathf.RoundToInt(boundsWidth / distanceBetweenRays), 2, 2 + Mathf.RoundToInt(boundsWidth / distanceBetweenRays));

        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }
}

