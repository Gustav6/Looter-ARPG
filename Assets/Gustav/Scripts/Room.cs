using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class Room : IHeapItem<Room>
{
    #region World position
    public Vector2 WorldPosition
    {
        get
        {
            return center;
        }
    }

    public Vector2 BottomLeft
    {
        get
        {
            return new Vector2(WorldPosition.x - width / 2, WorldPosition.y - height / 2) - Vector2.one;
        }
    }
    public Vector2 TopRight
    {
        get
        {
            return new Vector2(WorldPosition.x + width / 2 + 1, WorldPosition.y + height / 2 + 1) + Vector2.one;
        }
    }

    public Vector2 center;
    #endregion

    #region Size of room
    public readonly int width, height;
    #endregion

    #region Heap variables
    private int heapIndex;
    public int HeapIndex
    {
        get { return heapIndex; }
        set { heapIndex = value; }
    }
    #endregion

    public List<Vector3Int> walls = new();
    public List<Vector3Int> groundTiles = new();
    public readonly int tileCount = 0;

    public Room(int width, int height, Vector2 position, bool roundCorners = false)
    {
        this.width = width;
        this.height = height;

        Circle c1 = null, c2 = null;

        #region Set Circles
        if (roundCorners)
        {
            float r;
            Vector2 tempPosition1, tempPosition2;

            if (width < height)
            {
                r = width / 2f;
                tempPosition1 = new Vector2((int)position.x + width / 2f, (int)position.y + height - width / 2.65f);
                tempPosition2 = new Vector2((int)position.x + width / 2f, (int)position.y + width / 2.65f);
            }
            else if (width >= height)
            {
                r = height / 2f;
                tempPosition1 = new Vector2((int)position.x + width - height / 2.65f, (int)position.y + height / 2f);
                tempPosition2 = new Vector2((int)position.x + height / 2.65f, (int)position.y + height / 2f);
            }
            else
            {
                r = 0;
                tempPosition1 = Vector2.zero;
                tempPosition2 = Vector2.zero;
            }

            c1 = new Circle(tempPosition1, r);
            c2 = new Circle(tempPosition2, r);
        }
        #endregion

        Vector2Int tilePos;
        bool canAdd;

        for (int i = 0; i < width * height; i++)
        {
            tilePos = new Vector2Int(i % width, i / width) + Vector2Int.FloorToInt(position);

            if (roundCorners)
            {
                canAdd = false;

                if (tilePos.x <= c1.position.x && tilePos.x >= c2.position.x || tilePos.y <= c1.position.y && tilePos.y >= c2.position.y)
                {
                    canAdd = true;
                }
                else
                {
                    if (c1.Intersects(tilePos + (Vector2.one / 2)) || c2.Intersects(tilePos + (Vector2.one / 2)))
                    {
                        canAdd = true;
                    }
                }
            }
            else
            {
                canAdd = true;
            }

            if (canAdd)
            {
                groundTiles.Add((Vector3Int)tilePos);
                tileCount++;
            }
            else
            {
                walls.Add((Vector3Int)tilePos);
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                walls.Add(new Vector3Int(x, -1 - y) + Vector3Int.FloorToInt(position));
                walls.Add(new Vector3Int(x, height + y) + Vector3Int.FloorToInt(position));
            }
        }

        for (int x = -height; x < height * 2; x++)
        {
            for (int y = 0; y < width; y++)
            {
                walls.Add(new Vector3Int(-1 - y, x) + Vector3Int.FloorToInt(position));
                walls.Add(new Vector3Int(width + y, x) + Vector3Int.FloorToInt(position));
            }
        }

        center = new Vector2(width / 2f, height / 2f) + position;
    }

    public void MoveRoom(Vector3Int direction)
    {
        for (int i = 0; i < groundTiles.Count; i++)
        {
            groundTiles[i] += direction;
        }

        for (int i = 0; i < walls.Count; i++)
        {
            walls[i] += direction;
        }

        center += (Vector2Int)direction;
    }

    public int CompareTo(Room other)
    {
        int compare = tileCount.CompareTo(other.tileCount);

        return compare;
    }
}
