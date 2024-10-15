using System.Collections;
using System.Collections.Generic;
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
    public Vector2Int?[] groundTiles;
    public readonly int tileCount = 0;

    private readonly Circle c1, c2;

    public Room(int width, int height, Vector2 position, bool roundCorners = false)
    {
        this.width = width;
        this.height = height;
        c1 = null;
        c2 = null;

        groundTiles = new Vector2Int?[width * height];

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

        for (int i = 0; i < groundTiles.Length; i++)
        {
            bool canAdd = false;

            int xPosition = i % width + (int)position.x;
            int yPosition = i / width + (int)position.y;

            if (roundCorners)
            {
                if (c1.position.y == c2.position.y)
                {
                    if (xPosition <= c1.position.x && xPosition >= c2.position.x)
                    {
                        canAdd = true;
                    }
                }
                else if (c1.position.x == c2.position.x)
                {
                    if (yPosition <= c1.position.y && yPosition >= c2.position.y)
                    {
                        canAdd = true;
                    }
                }

                if (!canAdd)
                {
                    if (c1.Intersects(new Vector2(xPosition + 0.5f, yPosition + 0.5f)))
                    {
                        canAdd = true;
                    }
                    else if (c2.Intersects(new Vector2(xPosition + 0.5f, yPosition + 0.5f)))
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
                groundTiles[i] = new Vector2Int(xPosition, yPosition);
                tileCount++;
            }
            else
            {
                walls.Add(new Vector3Int(xPosition, yPosition));
            }
        }

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                walls.Add(new Vector3Int(i + (int)position.x, (int)position.y - 1 - j));
                walls.Add(new Vector3Int(i + (int)position.x, (int)position.y + height + j));
            }
        }

        for (int i = -height; i < height * 2; i++)
        {
            for (int j = 0; j < width; j++)
            {
                walls.Add(new Vector3Int((int)position.x - 1 - j, i + (int)position.y));
                walls.Add(new Vector3Int((int)position.x + width + j, i + (int)position.y));
            }
        }

        center = new Vector2((int)position.x + width / 2f, (int)position.y + height / 2f);
    }

    public void MoveRoom(Vector2Int direction)
    {
        for (int i = 0; i < groundTiles.Length; i++)
        {
            if (groundTiles[i] != null)
            {
                groundTiles[i] += direction;
            }
        }

        for (int i = 0; i < walls.Count; i++)
        {
            walls[i] += (Vector3Int)direction;
        }

        center += direction;
    }

    public int CompareTo(Room other)
    {
        int compare = tileCount.CompareTo(other.tileCount);

        return compare;
    }
}
