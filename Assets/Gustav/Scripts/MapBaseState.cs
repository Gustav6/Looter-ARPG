using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

#region Base State
public abstract class MapBaseState
{
    public abstract void EnterState(MapGenerationManager manager);
    public abstract void UpdateState(MapGenerationManager manager);
    public abstract void ExitState(MapGenerationManager manager);
}
#endregion

#region Generation State
public class GeneratingMapState : MapBaseState
{
    public List<Room> totalRooms = new();
    public List<Room> mainRooms = new();
    private Heap<Room> heap;

    private bool canMoveRoom = false;

    public override void EnterState(MapGenerationManager manager)
    {
        heap = new(manager.totalRoomsAmount);
        manager.AmountOfMainRooms = 5;

        totalRooms.Clear();

        for (int i = 0; i < manager.totalRoomsAmount; i++)
        {
            totalRooms.Add(GenerateRoom(manager));

            heap.Add(totalRooms[i]);
        }
    }

    public override void UpdateState(MapGenerationManager manager)
    {
        canMoveRoom = false;

        foreach (Room roomA in totalRooms)
        {
            foreach (Room roomB in totalRooms)
            {
                if (RoomIntersects(manager, roomA, roomB))
                {
                    canMoveRoom = true;
                    break;
                }
            }

            if (canMoveRoom)
            {
                break;
            }
        }

        if (canMoveRoom)
        {
            SeparateRooms(manager);
        }
        else
        {
            GenerateDelaunayTriangulation(manager, totalRooms);

            manager.SwitchState(manager.loadingState);
        }
    }

    public override void ExitState(MapGenerationManager manager)
    {
        if (manager.showEveryRoom)
        {
            PlaceRooms(manager, totalRooms);
        }
        else
        {
            mainRooms.Clear();

            for (int i = 0; i < manager.AmountOfMainRooms; i++)
            {
                mainRooms.Add(heap.RemoveFirst());
            }

            PlaceRooms(manager, mainRooms);
        }
    }

    #region Generate Room methods
    private Room GenerateRoom(MapGenerationManager manager)
    {
        int roomWidth = UnityEngine.Random.Range(5, manager.roomMaxSize.x + 1);
        int roomHeight = UnityEngine.Random.Range(5, manager.roomMaxSize.y + 1);
        
        Vector2Int offset = new(roomWidth / 2, roomHeight / 2);
        Vector2 position = RandomPosition(manager.generationRadius) - offset;

        return new Room(roomWidth, roomHeight, new Vector2(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y)));
    }

    public Vector2 RandomPosition(float radius)
    {
        float r = radius * Mathf.Sqrt(UnityEngine.Random.Range(0.0001f, 1));
        float theta = UnityEngine.Random.Range(0.0001f, 1) * 2 * Mathf.PI;

        return new Vector2(r * Mathf.Cos(theta), r * Mathf.Sin(theta));
        //return new Vector2(RoundM(r * Mathf.Cos(theta), MapGenerationManager.tileSize), RoundM(r * Mathf.Sin(theta), MapGenerationManager.tileSize));
    }
    #endregion

    #region Seperate room methods

    private void SeparateRooms(MapGenerationManager manager)
    {
        for (int i = 0; i < totalRooms.Count; i++)
        {
            totalRooms[i].MoveRoom(GetDirection(manager, totalRooms[i]));
        }
    }

    private bool RoomIntersects(MapGenerationManager manager, Room roomA, Room roomB)
    {
        if (roomA != roomB)
        {
            Vector2 roomALowerLeft = manager.tileMap.CellToWorld((Vector3Int)roomA.tiles[0, 0].gridPosition);
            Vector2 roomATopRight = manager.tileMap.CellToWorld((Vector3Int)roomA.tiles[roomA.width - 1, roomA.height - 1].gridPosition) + Vector3.one;

            Vector2 roomBLowerLeft = manager.tileMap.CellToWorld((Vector3Int)roomB.tiles[0, 0].gridPosition);
            Vector2 roomBTopRight = manager.tileMap.CellToWorld((Vector3Int)roomB.tiles[roomB.width - 1, roomB.height - 1].gridPosition) + Vector3.one;

            if (roomALowerLeft.x < roomBTopRight.x && roomATopRight.x > roomBLowerLeft.x)
            {
                if (roomALowerLeft.y < roomBTopRight.y && roomATopRight.y > roomBLowerLeft.y)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public Vector2Int GetDirection(MapGenerationManager manager, Room room)
    {
        Vector2 separationVelocity = Vector2.zero;
        float numberOfAgentsToAvoid = 0;

        for (int i = 0; i < totalRooms.Count; i++)
        {
            if (ReferenceEquals(totalRooms[i], room) || !RoomIntersects(manager, room, totalRooms[i]))
            {
                continue;
            }

            Vector2 otherPosition = totalRooms[i].WorldPosition;

            Vector2 otherAgentToCurrent = room.WorldPosition - otherPosition;
            Vector2 directionToTravel = otherAgentToCurrent.normalized;

            separationVelocity += directionToTravel;
            numberOfAgentsToAvoid++;
        }

        if (separationVelocity != Vector2.zero)
        {
            separationVelocity.Normalize();
        }

        #region Set velocity
        if (separationVelocity.x > 0)
        {
            separationVelocity.x = 1;
        }
        else if (separationVelocity.x < 0)
        {
            separationVelocity.x = -1;
        }
        if (separationVelocity.y > 0)
        {
            separationVelocity.y = 1;
        }
        else if (separationVelocity.y < 0)
        {
            separationVelocity.y = -1;
        }
        #endregion

        return new Vector2Int((int)separationVelocity.x, (int)separationVelocity.y);
    }
    #endregion

    private void GenerateDelaunayTriangulation(MapGenerationManager manager, List<Room> rooms)
    {
        Vector2[] points = new Vector2[rooms.Count];

        for (int i = 0; i < points.Length; i++)
        {
            points[i] = rooms[i].WorldPosition;
        }

        List<Triangle> triangles = new();

        #region Super Triangle
        Vector2 left, right, top;

        if (manager.generationRadius <= 0)
        {
            manager.generationRadius = 1;
        }

        left = new Vector2(-manager.generationRadius, -manager.generationRadius);
        right = new Vector2(manager.generationRadius, -manager.generationRadius);
        top = new Vector2(0, manager.generationRadius);

        Triangle superTriangle;

        while (true)
        {
            superTriangle = new Triangle(top, right, left);

            bool triangleContainsAllPoints = true;

            for (int i = 0; i < points.Length; i++)
            {
                if (!superTriangle.PointInTriangle(points[i]))
                {
                    triangleContainsAllPoints = false;

                    top *= 2;
                    left *= 2;
                    right *= 2;

                    break;
                }
            }

            if (triangleContainsAllPoints)
            {
                break;
            }
        }

        triangles.Add(superTriangle);
        #endregion

        List<Triangle> tempTriangles = new();

        List<Triangle> closedTriangles = new();

        for (int i = 0; i < points.Length; i++)
        {
            tempTriangles.Clear();

            for (int j = triangles.Count - 1; j >= 0; j--)
            {
                if (closedTriangles.Contains(triangles[j]))
                {
                    continue;
                }

                Vector2 circumPosition = triangles[j].Center();
                float radius = Vector2.Distance(circumPosition, triangles[j].top);

                Circle circumTemp = new(circumPosition, radius);

                if (circumTemp.Intersects(points[i]))
                {
                    tempTriangles.Add(new Triangle(points[i], triangles[j].left, triangles[j].right));
                    tempTriangles.Add(new Triangle(points[i], triangles[j].top, triangles[j].right));
                    tempTriangles.Add(new Triangle(points[i], triangles[j].left, triangles[j].top));
                    closedTriangles.Add(triangles[j]);
                }
            }

            for (int j = 0; j < tempTriangles.Count; j++)
            {
                triangles.Add(tempTriangles[j]);

                for (int k = 0; k < triangles.Count; k++)
                {
                    if (tempTriangles[j].Intersects(triangles[k]))
                    {
                        triangles.Remove(triangles[k]);
                    }
                }
            }
        }

        //#region Remove triangles connected to super triangel
        //for (int i = triangles.Count - 1; i >= 0; i--)
        //{
        //    if (triangles[i].ContainsPoint(superTriangle.top))
        //    {
        //        triangles.Remove(triangles[i]);
        //    }
        //    else if (triangles[i].ContainsPoint(superTriangle.right))
        //    {
        //        triangles.Remove(triangles[i]);
        //    }
        //    else if (triangles[i].ContainsPoint(superTriangle.left))
        //    {
        //        triangles.Remove(triangles[i]);
        //    }
        //}
        //#endregion

        #region Debug
        for (int i = 0; i < triangles.Count; i++)
        {
            GameObject debug = GameObject.Instantiate(manager.debugLineObject);

            LineRenderer ln = debug.GetComponent<LineRenderer>();

            ln.positionCount = 4;

            ln.SetPosition(0, new Vector3(triangles[i].left.x, triangles[i].left.y, -1));
            ln.SetPosition(1, new Vector3(triangles[i].right.x, triangles[i].right.y, -1));
            ln.SetPosition(2, new Vector3(triangles[i].top.x, triangles[i].top.y, -1));
            ln.SetPosition(3, new Vector3(triangles[i].left.x, triangles[i].left.y, -1));
        }
        #endregion

        //GameObject debugSuperTriangle = GameObject.Instantiate(manager.debugLineObject);

        //LineRenderer lnTemp = debugSuperTriangle.GetComponent<LineRenderer>();

        //lnTemp.positionCount = 4;

        //lnTemp.SetPosition(0, new Vector3(superTriangle.left.x, superTriangle.left.y, -1));
        //lnTemp.SetPosition(1, new Vector3(superTriangle.right.x, superTriangle.right.y, -1));
        //lnTemp.SetPosition(2, new Vector3(superTriangle.top.x, superTriangle.top.y, -1));
        //lnTemp.SetPosition(3, new Vector3(superTriangle.left.x, superTriangle.left.y, -1));
    }

    private void GetShortestSpanningTree()
    {

    }

    private void GenerateCorridors()
    {

    }

    private void PlaceWalls()
    {

    }

    private void PlaceRooms(MapGenerationManager manager, List<Room> rooms)
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            for (int x = 0; x < rooms[i].tiles.GetLength(0); x++)
            {
                for (int y = 0; y < rooms[i].tiles.GetLength(1); y++)
                {
                    TileBase tileTexture = manager.tileTexture;

                    if (x == 0 && y == 0)
                    {
                        tileTexture = manager.debugTexture;
                    }
                    else if (x == rooms[i].tiles.GetLength(0) - 1 && y == rooms[i].tiles.GetLength(1) - 1)
                    {
                        tileTexture = manager.debugTexture;
                    }

                    manager.tileMap.SetTile((Vector3Int)rooms[i].tiles[x, y].gridPosition, tileTexture);
                }
            }

            GameObject g = new();
            g.transform.position = rooms[i].WorldPosition;
        }
    }
}
#endregion

#region Load State
public class LoadMapState : MapBaseState
{
    public override void EnterState(MapGenerationManager manager)
    {

    }

    public override void UpdateState(MapGenerationManager manager)
    {

    }

    public override void ExitState(MapGenerationManager manager)
    {

    }
}
#endregion

#region Room and tile class
public class Room : IHeapItem<Room>
{
    #region World position
    public Vector2 WorldPosition
    {
        get
        {
            Vector2 worldPosition = MapGenerationManager.instance.tileMap.CellToWorld((Vector3Int)tiles[width - 1, height - 1].gridPosition) + Vector3.one;

            worldPosition.x -= width / 2f;
            worldPosition.y -= height / 2f;

            return worldPosition;
        }
    }
    #endregion

    #region Size of room
    public readonly int width, height;
    public int Size
    {
        get
        {
            return width * height;
        }
    }
    #endregion

    #region Heap variables
    private int heapIndex;
    public int HeapIndex
    {
        get { return heapIndex; }
        set { heapIndex = value; }
    }
    #endregion

    public RoomTile[,] tiles;

    public Room(int width, int height, Vector2 position)
    {
        this.width = width;
        this.height = height;
        tiles = new RoomTile[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int xPosition = x + (int)position.x;
                int yPosition = y + (int)position.y;

                tiles[x, y] = new RoomTile(new Vector2Int(xPosition, yPosition));
            }
        }
    }

    public void MoveRoom(Vector2Int direction)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                tiles[x, y].gridPosition += direction;
            }
        }
    }

    public int CompareTo(Room other)
    {
        int compare = Size.CompareTo(other.Size);

        return compare;
    }
}

public struct RoomTile
{
    public Vector2Int gridPosition;

    public RoomTile(Vector2Int gridPosition)
    {
        this.gridPosition = gridPosition;
    }
}
#endregion

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

public class Triangle
{
    public Vector2 left, right, top;

    public Triangle(Vector2 a, Vector2 b, Vector2 c)
    {
        if (b.y < a.y && c.y < a.y)
        {
            top = a;

            SetLeftAndRight(b, c);
        }
        else if (a.y < b.y && c.y < b.y)
        {
            top = b;

            SetLeftAndRight(a, c);
        }
        else if (b.y < c.y && a.y < c.y)
        {
            top = c;

            SetLeftAndRight(a, b);
        }
    }

    public bool ContainsPoint(Vector2 point)
    {
        if (top == point)
        {
            return true;
        }
        else if (left == point)
        {
            return true;
        }
        else if (right == point)
        {
            return true;
        }

        return false;
    }

    public bool PointInTriangle(Vector2 p)
    {
        double s1 = top.y - left.y;
        double s2 = top.x - left.x;
        double s3 = right.y - left.y;
        double s4 = p.y - left.y;

        double w1 = (left.x * s1 + s4 * s2 - p.x * s1) / (s3 * s2 - (right.x - left.x) * s1);
        double w2 = (s4 - w1 * s3) / s1;

        return w1 >= 0 && w2 >= 0 && (w1 + w2) <= 1;
    }

    public bool Intersects(Triangle triangle)
    {
        return false;
    }

    private void SetLeftAndRight(Vector2 pointA, Vector2 pointB)
    {
        if (pointA.x < pointB.x)
        {
            left = pointA;
            right = pointB;
        }
        else
        {
            left = pointB;
            right = pointA;
        }
    }

    public Vector2 Center()
    {
        return new Vector2((left.x + right.x + top.x) / 3, (left.y + right.y + top.y) / 3);
    }
}
