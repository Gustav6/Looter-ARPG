using System;
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
using static UnityEngine.RuleTile.TilingRuleOutput;

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
            GenerateShortestSpanningTree();
            GenerateCorridors(3);
            PlaceWalls();

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
        Vector2 a, b, c;

        if (manager.generationRadius <= 0)
        {
            manager.generationRadius = 1;
        }

        a = new Vector2(-manager.generationRadius, -manager.generationRadius) * 10;
        b = new Vector2(manager.generationRadius, -manager.generationRadius) * 10;
        c = new Vector2(0, manager.generationRadius) * 10;

        Triangle superTriangle;

        while (true)
        {
            superTriangle = new Triangle(a, b, c);

            bool triangleContainsAllPoints = true;

            foreach (Vector2 point in points)
            {
                if (!superTriangle.PointInTriangle(point))
                {
                    triangleContainsAllPoints = false;

                    c *= 2;
                    a *= 2;
                    b *= 2;

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

        //triangles.Add(new Triangle(points[0], superTriangle.left, superTriangle.right));
        //triangles.Add(new Triangle(points[0], superTriangle.top, superTriangle.right));
        //triangles.Add(new Triangle(points[0], superTriangle.left, superTriangle.top));

        Triangle[] tempArray = new Triangle[3];

        List<Triangle> effectedTriangles = new();

        for (int i = 0; i < points.Length; i++)
        {
            effectedTriangles.Clear();

            foreach (Triangle triangle in triangles)
            {
                if (triangle.PointInTriangle(points[i]))
                {
                    // Split triangle into 3 triangles
                    tempArray[0] = new Triangle(points[i], triangle.left, triangle.right);
                    tempArray[1] = new Triangle(points[i], triangle.top, triangle.left);
                    tempArray[2] = new Triangle(points[i], triangle.top, triangle.right);

                    effectedTriangles.AddRange(tempArray);

                    // Remove the original triangle that was split
                    triangles.Remove(triangle);

                    break;
                }
            }

            //for (int j = 0; j < triangles.Count; j++)
            //{
            //    Vector2 circumCirclePosition = triangles[j].CircumCenter();

            //    float r = Vector2.Distance(circumCirclePosition, triangles[j].vertices[0]);

            //    Circle circumCircle = new(circumCirclePosition, r);

            //    #region Debug
            //    GameObject test1 = GameObject.Instantiate(manager.debugLineObject);
            //    GameObject test2 = GameObject.Instantiate(manager.debugLineObject);

            //    LineRenderer lnTest1 = test1.GetComponent<LineRenderer>();
            //    LineRenderer lnTest2 = test2.GetComponent<LineRenderer>();

            //    lnTest1.positionCount = 2;
            //    lnTest2.positionCount = 2;

            //    lnTest1.SetPosition(0, new Vector3(circumCirclePosition.x - r, circumCirclePosition.y, -1));
            //    lnTest1.SetPosition(1, new Vector3(circumCirclePosition.x + r, circumCirclePosition.y, -1));

            //    lnTest2.SetPosition(0, new Vector3(circumCirclePosition.x, circumCirclePosition.y + r, -1));
            //    lnTest2.SetPosition(1, new Vector3(circumCirclePosition.x, circumCirclePosition.y - r, -1));
            //    #endregion
            //}

            List<Triangle> badTriangles = new();
            // Change triangles that don't meat condition

            
            for (int j = 0; j < effectedTriangles.Count; j++)
            {
                Triangle effected = effectedTriangles[j];

                List<Triangle> neighbors = GetNeighbors(triangles, effected);
                Debug.Log(neighbors.Count);

                for (int k = 0; k < neighbors.Count; k++)
                {
                    Vector2 circumCirclePosition = effected.CircumCenter();

                    float r = Vector2.Distance(circumCirclePosition, effected.vertices[0]);

                    Circle circumCircle = new(circumCirclePosition, r);

                    #region Debug
                    //GameObject test1 = GameObject.Instantiate(manager.debugLineObject);
                    //GameObject test2 = GameObject.Instantiate(manager.debugLineObject);

                    //LineRenderer lnTest1 = test1.GetComponent<LineRenderer>();
                    //LineRenderer lnTest2 = test2.GetComponent<LineRenderer>();

                    //lnTest1.positionCount = 2;
                    //lnTest2.positionCount = 2;

                    //lnTest1.SetPosition(0, new Vector3(circumCirclePosition.x - r, circumCirclePosition.y, -1));
                    //lnTest1.SetPosition(1, new Vector3(circumCirclePosition.x + r, circumCirclePosition.y, -1));

                    //lnTest2.SetPosition(0, new Vector3(circumCirclePosition.x, circumCirclePosition.y + r, -1));
                    //lnTest2.SetPosition(1, new Vector3(circumCirclePosition.x, circumCirclePosition.y - r, -1));
                    #endregion

                    foreach (Vector2 vertice in neighbors[k].vertices)
                    {
                        if (effected.vertices.Contains(vertice) && neighbors[k].vertices.Contains(vertice))
                        {
                            continue;
                        }

                        if (circumCircle.Intersects(vertice))
                        {
                            GameObject parent = new();
                            GameObject t1 = new(), t2 = new();

                            t1.transform.parent = parent.transform;
                            t2.transform.parent = parent.transform;

                            t1.transform.position = effected.Center();
                            t2.transform.position = neighbors[k].Center();

                            #region Get vertices
                            List<Vector2> vertices = new();
                            vertices.AddRange(effected.vertices);
                            vertices.AddRange(neighbors[k].vertices);

                            Vector2? pointA = null, pointB = null, pointC = null, pointD = null;

                            for (int l = vertices.Count - 1; l >= 0; l--)
                            {
                                if (effected.vertices.Contains(vertices[l]) && neighbors[k].vertices.Contains(vertices[l]))
                                {
                                    if (pointA == null)
                                    {
                                        pointA = vertices[l];
                                    }
                                    else if (pointB == null && vertices[l] != pointA)
                                    {
                                        pointB = vertices[l];
                                    }
                                }
                                else
                                {
                                    if (pointC == null)
                                    {
                                        pointC = vertices[l];
                                    }
                                    else if (pointD == null && vertices[l] != pointC)
                                    {
                                        pointD = vertices[l];
                                    }
                                }
                            }
                            #endregion

                            #region Debug
                            GameObject g1 = new();
                            g1.transform.position = pointA.Value;
                            GameObject g2 = new();
                            g2.transform.position = pointC.Value;
                            GameObject g3 = new();
                            g3.transform.position = pointD.Value;

                            g1.transform.parent = t1.transform;
                            g2.transform.parent = t1.transform;
                            g3.transform.parent = t1.transform;

                            GameObject g4 = new();
                            g4.transform.position = pointB.Value;
                            GameObject g5 = new();
                            g5.transform.position = pointC.Value;
                            GameObject g6 = new();
                            g6.transform.position = pointD.Value;

                            g4.transform.parent = t2.transform;
                            g5.transform.parent = t2.transform;
                            g6.transform.parent = t2.transform;
                            #endregion

                            triangles.Remove(neighbors[k]);
                            badTriangles.Add(effected);

                            Triangle newTriangle1 = new(pointA.Value, pointC.Value, pointD.Value);
                            Triangle newTriangle2 = new(pointB.Value, pointC.Value, pointD.Value);

                            effectedTriangles.Add(newTriangle1);
                            effectedTriangles.Add(newTriangle2);
                        }
                    }
                }
            }

            for (int j = effectedTriangles.Count - 1; j >= 0; j--)
            {
                if (badTriangles.Contains(effectedTriangles[j]))
                {
                    effectedTriangles.RemoveAt(j);
                }
            }

            triangles.AddRange(effectedTriangles);

        }

        #region Remove super triangel
        for (int i = triangles.Count - 1; i >= 0; i--)
        {
            if (triangles[i].ContainsPoint(superTriangle.top) ||
                triangles[i].ContainsPoint(superTriangle.right) ||
                triangles[i].ContainsPoint(superTriangle.left))
            {
                triangles.Remove(triangles[i]);
            }
        }
        #endregion

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

        #region Debug super triangle
        //GameObject debugSuperTriangle = GameObject.Instantiate(manager.debugLineObject);

        //LineRenderer lnTemp = debugSuperTriangle.GetComponent<LineRenderer>();

        //lnTemp.positionCount = 4;

        //lnTemp.SetPosition(0, new Vector3(superTriangle.left.x, superTriangle.left.y, -1));
        //lnTemp.SetPosition(1, new Vector3(superTriangle.right.x, superTriangle.right.y, -1));
        //lnTemp.SetPosition(2, new Vector3(superTriangle.top.x, superTriangle.top.y, -1));
        //lnTemp.SetPosition(3, new Vector3(superTriangle.left.x, superTriangle.left.y, -1));

        //GameObject test1 = GameObject.Instantiate(manager.debugLineObject);
        //GameObject test2 = GameObject.Instantiate(manager.debugLineObject);

        //LineRenderer lnTest1 = test1.GetComponent<LineRenderer>();
        //LineRenderer lnTest2 = test2.GetComponent<LineRenderer>();

        //Vector2 cir = superTriangle.Center();
        //float rad = Vector2.Distance(cir, superTriangle.vertices[0]);

        //lnTest1.positionCount = 2;
        //lnTest2.positionCount = 2;

        //lnTest1.SetPosition(0, new Vector3(cir.x - rad, cir.y, -1));
        //lnTest1.SetPosition(1, new Vector3(cir.x + rad, cir.y, -1));

        //lnTest2.SetPosition(0, new Vector3(cir.x, cir.y + rad, -1));
        //lnTest2.SetPosition(1, new Vector3(cir.x, cir.y - rad, -1));
        #endregion
    }

    private List<Triangle> GetNeighbors(List<Triangle> triangles, Triangle currentTriangle)
    {
        List<Triangle> neighbors = new();

        foreach (Triangle triangle in triangles)
        {
            if (triangle == currentTriangle)
            {
                continue;
            }

            int sharedVertices = 0;

            foreach (Vector2 vertice in triangle.vertices)
            {
                if (currentTriangle.vertices.Contains(vertice))
                {
                    sharedVertices++;

                    if (sharedVertices >= 2)
                    {
                        neighbors.Add(triangle);
                        break;
                    }
                }
            }
        }

        return neighbors;
    }

    private void GenerateShortestSpanningTree()
    {

    }

    private void GenerateCorridors(float width)
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

#region Circle class
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
#endregion

#region Triangle class
public class Triangle
{
    public Vector2 left, right, top;
    public readonly Vector2[] vertices = new Vector2[3];
    public Triangle(Vector2 a, Vector2 b, Vector2 c)
    {
        vertices[0] = a;
        vertices[1] = b;
        vertices[2] = c;

        SetPointVariables(a, b, c);
    }

    public bool ContainsPoint(Vector2 point)
    {
        return vertices.Contains(point);
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

    public Vector2 Center()
    {
        return new Vector2((left.x + right.x + top.x) / 3, (left.y + right.y + top.y) / 3);
    }

    public Vector2 CircumCenter()
    {
        List<double> a = new()
        {
            top.x,
            top.y
        };
        List<double> b = new()
        {
            left.x,
            left.y
        };
        List<double> c = new()
        {
            right.x,
            right.y
        };

        List<double> center = FindCircumCenter(a, b, c);

        return new Vector2((float)center[0], (float)center[1]);
    }

    public static void LineFromPoints(List<double> P, List<double> Q, ref double a, ref double b, ref double c)
    {
        a = Q[1] - P[1];
        b = P[0] - Q[0];
        c = a * (P[0]) + b * (P[1]);
    }

    // Function which converts the input line to its
    // perpendicular bisector. It also inputs the points
    // whose mid-point lies on the bisector
    public static void PerpendicularBisectorFromLine(List<double> P, List<double> Q, ref double a, ref double b, ref double c)
    {
        List<double> mid_point = new List<double>();
        mid_point.Add((P[0] + Q[0]) / 2);

        mid_point.Add((P[1] + Q[1]) / 2);

        // c = -bx + ay
        c = -b * (mid_point[0]) + a * (mid_point[1]);

        double temp = a;
        a = -b;
        b = temp;
    }

    // Returns the intersection point of two lines
    public static List<double> LineLineIntersection(double a1, double b1, double c1, double a2, double b2, double c2)
    {
        List<double> ans = new List<double>();
        double determinant = a1 * b2 - a2 * b1;
        if (determinant == 0)
        {
            // The lines are parallel. This is simplified
            // by returning a pair of FLT_MAX
            ans.Add(double.MaxValue);
            ans.Add(double.MaxValue);
        }

        else
        {
            double x = (b2 * c1 - b1 * c2) / determinant;
            double y = (a1 * c2 - a2 * c1) / determinant;
            ans.Add(x);
            ans.Add(y);
        }

        return ans;
    }

    public static List<double> FindCircumCenter(List<double> P, List<double> Q, List<double> R)
    {
        // Line PQ is represented as ax + by = c
        double a = 0;
        double b = 0;
        double c = 0;
        LineFromPoints(P, Q, ref a, ref b, ref c);

        // Line QR is represented as ex + fy = g
        double e = 0;
        double f = 0;
        double g = 0;
        LineFromPoints(Q, R, ref e, ref f, ref g);

        // Converting lines PQ and QR to perpendicular
        // vbisectors. After this, L = ax + by = c
        // M = ex + fy = g
        PerpendicularBisectorFromLine(P, Q, ref a, ref b, ref c);
        PerpendicularBisectorFromLine(Q, R, ref e, ref f, ref g);

        // The point of intersection of L and M gives
        // the circumcenter
        List<double> circumcenter = LineLineIntersection(a, b, c, e, f, g);

        return circumcenter;
    }

    private void SetPointVariables(Vector2 pointA, Vector2 pointB, Vector2 pointC)
    {
        if (pointB.y <= pointA.y && pointC.y <= pointA.y)
        {
            top = pointA;

            SetLetAndRightPoints(pointB, pointC);
        }
        else if (pointA.y <= pointB.y && pointC.y <= pointB.y)
        {
            top = pointB;

            SetLetAndRightPoints(pointA, pointC);
        }
        else if (pointB.y <= pointC.y && pointA.y <= pointC.y)
        {
            top = pointC;

            SetLetAndRightPoints(pointA, pointB);
        }
    }

    private void SetLetAndRightPoints(Vector2 pointA, Vector2 pointB)
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
}
#endregion
