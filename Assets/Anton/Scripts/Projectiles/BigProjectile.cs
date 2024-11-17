using Unity.VisualScripting;
using UnityEngine;

public class BigProjectile : Projectile
{
    public Transform topRayPos;
    public Transform bottomRayPos;
    public override void Start()
    {
        base.Start();
    }
    public override void Update()
    {
        base.Update();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        // if (bigProjectile)
        //{
        //    float distance = Vector2.Distance(transform.position, transform.position + (Vector3)rb.linearVelocity * Time.fixedDeltaTime);
        //    raycastHit2D = Physics2D.Raycast(topRayPos.transform.position, rb.linearVelocity, distance, collidableLayers);
        //    raycastHit2D = Physics2D.Raycast(bottomRayPos.transform.position, rb.linearVelocity, distance, collidableLayers);
        //    Debug.DrawRay(topRayPos.transform.position, rb.linearVelocity.normalized * distance, Color.red);
        //    Debug.DrawRay(bottomRayPos.transform.position, rb.linearVelocity.normalized * distance, Color.red);
        //}
    }
}
