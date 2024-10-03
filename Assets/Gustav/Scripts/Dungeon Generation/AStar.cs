using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class AStar : MonoBehaviour
{
    public static AStar instance;

    private Node[,] nodes;

    private int gridWidth, gridHeight;
    private int gridOffsetX, gridOffsetY;

    private Node start, target;

    private List<Vector3Int> path = new();
    private List<Node> currentNodeNeighbors = new();
    private HashSet<Node> closedNodes = new();
    private Heap<Node> openNodes;

    public int amountOfRequests;
    public TimeSpan totalTimeTaken;

    public void Start()
    {
        if (instance != null && instance != this)
        {
            Destroy(this);
            return;
        }

        instance = this;
    }

    public void StartFindPath()
    {

    }

    public List<Vector3Int> FindPath(Vector3Int _start, Vector3Int _target)
    {
        #region Diagnostic
        //System.Diagnostics.Stopwatch sw = new();
        //sw.Start();
        #endregion

        amountOfRequests++;

        gridWidth = Math.Abs(_start.x - _target.x) + 1;
        gridHeight = Math.Abs(_start.y - _target.y) + 1;

        if (_start.x < _target.x)
        {
            gridOffsetX = _start.x;
        }
        else
        {
            gridOffsetX = _target.x;
        }

        if (_start.y < _target.y)
        {
            gridOffsetY = _start.y;
        }
        else
        {
            gridOffsetY = _target.y;
        }

        path.Clear();
        closedNodes.Clear();

        if (openNodes == null || gridWidth * gridHeight > openNodes.length)
        {
            openNodes = new Heap<Node>(gridWidth * gridHeight);
        }
        else
        {
            openNodes.Clear();
        }

        nodes = new Node[gridWidth, gridHeight];

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3Int worldPosition = new (x + gridOffsetX, y + gridOffsetY);

                nodes[x, y] = new Node(worldPosition, new Vector3Int(x, y), true);

                if (_start == worldPosition)
                {
                    start = nodes[x, y];
                }
                else if (_target == worldPosition)
                {
                    target = nodes[x, y];
                }
            }
        }

        openNodes.Add(start);

        Node currentNode;

        while (openNodes.Count > 0)
        {
            currentNode = openNodes.RemoveFirst();

            closedNodes.Add(currentNode);

            if (currentNode == target)
            {
                path = RetracePath();

                //sw.Stop();
                //Debug.Log("Time taken to find path: " + sw.ElapsedMilliseconds + " MS");

                break;
            }

            #region Check neighbors

            GetNeighbors(currentNode.gridPosition, currentNodeNeighbors);

            foreach (Node neighbor in currentNodeNeighbors)
            {
                if (closedNodes.Contains(neighbor) || !neighbor.walkable)
                {
                    continue;
                }

                int newPathCost = currentNode.gCost + GetDistance(currentNode, neighbor);

                if (newPathCost < neighbor.gCost || !openNodes.Contains(neighbor))
                {
                    neighbor.gCost = newPathCost;
                    neighbor.hCost = GetDistance(neighbor, target);
                    neighbor.parent = currentNode.gridPosition;

                    if (!openNodes.Contains(neighbor))
                    {
                        openNodes.Add(neighbor);
                    }
                    else
                    {
                        openNodes.UpdateItem(neighbor);
                    }
                }
            }

            #endregion
        }

        return new List<Vector3Int>(path);
    }

    private List<Vector3Int> RetracePath()
    {
        path.Clear();
        Node temp = target;

        while (temp.gridPosition != start.gridPosition)
        {
            if (temp.parent != null)
            {
                path.Add(temp.worldPosition);

                temp = nodes[temp.parent.Value.x, temp.parent.Value.y];
            }
            else
            {
                break;
            }
        }

        path.Reverse();

        return path;
    }

    private int GetDistance(Node NodeA, Node NodeB)
    {
        int distanceX = Math.Abs(NodeA.gridPosition.x - NodeB.gridPosition.x);
        int distanceY = Math.Abs(NodeA.gridPosition.y - NodeB.gridPosition.y);

        if (distanceX > distanceY)
        {
            return 14 * distanceY + 10 * (distanceX - distanceY);
        }
        else
        {
            return 14 * distanceX + 10 * (distanceY - distanceX);
        }
    }

    private void GetNeighbors(Vector3Int position, List<Node> list)
    {
        list.Clear();
        Node NeighboringNode;

        #region Get neighbors
        if (InBounds(position.x + 1, position.y))
        {
            NeighboringNode = nodes[position.x + 1, position.y];

            list.Add(NeighboringNode);
        }
        if (InBounds(position.x - 1, position.y))
        {
            NeighboringNode = nodes[position.x - 1, position.y];

            list.Add(NeighboringNode);
        }
        if (InBounds(position.x, position.y + 1))
        {
            NeighboringNode = nodes[position.x, position.y + 1];

            list.Add(NeighboringNode);
        }
        if (InBounds(position.x, position.y - 1))
        {
            NeighboringNode = nodes[position.x, position.y - 1];

            list.Add(NeighboringNode);
        }

        if (InBounds(position.x + 1, position.y + 1))
        {
            NeighboringNode = nodes[position.x + 1, position.y + 1];

            list.Add(NeighboringNode);
        }
        if (InBounds(position.x + 1, position.y - 1))
        {
            NeighboringNode = nodes[position.x + 1, position.y - 1];

            list.Add(NeighboringNode);
        }
        if (InBounds(position.x - 1, position.y - 1))
        {
            NeighboringNode = nodes[position.x - 1, position.y - 1];

            list.Add(NeighboringNode);
        }
        if (InBounds(position.x - 1, position.y + 1))
        {
            NeighboringNode = nodes[position.x - 1, position.y + 1];

            list.Add(NeighboringNode);
        }
        #endregion
    }

    private bool InBounds(int x, int y)
    {
        return 0 <= y && y < gridHeight && 0 <= x && x < gridWidth;
    }
}
