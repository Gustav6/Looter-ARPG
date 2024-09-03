using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider2D))]
public class Controller2D : MonoBehaviour
{
    public LayerMask collisionMask;

    const float skinWidth = .015f;
    public int horizontalRays = 4;
    public int verticalRays = 4;

    float horizontalRaySpacing;
    float verticalRaySpacing;


    CapsuleCollider2D col;
    RaycastOrigins raycastOrigins;
    void Start()
    {
        col = GetComponent<CapsuleCollider2D>();
        CalculateRaySpacing();
    }

    public void Move(Vector3 velocity)
    {
        UpdateRaycastOrigins();

        if (velocity.x != 0)
        {
            HorizontalCollision(ref velocity);
        }

        if (velocity.y != 0)
        {
            VerticalCollision(ref velocity);
        }
      
        transform.Translate(velocity);
    }

    void HorizontalCollision(ref Vector3 velocity)
    {
        float directionX = Mathf.Sign(velocity.x);
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;
        for (int i = 0; i < horizontalRays; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? rayOrigin = raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.right * (verticalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

            if (hit)
            {
                velocity.x = (hit.distance - skinWidth) * directionX;
                rayLength = hit.distance;
            }
        }
    }

    void VerticalCollision(ref Vector3 velocity)
    {
        float directionY = Mathf.Sign(velocity.y);
        float rayLength = Mathf.Abs(velocity.y) + skinWidth;
        for (int i = 0; i < verticalRays; i++)
        {
            Vector2 rayOrigin = (directionY == -1) ? rayOrigin = raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

            if (hit)
            {
                velocity.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance;
            }
        }
    }

    void UpdateRaycastOrigins()
    {
        Bounds bounds = col.bounds;
        bounds.Expand(skinWidth * -2);

        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.max.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }


    void CalculateRaySpacing()
    {
        Bounds bounds = col.bounds;
        bounds.Expand(skinWidth * -2);

        horizontalRays = Mathf.Clamp(horizontalRays, 2, int.MaxValue);
        verticalRays = Mathf.Clamp(verticalRays, 2, int.MaxValue);

        horizontalRaySpacing = bounds.size.y / (horizontalRays - 1);
        verticalRaySpacing = bounds.size.x / (verticalRays - 1);
    }

    struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }
}