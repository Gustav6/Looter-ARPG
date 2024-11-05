using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class NormalBullet : Projectile
{
    public GameObject explosionCiclePrefab;
    public LayerMask collidableLayers;
    Vector3 prevPosition;

    public override void Start()
    {
        //RaycastHit2D raycastHit2D = Physics2D.CircleCast(transform.position, 1f, Vector3.right);

        //if (raycastHit2D)
        //{
        //    IDamagable damagable = raycastHit2D.transform.GetComponent<IDamagable>();

        //    if (damagable != null)
        //    {
        //        damagable.Damage(GunController.Damage);
        //    }

        //    Debug.Log("Tr�ffade n�got" + raycastHit2D.collider.tag);
        //}
        prevPosition = transform.position;
    }

    public void FixedUpdate()
    {
        float distance = Vector2.Distance(transform.position, transform.position + (Vector3)rb.linearVelocity * Time.fixedDeltaTime);
        Debug.Log(distance);
        RaycastHit2D raycastHit2D = Physics2D.Raycast(transform.position, rb.linearVelocity, distance, collidableLayers);
        Debug.DrawRay(transform.position, rb.linearVelocity.normalized * distance, Color.red);

        if (raycastHit2D)
        {
            //IDamagable damagable = raycastHit2D.transform.GetComponent<IDamagable>();

            //if (damagable != null)
            //{
            //    damagable.Damage(GunController.Damage);
            //}

            Destroy(gameObject);
            Debug.Log("Tr�ffade n�got" + raycastHit2D.collider.tag);
        }

        prevPosition = transform.position;
    }

    public override void Update()
    {

        // amountOfEnemiesHit += 1;
        //if (GunController.explosion)
        //{
        //   Instantiate(explosionCiclePrefab, transform.position, Quaternion.identity);
        //}

        //if (GunController.pierce)
        //{
        //   if (amountOfEnemiesHit >= GunController.pierceAmount)
        //   {
        //      Destroy(gameObject);
        //   }    
        //} 
        //else { Destroy(gameObject); }

    }      
}
