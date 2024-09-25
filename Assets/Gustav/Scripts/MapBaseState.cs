using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

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
    public List<Room> rooms = new();
    public List<Room> mainRooms = new();

    private List<Triangle> triangulation = new();
    private List<Edge> minimumSpanningTree = new();

    private List<TileChangeData> tileChangeData = new();

    private GameObject triangulationDebug, minimumSpanningTreeDebug, pointsDebug;

    private bool canMoveRoom = false;

    System.Diagnostics.Stopwatch stopwatch;

    public override void EnterState(MapGenerationManager manager)
    {
        manager.AmountOfMainRooms = 12;
        Heap<Room> heap = new(manager.totalRoomsAmount);

        manager.tileMap.ClearAllTiles();
        GameObject.Destroy(triangulationDebug);
        GameObject.Destroy(minimumSpanningTreeDebug);
        GameObject.Destroy(pointsDebug);

        #region Reset lists
        rooms.Clear();
        mainRooms.Clear();
        triangulation.Clear();
        minimumSpanningTree.Clear();
        tileChangeData.Clear();
        #endregion

        // Generate x amount of rooms
        for (int i = 0; i < manager.totalRoomsAmount; i++)
        {
            Room newRoom = GenerateRoom(manager);

            rooms.Add(newRoom);
            heap.Add(newRoom);
        }

        // Add x amount (Amount of main rooms) with the largest area
        for (int i = 0; i < manager.AmountOfMainRooms; i++)
        {
            Room mainRoom = heap.RemoveFirst();

            mainRooms.Add(mainRoom);
        }

        #region Diagnostic 
        stopwatch = new();
        stopwatch.Start();
        #endregion
    }

    public override void UpdateState(MapGenerationManager manager)
    {
        SeparateRooms(manager);

        if(!canMoveRoom)
        {
            SetData(manager, mainRooms);

            triangulation = GenerateDelaunayTriangulation(manager, mainRooms);
            minimumSpanningTree = GetMinimumSpanningTree(triangulation, mainRooms, 10);
            GenerateHallways(minimumSpanningTree, 4);
            PlaceWalls();

            manager.SwitchState(manager.loadingState);
        }
    }

    public override void ExitState(MapGenerationManager manager)
    {
        for (int i = 0; i < tileChangeData.Count; i++)
        {
            // Might need to change to false
            manager.tileMap.SetTile(tileChangeData[i], true);
        }

        stopwatch.Stop();

        Debug.Log("It took: " + stopwatch.ElapsedMilliseconds + " MS, to generate map");
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
        canMoveRoom = false;

        for (int i = 0; i < rooms.Count; i++)
        {
            Vector2Int direction = GetDirection(manager, rooms[i]);

            rooms[i].MoveRoom(direction);

            if (direction != Vector2.zero)
            {
                canMoveRoom = true;
            }
        }
    }

    private bool RoomIntersects(MapGenerationManager manager, Room roomA, Room roomB)
    {
        if (roomA != roomB)
        {
            Vector2 roomALowerLeft = manager.tileMap.CellToWorld((Vector3Int)roomA.grid[0, 0].gridPosition);
            Vector2 roomATopRight = manager.tileMap.CellToWorld((Vector3Int)roomA.grid[roomA.width - 1, roomA.height - 1].gridPosition) + Vector3.one;

            Vector2 roomBLowerLeft = manager.tileMap.CellToWorld((Vector3Int)roomB.grid[0, 0].gridPosition);
            Vector2 roomBTopRight = manager.tileMap.CellToWorld((Vector3Int)roomB.grid[roomB.width - 1, roomB.height - 1].gridPosition) + Vector3.one;

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

        for (int i = 0; i < rooms.Count; i++)
        {
            if (Equals(rooms[i], room) || !RoomIntersects(manager, room, rooms[i]))
            {
                continue;
            }

            Vector2 otherPosition = rooms[i].WorldPosition;

            Vector2 otherAgentToCurrent = room.WorldPosition - otherPosition;
            Vector2 directionToTravel = otherAgentToCurrent.normalized;

            separationVelocity += directionToTravel;
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
    private List<Triangle> GenerateDelaunayTriangulation(MapGenerationManager manager, List<Room> rooms)
    {
        if (rooms.Count <= 3)
        {
            return new List<Triangle>();
        }

        List<Vector2> pointsList = new();

        pointsDebug = new()
        {
            name = "Points"
        };

        foreach (Room room in rooms)
        {
            GameObject temp = new();
            temp.transform.parent = pointsDebug.transform;
            temp.transform.position = room.WorldPosition;

            pointsList.Add(room.WorldPosition);
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
                    newTriangles.Add(new Triangle(point, triangle.pointA, triangle.pointB));
                    newTriangles.Add(new Triangle(point, triangle.pointC, triangle.pointA));
                    newTriangles.Add(new Triangle(point, triangle.pointC, triangle.pointB));

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

                    List<Vector2> sharedVertices = new();
                    List<Vector2> notSharedVertices = new();

                    foreach (Vector2 vertice in newTriangles[i].vertices)
                    {
                        if (neighbor.vertices.Contains(vertice))
                        {
                            sharedVertices.Add(vertice);
                        }
                        else
                        {
                            notSharedVertices.Add(vertice); 
                        }
                    }

                    foreach (Vector2 vertice in neighbor.vertices)
                    {
                        if (!newTriangles[i].vertices.Contains(vertice))
                        {
                            notSharedVertices.Add(vertice);
                        }
                    }

                    foreach (Vector2 vertice in notSharedVertices)
                    {
                        if (newTriangles[i].vertices.Contains(vertice))
                        {
                            continue;
                        }

                        if (circumCircle.Intersects(vertice))
                        {
                            triangulation.Remove(neighbor);
                            badTriangles.Add(newTriangles[i]);

                            Triangle newTriangle1 = new(sharedVertices[0], notSharedVertices[0], notSharedVertices[1]);
                            Triangle newTriangle2 = new(sharedVertices[1], notSharedVertices[0], notSharedVertices[1]);

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
        triangulationDebug = new()
        {
            name = "Triangulation"
        };

        for (int i = 0; i < triangulation.Count; i++)
        {
            GameObject debug = new()
            {
                name = "Triangle " + i
            };
            debug.transform.SetParent(triangulationDebug.transform);

            LineRenderer ln = debug.AddComponent<LineRenderer>();
            ln.positionCount = 4;

            ln.SetPosition(0, new Vector3(triangulation[i].pointA.x, triangulation[i].pointA.y, -1));
            ln.SetPosition(1, new Vector3(triangulation[i].pointB.x, triangulation[i].pointB.y, -1));
            ln.SetPosition(2, new Vector3(triangulation[i].pointC.x, triangulation[i].pointC.y, -1));
            ln.SetPosition(3, new Vector3(triangulation[i].pointA.x, triangulation[i].pointA.y, -1));
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

    #region Minimum spanning tree
    private List<Edge> GetMinimumSpanningTree(List<Triangle> triangulation, List<Room> rooms, float percentageOfLoops)
    {
        // Check for valid triangulation 
        if (triangulation.Count == 0 || percentageOfLoops < 0 || percentageOfLoops > 100)
        {
            return new List<Edge>();
        }

        List<Edge> edgeList = new();

        foreach (Triangle triangle in triangulation)
        {
            foreach (Edge triangleEdge in triangle.edges)
            {
                bool canAdd = true;

                foreach (Edge edge in edgeList)
                {
                    if (edge.Equals(triangleEdge))
                    {
                        canAdd = false;
                    }
                }

                if (canAdd)
                {
                    edgeList.Add(triangleEdge);
                }
            }
        }

        // Check each point and get the next closest point  
        // *Important* use closed points list to avoid loops
        List<Edge> minimumSpanningTree = new();
        List<Vector2> pointsVisited = new();

        //List<Edge> openEdges = new();
        //List<Vector2> closedPoints = new();

        Heap<Edge> heap = new(edgeList.Count);

        for (int i = 0; i < edgeList.Count; i++)
        {
            heap.Add(edgeList[i]);
        }

        while (true)
        {
            if (heap.Count == 0)
            {
                Debug.Log("Error");
                break;
            }

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
                edgeList.Remove(shortestEdge);
                pointsVisited.AddRange(shortestEdge.points);
                minimumSpanningTree.Add(shortestEdge);
            }

            if (rooms.Count == minimumSpanningTree.Count + 1)
            {
                break;
            }
        }

        #region Debug
        minimumSpanningTreeDebug = new()
        {
            name = "MinimumSpanningTree"
        };

        for (int i = 0; i < minimumSpanningTree.Count; i++)
        {
            GameObject debug = new()
            {
                name = "Edge " + i
            };
            debug.transform.SetParent(minimumSpanningTreeDebug.transform);

            LineRenderer ln = debug.AddComponent<LineRenderer>();
            ln.positionCount = 2;

            //ln.SetPosition(0, new Vector3(edges[i].pointA.x, edges[i].pointA.y, -1));
            //ln.SetPosition(1, new Vector3(edges[i].pointB.x, edges[i].pointB.y, -1));
            ln.SetPosition(0, new Vector3(minimumSpanningTree[i].pointA.x, minimumSpanningTree[i].pointA.y, -1));
            ln.SetPosition(1, new Vector3(minimumSpanningTree[i].pointB.x, minimumSpanningTree[i].pointB.y, -1));
        }
        #endregion

        return minimumSpanningTree;
    }
    #endregion

    #region Hallways generation
    private void GenerateHallways(List<Edge> connections, float width)
    {
        List<Edge> test = new()
        {
            connections[0]
        };

        foreach (Edge connection in connections)
        {
            List<Room> roomList = new();

            foreach (Room room in mainRooms)
            {
                if (room.WorldPosition == connection.pointA || room.WorldPosition == connection.pointB)
                {
                    roomList.Add(room);

                    if (roomList.Count == 2)
                    {
                        break;
                    }
                }
            }

            #region Start and target positions
            Vector2Int startingPosition = Vector2Int.zero; // From room[0]
            Vector2Int targetPosition = Vector2Int.zero; // Towards room[1]

            if (roomList[0].WorldPosition.x + roomList[0].width / 2 < roomList[1].WorldPosition.x)
            {
                startingPosition.x = roomList[0].grid[roomList[0].width - 1, 0].gridPosition.x;
                targetPosition.x = roomList[1].grid[0, 0].gridPosition.x;
            }
            else if (roomList[0].WorldPosition.x - roomList[0].width / 2 > roomList[1].WorldPosition.x)
            {
                startingPosition.x = roomList[0].grid[0, 0].gridPosition.x;
                targetPosition.x = roomList[1].grid[roomList[1].width - 1, 0].gridPosition.x;
            }
            else
            {
                startingPosition.x = roomList[0].grid[roomList[0].width / 2 - 1, 0].gridPosition.x;
                targetPosition.x = roomList[1].grid[roomList[1].width / 2 - 1, 0].gridPosition.x;
            }

            if (roomList[0].WorldPosition.y + roomList[0].height / 2 < roomList[1].WorldPosition.y)
            {
                startingPosition.y = roomList[0].grid[0, roomList[0].height - 1].gridPosition.y;
                targetPosition.y = roomList[1].grid[0, 0].gridPosition.y;
            }
            else if (roomList[0].WorldPosition.y - roomList[0].height / 2 > roomList[1].WorldPosition.y)
            {
                startingPosition.y = roomList[0].grid[0, 0].gridPosition.y;
                targetPosition.y = roomList[1].grid[0, roomList[1].height - 1].gridPosition.y;
            }
            else
            {
                startingPosition.y = roomList[0].grid[0, roomList[0].height / 2 - 1].gridPosition.y;
                targetPosition.y = roomList[1].grid[0, roomList[1].height / 2 - 1].gridPosition.y;
            }
            #endregion

            // Find a path starting from first room in list towards the connected room.

            List<Vector2Int> hallwayTilePositions = AStar.instance.FindPath(startingPosition, targetPosition);

            //GameObject path = new()
            //{
            //    name = "Path"
            //};

            //foreach (Vector2Int vector in hallwayTilePositions)
            //{
            //    GameObject g = new();
            //    g.transform.parent = path.transform;
            //    g.transform.position = (Vector2)vector;
            //}

            // Add width to the "path", then add path to tile change data list.

            List<Vector2Int> widthTiles = new();

            foreach (Vector2Int vector in hallwayTilePositions)
            {
                for (int x = (int)-width / 2; x < width; x++)
                {
                    for (int y = (int)-width / 2; y < width; y++)
                    {
                        Vector2Int position = new (vector.x + x, vector.y + y);

                        if (!hallwayTilePositions.Contains(position))
                        {
                            widthTiles.Add(position);
                        }
                    }
                }
            }

            hallwayTilePositions.AddRange(widthTiles);

            foreach (Vector2Int vector in hallwayTilePositions)
            {
                TileChangeData data = new((Vector3Int)vector, MapGenerationManager.instance.tileTexture, Color.white, Matrix4x4.identity);
                tileChangeData.Add(data);
            }

            #region Room intersection check
            //foreach (Room room in rooms)
            //{
            //    if (mainRooms.Contains(room))
            //    {
            //        continue;
            //    }

            //    // Add rooms that collide with the hallways

            //    bool canAdd = false;

            //    for (int i = 0; i < room.tiles.Count; i++)
            //    {
            //        if (hallwayTilePositions.Contains(room.tiles[i].gridPosition))
            //        {
            //            // Add room to a list

            //            canAdd = true;
            //            Debug.Log("True");
            //            break;
            //        }
            //    }

            //    if (canAdd)
            //    {
            //        for (int x = 0; x < room.width; x++)
            //        {
            //            for (int y = 0; y < room.height; y++)
            //            {
            //                Vector2Int vector = room.grid[x, y].gridPosition;

            //                TileChangeData data = new((Vector3Int)vector, MapGenerationManager.instance.tileTexture, Color.white, Matrix4x4.identity);
            //                tileChangeData.Add(data);
            //            }
            //        }
            //    }
            //}
            #endregion
        }
    }
    #endregion

    private void PlaceWalls()
    {

    }

    private void Decorate()
    {

    }

    #region Set position and tile lists
    private void SetData(MapGenerationManager manager, List<Room> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            for (int x = 0; x < list[i].grid.GetLength(0); x++)
            {
                for (int y = 0; y < list[i].grid.GetLength(1); y++)
                {
                    TileChangeData data;

                    if (x == 0 && y == 0 || x == list[i].grid.GetLength(0) - 1 && y == 0 || x == 0 && y == list[i].grid.GetLength(1) - 1 || x == list[i].grid.GetLength(0) - 1 && y == list[i].grid.GetLength(1) - 1)
                    {
                        data = new((Vector3Int)list[i].grid[x, y].gridPosition, manager.tileTexture, Color.black, Matrix4x4.identity);
                    }
                    else
                    {
                        data = new((Vector3Int)list[i].grid[x, y].gridPosition, manager.tileTexture, Color.white, Matrix4x4.identity);
                    }

                    tileChangeData.Add(data);
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
            Vector2 worldPosition = MapGenerationManager.instance.tileMap.CellToWorld((Vector3Int)grid[width - 1, height - 1].gridPosition) + Vector3.one;

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

    public MapTile[,] grid;
    public readonly List<MapTile> tiles = new();

    public Room(int width, int height, Vector2 position)
    {
        this.width = width;
        this.height = height;
        grid = new MapTile[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int xPosition = x + (int)position.x;
                int yPosition = y + (int)position.y;

                grid[x, y] = new MapTile(new Vector2Int(xPosition, yPosition));
                tiles.Add(grid[x, y]);
            }
        }
    }

    public void MoveRoom(Vector2Int direction)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y].gridPosition += direction;
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
    public Vector2 pointA, pointB, pointC;
    public readonly Vector2[] vertices = new Vector2[3];

    public readonly Edge edgeAB, edgeBC, edgeCA;
    public readonly Edge[] edges = new Edge[3];

    public Triangle(Vector2 a, Vector2 b, Vector2 c)
    {
        pointA = a;
        pointB = b;
        pointC = c;

        vertices[0] = pointA;
        vertices[1] = pointB;
        vertices[2] = pointC;

        edgeAB = new Edge(a, b);
        edgeBC = new Edge(b, c);
        edgeCA = new Edge(c, a);

        edges[0] = edgeAB;
        edges[1] = edgeBC;
        edges[2] = edgeCA;
    }

    public bool ContainsPoint(Vector2 point)
    {
        return vertices.Contains(point);
    }

    float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }

    public bool PointInTriangle(Vector2 pt)
    {
        Vector2 v1 = pointA;
        Vector2 v2 = pointB;
        Vector2 v3 = pointC;

        float d1, d2, d3;
        bool has_neg, has_pos;

        d1 = Sign(pt, v1, v2);
        d2 = Sign(pt, v2, v3);
        d3 = Sign(pt, v3, v1);

        has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(has_neg && has_pos);
    }

    #region Circum circle related methods
    public Vector2 CircumCenter()
    {
        List<double> a = new()
        {
            pointA.x,
            pointA.y
        };
        List<double> b = new()
        {
            pointB.x,
            pointB.y
        };
        List<double> c = new()
        {
            pointC.x,
            pointC.y
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
}
#endregion
