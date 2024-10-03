using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : IHeapItem<Node>
{
    public readonly bool walkable;
    public readonly Vector3Int worldPosition;
    public readonly Vector3Int gridPosition;

    public int gCost; // Cost from starting node
    public int hCost; // How far away from end node
    public int FCost { get { return hCost + gCost; } }

    public Vector3Int? parent; // What node that "owns" this node

    private int heapIndex;
    public int HeapIndex
    {
        get { return heapIndex; }
        set { heapIndex = value; }
    }

    public Node(Vector3Int position, Vector3Int gridPosition, bool walkable)
    {
        worldPosition = position;
        this.gridPosition = gridPosition;
        this.walkable = walkable;
    }


    public int CompareTo(Node nodeToCompare)
    {
        int compare = FCost.CompareTo(nodeToCompare.FCost);

        if (compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }

        return -compare;
    }
}
