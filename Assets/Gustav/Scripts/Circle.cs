using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Circle
{
    public Vector2 position;
    public float radius;

    public Circle(Vector2 position, float radius)
    {
        this.position = position;
        this.radius = radius;
    }

    public bool Intersects(Vector2 point)
    {
        float distance = Mathf.Sqrt((position.x - point.x) * (position.x - point.x) + (position.y - point.y) * (position.y - point.y));

        if (distance <= radius)
        {
            return true;
        }

        return false;
    }
}
