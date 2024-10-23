using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

#region Base State
public abstract class MapBaseState
{
    public abstract void EnterState(MapManager manager);
    public abstract void UpdateState(MapManager manager);
    public abstract void ExitState(MapManager manager);
}
#endregion

#region Generation State
public class GeneratingMapState : MapBaseState
{
    public Room[] tempRoomList;
    public List<Room> mainRooms = new();

    private List<Triangle> triangulation = new();

    private HashSet<Vector3Int> groundTilePositions = new(), wallTilePositions = new();

    private GameObject triangulationDebug, minimumSpanningTreeDebug;

    private bool canMoveRoom;

    System.Diagnostics.Stopwatch stopwatch;
    float diagnosticTime;

    private System.Random rng;


    public override void EnterState(MapManager manager)
    {
        #region Diagnostic Start
        stopwatch = new();
        stopwatch.Start();
        #endregion

        #region Debug game objects
        GameObject.Destroy(triangulationDebug);
        GameObject.Destroy(minimumSpanningTreeDebug);
        #endregion

        rng = new System.Random(manager.Settings.seed);

        canMoveRoom = false;

        mainRooms.Clear();
        triangulation.Clear();
        groundTilePositions.Clear();
        wallTilePositions.Clear();

        tempRoomList = new Room[manager.Settings.totalRoomsCount];
        Heap<Room> roomHeapForSize = new(manager.Settings.totalRoomsCount);

        // Generate x amount of rooms
        for (int i = 0; i < manager.Settings.totalRoomsCount; i++)
        {
            Room newRoom = GenerateRoom(manager);

            tempRoomList[i] = newRoom;
            roomHeapForSize.Add(newRoom);
        }

        // Add x amount (Amount of main rooms) with the largest area
        for (int i = 0; i < manager.Settings.AmountOfMainRooms; i++)
        {
            mainRooms.Add(roomHeapForSize.RemoveFirst());
        }

        manager.startingRoom = mainRooms.First();

        diagnosticTime = stopwatch.ElapsedMilliseconds;
    }

    public override void UpdateState(MapManager manager)
    {
        SeparateRooms(manager);

        if (!canMoveRoom)
        {
            Debug.Log("It took: " + (stopwatch.ElapsedMilliseconds - diagnosticTime) + " MS, to separate rooms");

            manager.SwitchState(manager.loadedState);
        }
    }

    public override void ExitState(MapManager manager)
    {
        foreach (Room room in mainRooms)
        {
            foreach (Vector3Int? position in room.groundTiles.Select(v => (Vector3Int?)v))
            {
                if (position != null)
                {
                    groundTilePositions.Add(position.Value);
                }
            }

            wallTilePositions.UnionWith(room.walls);

            float[][,] noiseMaps = new float[manager.Settings.amountOfNoiseLoops][,];

            for (int i = 0; i < noiseMaps.Length; i++)
            {
                float xOffset = rng.Next(-100000, 100000);
                float yOffset = rng.Next(-100000, 100000);

                noiseMaps[i] = NoiseMapGenerator.GenerateMap(room.width, room.height, manager.Settings.seed, manager.Settings.noiseScale, manager.Settings.octaves, manager.Settings.persistence, manager.Settings.lacunarity, new(xOffset, yOffset));
            }

            foreach (float[,] noiseMap in noiseMaps)
            {
                for (int x = 0; x < noiseMap.GetLength(0); x++)
                {
                    for (int y = 0; y < noiseMap.GetLength(1); y++)
                    {
                        Vector3Int prefabPosition = new(x - (room.width / 2) + (int)room.center.x, y - (room.height / 2) + (int)room.center.y);

                        if (DestructibleManager.Instance.BreakablePositions.Contains(prefabPosition) || TrapManager.Instance.TrapsPositions.Contains(prefabPosition))
                        {
                            continue;
                        }

                        if (groundTilePositions.Contains(prefabPosition))
                        {
                            float currentHeight1 = noiseMap[x, y];

                            for (int i = 0; i < manager.Settings.regions.Length; i++)
                            {
                                if (currentHeight1 <= manager.Settings.regions[i].heightValue)
                                {
                                    if (manager.Settings.regions[i].prefab == null)
                                    {
                                        Debug.Log("Error within noise array, on position: " + i);
                                        continue;
                                    }

                                    if (manager.Settings.regions[i].prefab.CompareTag("Trap"))
                                    {
                                        TrapManager.Instance.AddTrap(prefabPosition, manager.Settings.regions[i].prefab, room);
                                    }
                                    else if (manager.Settings.regions[i].prefab.CompareTag("Destructible"))
                                    {
                                        DestructibleManager.Instance.AddBreakable(prefabPosition, manager.Settings.regions[i].prefab, room);
                                    }
                                    else
                                    {
                                        Debug.Log("Error with prefab no tag found: " + manager.Settings.regions[i].prefab);
                                    }

                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        manager.rooms = mainRooms.ToArray();

        #region Methods for generation
        diagnosticTime = stopwatch.ElapsedMilliseconds;
        triangulation = GenerateDelaunayTriangulation(manager, mainRooms);
        Debug.Log("It took: " + (stopwatch.ElapsedMilliseconds - diagnosticTime) + " MS, to triangulate points");

        diagnosticTime = stopwatch.ElapsedMilliseconds;

        manager.minimumSpanningTree = GetMinimumSpanningTree(manager);
        Debug.Log("It took: " + (stopwatch.ElapsedMilliseconds - diagnosticTime) + " MS, to get minimum spanning tree");

        diagnosticTime = stopwatch.ElapsedMilliseconds;
        GenerateHallways(manager);
        Debug.Log("It took: " + (stopwatch.ElapsedMilliseconds - diagnosticTime) + " MS, to generate hallways");
        #endregion

        #region Set ground tiles
        diagnosticTime = stopwatch.ElapsedMilliseconds;

        TileBase[] tempArray = new TileBase[groundTilePositions.Count];
        Array.Fill(tempArray, manager.tilePairs[TileTexture.ground]);

        manager.groundTileMap.SetTiles(groundTilePositions.ToArray(), tempArray);

        Debug.Log("It took: " + (stopwatch.ElapsedMilliseconds - diagnosticTime) + " MS, to set ground tiles");
        #endregion

        #region Set wall tiles
        diagnosticTime = stopwatch.ElapsedMilliseconds;

        tempArray = new TileBase[wallTilePositions.Count];
        Array.Fill(tempArray, manager.tilePairs[TileTexture.wall]);

        manager.wallTileMap.SetTiles(wallTilePositions.ToArray(), tempArray);

        Debug.Log("It took: " + (stopwatch.ElapsedMilliseconds - diagnosticTime) + " MS, to set wall tiles");
        #endregion

        #region Diagnostic End
        stopwatch.Stop();

        Debug.Log("It took a total of: " + stopwatch.ElapsedMilliseconds + " MS, to generate map");
        #endregion
    }

    #region Generate Room methods
    private Room GenerateRoom(MapManager manager)
    {
        Room room = null;
        Vector2Int offset;
        Vector2 position = Vector2.zero;

        int roomWidth = rng.Next(manager.Settings.RoomMinSize.x, manager.Settings.roomMaxSize.x + 1);
        int roomHeight = rng.Next(manager.Settings.RoomMinSize.y, manager.Settings.roomMaxSize.y + 1);
        offset = new(roomWidth / 2, roomHeight / 2);

        if (manager.Settings.spawnFunction == SpawnFunction.Circle)
        {
            position = RandomPositionInCircle(manager.Settings.generationRadius) - offset;
        }
        else if (manager.Settings.spawnFunction == SpawnFunction.Strip)
        {
            position = RandomPositionInStrip(manager.Settings.stripSize.x, manager.Settings.stripSize.y) - offset;
        }

        room = new Room(roomWidth, roomHeight, position, manager.Settings.roundCorners);

        return room;
    }

    public Vector2 RandomPositionInCircle(float radius)
    {
        float r = radius * Mathf.Sqrt((float)rng.NextDouble());
        float theta = (float)rng.NextDouble() * 2 * Mathf.PI;

        return new Vector2(r * Mathf.Cos(theta), r * Mathf.Sin(theta));
    }

    public Vector2 RandomPositionInStrip(int width, int height)
    {
        int x = rng.Next(-width / 2, width / 2);
        int y = rng.Next(-height / 2, height / 2);

        return new Vector2(x, y);
    }
    #endregion

    #region Seperate room methods

    private void SeparateRooms(MapManager manager)
    {
        canMoveRoom = false;

        foreach (Room room in tempRoomList)
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

    public Vector2Int GetDirection(MapManager manager, Room currentRoom)
    {
        Vector2 separationVelocity = Vector2.zero;
        Vector2 otherPosition, otherAgentToCurrent, directionToTravel;

        foreach (Room room in tempRoomList)
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
    private List<Triangle> GenerateDelaunayTriangulation(MapManager manager, List<Room> rooms)
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
    private List<Edge> GetMinimumSpanningTree(MapManager manager)
    {
        // Check for valid triangulation 
        if (triangulation.Count == 0 || manager.Settings.amountOfHallwayLoops < 0 || manager.Settings.amountOfHallwayLoops > 100)
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

            if (manager.rooms.Length == minimumSpanningTree.Count + 1)
            {
                break;
            }
        }

        if (manager.Settings.amountOfHallwayLoops > 0)
        {
            List<Edge> loopedEdges = new();
            float count = 0;

            while (true)
            {
                Edge current = edgeList[rng.Next(0, edgeList.Count)];

                if (!loopedEdges.Contains(current))
                {   
                    loopedEdges.Add(current);
                    count++;
                }

                if (count / edgeList.Count >= manager.Settings.amountOfHallwayLoops / 100)
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
    private void GenerateHallways(MapManager manager)
    {
        int hallwayWidth = manager.Settings.hallwayWidth;

        foreach (Edge connection in manager.minimumSpanningTree)
        {
            if (manager.Settings.randomizedHallwaySize)
            {
                hallwayWidth = rng.Next(manager.Settings.hallwayMinWidth, manager.Settings.hallwayMaxWidth);
            }

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

            List<Vector3Int> hallwayTilePositions = new (AStar.FindPath(startingPosition, targetPosition));

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

            for (int i = 0; i < hallwayTilePositions.Count; i++)
            {
                if (i + 1 < hallwayTilePositions.Count)
                {
                    if (hallwayTilePositions[i].x == hallwayTilePositions[i + 1].x)
                    {
                        for (int x = -hallwayWidth + 1; x < hallwayWidth; x++)
                        {
                            widthTiles.Add(new(hallwayTilePositions[i].x + x, hallwayTilePositions[i].y));
                        }

                        for (int j = 0; j < 35; j++)
                        {
                            wallTilePositions.Add(new(hallwayTilePositions[i].x + hallwayWidth + j, hallwayTilePositions[i].y));
                            wallTilePositions.Add(new(hallwayTilePositions[i].x - hallwayWidth - j, hallwayTilePositions[i].y));
                        }
                    }
                    else if (hallwayTilePositions[i].y == hallwayTilePositions[i + 1].y)
                    {
                        for (int y = -hallwayWidth + 1; y < hallwayWidth; y++)
                        {
                            widthTiles.Add(new(hallwayTilePositions[i].x, hallwayTilePositions[i].y + y));
                        }

                        for (int j = 0; j < 35; j++)
                        {
                            wallTilePositions.Add(new(hallwayTilePositions[i].x, hallwayTilePositions[i].y + hallwayWidth + j));
                            wallTilePositions.Add(new(hallwayTilePositions[i].x, hallwayTilePositions[i].y - hallwayWidth - j));
                        }
                    }
                    else
                    {
                        if (hallwayTilePositions[i].y > hallwayTilePositions[i + 1].y)
                        {
                            if (hallwayTilePositions[i].x > hallwayTilePositions[i + 1].x)
                            {
                                for (int x = 0; x < hallwayWidth; x++)
                                {
                                    widthTiles.Add(new(hallwayTilePositions[i].x - x, hallwayTilePositions[i].y));
                                }
                                for (int y = 0; y < hallwayWidth; y++)
                                {
                                    widthTiles.Add(new(hallwayTilePositions[i].x, hallwayTilePositions[i].y - y));
                                }

                                for (int j = 0; j < 35; j++)
                                {
                                    wallTilePositions.Add(new(hallwayTilePositions[i].x - hallwayWidth - j, hallwayTilePositions[i].y));
                                    wallTilePositions.Add(new(hallwayTilePositions[i].x, hallwayTilePositions[i].y - hallwayWidth - j));
                                }
                            }
                            else
                            {
                                for (int x = 0; x < hallwayWidth; x++)
                                {
                                    widthTiles.Add(new(hallwayTilePositions[i].x + x, hallwayTilePositions[i].y));
                                }
                                for (int y = 0; y < hallwayWidth; y++)
                                {
                                    widthTiles.Add(new(hallwayTilePositions[i].x, hallwayTilePositions[i].y - y));
                                }

                                for (int j = 0; j < 35; j++)
                                {
                                    wallTilePositions.Add(new(hallwayTilePositions[i].x + hallwayWidth + j, hallwayTilePositions[i].y));
                                    wallTilePositions.Add(new(hallwayTilePositions[i].x, hallwayTilePositions[i].y - hallwayWidth - j));
                                }
                            }
                        }
                        else
                        {
                            if (hallwayTilePositions[i].x > hallwayTilePositions[i + 1].x)
                            {
                                for (int x = 0; x < hallwayWidth; x++)
                                {
                                    widthTiles.Add(new(hallwayTilePositions[i].x - x, hallwayTilePositions[i].y));
                                }
                                for (int y = 0; y < hallwayWidth; y++)
                                {
                                    widthTiles.Add(new(hallwayTilePositions[i].x, hallwayTilePositions[i].y + y));
                                }

                                for (int j = 0; j < 35; j++)
                                {
                                    wallTilePositions.Add(new(hallwayTilePositions[i].x - hallwayWidth - j, hallwayTilePositions[i].y));
                                    wallTilePositions.Add(new(hallwayTilePositions[i].x, hallwayTilePositions[i].y + hallwayWidth + j));
                                }
                            }
                            else
                            {
                                for (int x = 0; x < hallwayWidth; x++)
                                {
                                    widthTiles.Add(new(hallwayTilePositions[i].x + x, hallwayTilePositions[i].y));
                                }
                                for (int y = 0; y < hallwayWidth; y++)
                                {
                                    widthTiles.Add(new(hallwayTilePositions[i].x, hallwayTilePositions[i].y + y));
                                }

                                for (int j = 0; j < 35; j++)
                                {
                                    wallTilePositions.Add(new(hallwayTilePositions[i].x + hallwayWidth + j, hallwayTilePositions[i].y));
                                    wallTilePositions.Add(new(hallwayTilePositions[i].x, hallwayTilePositions[i].y + hallwayWidth + j));
                                }
                            }
                        }
                    }
                }
            }

            hallwayTilePositions.AddRange(widthTiles);
            #endregion

            foreach (Vector3Int vector in hallwayTilePositions)
            {
                if (wallTilePositions.Contains(vector))
                {
                    wallTilePositions.Remove(vector);
                }
            }

            foreach (Vector3Int vector in groundTilePositions)
            {
                if (wallTilePositions.Contains(vector))
                {
                    wallTilePositions.Remove(vector);
                }
            }

            groundTilePositions.UnionWith(hallwayTilePositions);
        }
    }
    #endregion
}
#endregion

#region Loaded State
public class LoadedMapState : MapBaseState
{
    private Room currentRoom;
    private List<Vector2> points = new();

    private Vector3Int previousPlayerPosition;

    public override void EnterState(MapManager manager)
    {
        manager.currentMap = new()
        {
            seed = manager.Settings.seed,
            spawnFunction = manager.Settings.spawnFunction,
            roomMaxSize = manager.Settings.roomMaxSize,
            roomMinSize = manager.Settings.RoomMinSize,
            amountOfRooms = manager.Settings.totalRoomsCount,
            amountOfMainRooms = manager.Settings.AmountOfMainRooms,
            amountOfLoops = manager.Settings.amountOfHallwayLoops,
            hallwayWidth = manager.Settings.hallwayWidth,
            stripSpawnSize = manager.Settings.stripSize,
            spawnRadius = manager.Settings.generationRadius
        };

        points.Clear();
        manager.activeRooms.Clear();

        LoadRooms(manager, manager.startingRoom);

        Player.Instance.transform.position = currentRoom.WorldPosition;
        previousPlayerPosition = Vector3Int.FloorToInt(Player.Instance.transform.position);
    }

    public override void UpdateState(MapManager manager)
    {
        if (Player.Instance.IsMoving && previousPlayerPosition != Vector3Int.FloorToInt(Player.Instance.transform.position))
        {
            foreach (Room room in manager.activeRooms)
            {
                //if (room != currentRoom && player intersects with room)
                //{
                    //LoadRooms(manager, room);
                //}

                if (!DestructibleManager.Instance.BreakablesWithinRoom[room].First().transform.parent.gameObject.activeInHierarchy)
                {
                    if (IsVisible(room.center, new Vector3(room.width + 1, room.height + 1), manager.cameraTest))
                    {
                        DestructibleManager.Instance.BreakablesWithinRoom[room].First().transform.parent.gameObject.SetActive(true);
                        TrapManager.Instance.TrapsWithinRoom[room].First().transform.parent.gameObject.SetActive(true);
                    }
                }
                else
                {
                    if (!IsVisible(room.center, new Vector3(room.width + 1, room.height + 1), manager.cameraTest))
                    {
                        DestructibleManager.Instance.BreakablesWithinRoom[room].First().transform.parent.gameObject.SetActive(false);
                        TrapManager.Instance.TrapsWithinRoom[room].First().transform.parent.gameObject.SetActive(false);
                    }
                }
            }

            previousPlayerPosition = Vector3Int.FloorToInt(Player.Instance.transform.position);
        }
    }

    public override void ExitState(MapManager manager)
    {
        manager.groundTileMap.ClearAllTiles();
        manager.wallTileMap.ClearAllTiles();
        TrapManager.Instance.ClearTraps();
        DestructibleManager.Instance.ClearBreakables();
    }

    private bool IsVisible(Vector3 pos, Vector3 boundSize, Camera camera)
    {
        var bounds = new Bounds(pos, boundSize);
        var planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(planes, bounds);
    }

    private void LoadRooms(MapManager manager, Room room)
    {
        currentRoom = room;

        foreach (Room previouslyActiveRoom in manager.activeRooms)
        {
            if (previouslyActiveRoom != currentRoom)
            {
                DestructibleManager.Instance.BreakablesWithinRoom[previouslyActiveRoom].First().transform.parent.gameObject.SetActive(false);
                TrapManager.Instance.TrapsWithinRoom[previouslyActiveRoom].First().transform.parent.gameObject.SetActive(false);
            }
        }

        manager.activeRooms.Clear();

        manager.activeRooms.Add(currentRoom);

        foreach (Edge edge in manager.minimumSpanningTree)
        {
            if (edge.pointA == currentRoom.center)
            {
                points.Add(edge.pointB);
            }
            else if (edge.pointB == currentRoom.center)
            {
                points.Add(edge.pointA);
            }
        }

        foreach (Room connectedRoom in manager.rooms)
        {
            if (connectedRoom != currentRoom && points.Contains(connectedRoom.center))
            {
                manager.activeRooms.Add(connectedRoom);

                if (IsVisible(connectedRoom.center, new Vector3(connectedRoom.width, connectedRoom.height), manager.cameraTest))
                {
                    DestructibleManager.Instance.BreakablesWithinRoom[connectedRoom].First().transform.parent.gameObject.SetActive(true);
                    TrapManager.Instance.TrapsWithinRoom[connectedRoom].First().transform.parent.gameObject.SetActive(true);
                }
                else
                {
                    DestructibleManager.Instance.BreakablesWithinRoom[connectedRoom].First().transform.parent.gameObject.SetActive(false);
                    TrapManager.Instance.TrapsWithinRoom[connectedRoom].First().transform.parent.gameObject.SetActive(false);
                }
            }
        }
    }
}
#endregion

