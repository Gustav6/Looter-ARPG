using System;
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

    private HashSet<Vector3Int> groundTilePositions = new();
    private HashSet<Vector2Int> edgeTiles = new();

    private GameObject triangulationDebug, minimumSpanningTreeDebug, pointsDebug;

    private bool canMoveRoom;

    private Room startingRoom;

    System.Diagnostics.Stopwatch stopwatch;
    float diagnosticTime;


    public override void EnterState(MapGenerationManager manager)
    {
        #region Diagnostic Start
        stopwatch = new();
        stopwatch.Start();
        #endregion

        #region Debug game objects
        GameObject.Destroy(triangulationDebug);
        GameObject.Destroy(minimumSpanningTreeDebug);
        GameObject.Destroy(pointsDebug);
        #endregion

        Reset(manager);

        Heap<Room> heap = new(manager.Settings.totalRoomsCount);

        // Generate x amount of rooms
        for (int i = 0; i < manager.Settings.totalRoomsCount; i++)
        {
            Room newRoom = GenerateRoom(manager);

            rooms.Add(newRoom);
            heap.Add(newRoom);
        }

        // Add x amount (Amount of main rooms) with the largest area
        for (int i = 0; i < manager.Settings.AmountOfMainRooms; i++)
        {
            Room mainRoom = heap.RemoveFirst();

            mainRooms.Add(mainRoom);
        }

        startingRoom = mainRooms.First();
        diagnosticTime = stopwatch.ElapsedMilliseconds;
    }

    public override void UpdateState(MapGenerationManager manager)
    {
        SeparateRooms(manager);

        if (!canMoveRoom)
        {
            Debug.Log("It took: " + (stopwatch.ElapsedMilliseconds - diagnosticTime) + " MS, to separate rooms");

            diagnosticTime = stopwatch.ElapsedMilliseconds;
            triangulation = GenerateDelaunayTriangulation(manager, mainRooms);
            Debug.Log("It took: " + (stopwatch.ElapsedMilliseconds - diagnosticTime) + " MS, to triangulate points");

            diagnosticTime = stopwatch.ElapsedMilliseconds;
            minimumSpanningTree = GetMinimumSpanningTree(manager, triangulation, mainRooms, manager.Settings.amountOfLoops);
            Debug.Log("It took: " + (stopwatch.ElapsedMilliseconds - diagnosticTime) + " MS, to get minimum spanning tree");

            diagnosticTime = stopwatch.ElapsedMilliseconds;
            GenerateHallways(manager, minimumSpanningTree, manager.Settings.hallwayWidth);
            Debug.Log("It took: " + (stopwatch.ElapsedMilliseconds - diagnosticTime) + " MS, to generate hallways");

            diagnosticTime = stopwatch.ElapsedMilliseconds;

            foreach (Room room in mainRooms)
            {
                foreach (Vector2Int? position in room.tiles)
                {
                    if (position != null)
                    {
                        groundTilePositions.Add((Vector3Int)position.Value);
                    }
                }
            }

            Debug.Log("It took: " + (stopwatch.ElapsedMilliseconds - diagnosticTime) + " MS, to add room tiles");

            manager.SwitchState(manager.loadingState);
        }
    }

    public override void ExitState(MapGenerationManager manager)
    {
        diagnosticTime = stopwatch.ElapsedMilliseconds;

        TileBase[] tiles = new TileBase[groundTilePositions.Count];
        Array.Fill(tiles, manager.tilePairs[TileTexture.ruleTile]);

        manager.Settings.groundTileMap.SetTiles(groundTilePositions.ToArray(), tiles);

        Debug.Log("It took: " + (stopwatch.ElapsedMilliseconds - diagnosticTime) + " MS, to set ground tiles");

        Debug.Log("Tile changed: " + groundTilePositions.Count);

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            player.transform.position = startingRoom.WorldPosition;
        }

        #region Diagnostic End
        stopwatch.Stop();

        Debug.Log("It took a total of: " + stopwatch.ElapsedMilliseconds + " MS, to generate map");
        #endregion
    }

    #region Generate Room methods
    private Room GenerateRoom(MapGenerationManager manager)
    {
        Room room = null;
        Vector2Int offset;
        Vector2 position = Vector2.zero;

        int roomWidth = UnityEngine.Random.Range(manager.Settings.SquaredRoomMinSize.x, manager.Settings.squaredRoomMaxSize.x + 1);
        int roomHeight = UnityEngine.Random.Range(manager.Settings.SquaredRoomMinSize.y, manager.Settings.squaredRoomMaxSize.y + 1);
        offset = new(roomWidth / 2, roomHeight / 2);

        if (manager.Settings.generateInCircle)
        {
            position = RandomPositionInCircle(manager.Settings.generationRadius) - offset;
        }
        else if (manager.Settings.generateInStrip)
        {
            position = RandomPositionInStrip(manager.Settings.stripSize.x, manager.Settings.stripSize.y) - offset;
        }

        room = new Room(roomWidth, roomHeight, position, manager.Settings.roundCorners);

        return room;
    }

    public Vector2 RandomPositionInCircle(float radius)
    {
        float r = radius * Mathf.Sqrt(UnityEngine.Random.Range(0.0001f, 1));
        float theta = UnityEngine.Random.Range(0.0001f, 1) * 2 * Mathf.PI;

        return new Vector2(r * Mathf.Cos(theta), r * Mathf.Sin(theta));
    }

    public Vector2 RandomPositionInStrip(float width, float height)
    {
        float x = UnityEngine.Random.Range(-width / 2, width / 2);
        float y = UnityEngine.Random.Range(-height / 2, height / 2);

        return new Vector2(x, y);
    }
    #endregion

    #region Seperate room methods

    private void SeparateRooms(MapGenerationManager manager)
    {
        canMoveRoom = false;

        foreach (Room room in rooms)
        {
            Vector2Int direction = GetDirection(manager, room);

            if (direction != Vector2.zero)
            {
                room.MoveRoom(direction);

                if (!canMoveRoom)
                {
                    canMoveRoom = true;
                }
            }
        }
    }

    private bool RoomIntersects(Room roomA, Room roomB)
    {
        if (roomA != roomB)
        {
            if (roomA.BottomLeft.x < roomB.TopRight.x && roomA.TopRight.x > roomB.BottomLeft.x)
            {
                if (roomA.BottomLeft.y < roomB.TopRight.y && roomA.TopRight.y > roomB.BottomLeft.y)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public Vector2Int GetDirection(MapGenerationManager manager, Room currentRoom)
    {
        Vector2 separationVelocity = Vector2.zero;
        Vector2 otherPosition, otherAgentToCurrent, directionToTravel;

        foreach (Room room in rooms)
        {
            if (!RoomIntersects(currentRoom, room))
            {
                continue;
            }

            otherPosition = room.WorldPosition;

            otherAgentToCurrent = currentRoom.WorldPosition - otherPosition;
            directionToTravel = otherAgentToCurrent.normalized;

            separationVelocity += directionToTravel;
        }

        if (separationVelocity == Vector2Int.zero)
        {
            return Vector2Int.zero;
        }
        else
        {
            if (separationVelocity.x > 0 && separationVelocity.x < 1)
            {
                separationVelocity.x = 1;
            }
            else if (separationVelocity.x < 0 && separationVelocity.x > -1)
            {
                separationVelocity.x = -1;
            }

            if (separationVelocity.y > 0 && separationVelocity.y < 1)
            {
                separationVelocity.y = 1;
            }
            else if (separationVelocity.y < 0 && separationVelocity.y > -1)
            {
                separationVelocity.y = -1;
            }

            return new Vector2Int((int)separationVelocity.x, (int)separationVelocity.y);
        }
    }
    #endregion

    #region Delaunay triangulation
    private List<Triangle> GenerateDelaunayTriangulation(MapGenerationManager manager, List<Room> rooms)
    {
        if (rooms.Count <= 3)
        {
            return new List<Triangle>();
        }

        Vector2[] points = new Vector2[rooms.Count];

        for (int i = 0; i < rooms.Count; i++)
        {
            points[i] = rooms[i].WorldPosition;
        }

        List<Triangle> triangulation = new();

        #region Super Triangle
        Vector2 a, b, c;

        if (manager.Settings.generationRadius <= 0)
        {
            manager.Settings.generationRadius = 1;
        }

        a = new Vector2(-manager.Settings.generationRadius, -manager.Settings.generationRadius) * 100;
        b = new Vector2(manager.Settings.generationRadius, -manager.Settings.generationRadius) * 100;
        c = new Vector2(0, manager.Settings.generationRadius) * 100;

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

        triangulation.Add(superTriangle);
        #endregion

        List<Triangle> newTriangles = new();
        List<Triangle> badTriangles = new();

        foreach (Vector2 point in points)
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
        if (manager.Settings.debugTriangulation)
        {
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
    private List<Edge> GetMinimumSpanningTree(MapGenerationManager manager, List<Triangle> triangulation, List<Room> rooms, float percentageOfLoops)
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

        List<Edge> openEdges = new();
        List<Vector2> closedPoints = new();
        Edge shortestEdge;

        Heap<Edge> heap = new(edgeList.Count);

        for (int i = 0; i < edgeList.Count; i++)
        {
            heap.Add(edgeList[i]);
        }

        while (true)
        {
            if (heap.Count == 0)
            {
                Debug.Log("Error with heap");
                break;
            }

            shortestEdge = heap.RemoveFirst();
            bool canAdd = true;

            if (pointsVisited.Contains(shortestEdge.pointA) && pointsVisited.Contains(shortestEdge.pointB))
            {
                #region Check for loop

                openEdges.Clear();
                closedPoints.Clear();
                closedPoints.Add(shortestEdge.pointA);

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

        if (percentageOfLoops > 0)
        {
            List<Edge> loopedEdges = new();
            float count = 0;

            while (true)
            {
                Edge current = edgeList[UnityEngine.Random.Range(0, edgeList.Count)];

                if (!loopedEdges.Contains(current))
                {
                    loopedEdges.Add(current);
                    count++;
                }

                if (count / edgeList.Count >= percentageOfLoops / 100)
                {
                    break;
                }
            }

            minimumSpanningTree.AddRange(loopedEdges);
        }

        #region Debug
        if (manager.Settings.debugSpanningTree)
        {
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
        }
        #endregion

        return minimumSpanningTree;
    }
    #endregion

    #region Hallways generation
    private void GenerateHallways(MapGenerationManager manager, List<Edge> connections, float hallwayWidth)
    {
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

            Vector3Int startingPosition = new ((int)roomList[0].WorldPosition.x, (int)roomList[0].WorldPosition.y); // From roomList[0]
            Vector3Int targetPosition = new((int)roomList[1].WorldPosition.x, (int)roomList[1].WorldPosition.y); // Towards roomList[1]

            // Find a path starting from first room in list towards the connected room.

            HashSet<Vector3Int> hallwayTilePositions = new (AStar.FindPath(startingPosition, targetPosition));

            // *Working*s but not very good looking
            #region Room intersection check
            //foreach (Room room in rooms)
            //{
            //    if (mainRooms.Contains(room))
            //    {
            //        continue;
            //    }

            //    // Add rooms that collide with the hallways

            //    bool canAdd = false;

            //    for (int x = 0; x < room.grid.GetLength(0); x++)
            //    {
            //        for (int y = 0; y < room.grid.GetLength(1); y++)
            //        {
            //            if (room.grid[x, y].gridPosition != null && hallwayTilePositions.Contains(room.grid[x, y].gridPosition.Value))
            //            {
            //                canAdd = true;
            //                break;
            //            }
            //        }

            //        if (canAdd)
            //        {
            //            break;
            //        }
            //    }

            //    if (canAdd)
            //    {
            //        mainRooms.Add(room);
            //    }
            //}
            #endregion

            // Add width to the "path", then add path to tile change data list.
            #region Hallway width
            HashSet<Vector3Int> widthTiles = new();

            foreach (Vector3Int vector in hallwayTilePositions)
            {
                for (int x = (int)-hallwayWidth / 2; x < hallwayWidth; x++)
                {
                    for (int y = (int)-hallwayWidth / 2; y < hallwayWidth; y++)
                    {
                        widthTiles.Add(new (vector.x + x, vector.y + y));
                    }
                }
            }

            hallwayTilePositions.UnionWith(widthTiles);
            #endregion

            groundTilePositions.UnionWith(hallwayTilePositions);
        }
    }
    #endregion

    private void Decorate()
    {

    }

    private void Reset(MapGenerationManager manager)
    {
        manager.Settings.groundTileMap.ClearAllTiles();
        rooms.Clear();
        mainRooms.Clear();
        triangulation.Clear();
        minimumSpanningTree.Clear();
        groundTilePositions.Clear();
        edgeTiles.Clear();

        canMoveRoom = false;
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
            return center;
        }
    }

    public Vector2 BottomLeft
    {
        get
        {
            return new Vector2(WorldPosition.x - width / 2, WorldPosition.y - height / 2);
        }
    }
    public Vector2 TopRight
    {
        get
        {
            return new Vector2(WorldPosition.x + width / 2 + 1, WorldPosition.y + height / 2 + 1);
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

    //public MapTile[,] grid;
    public Vector2Int?[] tiles;
    public HashSet<Vector2Int> tilePositions = new();
    public readonly int tileCount = new();

    private Circle c1, c2;

    #region Square Room
    public Room(int width, int height, Vector2 position, bool roundCorners = false)
    {
        this.width = width;
        this.height = height;
        c1 = null;
        c2 = null;

        tiles = new Vector2Int?[width * height];

        #region Set Circles
        if (roundCorners)
        {
            float r = 0;
            Vector2 temp1 = Vector2.zero, temp2 = Vector2.zero;

            if (width < height)
            {
                r = width / 2f;
                temp1 = new Vector2((int)position.x + width / 2f, (int)position.y + height - height / 3f);
                temp2 = new Vector2((int)position.x + width / 2f, (int)position.y + height / 3f);
            }
            else if (width >= height)
            {
                r = height / 2f;
                temp1 = new Vector2((int)position.x + width - width / 3f, (int)position.y + height / 2f);
                temp2 = new Vector2((int)position.x + width / 3f, (int)position.y + height / 2f);
            }

            c1 = new Circle(temp1, r);
            c2 = new Circle(temp2, r);
        }
        #endregion

        for (int i = 0; i < tiles.Length; i++)
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
                tiles[i] = new Vector2Int(xPosition, yPosition);
                tileCount++;
            }
        }

        center = new Vector2((int)position.x + width / 2f, (int)position.y + height / 2f);
    }
    #endregion

    public void MoveRoom(Vector2Int direction)
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i] != null)
            {
                tiles[i] += direction;
            }
        }

        center += direction;
    }

    public int CompareTo(Room other)
    {
        int compare = tileCount.CompareTo(other.tileCount);

        return compare;
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
