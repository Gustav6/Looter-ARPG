using System.Collections;
using System.Collections.Generic;
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
    public List<Room> rooms = new();
    private Heap<Room> heap;

    private Tilemap tileMap;
    private bool canMoveRooms = true;
    private bool generationComplete = false;

    public override void EnterState(MapGenerationManager manager)
    {
        tileMap = manager.tileMap;
        heap = new(manager.amountOfRooms);
        manager.MainRoomDepth = 4;

        for (int i = 0; i < manager.amountOfRooms; i++)
        {
            rooms.Add(GenerateRoom(manager));

            heap.Add(rooms[i]);
        }
    }

    public override void UpdateState(MapGenerationManager manager)
    {
        generationComplete = true;

        SeparateRooms();

        for (int i = 0; i < rooms.Count; i++)
        {
            for (int j = 0; j < rooms.Count; j++)
            {
                if (RoomIntersects(rooms[i], rooms[j]))
                {
                    generationComplete = false;
                    break;
                }
            }

            if (!generationComplete)
            {
                break;
            }
        }

        if (generationComplete)
        {
            manager.SwitchState(manager.loadingState);
        }
    }

    public override void ExitState(MapGenerationManager manager)
    {
        List<Room> test = new()
        {
            heap.First()
        };

        for (int i = 0; i < manager.MainRoomDepth; i++)
        {
            test.AddRange(heap.GetChildrenOnIndex(i));
        }

        PlaceRooms(manager, rooms);
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

    private void PlaceRooms(MapGenerationManager manager, List<Room> mainRooms)
    {
        for (int i = 0; i < mainRooms.Count; i++)
        {
            for (int x = 0; x < mainRooms[i].tiles.GetLength(0); x++)
            {
                for (int y = 0; y < mainRooms[i].tiles.GetLength(1); y++)
                {
                    manager.tileMap.SetTile((Vector3Int)mainRooms[i].tiles[x, y].gridPosition, manager.tileTexture);
                }
            }

            GameObject g = new();
            g.transform.position = mainRooms[i].WorldPosition;
        }
    }

    #region Methods for separating the rooms

    private void SeparateRooms()
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            rooms[i].MoveRoom(GetDirection(rooms[i]));
        }
    }

    private bool RoomIntersects(Room roomA, Room roomB)
    {
        if (roomA != roomB)
        {
            Vector2 roomALowerLeft = tileMap.CellToWorld((Vector3Int)roomA.tiles[0, 0].gridPosition);
            Vector2 roomATopRight = tileMap.CellToWorld((Vector3Int)roomA.tiles[roomA.width - 1, roomA.height - 1].gridPosition) + Vector3.one;

            Vector2 roomBLowerLeft = tileMap.CellToWorld((Vector3Int)roomB.tiles[0, 0].gridPosition);
            Vector2 roomBTopRight = tileMap.CellToWorld((Vector3Int)roomB.tiles[roomB.width - 1, roomB.height - 1].gridPosition) + Vector3.one;

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

    public Vector2Int GetDirection(Room room)
    {
        Vector2 separationVelocity = Vector2.zero;
        float numberOfAgentsToAvoid = 0;

        for (int i = 0; i < rooms.Count; i++)
        {
            if (ReferenceEquals(rooms[i], room) || !RoomIntersects(room, rooms[i]))
            {
                continue;
            }

            Vector2 otherPosition = rooms[i].WorldPosition;

            //float distance = Vector2.Distance(otherPosition, room.WorldPosition);

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

    private List<Room> GetMainRooms()
    {
        // Find the rooms with biggest area and return those as main rooms

        return new List<Room>();
    }

    private void GetShortestSpanningTree()
    {

    }

    private void PlaceWalls()
    {

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

public class Room : IHeapItem<Room>
{
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
    
    public int Size
    {
        get
        {
            return width * height;
        }
    }

    public RoomTile[,] tiles;
    public readonly int width, height;

    private int heapIndex;
    public int HeapIndex
    {
        get { return heapIndex; }
        set { heapIndex = value; }
    }

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