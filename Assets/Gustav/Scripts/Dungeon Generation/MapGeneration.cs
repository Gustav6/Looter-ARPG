using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEngine.RuleTile.TilingRuleOutput;

public static class MapGeneration
{
    private static System.Random rng;

    private static Dictionary<TileMapType, HashSet<Vector3Int>> tileMaps;
    private static readonly int tilesSetPerFrame = 100;

    private static HashSet<Vector3Int> availableGroundPositions, availableWallPositions;

    public static event EventHandler OnGenerationCompleted;

    private static Vector3Int travelToNextMapObjectPosition;

    public static async void GenerateMapAsync(MapManager manager, GameObject mapPrefab)
    {
        // Every game object within map
        List<GameObjectPositionPair> gameObjects = new();

        // Ranges for how many tiles will be set at the same time
        List<Range> groundTileRanges = new(), wallTileRanges = new();

        // Spawn map game object
        GameObject mapGameObject = UnityEngine.Object.Instantiate(mapPrefab);
        mapGameObject.SetActive(false);

        UnityEngine.Object.DontDestroyOnLoad(mapGameObject);

        // Reference script
        Map mapReference = mapGameObject.GetComponent<Map>();

        if (manager.currentMap == null)
        {
            manager.currentMap = mapReference;
            mapGameObject.name = "Active map";
            mapReference.MapRegions = new();
        }
        else if (manager.nextMap == null)
        {
            manager.nextMap = mapReference;
            mapGameObject.name = "Next map";
            mapReference.MapRegions = new();
        }

        mapReference.SetRequiredSettings();

        tileMaps = await Task.Run(() =>
        {
            #region Diagnostic Start
            System.Diagnostics.Stopwatch stopwatch = new();
            stopwatch.Start();
            #endregion

            // Instantiate tile map dictionary
            tileMaps = new() { { TileMapType.ground, new() }, { TileMapType.wall, new() } };

            availableGroundPositions = new();
            availableWallPositions = new();

            // Controls what random outcome will appear
            rng = new System.Random(manager.Settings.seed);

            // Room related variables
            Room[] roomList = new Room[manager.Settings.totalRoomsCount], mainRooms = new Room[manager.Settings.AmountOfMainRooms];

            #region Room generation

            // Heap for keeping a reference to the largest rooms (Most amount of ground tiles)
            Heap<Room> roomHeapForSize = new(manager.Settings.totalRoomsCount);

            for (int i = 0; i < roomList.Length; i++)
            {
                Room newRoom = GenerateRoom(manager);
                roomHeapForSize.Add(newRoom);

                roomList[i] = newRoom;
            }

            for (int i = 0; i < mainRooms.Length; i++)
            {
                mainRooms[i] = roomHeapForSize.RemoveFirst();
            }
            #endregion

            #region Room seperation
            bool canMoveRoom;

            while (true)
            {
                canMoveRoom = false;

                // Move rooms away from any other room that intersect said room
                foreach (Room room in mainRooms)
                {
                    Vector3Int dir = (Vector3Int)GetDirection(manager, room, mainRooms);

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
            #endregion

            mapReference.startRoom = mainRooms.First();
            mapReference.endRoom = mainRooms.ElementAt(1);

            // Noise maps corner variables
            Vector3Int noiseMapTopRight = Vector3Int.zero, noiseMapBottomLeft = Vector3Int.zero;

            // Add rooms tiles to dictionary tiles and set noise maps corner variables
            foreach (Room room in mainRooms)
            {
                if (room != mapReference.startRoom || room != mapReference.endRoom)
                {
                    float distanceFromStartToEnd = Vector2.Distance(mapReference.startRoom.WorldPosition, mapReference.endRoom.WorldPosition);
                    float distanceFromStartToCurrentRoom = Vector2.Distance(mapReference.startRoom.WorldPosition, room.WorldPosition);

                    if (distanceFromStartToEnd < distanceFromStartToCurrentRoom)
                    {
                        mapReference.endRoom = room;
                    }
                }

                tileMaps[TileMapType.ground].UnionWith(room.groundTiles);
                tileMaps[TileMapType.wall].UnionWith(room.walls);

                #region Update noise map corners
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
                #endregion
            }

            // Connect rooms using: Delaunay triangulation
            List<Triangle> triangulation = GenerateDelaunayTriangulation(manager, mainRooms);

            // Connect rooms with the minimal amount of edges + loop amount
            List<Edge> minimumSpanningTree = GetMinimumSpanningTree(manager, triangulation, mainRooms);

            // Connect rooms with hallways
            GenerateHallways(manager, minimumSpanningTree, mainRooms);

            // Remove wall tiles that are on the same tile position as any ground tile
            tileMaps[TileMapType.wall].ExceptWith(tileMaps[TileMapType.ground]);

            // Add ground and wall tile positions as available positions for game objects
            availableGroundPositions.UnionWith(tileMaps[TileMapType.ground]);
            availableWallPositions.UnionWith(tileMaps[TileMapType.wall]);

            // Add teleporters position and remove it from available positions
            travelToNextMapObjectPosition = Vector3Int.FloorToInt(mapReference.endRoom.center);

            travelToNextMapObjectPosition.x += rng.Next(-mapReference.endRoom.width / 5, mapReference.endRoom.width / 5);
            travelToNextMapObjectPosition.y += rng.Next(-mapReference.endRoom.height / 5, mapReference.endRoom.height / 5);

            availableGroundPositions.Remove(travelToNextMapObjectPosition);

            // Remove the players spawn position and surrounding tiles from available list
            Vector2Int playerSpawnTopRightPosition, playerSpawnBottomLeftPosition;

            playerSpawnTopRightPosition = Vector2Int.CeilToInt(mapReference.startRoom.WorldPosition) + Vector2Int.one * 2;
            playerSpawnBottomLeftPosition = Vector2Int.FloorToInt(mapReference.startRoom.WorldPosition) - Vector2Int.one * 2;

            for (int x = playerSpawnBottomLeftPosition.x; x < playerSpawnTopRightPosition.x; x++)
            {
                for (int y = playerSpawnBottomLeftPosition.y; y < playerSpawnTopRightPosition.y; y++)
                {
                    availableGroundPositions.Remove(new(x, y));
                }
            }

            int noiseMapWidth = Math.Abs(noiseMapBottomLeft.x) + Math.Abs(noiseMapTopRight.x), noiseMapHeight = Math.Abs(noiseMapBottomLeft.y) + Math.Abs(noiseMapTopRight.y);
            Vector3Int noiseMapCenter = new(noiseMapBottomLeft.x + noiseMapWidth / 2, noiseMapBottomLeft.y + noiseMapHeight / 2);

            // Generate game objects within dungeon using noise map
            GenerateNoiseMap(manager, noiseMapWidth, noiseMapHeight, noiseMapCenter, gameObjects);

            AddEnemiesToList(manager, gameObjects);

            AddPrefabsToList(manager, gameObjects);

            #region Set ranges for coroutine
            foreach (TileMapType tileMap in tileMaps.Keys)
            {
                for (int i = 0; i < tileMaps[tileMap].Count; i++)
                {
                    if (i > 0 && i % tilesSetPerFrame == 0)
                    {
                        Range range;

                        range = new(-tilesSetPerFrame + i, i);

                        switch (tileMap)
                        {
                            case TileMapType.ground:
                                groundTileRanges.Add(range);

                                if (i + tilesSetPerFrame >= tileMaps[tileMap].Count)
                                {
                                    range = new(i - tilesSetPerFrame, tileMaps[tileMap].Count);
                                    groundTileRanges.Add(range);
                                    break;
                                }
                                break;
                            case TileMapType.wall:
                                wallTileRanges.Add(range);

                                if (i + tilesSetPerFrame >= tileMaps[tileMap].Count)
                                {
                                    range = new(i - tilesSetPerFrame, tileMaps[tileMap].Count);
                                    wallTileRanges.Add(range);
                                    break;
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            #endregion

            #region Diagnostic End
            stopwatch.Stop();

            Debug.Log("It took a total of: " + stopwatch.ElapsedMilliseconds + " MS, to generate map");
            #endregion

            return tileMaps;
        }, manager.TokenSource.Token);

        if (manager.TokenSource.IsCancellationRequested)
        {
            return;
        }

        manager.SpawnPrefab(manager.TravelToNextMapPrefab, travelToNextMapObjectPosition, mapReference, false);

        mapReference.WallMap.transform.GetComponent<TilemapCollider2D>().enabled = false;

        #region Coroutines
        mapReference.amountOfTilemapCoreroutinesStarted = 0;
        mapReference.amountOfTilemapCoreroutinesFinished = 0;

        manager.StartCoroutine(SpawnGameObjects(manager, mapReference, gameObjects));

        TileBase[] tiles;
        
        tiles = new TileBase[1000];
        Array.Fill(tiles, manager.tilePairs[TileTexture.ground]);

        manager.StartCoroutine(SetTiles(manager, mapReference, TileMapType.ground, groundTileRanges, tileMaps[TileMapType.ground].ToArray(), tiles));
        mapReference.amountOfTilemapCoreroutinesStarted++;

        //tiles = new TileBase[tileMaps[TileMapType.wall].Count];
        //Array.Fill(tiles, manager.tilePairs[TileTexture.wall]);

        //manager.wallMapTest.SetTiles(tileMaps[TileMapType.wall].ToArray(), tiles);

        tiles = new TileBase[1000];
        Array.Fill(tiles, manager.tilePairs[TileTexture.wall]);

        manager.StartCoroutine(SetTiles(manager, mapReference, TileMapType.wall, wallTileRanges, tileMaps[TileMapType.wall].ToArray(), tiles));
        mapReference.amountOfTilemapCoreroutinesStarted++;

        //tiles = new TileBase[1000];
        //Array.Fill(tiles, manager.tilePairs[TileTexture.walkableIcon]);

        //manager.StartCoroutine(SetTiles(mapReference, TileMapType.walkableIcon, groundTileRanges, tileMaps[TileMapType.ground].ToArray(), tiles));
        #endregion
    }

    #region Coroutines

    #region Spawning game objects
    private static IEnumerator SpawnGameObjects(MapManager manager, Map map, List<GameObjectPositionPair> gameObjectPairs)
    {
        for (int i = 0; i < gameObjectPairs.Count; i++)
        {
            manager.SpawnPrefab(gameObjectPairs[i].gameObject, gameObjectPairs[i].position, map, false);

            if (i > 0 && i % 10 == 0)
            {
                yield return null;
            }
        }

        Debug.Log("Prefabs have finished spawning");
    }
    #endregion

    #region placing tiles on tilemap
    private static IEnumerator SetTiles(MapManager manager, Map map, TileMapType tileMapType, List<Range> ranges, Vector3Int[] mapPositions, TileBase[] tiles)
    {
        foreach (Range range in ranges)
        {
            switch (tileMapType)
            {
                case TileMapType.ground:
                    map.GroundMap.SetTiles(mapPositions[range], tiles);
                    break;
                case TileMapType.wall:
                    map.WallMap.SetTiles(mapPositions[range], tiles);
                    break;
                case TileMapType.walkableIcon:
                    map.WallMapIcons.SetTiles(mapPositions[range], tiles);
                    break;
                default:
                    break;
            }

            yield return null;
        }

        if (tileMapType == TileMapType.wall)
        {
            map.WallMap.GetComponent<TilemapCollider2D>().enabled = true;
        }

        map.amountOfTilemapCoreroutinesFinished++;

        if (map.amountOfTilemapCoreroutinesFinished == map.amountOfTilemapCoreroutinesStarted)
        {
            manager.StartCoroutine(CleanUpTileMap(map));
        }

        Debug.Log(tileMapType + " Is done");
    }
    #endregion

    private static IEnumerator CleanUpTileMap(Map map)
    {
        map.WallMap.RefreshAllTiles();

        while (map.WallMap.transform.childCount > 0)
        {
            for (int i = map.WallMap.transform.childCount - 1; i >= 0; i--)
            {
                Debug.Log(map.WallMap.transform.childCount);

                Vector3Int position = Vector3Int.FloorToInt(map.WallMap.transform.GetChild(i).position);
                MapManager.Instance.currentMap.WallMap.SetTile(position, null);
                MapManager.Instance.currentMap.GroundMap.SetTile(position, MapManager.Instance.tilePairs[TileTexture.ground]);
            }

            map.WallMap.RefreshAllTiles();

            yield return new WaitForEndOfFrame();
        }

        for (int i = tileMaps[TileMapType.wall].Count - 1; i >= 0; i--)
        {
            if (!WallTileNeighborsAGroundTile(tileMaps[TileMapType.wall].ElementAt(i)))
            {
                map.WallMap.SetTile(tileMaps[TileMapType.wall].ElementAt(i), null);
            }
        }

        map.readyToLoad = true;
        Debug.Log("Map ready to load");
        OnGenerationCompleted?.Invoke(null, EventArgs.Empty);
    }

    #endregion

    #region Room generation
    private static Room GenerateRoom(MapManager manager)
    {
        Vector2 position = Vector2.zero;

        int roomWidth = rng.Next(manager.Settings.RoomMinSize.x, manager.Settings.roomMaxSize.x + 1);
        int roomHeight = rng.Next(manager.Settings.RoomMinSize.y, manager.Settings.roomMaxSize.y + 1);
        Vector2Int offset = new(roomWidth / 2, roomHeight / 2);

        if (manager.Settings.spawnFunction == SpawnFunction.Circle)
        {
            position = RandomPositionInCircle(manager.Settings.radiusForGen) - offset;
        }
        else if (manager.Settings.spawnFunction == SpawnFunction.Strip)
        {
            position = RandomPositionInStrip(manager.Settings.stripSizeForGen.x, manager.Settings.stripSizeForGen.y) - offset;
        }

        return new Room(roomWidth, roomHeight, position, rng, true);
    }

    #region Random Position
    private static Vector2 RandomPositionInCircle(float radius)
    {
        float r = radius * Mathf.Sqrt((float)rng.NextDouble());
        float theta = (float)rng.NextDouble() * 2 * Mathf.PI;

        return new Vector2(r * Mathf.Cos(theta), r * Mathf.Sin(theta));
    }

    private static Vector2 RandomPositionInStrip(int width, int height)
    {
        return new(rng.Next(-width / 2, width / 2), rng.Next(-height / 2, height / 2));
    }
    #endregion
    #endregion

    #region Room seperation

    private static Vector2Int GetDirection(MapManager manager, Room currentRoom, Room[] rooms)
    {
        Vector2 separationVelocity = Vector2.zero;
        Vector2 otherPosition, otherAgentToCurrent, directionToTravel;

        foreach (Room room in rooms)
        {
            if (!RoomIntersects(currentRoom, room, manager.Settings.distanceFromRoomToRoom))
            {
                continue;
            }

            otherPosition = room.WorldPosition;

            otherAgentToCurrent = currentRoom.WorldPosition - otherPosition;
            directionToTravel = otherAgentToCurrent.normalized;

            separationVelocity += directionToTravel;
        }

        if (separationVelocity == Vector2.zero)
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

    #region Room intersection

    private static bool RoomIntersects(Room roomA, Room roomB, int expandedSize = 0)
    {
        if (roomA != roomB)
        {
            if (roomA.BottomLeft.x - expandedSize < roomB.TopRight.x + expandedSize && roomA.TopRight.x + expandedSize > roomB.BottomLeft.x - expandedSize)
            {
                if (roomA.BottomLeft.y - expandedSize < roomB.TopRight.y + expandedSize && roomA.TopRight.y + expandedSize > roomB.BottomLeft.y - expandedSize)
                {
                    return true;
                }
            }
        }

        return false;
    }

    #endregion
    #endregion

    #region Delaunay triangulation
    private static List<Triangle> GenerateDelaunayTriangulation(MapManager manager, Room[] rooms)
    {
        if (rooms.Length <= 3)
        {
            return new List<Triangle>();
        }

        Vector2[] points = new Vector2[rooms.Length];

        for (int i = 0; i < rooms.Length; i++)
        {
            points[i] = rooms[i].WorldPosition;
        }

        List<Triangle> triangulation = new();

        #region Super Triangle
        Vector2 a, b, c;

        if (manager.Settings.radiusForGen <= 0)
        {
            manager.Settings.radiusForGen = 1;
        }

        a = new Vector2(-manager.Settings.radiusForGen, -manager.Settings.radiusForGen) * 100;
        b = new Vector2(manager.Settings.radiusForGen, -manager.Settings.radiusForGen) * 100;
        c = new Vector2(0, manager.Settings.radiusForGen) * 100;

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

        return triangulation;
    }

    #region Triangulation neighbors

    private static List<Triangle> GetNeighbors(List<Triangle> list, Triangle currentTriangle)
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
    #endregion

    #region Minimum spanning tree
    private static List<Edge> GetMinimumSpanningTree(MapManager manager, List<Triangle> triangulation, Room[] rooms)
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
                    if (edge.points.Contains(triangleEdge.pointA) && edge.points.Contains(triangleEdge.pointB))
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

        Heap<Edge> heap = new(edgeList.Count);

        for (int i = 0; i < edgeList.Count; i++)
        {
            heap.Add(edgeList[i]);
        }

        while (true)
        {
            if (heap.Count == 0)
            {
                break;
            }

            Edge shortestEdge = heap.RemoveFirst();
            bool canAdd = true;

            if (pointsVisited.Contains(shortestEdge.pointA) && pointsVisited.Contains(shortestEdge.pointB))
            {
                if (EdgeMakesLoop(shortestEdge, minimumSpanningTree))
                {
                    canAdd = false;
                }
            }

            if (canAdd)
            {
                edgeList.Remove(shortestEdge);
                pointsVisited.AddRange(shortestEdge.points);
                minimumSpanningTree.Add(shortestEdge);
            }

            if (rooms.Length == minimumSpanningTree.Count + 1)
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

        return minimumSpanningTree;
    }

    private static bool EdgeMakesLoop(Edge shortestEdge, List<Edge> currentMinimumSpanningTree)
    {
        List<Edge> openEdges = new();
        HashSet<Vector2> closedPoints = new()
        {
            shortestEdge.pointA
        };

        foreach (Edge edge in currentMinimumSpanningTree)
        {
            if (edge != shortestEdge)
            {
                if (edge.points.Contains(shortestEdge.pointB))
                {
                    openEdges.Add(edge);
                }
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

                    foreach (Edge edge in currentMinimumSpanningTree)
                    {
                        if (edge.points.Contains(point))
                        {
                            if (edge.points.Contains(shortestEdge.pointA))
                            {
                                return true;
                            }

                            openEdges.Add(edge);
                        }
                    }

                    closedPoints.Add(point);
                }

                openEdges.Remove(openEdges[i]);
            }
        }

        return false;
    }
    #endregion

    #region Hallway generation
    private static void GenerateHallways(MapManager manager, List<Edge> minimumSpanningTree, Room[] rooms)
    {
        int hallwayWidth = manager.Settings.hallwayWidth;

        foreach (Edge connection in minimumSpanningTree)
        {
            List<Room> roomList = new();

            foreach (Room room in rooms)
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

            List<Vector3Int> hallwayPath = new(AStar.FindPath(startingPosition, targetPosition, manager.Settings.randomizeHallwayPath, manager.Settings.seed));
            tileMaps[TileMapType.ground].UnionWith(hallwayPath);

            for (int i = 0; i < hallwayPath.Count; i++)
            {
                if (i + 1 >= hallwayPath.Count)
                {
                    break;
                }

                ExpandHallwayAndAddTiles(manager, hallwayPath[i], hallwayPath[i + 1], 5);
            }

            // *Working* but not very good looking
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

        }
    }

    #region Expand hallway path
    private static void ExpandHallwayAndAddTiles(MapManager manager, Vector3Int current, Vector3Int next, int wallWidth)
    {
        int hallwayWidth = manager.Settings.hallwayWidth;

        for (int i = -wallWidth; i < hallwayWidth + wallWidth + 1; i++)
        {
            if (manager.Settings.randomizedHallwayWidth)
            {
                hallwayWidth = rng.Next(manager.Settings.hallwayMinWidth, manager.Settings.hallwayMaxWidth);
            }

            for (int j = -wallWidth; j < hallwayWidth + wallWidth + 1; j++)
            {
                if (i >= 0 && j >= 0 && i <= hallwayWidth && j <= hallwayWidth)
                {
                    tileMaps[TileMapType.ground].Add(new(current.x + i - (hallwayWidth / 2), current.y + j - (hallwayWidth / 2)));
                }
                else
                {
                    tileMaps[TileMapType.wall].Add(new(current.x + i - (hallwayWidth / 2), current.y + j - (hallwayWidth / 2)));
                }
                //tileMaps[TileMapType.ground].Add(new(current.x, current.y + i - (hallwayWidth / 2)));
            }
        }

        //for (int i = 0; i < 2; i++)
        {
            //for (int j = 0; j < 2; j++)
            {
                //tileMaps[TileMapType.ground].Add(new(current.x + i - (hallwayWidth / 2), current.y + j - (hallwayWidth / 2)));
            }

            //tileMaps[TileMapType.wall].Add(new(current.x, current.y + hallwayWidth + i));
            //tileMaps[TileMapType.wall].Add(new(current.x, current.y - hallwayWidth - i));
            //tileMaps[TileMapType.wall].Add(new(current.x + hallwayWidth + i, current.y));
            //tileMaps[TileMapType.wall].Add(new(current.x - hallwayWidth - i, current.y));
        }

        /*
        if (current.y == next.y)
        {
            for (int i = -hallwayWidth; i < hallwayWidth; i++)
            {
                tileMaps[TileMapType.ground].Add(new(current.x + i, current.y));
                tileMaps[TileMapType.ground].Add(new(current.x, current.y + i));
            }

            for (int i = 1; i < 4; i++)
            {
                tileMaps[TileMapType.wall].Add(new(current.x, current.y + hallwayWidth + i - 1));
                tileMaps[TileMapType.wall].Add(new(current.x, current.y - hallwayWidth - i));
            }
        }
        else if (current.x == next.x)
        {
            for (int i = -hallwayWidth; i < hallwayWidth; i++)
            {
                tileMaps[TileMapType.ground].Add(new(current.x, current.y + i));
                tileMaps[TileMapType.ground].Add(new(current.x + i, current.y));
            }

            for (int i = 1; i < 4; i++)
            {
                tileMaps[TileMapType.wall].Add(new(current.x + hallwayWidth + i - 1, current.y));
                tileMaps[TileMapType.wall].Add(new(current.x - hallwayWidth - i, current.y));
            }
        }
        else
        {
            for (int i = 0; i < hallwayWidth; i++)
            {
                if (current.y > next.y)
                {
                    tileMaps[TileMapType.ground].Add(new(current.x, current.y - i));
                }
                else
                {
                    tileMaps[TileMapType.ground].Add(new(current.x, current.y + i));
                }

                if (current.x > next.x)
                {
                    tileMaps[TileMapType.ground].Add(new(current.x - i, current.y));
                }
                else
                {
                    tileMaps[TileMapType.ground].Add(new(current.x + i, current.y));
                }
            }

            if (current.y > next.y)
            {
                for (int i = 0; i < 5; i++)
                {
                    tileMaps[TileMapType.wall].Add(new(current.x, current.y - hallwayWidth - i));
                }
            }
            else
            {
                for (int i = 0; i < 5; i++)
                {
                    tileMaps[TileMapType.wall].Add(new(current.x, current.y + hallwayWidth + i));
                }
            }

            if (current.x > next.x)
            {
                for (int i = 0; i < 5; i++)
                {
                    tileMaps[TileMapType.wall].Add(new(current.x - hallwayWidth - i, current.y));
                }
            }
            else
            {
                for (int i = 0; i < 5; i++)
                {
                    tileMaps[TileMapType.wall].Add(new(current.x + hallwayWidth + i, current.y));
                }
            }
        }*/
    }
    #endregion
    #endregion

    #region Generate noise maps
    private static void GenerateNoiseMap(MapManager manager, int width, int height, Vector3Int center, List<GameObjectPositionPair> gameObjects)
    {
        float currentHeight;
        Vector3Int tilePosition;
        Vector3 spawnPosition;
        Vector2 offset;

        foreach (NoiseMap noiseMapSettings in manager.Settings.noiseMaps)
        {
            for (int i = 0; i < noiseMapSettings.amountOfNoiseLoops; i++)
            {
                offset = new(rng.Next(-100000, 100000), rng.Next(-100000, 100000));

                float[] noiseMap = NoiseMapGenerator.GenerateMap(width, height, manager.Settings.seed, noiseMapSettings.noiseScale, noiseMapSettings.octaves, noiseMapSettings.persistence, noiseMapSettings.lacunarity, offset);

                for (int j = 0; j < noiseMap.Length; j++)
                {
                    tilePosition = new((j % width) - (width / 2) + center.x, (j / width) - (height / 2) + center.y);
                    
                    currentHeight = noiseMap[j];

                    foreach (NoiseRegion region in noiseMapSettings.prefabs)
                    {
                        if (currentHeight <= region.heightValue)
                        {
                            if (region.name == "Empty")
                            {
                                break;
                            }

                            spawnPosition = tilePosition;

                            switch (noiseMapSettings.tileMapEffected)
                            {
                                case TileMapType.ground:
                                    if (availableGroundPositions.Contains(tilePosition))
                                    {
                                        availableGroundPositions.Remove(tilePosition);
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    break;
                                case TileMapType.wall:
                                    if (availableWallPositions.Contains(tilePosition) && !WallTileNeighborsAGroundTile(tilePosition))
                                    {
                                        availableWallPositions.Remove(tilePosition);
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    break;
                                default:
                                    break;
                            }

                            if (region.canRandomizePosition)
                            {
                                spawnPosition += new Vector3(rng.Next(0, region.randomPositionRange.x) / 100f, rng.Next(0, region.randomPositionRange.y) / 100f);
                            }

                            gameObjects.Add(new GameObjectPositionPair { gameObject = region.prefab, position = spawnPosition });

                            break;
                        }
                    }
                }
            }
        }
    }
    #endregion

    #region Add enemies
    private static void AddEnemiesToList(MapManager manager, List<GameObjectPositionPair> gameObjects)
    {
        GameObject prefab;
        Vector3Int tilePosition;

        for (int i = 0; i < manager.Settings.amountOfEnemies; i++)
        {
            if (availableGroundPositions.Count == 0)
            {
                break;
            }

            tilePosition = availableGroundPositions.ElementAt(rng.Next(0, availableGroundPositions.Count + 1));
            prefab = manager.Settings.enemyPrefabs[rng.Next(0, manager.Settings.enemyPrefabs.Length)];
            gameObjects.Add(new GameObjectPositionPair { gameObject = prefab, position = tilePosition });
            availableGroundPositions.Remove(tilePosition);
        }
    }
    #endregion

    #region Add prefabs
    private static void AddPrefabsToList(MapManager manager, List<GameObjectPositionPair> gameObjects)
    {
        Vector3Int tilePosition;
        Vector3 spawnPosition;

        foreach (PrefabSpawn pair in manager.Settings.prefabsToSpawn)
        {
            switch (pair.tileMapEffected)
            {
                case TileMapType.ground:
                    for (int i = 0; i < pair.amount; i++)
                    {
                        if (availableGroundPositions.Count == 0)
                        {
                            break;
                        }

                        tilePosition = availableGroundPositions.ElementAt(rng.Next(0, availableGroundPositions.Count));

                        spawnPosition = tilePosition;

                        if (pair.canRandomizePosition)
                        {
                            spawnPosition += new Vector3(rng.Next(0, pair.randomPositionRange.x) / 100f, rng.Next(0, pair.randomPositionRange.y) / 100f);
                        }

                        gameObjects.Add(new GameObjectPositionPair { gameObject = pair.prefab, position = tilePosition });
                        availableGroundPositions.Remove(tilePosition);
                    }
                    break;
                case TileMapType.wall:
                    for (int i = 0; i < pair.amount; i++)
                    {
                        if (availableWallPositions.Count == 0)
                        {
                            break;
                        }

                        tilePosition = availableWallPositions.ElementAt(rng.Next(0, availableWallPositions.Count + 1));

                        while (WallTileNeighborsAGroundTile(tilePosition) && availableWallPositions.Count > 0)
                        {
                            availableWallPositions.Remove(tilePosition);

                            tilePosition = availableWallPositions.ElementAt(rng.Next(0, availableWallPositions.Count + 1));
                        }

                        spawnPosition = tilePosition;

                        if (pair.canRandomizePosition)
                        {
                            spawnPosition += new Vector3(rng.Next(0, pair.randomPositionRange.x) / 100f, rng.Next(0, pair.randomPositionRange.y) / 100f);
                        }

                        gameObjects.Add(new GameObjectPositionPair { gameObject = pair.prefab, position = tilePosition });
                        availableWallPositions.Remove(tilePosition);
                    }
                    break;
                default:
                    break;
            }
        }
    }
    #endregion

    private static bool WallTileNeighborsAGroundTile(Vector3Int position)
    {
        for (int x = -1; x < 2; x++)
        {
            for (int y = -1; y < 2; y++)
            {
                if (tileMaps[TileMapType.ground].Contains(position + new Vector3Int(x, y)))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private struct GameObjectPositionPair
    {
        public GameObject gameObject;
        public Vector3 position;
    }
}
public enum TileMapType
{
    ground,
    wall,
    walkableIcon
}
