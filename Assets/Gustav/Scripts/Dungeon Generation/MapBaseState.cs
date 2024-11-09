using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public Room[] roomList;
    public List<Room> mainRooms = new();

    private Dictionary<TileMapType, HashSet<Vector3Int>> tileMaps;

    private GameObject triangulationDebug, minimumSpanningTreeDebug;

    System.Diagnostics.Stopwatch stopwatch;
    float diagnosticTime;

    private System.Random rng;

    public override void EnterState(MapManager manager)
    {
        #region Diagnostic Start
        stopwatch = new();
        stopwatch.Start();
        #endregion

        #region Game objects
        GameObject.Destroy(triangulationDebug);
        GameObject.Destroy(minimumSpanningTreeDebug);
        GameObject.Destroy(manager.activeGameObjectsParent);
        #endregion

        rng = new System.Random(manager.Settings.seed);

        mainRooms.Clear();

        tileMaps = new() { { TileMapType.ground, new() }, { TileMapType.wall, new() } };

        manager.gameObjectsList.Clear();
        manager.activeGameObjectsParent = new()
        {
            name = "Active Game Objects"
        };

        manager.regions = new();

        roomList = new Room[manager.Settings.totalRoomsCount];
        Heap<Room> roomHeapForSize = new(manager.Settings.totalRoomsCount);

        // Generate x amount of rooms
        for (int i = 0; i < manager.Settings.totalRoomsCount; i++)
        {
            Room newRoom = GenerateRoom(manager);

            roomList[i] = newRoom;
            roomHeapForSize.Add(newRoom);
        }

        // Add x amount (Amount of main rooms) with the largest area
        for (int i = 0; i < manager.Settings.AmountOfMainRooms; i++)
        {
            mainRooms.Add(roomHeapForSize.RemoveFirst());
        }

        manager.startingRoom = mainRooms.First();

        GenerateMapAsync(manager);
    }

    public override void UpdateState(MapManager manager)
    {

    }

    public override void ExitState(MapManager manager)
    {
        #region Diagnostic End
        stopwatch.Stop();

        Debug.Log("It took a total of: " + stopwatch.ElapsedMilliseconds + " MS, to generate map");
        #endregion
    }

    private async void GenerateMapAsync(MapManager manager)
    {
        List<GameObjectPositionPair> gameObjects = new();

        Dictionary<TileMapType, HashSet<Vector3Int>> result = await Task.Run(() =>
        {
            bool canMoveRoom;

            while (true)
            {
                canMoveRoom = false;

                foreach (Room room in roomList)
                {
                    Vector3Int dir = (Vector3Int)GetDirection(room);

                    if (dir != Vector3Int.zero)
                    {
                        room.MoveRoom(dir);
                        canMoveRoom = true;
                    }
                }

                if (!canMoveRoom)
                {
                    break;
                }
            }

            Vector3Int noiseMapTopRight = Vector3Int.zero, noiseMapBottomLeft = Vector3Int.zero;

            foreach (Room room in mainRooms)
            {
                tileMaps[TileMapType.ground].UnionWith(room.groundTiles);
                tileMaps[TileMapType.wall].UnionWith(room.walls);

                if (noiseMapTopRight.x < room.TopRight.x)
                {
                    noiseMapTopRight.x = Vector3Int.CeilToInt(room.TopRight).x + 1;
                }
                if (noiseMapTopRight.y < room.TopRight.y)
                {
                    noiseMapTopRight.y = Vector3Int.CeilToInt(room.TopRight).y + 1;
                }

                if (noiseMapBottomLeft.x > room.BottomLeft.x)
                {
                    noiseMapBottomLeft.x = Vector3Int.FloorToInt(room.BottomLeft).x - 1;
                }
                if (noiseMapBottomLeft.y > room.BottomLeft.y)
                {
                    noiseMapBottomLeft.y = Vector3Int.FloorToInt(room.BottomLeft).y - 1;
                }
            }

            List<Triangle> triangulation = GenerateDelaunayTriangulation(manager, mainRooms);

            List<Edge> minimumSpanningTree = GetMinimumSpanningTree(manager, triangulation);

            GenerateHallways(manager, minimumSpanningTree);

            int noiseMapWidth = Math.Abs(noiseMapBottomLeft.x) + Math.Abs(noiseMapTopRight.x), noiseMapHeight = Math.Abs(noiseMapBottomLeft.y) + Math.Abs(noiseMapTopRight.y);
            Vector3Int noiseMapCenter = new(noiseMapBottomLeft.x + noiseMapWidth / 2, noiseMapBottomLeft.y + noiseMapHeight / 2);

            GenerateNoiseMap(manager, noiseMapWidth, noiseMapHeight, noiseMapCenter, gameObjects);

            tileMaps[TileMapType.wall].ExceptWith(tileMaps[TileMapType.ground]);

            AddEnemies(manager, gameObjects);

            return new Dictionary<TileMapType, HashSet<Vector3Int>>();
        }, manager.TokenSource.Token);

        if (manager.TokenSource.IsCancellationRequested)
        {
            return;
        }

        diagnosticTime = stopwatch.ElapsedMilliseconds;

        foreach (GameObjectPositionPair pair in gameObjects)
        {
            manager.SpawnPrefab(pair.gameObject, pair.position, false);
        }

        Debug.Log("It took: " + (stopwatch.ElapsedMilliseconds - diagnosticTime) + " MS, to add gameObjects");

        manager.StartCoroutine(Test(manager, tileMaps[TileMapType.ground], TileTexture.ground, 100));
        manager.StartCoroutine(Test(manager, tileMaps[TileMapType.wall], TileTexture.wall, 100));

        manager.SwitchState(manager.loadedState);
    }

    private IEnumerator Test(MapManager manager, HashSet<Vector3Int> tiles, TileTexture tileTexture, int tilesSetPerFrame)
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            if (i > 0 && i % tilesSetPerFrame == 0)
            {
                TileBase[] groundTiles;
                Range range;

                if (i + tilesSetPerFrame <= tiles.Count)
                {
                    range = new(-tilesSetPerFrame + i, i);
                    groundTiles = new TileBase[tilesSetPerFrame];

                    Array.Fill(groundTiles, manager.tilePairs[tileTexture]);
                    manager.groundTileMap.SetTiles(tiles.ToArray()[range], groundTiles);
                }
                else
                {
                    range = new(i - tilesSetPerFrame, tiles.Count);
                    groundTiles = new TileBase[tiles.Count - i + tilesSetPerFrame];

                    Array.Fill(groundTiles, manager.tilePairs[tileTexture]);
                    manager.groundTileMap.SetTiles(tiles.ToArray()[range], groundTiles);
                    break;
                }

                yield return null;
            }
        }
    }

    #region Generate rooms
    private Room GenerateRoom(MapManager manager)
    {
        Vector2 position = Vector2.zero;

        int roomWidth = rng.Next(manager.Settings.RoomMinSize.x, manager.Settings.roomMaxSize.x + 1);
        int roomHeight = rng.Next(manager.Settings.RoomMinSize.y, manager.Settings.roomMaxSize.y + 1);
        Vector2Int offset = new(roomWidth / 2, roomHeight / 2);

        if (manager.Settings.spawnFunction == SpawnFunction.Circle)
        {
            position = RandomPositionInCircle(manager.Settings.generationRadius) - offset;
        }
        else if (manager.Settings.spawnFunction == SpawnFunction.Strip)
        {
            position = RandomPositionInStrip(manager.Settings.stripSize.x, manager.Settings.stripSize.y) - offset;
        }

        return new Room(roomWidth, roomHeight, position, manager.Settings.roundCorners);
    }

    public Vector2 RandomPositionInCircle(float radius)
    {
        float r = radius * Mathf.Sqrt((float)rng.NextDouble());
        float theta = (float)rng.NextDouble() * 2 * Mathf.PI;

        return new Vector2(r * Mathf.Cos(theta), r * Mathf.Sin(theta));
    }

    public Vector2 RandomPositionInStrip(int width, int height) => new(rng.Next(-width / 2, width / 2), rng.Next(-height / 2, height / 2));
    #endregion

    #region Seperate rooms

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

    public Vector2Int GetDirection(Room currentRoom)
    {
        Vector2 separationVelocity = Vector2.zero;
        Vector2 otherPosition, otherAgentToCurrent, directionToTravel;

        foreach (Room room in roomList)
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
    private List<Edge> GetMinimumSpanningTree(MapManager manager, List<Triangle> triangulation)
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
        List<Edge> minimumSpanningTree = new();
        HashSet<Vector2> pointsVisited = new();

        List<Edge> openEdges = new();
        HashSet<Vector2> closedPoints = new();

        Heap<Edge> heap = new(edgeList.Count);

        for (int i = 0; i < edgeList.Count; i++)
        {
            heap.Add(edgeList[i]);
        }

        while (true)
        {
            Edge shortestEdge = heap.RemoveFirst();
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
                        foreach (Vector2 point in openEdges[i].points)
                        {
                            if (closedPoints.Contains(point))
                            {
                                continue;
                            }

                            closedPoints.Add(point);

                            foreach (Edge edge in minimumSpanningTree)
                            {
                                if (edge.points.Contains(point))
                                {
                                    if (point == openEdges[i].pointA)
                                    {
                                        if (edge.points.Contains(shortestEdge.pointB))
                                        {
                                            canAdd = false;
                                            break;
                                        }
                                    }
                                    else
                                    {
                                        if (edge.points.Contains(shortestEdge.pointA))
                                        {
                                            canAdd = false;
                                            break;
                                        }
                                    }

                                    openEdges.Add(edge);
                                }
                            }
                        }

                        if (!canAdd)
                        {
                            openEdges.Clear();
                            break;
                        }
                        else
                        {
                            openEdges.Remove(openEdges[i]);
                        }
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

            if (mainRooms.Count == minimumSpanningTree.Count + 1)
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

                loopedEdges.Add(current);
                edgeList.Remove(current);
                count++;

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

                ln.SetPosition(0, new Vector3(minimumSpanningTree[i].pointA.x, minimumSpanningTree[i].pointA.y, -1));
                ln.SetPosition(1, new Vector3(minimumSpanningTree[i].pointB.x, minimumSpanningTree[i].pointB.y, -1));
            }
        }
        #endregion

        return minimumSpanningTree;
    }
    #endregion

    #region Hallway generation
    private void GenerateHallways(MapManager manager, List<Edge> minimumSpanningTree)
    {
        int hallwayWidth = manager.Settings.hallwayWidth;

        foreach (Edge connection in minimumSpanningTree)
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

            Vector3Int startingPosition = new((int)roomList[0].WorldPosition.x, (int)roomList[0].WorldPosition.y); // From roomList[0]
            Vector3Int targetPosition = new((int)roomList[1].WorldPosition.x, (int)roomList[1].WorldPosition.y); // Towards roomList[1]

            // Find a path starting from first room in list towards the connected room.

            List<Vector3Int> hallwayPath = new(AStar.FindPath(startingPosition, targetPosition));

            tileMaps[TileMapType.ground].UnionWith(hallwayPath);

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

            #region Add width to hallway

            for (int i = 0; i < hallwayPath.Count; i++)
            {
                if (i + 1 >= hallwayPath.Count)
                {
                    break;
                }

                ExpandHallwayAndAddTiles(hallwayPath[i], hallwayPath[i + 1], hallwayWidth);
            }

            #endregion
        }
    }

    private void ExpandHallwayAndAddTiles(Vector3Int current, Vector3Int next, int hallwayWidth)
    {
        if (current.x == next.x)
        {
            for (int j = -hallwayWidth + 1; j < hallwayWidth; j++)
            {
                tileMaps[TileMapType.ground].Add(new(current.x + j, current.y));
            }

            for (int j = 0; j < 35; j++)
            {
                tileMaps[TileMapType.wall].Add(new(current.x + hallwayWidth + j, current.y));
                tileMaps[TileMapType.wall].Add(new(current.x - hallwayWidth - j, current.y));
            }
        }
        else if (current.y == next.y)
        {
            for (int j = -hallwayWidth + 1; j < hallwayWidth; j++)
            {
                tileMaps[TileMapType.ground].Add(new(current.x, current.y + j));
            }

            for (int j = 0; j < 35; j++)
            {
                tileMaps[TileMapType.wall].Add(new(current.x, current.y + hallwayWidth + j));
                tileMaps[TileMapType.wall].Add(new(current.x, current.y - hallwayWidth - j));
            }
        }
        else
        {
            for (int i = 0; i < hallwayWidth; i++)
            {
                if (current.y > next.y)
                {
                    if (current.x > next.x)
                    {
                        tileMaps[TileMapType.ground].Add(new(current.x - i, current.y));
                    }
                    else
                    {
                        tileMaps[TileMapType.ground].Add(new(current.x + i, current.y));
                    }

                    tileMaps[TileMapType.ground].Add(new(current.x, current.y - i));
                }
                else
                {
                    if (current.x > next.x)
                    {
                        tileMaps[TileMapType.ground].Add(new(current.x - i, current.y));
                    }
                    else
                    {
                        tileMaps[TileMapType.ground].Add(new(current.x + i, current.y));
                    }

                    tileMaps[TileMapType.ground].Add(new(current.x, current.y + i));
                }
            }

            for (int i = 0; i < 35; i++)
            {
                if (current.y > next.y)
                {
                    if (current.x > next.x)
                    {
                        tileMaps[TileMapType.wall].Add(new(current.x - hallwayWidth - i, current.y));
                    }
                    else
                    {
                        tileMaps[TileMapType.wall].Add(new(current.x + hallwayWidth + i, current.y));
                    }

                    tileMaps[TileMapType.wall].Add(new(current.x, current.y - hallwayWidth - i));
                }
                else
                {
                    if (current.x > next.x)
                    {
                        tileMaps[TileMapType.wall].Add(new(current.x - hallwayWidth - i, current.y));
                    }
                    else
                    {
                        tileMaps[TileMapType.wall].Add(new(current.x + hallwayWidth + i, current.y));
                    }

                    tileMaps[TileMapType.wall].Add(new(current.x, current.y + hallwayWidth + i));
                }
            }
        }
    }
    #endregion

    #region Noise map
    private void GenerateNoiseMap(MapManager manager, int width, int height, Vector3Int center, List<GameObjectPositionPair> gameObjects)
    {
        HashSet<Vector3Int> occupiedPositions = new();

        float currentHeight;
        Vector3Int tilePosition;
        Vector2 offset;

        for (int i = 0; i < manager.Settings.amountOfNoiseLoops; i++)
        {
            offset = new(rng.Next(-100000, 100000), rng.Next(-100000, 100000));

            float[] noiseMap = NoiseMapGenerator.GenerateMap(width, height, manager.Settings.seed, manager.Settings.noiseScale, manager.Settings.octaves, manager.Settings.persistence, manager.Settings.lacunarity, offset);

            for (int j = 0; j < noiseMap.Length; j++)
            {
                tilePosition = new((j % width) - (width / 2) + center.x, (j / width) - (height / 2) + center.y);

                if (!tileMaps[TileMapType.ground].Contains(tilePosition) || occupiedPositions.Contains(tilePosition))
                {
                    continue;
                }

                currentHeight = noiseMap[j];

                foreach (NoiseRegion region in manager.Settings.prefabs)
                {
                    if (currentHeight <= region.heightValue)
                    {
                        if (region.prefab != null)
                        {
                            gameObjects.Add(new GameObjectPositionPair { gameObject = region.prefab, position = tilePosition } );
                            occupiedPositions.Add(tilePosition);
                        }

                        break;
                    }
                }
            }
        }
    }

    private struct GameObjectPositionPair
    {
        public GameObject gameObject;
        public Vector3Int position;
    }
    #endregion

    #region Enemies
    private void AddEnemies(MapManager manager, List<GameObjectPositionPair> gameObjects)
    {
        GameObject prefab;
        Vector3Int spawnPosition;

        for (int i = 0; i < manager.Settings.amountOfEnemies; i++)
        {
            spawnPosition = tileMaps[TileMapType.ground].ElementAt(rng.Next(0, tileMaps[TileMapType.ground].Count + 1));
            prefab = manager.Settings.enemyPrefabs[rng.Next(0, manager.Settings.enemyPrefabs.Length)];
            gameObjects.Add(new GameObjectPositionPair { gameObject = prefab, position = spawnPosition } );
        }
    }
    #endregion
}
#endregion

#region Loaded State
public class LoadedMapState : MapBaseState
{
    public Camera camera;

    private readonly HashSet<Vector2Int> previousRegions = new(), currentRegions = new();
    private Vector2Int cBottomLeft, cTopRight, region;

    public override void EnterState(MapManager manager)
    {
        previousRegions.Clear();
        currentRegions.Clear();

        camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        Player.Instance.transform.position = manager.startingRoom.WorldPosition;
        camera.transform.position = Player.Instance.transform.position;

        Player.Instance.OnRegionSwitch += Player_OnRegionSwitch;
        Player.Instance.UpdateRegion();
    }

    public override void UpdateState(MapManager manager)
    {

    }

    public override void ExitState(MapManager manager)
    {
        manager.groundTileMap.ClearAllTiles();
        manager.wallTileMap.ClearAllTiles();
    }

    private void Player_OnRegionSwitch(object sender, EventArgs e)
    {
        UpdateActiveRegions();
    }

    private void UpdateActiveRegions()
    {
        currentRegions.Clear();

        cBottomLeft = Vector2Int.FloorToInt(0.0625f * camera.ScreenToWorldPoint(new Vector3(0, 0, camera.nearClipPlane)));
        cTopRight = Vector2Int.FloorToInt(0.0625f * camera.ScreenToWorldPoint(new Vector3(camera.pixelWidth, camera.pixelHeight, camera.nearClipPlane)));

        for (int x = cBottomLeft.x - 1; x < cTopRight.x + 2; x++)
        {
            for (int y = cBottomLeft.y - 1; y < cTopRight.y + 2; y++)
            {
                region = new(x, y);

                if (!MapManager.Instance.regions.TryGetValue(region, out var enableList))
                {
                    continue;
                }
                else if (previousRegions.Contains(region))
                {
                    previousRegions.Remove(region);
                    currentRegions.Add(region);
                    continue;
                }

                for (int i = 0; i < enableList.Count; i++)
                {
                    enableList[i].SetActive(true);
                }

                currentRegions.Add(region);
            }
        }

        foreach (Vector2Int previousRegion in previousRegions)
        {
            if (MapManager.Instance.regions.TryGetValue(previousRegion, out var disableList))
            {
                for (int i = 0; i < disableList.Count; i++)
                {
                    disableList[i].SetActive(false);
                }
            }
        }

        previousRegions.Clear();
        previousRegions.UnionWith(currentRegions);
    }
}
#endregion

