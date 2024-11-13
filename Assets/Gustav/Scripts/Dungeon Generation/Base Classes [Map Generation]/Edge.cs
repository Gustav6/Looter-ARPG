using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Edge : IHeapItem<Edge>
{
    public Vector2 pointA, pointB;
    public readonly Vector2[] points = new Vector2[2];

    private int heapIndex;
    public int HeapIndex
    {
        get { return heapIndex; }
        set { heapIndex = value; }
    }

    public Edge(Vector2 pointA, Vector2 pointB)
    {
        this.pointA = pointA;
        this.pointB = pointB;

        points[0] = pointA;
        points[1] = pointB;
    }

    public bool Equals(Edge other)
    {
        if (other.pointA == pointA && other.pointB == pointB || other.pointB == pointA && other.pointA == pointB)
        {
            return true;
        }

        return false;
    }

    public int CompareTo(Edge other)
    {
        float distance = Vector2.Distance(pointA, pointB);

        float othersDistance = Vector2.Distance(other.pointA, other.pointB);

        int compare = othersDistance.CompareTo(distance);

        return compare;
    }
}
