using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
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
    public List<Vector2> roomPositions = new();
    private List<TileChangeData> tileChanges = new();
    private Heap<Room> heap;

    private bool canMoveRoom = false;

    public override void EnterState(MapGenerationManager manager)
    {
        heap = new(manager.totalRoomsAmount);
        manager.AmountOfMainRooms = 15;

        totalRooms.Clear();
        mainRooms.Clear();

        // Generate x amount of rooms
        for (int i = 0; i < manager.totalRoomsAmount; i++)
        {
            Room newRoom = GenerateRoom(manager);

            totalRooms.Add(newRoom);
            heap.Add(newRoom);
        }

        // Add x amount (Amount of main rooms) with the largest area
        for (int i = 0; i < manager.AmountOfMainRooms; i++)
        {
            mainRooms.Add(heap.RemoveFirst());
        }
    }

    public override void UpdateState(MapGenerationManager manager)
    {
        canMoveRoom = false;

        foreach (Room roomA in totalRooms)
        {
            foreach (Room roomB in totalRooms)
            {
                if (roomA.Equals(roomB))
                {
                    continue;
                }
                
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
            if (manager.showEveryRoom)
            {
                SetData(manager, totalRooms);
            }
            else
            {
                SetData(manager, mainRooms);
            }
            GenerateShortestSpanningTree(GenerateDelaunayTriangulation(manager, roomPositions), roomPositions, 10);
            GenerateCorridors(3);
            PlaceWalls();

            manager.SwitchState(manager.loadingState);
        }
    }

    public override void ExitState(MapGenerationManager manager)
    {
        for (int i = 0; i < tileChanges.Count; i++)
        {
            // Might need to change to false
            manager.tileMap.SetTile(tileChanges[i], true);
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

    #region Delaunay triangulation
    private List<Triangle> GenerateDelaunayTriangulation(MapGenerationManager manager, List<Vector2> pointsList)
    {
        if (pointsList.Count <= 3)
        {
            return new List<Triangle>();
        }

        List<Triangle> triangulation = new();

        #region Super Triangle
        Vector2 a, b, c;

        if (manager.generationRadius <= 0)
        {
            manager.generationRadius = 1;
        }

        a = new Vector2(-manager.generationRadius, -manager.generationRadius) * 100;
        b = new Vector2(manager.generationRadius, -manager.generationRadius) * 100;
        c = new Vector2(0, manager.generationRadius) * 100;

        Triangle superTriangle;

        while (true)
        {
            superTriangle = new Triangle(a, b, c);

            bool triangleContainsAllPoints = true;

            foreach (Vector2 point in pointsList)
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

        triangulation.Add(superTriangle);
        #endregion

        List<Triangle> newTriangles = new();
        List<Triangle> badTriangles = new();

        foreach (Vector2 point in pointsList)
        {
            newTriangles.Clear();
            badTriangles.Clear();

            foreach (Triangle triangle in triangulation)
            {
                if (triangle.PointInTriangle(point))
                {
                    // Split triangle into 3 triangles
                    newTriangles.Add(new Triangle(point, triangle.left, triangle.right));
                    newTriangles.Add(new Triangle(point, triangle.top, triangle.left));
                    newTriangles.Add(new Triangle(point, triangle.top, triangle.right));

                    // Remove the original triangle that was split
                    triangulation.Remove(triangle);

                    break;
                }
            }
            // Change triangles that don't meat condition

            for (int i = 0; i < newTriangles.Count; i++)
            {
                foreach (Triangle neighbor in GetNeighbors(triangulation, newTriangles[i]))
                {
                    Vector2 position = newTriangles[i].CircumCenter();
                    float radius = Vector2.Distance(position, newTriangles[i].vertices[0]);

                    Circle circumCircle = new(position, radius);

                    foreach (Vector2 vertice in neighbor.vertices)
                    {
                        if (newTriangles[i].vertices.Contains(vertice))
                        {
                            continue;
                        }

                        if (circumCircle.Intersects(vertice))
                        {
                            #region Get vertices
                            List<Vector2> vertices = new();
                            vertices.AddRange(newTriangles[i].vertices);
                            vertices.AddRange(neighbor.vertices);

                            Vector2? pointA = null, pointB = null, pointC = null, pointD = null;

                            for (int j = 0; j < vertices.Count; j++)
                            {
                                if (newTriangles[i].vertices.Contains(vertices[j]) && neighbor.vertices.Contains(vertices[j]))
                                {
                                    if (pointA == null)
                                    {
                                        pointA = vertices[j];
                                    }
                                    else if (pointB == null && vertices[j] != pointA)
                                    {
                                        pointB = vertices[j];
                                    }
                                }
                                else
                                {
                                    if (pointC == null)
                                    {
                                        pointC = vertices[j];
                                    }
                                    else if (pointD == null && vertices[j] != pointC)
                                    {
                                        pointD = vertices[j];
                                    }
                                }
                            }
                            #endregion

                            triangulation.Remove(neighbor);
                            badTriangles.Add(newTriangles[i]);

                            Triangle newTriangle1 = new(pointA.Value, pointC.Value, pointD.Value);
                            Triangle newTriangle2 = new(pointB.Value, pointC.Value, pointD.Value);

                            newTriangles.Add(newTriangle1);
                            newTriangles.Add(newTriangle2);
                        }
                    }
                }
            }

            foreach (Triangle badTriangle in badTriangles)
            {
                newTriangles.Remove(badTriangle);
            }

            triangulation.AddRange(newTriangles);
        }

        #region Remove super triangel from triangulation
        for (int i = triangulation.Count - 1; i >= 0; i--)
        {
            foreach (Vector2 vertice in superTriangle.vertices)
            {
                if (triangulation[i].ContainsPoint(vertice))
                {
                    triangulation.Remove(triangulation[i]);
                    break;
                }
            }
        }
        #endregion

        #region Debug
        GameObject triangulationGameObject = new()
        {
            name = "Triangulation"
        };

        for (int i = 0; i < triangulation.Count; i++)
        {
            GameObject debug = new()
            {
                name = "Triangle " + i
            };
            debug.transform.SetParent(triangulationGameObject.transform);

            LineRenderer ln = debug.AddComponent<LineRenderer>();
            ln.positionCount = 4;

            ln.SetPosition(0, new Vector3(triangulation[i].left.x, triangulation[i].left.y, -1));
            ln.SetPosition(1, new Vector3(triangulation[i].right.x, triangulation[i].right.y, -1));
            ln.SetPosition(2, new Vector3(triangulation[i].top.x, triangulation[i].top.y, -1));
            ln.SetPosition(3, new Vector3(triangulation[i].left.x, triangulation[i].left.y, -1));
        }
        #endregion

        return triangulation;
    }

    private List<Triangle> GetNeighbors(List<Triangle> list, Triangle currentTriangle)
    {
        List<Triangle> neighbors = new();
        int sharedVertices;

        foreach (Triangle triangle in list)
        {
            if (triangle.Equals(currentTriangle))
            {
                continue;
            }

            sharedVertices = 0;

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
    #endregion

    private void GenerateShortestSpanningTree(List<Triangle> triangulation, List<Vector2> pointList, float percentageOfLoops)
    {
        // Check for valid triangulation 
        if (triangulation.Count == 0 || percentageOfLoops < 0 || percentageOfLoops > 100)
        {
            return;
        }

        List<Edge> edges = new();

        foreach (Triangle triangle in triangulation)
        {
            foreach (Edge triangleEdge in triangle.edges)
            {
                bool canAdd = true;

                foreach (Edge edge in edges)
                {
                    if (edge.Equals(triangleEdge))
                    {
                        canAdd = false;
                    }
                }

                if (canAdd)
                {
                    edges.Add(triangleEdge);
                }
            }
        }

        // Check each point and get the next closest point  
        // *Important* use closed points list to avoid loops
        List<Edge> minimumSpanningTree = new();
        List<Vector2> pointsVisited = new();

        Heap<Edge> heap = new(edges.Count);

        for (int i = 0; i < edges.Count; i++)
        {
            heap.Add(edges[i]);
        }

        while (true)
        {
            Edge shortestEdge = heap.RemoveFirst();
            bool canAdd = true;

            if (pointsVisited.Contains(shortestEdge.pointA) && pointsVisited.Contains(shortestEdge.pointB))
            {
                #region Check for loop
                List<Edge> openEdges = new();
                List<Vector2> closedPoints = new()
                {
                    shortestEdge.pointA
                };

                foreach (Edge edge in minimumSpanningTree)
                {
                    if (edge.points.Contains(shortestEdge.pointA))
                    {
                        openEdges.Add(edge);
                    }
                }

                while (openEdges.Count > 0)
                {
                    for (int i = openEdges.Count - 1; i >= 0; i--)
                    {
                        if (!closedPoints.Contains(openEdges[i].pointA))
                        {
                            foreach (Edge edge in minimumSpanningTree)
                            {
                                if (edge.points.Contains(openEdges[i].pointA))
                                {
                                    if (edge.points.Contains(shortestEdge.pointB))
                                    {
                                        canAdd = false;
                                        break;
                                    }

                                    openEdges.Add(edge);
                                }
                            }

                            closedPoints.Add(openEdges[i].pointA);
                        }

                        if (!closedPoints.Contains(openEdges[i].pointB))
                        {
                            foreach (Edge edge in minimumSpanningTree)
                            {
                                if (edge.points.Contains(openEdges[i].pointB))
                                {
                                    if (edge.points.Contains(shortestEdge.pointB))
                                    {
                                        canAdd = false;
                                        break;
                                    }

                                    openEdges.Add(edge);
                                }
                            }

                            closedPoints.Add(openEdges[i].pointB);
                        }

                        openEdges.Remove(openEdges[i]);
                    }

                    if (!canAdd)
                    {
                        break;
                    }
                }
                #endregion
            }

            if (canAdd)
            {
                edges.Remove(shortestEdge);
                pointsVisited.AddRange(shortestEdge.points);
                minimumSpanningTree.Add(shortestEdge);
            }

            if (pointList.Count == minimumSpanningTree.Count + 1)
            {
                break;
            }
        }

        #region Debug
        GameObject DebugGameObject = new()
        {
            name = "MinimumSpanningTree"
        };

        for (int i = 0; i < minimumSpanningTree.Count; i++)
        {
            GameObject debug = new()
            {
                name = "Edge " + i
            };
            debug.transform.SetParent(DebugGameObject.transform);

            LineRenderer ln = debug.AddComponent<LineRenderer>();
            ln.positionCount = 2;

            //ln.SetPosition(0, new Vector3(edges[i].pointA.x, edges[i].pointA.y, -1));
            //ln.SetPosition(1, new Vector3(edges[i].pointB.x, edges[i].pointB.y, -1));
            ln.SetPosition(0, new Vector3(minimumSpanningTree[i].pointA.x, minimumSpanningTree[i].pointA.y, -1));
            ln.SetPosition(1, new Vector3(minimumSpanningTree[i].pointB.x, minimumSpanningTree[i].pointB.y, -1));
        }
        #endregion
    }

    private void GenerateCorridors(float width)
    {

    }

    private void PlaceWalls()
    {

    }

    #region Set position and tile lists
    private void SetData(MapGenerationManager manager, List<Room> rooms)
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            roomPositions.Add(rooms[i].WorldPosition);

            for (int x = 0; x < rooms[i].tiles.GetLength(0); x++)
            {
                for (int y = 0; y < rooms[i].tiles.GetLength(1); y++)
                {
                    TileChangeData data;

                    if (x == 0 && y == 0 || x == rooms[i].tiles.GetLength(0) - 1 && y == rooms[i].tiles.GetLength(1) - 1)
                    {
                        data = new((Vector3Int)rooms[i].tiles[x, y].gridPosition, manager.tileTexture, Color.black, Matrix4x4.identity);
                    }
                    else
                    {
                        data = new((Vector3Int)rooms[i].tiles[x, y].gridPosition, manager.tileTexture, Color.white, Matrix4x4.identity);
                    }

                    tileChanges.Add(data);
                }
            }
        }
    }
    #endregion
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

    public MapTile[,] tiles;

    public Room(int width, int height, Vector2 position)
    {
        this.width = width;
        this.height = height;
        tiles = new MapTile[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int xPosition = x + (int)position.x;
                int yPosition = y + (int)position.y;

                tiles[x, y] = new MapTile(new Vector2Int(xPosition, yPosition));
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

public struct MapTile
{
    public Vector2Int gridPosition;

    public MapTile(Vector2Int gridPosition)
    {
        this.gridPosition = gridPosition;
    }
}
#endregion

#region Edge class
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

    public readonly Edge edgeAB, edgeBC, edgeCA;
    public readonly Edge[] edges = new Edge[3];

    public Triangle(Vector2 a, Vector2 b, Vector2 c)
    {
        vertices[0] = a;
        vertices[1] = b;
        vertices[2] = c;

        edgeAB = new Edge(a, b);
        edgeBC = new Edge(b, c);
        edgeCA = new Edge(c, a);

        edges[0] = edgeAB;
        edges[1] = edgeBC;
        edges[2] = edgeCA;

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

    #region Circum cricle related methods
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
    #endregion

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
